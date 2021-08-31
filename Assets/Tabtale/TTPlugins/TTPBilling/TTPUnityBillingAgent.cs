#if TTP_BILLING && UNITY_PURCHASING
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using System;
using System.Linq;
using UnityEngine.Purchasing.Security;
using UnityEngine.Purchasing.MiniJSON;

namespace Tabtale.TTPlugins
{
    public class PurchaseIAPResult
    {
        public PurchaseIAPResult(InAppPurchasableItem purchasedItem, PurchaseIAPResultCode result = PurchaseIAPResultCode.Success, BillerErrors error = BillerErrors.NO_ERROR)
        {
            this.purchasedItem = purchasedItem;
            this.result = result;
            this.error = error;
        }

        public InAppPurchasableItem purchasedItem;
        public PurchaseIAPResultCode result;
        public BillerErrors error;
    }

    public enum PurchaseIAPResultCode
    {
        Success, Cancelled, Failed
    }

    public enum BillerErrors
    {
        NO_ERROR,
        PURCHASING_UNAVAILABLE,
        ATTEMPTING_TO_PURCHASE_PRODUCT_WITH_SAME_RECEIPT,
        PAYMENT_DECLINED,
        PRODUCT_UNAVAILABLE,
        REMOTE_VALIDATION_FAILED,
        APP_NOT_KNOWN,
        NO_PRODUCTS_AVAILABLE,
        UNKNOWN
    }

    public struct InAppPurchasableItem
    {
        public string mainId;
        public string id;
        public string name;
        public string description;
        public string localizedPrice;
        public string receipt;
        public string transactionId;
        public string currency;
        public bool isSubscription;
    }

    public class TTPUnityBillingAgent : IStoreListener
    {
        private const string IAPS_PERSISTENCY_KEY = "PSDKBillingIAPs";

        private IStoreController _storeController;
        private IAppleExtensions _appleExtensions;
        private bool _isInitialised = false;
        private bool _isPurchaseInProgress = false;
        private bool _isRestoreInProgress = false;
        private InAppPurchasableItem _iapInValidation;
        private PurchaseEventArgs _iapInValidationArgs;
        private string _noAdsIapIds = "";
        private Dictionary<string, string> _iapMap;
        private Dictionary<string,object> _configuration;

        private Action<BillerErrors> _onBillingInitAction;
        private Action<PurchaseIAPResult> _onPurchasedAction;
        private Action<string, string, string, string> _validatePurchaseAction;
        private Action<bool> _reportNoAdsPurchasedAction;
        private Action<bool> _transactionsRestoredAction;
        private const long ANALYTICS_TARGET_DELTA_DNA = 1 << 1;
        private const long ANALYTICS_TARGET_FIREBASE = 1 << 2;

        public TTPUnityBillingAgent(Action<PurchaseIAPResult> onPurchasedAction, Action<string,string,string,string> validatePurchaseAction, Action<bool> reportNoAdsPurchasedAction, Action<bool> transactionsRestoredAction, Dictionary<string, object> configuration)
        {
            _validatePurchaseAction = validatePurchaseAction;
            _onPurchasedAction = onPurchasedAction;
            _reportNoAdsPurchasedAction = reportNoAdsPurchasedAction;
            _transactionsRestoredAction = transactionsRestoredAction;
            _configuration = configuration;

            RegisterAppLifeCycle(true);
        }

        ~TTPUnityBillingAgent()
        {
            RegisterAppLifeCycle(false);
        }

        public void PurchaseItem(string itemId)
        {
            Debug.Log("Billing::PurchaseItem:" + itemId);

            if (_isPurchaseInProgress)
            {
                Debug.Log("Billing::PurchaseItem:attempted to buy iap while another purchase is in progress. id: " + itemId);
                return;
            }

            _isPurchaseInProgress = true;

            // An actual attempt to purchase was made, this means a restore is not in progress
            _isRestoreInProgress = false;

            string finalId = GetFinalId(itemId);
            if (_storeController == null)
            {
                _isPurchaseInProgress = false;
                return;
            }

            if (finalId != null)
                _storeController.InitiatePurchase(_storeController.products.WithStoreSpecificID(finalId));
            else
            {
                Debug.LogError("Billing::PurchaseItem:" + itemId + " does not exist as iapId or id");
                _isPurchaseInProgress = false;
            }
        }

        public bool IsConsumable(string itemId)
        {
            if (!IsInitialisedWithLogging())
                return false;
            string finalId = GetFinalId(itemId);
            if (finalId != null)
                return _storeController.products.WithStoreSpecificID(finalId).definition.type == ProductType.Consumable;
            else
                return false;
        }

        public bool IsPurchased(string itemId)
        {
            string finalId = GetFinalId(itemId);
            if (!IsInitialisedWithLogging())
                return false;

            Product item = _storeController.products.WithStoreSpecificID(finalId != null ? finalId : "");

            if (item == null)
            {
                Debug.Log("Billing::IsPurchased:Error, Cannot find item - " + finalId);
                return false;
            }

            Debug.Log("Billing::IsPurchased: " + item.receipt ?? "null");

            if (Application.platform == RuntimePlatform.Android && !CheckProductPurchaseStateInPayloadForGooglePlayStore(item.receipt))
            {
                return false;
            }

            bool isSubscription = item.definition.type == ProductType.Subscription;

            // Unity do not persist the receipt between app restarts, so we save the purchases locally
            if (!isSubscription)
            {
                if (item.hasReceipt) return true;
                return IsSavedPurchased(finalId);
            }
            else
            {
                return item.hasReceipt;
            }
        }

        private bool CheckProductPurchaseStateInPayloadForGooglePlayStore(string reciept)
        {
            Dictionary<string, object> recieptDic = Json.Deserialize(reciept) as Dictionary<string, object>;
            string payload = null;
            if(recieptDic != null && recieptDic.ContainsKey("Payload"))
            {
                payload = recieptDic["Payload"] as string;
            }
            if (string.IsNullOrEmpty(payload))
            {
                return false;
            }

            var payloadWrapper = (Dictionary<string, object>)MiniJson.JsonDecode(payload);
            if (!payloadWrapper.ContainsKey("json"))
            {
                Debug.LogError("Billing:: The product receipt does not contain enough information, the 'json' field is missing");
                return false;
            }

            var originalJsonPayloadWrapper = (Dictionary<string, object>)MiniJson.JsonDecode((string)payloadWrapper["json"]);
            if (originalJsonPayloadWrapper == null || !originalJsonPayloadWrapper.ContainsKey("purchaseState"))
            {
                Debug.LogError("Billing:: The product receipt does not contain enough information, the 'purchaseState' field is missing");
                return false;
            }
            var purchaseState = originalJsonPayloadWrapper["purchaseState"];
            return (long)purchaseState == 0;
        }

        public void RestorePurchases(Action<bool> onTransactionsRestoredAction)
        {
            // If Purchasing has not yet been set up ...
            if (!IsInitialisedWithLogging())
            {
                // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
                Debug.Log("Billing::RestorePurchases: FAIL. Not initialized.");
                return;
            }

            // If we are running on an Apple device ... 
            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                // ... begin restoring purchases
                Debug.Log("Billing::RestorePurchases started ...");

                _isRestoreInProgress = true;

                

                // Begin the asynchronous process of restoring purchases. Expect a confirmation response in the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
                _appleExtensions.RestoreTransactions((bool result) =>
                {
                    _isRestoreInProgress = false;
                    onTransactionsRestoredAction.Invoke(result);
                    _transactionsRestoredAction.Invoke(result);
                });

            }
            else
            {
                // We are not running on an Apple device. No work is necessary to restore purchases.
                Debug.Log("Billing::RestorePurchases: FAIL. Not supported on this platform. Current = " + Application.platform);
            }
        }

        public void InitBilling(Action<BillerErrors> onBillingInitAction)
        {
            _onBillingInitAction = onBillingInitAction;
            if (_isInitialised)
            {
                Debug.Log("Billing::InitBilling already init");
                return;
            }
            Debug.Log("Billing::InitBilling");

            // On Android restore is called automatically on app start, we want to be aware of it and not report to analytics as revenue
            if (Application.platform == RuntimePlatform.Android)
            {
                _isRestoreInProgress = true;
            }

            var module = StandardPurchasingModule.Instance();

            // The FakeStore supports: no-ui (always succeeding), basic ui (purchase pass/fail), and 
            // developer ui (initialization, purchase, failure code setting). These correspond to 
            // the FakeStoreUIMode Enum values passed into StandardPurchasingModule.useFakeStoreUIMode.
            module.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;


            var builder = ConfigurationBuilder.Instance(module);
            string googlePublicKey = _configuration != null ? _configuration.GetString("googlePlayLicenceKey", "") : null;
            if (Application.platform == RuntimePlatform.Android)
            {
                if (string.IsNullOrEmpty(googlePublicKey))
                {
                    Debug.Log("Billing::InitBilling:Missing google public key");
                }

            }

            ReadProductInfo(builder);

#if !AMAZON
            builder.Configure<IGooglePlayConfiguration>().SetPublicKey(googlePublicKey);
#endif

            UnityPurchasing.Initialize(this, builder);
        }

        public bool IsNoAdsItem(string itemId)
        {
            string finalId = GetFinalId(itemId);
            if (finalId != null)
                return _noAdsIapIds.Contains(finalId);
            else
                return false;
        }

        public string GetLocalizedPriceString(string itemId)
        {
            Debug.Log("Billing::GetLocalizedPriceString:" + itemId);

            if (!IsInitialisedWithLogging())
                return "";

            string finalId = GetFinalId(itemId);
            Product item = null;
            if (finalId != null)
                item = _storeController.products.WithStoreSpecificID(finalId);

            if (item == null)
            {
                Debug.Log("Billing::GetLocalizedPriceString:Error, cannot find item by id: " + itemId);
                return "";
            }

            return item.metadata.localizedPriceString;
        }

        public string GetISOCurrencySymbol(string itemId)
        {
            if (!IsInitialisedWithLogging())
                return "";
            string finalId = GetFinalId(itemId);
            Product item = null;
            if (finalId != null)
                item = _storeController.products.WithStoreSpecificID(finalId);
            if (item == null)
            {
                Debug.Log("Billing::GetISOCurrencySymbol:Error, cannot find item by id: " + itemId);
                return "";
            }

            return item.metadata.isoCurrencyCode;
        }

        public decimal GetPriceInLocalCurrency(string itemId)
        {
            if (!IsInitialisedWithLogging())
                return 0;
            string finalId = GetFinalId(itemId);
            Product item = null;
            if (finalId != null)
                item = _storeController.products.WithStoreSpecificID(finalId);
            if (item == null)
            {
                Debug.Log("Billing::GetPriceInLocalCurrency:Error, cannot find item by id: " + itemId);
                return 0;
            }

            return item.metadata.localizedPrice;

        }

        public void ClearTransactions()
        {
            PlayerPrefs.DeleteKey(IAPS_PERSISTENCY_KEY);
        }

        private bool IsInitialisedWithLogging()
        {
            if (!_isInitialised)
                Debug.Log("Attempted to use purchasing api when unity iap is not initialized. check Failed to initialize in the log for more details");
            return _isInitialised;
        }

        private bool IsSavedPurchased(string iapId)
        {
            string saveData = PlayerPrefs.GetString(IAPS_PERSISTENCY_KEY, "");
            string[] splitData = saveData.Split(new char[] { ';' });
            bool isPurchased = false;
            foreach (string item in splitData)
            {
                if (item.Equals(iapId))
                {
                    isPurchased = true;
                    break;
                }
            }
            return isPurchased;
        }

        public string GetFinalId(string itemId)
        {
            string finalId = null;

            if (_iapMap.Values.Contains(itemId))
            {
                finalId = itemId;
            }
            else
            {
                if (_iapMap.Keys.Contains(itemId))
                {
                    _iapMap.TryGetValue(itemId, out finalId);
                }
            }
            return finalId;
        }

#region IStoreListener implementation
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.Log("Billing::OnInitialized");

            _isInitialised = true;
            _storeController = controller;
            _appleExtensions = extensions.GetExtension<IAppleExtensions>();

            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                // On Apple platforms we need to handle deferred purchases caused by Apple's Ask to Buy feature.
                // On non-Apple platforms this will have no effect; OnDeferred will never be called.
                _appleExtensions.RegisterPurchaseDeferredListener(OnDeferred);
            }

            DeterminePurchaseAd(_storeController.products.all);

            _onBillingInitAction.Invoke(BillerErrors.NO_ERROR);
        }

        private void DeterminePurchaseAd(Product[] allProducts)
        {
            bool purchaseAd = false;
            foreach (Product product in _storeController.products.all)
            {
                Debug.Log("Billing::OnInitialized: id = " + product.definition.id + "reciept = " + product.receipt ?? "null");
                if (IsNoAdsItem(product.definition.id))
                {
                    if (product.hasReceipt)
                    {
                        if (product.definition.type == ProductType.Subscription)
                        {
                            SubscriptionInfo itemInfo = GetSubscriptionInfo(product.definition.id);
                            if (itemInfo != null)
                            {
                                if (itemInfo.isSubscribed() == Result.True)
                                {
                                    Debug.Log("Billing::OnInitialized: subscribed to item " + product.definition.id + " which is defined as noAds. notifying TT Plugins.");
                                    purchaseAd = true;
                                }
                            }
                        }
                        else
                        {
                            Debug.Log("Billing::OnInitialized: purchased item " + product.definition.id + " which is defined as noAds. notifying TT Plugins.");
                            purchaseAd = true;
                        }
                    }
                    else
                    {
                        if (product.definition.type != ProductType.Subscription && IsSavedPurchased(product.definition.id))
                        {
                            Debug.Log("Billing::OnInitialized: purchased item " + product.definition.id + " which is defined as noAds. notifying TT Plugins. persistency check.");
                            purchaseAd = true;
                        }
                    }
                }
            }
            if (purchaseAd)
            {
                if (_reportNoAdsPurchasedAction != null)
                    _reportNoAdsPurchasedAction.Invoke(true);
            }
            else
            {
                Debug.Log("Billing::OnInitialized: did not find any noAds item subscribed or purchased. Notifying TT Plugins.");
                if (_reportNoAdsPurchasedAction != null)
                    _reportNoAdsPurchasedAction.Invoke(false);
            }
        }

        private void OnDeferred(Product item)
        {
            Debug.Log("Billing::OnDeferred:Purchase deferred: " + item.definition.id);
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            _isInitialised = false;
            if (_reportNoAdsPurchasedAction != null)
                _reportNoAdsPurchasedAction.Invoke(false);
            Debug.Log("Billing::Unity billing failed to initialize!");
            switch (error)
            {
                case InitializationFailureReason.AppNotKnown:
                    Debug.Log("Billing::Failed to initialize - Is your App correctly uploaded on the relevant publisher console?");
                    _onBillingInitAction.Invoke(BillerErrors.APP_NOT_KNOWN);
                    break;
                case InitializationFailureReason.PurchasingUnavailable:
                    // Ask the user if billing is disabled in device settings.
                    Debug.Log("Billing::Failed to initialize - Billing disabled!");
                    _onBillingInitAction.Invoke(BillerErrors.PURCHASING_UNAVAILABLE);
                    break;
                case InitializationFailureReason.NoProductsAvailable:
                    // Developer configuration error; check product metadata.
                    Debug.Log("Billing::Failed to initialize - No products available for purchase!");
                    _onBillingInitAction.Invoke(BillerErrors.NO_PRODUCTS_AVAILABLE);
                    break;
            }
        }

        public void OnPurchaseFailed(Product item, PurchaseFailureReason reason)
        {
            Debug.Log("Billing::OnPurchaseFailed:" + item.metadata.localizedTitle);

            PurchaseIAPResultCode resultCode = PurchaseIAPResultCode.Failed;
            BillerErrors error = BillerErrors.NO_ERROR;
            InAppPurchasableItem inApp = CreatePurchasableInAppItem(item);

            switch (reason)
            {
                case PurchaseFailureReason.ExistingPurchasePending:
                    error = BillerErrors.ATTEMPTING_TO_PURCHASE_PRODUCT_WITH_SAME_RECEIPT;
                    break;
                case PurchaseFailureReason.PaymentDeclined:
                    error = BillerErrors.PAYMENT_DECLINED;
                    break;
                case PurchaseFailureReason.ProductUnavailable:
                    error = BillerErrors.PRODUCT_UNAVAILABLE;
                    break;
                case PurchaseFailureReason.PurchasingUnavailable:
                    error = BillerErrors.PURCHASING_UNAVAILABLE;
                    break;
                case PurchaseFailureReason.SignatureInvalid:
                    error = BillerErrors.REMOTE_VALIDATION_FAILED;
                    break;
                case PurchaseFailureReason.Unknown:
                    error = BillerErrors.UNKNOWN;
                    break;
                case PurchaseFailureReason.UserCancelled:
                    resultCode = PurchaseIAPResultCode.Cancelled;
                    break;
            }

            _isPurchaseInProgress = false;

            PurchaseIAPResult result = new PurchaseIAPResult(inApp, resultCode, error);
            _onPurchasedAction.Invoke(result);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
        {
            Debug.Log("Billing::Processing Purchase: " + e.purchasedProduct.definition.id);
            Debug.Log("Billing::Receipt: " + e.purchasedProduct.receipt);
            Debug.Log("Billing::Transaction Id: " + e.purchasedProduct.transactionID);

            _iapInValidation = CreatePurchasableInAppItem(e);
            _iapInValidationArgs = e;

            // If restore is in progress - Skip receipt validation ! We have to do this because the server will not validate a receipt that has already been validated in the past
            if (_isRestoreInProgress)
            {
                // Indicate we have handled this purchase, we will not be informed of it again.
                Debug.Log("Billing::ProcessPurchase : Skipping receipt validation since restore is in progress");
                if (IsNoAdsItem(_iapInValidation.id))
                {
                    Debug.Log("Billing::ProcessPurchase : IsNoAdsItem=true calling PurchaseAd");
                    if (_reportNoAdsPurchasedAction != null)
                        _reportNoAdsPurchasedAction.Invoke(true);
                }
                else
                {
                    Debug.Log("Billing::ProcessPurchase : IsNoAdsItem=false");
                }
                PurchaseIAPResult result = new PurchaseIAPResult(_iapInValidation, PurchaseIAPResultCode.Success, BillerErrors.NO_ERROR);
                SavePurchaseLocally(_iapInValidation.id);
                _onPurchasedAction.Invoke(result);
                _isPurchaseInProgress = false;
                return PurchaseProcessingResult.Complete;
            }

            Dictionary<string, object> recieptJson = Json.Deserialize(_iapInValidation.receipt) as Dictionary<string, object>;
            string payload = recieptJson.Get<string>("Payload");

            string purchaseToken = null;

#if UNITY_ANDROID
            if (payload != null) {
                Dictionary<string,object> tmp = Json.Deserialize(payload) as Dictionary<string,object>;
                if (tmp != null) {
                    string tmpStr = tmp.Get<string> ("json");
                    if (tmpStr != null) {
                        Dictionary<string,object> innerTmp = Json.Deserialize(tmpStr) as Dictionary<string,object>;
                        if (innerTmp != null) {
                            purchaseToken = innerTmp.Get<string> ("purchaseToken");
                        }
                        else {
                            Debug.Log ("ProcessPurchase failed to Deserialize json");
                        }
                    }
                    else {
                        Debug.Log ("ProcessPurchase failed to find json attribut in payload");
                    }
                }
                else {
                    Debug.Log ("ProcessPurchase failed to Deserialize payload");
                }
            } 
            else {
                Debug.Log ("ProcessPurchase failed to find payload attribute");
            }
            if(e.purchasedProduct.definition.type == ProductType.Subscription)
            {
                SubscriptionInfo subscriptionInfo = GetSubscriptionInfo(e.purchasedProduct.definition.id);
                if(subscriptionInfo != null && subscriptionInfo.isSubscribed() == Result.True)
                {
                    int trialPeriod = subscriptionInfo.isFreeTrial() == Result.True ? 1 : 0;
                    Dictionary<string, object> postData = new Dictionary<string, object>
                                   {
                                        {"trialPeriod", trialPeriod},
                                        {"purchasedProductID",e.purchasedProduct.definition.id},
                                        {"transactionID",e.purchasedProduct.transactionID},
                                        {"originalTransactionID", purchaseToken},
                                        {"revenue",e.purchasedProduct.metadata.localizedPrice},
                                        {"revenueCurrency",e.purchasedProduct.metadata.isoCurrencyCode}
                                    };
                    LogSubscriptionStarted(postData);
                }
            }
#else
            purchaseToken = payload;
            if (e.purchasedProduct.definition.type == ProductType.Subscription)
            {
                Debug.Log("ProcessPurchase : identified subscription on iOS");
                SubscriptionInfo subscriptionInfo = GetSubscriptionInfo(e.purchasedProduct.definition.id);
                if (subscriptionInfo != null && subscriptionInfo.isSubscribed() == Result.True)
                {
                    ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
                    IAppleConfiguration appleConfig = builder.Configure<IAppleConfiguration>();
                    byte[] receiptData = Convert.FromBase64String(appleConfig.appReceipt);
                    Debug.Log("ProcessPurchase : reciept data - " + receiptData);
                    if (receiptData != null)
                    {
                        AppleReceipt appleReceipt = new AppleValidator(AppleTangle.Data()).Validate(receiptData);
                        if (appleReceipt != null)
                        {
                            Debug.Log("ProcessPurchase: succeeded to create apple receipt item for -" + e.purchasedProduct.definition.id);
                            AppleInAppPurchaseReceipt currentIAPReceipt = null;
                            foreach (AppleInAppPurchaseReceipt inAppPurchaseReceipt in appleReceipt.inAppPurchaseReceipts)
                            {
                                Debug.Log("ProcessPurchase : apple iap receipt found for - " + inAppPurchaseReceipt.productID);
                                if (inAppPurchaseReceipt.productID == e.purchasedProduct.definition.id)
                                {
                                    Debug.Log("ProcessPurchase : found receipt for current product!");
                                    currentIAPReceipt = inAppPurchaseReceipt;
                                    break;
                                }
                            }
                            if (currentIAPReceipt != null)
                            {
                                if (currentIAPReceipt.transactionID == currentIAPReceipt.originalTransactionIdentifier)
                                {
                                    int trialPeriod = subscriptionInfo.isFreeTrial() == Result.True ? 1 : 0;
                                    Dictionary<string, object> postData = new Dictionary<string, object>
                                    {
                                        {"trialPeriod", trialPeriod},
                                        {"purchasedProductID",e.purchasedProduct.definition.id},
                                        {"transactionID",e.purchasedProduct.transactionID},
                                        {"originalTransactionID",currentIAPReceipt.originalTransactionIdentifier},
                                        {"revenue",e.purchasedProduct.metadata.localizedPrice},
                                        {"revenueCurrency",e.purchasedProduct.metadata.isoCurrencyCode}
                                    };
                                    LogSubscriptionStarted(postData);
                                }
                                else
                                {
                                    Debug.Log("ProcessPurchase : transaction id does not equal original. will not send data.");
                                }
                            }
                        }
                        else
                        {
                            Debug.Log("ProcessPurchase : could not parse apple recipet");

                        }
                    }
                }
                else
                {
                    Debug.Log("ProcessPurchase : product is not a valid subscription, or user did not subscribe.");
                }
            }
#endif
            if (purchaseToken != null)
            {
                Debug.Log("ProcessPurchase purchaseToken: " + purchaseToken);
                _validatePurchaseAction.Invoke(purchaseToken,_iapInValidation.localizedPrice, _iapInValidation.currency, _iapInValidation.id);
            }
            else
            {
                Debug.Log("ProcessPurchase purchaseToken was not found.");
            }
            OnValidPurchaseResponse(_iapInValidation.localizedPrice, _iapInValidation.currency, _iapInValidation.id, true);
            return PurchaseProcessingResult.Complete;
        }
#endregion

        private void OnValidPurchaseResponse(string price, string currency, string itemId, bool isValid)
        {
            Debug.Log("Billing::OnValidPurchaseResponseEvent: Receipt validation result : " + isValid);
            PurchaseIAPResult result;
            if (isValid)
            {
                result = new PurchaseIAPResult(_iapInValidation, PurchaseIAPResultCode.Success, BillerErrors.NO_ERROR);
                SavePurchaseLocally(_iapInValidation.id);
                if (IsNoAdsItem(_iapInValidation.id))
                {
                    Debug.Log("Billing::OnValidPurchaseResponseEvent IsNoAdsItem=true calling PurchaseAd");
                    if(_reportNoAdsPurchasedAction != null)
                        _reportNoAdsPurchasedAction.Invoke(true);
                }
                else
                {
                    Debug.Log("Billing::OnValidPurchaseResponseEvent IsNoAdsItem=false");
                }
            }
            else
            {
                Debug.Log("Billing::OnValidPurchaseResponseEvent item is not valid");
                result = new PurchaseIAPResult(_iapInValidation, PurchaseIAPResultCode.Failed, BillerErrors.REMOTE_VALIDATION_FAILED);
            }
            _onPurchasedAction.Invoke(result);
            _isPurchaseInProgress = false;
            _storeController.ConfirmPendingPurchase(_iapInValidationArgs.purchasedProduct);
            ResetPurchasableItem(_iapInValidation);
            _iapInValidationArgs = null;

        }

        private void SavePurchaseLocally(string iapId)
        {
            string iaps = PlayerPrefs.GetString(IAPS_PERSISTENCY_KEY, "");
            if (iaps.Length > 0)
                iaps += ";";
            iaps += iapId;
            PlayerPrefs.SetString(IAPS_PERSISTENCY_KEY, iaps);
        }

        private InAppPurchasableItem CreatePurchasableInAppItem(PurchaseEventArgs purchaseEventArgs)
        {
            Product product = purchaseEventArgs.purchasedProduct;

            return CreatePurchasableInAppItem(product);
        }

        private InAppPurchasableItem CreatePurchasableInAppItem(Product product)
        {
            InAppPurchasableItem purchasableInAppItem;
            purchasableInAppItem.id = product.definition.id;
            purchasableInAppItem.mainId = GetMainId(purchasableInAppItem.id);
            purchasableInAppItem.name = product.metadata.localizedTitle;
            purchasableInAppItem.description = product.metadata.localizedDescription;
            purchasableInAppItem.localizedPrice = product.metadata.localizedPrice.ToString();
            purchasableInAppItem.receipt = product.receipt;
            purchasableInAppItem.transactionId = product.transactionID;
            purchasableInAppItem.currency = product.metadata.isoCurrencyCode;
            purchasableInAppItem.isSubscription = product.definition.type == ProductType.Subscription;
            return purchasableInAppItem;
        }

        private void ResetPurchasableItem(InAppPurchasableItem purchasableInAppItem)
        {
            purchasableInAppItem.id = null;
            purchasableInAppItem.name = null;
            purchasableInAppItem.description = null;
            purchasableInAppItem.localizedPrice = null;
            purchasableInAppItem.receipt = null;
            purchasableInAppItem.transactionId = null;
            purchasableInAppItem.currency = null;
            purchasableInAppItem.isSubscription = false;
        }

        private string GetMainId(string storeId)
        {
            if(_iapMap != null)
            {
                foreach(KeyValuePair<string, string> kvp in _iapMap)
                {
                    if(kvp.Value == storeId)
                    {
                        return kvp.Key;
                    }
                }
            }
            return "";
        }

        private void ReadProductInfo(ConfigurationBuilder builder)
        {
            _iapMap = new Dictionary<string, string>();
            List<object> productArr = _configuration != null ? _configuration.Get<List<object>>("iaps") : null;
            if (productArr != null)
            {
                foreach (object productObj in productArr)
                {
                    if (productObj.GetType() == typeof(Dictionary<string, object>))
                    {
                        Dictionary<string, object> productDict = productObj as Dictionary<string, object>;
                        string productTypeStr = productDict.GetString("type", "");
                        ProductType productType;
                        if (TryParseProductType(productTypeStr, out productType))
                        {
                            string iapId = productDict.GetString("iapId", "");
                            string itemId = productDict.GetString("id", "");
                            bool isNoAds = productDict.GetBool("noAds");
                            if (itemId.Length > 0 && iapId.Length > 0)
                            {
                                Debug.Log("Billing::ReadProductInfo:Adding Product - iapId = " + iapId + ", id = " + itemId + ", type = " + productTypeStr + ", noAds = " + isNoAds.ToString());
                                _iapMap.Add(itemId, iapId);
                                if (isNoAds)
                                {
                                    _noAdsIapIds += ";" + iapId;
                                }
                                string storeName = (Application.platform == RuntimePlatform.IPhonePlayer) ? AppleAppStore.Name : GooglePlay.Name;
                                Debug.Log("Billing::ReadProductInfo:" + storeName + " selected");
                                builder.AddProduct(iapId, productType, new IDs {
                                    {iapId,storeName}
                                });
                            }
                        }
                    }
                }
            }
        }

        private bool TryParseProductType(string productTypeStr, out ProductType productType)
        {
            productType = ProductType.Consumable;
            if (productTypeStr == "consumable")
            {
                productType = ProductType.Consumable;
                return true;
            }
            else if (productTypeStr == "non-consumable")
            {
                productType = ProductType.NonConsumable;
                return true;
            }
            else if (productTypeStr == "subscription")
            {
                productType = ProductType.Subscription;
                return true;
            }
            else
            {
                Debug.LogError("Billing::ParseProductType: failed to parse product type - " + productTypeStr);
                return false;
            }
        }

        //copied from IAPDemo.cs
        private bool CheckIfProductIsAvailableForSubscriptionManager(string receipt)
        {
            if (receipt == null)
            {
                Debug.Log("Billing::CheckIfProductIsAvailableForSubscriptionManager:receipt is null");
                return false;
            }
            var receipt_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(receipt);
            if (!receipt_wrapper.ContainsKey("Store") || !receipt_wrapper.ContainsKey("Payload"))
            {
                Debug.Log("Billing::CheckIfProductIsAvailableForSubscriptionManager:The product receipt does not contain enough information");
                return false;
            }
            var store = (string)receipt_wrapper["Store"];
            var payload = (string)receipt_wrapper["Payload"];

            if (payload != null)
            {
                switch (store)
                {
                    case GooglePlay.Name:
                        {
                            var payload_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(payload);
                            if (!payload_wrapper.ContainsKey("json"))
                            {
                                Debug.Log("Billing::CheckIfProductIsAvailableForSubscriptionManager:The product receipt does not contain enough information, the 'json' field is missing");
                                return false;
                            }
                            var original_json_payload_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode((string)payload_wrapper["json"]);
                            if (original_json_payload_wrapper == null || !original_json_payload_wrapper.ContainsKey("developerPayload"))
                            {
                                Debug.Log("Billing::CheckIfProductIsAvailableForSubscriptionManager:The product receipt does not contain enough information, the 'developerPayload' field is missing");
                                return false;
                            }
                            var developerPayloadJSON = (string)original_json_payload_wrapper["developerPayload"];
                            var developerPayload_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(developerPayloadJSON);
                            if (developerPayload_wrapper == null || !developerPayload_wrapper.ContainsKey("is_free_trial") || !developerPayload_wrapper.ContainsKey("has_introductory_price_trial"))
                            {
                                Debug.Log("Billing::CheckIfProductIsAvailableForSubscriptionManager:The product receipt does not contain enough information, the product is not purchased using 1.19 or later");
                                return false;
                            }
                            return true;
                        }
                    case AppleAppStore.Name:
                    case AmazonApps.Name:
                    case MacAppStore.Name:
                        {
                            return true;
                        }
                    default:
                        {
                            return false;
                        }
                }
            }
            return false;
        }

        public SubscriptionInfo GetSubscriptionInfo(string itemId)
        {
            string finalId = GetFinalId(itemId);
            if (!IsInitialisedWithLogging())
                return null;
            Product item = _storeController.products.WithStoreSpecificID(finalId ?? "");
            if (item == null)
            {
                Debug.Log("Billing::GetSubscriptionInfo:Error, Cannot find item - " + finalId);
                return null;
            }
            if (item.definition.type != ProductType.Subscription)
            {
                Debug.Log("Billing::GetSubscriptionInfo:Error, item is not a subscription - " + finalId);
                return null;
            }
            if (!CheckIfProductIsAvailableForSubscriptionManager(item.receipt))
            {
                Debug.Log("Billing::GetSubscriptionInfo:Error, item is not available in sub manager - " + finalId);
                return null;
            }
            Dictionary<string, string> introductory_info_dict = _appleExtensions.GetIntroductoryPriceDictionary();
            string intro_json = (introductory_info_dict == null || !introductory_info_dict.ContainsKey(item.definition.storeSpecificId)) ? null : introductory_info_dict[item.definition.storeSpecificId];
            SubscriptionManager p = new SubscriptionManager(item, intro_json);
            return p.getSubscriptionInfo();
        }

        private void OnApplicationFocus()
        {
            if (_isInitialised && _storeController != null)
            {
                Debug.Log("Billing::OnResume: DeterminePurchaseAd called");
                DeterminePurchaseAd(_storeController.products.all);
            }
        }

        private void RegisterAppLifeCycle(bool register)
        {
            System.Reflection.MethodInfo method = typeof(TTPCore).GetMethod("GetTTPGameObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (method != null)
            {
                GameObject gameObject = method.Invoke(null, null) as GameObject;
                if (gameObject != null)
                {
                    TTPCore.TTPGameObject ttpGo = gameObject.GetComponent<TTPCore.TTPGameObject>();
                    if(register)
                        ttpGo.OnApplicationFocusEvent += OnApplicationFocus;
                    else
                        ttpGo.OnApplicationFocusEvent -= OnApplicationFocus;
                }
            }
            else
            {
                Debug.LogWarning("Billing::RegisterAppLifeCycle:: could not find method GetTTPGameObject");
            }
        }

        private void LogSubscriptionStarted(Dictionary<string, object> postData)
        {
            System.Type analyticsClsType = System.Type.GetType("Tabtale.TTPlugins.TTPAnalytics");
            if (analyticsClsType != null)
            {
                System.Reflection.MethodInfo method = analyticsClsType.GetMethod("LogEvent", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (method != null)
                {
                    method.Invoke(null, new object[] { ANALYTICS_TARGET_DELTA_DNA | ANALYTICS_TARGET_FIREBASE, "subscriptionStarted", postData, false, true });
                }
                else
                {
                    Debug.LogWarning("CallAnalyticsByReflection:: could not find method - LogEvent");
                }
            }
            else
            {
                Debug.LogWarning("CallAnalyticsByReflection:: could not find TTPAnalytics class");
            }
        }


    }


}
#endif
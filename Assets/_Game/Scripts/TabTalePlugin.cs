using UnityEngine;
using Tabtale.TTPlugins;

public class TabTalePlugin : MonoBehaviour
{
    void Awake()
    {
        TTPCore.Setup();
    }
}

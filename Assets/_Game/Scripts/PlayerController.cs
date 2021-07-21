using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    List<GameObject> currentlyFilling = new List<GameObject>();

    [SerializeField] Transform playerLandParent;
    [SerializeField] Transform enemyLandParent;

    [SerializeField] Material seaMaterial;

    public static string currentTemp;
    string landTag;
    string otherPlayerLandTag;
    string temp1;
    string temp2;

    FloodFill floodFill;

    MeshRenderer meshRenderer;

    bool isFilling = false;

    #region Unity Methods
    void Start()
    {
        floodFill = GetComponent<FloodFill>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (floodFill.GetIsPlayer())
        {
            landTag = "PlayerLand";
            otherPlayerLandTag = "EnemyLand";
            temp1 = "Temp1";
            temp2 = "Temp2";
        }
        else
        {
            landTag = "EnemyLand";
            otherPlayerLandTag = "PlayerLand";
            temp1 = "Temp3";
            temp2 = "Temp4";
        }
    }
    void OnTriggerEnter(Collider other)
    {
        SeaHandler(other);
        if (other.gameObject.CompareTag("Wall") || other.gameObject.CompareTag(landTag))
            ListHandlers();
    }

    private void ListHandlers()
    {
        if (currentlyFilling.Count < 1) { return; }

        GameObject lastCube = currentlyFilling[0];
        floodFill.CheckAreas((int)lastCube.transform.position.x, (int)lastCube.transform.position.z);

        if (floodFill.checkArea1.Count < 1)
        {
            GameObject firstCheckObj = floodFill.initialCheckArea[0];
            floodFill.FillList1((int)firstCheckObj.transform.position.x, (int)firstCheckObj.transform.position.z);
        }
        if (floodFill.checkArea2.Count < 1)
        {
            GameObject secondCheckObj = floodFill.initialCheckArea[1];
            floodFill.FillList2((int)secondCheckObj.transform.position.x, (int)secondCheckObj.transform.position.z);
        }

        if (floodFill.checkArea1.Count > floodFill.checkArea2.Count)
        {
            floodFill.FillSmallArea((int)floodFill.checkArea2[0].transform.position.x, (int)floodFill.checkArea2[0].transform.position.z);
            currentTemp = temp2;
        }
        else
        {
            floodFill.FillSmallArea((int)floodFill.checkArea1[0].transform.position.x, (int)floodFill.checkArea1[0].transform.position.z);
            currentTemp = temp1;
        }

        for (int z = 0; z < FloodFill.HEIGHT; z++)
        {
            for (int x = 0; x < FloodFill.WIDTH; x++)
            {
                if (floodFill.field[x, z].tag == currentTemp || floodFill.field[x, z].tag == "Sea")
                {
                    floodFill.field[x, z].GetComponent<MeshRenderer>().material = meshRenderer.material;
                }

                if (floodFill.field[x, z].CompareTag(otherPlayerLandTag)) { return; }

                if (floodFill.field[x, z].tag == temp1 || floodFill.field[x, z].tag == temp2)
                {
                    floodFill.field[x, z].tag = "Sea";
                }
            }
        }
        if (floodFill.checkArea1.Count < 1 || floodFill.checkArea2.Count < 1) { return; }

        if (floodFill.checkArea1.Count > floodFill.checkArea2.Count)
        {
            for (int i = 1; i < floodFill.checkArea2.Count; i++)
            {
                floodFill.checkArea2[i].tag = landTag;
                floodFill.checkArea2[i].name = landTag;

                if (floodFill.GetIsPlayer())
                {
                    floodFill.checkArea2[i].transform.parent = playerLandParent;
                }
                else floodFill.checkArea2[i].transform.parent = enemyLandParent;
            }
            for (int i = 0; i < floodFill.checkArea1.Count; i++)
            {
                floodFill.checkArea1[i].tag = "Sea";

                floodFill.checkArea1[0].GetComponent<MeshRenderer>().material = seaMaterial;
            }
        }
        else
        {
            for (int i = 1; i < floodFill.checkArea1.Count; i++)
            {
                floodFill.checkArea1[i].tag = landTag;
                floodFill.checkArea1[i].name = landTag;

                if (floodFill.GetIsPlayer())
                {
                    floodFill.checkArea1[i].transform.parent = playerLandParent;
                }
                else floodFill.checkArea2[i].transform.parent = enemyLandParent;
            }
            for (int i = 0; i < floodFill.checkArea2.Count; i++)
            {
                floodFill.checkArea2[i].tag = "Sea";

                floodFill.checkArea2[0].GetComponent<MeshRenderer>().material = seaMaterial;
            }
        }
        floodFill.ResetParams();
    }

    private void SeaHandler(Collider other)
    {
        if (other.gameObject.CompareTag("Sea") || other.gameObject.CompareTag(otherPlayerLandTag))
        {
            GameObject otherObj = other.gameObject;
            otherObj.transform.position = new Vector3(otherObj.transform.position.x, -1f, otherObj.transform.position.z);

            otherObj.GetComponent<MeshRenderer>().material = meshRenderer.material;

            other.gameObject.tag = landTag;
            other.gameObject.name = landTag;

            if (floodFill.GetIsPlayer())
            {
                other.gameObject.transform.parent = playerLandParent;
            }
            else other.gameObject.transform.parent = enemyLandParent;
            isFilling = true;

            if (isFilling)
            {
                currentlyFilling.Add(other.gameObject);
            }
        }
        else
        {
            for (int i = 0; i < currentlyFilling.Count; i++)
            {
                currentlyFilling[i].transform.position = new Vector3(currentlyFilling[i].transform.position.x, 0f, currentlyFilling[i].transform.position.z);
                //currentlyFilling[i].tag = landTag;
            }

            isFilling = false;
            currentlyFilling.Clear();
        }
    }
    #endregion
}
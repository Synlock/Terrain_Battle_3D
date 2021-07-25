using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloodFillOld : MonoBehaviour
{
    [SerializeField] FloodFillOld otherPlayerFill;
    public const int WIDTH = 28;
    public const int HEIGHT = 50;
    public GameObject[,] field = new GameObject[WIDTH, HEIGHT];

    GameObject[,] fieldCopy1;
    GameObject[,] fieldCopy2;

    public List<GameObject> initialCheckArea = new List<GameObject>();
    public List<GameObject> checkArea1 = new List<GameObject>();
    public List<GameObject> checkArea2 = new List<GameObject>();

    //string landTag;
    string otherPlayerLandTag;
    string temp1;
    string temp2;

    [SerializeField] Transform wallParent;
    [SerializeField] Transform seaParent;

    [SerializeField] GameObject wall;
    [SerializeField] GameObject sea;

    [SerializeField] bool isPlayer = true;

    MeshRenderer meshRenderer;

    #region Start/Update

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        fieldCopy1 = new GameObject[WIDTH, HEIGHT];
        fieldCopy2 = new GameObject[WIDTH, HEIGHT];

        if (isPlayer)
        {
            //landTag = "PlayerLand";
            otherPlayerLandTag = "EnemyLand";
            temp1 = "Temp1";
            temp2 = "Temp2";
        }
        else
        {
            //landTag = "EnemyLand";
            otherPlayerLandTag = "PlayerLand";
            temp1 = "Temp3";
            temp2 = "Temp4";
        }
        if (isPlayer)
        {
            Init();
        }
        else field = otherPlayerFill.field;
    }

    private void Init()
    {
        int x_start = 0;
        int z_start = 0;

        int current_pos_x = x_start;
        int current_pos_z = z_start;

        for (int z = 0; z < HEIGHT; z++, current_pos_z++, current_pos_x = x_start)
        {
            for (int x = 0; x < WIDTH; x++, current_pos_x++)
            {
                field[x, z] =
                    (x < 1 || x > WIDTH - 2 || z < 1 || z > HEIGHT - 2) ?
                    Instantiate(wall, new Vector3(current_pos_x, 0f, current_pos_z), Quaternion.identity, wallParent) :
                    Instantiate(sea, new Vector3(current_pos_x, 0f, current_pos_z), Quaternion.identity, seaParent);
            }
        }
    }

    #endregion

    #region Getters/Setters

    public bool GetIsPlayer()
    {
        return isPlayer;
    }

    #endregion

    #region Methods

    public void ResetParams()
    {
        initialCheckArea.Clear();
        checkArea1.Clear();
        checkArea2.Clear();
    }

    public void CheckAreas(int x, int z)
    {
        //need to run a flood fill to set the first side that got checked to list 1
        //run another flood fill on other side to set to list 2
        //determine which side is small
        //set flood fill to change colors of cubes

        /*if (!field[x, z].CompareTag(landTag)) { return; }

        if (field[x + 1, z].CompareTag("Sea") || field[x + 1, z].CompareTag(otherPlayerLandTag))
        {
            CheckAreas(x + 1, z);

            initialCheckArea.Add(field[x + 1, z]);
        }

        if (field[x - 1, z].CompareTag("Sea") || field[x - 1, z].CompareTag(otherPlayerLandTag))
        {
            CheckAreas(x - 1, z);

            initialCheckArea.Add(field[x - 1, z]);
        }

        if (field[x, z + 1].CompareTag("Sea") || field[x, z + 1].CompareTag(otherPlayerLandTag))
        {
            CheckAreas(x, z + 1);

            initialCheckArea.Add(field[x, z + 1]);
        }

        if (field[x, z - 1].CompareTag("Sea") || field[x, z - 1].CompareTag(otherPlayerLandTag))
        {
            CheckAreas(x, z - 1);

            initialCheckArea.Add(field[x, z - 1]);
        }*/
        if (field[x + 1, z].CompareTag("Sea") || field[x + 1, z].CompareTag(otherPlayerLandTag))
        {
            initialCheckArea.Add(field[x + 1, z]);
            initialCheckArea.Add(field[x - 1, z]);
        }
        if (field[x, z + 1].CompareTag("Sea") || field[x, z + 1].CompareTag(otherPlayerLandTag))
        {
            initialCheckArea.Add(field[x, z + 1]);
            initialCheckArea.Add(field[x, z - 1]);
        }
    }

    public void FillList1(int x, int z)
    {
        Array.Copy(field, fieldCopy1, field.Length);

        FillList1Recursive(x,z);
    }
    void FillList1Recursive(int x, int z)
    {
        if (!fieldCopy1[x, z].CompareTag("Sea") && fieldCopy1[x, z].CompareTag(temp1)) { return; }

        fieldCopy1[x, z].tag = temp1;

        checkArea1.Add(fieldCopy1[x, z]);

        if (fieldCopy1[x + 1, z].CompareTag("Sea") || fieldCopy1[x + 1, z].CompareTag(otherPlayerLandTag))
        {
            FillList1Recursive(x + 1, z);
            checkArea1.Add(fieldCopy1[x + 1, z]);
        }

        if (fieldCopy1[x - 1, z].CompareTag("Sea") || fieldCopy1[x - 1, z].CompareTag(otherPlayerLandTag))
        {
            FillList1Recursive(x - 1, z);
            checkArea1.Add(fieldCopy1[x - 1, z]);
        }

        if (fieldCopy1[x, z + 1].CompareTag("Sea") || fieldCopy1[x, z + 1].CompareTag(otherPlayerLandTag))
        {
            FillList1Recursive(x, z + 1);
            checkArea1.Add(fieldCopy1[x, z + 1]);
        }

        if (fieldCopy1[x, z - 1].CompareTag("Sea") || fieldCopy1[x, z - 1].CompareTag(otherPlayerLandTag))
        {
            FillList1Recursive(x, z - 1);
            checkArea1.Add(fieldCopy1[x, z - 1]);
        }
        return;

    }
    public void FillList2(int x, int z)
    {
        Array.Copy(field, fieldCopy2, field.Length);

        FillList2Recursive(x,z);
    }
    void FillList2Recursive(int x, int z)
    {
        if (!fieldCopy2[x, z].CompareTag("Sea") && fieldCopy2[x, z].CompareTag(temp2)) { return; }

        fieldCopy2[x, z].tag = temp2;

        checkArea2.Add(fieldCopy2[x, z]);

        if (fieldCopy2[x + 1, z].CompareTag("Sea") || fieldCopy2[x + 1, z].CompareTag(otherPlayerLandTag))
        {
            FillList2Recursive(x + 1, z);
            checkArea2.Add(fieldCopy2[x + 1, z]);
        }

        if (fieldCopy2[x - 1, z].CompareTag("Sea") || fieldCopy2[x - 1, z].CompareTag(otherPlayerLandTag))
        {
            FillList2Recursive(x - 1, z);
            checkArea2.Add(fieldCopy2[x - 1, z]);
        }

        if (fieldCopy2[x, z + 1].CompareTag("Sea") || fieldCopy2[x, z + 1].CompareTag(otherPlayerLandTag))
        {
            FillList2Recursive(x, z + 1);
            checkArea2.Add(fieldCopy2[x, z + 1]);
        }

        if (fieldCopy2[x, z - 1].CompareTag("Sea") || fieldCopy2[x, z - 1].CompareTag(otherPlayerLandTag))
        {
            FillList2Recursive(x, z - 1);
            checkArea2.Add(fieldCopy2[x, z - 1]);
        }

        return;

    }
    public void FillSmallArea(int x, int z)
    {

        if (!field[x, z].CompareTag("Sea")) { return; }

        if (field[x + 1, z].CompareTag("Sea") || field[x + 1, z].CompareTag(otherPlayerLandTag))
        {
            FillSmallArea(x + 1, z);
        }

        if (field[x - 1, z].CompareTag("Sea") || field[x - 1, z].CompareTag(otherPlayerLandTag))
        {
            FillSmallArea(x - 1, z);
        }

        if (field[x, z + 1].CompareTag("Sea") || field[x, z + 1].CompareTag(otherPlayerLandTag))
        {
            FillSmallArea(x, z + 1);
        }

        if (field[x, z - 1].CompareTag("Sea") || field[x, z - 1].CompareTag(otherPlayerLandTag))
        {
            FillSmallArea(x, z - 1);
        }

    }

    #endregion
}
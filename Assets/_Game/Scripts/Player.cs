using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    //TODO: 1. create trail handler
    //2. check if flood fill is needed
    //3. get initial check area
    //4. call FillList1()
    [SerializeField] string myTag;
    [SerializeField] Color myColor;

    List<GameObject> initialCheckArea = new List<GameObject>();

    GridMovement gridMovement;

    void Start()
    {
        gridMovement = GetComponent<GridMovement>();
        gridMovement.OnStep += OnStepHandler;
    }

    private void OnStepHandler(object sender, System.EventArgs e)
    {
        GridMovement.StepEventArgs se = e as GridMovement.StepEventArgs;

        GameObject nextObj;

        if (initialCheckArea.Count < 1)
        {
            int x = (int)transform.position.x;
            int z = (int)transform.position.z;

            GameManager.field[x,z].GetComponent<MeshRenderer>().material.color = myColor;
            GameManager.field[x,z].tag = myTag;

            if (!(GameManager.field[x + 1, z].CompareTag("Wall") || GameManager.field[x + 1, z].CompareTag(myTag)))
            {
                initialCheckArea.Add(GameManager.field[x + 1, z]);
                initialCheckArea.Add(GameManager.field[x - 1, z]);
            }
            if (!(GameManager.field[x, z + 1].CompareTag("Wall") || GameManager.field[x, z + 1].CompareTag(myTag)))
            {
                initialCheckArea.Add(GameManager.field[x, z + 1]);
                initialCheckArea.Add(GameManager.field[x, z - 1]);
            }
        }

        if (se.Direction == Vector3.right)
            nextObj = GameManager.field[(int)transform.position.x + 1, (int)transform.position.z];
        else if (se.Direction == Vector3.left)
            nextObj = GameManager.field[(int)transform.position.x - 1, (int)transform.position.z];
        else if (se.Direction == Vector3.forward)
            nextObj = GameManager.field[(int)transform.position.x, (int)transform.position.z + 1];
        else
            nextObj = GameManager.field[(int)transform.position.x, (int)transform.position.z - 1];


        if (!(nextObj.CompareTag("Wall") || nextObj.CompareTag(myTag))) { return; }


        int fill0 = CheckBothAreasSize((int)initialCheckArea[0].transform.position.x, (int)initialCheckArea[0].transform.position.z);
        int fill1 = CheckBothAreasSize((int)initialCheckArea[1].transform.position.x, (int)initialCheckArea[1].transform.position.z);

        if (fill0 < fill1)
        {
            FillSmallArea((int)initialCheckArea[0].transform.position.x, (int)initialCheckArea[0].transform.position.z);
        }
        else
        {
            FillSmallArea((int)initialCheckArea[1].transform.position.x, (int)initialCheckArea[1].transform.position.z);
        }
        initialCheckArea.Clear();
    }

    List<GameObject> checkArea1 = new List<GameObject>();
    GameObject[,] fieldCopy1 = new GameObject[GameManager.WIDTH, GameManager.HEIGHT];
    public int CheckBothAreasSize(int x, int z)
    {
        Array.Copy(GameManager.field, fieldCopy1, fieldCopy1.Length);

        FloodFill(x, z);

        int count = checkArea1.Count;
        checkArea1.Clear();

        return count;
    }
    void FloodFill(int x, int z)
    {
        if (fieldCopy1[x, z].CompareTag("Wall") || fieldCopy1[x, z].CompareTag(myTag)) { return; }

        checkArea1.Add(fieldCopy1[x, z]);

        FloodFill(x + 1, z);
        FloodFill(x - 1, z);
        FloodFill(x, z + 1);
        FloodFill(x, z - 1);

        return;
    }
    void FillSmallArea(int x, int z)
    {
        if (GameManager.field[x, z].CompareTag("Wall") || GameManager.field[x, z].CompareTag(myTag)) { return; }

        GameManager.field[x, z].GetComponent<MeshRenderer>().material.color = myColor;
        GameManager.field[x, z].tag = myTag;

        FillSmallArea(x + 1, z);
        FillSmallArea(x - 1, z);
        FillSmallArea(x, z + 1);
        FillSmallArea(x, z - 1);

        return;
    }
}
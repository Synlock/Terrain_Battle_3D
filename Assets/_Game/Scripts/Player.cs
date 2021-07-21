﻿using System;
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
    [SerializeField] string myTrailTag;
    [SerializeField] Color myColor;

    List<GameObject> initialCheckArea = new List<GameObject>();

    List<GameObject> trailObjs = new List<GameObject>();

    bool hitWall = false;

    GridMovement gridMovement;

    void Start()
    {
        gridMovement = GetComponent<GridMovement>();
        
        gridMovement.BeforeStep += BeforeStepHandler;
        gridMovement.AfterStep += AfterStepHandler;
    }

    private void BeforeStepHandler(object sender, EventArgs e)
    {
        GridMovement.StepEventArgs se = e as GridMovement.StepEventArgs;

        GameObject nextObj;

        if (se.Direction == Vector3.right)
            nextObj = GameManager.field[(int)transform.position.x + 1, (int)transform.position.z];
        else if (se.Direction == Vector3.left)
            nextObj = GameManager.field[(int)transform.position.x - 1, (int)transform.position.z];
        else if (se.Direction == Vector3.forward)
            nextObj = GameManager.field[(int)transform.position.x, (int)transform.position.z + 1];
        else
            nextObj = GameManager.field[(int)transform.position.x, (int)transform.position.z - 1];

        if (nextObj.CompareTag("Wall"))
            hitWall = true;

        if (nextObj.CompareTag("Wall") || nextObj.CompareTag(myTag)) { return; }

        trailObjs.Add(nextObj);
        nextObj.GetComponent<MeshRenderer>().material.color = myColor;
        nextObj.tag = myTag;
    }

    private void AfterStepHandler(object sender, EventArgs e)
    {
        //GridMovement.StepEventArgs se = e as GridMovement.StepEventArgs;

        if (initialCheckArea.Count < 1)
        {
            int x = (int)transform.position.x;
            int z = (int)transform.position.z;

            if (!(GameManager.field[(int)trailObjs[0].transform.position.x + 1, (int)trailObjs[0].transform.position.z].CompareTag("Wall") ||
                    GameManager.field[(int)trailObjs[0].transform.position.x + 1, (int)trailObjs[0].transform.position.z].CompareTag(myTag)))
            {
                initialCheckArea.Add(GameManager.field[x + 1, z]);
                initialCheckArea.Add(GameManager.field[x - 1, z]);
            }
            else if (!(GameManager.field[(int)trailObjs[0].transform.position.x, (int)trailObjs[0].transform.position.z + 1].CompareTag("Wall") ||
                        GameManager.field[(int)trailObjs[0].transform.position.x, (int)trailObjs[0].transform.position.z + 1].CompareTag(myTag)))
            {
                initialCheckArea.Add(GameManager.field[x, z + 1]);
                initialCheckArea.Add(GameManager.field[x, z - 1]);
            }
        }

        /*if (se.Direction == Vector3.right)
            nextObj = GameManager.field[(int)transform.position.x + 1, (int)transform.position.z];
        else if (se.Direction == Vector3.left)
            nextObj = GameManager.field[(int)transform.position.x - 1, (int)transform.position.z];
        else if (se.Direction == Vector3.forward)
            nextObj = GameManager.field[(int)transform.position.x, (int)transform.position.z + 1];
        else
            nextObj = GameManager.field[(int)transform.position.x, (int)transform.position.z - 1];*/


        if (!(GameManager.field[(int)transform.position.x, (int)transform.position.z].CompareTag("Wall") || 
                GameManager.field[(int)transform.position.x, (int)transform.position.z].CompareTag(myTag))) { return; }

        int fill0 = CheckAreaSize((int)initialCheckArea[0].transform.position.x, (int)initialCheckArea[0].transform.position.z);
        int fill1 = CheckAreaSize((int)initialCheckArea[1].transform.position.x, (int)initialCheckArea[1].transform.position.z);

        if (fill0 < fill1)
        {
            FillSmallArea((int)initialCheckArea[0].transform.position.x, (int)initialCheckArea[0].transform.position.z);
        }
        else
        {
            FillSmallArea((int)initialCheckArea[1].transform.position.x, (int)initialCheckArea[1].transform.position.z);
        }
        initialCheckArea.Clear();
        for (int i = 0; i < trailObjs.Count; i++)
        {
            trailObjs[i].tag = myTag;
        }
    }

    List<GameObject> checkArea1 = new List<GameObject>();
    GameObject[,] fieldCopy1 = new GameObject[GameManager.WIDTH, GameManager.HEIGHT];
    public int CheckAreaSize(int x, int z)
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
        if (hitWall)
        {
            checkArea1.Add(fieldCopy1[x, z]);

            FloodFill(x + 1, z);
            FloodFill(x - 1, z);
            FloodFill(x, z + 1);
            FloodFill(x, z - 1);
        }

        return;
    }
    void FillSmallArea(int x, int z)
    {
        if (GameManager.field[x, z].CompareTag("Wall") || GameManager.field[x, z].CompareTag(myTag)) { return; }

        GameManager.field[x, z].GetComponent<MeshRenderer>().material.color = myColor;
        GameManager.field[x, z].tag = myTag;

        FloodFill(x + 1, z);
        FloodFill(x - 1, z);
        FloodFill(x, z + 1);
        FloodFill(x, z - 1);

        return;
    }
}
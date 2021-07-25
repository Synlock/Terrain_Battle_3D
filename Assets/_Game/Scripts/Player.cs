using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GridMovement))]
[RequireComponent(typeof(MeshRenderer))]
public class Player : MonoBehaviour
{
    public Color myColor { get; private set; }
    public float percent;

    List<Vector3> Trail = new List<Vector3>();
    List<Color> tileColorsBeforeOwn = new List<Color>();

    List<Tile> tilesOwned = new List<Tile>();
    [SerializeField] Transform tilesParent;


    bool canFill = true;

    GridMovement gridMovement;

    #region Unity Methods
    void Start()
    {
        gridMovement = GetComponent<GridMovement>();
        gridMovement.DownBound = 0;
        gridMovement.LeftBound = 0;
        gridMovement.UpBound = GameManager.HEIGHT - 1;
        gridMovement.RightBound = GameManager.WIDTH - 1;

        myColor = GetComponent<MeshRenderer>().material.color;

        gridMovement.BeforeStep += BeforeStepHandler;
        gridMovement.AfterStep += AfterStepHandler;
    }
    void Update()
    {
        CalculatePercentOfTilesOwned();
    }
    #endregion

    #region Before/After Step Handlers
    void BeforeStepHandler(object sender, EventArgs e)
    {
        Tile obj = GameManager.GetFieldPosition(transform);

        if (obj.IsWall || obj.Owner == this)
        {
            canFill = true;
            gridMovement.BlockReverse = false;
            return;
        }

        if (!canFill) return;

        tileColorsBeforeOwn.Add(GameManager.GetFieldPosition(transform.position).color);

        obj.Owner = this;
        obj.IsTrail = true;
        obj.tilePos = new Vector3(obj.gameObject.transform.position.x, -1f, obj.gameObject.transform.position.z);
        obj.gameObject.transform.parent = tilesParent;

        Trail.Add(transform.position);
    }
    void AfterStepHandler(object sender, EventArgs e)
    { 
        Tile obj = GameManager.GetFieldPosition(transform);

        if (obj.Owner == this && obj.IsTrail)
            SelfCollisionHandler();

        if (obj.IsTrail)
            OtherCollisionHandler(obj);

        if (!(obj.IsWall || obj.Owner == this))
        {
            gridMovement.BlockReverse = true;
            return;
        }
        if (Trail.Count == 0) return;
        
        if (!canFill) return;
        // flood fill
        (Vector2Int, Vector2Int) InitialTiles = GetInitialTiles();
        int size1 = CheckAreaSize(InitialTiles.Item1);
        int size2 = CheckAreaSize(InitialTiles.Item2);

        if (size1 < size2)
            FillSmallArea(InitialTiles.Item1);
        else
            FillSmallArea(InitialTiles.Item2);

        OccupyTrail();
        tileColorsBeforeOwn.Clear();
        gridMovement.BlockReverse = false;
    }
    #endregion

    #region Trail Collision Handlers
    void SelfCollisionHandler()
    {
        foreach (Vector3 v in Trail)
        {
            GameManager.GetFieldPosition(v).IsTrail = false;
            GameManager.GetFieldPosition(v).Owner = null;
            for (int i = 0; i < tileColorsBeforeOwn.Count; i++)
            {
                GameManager.GetFieldPosition(Trail[i]).color = tileColorsBeforeOwn[i];

                Vector3 startTilePos = GameManager.GetFieldPosition(Trail[i]).tilePos;
                GameManager.GetFieldPosition(Trail[i]).tilePos = new Vector3(startTilePos.x, 0f, startTilePos.z);
            }
        }
        canFill = false;
        Trail.Clear();
        tileColorsBeforeOwn.Clear();
    }
    void OtherCollisionHandler(Tile obj)
    {
        if (obj.Owner == this) return;

        Player other = obj.Owner;

        foreach (Vector3 v in other.Trail)
        {
            GameManager.GetFieldPosition(v).IsTrail = false;
            GameManager.GetFieldPosition(v).Owner = null;
            for (int i = 0; i < other.tileColorsBeforeOwn.Count; i++)
            {
                GameManager.GetFieldPosition(other.Trail[i]).color = other.tileColorsBeforeOwn[i];
            }
        }
        other.canFill = false;
        other.Trail.Clear();
        other.tileColorsBeforeOwn.Clear();
    }
    #endregion

    #region Tile Percentage Handlers
    void StealTerrainHandler(Tile t)
    {
        if (t.Owner != null && t.Owner != this)
        {
            Player other = GameManager.GetFieldPosition(t.tilePos).Owner;
            other.tilesOwned.Remove(t);
        }
    }
    void CalculatePercentOfTilesOwned()
    {
        percent = tilesOwned.Count * 100 / GameManager.tilesCounter;

        //TODO: fix temporary fix for the extra 3 percent that is calculated  
        if (tilesParent.childCount < 1)
        {
            percent = 0;
            tilesOwned.Clear();
        }
    }
    #endregion

    #region Flood Fill Handlers
    void OccupyTrail()
    {
        foreach (Vector3 v in Trail)
        {
            Vector3 tilePos = GameManager.GetFieldPosition(v).tilePos;
            GameManager.GetFieldPosition(v).tilePos = new Vector3(tilePos.x, 0f, tilePos.z);

            tilesOwned.Add(GameManager.GetFieldPosition(v));
            GameManager.GetFieldPosition(v).IsTrail = false;
        }
        Trail.Clear();
    }

    (Vector2Int, Vector2Int) GetInitialTiles()
    {
        Vector3 first = Trail[0];

        // This needs to be much, much more complicated.
        if (!(GameManager.GetFieldPosition(first.x + 1, first.z).IsWall ||
            GameManager.GetFieldPosition(first.x + 1, first.z).Owner == this))
        {
            return (new Vector2Int((int)first.x + 1, (int)first.z),
                new Vector2Int((int)first.x - 1, (int)first.z));
        }
        else if (!(GameManager.GetFieldPosition(first.x, first.z + 1).IsWall ||
                    GameManager.GetFieldPosition(first.x, first.z - 1).Owner == this))
        {
            return (new Vector2Int((int)first.x, (int)first.z + 1),
                new Vector2Int((int)first.x, (int)first.z - 1));
        }

        return (Vector2Int.zero, Vector2Int.zero);
    }
    bool[,] GetFieldCopy()
    {
        return GameManager.field.Select(t => t.IsWall || t.Owner == this);
    }
    private void MarkBool (ref bool b)
    {
        b = true;
    }
    public int CheckAreaSize(Vector2Int v)
    {
        FloodFill<bool> ff = new FloodFill<bool>(GetFieldCopy(), b => b, MarkBool);
        return ff.Start(v.x, v.y);
    }
    private void MarkTile(ref Tile t)
    {
        StealTerrainHandler(t);
        
        t.Owner = this;
        t.IsTrail = false;
        t.tilePos = new Vector3(t.gameObject.transform.position.x, 0f, t.gameObject.transform.position.z);
        tilesOwned.Add(t);

        t.gameObject.transform.parent = tilesParent;
    }
    void FillSmallArea(Vector2Int v)
    {
        FloodFill<Tile> ff = new FloodFill<Tile>(GameManager.field,
            tile => tile.IsWall || tile.Owner == this, MarkTile);
        ff.Start(v.x, v.y);
    }
    #endregion

}
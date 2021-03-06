using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridMovement))]
[RequireComponent(typeof(MeshRenderer))]
public class Player : MonoBehaviour
{
    //TODO: when trying to fill this player's island that is not connected to a wall, the entire field will fill with this color.
    // during the flood fill check of both points - check to see if any two points overlap - need to create a list of each flood fill
    // if any two points overlap, stop filling at tile.Owner != this or tile.Owner == null

    //TODO: when crashing into other trail convert all tiles to owner == this - possibly complete
    //TODO: filling over other player while they are on their own land and they are moving - need to set their canFill = false

    #region Member Variables
    public Color myColor { get; private set; }
    [SerializeField] Color[] colorEachLevel = { };
    Color defaultColor;

    public float percent;

    List<Vector3> Trail = new List<Vector3>();
    List<Color> tileColorsBeforeOwn = new List<Color>();

    public Transform tilesParent;
    [SerializeField] Transform defaultTilesParent;

    [Header("FX")]
    [SerializeField] ParticleSystem trailFX;
    [SerializeField] ParticleSystem wallCrashFX;
    [SerializeField] ParticleSystem trailCrashFX;
    [SerializeField] ParticleSystem powerUpFX;


    public Slider slider;

    public bool canFill = true;
    bool onMyLand = false;

    [HideInInspector] public GridMovement gridMovement;
    [HideInInspector] public MeshRenderer meshRenderer;
    #endregion

    #region Unity Methods
    void Awake()
    {
        ColorHandler();
    }
    void Start()
    {
        gridMovement = GetComponent<GridMovement>();

        HandleFXOnStart();

        var mm = trailCrashFX.main;
        mm.startColor = meshRenderer.material.color;

        gridMovement.DownBound = 0;
        gridMovement.LeftBound = 0;
        gridMovement.UpBound = GameManager.HEIGHT - 1;
        gridMovement.RightBound = GameManager.WIDTH - 1;

        gridMovement.BeforeStep += BeforeStepHandler;
        gridMovement.AfterStep += AfterStepHandler;
    }

    void Update()
    {
        CalculatePercentOfTilesOwned();
        FilledOverNotMovingHandler();

        if (gridMovement.animator.GetBool("isMoving"))
        {
            trailFX.gameObject.SetActive(true);
        }
        else trailFX.gameObject.SetActive(false);
    }
    #endregion

    #region Before/After Step Handlers
    void BeforeStepHandler(object sender, EventArgs e)
    {
        Tile obj = GameManager.GetFieldPosition(transform);

        if (obj.Owner != this)
            onMyLand = false;
        else onMyLand = true;

        if (obj.IsWall || obj.Owner == this)
        {
            canFill = true;
            gridMovement.BlockReverse = false;

            return;
        }

        FilledOverMovingHandler();

        if (!canFill) return;

        tileColorsBeforeOwn.Add(GameManager.GetFieldPosition(transform.position).color);

        obj.Owner = this;
        obj.IsTrail = true;
        obj.tilePos = new Vector3(obj.gameObject.transform.position.x, -1f, obj.gameObject.transform.position.z);

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
            if (canFill)
                gridMovement.BlockReverse = true;
            else gridMovement.BlockReverse = false;

            return;
        }
        else gridMovement.moves = 0;

        if (Trail.Count == 0 || !canFill) return;

        // flood fill
        (Vector2Int, Vector2Int) InitialTiles = GetInitialTiles();
        int size1 = CheckAreaSize(InitialTiles.Item1);
        int size2 = CheckAreaSize(InitialTiles.Item2);

        if (size1 < size2)
            FillSmallArea(InitialTiles.Item1);
        else
            FillSmallArea(InitialTiles.Item2);

        OccupyTrail();
    }
    #endregion

    #region Collision Handlers
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
        
        if(trailCrashFX != null)
        {
            
            trailCrashFX.Play(); 
        }
    }
    void OtherCollisionHandler(Tile obj)
    {
        if (obj.Owner == this) return;

        Player other = obj.Owner;

        foreach (Vector3 v in other.Trail)
        {
            GameManager.GetFieldPosition(v).IsTrail = false;
            GameManager.GetFieldPosition(v).Owner = GameManager.GetFieldPosition(v).previousOwner;
            for (int i = 0; i < other.tileColorsBeforeOwn.Count; i++)
            {
                GameManager.GetFieldPosition(other.Trail[i]).color = other.tileColorsBeforeOwn[i];

                Vector3 trailTilePos = GameManager.GetFieldPosition(other.Trail[i]).tilePos;
                GameManager.GetFieldPosition(other.Trail[i]).tilePos = new Vector3(trailTilePos.x, 0f, trailTilePos.z);
            }
        }
        other.canFill = false;
        other.Trail.Clear();
        other.tileColorsBeforeOwn.Clear();
        other.gridMovement.BlockReverse = false;
        
        if(trailCrashFX != null)
            trailCrashFX.Play();
    }
    void FilledOverMovingHandler()
    {
        if (Trail.Count > 0)
        {
            for (int i = 0; i < Trail.Count; i++)
            {
                if (GameManager.GetFieldPosition(Trail[i]).Owner != this)
                {
                    tileColorsBeforeOwn.Clear();
                    Trail.Clear();
                    canFill = false;
                }
            }
        }
    }
    void FilledOverNotMovingHandler()
    {
        if (onMyLand &&
            GameManager.GetFieldPosition(transform).Owner != this &&
            GameManager.GetFieldPosition(transform).Owner != null &&
            !GameManager.GetFieldPosition(transform).IsWall &&
            gridMovement.moves < 1)
            canFill = false;
    }
    #endregion

    #region Tile Percentage Handlers
    void CalculatePercentOfTilesOwned()
    {
        percent = tilesParent.childCount * 100 / GameManager.tilesCounter;
    }
    #endregion

    #region Flood Fill Handlers
    void OccupyTrail()
    {
        foreach (Vector3 v in Trail)
        {
            Vector3 tilePos = GameManager.GetFieldPosition(v).tilePos;
            GameManager.GetFieldPosition(v).tilePos = new Vector3(tilePos.x, 0f, tilePos.z);
            GameManager.GetFieldPosition(v).IsTrail = false;
            GameManager.GetFieldPosition(v).myParentTransform = tilesParent;
        }
        Trail.Clear();
        tileColorsBeforeOwn.Clear();

        gridMovement.BlockReverse = false;

        if (wallCrashFX != null)
        {
            wallCrashFX.Play();
        }
    }

    (Vector2Int, Vector2Int) GetInitialTiles()
    {
        bool readyToFill = false;
        for (int i = 0; i < Trail.Count; i++)
        {
            //loop through trail list to find fill points
            Vector3 first = new Vector3();
            if (!(GameManager.GetFieldPosition(Trail[i].x + 1, Trail[i].z).IsWall ||
                GameManager.GetFieldPosition(Trail[i].x + 1, Trail[i].z).Owner == this) &&
                !(GameManager.GetFieldPosition(Trail[i].x - 1, Trail[i].z).IsWall ||
                GameManager.GetFieldPosition(Trail[i].x - 1, Trail[i].z).Owner == this))
            {
                first = Trail[i];
                readyToFill = true;
            }
            else if (!(GameManager.GetFieldPosition(Trail[i].x, Trail[i].z + 1).IsWall ||
                GameManager.GetFieldPosition(Trail[i].x, Trail[i].z + 1).Owner == this) &&
                !(GameManager.GetFieldPosition(Trail[i].x, Trail[i].z - 1).IsWall ||
                GameManager.GetFieldPosition(Trail[i].x, Trail[i].z - 1).Owner == this))
            {
                first = Trail[i];
                readyToFill = true;
            }

            if (readyToFill)
            {
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
            }
        }
        return (Vector2Int.zero, Vector2Int.zero);
    }
    bool[,] GetFieldCopy()
    {
        return GameManager.field.Select(t => t.IsWall || t.Owner == this);
    }
    private void MarkBool(ref bool b)
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
        t.Owner = this;
        t.IsTrail = false;
        t.tilePos = new Vector3(t.gameObject.transform.position.x, 0f, t.gameObject.transform.position.z);
        t.gameObject.transform.parent = tilesParent;
    }
    void FillSmallArea(Vector2Int v)
    {
        FloodFill<Tile> ff = new FloodFill<Tile>(GameManager.field,
            tile => tile.IsWall || tile.Owner == this, MarkTile);
        ff.Start(v.x, v.y);
    }
    #endregion
    
    #region Methods
    void ColorHandler()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        myColor = meshRenderer.material.color;
        defaultColor = myColor;

        if (colorEachLevel.Length > LoadScenesManager.currentLevel)
            myColor = colorEachLevel[LoadScenesManager.currentLevel];
        else
            myColor = defaultColor;

        meshRenderer.material.color = myColor;
    }
    void HandleFXOnStart()
    {
        if (trailFX != null)
        {
            ParticleSystem.MainModule mm = trailFX.main;
            mm.startColor = meshRenderer.material.color;
        }
        if(wallCrashFX != null)
        {
            wallCrashFX.gameObject.SetActive(true);

            ParticleSystem.MainModule mm = wallCrashFX.main;
            mm.playOnAwake = false;
        }
        if (trailCrashFX != null)
        {
            trailCrashFX.gameObject.SetActive(true);

            ParticleSystem.MainModule mm = trailCrashFX.main;
            mm.playOnAwake = false;
        }
    }
    #endregion
}
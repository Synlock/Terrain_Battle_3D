using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    [SerializeField] Vector3[] tiles = { };

    [SerializeField] Material obstacleMat;
    void Start()
    {
        SetObstaclesOnStart();

        GameManager.tilesCounter -= tiles.Length;
    }
    
    void SetObstaclesOnStart()
    {
        foreach(Vector3 tilePos in tiles)
        {
            GameManager.GetFieldPosition(tilePos).IsWall = true;
            GameManager.GetFieldPosition(tilePos).meshRenderer.material = obstacleMat;

            Vector3 pos = GameManager.GetFieldPosition(tilePos).tilePos;
            GameManager.GetFieldPosition(tilePos).tilePos = new Vector3(pos.x, 1f, pos.z);

            Destroy(GameManager.GetFieldPosition(tilePos).gameObject.GetComponent<EndingRotator>());

        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public const int WIDTH = 28;
    public const int HEIGHT = 50;

    public static GameObject[,] field = new GameObject[WIDTH, HEIGHT];

    [SerializeField] GameObject wall;
    [SerializeField] Transform wallParent;

    [SerializeField] GameObject sea;
    [SerializeField] Transform seaParent;

    void Start()
    {
        Init();
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
}

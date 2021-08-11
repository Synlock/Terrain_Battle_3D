using UnityEngine;

public class GameManager : MonoBehaviour
{
    public const int WIDTH = 26;
    public const int HEIGHT = 40;

    public static int percentToWin = 80;
    public static bool isGameOver = false;
    public static bool hasGameStarted = false;

    public static Tile[,] field = new Tile[WIDTH, HEIGHT];

    public static Player[] players;

    public static int tilesCounter = 0;
    int wallCounter;

    [SerializeField] GameObject wall;
    [SerializeField] Transform wallParent;

    [SerializeField] GameObject sea;
    [SerializeField] Transform seaParent;

    void Awake()
    {
        //TODO: fix to automatically fill without string
        players = new Player[] { GameObject.Find("Player").GetComponent<Player>(), GameObject.Find("Enemy").GetComponent<Player>() };

        Init();
    }
    private void Init()
    {
        isGameOver = false;

        int x_start = 0;
        int z_start = 0;

        int current_pos_x = x_start;
        int current_pos_z = z_start;

        for (int z = 0; z < HEIGHT; z++, current_pos_z++, current_pos_x = x_start)
        {
            for (int x = 0; x < WIDTH; x++, current_pos_x++)
            {
                if (x < 1 || x > WIDTH - 2 || z < 1 || z > HEIGHT - 2)
                {
                    field[x, z] = new Tile()
                    {
                        gameObject = Instantiate(wall, new Vector3(current_pos_x, 0f, current_pos_z), Quaternion.identity, wallParent),
                        Owner = null,
                        IsTrail = false,
                        IsWall = true,
                    };
                    wallCounter++;
                }
                else
                {
                    field[x, z] = new Tile()
                    {
                        gameObject = Instantiate(sea, new Vector3(current_pos_x, 0f, current_pos_z), Quaternion.identity, seaParent),
                        Owner = null,
                        IsTrail = false,
                        IsWall = false,
                    };
                }
                tilesCounter = field.Length - wallCounter;
            }
        }
    }

    #region Helpers
    public static Tile GetFieldPosition(int x, int z)
    {
        return field[x, z];
    }
    public static Tile GetFieldPosition(double x, double z)
    {
        return GetFieldPosition((int)x, (int)z);
    }
    public static Tile GetFieldPosition(Vector3 pos)
    {
        return GetFieldPosition(pos.x, pos.z);
    }
    public static Tile GetFieldPosition(Transform t)
    {
        return GetFieldPosition(t.position);
    }
    #endregion

}
using System;

public class FloodFill<T>
{
    public Predicate<T> StopCondition;
    public T[,] Terrain;
    public ActionRef<T> Mark;
    private int count = 0;

    public FloodFill(T[,] terrain, Predicate<T> stopCondition, ActionRef<T> mark)
    {
        Terrain = terrain;
        StopCondition = stopCondition;
        Mark = mark;
    }

    public int Start(int x, int z)
    {
        count = 0;
        algo(x, z);
        return count;
    }

    void algo(int x, int z)
    {
        if (x < 0 || x >= Terrain.GetLength(0)) return;
        if (z < 0 || z >= Terrain.GetLength(1)) return;
        if (StopCondition(Terrain[x, z])) return;

        Mark(ref Terrain[x, z]);
        count++;
        
        algo(x + 1, z);
        algo(x - 1, z);
        algo(x, z + 1);
        algo(x, z - 1);
    }

}
using System;
using UnityEngine;
using System.Linq;

public class Tile
{
    public GameObject gameObject;
    public Player previousOwner;
    private Player _owner;
    public Player Owner
    {
        get => _owner;
        set
        {
            previousOwner = _owner;
            _owner = value;
            if (_owner != null)
            {
                color = _owner.myColor;
            }
        }
    }

    public bool IsTrail;
    public bool IsWall;

    public Color color
    {
        get
        {
            return gameObject.GetComponent<MeshRenderer>().material.color;
        }
        set
        {
            gameObject.GetComponent<MeshRenderer>().material.color = value;
        }
    }
    public Vector3 tilePos
    {
        get
        {
            return gameObject.transform.position;
        }
        set
        {
            gameObject.transform.position = value;
        }
    }
    public Transform myParentTransform
    {
        get { return gameObject.transform.parent; }
        set
        {
            gameObject.transform.parent = value;
        }
    }
    public MeshRenderer meshRenderer
    {
        get { return gameObject.GetComponent<MeshRenderer>(); }
        set { meshRenderer = value; }
    }
}

public delegate void ActionRef<T>(ref T item);

public class SelfCollisionException : Exception { }
public class CollisionException : Exception { public Player other; }

static class Extensions
{
    public static R[,] Select<T, R>(this T[,] items, Func<T, R> f)
    {
        int d0 = items.GetLength(0);
        int d1 = items.GetLength(1);
        R[,] result = new R[d0, d1];
        for (int i0 = 0; i0 < d0; i0 += 1)
            for (int i1 = 0; i1 < d1; i1 += 1)
                result[i0, i1] = f(items[i0, i1]);
        return result;
    }
}

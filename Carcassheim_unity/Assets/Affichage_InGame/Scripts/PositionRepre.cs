using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PositionRepre
{
    public Vector3 pos;
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Rotation { get; private set; } = -1; // 0: N; 1 : E; 2 : W; 3 : S

    public PositionRepre(int x, int y, int rotation = -1)
    {
        X = x;
        Y = y;
        Rotation = rotation;
    }

    public PositionRepre()
    {
        X = 0;
        Y = 0;
        Rotation = -1;
    }

    public override string ToString()
    {
        return "(" + X.ToString() + ", " + Y.ToString() + ") rot : " + Rotation.ToString();
    }

    public static bool EqualWithoutRotation(PositionRepre pos1, PositionRepre pos2)
    {
        if (pos1 == null && pos2 == null)
            return true;
        else if ((pos1 == null && pos2 != null) || (pos1 != null && pos2 == null))
            return false;
        else
            return (pos1.X == pos2.X && pos1.Y == pos2.Y) ? true : false;

    }
}

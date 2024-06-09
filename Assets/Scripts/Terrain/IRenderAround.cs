using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRenderAround
{
    public Vector2 getCenterPosition();
    public int getRenderDistanceChunks();
}

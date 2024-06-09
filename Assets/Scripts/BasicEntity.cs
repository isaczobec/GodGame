using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEntity : MonoBehaviour, IRenderAround
{
    [SerializeField] private int renderDistance = 2;

    public Vector2 getCenterPosition()
    {
        return new Vector2(transform.position.x, transform.position.z);
    }

    public float getRenderDistance()
    {
        return renderDistance;
    }

    public int getRenderDistanceChunks()
    {
        return renderDistance;
    }
}

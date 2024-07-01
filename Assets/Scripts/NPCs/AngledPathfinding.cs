using System;
using System.Collections.Generic;
using UnityEngine;

public class AngledPathfinding {

    public List<ChunkTile> NPCGetPathAngledTo(NPC npc, Vector2Int destinationCoordinates, float angleIncrement, int radius, int maxIterationsPerRadius) {

        ChunkTile currentTile = npc.chunkTile;
        List<ChunkTile> fullPath = new List<ChunkTile>();

        int max = 10000;
        int i = 0;
        while (Vector2Int.Distance(currentTile.coordinates, destinationCoordinates) > radius*1.3) {
            int turningDirection = 1;
            float currentAngle = 0;
            Vector2Int nextPos = GetNextPosAngled(destinationCoordinates, currentTile.coordinates, radius, currentAngle);
            List<ChunkTile> path = null;
            while (path == null) {
                path = NPCPathfinding.instance.NPCGetPathTo(currentTile, npc, nextPos, maxIterationsPerRadius);
                currentAngle += angleIncrement;
                nextPos = GetNextPosAngled(destinationCoordinates, currentTile.coordinates, radius, currentAngle);

                if (currentAngle >= 180) {
                    if (turningDirection == -1) return fullPath; // cant find a path
                    turningDirection *= -1;
                    currentAngle = 0;
                }

            }

            fullPath.AddRange(path);
            currentTile = path[path.Count - 1];

            i++;
            if (i > max) {
                Debug.Log("Max iterations reached.");
                return fullPath;
            }
        }

        List<ChunkTile> lastPath = NPCPathfinding.instance.NPCGetPathTo(currentTile, npc, destinationCoordinates, maxIterationsPerRadius);
        if (lastPath != null) {
            fullPath.AddRange(lastPath);
        }

        return fullPath;


    }

    public Vector2Int GetNextPosAngled(Vector2Int destination, Vector2Int current, int radius, float angle) {

        angle = angle * Mathf.Deg2Rad;

        Vector2 direction = destination - current;
        direction.Normalize();
        direction *= radius;

        float newX = current.x + direction.x * Mathf.Cos(angle) - direction.y * Mathf.Sin(angle);
        float newY = current.y + direction.x * Mathf.Sin(angle) + direction.y * Mathf.Cos(angle);

        return new Vector2Int(Mathf.RoundToInt(newX), Mathf.RoundToInt(newY));

    
    }

}
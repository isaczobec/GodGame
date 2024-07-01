using System;
using System.Collections.Generic;
using UnityEngine;

public class AngledPathfinding {

    public List<ChunkTile> NPCGetPathAngledTo(NPC npc, Vector2Int destinationCoordinates, float angleIncrement, int radius, int maxIterationsPerRadius, float angleMultiplier = 0.8f, float radiusMultiplier = 0.3f, float maxIterationsMultiplier = 0.8f) {

        ChunkTile currentTile = npc.chunkTile;
        List<ChunkTile> fullPath = new List<ChunkTile>();

        int max = 10000;
        int i = 0;

        int rotationDirection = 1;

        while (Vector2Int.Distance(currentTile.coordinates, destinationCoordinates) > radius*1.3) {
            // int turningDirection = 1;
            float currentAngle = 0;
            Vector2Int nextPos = GetNextPosAngled(destinationCoordinates, currentTile.coordinates, radius, currentAngle);
            List<ChunkTile> path = null;

            int usedRadius = radius;
            float usedAngleIncrement = angleIncrement;
            int usedMaxIterationsPerRadius = maxIterationsPerRadius;

            while (path == null) {
                path = NPCPathfinding.instance.NPCGetPathTo(currentTile, npc, nextPos, usedMaxIterationsPerRadius);
                currentAngle += usedAngleIncrement;
                nextPos = GetNextPosAngled(destinationCoordinates, currentTile.coordinates, usedRadius, currentAngle * rotationDirection);

                if (currentAngle >= 360) {
                    if (rotationDirection == -1) {
                        Debug.Log("No complete path found after " + i + " iterations and changing direction.");
                        return fullPath;
                    }
                    rotationDirection *= -1;
                    fullPath = new List<ChunkTile>(); // reset the path
                    path = null;
                    usedRadius = radius;
                    usedAngleIncrement = angleIncrement;
                    usedMaxIterationsPerRadius = maxIterationsPerRadius;
                    currentTile = npc.chunkTile;
                } else {
                    usedRadius = Mathf.RoundToInt(radius * radiusMultiplier);
                    usedAngleIncrement = angleIncrement * angleMultiplier;
                    usedMaxIterationsPerRadius = Mathf.RoundToInt(maxIterationsPerRadius * maxIterationsMultiplier);
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
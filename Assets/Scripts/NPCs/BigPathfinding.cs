using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class BigPathNode {

    public List<ChunkTile> pathFromPrevious;
    public bool walkable;
    /// <summary>
    /// cost from start to this node
    /// </summary>
    public float gCost;
    /// <summary>
    /// cost from this node to the end
    /// </summary>
    public float hCost;
    public float fCost {
        get {
            return gCost + hCost;
        }
    }

    public ChunkTile tile;

    /// <summary>
    /// Whether this node has been visited by the pathfinding algorithm.
    /// </summary>
    public bool visited = false;


    /// <summary>
    /// The previous node in the path. Should only be null for the start node.
    /// </summary>
    public BigPathNode previousNode = null;

    private Dictionary<Vector2Int, BigPathNode> neighbours;

    public BigPathNode(ChunkTile tile, BigPathNode createdFromNode = null) {
        this.tile = tile;

        // initialize neighbours
        neighbours = new Dictionary<Vector2Int, BigPathNode>() {
            {Vector2Int.up, null},
            {Vector2Int.down, null},
            {Vector2Int.left, null},
            {Vector2Int.right, null},
            {new Vector2Int(1, 1), null},
            {new Vector2Int(1, -1), null},
            {new Vector2Int(-1, 1), null},
            {new Vector2Int(-1, -1), null}
        };

        if (createdFromNode != null) {
            neighbours[createdFromNode.tile.coordinates-tile.coordinates] = createdFromNode;
        }
    }

    /// <summary>
    /// Sets the neighbour of this node in the given direction.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public BigPathNode GetAdjacentNode(Vector2Int direction) {
        return neighbours[direction];
    }

    public BigPathNode[] GetNeighbours(bool allowVisitedNeighbours = false, bool allowUnwalkableNeighbours = false, bool allowNull = false) {
        
        List<BigPathNode> neighbourList = new List<BigPathNode>();
        if (!allowVisitedNeighbours || !allowUnwalkableNeighbours) {
        foreach (Vector2Int direction in neighbours.Keys) {

            bool add = true;
            if (neighbours[direction] != null) {
                if (neighbours[direction].visited) {
                    add =false;
                } 
                if (!neighbours[direction].walkable) {
                    add = false;
                }
                if (neighbours[direction] == null) {
                    add = false;
                }
                if (add) neighbourList.Add(neighbours[direction]);
            }

        }

        return neighbourList.ToArray();
        } else {
            return neighbours.Values.ToArray();
        }
    }

    public Dictionary<Vector2Int, BigPathNode> GetNeighboursDict() {
        return neighbours;
    }


}


public class BigPathFinding {
    public List<ChunkTile> GetNPCPathFindingBig(ChunkTile startTile, NPC npc, Vector2Int destinationCoordinates, int maxIterations = 50, int bigNodeIncrement = 20, int maxBigIterations = 1000) {

        BigPathNode startNode = new BigPathNode(startTile);

        List<BigPathNode> unvisitedNodes = new List<BigPathNode>();
        List<BigPathNode> visitedNodes = new List<BigPathNode>();

        int iterations = 0;
        BigPathNode currentNode = startNode;
        while (true) {

            CalculateAdjacentNodes(destinationCoordinates, currentNode, npc, maxIterations, bigNodeIncrement);
            BigPathNode[] neighbours = currentNode.GetNeighbours(allowVisitedNeighbours: false, allowUnwalkableNeighbours: false, allowNull: false);

            unvisitedNodes.AddRange(neighbours);

            if (unvisitedNodes.Count == 0) {
                
                Debug.Log("No path found after " + iterations + " iterations.");

                return null;
            }

            BigPathNode lowestFCostNode = unvisitedNodes[0];
            for (int i = 1; i < unvisitedNodes.Count; i++) {
                if (unvisitedNodes[i].fCost < lowestFCostNode.fCost || (unvisitedNodes[i].fCost == lowestFCostNode.fCost && unvisitedNodes[i].hCost < lowestFCostNode.hCost)) {
                    lowestFCostNode = unvisitedNodes[i]; // find the lowest fCost&hCost node
                }
            }

            unvisitedNodes.Remove(lowestFCostNode);

            currentNode.visited = true;
            visitedNodes.Add(currentNode);

            currentNode = lowestFCostNode;

            if (Vector2.Distance(currentNode.tile.coordinates, destinationCoordinates) <= bigNodeIncrement*1.3f) {
                Debug.Log("found after "+iterations+" iterations.");


                List<ChunkTile> path = new List<ChunkTile>();
                BigPathNode node = currentNode;
                while (node.previousNode != null) {
                    node.pathFromPrevious.Reverse();
                    path.AddRange(node.pathFromPrevious);
                    node = node.previousNode;
                }
                path.Reverse();

                List<ChunkTile> lastPath = NPCPathfinding.instance.NPCGetPathTo(currentNode.tile, npc, destinationCoordinates, maxIterations);
                if (lastPath != null) {
                    path.AddRange(lastPath);
                }

                return path;
            }

            iterations++;
            if (iterations > maxBigIterations) {
                Debug.Log("Max iterations reached");
                return null;
            }
        }
        
        

    }   

    /// <summary>
    /// Calculates the g,h, and f costs of the adjacent nodes of the given node, And checks if npc can walk on them.
    /// </summary>
    /// <param name="destinationCoordinates"></param>
    /// <param name="node"></param>
    /// <param name="npc"></param>
    public void CalculateAdjacentNodes(Vector2Int destinationCoordinates, BigPathNode node, NPC npc, int maxIterations = 50, int bigNodeIncrement = 20) {

        Dictionary<Vector2Int, BigPathNode> neighbours = node.GetNeighboursDict();
        for (int i = 0; i < neighbours.Count; i++) {

                bool didExist = false;
                BigPathNode neighbourNode;
                Vector2Int centerCoords = node.tile.coordinates + neighbours.Keys.ElementAt(i) * bigNodeIncrement;
                int x = Mathf.Clamp(destinationCoordinates.x, centerCoords.x -bigNodeIncrement/2, centerCoords.x + bigNodeIncrement/2);
                int y = Mathf.Clamp(destinationCoordinates.y, centerCoords.y -bigNodeIncrement/2, centerCoords.y + bigNodeIncrement/2);
                ChunkTile neighbourTile = WorldDataGenerator.instance.GetChunkTileFromCoordinates(new Vector2Int(x, y));
                List<ChunkTile> pathFromPrevious;
                pathFromPrevious = neighbourTile != null? NPCPathfinding.instance.NPCGetPathTo(node.tile, npc, neighbourTile.coordinates, maxIterations): null;
                if (neighbours.Values.ElementAt(i) == null) { // if the neighbour node is null, ie hasnt been visited yet
                    // need to create the node

                    // should find some way to try multiple tiles close to this one
                    
                    if (pathFromPrevious == null) { // no path was found
                        neighbourNode = new BigPathNode(node.tile, node);
                        neighbourNode.walkable = false;
                    } else {
                        neighbourNode = new BigPathNode(pathFromPrevious[pathFromPrevious.Count-1], node);
                        neighbourNode.walkable = true;
                        neighbourNode.pathFromPrevious = pathFromPrevious;
                    }

                    neighbours[neighbours.Keys.ElementAt(i)] = neighbourNode;

                } else {
                    neighbourNode = neighbours.Values.ElementAt(i);
                    didExist = true;
                }

                if (!neighbourNode.walkable) continue; // if the neighbour is not walkable, skip it

                // calculate the gCost, hCost and fCost of the neighbour node
                float newGCost = node.gCost + Vector2Int.Distance(node.tile.coordinates, neighbourNode.tile.coordinates);
                float newHCost = Vector2Int.Distance(neighbourNode.tile.coordinates, destinationCoordinates);

                if (didExist) {
                    if (newGCost < neighbourNode.gCost && pathFromPrevious != null) { // only update the node if the new gCost is lower than the old one. The H cost should and will never change.
                        neighbourNode.gCost = newGCost;
                        neighbourNode.hCost = newHCost;
                        neighbourNode.previousNode = node; // only update the previous node if the new gCost is lower than the old one, ie this path was better
                        neighbourNode.pathFromPrevious = pathFromPrevious;
                    }
                    Vector2Int direction = neighbours.Keys.ElementAt(i);
                    neighbourNode.GetNeighboursDict()[-direction] = node; // update the neighbour node of the current node
                } else {
                    neighbourNode.gCost = newGCost;
                    neighbourNode.hCost = newHCost;
                    neighbourNode.previousNode = node;
                }

            }
        }
}

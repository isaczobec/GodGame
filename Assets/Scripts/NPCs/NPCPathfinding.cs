
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

class NPCPathfinding {


    public static NPCPathfinding instance;    
    public static BigPathFinding bigPathFinding;
    public static AngledPathfinding angledPathfinding;


    /// <summary>
    /// Returns a list of chunkTiles that the NPC should move to in order to reach the destination. Returns null if there is no path to the destination.
    /// </summary>
    public List<ChunkTile> NPCGetPathTo(ChunkTile startTile, NPC npc, Vector2Int destinationCoordinates, int maxIterations = 1000, bool npcsAreObstacles = false) {

        PathFindingNode startNode = new PathFindingNode(startTile);

        // check if the destination is walkable
        ChunkTile endTile = WorldDataGenerator.instance.GetChunkTileFromCoordinates(destinationCoordinates);
        if (endTile.GetMaxSteepness() > npc.baseStats.maxWalkableSteepness || (endTile.terrainObject != null && endTile.terrainObject.blocksMovement) || endTile == null) {
            return null;
        }

        List<PathFindingNode> queue = new List<PathFindingNode>() {startNode};

        List<PathFindingNode> visitedNodes = new List<PathFindingNode>();

        List<PathFindingNode> unVisitedNodes = new List<PathFindingNode>() {startNode};

        PathFindingNode currentNode = startNode;

        int iterations = 0;
        while (true) {

            if (!currentNode.visited) CalculateAdjacentNodes(destinationCoordinates, currentNode, npc, npcsAreObstacles: npcsAreObstacles);

            PathFindingNode[] neighbourNodes = currentNode.GetNeighbours(); // doesnt return visited, null or unwalkable nodes
            unVisitedNodes.AddRange(neighbourNodes);
            
            // find the minimum fCost of the neighbours
            float lowestFCost = Mathf.Infinity;
            foreach (PathFindingNode neighbour in neighbourNodes) {
                if (neighbour == null) { // this should never happen due to the calculation of the adjacent nodes
                    continue;
                }
                if (!neighbour.walkable) continue; // if the neighbour is not walkable, skip it
                if (neighbour.visited) continue; // if the neighbour has already been visited, skip it

                if (neighbour.fCost < lowestFCost) {
                    lowestFCost = neighbour.fCost;
                }
            }

            // get the neighbours with the lowest fCost 
            List<PathFindingNode> lowestFCostNodes = new List<PathFindingNode>();
            foreach (PathFindingNode neighbour in neighbourNodes) {
                if (neighbour == null) { // this should never happen due to the calculation of the adjacent nodes
                    continue;
                }
                if (!neighbour.walkable) continue; // if the neighbour is not walkable, skip it
                if (neighbour.visited) continue; // if the neighbour has already been visited, skip it

                if (neighbour.fCost <= lowestFCost) {
                    lowestFCostNodes.Add(neighbour);
                }
            }

            if (lowestFCostNodes.Count == 1) { // if there was only one node with the lowest fCost we can just add it to the queue
                queue.Insert(0,lowestFCostNodes[0]);
            } else { // find the ones with the lowest hCost and add them to the queue
                float lowestHCost = Mathf.Infinity;
                foreach (PathFindingNode node in lowestFCostNodes) {
                    if (node.hCost < lowestHCost) {
                        lowestHCost = node.hCost;
                    }
                }

                foreach (PathFindingNode node in lowestFCostNodes) {
                    if (node.hCost <= lowestHCost) {
                        queue.Insert(0,node);
                    }
                } 

            }


            queue.Remove(currentNode); // remove the current node from the queue
            unVisitedNodes.Remove(currentNode); // remove the current node from the unvisited nodes

            PathFindingNode nextNode = null;
            if (queue.Count == 0) {

                // find the node with the lowest fCost in the unvisited nodes
                float lowestFCostInUnvisited = Mathf.Infinity;
                foreach (PathFindingNode node in visitedNodes) {
                    if (node.fCost < lowestFCostInUnvisited) {
                        lowestFCostInUnvisited = node.fCost;
                    }
                }

                // get the node with the lowest fCost in the unvisited nodes
                foreach (PathFindingNode node in visitedNodes) {
                    if (node.fCost == lowestFCostInUnvisited) {
                        nextNode = node;
                        break; // use the first one we find
                    }
                }

            } else {

                // find the node with the lowest fCost in the queue
                float lowestFCostInQueue = Mathf.Infinity;
                foreach (PathFindingNode node in queue) {
                    if (node.fCost < lowestFCostInQueue) {
                        lowestFCostInQueue = node.fCost;
                    }
                }

                // get the node with the lowest fCost in the queue
                foreach (PathFindingNode node in queue) {
                    if (node.fCost == lowestFCostInQueue) {
                        nextNode = node;
                        break; // use the first one we find
                    }
                }
            }



            currentNode.visited = true;
            visitedNodes.Add(currentNode);
            currentNode = nextNode; // move to the next node
            
            // there is no path to the destination; the queue is empty
            if (currentNode == null) {
                Debug.Log("No path to destination");
                return null;
            }


            // check if we have reached the destination
            if (currentNode.tile.coordinates == destinationCoordinates) {
                break;
            }

            iterations++;
            if (iterations > maxIterations) {
                Debug.Log("Pathfinding took too long");
                return null;
            }

        }

        int co = 0;
        // reconstruct the path
        List<PathFindingNode> path = new List<PathFindingNode>();
        PathFindingNode n = currentNode; // start at the destination
        while (n != null) { // the only node with null previousNode is the start node
            path.Add(n);

            // if (n.previousNode != null && Vector2Int.Distance(n.tile.coordinates, n.previousNode.tile.coordinates) > 1.6f) {
            //     Debug.Log("firstNodeWalkable: " + n.walkable + " secondNodeWalkable: " + n.previousNode.walkable);
            //     Debug.Log(co);
            // }

            n = n.previousNode; // move to the previous node, reconstructing the path

            co++;
        }

        // get a list of the chunkTiles in the path
        List<ChunkTile> chunkTiles = new List<ChunkTile>();
        for (int i = 1; i < path.Count; i++) { // one because we dont need to return the tile the NPC is already on
            chunkTiles.Add(path[path.Count - i - 1].tile);
        }

        return chunkTiles;

    }

    /// <summary>
    /// Calculates the g,h, and f costs of the adjacent nodes of the given node, And checks if npc can walk on them.
    /// </summary>
    /// <param name="destinationCoordinates"></param>
    /// <param name="node"></param>
    /// <param name="npc"></param>
    public void CalculateAdjacentNodes(Vector2Int destinationCoordinates, PathFindingNode node, NPC npc, bool npcsAreObstacles = false) {

        Dictionary<Vector2Int, PathFindingNode> neighbours = node.GetNeighboursDict();
        for (int i = 0; i < neighbours.Count; i++) {

                bool didExist = false;
                PathFindingNode neighbourNode;
                if (neighbours.Values.ElementAt(i) == null) { // if the neighbour node is null, ie hasnt been visited yet
                    // need to create the node


                    Vector2Int direction = neighbours.Keys.ElementAt(i);
                    ChunkTile neighbourTile = node.tile.GetChunkTileFromRelativePosition(direction);

                    bool walkable = true;
                    if (neighbourTile.terrainObject != null) {
                        walkable = !neighbourTile.terrainObject.blocksMovement;
                    }


                    neighbourNode = new PathFindingNode(neighbourTile,node);
                    neighbours[direction] = neighbourNode;
                    neighbourNode.walkable = walkable;

                } else {
                    neighbourNode = neighbours.Values.ElementAt(i);
                    didExist = true;
                }

                // check if the neighbour is too steep to walk on
                if (node.CheckIfToSteepToWalkOn(neighbourNode,npc)) {
                    neighbourNode.walkable = false;
                }

                
                if (npcsAreObstacles) { // if npcs are obstacles, check if the neighbour tile has an npc on it
                    if (neighbourNode.tile.npc != null) { 
                        if (neighbourNode.tile.npc != npc) {
                            neighbourNode.walkable = false;
                        }
                    }
                }

                if (!neighbourNode.walkable) continue; // if the neighbour is not walkable, skip it

                // calculate the gCost, hCost and fCost of the neighbour node
                float newGCost = node.gCost + Vector2Int.Distance(node.tile.coordinates, neighbourNode.tile.coordinates);
                float newHCost = Vector2Int.Distance(neighbourNode.tile.coordinates, destinationCoordinates);

                if (didExist) {
                    if (newGCost < neighbourNode.gCost) { // only update the node if the new gCost is lower than the old one. The H cost should and will never change.
                        neighbourNode.gCost = newGCost;
                        neighbourNode.hCost = newHCost;
                        neighbourNode.fCost = newGCost + newHCost;
                        neighbourNode.previousNode = node; // only update the previous node if the new gCost is lower than the old one, ie this path was better
                    }
                    Vector2Int direction = neighbours.Keys.ElementAt(i);
                    neighbourNode.GetNeighboursDict()[-direction] = node; // update the neighbour node of the current node
                } else {
                    neighbourNode.gCost = newGCost;
                    neighbourNode.hCost = newHCost;
                    neighbourNode.fCost = newGCost + newHCost;
                    neighbourNode.previousNode = node;
                }

            }
        }
    }





class PathFindingNode {


    public ChunkTile tile;

    public bool walkable;
    /// <summary>
    /// cost from start to this node
    /// </summary>
    public float gCost;
    /// <summary>
    /// cost from this node to the end
    /// </summary>
    public float hCost;
    public float fCost;

    /// <summary>
    /// Whether this node has been visited by the pathfinding algorithm.
    /// </summary>
    public bool visited = false;


    /// <summary>
    /// The previous node in the path. Should only be null for the start node.
    /// </summary>
    public PathFindingNode previousNode = null;

    private Dictionary<Vector2Int, PathFindingNode> neighbours;

    public PathFindingNode(ChunkTile tile, PathFindingNode createdFromNode = null) {
        this.tile = tile;

        // initialize neighbours
        neighbours = new Dictionary<Vector2Int, PathFindingNode>() {
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
    public PathFindingNode GetAdjacentNode(Vector2Int direction) {
        return neighbours[direction];
    }

    public PathFindingNode[] GetNeighbours(bool allowVisitedNeighbours = false, bool allowUnwalkableNeighbours = false, bool allowNull = false) {
        
        List<PathFindingNode> neighbourList = new List<PathFindingNode>();
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

    public Dictionary<Vector2Int, PathFindingNode> GetNeighboursDict() {
        return neighbours;
    }

    /// <summary>
    /// Checks if the difference in height between this node and the other node is too steep for the NPC to walk on.
    /// Returns true if the difference in height is too steep ie not walkable, false otherwise.
    /// </summary>
    /// <param name="other"></param>
    /// <param name="npc"></param>
    /// <returns></returns>
    public bool CheckIfToSteepToWalkOn(PathFindingNode other, NPC npc) {
        float slope = Mathf.Abs((tile.height - other.tile.height) / Vector2Int.Distance(tile.coordinates, other.tile.coordinates) / WorldDataGenerator.instance.tileSize);
        if (slope > npc.baseStats.maxWalkableSteepness) {
            return true;
        }
        return false;

    }

}
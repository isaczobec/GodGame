using UnityEngine;

class ChunkTree {

    public ChunkTreeNode root;
    public int levels; // the number of levels in the tree. If it contains 8 chunks, it has 3 levels

    public ChunkTree(Vector2Int topLeftPosition, Vector2Int bottomRightPosition, int amountChunks) {

        // set levels to be log2 of amountChunks
        int c = 2;
        int l = 1;
        while (l < amountChunks) {
            c *= 2;
            l += 1;
        }
        levels = l;

        root = new ChunkTreeNode
        {
            topLeftPosition = topLeftPosition,
            bottomRightPosition = bottomRightPosition,
            level = levels
        };
    }
    

    public Chunk CreateOrGetChunk(Vector2Int chunkPosition, bool allowCreation = true) {

        int l = levels;
        ChunkTreeNode node = root;
        while (l > 1) {

            ChunkTreeNode newNode = node.GetNodeFromPosition(chunkPosition);
            if (newNode == null) {
                if (!allowCreation) {
                    return null;
                }
                // create a child at the apropriate position if it doesn't exist
                node = node.CreateChild(chunkPosition.x < (node.topLeftPosition.x + node.bottomRightPosition.x) / 2, chunkPosition.y < (node.topLeftPosition.y + node.bottomRightPosition.y) / 2);
            }
            else {
                node = newNode; // the node exists, so we can move to it
            }
            
            l -= 1; // go down a level

        }

        // create the chunk if it doesn't exist
        if (node.chunk == null) {
            if (!allowCreation) return null;
            node.chunk = new Chunk(chunkPosition);
        }
        return node.chunk;

    }
}


class ChunkTreeNode {

    // child nodes
    public ChunkTreeNode topLeft;
    public ChunkTreeNode topRight;
    public ChunkTreeNode bottomLeft;
    public ChunkTreeNode bottomRight;

    public Vector2Int topLeftPosition;
    public Vector2Int bottomRightPosition;

    public Chunk chunk;

    public int level;


    /// <summary>
    /// Returns the node that contains the given position. 
    /// </summary>
    public ChunkTreeNode GetNodeFromPosition(Vector2Int position) {
        if (position.x < (topLeftPosition.x + bottomRightPosition.x) / 2) {
            if (position.y < (topLeftPosition.y + bottomRightPosition.y) / 2) {
                return topLeft;
            } else {
                return bottomLeft;
            }
        } else {
            if (position.y < (topLeftPosition.y + bottomRightPosition.y) / 2) {
                return topRight;
            } else {
                return bottomRight;
            }
        }
    }

    public ChunkTreeNode CreateChild(bool left, bool top) {
        Vector2Int newTopLeft = topLeftPosition;
        Vector2Int newBottomRight = bottomRightPosition;

        if (left) {
            newBottomRight.x = (topLeftPosition.x + bottomRightPosition.x) / 2;
        } else {
            newTopLeft.x = (topLeftPosition.x + bottomRightPosition.x) / 2;
        }

        if (top) {
            newBottomRight.y = (topLeftPosition.y + bottomRightPosition.y) / 2;
        } else {
            newTopLeft.y = (topLeftPosition.y + bottomRightPosition.y) / 2;
        }

        if (left && top) {
            topLeft = new ChunkTreeNode
            {
                topLeftPosition = newTopLeft,
                bottomRightPosition = newBottomRight,
                level = level - 1
            };
            return topLeft;
        } else if (left && !top) {
            bottomLeft = new ChunkTreeNode
            {
                topLeftPosition = newTopLeft,
                bottomRightPosition = newBottomRight,
                level = level - 1
            };
            return bottomLeft;
        } else if (!left && top) {
            topRight = new ChunkTreeNode
            {
                topLeftPosition = newTopLeft,
                bottomRightPosition = newBottomRight,
                level = level - 1
            };
            return topRight;
        } else {
            bottomRight = new ChunkTreeNode
            {
                topLeftPosition = newTopLeft,
                bottomRightPosition = newBottomRight,
                level = level - 1
            };
            return bottomRight;
        }
    }

}


using UnityEngine;

class WorldDataGenerator: MonoBehaviour {


    [Header("World Generation Settings")]
    [SerializeField] public int maxChunkSize;
    [SerializeField] public int initialWorldSize;
    [SerializeField] public int fullWorldSizeChunks;
    [SerializeField] public int LODlevels;
    [SerializeField] public float quadSize;


    private ChunkTree chunkTree;


    public static WorldDataGenerator instance {get; private set;}

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("There should only be one instance of WorldDataGenerator");
        }

        CheckValuesValid();

    }

    private void CheckValuesValid()
    {
        // check so values are valid
        if ((maxChunkSize - 1) % Mathf.Pow(2, LODlevels) != 0)
        {
            Debug.LogError("maxChunkSize-1 must be a multiple of 2^LODlevels");
        }

        int p = 2;
        bool pow2 = false;
        while (p <= fullWorldSizeChunks)
        {
            p *= 2;
            if (p == fullWorldSizeChunks)
            {
                pow2 = true;
                break;
            }
        }
        if (!pow2)
        {
            Debug.LogError("fullWorldSizeChunks must be a power of 2");
        }
    }

    public void Start() {
        chunkTree = new ChunkTree(new Vector2Int(0,0), new Vector2Int(fullWorldSizeChunks, fullWorldSizeChunks), fullWorldSizeChunks);
    }

    


}
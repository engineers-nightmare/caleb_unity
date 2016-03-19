using UnityEngine;

public class ChunkData : MonoBehaviour
{

    public byte[,,] Contents = new byte[Constants.ChunkSize, Constants.ChunkSize, Constants.ChunkSize];
    public int generation;

    ChunkData()
    {
        // some initial data
        Contents[0, 0, 0] = 1;
        Contents[0, 0, 1] = 1;
        Contents[0, 1, 1] = 1;
        generation = 0;
    }

    // Use this for initialization
    void Start()
    {
        
    }
}

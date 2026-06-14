using UnityEngine;
using UnityEngine.Tilemaps;

public class SceneSetup : MonoBehaviour
{
    public Tilemap groundTilemap;
    public TileBase groundTile;
    public GameObject trainPrefab;
    
    private void Start()
    {
        SetupGroundTilemap();
        SpawnTrain();
    }
    
    private void SetupGroundTilemap()
    {
        if (groundTilemap != null && groundTile != null)
        {
            // 设置一个简单的地面区域
            for (int x = -10; x < 10; x++)
            {
                for (int y = -5; y < 5; y++)
                {
                    groundTilemap.SetTile(new Vector3Int(x, y, 0), groundTile);
                }
            }
        }
    }
    
    private void SpawnTrain()
    {
        if (trainPrefab != null)
        {
            // 在场景中生成列车对象
            GameObject train = Instantiate(trainPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            train.name = "Train";
        }
    }
}
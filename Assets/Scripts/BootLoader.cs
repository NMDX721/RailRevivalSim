using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoader : MonoBehaviour
{
    private void Start()
    {
        // 加载 StationSlice_V1 场景
        SceneManager.LoadScene("StationSlice_V1");
    }
}
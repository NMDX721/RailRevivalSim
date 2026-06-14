using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(Renderer))]
public class CloudSeaTrainBackground : MonoBehaviour
{
    [Header("Video")]
    public VideoClip videoClip;
    public bool loop = true;

    private VideoPlayer _videoPlayer;
    private Renderer _renderer;
    private RenderTexture _rt;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();

        if (videoClip == null)
        {
            videoClip = Resources.Load<VideoClip>("Videos/cloud_sea_bg");
        }

        if (videoClip == null)
        {
            Debug.LogError("[CloudSea] 视频未找到");
            return;
        }

        _rt = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32);
        _rt.filterMode = FilterMode.Point;
        _rt.Create();

        var mat = new Material(Shader.Find("Unlit/Texture"));
        mat.mainTexture = _rt;
        _renderer.material = mat;

        _videoPlayer = gameObject.AddComponent<VideoPlayer>();
        _videoPlayer.clip = videoClip;
        _videoPlayer.playOnAwake = true;
        _videoPlayer.isLooping = loop;
        _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        _videoPlayer.targetTexture = _rt;
        _videoPlayer.aspectRatio = VideoAspectRatio.Stretch;
        _videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        _videoPlayer.skipOnDrop = false;
        _videoPlayer.source = VideoSource.VideoClip;

        Debug.Log($"[CloudSea] 视频初始化: {videoClip.name} {videoClip.width}x{videoClip.height}");
    }

    void OnDestroy()
    {
        if (_rt != null)
        {
            _rt.Release();
        }
    }
}

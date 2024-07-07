using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

public class VideoStreamer : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    [SerializeField] private string videoId = "1xeN832aL55qqRyxnFu4UdkZTFcDDKiU_";
    private string videoUrl;

    private void Awake()
    {
        videoUrl = $"https://drive.google.com/uc?export=download&id={videoId}";

        videoPlayer = gameObject.GetComponent<VideoPlayer>();
        if (videoPlayer)
        {
            videoPlayer.url = videoUrl;
            videoPlayer.playOnAwake = false;
            videoPlayer.Prepare();

            videoPlayer.prepareCompleted += OnVideoPrepared;
        }
    }

    private void OnVideoPrepared(VideoPlayer source)
    {
        videoPlayer.Play();
    }
}

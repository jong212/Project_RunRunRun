using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

public class VideoStreamer : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public NetworkManager networkManager;
    public GameObject[] players;
    private string videoUrl;

    private void Awake()
    {
        videoPlayer = gameObject.GetComponent<VideoPlayer>();
        if (videoPlayer)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.Prepare();

            videoPlayer.prepareCompleted += OnVideoPrepared;
        }
    }

    private void OnEnable()
    {
        StartCoroutine(CheckPlayerAndSetUrl());
    }

    private void OnDisable()
    {
        foreach (var player in players)
        {
            player.SetActive(false);
        }
    }

    public void OnClickRoomUIOn()
    {
        networkManager.RoomPanel.SetActive(true);
        networkManager._playerView.gameObject.SetActive(true);
        transform.gameObject.SetActive(false);
    }

    private void OnVideoPrepared(VideoPlayer source)
    {
        videoPlayer.Play();
    }

    private IEnumerator CheckPlayerAndSetUrl()
    {
        // Wait until the URL is set in the GoogleDriveUploader
        yield return new WaitUntil(() => GoogleDriveUploader.Instance.Url != null);

        foreach (var player in players)
        {
            player.SetActive(false);
            if (networkManager._playerView.name == player.name + "(Clone)")
            {
                player.SetActive(true);
                videoUrl = GoogleDriveUploader.Instance.Url;
                videoPlayer.url = videoUrl;
                videoPlayer.Prepare();
            }
        }
    }
}

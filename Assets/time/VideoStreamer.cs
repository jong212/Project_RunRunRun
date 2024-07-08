using Cinemachine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

public class VideoStreamer : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public NetworkManager networkManager;
    public GameObject[] players;
    public CinemachineVirtualCamera virtualCamera;
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
        foreach (Player player in networkManager.RoomPlayerList)
        {
            if (player.CustomProperties.ContainsKey("objectViewID"))
            {
                int viewID = (int)player.CustomProperties["objectViewID"];
                PhotonView view = PhotonView.Find(viewID);
                if (view != null)
                {
                    view.gameObject.SetActive(true); // Corrected this line
                }
            }
        }
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
            if (networkManager._firstObjectName == player.name + "(Clone)")
            {
                virtualCamera.Follow = player.transform;
                virtualCamera.LookAt = player.transform;
                player.SetActive(true);
                videoUrl = GoogleDriveUploader.Instance.Url;
                videoPlayer.url = videoUrl;
                videoPlayer.Prepare();
            }
        }
    }
}

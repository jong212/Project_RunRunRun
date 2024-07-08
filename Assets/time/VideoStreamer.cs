using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;
using Cinemachine;
using Photon.Pun;
using Photon.Realtime;
public class VideoStreamer : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public NetworkManager networkManager;
    public GameObject[] players;

    public Transform playerparents;
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
        // �̰Ŷ� �����ʸ��ν����� ���� �ּ�
        //yield return new WaitForSeconds(3);

        yield return new WaitUntil(() => GoogleDriveUploader.Instance.Url != null);
        
        foreach (var player in players)
        {
            player.SetActive(false);
            if (networkManager._firstObjectName == player.name + "(Clone)")
            {
                Transform childTransform = playerparents.Find(player.name);
                if (childTransform != null)
                {
                    virtualCamera.Follow = childTransform;
                    virtualCamera.LookAt = childTransform;
                }
                else
                {
                    // �ڽ� ������Ʈ�� ã�� ������ ���� ó��
                    Debug.Log("Child not found");
                }
              
                player.SetActive(true);
                videoUrl = GoogleDriveUploader.Instance.Url;
                videoPlayer.url = videoUrl;
                videoPlayer.Prepare();
            }
        }
    }
}

using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class TestSceneManager : MonoBehaviourPunCallbacks
{
    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Connecting to Photon...");
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.Log("Already connected to Photon.");
        }
    }

    void Update()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (PhotonNetwork.CurrentRoom != null)
            {
                Debug.Log("현재 방 인원수 : " + PhotonNetwork.CurrentRoom.PlayerCount);
            }
            else
            {
                Debug.Log("현재 방 없음");
            }
        }
        else
        {
            Debug.Log("마스터 서버에 연결되지 않음: " + PhotonNetwork.NetworkClientState);
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server.");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError($"Disconnected from Photon with reason: {cause}");
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby.");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Failed to join room: {message}");
    }

    public void GoToLobby()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            SceneManager.LoadScene(1); // Load the lobby scene at index 1
        }
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(1); // Load the lobby scene at index 1
    }
}

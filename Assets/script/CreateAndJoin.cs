using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class CreateAndJoin : MonoBehaviourPunCallbacks
{
    public TMP_InputField input_Create;
    public TMP_InputField input_Join;

    private bool isConnecting = false;

    private void Awake()
    {
        Debug.Log("Test+ " + PhotonNetwork.IsConnected);
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
            {
                Debug.Log("Already connected, joining lobby.");
                PhotonNetwork.JoinLobby();
            } else
            {
                Debug.Log("연결 안 됨 !!!!!!!!!!");
            }
        }
        else
        {
            Debug.Log("Not connected, connecting to Photon.");
            isConnecting = true;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(input_Create.text, new RoomOptions { MaxPlayers = 2 });
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(input_Join.text);
    }

    public void JoinRoomInList(string RoomName)
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinRoom(RoomName);
            }
            else
            {
                PhotonNetwork.JoinLobby();
            }
        }
        else
        {
            Debug.LogError("Cannot join room: Not connected to Master Server");
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server");
        if (isConnecting)
        {
            isConnecting = false;
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
        // 로비에 참여했을 때 추가 작업을 여기에 작성
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("GamePlay");
        Debug.Log("방에 참가한 플레이어 인원 수 " + PhotonNetwork.CountOfPlayersInRooms);
    }

    [ContextMenu("정보")]
    void Info()
    {
        if (PhotonNetwork.InRoom)
        {
            Debug.Log("현재 방 이름 : " + PhotonNetwork.CurrentRoom.Name);
            Debug.Log("현재 방 인원수 : " + PhotonNetwork.CurrentRoom.PlayerCount);
            Debug.Log("현재 방 최대인원수 : " + PhotonNetwork.CurrentRoom.MaxPlayers);

            string playerStr = "방에 있는 플레이어 목록 : ";
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                playerStr += PhotonNetwork.PlayerList[i].NickName + ", ";
            Debug.Log(playerStr);
        }
        else
        {
            Debug.Log("접속한 인원 수 : " + PhotonNetwork.CountOfPlayers);
            Debug.Log("방 개수 : " + PhotonNetwork.CountOfRooms);
            Debug.Log("모든 방에 있는 인원 수 : " + PhotonNetwork.CountOfPlayersInRooms);
            Debug.Log("로비에 있는지? : " + PhotonNetwork.InLobby);
            Debug.Log("연결됐는지? : " + PhotonNetwork.IsConnected);
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError("Disconnected from Photon Server: " + cause.ToString());
        // 연결이 끊겼을 때 추가 작업을 여기에 작성
    }
}

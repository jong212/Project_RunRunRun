using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Realtime;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    public static ConnectToServer instance;
    [SerializeField] public TMP_InputField input_Create;
    [SerializeField] public TMP_InputField input_Join;  
    public GameObject RoomPrefab; // 방을 나타내는 프리팹 
    List<RoomInfo> myList = new List<RoomInfo>(); // 현재 유지하는 방 목록 리스트s
    private void Awake()
    {
        Screen.SetResolution(960, 540, false);

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            ConnectToPhoton();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // myList에 있는 방 이름 리스트 생성
        List<string> currentRoomNames = new List<string>();
        foreach (RoomInfo room in myList)
        {
            currentRoomNames.Add(room.Name);
        }

        int roomCount = roomList.Count;
        for (int i = 0; i < roomCount; i++)
        {
            if (!roomList[i].RemovedFromList)
            {
                // 방이 제거되지 않은 경우, 추가 또는 업데이트 처리
                if (!myList.Contains(roomList[i]))
                {
                    // 새로운 방이므로 myList에 추가하고 RoomPrefab을 인스턴스화하여 UI에 표시
                    myList.Add(roomList[i]);
                    GameObject Room = Instantiate(RoomPrefab, Vector3.zero, Quaternion.identity, GameObject.Find("Content").transform);
                    Room.transform.localPosition = Vector3.zero;
                    Room.GetComponent<Room>().Name.text = roomList[i].Name; // Room 스크립트가 RoomPrefab에 부착되어 있다고 가정
                    StartCoroutine(LogPositionNextFrame(Room)); // 다음 프레임에서 위치를 로깅하기 위해 코루틴 시작
                }
                else
                {
                    // myList에 이미 있는 방이므로 업데이트 처리
                    myList[myList.IndexOf(roomList[i])] = roomList[i];
                }
            }
            else
            {
                // 방이 목록에서 제거된 경우, myList에서도 제거
                if (myList.Contains(roomList[i]))
                {
                    myList.Remove(roomList[i]);

                    // 해당 GameObject를 찾아서 파괴
                    Transform contentTransform = GameObject.Find("Content").transform;
                    foreach (Transform child in contentTransform)
                    {
                        Room roomComponent = child.GetComponent<Room>();
                        if (roomComponent != null && roomComponent.Name.text == roomList[i].Name)
                        {
                            Destroy(child.gameObject);
                            break;
                        }
                    }
                }
            }
        }
    }
    public void CreateRoom()
    {
        Debug.Log(input_Create.text);
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
            PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
        SceneManager.LoadScene("Lobby");
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
    private IEnumerator LogPositionNextFrame(GameObject room)
    {
        yield return null; // 다음 프레임까지 대기
        // 여기서는 주로 방의 위치를 출력하거나 다른 처리를 추가할 수 있습니다.
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError("Disconnected from Photon Server: " + cause.ToString());
        // 연결이 끊겼을 때 추가 작업을 여기에 작성
    }

    private void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }
}

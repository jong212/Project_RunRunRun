/*
 
 - OnLeftRoom : 본인 클라에서만 호출
 - 커스텀속성 변경시 : 로컬 + 다른 클라창들  모두 동기화 됨 + OnPlayerPropertiesUpdate 콜백함수 있음
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;


public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("DisconnectPanel")]
    public InputField NickNameInput;

    [Header("LoginPanel")]
    public GameObject LoginPanel;
    [Header("LobbyPanel")]
    public GameObject LobbyPanel;
    public InputField RoomInput;
    public Text WelcomeText;
    public Text LobbyInfoText;
    public Button[] CellBtn;
    public Button PreviousBtn;
    public Button NextBtn;

    [Header("RoomPanel")]
    public GameObject RoomPanel;
    public Text ListText;
    public Text RoomInfoText;
    public Text[] ChatText;
    public InputField ChatInput;

    [Header("ETC")]
    public Text StatusText;
    public PhotonView PV;

    List<RoomInfo> myList = new List<RoomInfo>();
    int currentPage = 1, maxPage, multiple;
    private string currentPrafab; // 사용자가 착용중인 캐릭터 프리팹 Name
    private string currentPrafab_chk; // 다른 캐릭터로 변경했는지 체크하기 위한 변수
    private string PlayerNickName; // 사용자 로그인 아이디 
    public GameObject localPlayerPrefab;
    public GameObject _loginCamera;



    private Dictionary<int, bool> playerReadyState = new Dictionary<int, bool>(); // 플레이어 ID와 준비 상태를 저장하는 딕셔너리
    public Button ReadyButton; // 준비 버튼 추가
    void Start()
    {
        ReadyButton.onClick.AddListener(OnReadyButtonClicked);
    }

    //기능 추가 

    // A 버튼을 클릭하면
    void OnReadyButtonClicked()
    {
        // A-1 값 있음 True 로 가져오고 아님 False
        // 로컬플레이어에 해당하는 isready "키" 가 "CustomProperties"사전에 있는지 체크하고 있으면 그게 참인지 거짓 값인지를 가져와서 isready에 넣는다
        bool isReady = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("isReady") && (bool)PhotonNetwork.LocalPlayer.CustomProperties["isReady"];

        // A-2 버튼 클릭했으니 가져온 값 반대로 세팅
        // 세팅할때 Setcustomproperties 함수에 세팅 하는 이유가 해당 함수를 통해 Customproperties 값이 바뀌면 > OnPlayerPropertiesUpdate 콜핵함수를 호출시키고 모든 클라에게 공유되기 때문 (서버 전용 전역변수 느낌,그렇다고 서버 변수는 아님)
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "isReady", !isReady } });
    }

        // A-3 A-2 과정이 끝나면 자동으로 콜백 함수가 불러짐 
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey("isReady"))
        {
            UpdatePlayerReadyStates();
        }
    }
    #region 방리스트 갱신
    // ◀버튼 -2 , ▶버튼 -1 , 셀 숫자
    public void MyListClick(int num)
    {
        if (num == -2) --currentPage;
        else if (num == -1) ++currentPage;
        else PhotonNetwork.JoinRoom(myList[multiple + num].Name);
        MyListRenewal();
    }

    void MyListRenewal()
    {
        // 최대페이지
        maxPage = (myList.Count % CellBtn.Length == 0) ? myList.Count / CellBtn.Length : myList.Count / CellBtn.Length + 1;

        // 이전, 다음버튼
        PreviousBtn.interactable = (currentPage <= 1) ? false : true;
        NextBtn.interactable = (currentPage >= maxPage) ? false : true;

        // 페이지에 맞는 리스트 대입
        multiple = (currentPage - 1) * CellBtn.Length;
        for (int i = 0; i < CellBtn.Length; i++)
        {
            CellBtn[i].interactable = (multiple + i < myList.Count) ? true : false;
            CellBtn[i].transform.GetChild(0).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].Name : "";
            CellBtn[i].transform.GetChild(1).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].PlayerCount + "/" + myList[multiple + i].MaxPlayers : "";
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        int roomCount = roomList.Count;
        for (int i = 0; i < roomCount; i++)
        {
            if (!roomList[i].RemovedFromList)
            {
                if (!myList.Contains(roomList[i])) myList.Add(roomList[i]);
                else myList[myList.IndexOf(roomList[i])] = roomList[i];
            }
            else if (myList.IndexOf(roomList[i]) != -1) myList.RemoveAt(myList.IndexOf(roomList[i]));
        }
        MyListRenewal();
    }
    #endregion


    #region 서버연결
    void Awake() => Screen.SetResolution(960, 540, false);

    void Update()
    {
        StatusText.text = PhotonNetwork.NetworkClientState.ToString();
        LobbyInfoText.text = (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + "로비 / " + PhotonNetwork.CountOfPlayers + "접속";

            
    }
    public void Connect(string characterId, string playerNickName)
    {
        Debug.Log("ddddd" + characterId);
        this.currentPrafab = characterId;
        this.currentPrafab_chk = characterId;
        this.PlayerNickName = playerNickName;
        _loginCamera.SetActive(false);

        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();

    // 로비 입장 시 
    // 로컬 클라 에서만 호출 도는 메서드
    public override void OnJoinedLobby()
    {
        LoginPanel.SetActive(false);
        LobbyPanel.SetActive(true);
        PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
        WelcomeText.text = PhotonNetwork.LocalPlayer.NickName + "님 환영합니다";
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "characterId", currentPrafab } });
        CreateLocalPlayer();
        myList.Clear();
    }

    void CreateLocalPlayer()
    {
        if (localPlayerPrefab == null || currentPrafab != currentPrafab_chk)
        {
            string prefabName = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("characterId") ? PhotonNetwork.LocalPlayer.CustomProperties["characterId"].ToString() : "DefaultPlayerPrefab";
            Debug.Log("test11111" + prefabName);
            localPlayerPrefab = Instantiate(Resources.Load<GameObject>(prefabName), new Vector3(-139, 5, 0), Quaternion.identity);
            currentPrafab_chk = currentPrafab;
        }
    }
    public void Disconnect() => PhotonNetwork.Disconnect();
     
    // 로비에서 X 버튼 누를 때 콜백 되는 함수 
    public override void OnDisconnected(DisconnectCause cause)
    {
        if (localPlayerPrefab != null) { Destroy(localPlayerPrefab); }
        
        LoginPanel.SetActive(true);

        LobbyPanel.SetActive(false);
        _loginCamera.SetActive(true);
        RoomPanel.SetActive(false);
    }
    #endregion


    #region 방
    public void CreateRoom() => PhotonNetwork.CreateRoom(RoomInput.text == "" ? "Room" + Random.Range(0, 100) : RoomInput.text, new RoomOptions { MaxPlayers = 4 });

    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();

    public void LeaveRoom() => PhotonNetwork.LeaveRoom();

    // 방 입장 시
    // 로컬 클라 에서만 호출 도는 메서드
    public override void OnJoinedRoom()
    {
        if (localPlayerPrefab != null) { Destroy(localPlayerPrefab); }
        LobbyPanel.SetActive(false);
        RoomPanel.SetActive(true);

        RoomRenewal();
        ChatInput.text = "";
        for (int i = 0; i < ChatText.Length; i++) ChatText[i].text = "";
        Vector3 spawnPosition = new Vector3(0, 2, 0);  
        PhotonNetwork.Instantiate("Player", spawnPosition, Quaternion.identity);

        UpdatePlayerReadyStates(); 
    }
  
    public override void OnCreateRoomFailed(short returnCode, string message) { RoomInput.text = ""; CreateRoom(); } 

    public override void OnJoinRandomFailed(short returnCode, string message) { RoomInput.text = ""; CreateRoom(); }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RoomRenewal();
        ChatRPC("<color=yellow>" + newPlayer.NickName + "님이 참가하셨습니다</color>");

        UpdatePlayerReadyStates();  
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {

        RoomRenewal();
        ChatRPC("<color=yellow>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");
     

        UpdatePlayerReadyStates();  
    }

    void RoomRenewal()
    {
        ListText.text = "";
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            ListText.text += PhotonNetwork.PlayerList[i].NickName + ((i + 1 == PhotonNetwork.PlayerList.Length) ? "" : ", ");
        RoomInfoText.text = PhotonNetwork.CurrentRoom.Name + " / " + PhotonNetwork.CurrentRoom.PlayerCount + "명 / " + PhotonNetwork.CurrentRoom.MaxPlayers + "최대";
    }

    private bool allPlayersReady = false;
    private Coroutine countdownCoroutine;

    void UpdatePlayerReadyStates()
    {
        bool allReady = true;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.ContainsKey("isReady"))
            {
                bool isReady = (bool)player.CustomProperties["isReady"];
                Debug.Log(player.NickName + " is " + (isReady ? "ready" : "not ready"));
                if (!isReady)
                {
                    allReady = false;
                }
            }
            else
            {
                Debug.Log(player.NickName + " has not set ready state.");
                allReady = false;
            }
        }

        if (allReady && !allPlayersReady)
        {
            allPlayersReady = true;
            PV.RPC("StartCountdown", RpcTarget.All);
        }
        else if (!allReady && allPlayersReady)
        {
            allPlayersReady = false;
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
                PV.RPC("StopCountdown", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    void StartCountdown()
    {
        if (countdownCoroutine == null)
        {
            countdownCoroutine = StartCoroutine(Countdown());
        }
    }

    [PunRPC]
    void StopCountdown()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }
    }

    IEnumerator Countdown()
    {
        int count = 5;
        while (count > 0)
        {
            Debug.Log("Countdown: " + count);
            // 여기서 UI 텍스트를 업데이트하는 코드를 추가할 수 있습니다.
            yield return new WaitForSeconds(1);
            count--;
        }

        if (allPlayersReady)
        {
            Debug.Log("All players are ready. Starting the game...");
            // 게임을 시작하는 로직을 여기에 추가합니다.
        }
    }
    // 방 나갈 시
    // 본인 클라에서만 호출
    #endregion
    public override void OnLeftRoom()
    {
        
        if (localPlayerPrefab != null) { Destroy(localPlayerPrefab); }
        RoomPanel.SetActive(false);
        LobbyPanel.SetActive(true);
        // Set the local player's isReady state to false when they leave the room
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "isReady", false } });
    }
  
    #region 채팅
    public void Send()
    {
        PV.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + ChatInput.text);
        ChatInput.text = "";
    }

    [PunRPC] // RPC는 플레이어가 속해있는 방 모든 인원에게 전달한다
    void ChatRPC(string msg)
    {
        bool isInput = false;
        for (int i = 0; i < ChatText.Length; i++)
            if (ChatText[i].text == "")
            {
                isInput = true;
                ChatText[i].text = msg;
                break;
            }
        if (!isInput) // 꽉차면 한칸씩 위로 올림
        {
            for (int i = 1; i < ChatText.Length; i++) ChatText[i - 1].text = ChatText[i].text;
            ChatText[ChatText.Length - 1].text = msg;
        }
    }
   
    #endregion
}

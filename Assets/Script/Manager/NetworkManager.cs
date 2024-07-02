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
using System.Linq;
using System;


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
    public GameObject LobbyWaitObjec1; // 대기방1 오브젝트
    public GameObject LobbyWaitObjec2; // 대기방2 오브젝트
    public GameObject GameWaitObjec1;  // 게임방1 오브젝트 
    public GameObject RoomListParentsPopup;
    [Header("RoomPanel")]
    public GameObject RoomPanel;
    //public Text ListText;
    public Text RoomInfoText;
    public Text[] ChatText;
    public InputField ChatInput;

    [Header("ETC")]
    public Text StatusText;
    public PhotonView PV;

    List<RoomInfo> myList = new List<RoomInfo>();
    int currentPage = 1, maxPage, multiple;
    private string currentPrafab;                       // 사용자가 착용중인 캐릭터 프리팹 Name
    public List<string> OwnedCharacters { get; set; }   // 구매한 프리팹들
    public int currentMoney { get; set; }               // 돈
    //private string currentPrafab_chk;                   // 다른 캐릭터로 변경했는지 체크하기 위한 변수
    private string PlayerNickName;                      // 사용자 로그인 아이디 
    public GameObject localPlayerPrefab;
    public GameObject _loginCamera;


    private Dictionary<int, bool> playerReadyState = new Dictionary<int, bool>(); // 플레이어 ID와 준비 상태를 저장하는 딕셔너리
    public Button ReadyButton; // 준비 버튼 추가
    void Start()
    {
        ReadyButton.onClick.AddListener(OnReadyButtonClicked);
    }

    //기능 추가 
    //UpdatePlayerCharacterId
    // A 버튼을 클릭하면
    void OnReadyButtonClicked()
    {
        // A-1 
        /*값 있음 True 로 가져오고 아님 False 
        결과적으로, isReady 변수는 로컬 플레이어가 "준비됨" 상태인지(isReady 키가 존재하고 그 값이 true인지)를 나타냅니다.
        CustomProperties에 "isReady" 키가 있고 그 값이 true라면, isReady는 true가 됩니다.
        CustomProperties에 "isReady" 키가 없거나, 그 값이 false라면, isReady는 false가 됩니다.*/
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
    #region UI
    // #2 오브젝트 비활성화면 활성화 시키고 활성화면 비활성화 시키는 코드인데 분석좀 해봐야함
    public void LobbyUIOnOff()
    {
        LobbyPanel.SetActive(!LobbyPanel.activeSelf);
    }
    #endregion

    #region 서버연결
    void Awake() {
        Screen.SetResolution(960, 540, false);
        OwnedCharacters = new List<string>();
    }


    void Update()
    {
        StatusText.text = PhotonNetwork.NetworkClientState.ToString();
        LobbyInfoText.text = (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + "로비 / " + PhotonNetwork.CountOfPlayers + "접속";

       // Debug.Log(currentMoney);
    }
    public void Connect(string characterId, string playerNickName,int currentMoney)
    {
        this.currentPrafab = characterId;       // 선택 되어있는 캐릭터 이름
        //this.currentPrafab_chk = characterId;
        this.PlayerNickName = playerNickName;   // ID
        this.currentMoney = currentMoney;       // DB Select 해서 가져온 플레이어 보유금액
        _loginCamera.SetActive(false);

        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();

    // 로비 입장 시 
    // 로컬 클라 에서만 호출 도는 메서드
    public override void OnJoinedLobby()
    {
        LobbyWaitObjec1.SetActive(true);
        LobbyWaitObjec2.SetActive(true);
        GameWaitObjec1.SetActive(false);
        UIManager.Instance.LobbyUIControll("on");
        LobbyDataManager.Inst.ReadAllDataOnAwake();                                 // 로비 XML 파일 초기화
        LoginPanel.SetActive(false);                                                // 로그인 패널 OFF
        //LobbyPanel.SetActive(true);                                                 // 로비 패널 ON
        PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
        WelcomeText.text = PhotonNetwork.LocalPlayer.NickName + "님 환영합니다";
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "characterId", currentPrafab } });// 현재 착용중인 캐릭터의 프리팹 이름
        CreateLocalPlayer();
        myList.Clear();
    }
     
    public void ChangeChar(string changedName)
    {
        currentPrafab = changedName;
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "characterId", currentPrafab } });// 현재 착용중인 캐릭터의 프리팹 이름
        Destroy(localPlayerPrefab);
        localPlayerPrefab = null; // Set to null explicitly

        CreateLocalPlayer();
    }
    void CreateLocalPlayer()
    {
        /*if (localPlayerPrefab == null || currentPrafab != currentPrafab_chk)*/
        if (localPlayerPrefab == null)
        {
            string prefabName = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("characterId") ? PhotonNetwork.LocalPlayer.CustomProperties["characterId"].ToString() : "DefaultPlayerPrefab";
            localPlayerPrefab = Instantiate(Resources.Load<GameObject>(prefabName), new Vector3(-154, 12.49f, -49.54883f), Quaternion.Euler(0,25.525f,0));

            InitializeCamera();

        }
    }
    void InitializeCamera()
    {
        // 프리룩 카메라를 찾습니다 (경로를 정확히 명시)
        Transform freeLookCameraTransform = localPlayerPrefab.transform.Find("LobbyCamera/FreeLook Camera"); // 정확한 경로로 수정

        if (freeLookCameraTransform != null)
        {
            GameObject freeLookCamera = freeLookCameraTransform.gameObject;
            freeLookCamera.SetActive(true);

            // 카메라를 캐릭터의 자식으로 설정하여 캐릭터를 따라가게 합니다.
            freeLookCamera.transform.SetParent(localPlayerPrefab.transform);

            // 카메라의 위치와 회전을 초기화합니다.
            freeLookCamera.transform.localPosition = new Vector3(0, 2, -3); // 캐릭터의 뒤쪽 약간 위쪽에 위치하도록 설정 (필요에 따라 조정)
            freeLookCamera.transform.localRotation = Quaternion.Euler(10, 0, 0); // 약간 아래쪽을 바라보도록 설정 (필요에 따라 조정)
        }
        else
        {
            Debug.LogWarning("FreeLookCamera not found! Please check the path.");
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
        UIManager.Instance.LobbyUIControll("off");


    }
    #endregion


    #region 방
    public void CreateRoom() => PhotonNetwork.CreateRoom(RoomInput.text == "" ? "Room" + UnityEngine.Random.Range(0, 100) : RoomInput.text, new RoomOptions { MaxPlayers = 4 });

    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();

    public void LeaveRoom() => PhotonNetwork.LeaveRoom();

    // 방 입장 시
    // 로컬 클라 에서만 호출 도는 메서드
    public override void OnJoinedRoom()
    {
        LobbyWaitObjec1.SetActive(false);
        LobbyWaitObjec2.SetActive(false);
        GameWaitObjec1.SetActive(true);

        if (localPlayerPrefab != null) { Destroy(localPlayerPrefab); }
        LobbyPanel.SetActive(false);
        UIManager.Instance.LobbyUIControll("off");
        RoomPanel.SetActive(true);

        RoomRenewal();
        ChatInput.text = "";
        for (int i = 0; i < ChatText.Length; i++) ChatText[i].text = "";
        Vector3 spawnPosition = new Vector3(-157, 46, -55);

        PhotonNetwork.Instantiate(currentPrafab, spawnPosition, Quaternion.identity);

        UpdatePlayerReadyStates(); 
    }
  
    public override void OnCreateRoomFailed(short returnCode, string message) { RoomInput.text = ""; CreateRoom(); } 

    public override void OnJoinRandomFailed(short returnCode, string message) { RoomInput.text = ""; CreateRoom(); }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("test");
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
        //ListText.text = "";
        //for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        //ListText.text += PhotonNetwork.PlayerList[i].NickName + ((i + 1 == PhotonNetwork.PlayerList.Length) ? "" : ", ");
        RoomInfoText.text = PhotonNetwork.CurrentRoom.Name + " (" + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers +")";
    }

    private bool allPlayersReady = false;
    private Coroutine countdownCoroutine;

    void UpdatePlayerReadyStates() // 방에 들어오거나 레디를 했거나 방을 떠났거나
    {
        bool allReady = true;
      

        // Assuming you have a list or array of the existing player divs
        GameObject[] playerDivs = RoomListParentsPopup.GetComponentsInChildren<Transform>(true)
                                                    .Where(t => t.name.Contains("Graphics")).Select(t => t.gameObject).ToArray();
        foreach (GameObject listItem in playerDivs)
        {
            listItem.transform.Find("NickName").GetComponent<Text>().text = "";
            listItem.transform.Find("Ready").gameObject.SetActive(false);
        }
        int index = 0;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            bool isReady = player.CustomProperties.ContainsKey("isReady") && (bool)player.CustomProperties["isReady"];

            // Debug output
            Debug.Log(player.NickName + " is " + (isReady ? "ready" : "not ready"));

            if (index < playerDivs.Length)
            {
                GameObject listItem = playerDivs[index];
                listItem.transform.Find("NickName").GetComponent<Text>().text = player.NickName;
                listItem.transform.Find("Ready").gameObject.SetActive(isReady);
            }

            // Check if all players are ready
            if (!isReady)
            {
                allReady = false;
            }

            index++;
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
        //UIManager.Instance.LobbyUIControll("off");
        if (localPlayerPrefab != null) { Destroy(localPlayerPrefab); }
        RoomPanel.SetActive(false);
        //LobbyPanel.SetActive(true);
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

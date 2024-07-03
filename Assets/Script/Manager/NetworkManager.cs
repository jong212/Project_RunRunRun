﻿/*
 
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
using TMPro;
using DG.Tweening;

public class NetworkManager : MonoBehaviourPunCallbacks
{

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
    public GameObject LobbyWaitObjec1;                          // 로비 지형지물 A
    public GameObject LobbyWaitObjec2;                          // 로비 지형지물 B
    public GameObject GameWaitObjec1;                           // 게임룸 지형지물 A
    public GameObject lbDissconnectBtn;                         // 로비 닫기 버튼
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
    private string currentPrafab;                                                          // 사용자가 착용중인 캐릭터 프리팹 Name
    public List<string> OwnedCharacters { get; set; }                                      // 구매한 프리팹들
    public int currentMoney { get; set; }                                                  // 돈
    public string PlayerNickName { get;set; }                                              // 사용자 로그인 아이디 
    public GameObject localPlayerPrefab;
    public GameObject _loginCamera;

    private Dictionary<int, bool> playerReadyState = new Dictionary<int, bool>();          // 플레이어 ID와 준비 상태를 저장하는 딕셔너리
    public Button ReadyButton; // 준비 버튼 추가

    [Header("GameMap")]
    public List<Transform> MapPointsList = new List<Transform>();                          // B 체크포인트 리스트
    private List<Player> RoomPlayerList = new List<Player>();                              // B 현재 방의 플레이어 리스트
    private Dictionary<string, int> PlayerLastChkPoint = new Dictionary<string, int>();    // B 각 플레이어의 체크포인트 상태
    private Dictionary<string, float> NextPointDistance = new Dictionary<string, float>(); // B 각 플레이어의 다음 체크포인트까지의 거리

    private Dictionary<string, Transform> currentPlayerTransformDic = new Dictionary<string, Transform>(); // 키 닉네임으로 사용
    private bool IsGamestartCheck { get; set; }

    [Header("CountDown")]
    public GameObject countPanel;
    public TMP_Text countDownText;
    public CanvasGroup countPanel_CanvasGroup;

    // B-1 게임 시작 시 체크포인트 및 거리 정보에 대한 플레이어 정보 초기화
    void InitGameStartPlayers() // 이 함수 초기화 단게에서 마스터클라이언트를 안 걸어야 방장이 중간에 나가도 마스터클라 양도받은 컴에서 Update 이어서 칠 수 있음.
    {
      
        Debug.Log("[TEST2 : 모든 클라에서 호출되는지 테스트]"); // 모든 클라에서 호출 됨 cowntdown 코루틴 자체가 Rpc로 호출을 시켜버림
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "isReady", false } });
        }

        RoomPlayerList = PhotonNetwork.PlayerList.ToList();
        PlayerLastChkPoint.Clear();
        NextPointDistance.Clear();
        currentPlayerTransformDic.Clear();

        foreach (Player player in RoomPlayerList)
        {
            PlayerLastChkPoint[player.NickName] = 0;
            NextPointDistance[player.NickName] = float.MaxValue;

            if (player.CustomProperties.ContainsKey("objectViewID"))
            {
                int viewID = (int)player.CustomProperties["objectViewID"];
                PhotonView view = PhotonView.Find(viewID);
                if (view != null)
                {
                    currentPlayerTransformDic[player.NickName] = view.transform;
                }
            }
        }

        IsGamestartCheck = true;
    }

    void Start()
    {
        ReadyButton.onClick.AddListener(OnReadyButtonClicked);

    }

    // A 게임방에서 레디 버튼 클릭할 때 아래 함수 호출
    void OnReadyButtonClicked()
    {
        // A-1  순서는 값이 있는지 없는지 체크하고 있으면 isReady에 true로 저장되며 없으면 false로 저장된다 그리고 나서 true였다면 false로 세팅하고 false였다면 true로 강제 세팅을 해줌               
        // 결과적으로, isReady 변수는 로컬 플레이어가 "준비됨" 상태인지(isReady 키가 존재하고 그 값이 true인지)를 나타냅니다.
        // CustomProperties에 "isReady" 키가 있고 그 값이 true라면, isReady는 true가 됩니다.
        // CustomProperties에 "isReady" 키가 없거나, 그 값이 false라면, isReady는 false가 됩니다.        
        bool isReady = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("isReady") && (bool)PhotonNetwork.LocalPlayer.CustomProperties["isReady"];

        // A-2 버튼 클릭했으니 가져온 값 반대로 세팅 (세팅할때 Setcustomproperties 함수에 세팅 하는 이유가 해당 함수를 통해 Customproperties 값이 바뀌면 > OnPlayerPropertiesUpdate 콜핵함수를 호출시키고 모든 클라에게 공유되기 때문 (서버 전용 전역변수 느낌,그렇다고 서버 변수는 아님))
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "isReady", !isReady } });
    }

    // A-3 SetCustomProperties 값 수정 시 모든 클라에서 OnPlayerPropertiesUpdate 함수 콜백으로 호출됨 예를들어 A클라에서 SetcustomProperties 값변경하면 A 포함해서 다른 모든 클라에서 아래 함수 호출 됨 
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey("isReady"))
        {
            // 어떤 클라에서든지 게임방에서 레디 버튼 누르면 아래 함수 호출
            UpdatePlayerReadyStates();
        }
        if (changedProps.ContainsKey("objectViewID"))
        {
            int viewID = (int)changedProps["objectViewID"];
            PhotonView view = PhotonView.Find(viewID);
            if (view != null)
            {
                currentPlayerTransformDic[targetPlayer.NickName] = view.transform;
            }
            else
            {
                currentPlayerTransformDic.Remove(targetPlayer.NickName);
            }
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

    // 모든 클라이언트에서 호출되는 콜백 함수는 아님. 정확히 말하면, 이 콜백 함수는 로비에 있는 클라이언트들에서 호출
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
    void Awake()
    {
        Screen.SetResolution(960, 540, false);
        OwnedCharacters = new List<string>();
    }

    void Update()
    {
        StatusText.text = PhotonNetwork.NetworkClientState.ToString();
        LobbyInfoText.text = (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + "로비 / " + PhotonNetwork.CountOfPlayers + "접속";

        //B2 게임 시작 후 IsGamestartCheck 의 불 값이 변경되면서 아래 함수는 실행된다 
        Debug.Log(IsGamestartCheck + "startCheck");
        if (IsGamestartCheck && PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Test1 : [마스터 클라이언트 양도 테스트]"); // 게임진행 중 방장 나가면 그 방에있는 아무나한테 마스터클라이언트 권한이 양도 되는지 테스트 해봤는데 양도 잘 됨
            foreach (Player player in RoomPlayerList)
            {
                UpdatePlayerDistance(player);
            }
            CalculateRankings();
            //DisplayRankings();
        }
    }

    void CalculateRankings()
    {
        var activePlayers = PhotonNetwork.PlayerList.ToList();

        var rankings = activePlayers
            .OrderByDescending(player => {
                int checkpoints = 0;
                if (PlayerLastChkPoint.TryGetValue(player.NickName, out checkpoints))
                {
                    return checkpoints;
                }
                return 0;
            })
            .ThenBy(player => {
                float distance = float.MaxValue;
                if (NextPointDistance.TryGetValue(player.NickName, out distance))
                {
                    return distance;
                }
                return float.MaxValue;
            })
            .ToList();

        for (int i = 0; i < rankings.Count; i++)
        {
            Debug.Log($"{i + 1}위: {rankings[i].NickName}");
        }
    }

    void UpdatePlayerDistance(Player player)
    {
        string playerName = player.NickName;
        
        //초기화: checkpointIndex 변수를 0으로 초기화합니다. 이는 필수는 아니지만 가독성을 높이고 명확하게 하기 위해 수행됩니다.
        int checkpointIndex = 0;

        //playerName 키가 딕셔너리에 존재하는지 확인하고, 존재하면 checkpointIndex에 해당 값(마지막 밟은 체크포인트 값)을 할당합니다.
        // 그리고 currentPlayerTransformDic 딕셔너리에 해당 플레이어의 트랜스폼이 존재하는지 확인합니다.

        if (PlayerLastChkPoint.TryGetValue(playerName, out checkpointIndex) && currentPlayerTransformDic.ContainsKey(playerName))
        {
            // 플레이어의 체크포인트 인덱스가 맵의 체크포인트 리스트 범위 내에 있는지 확인합니다.
            if (checkpointIndex < MapPointsList.Count)
            {
                Transform playerTransform = currentPlayerTransformDic[playerName];
                Transform checkpointTransform = MapPointsList[checkpointIndex];

                if (playerTransform != null && checkpointTransform != null)
                {
                    float distance = Vector3.Distance(playerTransform.position, checkpointTransform.position);
                    NextPointDistance[playerName] = distance;
                    if (distance < 5.0f) // 예: 5 유닛 이내로 접근하면 체크포인트 도달로 간주
                    {
                        PlayerLastChkPoint[playerName] = checkpointIndex + 1;
                        Debug.Log($"Player {playerName} +1 되어 {checkpointIndex}에서 {PlayerLastChkPoint[playerName]} 가 되었음");
                    }
                    //Debug.Log($"Player: {playerName}, Transform: {playerTransform.name}, Distance: {distance}");
                }
                else
                {
                    Debug.LogWarning($"Player {playerName}'s transform or checkpoint transform is null.");
                }
            }
        }
    }

    public void Connect(string characterId, string playerNickName, int currentMoney)
    {
        this.currentPrafab = characterId;       // 선택 되어있는 캐릭터 이름
        this.PlayerNickName = playerNickName;   // ID
        this.currentMoney = currentMoney;       // DB Select 해서 가져온 플레이어 보유금액
        _loginCamera.SetActive(false);

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();

    // 로비 입장 시 로컬 콜백으로 실행되는 함수
    
    public override void OnJoinedLobby()
    {

        lbDissconnectBtn.SetActive(true);                                           // 로비 닫기 버튼 On
        LobbyWaitObjec1.SetActive(true);                        
        LobbyWaitObjec2.SetActive(true);
        GameWaitObjec1.SetActive(false);
        UIManager.Instance.LobbyUIControll("on");
        LobbyDataManager.Inst.ReadAllDataOnAwake();                                 // 로비 XML 파일 초기화
        LoginPanel.SetActive(false);                                                // 로그인 패널 OFF
        PhotonNetwork.LocalPlayer.NickName = PlayerNickName;
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
            localPlayerPrefab = Instantiate(Resources.Load<GameObject>(prefabName), new Vector3(-154, 12.49f, -49.54883f), Quaternion.Euler(0, 25.525f, 0));

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
        lbDissconnectBtn.SetActive(false);
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
        lbDissconnectBtn.SetActive(false);
        if (localPlayerPrefab != null) { Destroy(localPlayerPrefab); }
        LobbyPanel.SetActive(false);
        UIManager.Instance.LobbyUIControll("off");
        RoomPanel.SetActive(true);

        RoomRenewal();
        ChatInput.text = "";
        for (int i = 0; i < ChatText.Length; i++) ChatText[i].text = "";
        Vector3 spawnPosition = new Vector3(-157, 46, -55);
        GameObject playerObject = PhotonNetwork.Instantiate(currentPrafab, spawnPosition, Quaternion.identity);
        int viewID = playerObject.GetComponent<PhotonView>().ViewID;
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "objectViewID", viewID } });

        UpdatePlayerReadyStates();
    }

    public override void OnCreateRoomFailed(short returnCode, string message) { RoomInput.text = ""; CreateRoom(); }

    public override void OnJoinRandomFailed(short returnCode, String message) { RoomInput.text = ""; CreateRoom(); }

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

        currentPlayerTransformDic.Remove(otherPlayer.NickName);
        NextPointDistance.Remove(otherPlayer.NickName);
        PlayerLastChkPoint.Remove(otherPlayer.NickName);
        RoomPlayerList.Remove(otherPlayer); // RoomPlayerList에서 플레이어를 제거합니다.

        UpdatePlayerReadyStates();
        PV.RPC("StopCountdown", RpcTarget.All);
    }

    void RoomRenewal()
    {
        RoomInfoText.text = PhotonNetwork.CurrentRoom.Name + " (" + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers + ")";
    }

    private bool allPlayersReady = false;
    private Coroutine countdownCoroutine;

    void UpdatePlayerReadyStates() // 방에 들어오거나 레디를 했거나 방을 떠났거나
    {
        Debug.Log("Updating player ready states...");
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

            // 한 명이라도 레디 안 하면 allReady를 false로 변경시켜서  아래 else if stopcoroutine을 실행시킴 
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
        countPanel.SetActive(true);
        while (count > 0)
        {                       
            countDownText.text = count.ToString();
            countPanel_CanvasGroup.DOFade(1f, 0.3f).SetUpdate(true).OnComplete(()=>
            {
                countPanel_CanvasGroup.DOFade(0f, .3f).SetUpdate(true).SetDelay(.3f);
            });
            countDownText.rectTransform.DOScale(Vector3.one, .3f).SetUpdate(true).OnComplete(() =>
            {
                countDownText.rectTransform.DOScale(Vector3.zero, .3f).SetUpdate(true).SetDelay(.3f);
            });
            // 여기서 UI 텍스트를 업데이트하는 코드를 추가할 수 있습니다.
            yield return new WaitForSeconds(1);
            count--;
        }
        Debug.Log("booltest" + allPlayersReady);
        if (allPlayersReady)
        {
            PV.RPC("ShowStartText", RpcTarget.All);
            InitGameStartPlayers(); // B-1 게임 시작 시 체크포인트 및 거리 정보에 대한 플레이어 정보 초기화 
            yield return new WaitForSeconds(1);
        }
        countPanel.SetActive(false);
    }
    [PunRPC]
    void ShowStartText()
    {
        countDownText.text = "Start !";
        countPanel_CanvasGroup.DOFade(1f, 0.3f).SetUpdate(true).OnComplete(() =>
        {
            countPanel_CanvasGroup.DOFade(0f, .3f).SetUpdate(true).SetDelay(.3f);
        });
        countDownText.rectTransform.DOScale(Vector3.one, .3f).SetUpdate(true).OnComplete(() =>
        {
            countDownText.rectTransform.DOScale(Vector3.zero, .3f).SetUpdate(true).SetDelay(.3f);
        });
        Debug.Log("All players are ready. Starting the game...");
    }
    // 방 나갈 시
    // 본인 클라에서만 호출
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

    [PunRPC] // 이거 Send로 호출되는거면 채팅 쓸 때 다른 클라한테 동기화 되는거고 방 떠날때는 이거 적용 안 됨 바로 ChatRPC일반함수를 호출하며 방에 남은 클라에서만 방 나갔다는 로그 뜸
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
#endregion
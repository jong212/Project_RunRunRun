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
using TMPro;
using DG.Tweening;
using System.IO;
using ithappy;
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
    public GameObject GameWaitObjec1;                           // 게임룸 지형지물 A
    public GameObject lbDissconnectBtn;                         // 로비 닫기 버튼
    public GameObject RoomListParentsPopup;
    [Header("RoomPanel")]
    public GameObject RoomPanel;
    //public Text ListText;
    public Text RoomInfoText;
    public Text[] ChatText;
    public InputField ChatInput;
    public GameObject obj;
    [Header("ETC")]
    public Text StatusText;
    public PhotonView PV;
    public PhotonView _playerView;
    public GameObject ObstracleParent;
    public GameObject _gameEndObject;
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
    public string _firstObjectName { get; set; }
    [Header("GameMap")]
    public List<Transform> MapPointsList = new List<Transform>();                          // B 체크포인트 리스트
    public List<Player> RoomPlayerList = new List<Player>();                              // B 현재 방의 플레이어 리스트
    private Dictionary<string, int> PlayerLastChkPoint = new Dictionary<string, int>();    // B 각 플레이어의 체크포인트 상태
    private Dictionary<string, float> NextPointDistance = new Dictionary<string, float>(); // B 각 플레이어의 다음 체크포인트까지의 거리

    private Dictionary<string, Transform> currentPlayerTransformDic = new Dictionary<string, Transform>(); // 키 닉네임으로 사용
    private bool IsGamestartCheck { get; set; }
    public GameObject RankUIParents;
    public GameObject DoorObj;
    [Header("CountDown")]
    public GameObject countPanel;
    public TMP_Text countDownText;
    public CanvasGroup countPanel_CanvasGroup;
    public bool GameStart = false;
    public GameObject ObstracleParentPink; // Reference to the parent GameObject containing obstacles

    public Text WinnerNickname;

    // B-1 게임 시작 시 체크포인트 및 거리 정보에 대한 플레이어 정보 초기화
    [PunRPC]
    void InitGameStartPlayers() // 이 함수 초기화 단게에서 마스터클라이언트를 안 걸어야 방장이 중간에 나가도 마스터클라 양도받은 컴에서 Update 이어서 칠 수 있음.
    {
        ObstracleParent.SetActive(false);
        ObstracleParent.SetActive(true);

       var  projectRoot = Application.dataPath.Replace("/Assets", "");
       var  imageOutputFolder = Path.Combine(projectRoot, "CapturedFrames");
       var  videoOutputFolder = Path.Combine(projectRoot, "Videos");
        ClearFolder(imageOutputFolder);
        ClearFolder(videoOutputFolder);

        RankUIParents.SetActive(true);
        StopCountdown();

        GameStart = true;
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
        GameStartInit();
        Door door = DoorObj.GetComponent<Door>();
        if(door != null)
        {
             Vector3 doorPosition = door.transform.position;
             doorPosition.y = 6.249999f;
             door.transform.localPosition = doorPosition;  // 이 부분이 누락됨            
            door.MoveDoorUp();
        }
        /*Door doorUp = DoorObj.GetComponent<Door>();
        if (doorUp != null)
        {
            if (doorUp != null)
            {
                doorUp.MoveDoorUp();
            }

        }*/


        IsGamestartCheck = true;
    }
    private void ClearFolder(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            var directories = Directory.GetDirectories(folderPath);
            foreach (var directory in directories)
            {
                Directory.Delete(directory, true);
            }

            var files = Directory.GetFiles(folderPath);
            foreach (var file in files)
            {
                File.Delete(file);
            }

            Debug.Log($"Cleared folder: {folderPath}");  // Add debug log for folder clearing
        }
    }
    void GameStartInit()
    {
        RoomPanel.SetActive(false);
        Invoke("HideCountPanel", 1f); // 1초 뒤에 HideCountPanel 메서드 호출

    }
    void HideCountPanel()
    {
        countPanel.SetActive(false);
    }
    void Start()
    {
        ReadyButton.onClick.AddListener(OnReadyButtonClicked);

        StartCoroutine(RankCheck());

    }
    IEnumerator RankCheck()
    {

        while (true)
        {

            yield return new WaitForSeconds(.1f);
            if (IsGamestartCheck && PhotonNetwork.IsMasterClient)
            {
               //[TEST : 양도 테스트] Debug.Log("Test1 : [마스터 클라이언트 양도 테스트]"); // 게임진행 중 방장 나가면 그 방에있는 아무나한테 마스터클라이언트 권한이 양도 되는지 테스트 해봤는데 양도 잘 됨
                foreach (Player player in RoomPlayerList)
                {
                    UpdatePlayerDistance(player);
                }
                CalculateRankings();
            }

        }
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
    // 특정 키로 호출한 것만 아래에서 containskey로 검사해서 해당 로직만 실행되는듯 위에선 isready를 키로 보내서 아래에선 isready에 대한 구문만 실행 된다고 함 좋은듯?
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
    private List<Player> arrivalOrder = new List<Player>();

    void Update()
    {
        StatusText.text = PhotonNetwork.NetworkClientState.ToString();
        LobbyInfoText.text = (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + "로비 / " + PhotonNetwork.CountOfPlayers + "접속";

        //B2 게임 시작 후 IsGamestartCheck 의 불 값이 변경되면서 아래 함수는 실행된다         

    }

    void UpdatePlayerDistance(Player player)
    {
        string playerName = player.NickName;
        int checkpointIndex = 0;

        // 도착한 플레이어는 거리 계산에서 제외
        if (arrivalOrder.Contains(player))
        {
            Debug.Log("return");
            return;
        }

        if (PlayerLastChkPoint.TryGetValue(playerName, out checkpointIndex) && currentPlayerTransformDic.ContainsKey(playerName))
        {
            // 플레이어의 체크포인트 인덱스가 맵의 체크포인트 리스트 범위 내에 있는지 확인합니다.
            if (checkpointIndex < MapPointsList.Count)
            {
                Transform playerTransform = currentPlayerTransformDic[playerName];
                Transform checkpointTransform = MapPointsList[checkpointIndex];

                if (playerTransform != null && checkpointTransform != null)
                {
                    // XZ 평면에서의 거리 계산
                    Vector3 playerPositionXZ = new Vector3(playerTransform.position.x, 0, playerTransform.position.z);
                    Vector3 checkpointPositionXZ = new Vector3(checkpointTransform.position.x, 0, checkpointTransform.position.z);

                    float distance = Vector3.Distance(playerPositionXZ, checkpointPositionXZ);
                    NextPointDistance[playerName] = distance;
                    Debug.Log(distance);
                    if (distance < 10f) // 예: 5 유닛 이내로 접근하면 체크포인트 도달로 간주
                    {
                        PlayerLastChkPoint[playerName] = checkpointIndex + 1;
                        Debug.Log($"Player {playerName} +1 되어 {checkpointIndex}에서 {PlayerLastChkPoint[playerName]} 가 되었음");

                        // 도착지점에 도달했는지 확인
                        if (PlayerLastChkPoint[playerName] >= MapPointsList.Count)
                        {
                            Debug.Log($"Player {playerName} has reached the final destination.");
                            HandlePlayerArrival(player);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"Player {playerName}'s transform or checkpoint transform is null.");
                }
            }
            else
            {
                // 플레이어가 모든 체크포인트를 통과하여 도착지점에 도달했음을 의미
                Debug.Log($"Player {playerName} has reached the final destination.");
                HandlePlayerArrival(player);
            }
        }
    }

    void HandlePlayerArrival(Player player)
    {
        string playerName = player.NickName;

        // 도착한 플레이어를 리스트에 추가
        if (!arrivalOrder.Contains(player))
        {
            if(arrivalOrder.Count == 0)
            {
                Player targetPlayer = PhotonNetwork.CurrentRoom.GetPlayer(player.ActorNumber);
                GameObject playerObject = GetPlayerObject(targetPlayer);

                CallFunctionOnSpecificClient(player);
                int viewID = playerObject.GetComponent<PhotonView>().ViewID;
                PV.RPC("FirstArrive", RpcTarget.All, viewID, player);

                PV.RPC("StartCountdown", RpcTarget.All,10);


            }
            arrivalOrder.Add(player);
            Debug.Log($"Player {playerName} has been added to the arrival order.");
        }

        // 게임 종료 조건 확인 (예: 모든 플레이어가 도착했는지)
        if (arrivalOrder.Count == RoomPlayerList.Count)
        {
            Debug.Log("All players have arrived. Calculating final rankings.");
        }
    }

    void CalculateRankings()
    {
        var activePlayers = RoomPlayerList;

        // 도착하지 않은 플레이어들의 순위 계산
        var nonArrivedRankings = activePlayers
            .Where(player => !arrivalOrder.Contains(player))
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

        // 최종 랭킹 계산 (도착한 플레이어 + 도착하지 않은 플레이어)
        var finalRankings = arrivalOrder.Concat(nonArrivedRankings).ToList();

        for (int i = 0; i < finalRankings.Count; i++)
        {
            //[TEST : 등수] Debug.Log($"{i + 1}위: {finalRankings[i].NickName}");
        }
        // 순위를 RPC로 전송
        List<string> rankingNames = finalRankings.Select(p => p.NickName).ToList();

        if (PV != null)
        {
            PV.RPC("UpdateRankings", RpcTarget.All, new object[] { rankingNames.ToArray() });
        }
        else
        {
            Debug.LogError("PhotonView is not initialized.");
        }
    }
    [PunRPC]
    void UpdateRankings(string[] rankingNames)
    {
        int tempNum = 0;
        foreach (Transform child in RankUIParents.transform)
        {
            GameObject childGameObject = child.gameObject;

            if (tempNum < rankingNames.Length)
            {
                if (childGameObject != null)
                {
                    Text rankName = childGameObject.GetComponentInChildren<Text>();
                    if (rankName != null)
                    {
                        if (PhotonNetwork.NickName == rankingNames[tempNum])
                        {
                            Image img = child.GetComponent<Image>();
                            if (img != null)
                            {
                                img.color = new Color(165f / 255f, 255f / 255f, 169f / 255f, 1f); // RGB(165, 255, 169), Alpha(1)
                            }

                        }
                        else
                        {
                            Image img = child.GetComponent<Image>();
                            if (img != null)
                            {
                                img.color = new Color(0f / 255f, 0f / 255f, 0f / 255f, 47f / 255f); // RGB(0, 0, 0), Alpha(47/255)
                            }
                        }
                        rankName.text = $"{rankingNames[tempNum]}";
                    }

                    // 두 번째 자식의 자식 텍스트 컴포넌트 접근
                    if (child.childCount > 1)
                    {
                        Transform secondChild = child.GetChild(1);
                        if (secondChild.childCount > 0)
                        {
                            Text secondRankName = secondChild.GetComponentInChildren<Text>();
                            if (secondRankName != null)
                            {
                                secondRankName.text = $"{tempNum +1}";
                            }
                        }
                    }
                    childGameObject.SetActive(true);
                }
                tempNum++;
            }
            else
            {
                if (childGameObject != null)
                {
                    childGameObject.SetActive(false);
                }
            }
        }
    }

    public void Connect(string characterId, string playerNickName, int currentMoney)
    {
        this.currentPrafab = characterId;       // 선택 되어있는 캐릭터 이름
        this.PlayerNickName = playerNickName;   // ID
        this.currentMoney = currentMoney;       // DB Select 해서 가져온 플레이어 보유금액

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();

    // 로비 입장 시 로컬 콜백으로 실행되는 함수
    
    public override void OnJoinedLobby()
    {
        _loginCamera.SetActive(false);
        obj.gameObject.SetActive(false);
        ObstracleParent.SetActive(false);
        lbDissconnectBtn.SetActive(true);                                           // 로비 닫기 버튼 On
        LobbyWaitObjec1.SetActive(true);                        
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
        if (PhotonNetwork.IsMasterClient) { InitializeObstacles(); }
        obj.SetActive(true);
        LobbyWaitObjec1.SetActive(false);
        GameWaitObjec1.SetActive(true);
        lbDissconnectBtn.SetActive(false);
        if (localPlayerPrefab != null) { Destroy(localPlayerPrefab); }
        LobbyPanel.SetActive(false);
        UIManager.Instance.LobbyUIControll("off");
        RoomPanel.SetActive(true);

        RoomRenewal();
        ChatInput.text = "";
        for (int i = 0; i < ChatText.Length; i++) ChatText[i].text = "";
        Vector3 spawnPosition = new Vector3(-157, 26, -62);
        GameObject playerObject = PhotonNetwork.Instantiate(currentPrafab, spawnPosition, Quaternion.identity);
     
        int viewID = playerObject.GetComponent<PhotonView>().ViewID;
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "objectViewID", viewID } });

        UpdatePlayerReadyStates();
    }
    private void InitializeObstacles()
    {
        if (ObstracleParentPink != null)
        {
            // Use GetComponentsInChildren to get all Rnd_Animation components in the entire hierarchy under ObstracleParent
            var obstacles = ObstracleParentPink.GetComponentsInChildren<Rnd_Animation>(true);
            foreach (var obstacle in obstacles)
            {
                obstacle.InitializeObstacle(); // Initialize each obstacle
            }
        }
        else
        {
            Debug.LogError("ObstracleParent is not assigned!");
        }
    }
    public override void OnCreateRoomFailed(short returnCode, string message) { RoomInput.text = ""; CreateRoom(); }

    public override void OnJoinRandomFailed(short returnCode, String message) { RoomInput.text = ""; CreateRoom(); }

    //딴 클라가 방 들어오면 Local플레이어 콜백함수로 호출 됨
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RoomRenewal();
        ChatRPC("<color=yellow>" + newPlayer.NickName + "님이 참가하셨습니다</color>");

        UpdatePlayerReadyStates();
    }
    //딴 클라가 방 나가면 Local플레이어 콜백함수로 호출 됨
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RoomRenewal();
        ChatRPC("<color=yellow>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");

        currentPlayerTransformDic.Remove(otherPlayer.NickName);
        NextPointDistance.Remove(otherPlayer.NickName);
        PlayerLastChkPoint.Remove(otherPlayer.NickName);
        RoomPlayerList.Remove(otherPlayer); // RoomPlayerList에서 플레이어를 제거합니다.

        UpdatePlayerReadyStates();
        Debug.Log("chk");
        PV.RPC("StopCountdown", RpcTarget.All);
    }

    void RoomRenewal()
    {
        RoomInfoText.text = PhotonNetwork.CurrentRoom.Name + " (" + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers + ")";
    }

    private bool allPlayersReady = false;
    private Coroutine countdownCoroutine;

    // 1. 레디 했거나
    // 2. 방 참가 했거나
    // 3. 다른 클라가 방을 나가거나
    // 4. 들어오거나
    // 5. 게임 END
    void UpdatePlayerReadyStates() 
    {
        if (GameStart) return;
        Debug.Log("?????");
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
            if (!PhotonNetwork.IsMasterClient) return;
            PV.RPC("StartCountdown", RpcTarget.All,5);
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
    void StartCountdown(int t)
    {
        if (countdownCoroutine == null)
        {
            countdownCoroutine = StartCoroutine(Countdown(t));
        }
    }
    [PunRPC]
    void FirstArrive(int viewID, Player player)
    {
        PhotonView firstView = PhotonView.Find(viewID);
        if (firstView != null)
        {
            _firstObjectName = firstView.gameObject.name;
            if(WinnerNickname != null)
            {
                WinnerNickname.text = string.Format("{0} 님의 " +
                    "하이라이트 영상 !!", player.NickName);
            }
            
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

    IEnumerator Countdown(int time)
    {

        int temptime = time;
        countPanel.SetActive(true);
        while (time > 0)
        {
            countDownText.text = time.ToString();
            AnimateCountPanel();
            yield return new WaitForSeconds(1);
            time--;
        }



        if (allPlayersReady && temptime == 5)
        {
            ShowStartText();
            if (PhotonNetwork.IsMasterClient) // 모든 RPC 타다가 마스터클라이언트 에서만 또 다시 아래 RPC를 호출함
            {

                PV.RPC("InitGameStartPlayers", RpcTarget.All);// B-1 게임 시작 시 체크포인트 및 거리 정보에 대한 플레이어 정보 초기화 
            yield return new WaitForSeconds(1);
            }
        } else if (temptime == 10)
        {
            GameShowEndText();
        }
    }

    void ShowStartText()
    {
        countDownText.text = "Start !";
        AnimateCountPanel();
        Debug.Log("All players are ready. Starting the game...");
    }

    void AnimateCountPanel()
    {
        countPanel_CanvasGroup.DOFade(1f, 0.3f).SetUpdate(true).OnComplete(() =>
        {
            countPanel_CanvasGroup.DOFade(0f, .3f).SetUpdate(true).SetDelay(.3f);
        });
        countDownText.rectTransform.DOScale(Vector3.one, .3f).SetUpdate(true).OnComplete(() =>
        {
            countDownText.rectTransform.DOScale(Vector3.zero, .3f).SetUpdate(true).SetDelay(.3f);
        });
    }


    void GameShowEndText()
    {
        countDownText.text = "E N D !";
        AnimateCountPanel();
        StopCountdown();

        // 게임 관련 변수 초기화
        IsGamestartCheck = false;

        GameStart = false;

        // 플레이어 도착 관련 배열 초기화
        arrivalOrder.Clear();
        int localPlayerViewID = (int)PhotonNetwork.LocalPlayer.CustomProperties["objectViewID"];
        foreach (Player player in RoomPlayerList)
        {
            if (player.CustomProperties.ContainsKey("objectViewID"))
            {
                int viewID = (int)player.CustomProperties["objectViewID"];
                PhotonView view = PhotonView.Find(viewID);
                if (view != null)
                {
                    view.gameObject.SetActive(false); // Corrected this line
                }
            }
        }


        _playerView = PhotonView.Find(localPlayerViewID);
        _playerView.gameObject.transform.position = new Vector3(-157.96f, 17.11f, -64.27f);
        _playerView.gameObject.SetActive(false);        
        _gameEndObject.SetActive(true);

        // 순위 UI 비활성화
        RankUIParents.SetActive(false);
        // 카운트다운 텍스트 초기화
        /*countDownText.text = "";*/

        // 게임 방 패널 활성화
        //RoomPanel.SetActive(true);


        // 플레이어 준비 상태 초기화
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "isReady", false } });
        // 입구 초기화
        Door door = DoorObj.GetComponent<Door>();
        if (door != null)
        {
            Vector3 doorPosition = door.transform.position;
            doorPosition.y = 6.249999f;
            door.transform.localPosition = doorPosition;  // 이 부분이 누락됨            
        }
        UpdatePlayerReadyStates();

    }


    // 방 나갈 시
    // 본인 클라에서만 호출
    public override void OnLeftRoom()
    {
        RankUIParents.SetActive(false);
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

    [PunRPC] // Send() 함수로 이거 호출한거면 PV.RPC로 호출한거라 모든 클라 실행이지만 ChatRPC() 함수 직접실행은 모든클라 실행이 아님 그냥 다른 사람이 방 나가면 방에 남아있는 모든 클라가 각자의 클라에서 콜백함수로 함수를 실행시키고 이 일반함수를 그냥 본인 PC에 호출하는 거임
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

    // 특정 클라이언트 로컬 콜백함수
    private void CallFunctionOnSpecificClient(Player targetPlayer)
    {


        if (photonView != null)
        {
            photonView.RPC("ReceiveMasterClientCommand", targetPlayer, targetPlayer);
        }
        else
        {
            Debug.LogWarning("PhotonView not found for player: " + targetPlayer.NickName);
        }
    }
    [PunRPC]
    void ReceiveMasterClientCommand(Player player)
    {
        // Find the player object using the ActorNumber
        Player targetPlayer = PhotonNetwork.CurrentRoom.GetPlayer(player.ActorNumber);
        if (targetPlayer != null)
        {
            GameObject playerObject = GetPlayerObject(targetPlayer);
            if (playerObject != null)
            {
                
                // Now you can get components on the playerObject
                var component = playerObject.transform.GetChild(0).GetComponent<CameraRecorder>();
                if (component != null)
                {
                    Debug.Log("videocombineing...");
                    component.CombineSegmentsIntoVideo();
                }
            }
        }
        int localPlayerViewID = (int)PhotonNetwork.LocalPlayer.CustomProperties["objectViewID"];

    }
    private GameObject GetPlayerObject(Player player)
    {
        foreach (var photonView in PhotonNetwork.PhotonViews)
        {
            if (photonView.Owner == player)
            {
                return photonView.gameObject;
            }
        }
        return null;
    }
}
#endregion
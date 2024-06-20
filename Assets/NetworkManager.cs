using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager NM;


    void Awake()
    {
        if (NM == null)
        {
            NM = this;
        }

        PV = GetComponent<PhotonView>();
        if (PV == null)
        {
            PV = gameObject.AddComponent<PhotonView>();
        }
        PhotonNetwork.AutomaticallySyncScene = true;
        test();
    }

    public PhotonView PV; // 수정: [HideInInspector] 제거
    [SerializeField] bool isStart;
    private Dictionary<int, bool> playerReadyStatus = new Dictionary<int, bool>();


    public void ChangeSceneWithDelay(string sceneName)
    {
        StartCoroutine(ChangeSceneAfterDelay(sceneName, 5f));
    }

    // Coroutine to change the scene after a delay
    private IEnumerator ChangeSceneAfterDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        PhotonNetwork.LoadLevel(sceneName);
    }

    // Example method to call when you want to change the scene 
    public void test()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ChangeSceneWithDelay("test"); // Replace "YourSceneName" with the actual scene name
        }
        else
        {
            Debug.LogWarning("Only the MasterClient can trigger a scene change.");
        }
    }

    public bool IsStart
    {
        get => isStart;
        set
        {
            PV.RPC(nameof(SetIsStartRPC), RpcTarget.AllBufferedViaServer, value);
            HandleGameStart();
        }
    }
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(1); // Load the lobby scene at index 1
    }


    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
    [PunRPC]
    void SetIsStartRPC(bool value)
    {
        isStart = value;
    }

    void Update()
    {
        print("현재 방 인원수 : " + PhotonNetwork.CurrentRoom.PlayerCount);

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            print(isStart);
            CheckAllPlayersReady();
        }
    }

    public void SetPlayerReady(int playerID, bool ready)
    {
        PV.RPC(nameof(SetPlayerReadyRPC), RpcTarget.All, playerID, ready);
    }

    [PunRPC]
    void SetPlayerReadyRPC(int playerID, bool ready)
    {
        if (playerReadyStatus.ContainsKey(playerID))
        {
            playerReadyStatus[playerID] = ready;
        }
        else
        {
            playerReadyStatus.Add(playerID, ready);
        }
        CheckAllPlayersReady();
    }

    void CheckAllPlayersReady()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (!playerReadyStatus.ContainsKey(player.ActorNumber) || !playerReadyStatus[player.ActorNumber])
            {
                return;
            }
        }
        IsStart = true;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (playerReadyStatus.ContainsKey(otherPlayer.ActorNumber))
        {
            playerReadyStatus.Remove(otherPlayer.ActorNumber);
        }
        base.OnPlayerLeftRoom(otherPlayer);
    }

    void HandleGameStart()
    {
        if (isStart)
        {
            // 게임 시작 로직 추가
            Debug.Log("All players are ready. Game Started!");
            // 예: 타이머 시작, 게임 요소 활성화 등
        }
    }
}

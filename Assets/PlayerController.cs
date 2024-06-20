using Photon.Pun;
using UnityEngine;
using static NetworkManager;
public class PlayerController : MonoBehaviourPun
{


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) // 플레이어가 R 키를 눌러 준비 상태를 설정한다고 가정
        {
            NM.SetPlayerReady(PhotonNetwork.LocalPlayer.ActorNumber, true);
        }
    }
}
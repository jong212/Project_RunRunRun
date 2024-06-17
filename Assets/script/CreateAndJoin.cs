using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class CreateAndJoin : MonoBehaviourPunCallbacks
{
    public TMP_InputField input_Create;
    public TMP_InputField input_Join;

    public void CreateRoom()
    {
        //PhotonNetwork.JoinRandomOrCreateRoom(null, 4);
        RoomOptions roomOptions = new RoomOptions() { MaxPlayers = 8, IsVisible = true, IsOpen = true };
        PhotonNetwork.CreateRoom(input_Create.text, roomOptions, TypedLobby.Default, null);
    }
    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(input_Join.text);
    }
    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("GamePlay");
        print("방에 참가한 플레이어 인원 수 " + PhotonNetwork.CountOfPlayersInRooms);
    }
}

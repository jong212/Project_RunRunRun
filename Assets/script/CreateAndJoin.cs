using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class CreateAndJoin : MonoBehaviourPunCallbacks
{
    public TMP_InputField input_Create;
    public TMP_InputField input_Join;
    
    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(input_Create.text);
    }
    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(input_Join.text);
    }
    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("GamePlay");
    }
}

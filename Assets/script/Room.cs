using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Room : MonoBehaviour
{
    public Text Name;

    // Start is called before the first frame update
    public void JoinRoom()
    {
        ConnectToServer.instance.JoinRoomInList(Name.text);
    }
}

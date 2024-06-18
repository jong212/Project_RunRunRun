using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RoomList : MonoBehaviourPunCallbacks
{
    public GameObject RoomPrefab; // 방을 나타내는 프리팹
    List<RoomInfo> myList = new List<RoomInfo>(); // 현재 유지하는 방 목록 리스트

    void Awake()
    {
        // 로비에 입장하여 방 목록 업데이트를 시작
        // PhotonNetwork.JoinLobby();
    }

    // Photon PUN 콜백 메서드를 사용하여 방 목록 업데이트 처리
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

    // 다음 프레임에서 방의 위치를 로깅하기 위한 코루틴
    private IEnumerator LogPositionNextFrame(GameObject room)
    {
        yield return null; // 다음 프레임까지 대기
        // 여기서는 주로 방의 위치를 출력하거나 다른 처리를 추가할 수 있습니다.
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
        // 로비에 입장했을 때, 방 목록을 업데이트하도록 설정
    }

    public override void OnLeftLobby()
    {
        Debug.Log("Left Lobby");
        // 로비를 떠났을 때의 처리
    }
}

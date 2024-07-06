using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleCollider : MonoBehaviourPunCallbacks
{
    public CameraRecorder cm;
    bool _chkBool { get; set; }



    private void OnEnable()
    {
        _chkBool = false;
    }
    private void OnDisable()
    {
        _chkBool = false;

    }
    private void OnTriggerEnter(Collider other)
    {
        if (_chkBool) return;
        PhotonView pvPlayer = other.GetComponent<PhotonView>();
        if (pvPlayer != null)
        {
            // Get the local player's PhotonView ID from custom properties
            int localPlayerViewID = (int)PhotonNetwork.LocalPlayer.CustomProperties["objectViewID"];

            if (pvPlayer.ViewID == localPlayerViewID)
            {
                 cm = pvPlayer.transform.GetChild(0).GetComponent<CameraRecorder>();
                
                if (cm != null)
                {
                    _chkBool = true;
                    Debug.Log("ю╫..");
                    cm.OnRecordButtonClicked();
                }
               
            }
            else
            {
                Debug.Log("╥ндц╬ф╢т");
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

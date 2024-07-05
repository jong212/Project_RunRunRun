using Photon.Pun;
using UnityEngine;
using DG.Tweening;

public class Door : MonoBehaviourPun
{
    public void MoveDoorUp()
    {
        transform.DOMoveY(transform.position.y + 5, 1.5f);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WNDRigidbodyCenter : MonoBehaviour
{
    public Vector3 CenterOfMass;
    void Awake()
    {
        GetComponent<Rigidbody>().centerOfMass = CenterOfMass;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color32(0, 255, 0, 50);
        Gizmos.DrawSphere(transform.TransformPoint(CenterOfMass), 0.5f);
    }
}

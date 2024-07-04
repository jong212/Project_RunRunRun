using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnOffInit : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnEnable()
    {
        Debug.Log("on");
    }
    private void OnDisable()
    {
        Debug.Log("off");
    }
}

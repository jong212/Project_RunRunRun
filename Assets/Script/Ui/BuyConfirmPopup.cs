using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuyConfirmPopup : MonoBehaviour
{
    private Button _btn;
    void Start()
    {
        _btn = GetComponent<Button>();
        if (_btn != null)
        {
            _btn.onClick.AddListener(CallParentFunction);
        }
    }
    void CallParentFunction()
    {
        // 부모 오브젝트 찾기
        /*   Transform parent = transform.parent.parent;
           if (parent != null)
           {
               parent.gameObject.SetActive(false);
           }
           else
           {
               Debug.LogError("Parent transform is null.");
           }*/
        UIManager.Instance.CloseSpecificUI(UIType.BuyPopup);
    }
    // Update is called once per frame

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuyBtn : MonoBehaviour
{
    private Button _btn;
    void Start()
    {
        _btn = GetComponent<Button>();
        if(_btn!= null)
        {
            _btn.onClick.AddListener(CallParentFunction);
        }
    }
    void CallParentFunction()
    {
        // 부모 오브젝트 찾기
        MainUI parent = GetComponentInParent<MainUI>();
        if (parent != null)
        {
            parent.SetBuyButton(gameObject);

            parent.OnClick_ShopUIBuyBtn();
        }
        else
        {
            Debug.LogError("ParentObject script not found in parent hierarchy.");
        }

    }
    // Update is called once per frame

}

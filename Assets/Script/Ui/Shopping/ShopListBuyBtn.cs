using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ShopListBuyBtn : MonoBehaviour
{
    private Button _btn;
    [SerializeField] private Text _PriceValue;
    [SerializeField] private Text _CharacterNameValue;
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
            // A-0 : 누른 버튼의 캐릭터 이름 전달 (ex Tony)
            parent.SetBuyButton(_CharacterNameValue.text);
            parent.otherScriptBtn = _btn;
            // A-2 : 누른 버튼의 캐릭터 판매가격을 MainUI에 저장
            parent.OnClick_ShopUIBuyBtn(_PriceValue.text);
        }
        else
        {
            Debug.LogError("ParentObject script not found in parent hierarchy.");
        }

    }
    // Update is called once per frame
 
}

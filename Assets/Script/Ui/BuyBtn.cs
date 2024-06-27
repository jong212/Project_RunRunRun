using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// 캐릭터 구매하기 버튼을 누르면 캐릭터 이름 Value를 MainUI에 세팅해준다


public class BuyBtn : MonoBehaviour
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
            parent.SetBuyButton(_CharacterNameValue.text);


            parent.OnClick_ShopUIBuyBtn();
        }
        else
        {
            Debug.LogError("ParentObject script not found in parent hierarchy.");
        }

    }
    // Update is called once per frame

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// A : 캐릭터 Buy 버튼 클릭 > 캐릭터 이름값을 MainUI 에 있는 변수에 세팅 


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
            // A
            parent.SetBuyButton(_CharacterNameValue.text);


            parent.OnClick_ShopUIBuyBtn(_PriceValue.text);
        }
        else
        {
            Debug.LogError("ParentObject script not found in parent hierarchy.");
        }

    }
    // Update is called once per frame

}

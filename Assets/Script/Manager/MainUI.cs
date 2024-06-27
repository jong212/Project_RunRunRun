using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    public string CharacterPriceValue { get; set; }
    public string CharacterNameValue { get; set; }
    [SerializeField] private DBManager _DBManager;
    private void OnDisable()
    {
        UIManager.Instance.RegisterOnClickConfirmEvent(false, OnClickConfirmPopup);
    }

    // 상점 UI 관련 : 상점 아이콘 클릭 시
    public void OnClick_ShopUIBtn()
    {
        UIManager.Instance.OpenShopPopupBtn();
    }

    // 상점 UI 관련 : 상점리스트의 Buy클릭 시
    public void OnClick_ShopUIBuyBtn()
    {
        
        UIManager.Instance.OpenShopBuyPopupBtn();

        // UI 매니저에게 콜백함수 넘겨줌
        // UIManager에서는 전달받은 콜백을 ShopBuyYesBtn로 다시 전달해서 구독함
        UIManager.Instance.RegisterOnClickConfirmEvent(true, OnClickConfirmPopup);
    }
    // 상점 UI 관련 : Invoke 이벤트 호출 될 때  (최종 BuyPopup에서 예 버튼 누를 때)
    public void OnClickConfirmPopup()
    {
        // 최종 구매하기 예 버튼을 누르면 캐릭터이름값만 DB에 전달시킴 > DB에서는 플레이어 닉네임에 해당하는 구매한 캐릭터 프리팹 이름을 insert
        _DBManager.InsertCharacterInfo(CharacterNameValue);

    }
    // 상점 UI 관련 : 
    public void SetBuyButton(string characterNameValue)
    {
        CharacterNameValue = characterNameValue;
    }

    // 캐릭터 UI 관련 
    // UIManager 에게 요청
    public void OnClick_CharacterIconBtn()
    {
        UIManager.Instance.OpenCharacterPopup();
    }
}

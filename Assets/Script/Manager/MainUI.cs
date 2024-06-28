using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    [SerializeField] Text currentMoney; // UI MOney
    public string CharacterNameValue { get; set; }
    public int CharacterPriceValue { get; set; }
    [SerializeField] private DBManager _DBManager;
    private void OnDisable()
    {
        UIManager.Instance.RegisterOnClickConfirmEvent(false, OnClickConfirmPopup);
    }

    // 상점UI 아이콘 클릭 시
    public void OnClick_ShopUIBtn()
    {
        UIManager.Instance.OpenShopPopupBtn();
    }

    // 열린상점에서 Buy 버튼 클릭 시
    public void OnClick_ShopUIBuyBtn(string price)
    {
        //to int 로 change
        if (int.TryParse(price, out int priceValue))
        {
            CharacterPriceValue = priceValue;
            UIManager.Instance.OpenShopBuyPopupBtn();

            // UI 매니저에게 콜백함수 넘겨줌        
            UIManager.Instance.RegisterOnClickConfirmEvent(true, OnClickConfirmPopup);
        }

    }
    // 사용자가 구매하기 팝업에서 Yes를 클릭했을 때 Invoke로 실행 됨/
    public void OnClickConfirmPopup()
    { 
                
        if(currentMoney != null){
       
                int currentMoneyInt = Convert.ToInt32(currentMoney.text);
            // 사용자가 물건을 구매할 때마다 DB에서 플레이어의 보유금액을 select 해올 수 없어서
            // 로그인 할 때 1회만 select 해와서 보유금액 받아오고 CurrentGold에 넣어둔 다음
            // 인게임에서 구매가 일어나면 일단은 CurrentGold와 UI의 currentMoneyInt 값이 같은지를 비교

            
            if (_DBManager.CurrentGold == currentMoneyInt)
            {   
                // 같다면 현재 보유 금액에서 구매할 캐릭의 금액을 차감시켜서 구매 가능한지 체크
                var tempCalcValue = currentMoneyInt - CharacterPriceValue;
                if (tempCalcValue < 0)
                {
                    Debug.Log("돈이 부족함");
                } 
                else
                {
                    _DBManager.CurrentGold = _DBManager.CurrentGold - CharacterPriceValue;
                    _DBManager.UpdatePlayerGold(_DBManager.CurrentGold);
                    currentMoney.text = _DBManager.CurrentGold.ToString();
                }
            }
            else
            {
                Debug.Log("The current money does not match the DB value.");
            }
        }
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

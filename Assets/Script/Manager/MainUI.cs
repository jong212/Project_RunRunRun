using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    [SerializeField] Text currentMoney; // UI MOney
    public Button otherScriptBtn;
    public string CharacterNameValue { get; set; }
    public int CharacterPriceValue { get; set; } // Buy 버튼 누를 때마다 여기 가격이 최신화 됨
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
    private void Update()
    {
        //Debug.Log(CharacterNameValue);
    }
    // A-3 : UI 매니저에게 Last 팝업 열도록 요청함과 동시에 콜백함수 UI Manager에게 전달
    public void OnClick_ShopUIBuyBtn(string price)
    {
        if (int.TryParse(price, out int priceValue))
        {
            CharacterPriceValue = priceValue; 
            UIManager.Instance.OpenShopBuyPopupBtn(); // 요청

            
            UIManager.Instance.RegisterOnClickConfirmEvent(true, OnClickConfirmPopup); // UI 매니저에게 콜백함수 넘겨줌        
        }

    }
    // A-6 : 사용자가 구매하기 팝업에서 Yes를 클릭했을 때 Invoke로 실행 됨/
    public void OnClickConfirmPopup()
    { 
                
        if(currentMoney != null){
       
                int currentMoneyInt = Convert.ToInt32(currentMoney.text);
            // 사용자가 물건을 구매할 때마다 DB에서 플레이어의 보유금액을 select 해올 수 없어서
            // 로그인 할 때 1회만 select 해와서 보유금액 받아오고 CurrentGold에 넣어둔 다음
            // 인게임에서 구매가 일어나면 일단은 CurrentGold와 UI의 currentMoneyInt 값이 같은지를 비교

            
            if (_DBManager.CurrentGold == currentMoneyInt)
            {   
                
                var tempCalcValue = currentMoneyInt - CharacterPriceValue;// 같다면 현재 보유 금액에서 구매할 캐릭의 금액을 차감시켜서 구매 가능한지 체크
                if (tempCalcValue < 0)
                {
                    Debug.Log("돈이 부족함");
                } 
                else
                {                    
                    /*
                     * 보유금액 3개 최신화 함 
                     * 1. DBManager의 CurrentGold  
                     * 2. 실제 DB 보유금액
                     * 3. UI 보유금액 모두 업데이트
                     */
                    _DBManager.CurrentGold = _DBManager.CurrentGold - CharacterPriceValue;// 1. 구매 했으니 보유금액 동기화(실제 MariaDb접근은 아님)
                    _DBManager.UpdatePlayerGold(_DBManager.CurrentGold);                  // 2. Maria DB Player  보유금액 Update 
                    currentMoney.text = _DBManager.CurrentGold.ToString();                // 3. UI보유금액 최신화 
                    otherScriptBtn.interactable = false;
                    UIManager.Instance.CloseSpecificUI(UIType.ShopBuyPopup);
                    
                    
                    // 구매완료 시 실제 DB에 캐릭터이름 추가하고 후처리로 DB매니저에서 OwnedCharacters List에도 추가 되도록 
                    _DBManager.InsertCharacterInfo(CharacterNameValue);
                }
            }
            else
            {
                Debug.Log("The current money does not match the DB value.");
            }
        }


        
    }
    // A-1 : 마지막으로 어떤 캐릭터를 구매하려 했는지 알기 위해 리스트에서 buy 클릭하면 해당 캐릭터 이름 예를들어 Tonnny 변수에 저장
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

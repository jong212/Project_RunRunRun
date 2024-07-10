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
    public int CharacterPriceValue { get; set; } // Buy ��ư ���� ������ ���� ������ �ֽ�ȭ ��
    [SerializeField] private DBManager _DBManager;
    private void OnDisable()
    {
        UIManager.Instance.RegisterOnClickConfirmEvent(false, OnClickConfirmPopup);
    }

    // ����UI ������ Ŭ�� ��
    public void OnClick_ShopUIBtn()
    {
        UIManager.Instance.OpenShopPopupBtn();
    }
    private void Update()
    {
        //Debug.Log(CharacterNameValue);
    }
    // A-3 : UI �Ŵ������� Last �˾� ������ ��û�԰� ���ÿ� �ݹ��Լ� UI Manager���� ����
    public void OnClick_ShopUIBuyBtn(string price)
    {
        if (int.TryParse(price, out int priceValue))
        {
            CharacterPriceValue = priceValue; 
            UIManager.Instance.OpenShopBuyPopupBtn(); // ��û

            
            UIManager.Instance.RegisterOnClickConfirmEvent(true, OnClickConfirmPopup); // UI �Ŵ������� �ݹ��Լ� �Ѱ���        
        }

    }
    // A-6 : ����ڰ� �����ϱ� �˾����� Yes�� Ŭ������ �� Invoke�� ���� ��/
    public void OnClickConfirmPopup()
    { 
                
        if(currentMoney != null){
       
                int currentMoneyInt = Convert.ToInt32(currentMoney.text);
            // ����ڰ� ������ ������ ������ DB���� �÷��̾��� �����ݾ��� select �ؿ� �� ���
            // �α��� �� �� 1ȸ�� select �ؿͼ� �����ݾ� �޾ƿ��� CurrentGold�� �־�� ����
            // �ΰ��ӿ��� ���Ű� �Ͼ�� �ϴ��� CurrentGold�� UI�� currentMoneyInt ���� �������� ��

            
            if (_DBManager.CurrentGold == currentMoneyInt)
            {   
                
                var tempCalcValue = currentMoneyInt - CharacterPriceValue;// ���ٸ� ���� ���� �ݾ׿��� ������ ĳ���� �ݾ��� �������Ѽ� ���� �������� üũ
                if (tempCalcValue < 0)
                {
                    Debug.Log("���� ������");
                } 
                else
                {                    
                    /*
                     * �����ݾ� 3�� �ֽ�ȭ �� 
                     * 1. DBManager�� CurrentGold  
                     * 2. ���� DB �����ݾ�
                     * 3. UI �����ݾ� ��� ������Ʈ
                     */
                    _DBManager.CurrentGold = _DBManager.CurrentGold - CharacterPriceValue;// 1. ���� ������ �����ݾ� ����ȭ(���� MariaDb������ �ƴ�)
                    _DBManager.UpdatePlayerGold(_DBManager.CurrentGold);                  // 2. Maria DB Player  �����ݾ� Update 
                    currentMoney.text = _DBManager.CurrentGold.ToString();                // 3. UI�����ݾ� �ֽ�ȭ 
                    otherScriptBtn.interactable = false;
                    UIManager.Instance.CloseSpecificUI(UIType.ShopBuyPopup);
                    
                    
                    // ���ſϷ� �� ���� DB�� ĳ�����̸� �߰��ϰ� ��ó���� DB�Ŵ������� OwnedCharacters List���� �߰� �ǵ��� 
                    _DBManager.InsertCharacterInfo(CharacterNameValue);
                }
            }
            else
            {
                Debug.Log("The current money does not match the DB value.");
            }
        }


        
    }
    // A-1 : ���������� � ĳ���͸� �����Ϸ� �ߴ��� �˱� ���� ����Ʈ���� buy Ŭ���ϸ� �ش� ĳ���� �̸� ������� Tonnny ������ ����
    public void SetBuyButton(string characterNameValue)
    {
        CharacterNameValue = characterNameValue;
    }

    // ĳ���� UI ���� 
    // UIManager ���� ��û
    public void OnClick_CharacterIconBtn()
    {
        UIManager.Instance.OpenCharacterPopup();
    }
}

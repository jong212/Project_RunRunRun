using System;
using UnityEngine;

public class ShopPopupUI : MonoBehaviour
{

 
    
    //쇼핑 팝업 닫기
    public void onClick_Close()
    {
        UIManager.Instance.CloseSpecificUI(UIType.ShopPopup);
        UIManager.Instance.CloseSpecificUI(UIType.BuyPopup);
    }

    public void onClick_BuyBtnClose()
    {
        UIManager.Instance.CloseSpecificUI(UIType.BuyPopup);
    }

    // Main Ui 에 있는 콜백함수 실행

}

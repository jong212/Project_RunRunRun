using System;
using UnityEngine;

public class ShopPopupUI : MonoBehaviour
{
    
    //¼îÇÎ ÆË¾÷ ´Ý±â
    public void onClick_Close()
    {
        UIManager.Instance.CloseSpecificUI(UIType.ShopPopup);
        UIManager.Instance.CloseSpecificUI(UIType.ShopBuyPopup);
    }

    public void onClick_BuyBtnClose()
    {
        UIManager.Instance.CloseSpecificUI(UIType.ShopBuyPopup);
    }

}

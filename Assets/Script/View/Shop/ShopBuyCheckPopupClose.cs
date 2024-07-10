using Photon.Pun.UtilityScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopBuyCheckPopupClose : MonoBehaviour
{
    public void OnClick_BuyCheckPopupClock()
    {
        UIManager.Instance.CloseSpecificUI(UIType.ShopBuyPopup);
    }
}

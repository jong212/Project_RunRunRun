using System;
using UnityEngine;

public class ShopPopupUI : MonoBehaviour
{
    public Action _onConfirmEventHandler;

    private void OnDisable()
    {
        if (_onConfirmEventHandler != null)
            _onConfirmEventHandler = null;
    }

    public void RegisterOnClickConfirmEvent(bool isRegister, Action callback)
    {
        if (isRegister)
            _onConfirmEventHandler += callback;
        else
            _onConfirmEventHandler -= callback;
    }

    
    //¼îÇÎ ÆË¾÷ ´Ý±â
    public void onClick_Close()
    {
        UIManager.Instance.CloseSpecificUI(UIType.ShopPopup);
        UIManager.Instance.CloseSpecificUI(UIType.BuyPopup);
    }

    public void onClick_BuyBtnClose()
    {
        UIManager.Instance.CloseSpecificUI(UIType.BuyPopup);
    }

    public void OnClick_Confirm()
    {
        _onConfirmEventHandler?.Invoke();
    }
}

using System;
using UnityEngine;

public class ConfirmButton : MonoBehaviour
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

     
    public void onClick_Close()
    {
        UIManager.Instance.CloseSpecificUI(UIType.ConfirmPopup);
    }

    public void OnClick_Confirm()
    {
        _onConfirmEventHandler?.Invoke();
    }
}

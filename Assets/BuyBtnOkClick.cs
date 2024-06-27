using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BuyBtnOkClick : MonoBehaviour
{
    public Button _btn;
    public Action _onConfirmEventHandler;

    private void Start()
    {
        _btn = GetComponent<Button>();
        _btn.onClick.AddListener(OnClick_Confirm);
    }
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
    public void OnClick_Confirm()
    {
        Debug.Log("dsdfdsdf");
        _onConfirmEventHandler?.Invoke();
    }
}

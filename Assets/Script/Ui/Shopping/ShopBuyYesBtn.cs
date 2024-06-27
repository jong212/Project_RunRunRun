using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
// A 상점 열 때 이벤트 등록
// B 상점에서 Buy 버튼 클릭 후 Yes or No 단계에서 Yes 누를 때 OnClick_Confirm 인보크 실행
public class ShopBuyYesBtn : MonoBehaviour
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
    //A
    public void RegisterOnClickConfirmEvent(bool isRegister, Action callback)
    {
        if (isRegister)
            _onConfirmEventHandler += callback;
        else
            _onConfirmEventHandler -= callback;
    }
    //B
    public void OnClick_Confirm()
    {
        _onConfirmEventHandler?.Invoke();
    }
}

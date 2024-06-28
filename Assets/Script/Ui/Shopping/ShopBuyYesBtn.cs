using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
// A 상점 열 때 이벤트를 미리 등록
// B 최종 구매팝업에서 Yes 버튼 누를 때 인보크 실행
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

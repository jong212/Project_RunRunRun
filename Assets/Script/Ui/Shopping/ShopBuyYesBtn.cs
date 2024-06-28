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

    //A-4 : Ui 매니저가 구독하라고 지시
    public void RegisterOnClickConfirmEvent(bool isRegister, Action callback)
    {
        if (isRegister)
            _onConfirmEventHandler += callback;
        else
            _onConfirmEventHandler -= callback;
    }
    //A-5 : 마지막 확인 버튼이 눌리면 이벤트 발생은 준비가 된 상태라 아래 Invoke를 실행시켜서 등록된 함수를 실행한다
    public void OnClick_Confirm()
    {
        _onConfirmEventHandler?.Invoke();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//(이벤트 연결 순서 정리) 상점 Ui 여는 버튼 클릭(여길 타겠지) > UI 매니저에게 콜백함수 넘겨줌 > UI 매니저는 받은 콜백함수로 매니저에서 뭔가 처리를  하지는 않고 상점 UI가 하이어라키에 있는지 체크만 하고 있다면 "상점 UI 에게 콜백을 다시 넘기"면서 > 상점 UI에서는 넘겨받은 콜백함수를 += 등록만 시켜놓음 > 상점 UI 에서 특정 버튼을 누르면 += 해놓은거 인보크 할 수 있는 함수 만들고 연결만 하면 됨
public class MainUI : MonoBehaviour
{
    //BuyBtn 에서 상점에서 구매 버튼 클릭했을 때 아래 넣어놓음
    public GameObject BuyBtn { get; set; }

    private void OnDisable()
    {
        UIManager.Instance.RegisterOnClickConfirmEvent(false, OnClickConfirmPopup);
    }


    public void OnClick_ShopUIBtn()
    {
        UIManager.Instance.OpenShopPopupBtn();
    }
    public void OnClick_ShopUIBuyBtn()
    {
        Debug.Log("test");
        UIManager.Instance.OpenShopBuyPopupBtn();
        UIManager.Instance.RegisterOnClickConfirmEvent(true, OnClickConfirmPopup);//ToDO 이건 Buy 버튼 누를 때 처리되는 콜백들로 추후 변경해야할듯
    }

    public void OnClickConfirmPopup()
    {
        Debug.Log("...");
    }
    public void SetBuyButton(GameObject button)
    {
        BuyBtn = button;
        Debug.Log(button);
    }
}

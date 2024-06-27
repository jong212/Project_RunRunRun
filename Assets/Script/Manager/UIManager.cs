using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UIType
{
    MainUI,
    ShopPopup,
    BuyPopup,
    CharacterPopup,
}


public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject ShopParentsObj;
    [SerializeField] Transform CharacterParentsObj;


    public static UIManager Instance { get; set; }

    // 얘는 생성과 제거에 관한 부분 -> Instancing과 가비지컬렉터와 연관이 있는 애
    private Dictionary<UIType, GameObject> _createdUIDic = new Dictionary<UIType, GameObject>();
    // 얘는 활성과 비활성에 관한 부분 -> SetActive
    private HashSet<UIType> _openedUIDic = new HashSet<UIType>();

    private void Awake()
    {
        Instance = this;
    }

    // 공용
    // 열어주고 
    private void OpenUI(UIType uiType, GameObject uiObject)
    {
        if (_openedUIDic.Contains(uiType) == false)
        {
            //OpenUI를 바로 타는 케이스가 있어서 비활성화 되어있는 오브젝트를 활성화 시키고 싶어서
            uiObject.SetActive(true);
            _openedUIDic.Add(uiType);
        }
    }

    // 공용
    // 닫아주고 
    private void CloseUI(UIType uiType)
    {
        if (_openedUIDic.Contains(uiType))
        {
            var uiObject = _createdUIDic[uiType];
            uiObject.SetActive(false);
            _openedUIDic.Remove(uiType);
        }
    }
    

    // 공용
    // 생성해주고  
    private void CreateUI(UIType uiType)
    {
        if (_createdUIDic.ContainsKey(uiType) == false)
        {
            string path = GetUIPath(uiType);
            GameObject loadedObj = (GameObject)Resources.Load(path);

            GameObject gObj = null; 
            if (uiType == UIType.ShopPopup)
            {
                gObj = Instantiate(loadedObj, ShopParentsObj.transform);
            } else if (uiType == UIType.BuyPopup)
            {
                gObj = Instantiate(loadedObj, ShopParentsObj.GetComponentInChildren<ShopPopupUI>().transform);
            } else if (uiType == UIType.CharacterPopup)
            {
                gObj = Instantiate(loadedObj, CharacterParentsObj.transform);
            }

            
            if (gObj != null)
            {
                _createdUIDic.Add(uiType, gObj);
            }
        }
    }

    // 공통 : 생성요청
    
    private GameObject GetCreatedUI(UIType uiType)
    {
        if (_createdUIDic.ContainsKey(uiType) == false)
        {
            CreateUI(uiType);
        }

        return _createdUIDic[uiType];
    }

    // 공통 : 리소스 경로 반환
    private string GetUIPath(UIType uiType)
    {
        string path = string.Empty; // "" == string.Empty
        switch (uiType)
        {
            case UIType.ShopPopup:
                path = "UI/LobbyShopPopupUI";
                break;
            case UIType.BuyPopup:
                path = "UI/BuyPopup";
                break;
            case UIType.CharacterPopup:
                path = "UI/Slider/Card Slider";
                break;
        }

        return path;
    }
    public void CloseSpecificUI(UIType uiType)
    {
        CloseUI(uiType);
    }

    #region Dic Add && Open 
    // 상점 UI Open : 단독 : 확장가능
    public void OpenShopPopupBtn()
    {
        // 하이어라키에 존재하는지 체크
        var gObj = GetCreatedUI(UIType.ShopPopup);
        if (gObj != null)
        {
            OpenUI(UIType.ShopPopup, gObj);
             var ConfirmButton = gObj.GetComponent<ShopPopupUI>();
        }
    }
    // 상점 BuyListPopup Open : 단독 : 확장가능
    public void OpenShopBuyPopupBtn()
    {
        // 하이어라키에 존재하는지 체크
        var gObj = GetCreatedUI(UIType.BuyPopup);
        if (gObj != null)
        { 
            OpenUI(UIType.BuyPopup, gObj);
            var ConfirmButton = gObj.GetComponent<ShopPopupUI>();
        }
    }


    // 캐릭터 UI 관련
    public void OpenCharacterPopup()
    {
        // 하이어라키에 존재하는지 체크
        var gObj = GetCreatedUI(UIType.CharacterPopup);
        if (gObj != null)
        {
            OpenUI(UIType.CharacterPopup, gObj);
            var ConfirmButton = gObj.GetComponent<ShopPopupUI>();
        }
    }
    #endregion



    public void RegisterOnClickConfirmEvent(bool isRegister, Action callback)
    {
        if (_createdUIDic.ContainsKey(UIType.BuyPopup))
        {
            var gObj = _createdUIDic[UIType.BuyPopup];
            var ShopPopup = gObj.GetComponentInChildren<ShopBuyYesBtn>();
            ShopPopup?.RegisterOnClickConfirmEvent(isRegister, callback);
        }
    }


}

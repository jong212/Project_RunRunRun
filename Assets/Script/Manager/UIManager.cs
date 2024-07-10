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
    [SerializeField] GameObject LobbyUIContainer;
    [SerializeField] Transform CharacterParentsObj;
    [SerializeField] GameObject BottomUIGroup;
    [SerializeField] GameObject TopUIGroup;

    public static UIManager Instance { get; set; }

    // ��� ������ ���ſ� ���� �κ� -> Instancing�� �������÷��Ϳ� ������ �ִ� ��
    private Dictionary<UIType, GameObject> _createdUIDic = new Dictionary<UIType, GameObject>();
    // ��� Ȱ���� ��Ȱ���� ���� �κ� -> SetActive
    private HashSet<UIType> _openedUIDic = new HashSet<UIType>();

    private void Awake()
    {
        Instance = this;
    }

    // ����
    // �����ְ� 
    private void OpenUI(UIType uiType, GameObject uiObject)
    {
        if (_openedUIDic.Contains(uiType) == false)
        {
            //OpenUI�� �ٷ� Ÿ�� ���̽��� �־ ��Ȱ��ȭ �Ǿ��ִ� ������Ʈ�� Ȱ��ȭ ��Ű�� �;
            uiObject.SetActive(true);
            _openedUIDic.Add(uiType);
        }
    }

    // ����
    // �ݾ��ְ� 
    public void CloseUI(UIType uiType)
    {
        if (_openedUIDic.Contains(uiType))
        {
            var uiObject = _createdUIDic[uiType];
            uiObject.SetActive(false);
            _openedUIDic.Remove(uiType);
            if(uiType == UIType.CharacterPopup)
            {
                _createdUIDic.Remove(uiType);
            }
        }
    }
    

    // ����
    // �������ְ�  
    private void CreateUI(UIType uiType)
    {
        if (_createdUIDic.ContainsKey(uiType) == false)
        {
            string path = GetUIPath(uiType);
            GameObject loadedObj = (GameObject)Resources.Load(path);

            GameObject gObj = null; 
            if (uiType == UIType.ShopPopup)
            {
                gObj = Instantiate(loadedObj, LobbyUIContainer.transform);
            } else if (uiType == UIType.BuyPopup)
            {
                gObj = Instantiate(loadedObj, LobbyUIContainer.GetComponentInChildren<ShopPopupUI>().transform);
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

    // ���� : ������û
    
    private GameObject GetCreatedUI(UIType uiType)
    {
        if (_createdUIDic.ContainsKey(uiType) == false)
        {
            CreateUI(uiType);
        }

        return _createdUIDic[uiType];
    }

    // ���� : ���ҽ� ��� ��ȯ
    private string GetUIPath(UIType uiType)
    {
        string path = string.Empty; // "" == string.Empty
        switch (uiType)
        {
            case UIType.ShopPopup:
                path = "UI/Lobby/ShopPopup";
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
    // ���� UI Open : �ܵ� : Ȯ�尡��
    public void OpenShopPopupBtn()
    {
        // ���̾��Ű�� �����ϴ��� üũ
        var gObj = GetCreatedUI(UIType.ShopPopup);
        if (gObj != null)
        {
            OpenUI(UIType.ShopPopup, gObj);
             var ConfirmButton = gObj.GetComponent<ShopPopupUI>();
        }
    }
    // ���� BuyListPopup Open : �ܵ� : Ȯ�尡��
    public void OpenShopBuyPopupBtn()
    {
        // ���̾��Ű�� �����ϴ��� üũ
        var gObj = GetCreatedUI(UIType.BuyPopup);
        if (gObj != null)
        { 
            OpenUI(UIType.BuyPopup, gObj);
            var ConfirmButton = gObj.GetComponent<ShopPopupUI>();
        }
    }


    // ĳ���� UI ����
    public void OpenCharacterPopup()
    {
        // ���̾��Ű�� �����ϴ��� üũ
        var gObj = GetCreatedUI(UIType.CharacterPopup);
        if (gObj != null)
        {
            OpenUI(UIType.CharacterPopup, gObj);
            /*var ConfirmButton = gObj.GetComponent<ShopPopupUI>();*/
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

    public void LobbyUIControll (string onoff)
    {
        if(onoff == "on")
        {
            BottomUIGroup.SetActive(true);
            TopUIGroup.SetActive(true);
        } else
        {
            BottomUIGroup.SetActive(false);
            TopUIGroup.SetActive(false);
        }
        
    }
}

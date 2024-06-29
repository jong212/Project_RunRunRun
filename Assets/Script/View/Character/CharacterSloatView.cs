using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSloatView : MonoBehaviour
{
    [SerializeField] Image Image_Icon;
    [SerializeField] DBManager db;
    public string _cName {get;set;}
    //#1 GetSet »ç¿ë¹ý
    [SerializeField] private bool _isBool;

    private void Awake()
    {
        db = GameObject.FindWithTag("DB").GetComponent<DBManager>();
    }
    public bool _Isbool
    {
        get{ return _isBool; }
        set{ _isBool = value; }
    }
    private Shop _characterInfo;

    public void SetUI(Shop cinfo, bool _isbool)
    {
        _cName = cinfo.Name;
        _characterInfo = cinfo;
        _Isbool = _isbool;
        //var skillData = LobbyDataManager.Inst.GetCharacterData(_skillClassName);
        //Debug.Log(skillData);
        if (_characterInfo != null)
        {
            var path = $"Textures/ShopIcon/{_characterInfo.Name}";
            Image_Icon.sprite = Resources.Load<Sprite>(path);
        }
    }
    public void CharacterCHange()
    {
        if(_cName != null)
        {
            db.UpdatePlayerCharacterId(_cName);
        }
    }
    /*public void OnClick_OpenTooltip()
    {
        var skillData = DataManager.Inst.GetSkillData(_skillClassName);
        if (skillData == null)
            return;

        UIManager.Instance.OpenTooltipPopup(string.Format(skillData.Description, skillData.BaseDamage));
    }*/
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSloatView : MonoBehaviour
{
    [SerializeField] Image Image_Icon;

    //#1 GetSet »ç¿ë¹ý
    [SerializeField] private bool _isBool;

    public bool _Isbool
    {
        get{ return _isBool; }
        set{ _isBool = value; }
    }
    private Shop _skillClassName;

    public void SetUI(Shop skillClassName, bool _isbool)
    {
        _skillClassName = skillClassName;
        _Isbool = _isbool;
        //var skillData = LobbyDataManager.Inst.GetCharacterData(_skillClassName);
        //Debug.Log(skillData);
        if (_skillClassName != null)
        {
            var path = $"Textures/ShopIcon/{_skillClassName.Name}";
            Image_Icon.sprite = Resources.Load<Sprite>(path);
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

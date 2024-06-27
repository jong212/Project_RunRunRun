using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSloatView : MonoBehaviour
{
    [SerializeField] Image Image_Icon;

    private Shop _skillClassName;

    public void SetUI(Shop skillClassName)
    {
        _skillClassName = skillClassName;

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

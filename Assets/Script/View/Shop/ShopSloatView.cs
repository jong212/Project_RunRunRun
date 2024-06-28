using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopSloatView : MonoBehaviour
{
    [SerializeField] Image Image_Icon;
    [SerializeField] Text Text_SkillName;
    [SerializeField] Text Text_CharacterName;
    [SerializeField] Button hideBtn;
    private Shop _skillClassName;

    public void SetUI(Shop skillClassName,bool chk)
    {
        _skillClassName = skillClassName;

        if (chk) hideBtn.interactable = false;
        if (_skillClassName != null)
        {

            var path = $"Textures/ShopIcon/{_skillClassName.Name}";
            Image_Icon.sprite = Resources.Load<Sprite>(path);
            Text_SkillName.text = skillClassName.Price;
            Text_CharacterName.text = skillClassName.Name;
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

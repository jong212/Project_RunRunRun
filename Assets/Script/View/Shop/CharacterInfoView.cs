using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInfoView : MonoBehaviour
{
    [SerializeField] Text Text_Name;
    [SerializeField] Text Text_Description;
    [SerializeField] GameObject Transform_SlotRoot;
    [SerializeField] GameObject Prefab_SkillSlot;

    private List<Shop> _allCharacters;
    public void Start()
    {
        //SetCharacterInfo();
        LoadAllCharacterData();
    }
    private void LoadAllCharacterData()
    {
        _allCharacters = LobbyDataManager.Inst.GetAllCharacterData();
        if (_allCharacters == null || _allCharacters.Count == 0)
        {
            return;
        }
        foreach (var character in _allCharacters)
        {
            // 여기서 각 캐릭터에 대해 필요한 초기화 작업을 수행
            //Debug.Log($"Initializing character: {character.Name}");

            // 각 캐릭터의 스킬 UI를 설정
            var gObj = Instantiate(Prefab_SkillSlot, Transform_SlotRoot.transform);
            var skillSlot = gObj.GetComponent<ShopSloatView>();
            if (skillSlot == null)
                continue;

            skillSlot.SetUI(character);
        }

    }
}

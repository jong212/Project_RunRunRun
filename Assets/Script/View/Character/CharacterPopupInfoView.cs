using System.Collections;
using System.Collections.Generic;
using tkitfacn.UI;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPopupInfoView : MonoBehaviour
{
    [SerializeField] Text Text_Name;
    [SerializeField] Text Text_Description;
    [SerializeField] GameObject Transform_SlotRoot;
    [SerializeField] GameObject Prefab_SkillSlot;
    [SerializeField] CardSlider cardSlider;
    [SerializeField] NetworkManager networkManager;

    private List<Shop> _allCharacters;

    private void Awake()
    {
        var networkManagerObject = GameObject.FindWithTag("NetManager");
        if (networkManagerObject != null)
        {
            networkManager = networkManagerObject.GetComponent<NetworkManager>();
        }

        if (networkManager == null)
        {
            Debug.LogError("NetworkManager not found in the scene.");
            return;
        }
    }
    private void OnEnable()
    {
        LoadAllCharacterData();
    }
    private void OnDisable()
    {
        foreach (Transform child in Transform_SlotRoot.transform)
        {
            Destroy(child.gameObject);
        }
    }
    private void LoadAllCharacterData()
    {

        _allCharacters = LobbyDataManager.Inst.GetAllCharacterData();
        if (_allCharacters == null || _allCharacters.Count == 0)
        {
            return;
        }
        _allCharacters.Reverse();

        foreach (var character in _allCharacters)
        {

            bool _isBool = false;
            foreach (var mycrt in networkManager.OwnedCharacters)
            {
                if(character.Name == mycrt)
                {
                    _isBool = true;
                    break;
                }
            }
            var gObj = Instantiate(Prefab_SkillSlot, Transform_SlotRoot.transform);
            var skillSlot = gObj.GetComponent<CharacterSloatView>();
            if (skillSlot == null)
                continue;

            skillSlot.SetUI(character,_isBool);
        }
        if (cardSlider != null)
        {
            cardSlider.Sort();
        }
    }
}

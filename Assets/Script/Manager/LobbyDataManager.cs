using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

// Xml ������ �ҷ����� // ��Ÿ�ӿ� ����
public class LobbyDataManager : MonoBehaviour
{
    public static LobbyDataManager Inst { get; private set; }

    public Dictionary<int, Shop> LoadedCharacterList { get; private set; }


    private readonly string _dataRootPath = "C:\\Users\\jongh\\OneDrive\\���� ȭ��\\DataParser";

    private void Awake()
    {
        Inst = this;
        //ReadAllDataOnAwake();
    }

    public void ReadAllDataOnAwake()
    {
        ReadData(nameof(Shop)); // == ReadData("Character")
    }

    private void ReadData(string tableName)
    {
        // �� �κ��� ����� ������ �� ����
        switch (tableName)
        {
            case "Shop":
                ReadCharacterTable(tableName);
                break;
        }
    }

    private void ReadCharacterTable(string tableName)
    {
        LoadedCharacterList = new Dictionary<int, Shop>();

        XDocument doc = XDocument.Load($"{_dataRootPath}/{tableName}.xml");
        var dataElements = doc.Descendants("data");

        foreach (var data in dataElements)
        {
            var tempCharacter = new Shop();
            tempCharacter.DataId = int.Parse(data.Attribute(nameof(tempCharacter.DataId)).Value);
            tempCharacter.Name = data.Attribute(nameof(tempCharacter.Name)).Value;
            tempCharacter.Description = data.Attribute(nameof(tempCharacter.Description)).Value;
            tempCharacter.Price = data.Attribute(nameof(tempCharacter.Price)).Value;
            //tempCharacter.PrefabName = data.Attribute(nameof(tempCharacter.PrefabName)).Value;

            /*
                        string skillNameListStr = data.Attribute("SkillNameList").Value;
                        if (!string.IsNullOrEmpty(skillNameListStr))
                        {
                            skillNameListStr = skillNameListStr.Replace("{", string.Empty);
                            skillNameListStr = skillNameListStr.Replace("}", string.Empty);

                            var skillNames = skillNameListStr.Split(',');

                            var list = new List<string>();
                            if (skillNames.Length > 0)
                            {
                                foreach (var name in skillNames)
                                {
                                    list.Add(name);
                                }
                            }
                            tempCharacter.SkillClassNameList = list;
                        }*/

            LoadedCharacterList.Add(tempCharacter.DataId, tempCharacter);
        }

    }
    /*private void ReadSkillTable(string tableName)
    {
        LoadedSkillList = new Dictionary<string, Skill>();

        XDocument doc = XDocument.Load($"{_dataRootPath}/{tableName}.xml");
        var dataElements = doc.Descendants("data");

        foreach (var data in dataElements)
        {
            var tempSkill = new Skill();
            tempSkill.SkillClassName = data.Attribute("DataName").Value;
            tempSkill.Name = data.Attribute(nameof(tempSkill.Name)).Value;
            tempSkill.Description = data.Attribute(nameof(tempSkill.Description)).Value;
            tempSkill.BaseDamage = int.Parse(data.Attribute(nameof(tempSkill.BaseDamage)).Value);
            tempSkill.DamageMultiSkillLevelName = float.Parse(data.Attribute(nameof(tempSkill.DamageMultiSkillLevelName)).Value);
            tempSkill.IconName = data.Attribute(nameof(tempSkill.IconName)).Value;

            string skillNameListStr = data.Attribute(nameof(tempSkill.BuffNameList)).Value;
            if (!string.IsNullOrEmpty(skillNameListStr))
            {
                skillNameListStr = skillNameListStr.Replace("{", string.Empty);
                skillNameListStr = skillNameListStr.Replace("}", string.Empty);

                var names = skillNameListStr.Split(',');

                var list = new List<string>();
                if (names.Length > 0)
                {
                    foreach (var name in names)
                    {
                        list.Add(name);
                    }
                }
                tempSkill.BuffNameList = list;

            }
            LoadedSkillList.Add(tempSkill.SkillClassName, tempSkill);
        }
    }

    private void ReadBuffTable(string tableName)
    {
        LoadedBuffList = new Dictionary<string, Buff>();

        XDocument doc = XDocument.Load($"{_dataRootPath}/{tableName}.xml");
        var dataElements = doc.Descendants("data");

        foreach (var data in dataElements)
        {
            var tempBuff = new Buff();
            tempBuff.BuffClassName = data.Attribute("DataName").Value;
            tempBuff.Name = data.Attribute(nameof(tempBuff.Name)).Value;
            tempBuff.Description = data.Attribute(nameof(tempBuff.Description)).Value;
            tempBuff.BuffTime = int.Parse(data.Attribute(nameof(tempBuff.BuffTime)).Value);

            string buffValuesStr = data.Attribute(nameof(tempBuff.BuffValues)).Value;
            if (!string.IsNullOrEmpty(buffValuesStr))
            {
                buffValuesStr = buffValuesStr.Replace("{", string.Empty);
                buffValuesStr = buffValuesStr.Replace("}", string.Empty);

                var values = buffValuesStr.Split(',');

                var list = new List<float>();
                if (values.Length > 0)
                {
                    foreach (var buffValue in values)
                    {
                        list.Add(float.Parse(buffValue));
                    }
                }
                tempBuff.BuffValues = list;
            }
            LoadedBuffList.Add(tempBuff.BuffClassName, tempBuff);
        }
    }*/
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop
{
    public int DataId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string IconPath { get; set; }
    public string PrefabPath { get; set; }

    public List<string> SkillClassNameList { get; set; }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop
{
    public int DataId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string IconPath { get; set; }
    public string Price { get; set; }
    public string PrefabName { get; set; }

    public List<string> SkillClassNameList { get; set; }
}

/*public class Skill
{
    public string SkillClassName { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int BaseDamage { get; set; }
    public float DamageMultiSkillLevelName { get; set; }

    public string IconName { get; set; }

    public List<string> BuffNameList { get; set; }
}

public class Buff
{
    public string BuffClassName { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int BuffTime { get; set; }
    public List<float> BuffValues { get; set; }
}*/
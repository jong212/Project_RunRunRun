using System.Collections.Generic;
using System.Linq;

public static class DataManigingExtensions
{
    public static List<Shop> GetAllCharacterData(this LobbyDataManager manager)
    {
        return manager.LoadedCharacterList.Values.ToList();
    }

    public static Shop GetCharacterData(this LobbyDataManager manager, int dataId)
    {
        var loadedCharacterList = manager.LoadedCharacterList;
        if (loadedCharacterList.Count == 0
            || loadedCharacterList.ContainsKey(dataId) == false)
        {
            return null;
        }
        return loadedCharacterList[dataId];
    }

 
    /*  
    public static Buff GetBuffData(this LobbyDatamanager manager, string dataClassName)
    {
        var loadedBuffList = manager.LoadedBuffList;
        if (loadedBuffList.Count == 0
           || loadedBuffList.ContainsKey(dataClassName) == false)
        {
            return null;
        }

        return loadedBuffList[dataClassName];
    }*/

    /*public static string GetSkillName(this LobbyDatamanager manager, string dataClassName)
    {
        var skillData = manager.GetSkillData(dataClassName);
        return (skillData != null) ? skillData.Name : string.Empty;
    }

    public static string GetBuffDescription(this LobbyDatamanager manager, string dataClassName)
    {
        var buffData = manager.GetBuffData(dataClassName);
        string desc = string.Empty;
        if (buffData != null)
        {
            desc = string.Format(buffData.Description, buffData.BuffValues);
        }
        return desc;
    }*/
}

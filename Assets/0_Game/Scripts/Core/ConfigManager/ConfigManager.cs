using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConfigManager : SingletonMono<ConfigManager>
{
    [SerializeField] private List<ConfigCollection> _configs;

    public T GetConfig<T>() where T : ConfigCollection
    {
        var configGet = _configs.FirstOrDefault(config => config.ID == typeof(T).ToString());
        if (configGet == null)
        {
            this.Log("NO CONFIG FOUND");
        }
        return configGet as T;
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        LoadGameConfigs();
        CheckDuplicatedConfig();
    }

    private void LoadGameConfigs()
    {
        _configs = new();

        string[] scriptGuids = UnityEditor.AssetDatabase.FindAssets("t:ConfigCollection", new[] { "Assets" });
        foreach (string guid in scriptGuids)
        {
            string scriptPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            ConfigCollection configCollection = UnityEditor.AssetDatabase.LoadAssetAtPath<ConfigCollection>(scriptPath);
            _configs.Add(configCollection);
        }
    }

    private void CheckDuplicatedConfig()
    {
        Dictionary<string, bool> idDictionary = new Dictionary<string, bool>();
        List<string> duplicateIds = new List<string>();

        foreach (ConfigCollection config in _configs)
        {
            if (idDictionary.ContainsKey(config.ID))
            {
                if (!duplicateIds.Contains(config.ID))
                {
                    duplicateIds.Add(config.ID);
                }
            }
            else
            {
                idDictionary[config.ID] = true;
            }
        }

        if (duplicateIds.Count > 0)
        {
            this.LogError("Duplicate config found: " + string.Join(", ", duplicateIds));
        }
    }

#endif
}
using System.Collections.Generic;
using System.Linq;
using IsoTools;
using NFramework;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace YoyoDesign
{
    public class FurnitureManager : SingletonMono<FurnitureManager>
    {
        [SerializeField] private IsoWorld _furContainer;
        [SerializeField] private List<FurnitureConfigSO> _furConfigList;

        private List<FurnitureController> _furPool = new();
        private Dictionary<ETrashType, List<FurnitureConfigSO>> _trashConfigDic;

        public List<FurnitureConfigSO> FurConfigList => _furConfigList;
        public Dictionary<ETrashType, List<FurnitureConfigSO>> TrashConfigDic => _trashConfigDic;

        public FurnitureController Get(string furId)
        {
            var furGet = _furPool.FirstOrDefault(f => f.Config.Id == furId);

            if (furGet == null)
            {
                var configGet = GetFurnitureConfig(furId);
                furGet = Instantiate(configGet.Config.Prefab, _furContainer.transform);
                _furPool.Add(furGet);
            }

            _furPool.Remove(furGet);
            return furGet;
        }

        public void Release(FurnitureController furRelease)
        {
            furRelease.VisualController.OnRelease();
            furRelease.NestedController.OnRelease();
            furRelease.gameObject.SetActive(false);
            furRelease.transform.SetParent(_furContainer.transform);
            _furPool.Add(furRelease);
        }

        public FurnitureConfigSO GetRandomTrash(ETrashType type)
        {
            if (_trashConfigDic == null)
            {
                _trashConfigDic = new Dictionary<ETrashType, List<FurnitureConfigSO>>();
                foreach (var data in _furConfigList)
                {
                    var trashType = data.Config.TrashType;

                    if (trashType == ETrashType.None)
                        continue;

                    if (!_trashConfigDic.ContainsKey(trashType))
                    {
                        _trashConfigDic.Add(trashType, new List<FurnitureConfigSO>() { data });
                    }
                    else
                    {
                        _trashConfigDic[trashType].Add(data);
                    }
                }
            }

            return _trashConfigDic[type].RandomItem();
        }

        public FurnitureConfigSO GetFurnitureConfig(string furId)
        {
            var config = _furConfigList.FirstOrDefault(x => x.Config.Id == furId);
            if (config == null)
            {
                Logger.Log(this, $"Cannot find furniture config with id [{furId}]");
            }
            return config;
        }

#if UNITY_EDITOR

        [Button]
        public void OnValidate()
        {
            _furConfigList.Clear();
            string[] guids = AssetDatabase.FindAssets("t:FurnitureConfigSO");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                FurnitureConfigSO furnitureConfig = AssetDatabase.LoadAssetAtPath<FurnitureConfigSO>(path);
                if (furnitureConfig != null)
                {
                    _furConfigList.Add(furnitureConfig);
                }
            }
        }

#endif
    }
}
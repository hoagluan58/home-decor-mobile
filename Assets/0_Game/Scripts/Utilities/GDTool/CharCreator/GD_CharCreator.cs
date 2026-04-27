using Redcode.Extensions;
using Sirenix.OdinInspector;
using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YoyoDesign
{
    public class GD_CharCreator : MonoBehaviour
    {
        [SerializeField] private BotConfigSO _botConfigSO;
        [SerializeField] private OutfitConfigSO _config;
        [SerializeField] private List<CharacterOutfitData> _characterOutfitDatas;
        [SerializeField] private SkeletonGraphic _spineGraphic;
        [SerializeField] private Skin _combinedSkin = new Skin("combinedSkins");

        private void Start()
        {
            _botConfigSO.Init();
            _config.Init();
            StartCoroutine(CRChangeSkin());
        }

        private IEnumerator CRChangeSkin()
        {
            while (true)
            {
                var pair = _botConfigSO.BotConfigDic.GetRandomElement();
                _combinedSkin.Clear();
                pair.Value.OutfitDatas.ForEach(x =>
                {
                    if (_config.OutfitConfigDataDic.TryGetValue(x.OutfitId, out var outfitConfig))
                    {
                        var skinName = outfitConfig.NameSkin;
                        var skin = _spineGraphic.Skeleton.Data.FindSkin(skinName);
                        if (skin != null)
                        {
                            _combinedSkin.AddSkin(skin);
                        }
                    }
                });

                if (_spineGraphic != null)
                {
                    _spineGraphic.Skeleton.SetSkin(_combinedSkin);
                    _spineGraphic.Skeleton.SetSlotsToSetupPose();
                }

                yield return new WaitForSeconds(5f);
            }
        }
    }
}

using Spine;
using Spine.Unity;
using System.Collections.Generic;

namespace YoyoDesign
{
    public static class CharacterHelper
    {
        public static void ApplyCharacterOutfit(this SkeletonGraphic characterSkeletonGraphic, List<DressUpData.CharacterOutfitSaveData> outfitDatas)
        {
            Skin combinedSkin = new("combinedSkins");
            combinedSkin.Clear();

            outfitDatas.ForEach(x =>
            {
                var outfitConfigData = DressUpManager.Instance.GetDressUpConfigData(x.Id);
                if (outfitConfigData == null) return;
                var skinName = outfitConfigData.NameSkin;
                var skin = characterSkeletonGraphic.Skeleton.Data.FindSkin(skinName);
                if (skin != null) combinedSkin.AddSkin(skin);
            });

            if (characterSkeletonGraphic != null)
            {
                characterSkeletonGraphic.Skeleton.SetSkin(combinedSkin);
                characterSkeletonGraphic.Skeleton.SetSlotsToSetupPose();
            }
        }

        public static void ApplyCharacterOutfit(this SkeletonGraphic characterSkeletonGraphic, List<CharacterOutfitData> outfitDatas)
        {
            Skin combinedSkin = new("combinedSkins");
            combinedSkin.Clear();

            outfitDatas.ForEach(x =>
            {
                if (AllConfig.Instance.OutfitConfigDic.TryGetValue(x.OutfitId, out var outfitConfigData))
                {
                    var skinName = outfitConfigData.NameSkin;
                    var skin = characterSkeletonGraphic.Skeleton.Data.FindSkin(skinName);
                    if (skin != null) 
                        combinedSkin.AddSkin(skin);
                };
            });

            if (characterSkeletonGraphic != null)
            {
                characterSkeletonGraphic.Skeleton.SetSkin(combinedSkin);
                characterSkeletonGraphic.Skeleton.SetSlotsToSetupPose();
            }
        }
        
        public static void ApplyCharacterOutfit(this SkeletonAnimation characterSkeletonGraphic, List<DressUpData.CharacterOutfitSaveData> outfitDatas)
        {
            Skin combinedSkin = new("combinedSkins");
            combinedSkin.Clear();

            outfitDatas.ForEach(x =>
            {
                var outfitConfigData = DressUpManager.Instance.GetDressUpConfigData(x.Id);
                if (outfitConfigData == null) return;
                var skinName = outfitConfigData.NameSkin;
                var skin = characterSkeletonGraphic.Skeleton.Data.FindSkin(skinName);
                if (skin != null) combinedSkin.AddSkin(skin);
            });

            if (characterSkeletonGraphic != null)
            {
                characterSkeletonGraphic.Skeleton.SetSkin(combinedSkin);
                characterSkeletonGraphic.Skeleton.SetSlotsToSetupPose();
            }
        }
    }
}
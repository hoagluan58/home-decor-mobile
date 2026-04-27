using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using Spine;
using Spine.Unity;
using UnityEngine;
using static YoyoDesign.DressUpData;

namespace YoyoDesign
{
    public class CharacterCellView : EnhancedScrollerCellView
    {
        #region PARAM

        [SerializeField] private SkeletonGraphic _characterSkeleton;
        [SerializeField] private Skin combinedSkin = new Skin("combinedSkins");

        #endregion

        #region UNITY METHODS

        #endregion

        #region FEATURE METHODS

        public void SetData()
        {
            
        }

        private void ApplyCharacterOutfit(List<CharacterOutfitSaveData> outfitSaveDatas)
        {
            combinedSkin.Clear();

            outfitSaveDatas.ForEach(x =>
            {
                var skinName = DressUpManager.Instance.GetDressUpConfigData(x.Id).NameSkin;
                Skin skin = _characterSkeleton.Skeleton.Data.FindSkin(skinName);
                if (skin != null) combinedSkin.AddSkin(skin);
            });

            Skin leg = _characterSkeleton.Skeleton.Data.FindSkin("leg/leg");
            combinedSkin.AddSkin(leg);

            if (_characterSkeleton != null)
            {
                _characterSkeleton.Skeleton.SetSkin(combinedSkin);
                _characterSkeleton.Skeleton.SetSlotsToSetupPose();
            }

            //HighLight Current outfit
            OutfitItemUI.CheckEquipEvent?.Invoke();
        }

        #endregion
    }
}

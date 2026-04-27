using DG.Tweening;
using NFramework;
using Spine;
using Spine.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YoyoDesign
{
    /// <summary>
    /// Controls all the character's spine parts.
    /// </summary>
    public class CharacterOutfitControllerUI : MonoBehaviour
    {
        #region PARAM 

        [SerializeField] private List<CharacterOutfitData> _characterOutfitDatas = new();
        [SerializeField] private List<string> _dressUpAnim = new();

        public List<CharacterOutfitData> CurrentOutfitData => _characterOutfitDatas;

        [SerializeField] private SkeletonGraphic _spineGraphic;
        [SerializeField] private Skin _combinedSkin = new Skin("combinedSkins");

        Tween _animDressUpTween;
        #endregion

        #region FEATURE METHODS
        public void LoadCurrentCharacter()
        {
            CharacterConfigData characterConfigData = CharacterManager.Instance
                .GetCharacterData(DressUpData.Instance.CurCharacterId);
            _spineGraphic.skeletonDataAsset = characterConfigData.SkeletonDataAsset;
            _spineGraphic.Initialize(true);

            LoadCurrentOutfit(DressUpData.Instance.GetCharacterOutfitSaveDatas());
        }

        /// <summary>
        /// Load outfit Saved
        /// </summary>
        /// <param name="saveDatas"></param> Data from dressUpData
        public void LoadCurrentOutfit(List<DressUpData.CharacterOutfitSaveData> saveDatas)
        {
            foreach(var saveData in saveDatas)
            {
                var curData = _characterOutfitDatas.FirstOrDefault(d => d.OutfitType == saveData.OutfitType);
                if(curData == null)
                {
                    curData = new CharacterOutfitData(saveData.OutfitType, saveData.Id);
                    _characterOutfitDatas.Add(curData);
                }
                else
                {
                    curData.OutfitId = saveData.Id;
                }
            }
            ApplyOutfitSkin();
        }

        public void ApplyOutfit(OutfitType outfitType, string outfitId)
        {
            CharacterOutfitData curDataSkirt;
            CharacterOutfitData curDataShirt;
            CharacterOutfitData curDataDress;
            switch(outfitType)
            {
                case OutfitType.Dress:

                    if(outfitId == "dressUp_dress_00") return;
                    //Dress
                    var DataDress = _characterOutfitDatas.FirstOrDefault(d => d.OutfitType == outfitType);
                    if(DataDress == null)
                    {
                        _characterOutfitDatas.Add(new CharacterOutfitData(outfitType, outfitId));
                    }
                    else
                    {
                        DataDress.OutfitId = outfitId;
                    }
                    //Skirt
                    curDataSkirt = _characterOutfitDatas.FirstOrDefault(d => d.OutfitType == OutfitType.Skirt);
                    if(curDataSkirt == null)
                    {
                        _characterOutfitDatas.Add(new CharacterOutfitData(outfitType, "dressUp_skirt_00"));
                    }
                    else
                    {
                        curDataSkirt.OutfitId = "dressUp_skirt_00";
                    }
                    //Shirt
                    curDataShirt = _characterOutfitDatas.FirstOrDefault(d => d.OutfitType == OutfitType.Shirt);
                    if(curDataShirt == null)
                    {
                        _characterOutfitDatas.Add(new CharacterOutfitData(outfitType, "dressUp_shirt_00"));
                    }
                    else
                    {
                        curDataShirt.OutfitId = "dressUp_shirt_00";
                    }
                    break;
                case OutfitType.Skirt:
                    //Dress
                    Debug.Log("skirt");

                    if(outfitId == "dressUp_skirt_00") return;

                    curDataDress = _characterOutfitDatas.FirstOrDefault(d => d.OutfitType == OutfitType.Dress);
                    if(curDataDress == null)
                    {
                        _characterOutfitDatas.Add(new CharacterOutfitData(outfitType, "dressUp_dress_00"));
                    }
                    else
                    {
                        curDataDress.OutfitId = "dressUp_dress_00";
                    }
                    //Shirt
                    curDataShirt = _characterOutfitDatas.FirstOrDefault(d => d.OutfitType == OutfitType.Shirt);
                    if(curDataShirt == null)
                    {
                        _characterOutfitDatas.Add(new CharacterOutfitData(outfitType, "dressUp_shirt_01"));
                    }
                    else
                    {
                        if(curDataShirt.OutfitId == "dressUp_shirt_00")
                            curDataShirt.OutfitId = "dressUp_shirt_01";
                    }
                    //Skirt
                    var DataSkirt = _characterOutfitDatas.FirstOrDefault(d => d.OutfitType == outfitType);
                    if(DataSkirt == null)
                    {
                        _characterOutfitDatas.Add(new CharacterOutfitData(outfitType, outfitId));
                    }
                    else
                    {
                        DataSkirt.OutfitId = outfitId;
                    }

                    break;
                case OutfitType.Shirt:
                    //Dress
                    Debug.Log("Shirt");
                    if(outfitId == "dressUp_shirt_00") return;

                    curDataDress = _characterOutfitDatas.FirstOrDefault(d => d.OutfitType == OutfitType.Dress);
                    if(curDataDress == null)
                    {
                        _characterOutfitDatas.Add(new CharacterOutfitData(outfitType, "dressUp_dress_00"));
                    }
                    else
                    {
                        curDataDress.OutfitId = "dressUp_dress_00";
                    }
                    //Skirt
                    curDataSkirt = _characterOutfitDatas.FirstOrDefault(d => d.OutfitType == OutfitType.Skirt);
                    if(curDataSkirt == null)
                    {
                        _characterOutfitDatas.Add(new CharacterOutfitData(outfitType, "dressUp_skirt_01"));
                    }
                    else
                    {
                        if(curDataSkirt.OutfitId == "dressUp_skirt_00")
                            curDataSkirt.OutfitId = "dressUp_skirt_01";
                    }
                    //Shirt
                    var DataShirt = _characterOutfitDatas.FirstOrDefault(d => d.OutfitType == outfitType);
                    if(DataShirt == null)
                    {
                        _characterOutfitDatas.Add(new CharacterOutfitData(outfitType, outfitId));
                    }
                    else
                    {
                        DataShirt.OutfitId = outfitId;
                    }

                    break;
                default:

                    var curData = _characterOutfitDatas.FirstOrDefault(d => d.OutfitType == outfitType);
                    if(curData == null)
                    {
                        _characterOutfitDatas.Add(new CharacterOutfitData(outfitType, outfitId));
                    }
                    else
                    {
                        curData.OutfitId = outfitId;
                    }

                    break;
            }

            ApplyOutfitSkin();
        }

        private void ApplyOutfitSkin()
        {
            _combinedSkin.Clear();
            _characterOutfitDatas.ForEach(
                x =>
                {
                    OutfitConfigData outfitConfigData = DressUpManager.Instance.GetDressUpConfigData(x.OutfitId);
                    if(outfitConfigData != null)
                    {
                    var skinName = DressUpManager.Instance.GetDressUpConfigData(x.OutfitId).NameSkin;
                    Skin skin = _spineGraphic.Skeleton.Data.FindSkin(skinName);
                    if(skin != null)
                        _combinedSkin.AddSkin(skin);
                    }
                });

            if(_spineGraphic != null)
            {
                _spineGraphic.Skeleton.SetSkin(_combinedSkin);
                _spineGraphic.Skeleton.SetSlotsToSetupPose();
            }

            OutfitItemUI.CheckEquipEvent?.Invoke();
        }

        public void PlayAnimEquipOutfit()
        {
            string animName = _dressUpAnim.RandomItem<string>();
            _spineGraphic.AnimationState.SetAnimation(0, animName, false);
            float timeAnim = _spineGraphic.skeletonDataAsset.GetSkeletonData(true).FindAnimation(animName).Duration;

            _animDressUpTween.Kill();
            _animDressUpTween = DOVirtual.DelayedCall(timeAnim, () => _spineGraphic.AnimationState.SetAnimation(0, "idle", true));
        }

        #endregion
    }

    [Serializable]
    public class CharacterOutfitData
    {
        public CharacterOutfitData() { }
        public CharacterOutfitData(OutfitType outfitType, string outfitId)
        {
            OutfitType = outfitType;
            OutfitId = outfitId;
        }

        public OutfitType OutfitType;
        public string OutfitId;
    }
}
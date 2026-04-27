using JSAM;
using Spine;
using Spine.Unity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class CharacterItem : MonoBehaviour
    {
        #region Param

        [Header("UI")]
        [SerializeField] private Button _button;
        [SerializeField] private Image _image;
        [SerializeField] private Image _imageIconCharacter;
        [SerializeField] private Sprite _spriteSelect;
        [SerializeField] private Sprite _spriteDeSelect;
        [SerializeField] private Image _imageTick;
        [Header("CHARACTER SPINE")]
        [SerializeField] private SkeletonGraphic _characterSkeletonGraphic;
        [SerializeField] private Skin _combinedSkin = new Skin("combinedSkins");
        [SerializeField] private SkeletonDataAsset _characterDataAsset;

        private CharacterConfigData _characterConfigData;
        public CharacterConfigData CharacterConfigData => _characterConfigData;

        #endregion

        #region Unity Method

        private void OnEnable()
        {
            DressUpData.EventCharacterID.AddListener(CheckSelectItem);
            _button.onClick.AddListener(OnClick);
        }
        private void OnDisable()
        {
            DressUpData.EventCharacterID.RemoveListener(CheckSelectItem);
            _button.onClick.RemoveListener(OnClick);
        }

        #endregion

        #region Feature Method

        public void Init(CharacterConfigData characterConfigData, SkeletonGraphic skeletonGraphic)
        {
            _characterConfigData = characterConfigData;
            _characterSkeletonGraphic = skeletonGraphic;
            _characterDataAsset = _characterConfigData.SkeletonDataAsset;
            _imageIconCharacter.sprite = _characterConfigData.Icon;

            AllConfig.Instance.SetOutfitConfigSo = CharacterConfigData.OutfitConfigSo;

            DeSelect();
            if (DressUpData.Instance.CurCharacterId == characterConfigData.Id)
            {
                DressUpData.Instance.SaveCharacter(_characterConfigData.Id);
                DressUpData.Instance.SaveOutfit(_characterConfigData.OutfitDefault);
            }
        }
        private void ApplyCharacterOutfit(List<CharacterOutfitData> outfitDatas)
        {
            _combinedSkin.Clear();

            outfitDatas.ForEach(x =>
            {
                var skinName = DressUpManager.Instance.GetDressUpConfigData(x.OutfitId).NameSkin;
                Skin skin = _characterSkeletonGraphic.Skeleton.Data.FindSkin(skinName);
                if(skin != null) _combinedSkin.AddSkin(skin);
            });

            if(_characterSkeletonGraphic != null)
            {
                _characterSkeletonGraphic.Skeleton.SetSkin(_combinedSkin);
                _characterSkeletonGraphic.Skeleton.SetSlotsToSetupPose();
            }
        }
        public void CheckSelectItem(object data)
        {
            string CurCharId = (string)data;
            if(CharacterConfigData.Id == CurCharId)
            {
                Select();
            }
            else
            {
                DeSelect();
            }
        }
        private void Select()
        {
            _image.sprite = _spriteSelect;
            _imageTick.gameObject.SetActive(true);

            //_characterSkeletonGraphic.skeletonDataAsset = _characterDataAsset;
            //_characterSkeletonGraphic.Initialize(true);

            //Apply Current Character Skin
            //ApplyCharacterOutfit(DressUpData.Instance.GetCharacterOutfitSaveDatas());
            ApplyCharacterOutfit(_characterConfigData.OutfitDefault);

        }
        private void DeSelect()
        {
            _image.sprite = _spriteDeSelect;
            _imageTick.gameObject.SetActive(false);
        }

        #endregion

        #region OnClick Methods

        public void OnClick()
        {
            AudioManager.PlaySound(ESound.Click);
            OutfitConfigSO outfitConfigSo = CharacterConfigData.OutfitConfigSo;
            AllConfig.Instance.SetOutfitConfigSo = outfitConfigSo;
            DressUpData.Instance.SaveCharacter(_characterConfigData.Id);
            DressUpData.Instance.SaveOutfit(_characterConfigData.OutfitDefault);
        }

        #endregion
    }
}

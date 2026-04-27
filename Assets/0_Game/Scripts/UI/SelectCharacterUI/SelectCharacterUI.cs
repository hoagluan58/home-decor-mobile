using JSAM;
using NFramework;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class SelectCharacterUI : BaseUIView
    {
        #region PARAM

        [Header("ITEMS UI")]
        [SerializeField] private CharacterItem _characterItemPrefab;
        [SerializeField] private Transform _tfParentLayout;

        [Header("CHARACTER SPINE")]
        [SerializeField] private SkeletonGraphic _characterSkeletonGraphic;

        [Header("SCOLL VIEW")]
        [SerializeField] private CharacterControllView _characterControllView;

        [Header("UI")]
        [SerializeField] private Button _buttonNext;
        [SerializeField] private Button _buttonSettting;

        private Button _buttonTutorial;

        #endregion

        #region UNITY METHODS

        private void Awake()
        {
            SpawnCharacterItem();
        }

        private void OnEnable()
        {
            _buttonNext.onClick.AddListener(OnBtnNext);
            _buttonSettting.onClick.AddListener(OnBtnSetting);
        }

        private void OnDisable()
        {
            _buttonNext.onClick.RemoveListener(OnBtnNext);
            _buttonSettting.onClick.RemoveListener(OnBtnSetting);
        }

        #endregion

        #region FEATURE METHODS

        private void SpawnCharacterItem()
        {
            for (int i = 0; i < CharacterManager.Instance.GetCharacterConfigDatas().Count; i++)
            {
                CharacterItem characterItem = Instantiate(_characterItemPrefab.transform, _tfParentLayout).GetComponent<CharacterItem>();
                characterItem.Init(CharacterManager.Instance.GetCharacterConfigDatas()[i], _characterSkeletonGraphic);
            }
        }

        #endregion

        #region ONCLICK METHODS

        private void OnBtnNext()
        {
            AudioManager.PlaySound(ESound.Click);
            foreach (var item in DressUpData.Instance.GetCharacterOutfitSaveDatas())
            {
                DressUpData.Instance.UnlockOutfit(item.Id);
            }
            FeatureNavigator.Instance.Go(EGameFeature.Home).Forget();
        }

        private void OnBtnSetting()
        {
            AudioManager.PlaySound(ESound.Click);
            UIManager.Instance.Open(Define.UIName.SETTINGS_POPUP);
        }

        #endregion
    }
}
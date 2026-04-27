using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using JSAM;
using NFramework;
using Sirenix.OdinInspector;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class TutorialDecorMenuUI : BaseUIView
    {
        [Header("UI")]
        [SerializeField] private ScrollRect _furItemScroll;
        [SerializeField] private RectTransform _categoryButtonContainer;
        [SerializeField] private RectTransform _subCategoryContainer;

        [Header("CONFIG")]
        [SerializeField] private FurnitureCategoryConfigSO _categoryConfig;

        [Header("PREFAB")]
        [SerializeField] private FurnitureItemUI _furnitureItemPrefab;
        [SerializeField] private CategoryButtonUI _categoryButtonPrefab;
        [SerializeField] private SubCategoryButtonUI _subCategoryBtnPrefab;

        [Header("FAKE BUTTONS")]
        [SerializeField] private GameObject _fakeFurnitureCategoryButton;
        [SerializeField] private GameObject _fakeBedButton;
        [SerializeField] private GameObject _fakeChairSubCategoryButton;
        [SerializeField] private GameObject _fakeChairButton;

        [Header("DIALOG")]
        [SerializeField] private GameObject _dialogPanel;
        [SerializeField] private Button _nextDialogButton;
        [SerializeField] private TextMeshProUGUI _dialogTMP;
        [Header("CHARACTER")]
        [SerializeField] private SkeletonGraphic _characterSkeletonGraphic;
        private Skin _combinedSkin = new("combinedSkins");

        private int _currentDialogIndex;

        private readonly List<FurnitureItemUI> _displayItems = new();

        private readonly List<CategoryButtonUI> _categoryButtons = new();
        private readonly List<SubCategoryButtonUI> _displayCategoryButton = new();

        private readonly List<SubCategoryButtonUI> _subCategoryButtons = new();
        private readonly List<SubCategoryButtonUI> _displaySubCategoryButton = new();

        private EFurnitureCategory _curCategory;
        private EFurnitureSubCategory _curSubCategory;

        private ObjectPool<FurnitureItemUI> _furItemPool;

        private List<FurnitureConfigSO> _furConfigList => FurnitureManager.Instance.FurConfigList;

        #region BASE

        public void Start()
        {
            GetComponent<Canvas>().worldCamera = Camera.main;

            // Init pool.
            _furItemPool = new(
                () => Instantiate(_furnitureItemPrefab, _furItemScroll.content),
                item => item.gameObject.SetActive(true),
                item => item.gameObject.SetActive(false),
                item => Destroy(item.gameObject));

            // Generate main category buttons.
            foreach (var categoryConfig in _categoryConfig.Config)
            {
                var mainCateBtn = Instantiate(_categoryButtonPrefab, _categoryButtonContainer);
                mainCateBtn.OnCreate(
                    categoryConfig.MainCategory,
                    categoryConfig.CategoryIcon,
                    categoryConfig.SelectCategoryIcon,
                    categoryConfig.MainCategoryName);
                _categoryButtons.Add(mainCateBtn);
                mainCateBtn.SetInteractable(false);

                // Generate sub category buttons.
                foreach (var subCategoryCfg in categoryConfig.SubCategories)
                {
                    var subCateBtn = Instantiate(_subCategoryBtnPrefab, _subCategoryContainer);
                    subCateBtn.OnCreate(categoryConfig.MainCategory, subCategoryCfg.SubCategory,
                        subCategoryCfg.IconSprite);
                    _subCategoryButtons.Add(subCateBtn);
                    subCateBtn.gameObject.SetActive(false);
                }
            }
        }

        [Button]
        public override async void OnOpen()
        {
            base.OnOpen();
            await UniTask.Yield();
            ApplyCharacterOutfit();
            ChangeCategory(EFurnitureCategory.Basic);
        }

        public void ApplyCharacterOutfit()
        {
            var outfitDatas = DressUpData.Instance.GetCharacterOutfitSaveDatas();
            _combinedSkin.Clear();

            outfitDatas.ForEach(x =>
            {
                var outfitConfigData = DressUpManager.Instance.GetDressUpConfigData(x.Id);
                if (outfitConfigData == null) return;
                var skinName = outfitConfigData.NameSkin;
                var skin = _characterSkeletonGraphic.Skeleton.Data.FindSkin(skinName);
                if (skin != null) _combinedSkin.AddSkin(skin);
            });

            if (_characterSkeletonGraphic != null)
            {
                _characterSkeletonGraphic.Skeleton.SetSkin(_combinedSkin);
                _characterSkeletonGraphic.Skeleton.SetSlotsToSetupPose();
            }
        }

        private void PlayDialogAnimation(string message)
        {
            _dialogTMP.DOKill();
            _dialogTMP.text = "";
            _dialogTMP.DOText(message, Define.TimeLength.DIALOG_TIME)
                .SetEase(Ease.Linear);
        }

        public void ChangeCategory(EFurnitureCategory mainCategory)
        {
            if (mainCategory == _curCategory) return;
            _curCategory = mainCategory;

            // Set highlight
            foreach (var btn in _categoryButtons)
            {
                btn.SetHighlight(btn.Category == mainCategory);
            }

            // Hide all old sub category buttons.
            foreach (var btn in _displaySubCategoryButton)
            {
                btn.gameObject.SetActive(false);
            }
            _displaySubCategoryButton.Clear();

            // Show current sub category buttons.
            foreach (var subCateBtn in _subCategoryButtons
                         .Where(subCateBtn => subCateBtn.MainCategory == mainCategory))
            {
                subCateBtn.gameObject.SetActive(true);
                _displaySubCategoryButton.Add(subCateBtn);
            }

            ChangeSubCategory(_displaySubCategoryButton[0].SubCategory);
        }

        private void ChangeSubCategory(EFurnitureSubCategory subCategory)
        {
            if (_curSubCategory == subCategory) return;
            _curSubCategory = subCategory;

            // Hide all old display items
            foreach (var item in _displayItems)
            {
                _furItemPool.Release(item);
            }
            _displayItems.Clear();

            // Set highlight button
            foreach (var btn in _displaySubCategoryButton)
            {
                btn.SetHighlight(btn.SubCategory == subCategory);
                btn.SetInteractable(false);
            }

            LoadFurnitureItem(subCategory);
        }

        private void LoadFurnitureItem(EFurnitureSubCategory subCategory)
        {
            switch (subCategory)
            {
                // Load items
                case EFurnitureSubCategory.Basic_Ground:
                {
                    foreach (var config in AllConfig.Instance.FloorConfigDic.Values)
                    {
                        if (config.IsMessy) continue;
                        if (!config.IsDecor) continue;

                        var itemShow = _furItemPool.Get();

                        itemShow.LoadConfig(config.Id,
                            config.Sprite,
                            FurnitureDirection.Left,
                            EFurnitureSubCategory.Basic_Ground,
                            config.UnlockType, config.Price);

                        var isItemUnlock = UserData.Instance.IsFurnitureUnlocked(config.Id);
                        itemShow.LoadData(isItemUnlock);

                        itemShow.OnShow();
                        itemShow.SetInteractable(false);
                        _displayItems.Add(itemShow);
                    }
                    break;
                }
                case EFurnitureSubCategory.Basic_Wall:
                {
                    foreach (var config in AllConfig.Instance.WallConfigDic.Values)
                    {
                        if (config.IsMessy) continue;
                        if (!config.IsDecor) continue;

                        var itemShow = _furItemPool.Get();

                        itemShow.LoadConfig(config.Id,
                            config.Sprite,
                            FurnitureDirection.Left,
                            EFurnitureSubCategory.Basic_Wall,
                            config.UnlockType, config.Price);
                        var isItemUnlock = UserData.Instance.IsFurnitureUnlocked(config.Id);
                        itemShow.LoadData(isItemUnlock);

                        itemShow.OnShow();
                        itemShow.SetInteractable(false);
                        _displayItems.Add(itemShow);
                    }
                    break;
                }
                default:
                {
                    foreach (var config in _furConfigList)
                    {
                        if (config.Config.SubCategory != subCategory) continue;
                        if (config.Config.IsMessy) continue;
                        var itemShow = _furItemPool.Get();

                        itemShow.LoadConfig(config.Config.Id, config.Config.Sprite, config.Config.BaseDirection,
                            config.Config.SubCategory,
                            config.Config.UnlockType, config.Config.Price);
                        var isItemUnlock = UserData.Instance.IsFurnitureUnlocked(config.Config.Id);
                        itemShow.LoadData(isItemUnlock);

                        itemShow.OnShow();
                        itemShow.SetInteractable(false);
                        _displayItems.Add(itemShow);
                    }
                    break;
                }
            }
        }

        #endregion

        #region TUTORIAL

        public void ShowDialogTutorialRemoveBed()
        {
            _dialogPanel.gameObject.SetActive(true);
            
            PlayDialogAnimation("But first I have to start cleaning, the house seems messy.");

            _nextDialogButton.onClick.AddListener(() =>
            {
                AudioManager.PlaySound(ESound.Click);
                _nextDialogButton.onClick.RemoveAllListeners();
                PlayDialogAnimation("This bed doesn't look very safe, I think it will break when I lie down to sleep at night. Take it away!");

                _nextDialogButton.onClick.AddListener(() =>
                {
                    AudioManager.PlaySound(ESound.Click);
                    _nextDialogButton.onClick.RemoveAllListeners();
                    _dialogPanel.gameObject.SetActive(false);
                    DecorTutorial.Instance.TutorClickOldBed();
                });
            });
        }


        public void TutorialClickFurnitureCategory()
        {
            _dialogPanel.gameObject.SetActive(true);
            PlayDialogAnimation(
                "Finally, part of the cleaning is done, now it's time to move in the new furniture, I'm looking forward to seeing what my house will look like.");

            _nextDialogButton.onClick.AddListener(() =>
            {
                AudioManager.PlaySound(ESound.Click);
                _nextDialogButton.onClick.RemoveAllListeners();
                _dialogPanel.gameObject.SetActive(false);

                var furnitureCategoryButton = _categoryButtons.FirstOrDefault(btn => btn.Category == EFurnitureCategory.Furniture);
                furnitureCategoryButton.SetInteractable(true);
                _fakeFurnitureCategoryButton.gameObject.SetActive(true);
                _fakeFurnitureCategoryButton.transform.position = furnitureCategoryButton.transform.position;
                CategoryButtonUI.ClickEvent.AddListener(OnFurnitureCategoryButtonClick);
            });
        }

        public void OnFurnitureCategoryButtonClick(EFurnitureCategory category)
        {
            _fakeFurnitureCategoryButton.gameObject.SetActive(false);
            ChangeCategory(EFurnitureCategory.Furniture);
            ChangeSubCategory(EFurnitureSubCategory.Fur_Bed);

            _categoryButtons.FirstOrDefault(btn => btn.Category == EFurnitureCategory.Furniture).SetInteractable(false);

            CategoryButtonUI.ClickEvent.RemoveListener(OnFurnitureCategoryButtonClick);
            FurnitureItemUI.ClickEvent.AddListener(OnBedItemClick);

            var itemGet = _displayItems.FirstOrDefault(item => item.FurId == "br1_bed_1");
            itemGet?.SetInteractable(true);
            itemGet?.transform.SetSiblingIndex(0);
            MoveFakeBedButton(itemGet.transform).Forget();
        }

        private async UniTaskVoid MoveFakeBedButton(Transform realButton)
        {
            await UniTask.Yield();
            _fakeBedButton.gameObject.SetActive(true);
            _fakeBedButton.transform.position = realButton.transform.position;
        }

        private void OnBedItemClick(FurnitureItemUI itemClick)
        {
            AudioManager.PlaySound(ESound.Click);
            FurnitureItemUI.ClickEvent.RemoveListener(OnBedItemClick);
            _displayItems.FirstOrDefault(item => item.FurId == "br1_bed_1").SetInteractable(false);
            _fakeBedButton.gameObject.SetActive(false);
            DecorTutorial.Instance.SpawnNewBed();
        }

        public void TutorialClickSofaCategory()
        {
            _dialogPanel.gameObject.SetActive(true);
            PlayDialogAnimation("Tonight I will sleep well with this new bed. Now do the same with the sofa.");

            _nextDialogButton.onClick.AddListener(() =>
            {
                AudioManager.PlaySound(ESound.Click);
                _nextDialogButton.onClick.RemoveAllListeners();
                _dialogPanel.gameObject.SetActive(false);

                var chairSubCategoryButton = _subCategoryButtons.FirstOrDefault(btn => btn.SubCategory == EFurnitureSubCategory.Fur_Sofa);
                chairSubCategoryButton.SetInteractable(true);
                _fakeChairSubCategoryButton.gameObject.SetActive(true);
                _fakeChairSubCategoryButton.transform.position = chairSubCategoryButton.transform.position;

                SubCategoryButtonUI.ClickEvent.AddListener(OnSofaSubcategoryButtonClick);
            });
        }

        public void OnSofaSubcategoryButtonClick(EFurnitureSubCategory subCate)
        {
            SubCategoryButtonUI.ClickEvent.RemoveListener(OnSofaSubcategoryButtonClick);

            _fakeChairSubCategoryButton.gameObject.SetActive(false);
            ChangeSubCategory(EFurnitureSubCategory.Fur_Sofa);

            _subCategoryButtons.FirstOrDefault(btn => btn.SubCategory == EFurnitureSubCategory.Fur_Sofa)
                .SetInteractable(false);

            var chairItem = _displayItems.FirstOrDefault(item => item.FurId == "br1_chair_1");
            chairItem.SetInteractable(true);
            chairItem.transform.SetSiblingIndex(0);
            MoveFakeChairButton(chairItem.transform).Forget();
            FurnitureItemUI.ClickEvent.AddListener(OnSofaItemClick);
        }

        private async UniTaskVoid MoveFakeChairButton(Transform realButton)
        {
            await UniTask.Yield();
            _fakeChairButton.gameObject.SetActive(true);
            _fakeChairButton.transform.position = realButton.transform.position;
        }

        public void OnSofaItemClick(FurnitureItemUI itemClick)
        {
            var chairItem = _displayItems.FirstOrDefault(item => item.FurId == "br1_chair_1");
            chairItem.SetInteractable(false);
            _fakeChairButton.gameObject.SetActive(false);
            FurnitureItemUI.ClickEvent.RemoveListener(OnSofaItemClick);
            DecorTutorial.Instance.SpawnNewSofa();
        }

        public void ShowDialogComplete()
        {
            _dialogPanel.gameObject.SetActive(true);
            PlayDialogAnimation("Great, now you can further decorate your room!");

            _nextDialogButton.onClick.AddListener(() =>
            {
                AudioManager.PlaySound(ESound.Click);
                _nextDialogButton.onClick.RemoveAllListeners();
                _dialogPanel.gameObject.SetActive(false);

                DecorTutorial.Instance.CompleteTutorial();
            });
        }

        #endregion
    }
}
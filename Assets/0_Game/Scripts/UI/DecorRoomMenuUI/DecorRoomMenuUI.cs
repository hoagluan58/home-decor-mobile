using Cysharp.Threading.Tasks;
using JSAM;
using NFramework;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class DecorRoomMenuUI : BaseUIView
    {
        [Header("UI")]
        [SerializeField] private ScrollRect _furItemScroll;
        [SerializeField] private RectTransform _mainCategoryContainer;
        [SerializeField] private RectTransform _subCategoryContainer;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _undoButton;
        [SerializeField] private Button _homeButton;
        [SerializeField] private Button _clearRoomButton;
        [SerializeField] private Button _upFloorButton;
        [SerializeField] private Button _downFloorButton;
        [SerializeField] private Button _settingsButton;

        [Header("CONFIG")]
        [SerializeField] private FurnitureCategoryConfigSO _categoryConfig;

        [Header("PREFAB")]
        [SerializeField] private FurnitureItemUI _furnitureItemPrefab;
        [SerializeField] private CategoryButtonUI _categoryButtonPrefab;
        [SerializeField] private SubCategoryButtonUI _subCategoryBtnPrefab;

        [Header("TUTORIAL")]
        [SerializeField] private GameObject _tutorialPanel;
        [SerializeField] private Button _fakeUpFloorButton;

        private readonly List<FurnitureItemUI> _displayItems = new();

        private readonly List<CategoryButtonUI> _categoryButtons = new();
        private readonly List<SubCategoryButtonUI> _displayCategoryButton = new();

        private readonly List<SubCategoryButtonUI> _subCategoryButtons = new();
        private readonly List<SubCategoryButtonUI> _displaySubCategoryButton = new();


        private EFurnitureCategory _curCategory;
        private EFurnitureSubCategory _curSubCategory;

        private ObjectPool<FurnitureItemUI> _furItemPool;

        private List<FurnitureConfigSO> _furConfigList => FurnitureManager.Instance.FurConfigList;

        private void OnEnable()
        {
            CategoryButtonUI.ClickEvent.AddListener(OnCategoryButtonClick);
            SubCategoryButtonUI.ClickEvent.AddListener(OnSubCategoryButtonClick);
            FurnitureItemUI.ClickEvent.AddListener(OnFurnitureItemClick);

            _saveButton.onClick.AddListener(OnSaveButtonClick);
            _undoButton.onClick.AddListener(OnUndoButtonClick);
            _homeButton.onClick.AddListener(OnHomeButtonClick);
            _clearRoomButton.onClick.AddListener(OnClearButtonClick);
            _upFloorButton.onClick.AddListener(OnUpFloorButtonClick);
            _downFloorButton.onClick.AddListener(OnDownFloorButtonClick);
            _settingsButton.onClick.AddListener(OnSettingsButtonClick);

            DecorHistoryController.HistoryChangeEvent += OnDecorHistoryChanged;
        }

        private void OnDisable()
        {
            CategoryButtonUI.ClickEvent.RemoveListener(OnCategoryButtonClick);
            SubCategoryButtonUI.ClickEvent.RemoveListener(OnSubCategoryButtonClick);
            FurnitureItemUI.ClickEvent.RemoveListener(OnFurnitureItemClick);

            _saveButton.onClick.RemoveListener(OnSaveButtonClick);
            _undoButton.onClick.RemoveListener(OnUndoButtonClick);
            _homeButton.onClick.RemoveListener(OnHomeButtonClick);
            _clearRoomButton.onClick.RemoveListener(OnClearButtonClick);
            _upFloorButton.onClick.RemoveListener(OnUpFloorButtonClick);
            _downFloorButton.onClick.RemoveListener(OnDownFloorButtonClick);
            _settingsButton.onClick.RemoveListener(OnSettingsButtonClick);

            DecorHistoryController.HistoryChangeEvent -= OnDecorHistoryChanged;
        }

        public void Start()
        {
            // Init pool.
            _furItemPool = new(
                () => Instantiate(_furnitureItemPrefab, _furItemScroll.content),
                item => item.gameObject.SetActive(true),
                item => item.gameObject.SetActive(false),
                item => Destroy(item.gameObject));

            // Generate main category buttons.
            foreach (var categoryConfig in _categoryConfig.Config)
            {
                var mainCateBtn = Instantiate(_categoryButtonPrefab, _mainCategoryContainer);
                mainCateBtn.OnCreate(
                    categoryConfig.MainCategory,
                    categoryConfig.CategoryIcon,
                    categoryConfig.SelectCategoryIcon,
                    categoryConfig.MainCategoryName);
                _categoryButtons.Add(mainCateBtn);

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
            ChangeCategory(EFurnitureCategory.Basic);
            await UniTask.Yield();
            UpdateFloorChangeButtons();

            // Play tutorial
            if (UserData.Instance.CurLevel >= 5 && UserData.Instance.CurTutorialIndex == 4)
            {
                PlayUpFloorTutorial();
            }
        }

        private void PlayUpFloorTutorial()
        {
            UIManager.Instance.Open(Define.UIName.BLOCK_POPUP);
            DialogController.Instance.Show();
            DialogController.Instance.PlayDialogAnimation("Click on the arrow to switch rooms!");
            DialogController.Instance.NextDialogEvent.AddListener(() =>
            {
                AudioManager.PlaySound(ESound.Click);
                DialogController.Instance.Hide();
                DialogController.Instance.NextDialogEvent.RemoveAllListeners();
                _tutorialPanel.gameObject.SetActive(true);
                _fakeUpFloorButton.onClick.AddListener(() =>
                {
                    _fakeUpFloorButton.onClick.RemoveAllListeners();
                    _tutorialPanel.gameObject.SetActive(false);
                    OnUpFloorButtonClick();

                    DialogController.Instance.Show();
                    DialogController.Instance.PlayDialogAnimation("I can't wait to decorate this new room!");
                    DialogController.Instance.NextDialogEvent.AddListener(() =>
                    {
                        AudioManager.PlaySound(ESound.Click);
                        DialogController.Instance.Hide();
                        DialogController.Instance.NextDialogEvent.RemoveAllListeners();
                        UserData.Instance.CurTutorialIndex++;
                        UIManager.Instance.Close(Define.UIName.BLOCK_POPUP);
                    });
                });
            });
            DialogController.Instance.SkipEvent.AddListener(OnSkipDialog);

            void OnSkipDialog()
            {
                AudioManager.PlaySound(ESound.Click);
                DialogController.Instance.Hide();
                DialogController.Instance.NextDialogEvent.RemoveAllListeners();
                DialogController.Instance.SkipEvent.RemoveAllListeners();
                UserData.Instance.CurTutorialIndex++;
                _tutorialPanel.gameObject.SetActive(false);
                UIManager.Instance.Close(Define.UIName.BLOCK_POPUP);
            }
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
            }

            LoadFurnitureItem(subCategory);
        }

        private void UpdateFloorChangeButtons()
        {
            _upFloorButton.gameObject.SetActive(UserData.Instance.CurLevel >= 5 && DecorRoomManager.Instance.CurRoomIdnex == 0);
            _downFloorButton.gameObject.SetActive(UserData.Instance.CurLevel >= 5 && DecorRoomManager.Instance.CurRoomIdnex == 1);
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
                            itemShow.transform.SetAsLastSibling();

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
                            itemShow.transform.SetAsLastSibling();

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
                            itemShow.transform.SetAsLastSibling();

                            _displayItems.Add(itemShow);
                        }
                        break;
                    }
            }
        }

        #region EVENT LISTENERS

        private void OnCategoryButtonClick(EFurnitureCategory furCategory)
        {
            ChangeCategory(furCategory);
        }

        private void OnSubCategoryButtonClick(EFurnitureSubCategory furSubCategory)
        {
            ChangeSubCategory(furSubCategory);
        }

        private void OnFurnitureItemClick(FurnitureItemUI itemClick)
        {
            if (itemClick.IsLock)
            {
                switch (itemClick.UnlockType)
                {
                    case FurnitureUnlockType.Ads:
                        {
                            // TODO: SHOW ADS
                            UserData.Instance.UnlockFurniture(itemClick.FurId);
                            itemClick.Unlock();
                            AudioManager.PlaySound(ESound.AdsItem);
                            break;
                        }

                    case FurnitureUnlockType.Coin:
                        {
                            if (UserData.Instance.GetCurrencyAmount(ECurrencyType.Diamond) < itemClick.Price)
                            {
                                AudioManager.PlaySound(ESound.Error);
                                return;
                            }
                            else
                            {
                                AudioManager.PlaySound(ESound.AdsItem);
                            }
                            UserData.Instance.ModifyCurrencyDic(ECurrencyType.Diamond, -itemClick.Price);
                            UserData.Instance.UnlockFurniture(itemClick.FurId);
                            itemClick.Unlock();
                            break;
                        }
                }
            }
            else
            {
                AudioManager.PlaySound(ESound.Click);
            }

            switch (itemClick.SubCategory)
            {
                // Load items
                case EFurnitureSubCategory.Basic_Ground:
                    {
                        DecorRoomManager.Instance.CurRoom.SetFloor(itemClick.FurId);
                        break;
                    }
                case EFurnitureSubCategory.Basic_Wall:
                    {
                        DecorRoomManager.Instance.CurRoom.SetWall(itemClick.FurId);
                        break;
                    }
                default:
                    {
                        DecorRoomManager.Instance.CurRoom.FurnitureController.SpawnFurniture(itemClick.FurId);
                        break;
                    }
            }
        }

        private void OnSaveButtonClick()
        {
            AudioManager.PlaySound(ESound.Click);
            DecorRoomManager.Instance.SaveAllRoomData();
            FeatureNavigator.Instance.Go(EGameFeature.Home, onFullyShowLoading: () => { DecorRoomManager.Instance.ReleaseAllRoomFurniture(); })
                .Forget();
        }

        private void OnUndoButtonClick()
        {
            AudioManager.PlaySound(ESound.Click);
            DecorRoomManager.Instance.CurRoom.HistoryController.Undo();
        }

        private void OnDecorHistoryChanged()
        {
            _undoButton.interactable = DecorRoomManager.Instance.CurRoom.HistoryController.CanUndo;
        }

        private void OnClearButtonClick()
        {
            AudioManager.PlaySound(ESound.Click);
            UIManager.Instance.Open(Define.UIName.CONFIRM_DELETE_ROOM_POPUP);
        }

        private void OnHomeButtonClick()
        {
            AudioManager.PlaySound(ESound.Click);
            var popup = UIManager.Instance.Open<ConfirmReturnHomePopupUI>(Define.UIName.CONFIRM_RETURN_HOME_POPUP);
            popup.SetData(OnYesButtonClicked, OnNoButtonClicked);

            void OnYesButtonClicked()
            {
                AudioManager.PlaySound(ESound.Click);
                FeatureNavigator.Instance.Go(EGameFeature.Home, onFullyShowLoading: () =>
                {
                    DecorRoomManager.Instance.ReleaseAllRoomFurniture();
                }).Forget();
                popup.CloseSelf();
            }

            void OnNoButtonClicked()
            {
                AudioManager.PlaySound(ESound.Click);
                popup.CloseSelf();
            }
        }

        private void OnUpFloorButtonClick()
        {
            AudioManager.PlaySound(ESound.Click);
            DecorRoomManager.Instance.DownFloor();
            UpdateFloorChangeButtons();
        }

        private void OnDownFloorButtonClick()
        {
            AudioManager.PlaySound(ESound.Click);
            DecorRoomManager.Instance.UpFloor();
            UpdateFloorChangeButtons();
        }

        private void OnSettingsButtonClick()
        {
            AudioManager.PlaySound(ESound.Click);
            UIManager.Instance.Open(Define.UIName.SETTINGS_POPUP);
        }

        #endregion
    }
}
using JSAM;
using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class HomeLandWorldUI : MonoBehaviour
    {
        [SerializeField] private UnlockFurButton _unlockFurButtonPf;
        [SerializeField] private RepaintingButton _repaintingButtonPf;
        [SerializeField] private Transform _unlockFurButtonContainer;
        [SerializeField] private Transform _repaintingButtonContainer;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Space(10)]
        [Header("Direction Button")]
        [SerializeField] private SerializableDictionaryBase<ELandNavigation, Vector2> _directionOffsetDic;
        [SerializeField] private SerializableDictionaryBase<ELandNavigation, Button> _directionButtonDic;

        private HomeLand _land;
        private List<RepaintingButton> _repaintingButtons = new List<RepaintingButton>();
        private List<UnlockFurButton> _unlockFurButtons = new List<UnlockFurButton>();

        private ObjectPool<RepaintingButton> _poolRepaintingBtn;
        private ObjectPool<UnlockFurButton> _poolUnlockFurBtn;

        public Vector3 UnlockFurButtonPos
        {
            get
            {
                if (_unlockFurButtons.Count > 0)
                {
                    return _unlockFurButtons[0].transform.position;
                }
                return Vector3.zero;
            }
        }

        private void Start()
        {
            _canvas.worldCamera = Camera.main;
            _poolRepaintingBtn = new(
                () => Instantiate(_repaintingButtonPf, _repaintingButtonContainer),
                item => item.gameObject.SetActive(true),
                item => item.gameObject.SetActive(false),
                item => Destroy(item.gameObject));
            _poolUnlockFurBtn = new(
                () => Instantiate(_unlockFurButtonPf, _unlockFurButtonContainer),
                item => item.gameObject.SetActive(true),
                item => item.gameObject.SetActive(false),
                item => Destroy(item.gameObject));
            HomeLandDecorController.OnDragFurniture += HomeLandDecorController_OnDragFurniture;
        }

        private void OnDestroy()
        {
            HomeLandDecorController.OnDragFurniture -= HomeLandDecorController_OnDragFurniture;
        }

        private void HomeLandDecorController_OnDragFurniture(bool isDragging) => _canvas.enabled = !isDragging;

        private void OrderOptionPopupUI_OnShownPopup(bool onShown) => EnableInteraction(!onShown);

        public void OnInit(HomeLand land) => _land = land;

        public void OnEnter()
        {
            SetActive(true);
            ShowDirectionButton();
            EnableInteraction(true);
            OrderOptionPopupUI.OnShownPopup += OrderOptionPopupUI_OnShownPopup;
        }

        public void OnExit()
        {
            foreach (var button in _directionButtonDic.Values)
            {
                button.gameObject.SetActive(false);
                button.onClick.RemoveAllListeners();
            }
            OrderOptionPopupUI.OnShownPopup -= OrderOptionPopupUI_OnShownPopup;
        }

        public void ShowUnlockFurButton(Vector3 position, int price, Action onClickUnlockFur = null, Action onClickDone = null)
        {
            var isEnoughCurrency = UserData.Instance.GetCurrencyAmount(ECurrencyType.Star) >= price;
            var button = _poolUnlockFurBtn.Get();
            button.transform.position = position;
            button.transform.SetParent(_unlockFurButtonContainer);
            button.SetData(new UnlockFurButton.ButtonModel()
            {
                IsEnough = isEnoughCurrency,
                Price = price,
                OnClickUnlock = OnClickUnlock,
                OnClickDone = OnClickDone,
            });
            var isShowHandTutorial = UserData.Instance.CurTutorialIndex < Define.TutorialIndex.DONE_UNLOCK_FURNITURE;
            button.ShowTutorial(isShowHandTutorial);

            _unlockFurButtons.Add(button);

            void OnClickUnlock()
            {
                onClickUnlockFur?.Invoke();
                button.ShowTutorial(false);
                if (isEnoughCurrency)
                {
                    _poolUnlockFurBtn.Release(button);
                    _unlockFurButtons.Remove(button);
                }
            }

            void OnClickDone() => onClickDone?.Invoke();
        }

        public void ShowRepaintingFurButton(Vector3 position, Action onClickRepainting = null, Action onClickDone = null)
        {
            var button = _poolRepaintingBtn.Get();
            button.transform.position = position;
            button.transform.SetParent(_repaintingButtonContainer);
            button.SetData(new RepaintingButton.ButtonModel()
            {
                OnClickRepainting = OnClickRepainting,
                OnClickDone = OnClickDone,
            });
            _repaintingButtons.Add(button);

            void OnClickRepainting()
            {
                button.gameObject.SetActive(false);
                onClickRepainting?.Invoke();
            }

            void OnClickDone()
            {
                onClickDone?.Invoke();
            }
        }

        public void Refresh()
        {
            ReleaseButtons();
            SetActive(true);
        }

        public void ReleaseButtons()
        {
            _unlockFurButtons.ForEach(x => _poolUnlockFurBtn.Release(x));
            _unlockFurButtons.Clear();
            _repaintingButtons.ForEach(x => _poolRepaintingBtn.Release(x));
            _repaintingButtons.Clear();
        }

        public void EnableInteraction(bool isEnable)
        {
            if (_canvasGroup == null)
            {
                return;
            }
            _canvasGroup.blocksRaycasts = isEnable;
        }

        public void ShowAllRepaintingButton()
        {
            _repaintingButtons.ForEach(x => x.gameObject.SetActive(true));
        }

        public void SetActive(bool value) => gameObject.SetActive(value);

        private void ShowDirectionButton()
        {
            foreach (var pair in _land.LandConfig.LandNavigation)
            {
                var dir = pair.Key;
                var landId = pair.Value;

                if (!LandController.Instance.CanGoToLand(landId)) continue;

                var button = _directionButtonDic[dir];
                var offsetPosition = _directionOffsetDic[dir];
                var position = _land.IsoWorld.IsoToScreen(new Vector2(_land.Bounds.Max.x + offsetPosition.x, _land.Bounds.Max.y + offsetPosition.y));

                button.gameObject.SetActive(true);
                button.transform.position = position;
                button.onClick.AddListener(OnButtonClick);

                void OnButtonClick()
                {
                    AudioManager.PlaySound(ESound.Click);
                    LandController.Instance.GoToLandId(landId);
                }
            }
        }
    }
}

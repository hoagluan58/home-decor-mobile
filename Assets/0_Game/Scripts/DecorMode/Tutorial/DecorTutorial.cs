using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using IsoTools;
using JSAM;
using Redcode.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class DecorTutorial : SingletonMono<DecorTutorial>
    {
        [SerializeField] private IsoWorld _isoWorld;
        [SerializeField] private List<FurnitureController> _allFur;
        [SerializeField] private List<FurnitureController> _saveFur;
        [SerializeField] private TutorialDecorMenuUI _menuUI;

        [Header("OLD BED")]
        [SerializeField] private FurnitureController _oldBed;
        [SerializeField] private Button _destroyOldBedButton;
        [SerializeField] private GameObject _destroyOldBedPopup;
        [SerializeField] private GameObject _clickOldBedTutorialHand;
        [SerializeField] private ParticleSystem _removeOldBedFx;

        [Header("NEW BED")]
        [SerializeField] private FurnitureController _newBed;
        [SerializeField] private GameObject _newBedPopup;
        [SerializeField] private Button _spinNewBedButton;

        [Header("NEW BED")]
        [SerializeField] private FurnitureController _newSofa;
        [SerializeField] private GameObject _newSofaPopup;
        [SerializeField] private Button _confirmSofaButton;

        private bool _isTutorClickOldBed;

        public void Start() => StartTutorial().Forget();

        public async UniTaskVoid StartTutorial()
        {
            await UniTask.Yield();
            Camera.main?.transform.SetPositionY(0);
            _menuUI.OnOpen();
            await UniTask.WaitForSeconds(1);
            _menuUI.ShowDialogTutorialRemoveBed();
        }

        public void TutorClickOldBed()
        {
            _isTutorClickOldBed = true;
            _oldBed.SpriteRenderer.sortingOrder = 110;
            _clickOldBedTutorialHand.SetActive(true);
        }

        public void OnOldBedClicked()
        {
            AudioManager.PlaySound(ESound.PickUp);
            _isTutorClickOldBed = false;
            _oldBed.VisualController.Highlight();
            _oldBed.SpriteRenderer.sortingOrder = 110;
            TutorClickClearButton();
            _clickOldBedTutorialHand.SetActive(false);
            _allFur.Clear();
        }

        public void TutorClickClearButton()
        {
            _destroyOldBedPopup.gameObject.SetActive(true);
            _destroyOldBedButton.onClick.AddListener(OnDestroyOldBedButtonClick);
        }

        public void OnDestroyOldBedButtonClick()
        {
            AudioManager.PlaySound(ESound.Remove);
            _destroyOldBedButton.onClick.RemoveListener(OnDestroyOldBedButtonClick);
            _destroyOldBedPopup.gameObject.SetActive(false);
            Destroy(_oldBed.gameObject);
            _menuUI.TutorialClickFurnitureCategory();
            _removeOldBedFx.Play();
        }

        public void SpawnNewBed()
        {
            _newBed.gameObject.SetActive(true);
            _newBedPopup.gameObject.SetActive(true);
            _newBed.VisualController.Highlight();
            _newBed.SpriteRenderer.sortingOrder = 110;
            _spinNewBedButton.onClick.AddListener(OnClickFlipBedButton);
        }

        public void OnClickFlipBedButton()
        {
            _spinNewBedButton.onClick.RemoveListener(OnClickFlipBedButton);
            _newBedPopup.gameObject.SetActive(false);
            _newBed.FlipController.FlipNegative();
            _newBed.VisualController.Normalize();
            _newBed.SpriteRenderer.sortingOrder = 110;
            AudioManager.PlaySound(ESound.Rotate);
            AwaitTutorialClickSofaCategory().Forget();
            return;

            async UniTaskVoid AwaitTutorialClickSofaCategory()
            {
                await UniTask.WaitForSeconds(0.8f);
                _newBed.VisualController.Normalize();
                _menuUI.TutorialClickSofaCategory();
            }
        }


        private void Update()
        {
            if (_isTutorClickOldBed)
            {
                if (InputHelper.HasInput())
                {
                    var inputPosition = InputHelper.GetIsoPosition(_isoWorld);

                    // Begin select handle
                    if (InputHelper.GetTouchPhase() == TouchPhase.Began)
                    {
                        if (RoomHelper.GetFurnitureOnTouch(inputPosition, Vector3.one * 14, _allFur, null) != null)
                        {
                            OnOldBedClicked();
                        }
                    }
                }
            }
        }

        public void SpawnNewSofa()
        {
            _newSofa.gameObject.SetActive(true);
            _newSofaPopup.gameObject.SetActive(true);
            _newSofa.VisualController.Highlight();
            _newSofa.SpriteRenderer.sortingOrder = 110;
            _confirmSofaButton.onClick.AddListener(OnClickConfirmNewSofa);
        }

        public void OnClickConfirmNewSofa()
        {
            AudioManager.PlaySound(ESound.Drop);
            _confirmSofaButton.onClick.RemoveListener(OnClickConfirmNewSofa);
            _newSofaPopup.gameObject.SetActive(false);
            _newSofa.VisualController.Normalize();
            _newSofa.SpriteRenderer.sortingOrder = 110;
            AwaitShowDialogComplete().Forget();
            return;

            async UniTaskVoid AwaitShowDialogComplete()
            {
                await UniTask.WaitForSeconds(0.8f);
                _newSofa.VisualController.Normalize();
                _menuUI.ShowDialogComplete();
            }
        }

        public void CompleteTutorial()
        {
            UserData.Instance.CurTutorialIndex++;
            SaveRoomData();
            FeatureNavigator.Instance.Go(EGameFeature.Decor).Forget();
        }

        public void SaveRoomData()
        {
            var result = new RoomData
            {
                Index = 0,
                FloorId = Define.DefaultId.FLOOR,
                WallId = Define.DefaultId.WALL
            };

            foreach (var fur in _saveFur)
            {
                var newFurData = new RoomFurnitureData(fur.Config.Id, fur.Position, fur.CurDirection);
                result.FurnitureData.Add(newFurData);
            }
            DecorModeData.Instance.SaveDecor(result);
        }
    }
}
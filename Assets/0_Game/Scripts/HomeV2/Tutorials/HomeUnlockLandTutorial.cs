using Cysharp.Threading.Tasks;
using IsoTools;
using NFramework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace YoyoDesign
{
    public class HomeUnlockLandTutorial : MonoBehaviour
    {
        public static event Action OnUnlockLand;

        [SerializeField] private GameObject _goContainer;
        [SerializeField] private IsoWorld _isoWorld;
        [SerializeField] private HomeLand _tutorialLand;

        public bool IsDoneTutorial { get; private set; }

        private readonly Vector3 LAND_POSITION = new Vector3(-46, -36, 0);
        private bool _isUpdate = false;

        private void Start() => TutorialPhase2PopupUI.OnShowTutorialLand += ShowTutorialLand;

        private void OnDestroy() => TutorialPhase2PopupUI.OnShowTutorialLand -= ShowTutorialLand;

        private void Update()
        {
            if (!_isUpdate) return;
            if (Input.touchCount <= 0) return;

            var inputPosition = _isoWorld.TouchIsoPosition(0);
            var touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                var landOnTouch = LandHelper.GetLandOnTouch(inputPosition, new List<HomeLand> { _tutorialLand });
                if (landOnTouch != null && landOnTouch == _tutorialLand)
                {
                    LandController.Instance.UnlockTutorialLand();
                    _goContainer.SetActive(false);
                    UserData.Instance.CurTutorialIndex = Define.TutorialIndex.DONE_UNLOCK_LAND;
                    OnUnlockLand?.Invoke();
                    _isUpdate = false;
                    IsDoneTutorial = true;
                }
            }
        }

        public async void StartTutorial()
        {
            IsDoneTutorial = false;
            _goContainer.SetActive(true);
            _tutorialLand.gameObject.SetActive(false);
            _tutorialLand.MoveLand(LAND_POSITION.x, LAND_POSITION.y);

            var zoomPosition = LandController.Instance.GetClampedCameraPosition(_tutorialLand.RoomCenterPos, Define.Size.LAND_CAMERA_SIZE);
            CameraController.Instance.SetPosition(zoomPosition.x, zoomPosition.y);
            var popup = UIManager.Instance.Open(Define.UIName.TUTORIAL_PHASE_2_POPUP);

            await UniTask.WaitUntil(() => popup == null || !popup.gameObject.activeSelf);
        }

        private void ShowTutorialLand()
        {
            _tutorialLand.gameObject.SetActive(true);
            _isUpdate = true;
        }
    }
}

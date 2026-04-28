using System.Collections.Generic;
using UnityEngine;

namespace YoyoDesign
{
    public class HomeFocusLandTutorial : MonoBehaviour
    {
        [SerializeField] private GameObject _container;
        [SerializeField] private HomeLand _tutorialLand;
        [SerializeField] private WallController _wallController;
        [SerializeField] private FloorController _floorController;

        public bool IsUpdate => _isUpdate;

        private bool _isUpdate = false;
        private LandConfigData _landConfig;

        private void Update()
        {
            if (!_isUpdate) return;
            if (!InputHelper.HasInput()) return;

            var inputPosition = InputHelper.GetIsoPosition(_tutorialLand.IsoWorld);
            var touchPhase = InputHelper.GetTouchPhase();

            if (touchPhase == TouchPhase.Began)
            {
                var landOnTouch = LandHelper.GetLandOnTouch(inputPosition, new List<HomeLand> { _tutorialLand });
                if (landOnTouch != null && landOnTouch == _tutorialLand)
                {
                    _isUpdate = false;
                    _container.SetActive(false);
                    LandController.Instance.FocusTutorialLand();
                }
            }
        }

        public void StartTutorial()
        {
            _isUpdate = true;
            _container.SetActive(true);
            _landConfig = AllConfig.Instance.LandConfigDic[Define.DefaultId.TUTORIAL_FOCUS_LAND_ID];

            var wallConfig = AllConfig.Instance.WallConfigDic[_landConfig.DefaultWallId];
            var floorConfig = AllConfig.Instance.FloorConfigDic[_landConfig.DefaultFloorId];

            _wallController.ChangeWall(wallConfig.Id, wallConfig.Sprite);
            _floorController.ChangeFloor(floorConfig.Id, floorConfig.Sprite);
        }
    }
}

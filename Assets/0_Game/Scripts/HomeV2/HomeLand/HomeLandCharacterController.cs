using Cysharp.Threading.Tasks;
using DG.Tweening;
using IsoTools;
using Sirenix.OdinInspector;
using Spine.Unity;
using System.Collections.Generic;
using UnityEngine;

namespace YoyoDesign
{
    public class HomeLandCharacterController : MonoBehaviour
    {
        [SerializeField] private SkeletonAnimation _charSpine;
        [SerializeField] private FurnitureController _charFurnitureController;
        [SerializeField] private IsoObject _isoObject;

        public Vector3 Size => _isoObject.size;
        public Vector3 Position => _isoObject.position;
        public FurnitureController CharFurnitureController => _charFurnitureController;

        private bool _isPatrol = false;
        private HomeLand _land;

        public void OnEnter(HomeLand land)
        {
            _land = land;
            _charSpine.gameObject.SetActive(true);
            LoadCharacter();
            SetRenderOrder(15 + _land.LandConfig.AdditionSortingValue);
        }

        public void OnExit()
        {
            _charSpine.gameObject.SetActive(false);
            _isPatrol = false;
        }

        public void LoadCharacter()
        {
            _charSpine.ApplyCharacterOutfit(DressUpData.Instance.GetCharacterOutfitSaveDatas());
        }

        public void SetRenderOrder(int value)
        {
            _charSpine.gameObject.GetComponent<MeshRenderer>().sortingOrder = value;
        }

        public void SetPosition(Vector3 position) => _isoObject.position = position;

        [Button]
        public async UniTask StartPatrol(List<Vector2Int> positionList)
        {
            var timeMoveEachNode = 0.5f;
            _isPatrol = true;
            var lastPosition = Vector2Int.zero;
            foreach (var position in positionList)
            {
                if (!_isPatrol) break;
                var deltaPosition = position - lastPosition;
                await DOTween.To(() => _isoObject.position, x =>
                {
                    _isoObject.position = x;
                    _charSpine.initialFlipX = deltaPosition.x < 0 || deltaPosition.y > 0;
                }, position, timeMoveEachNode).SetEase(Ease.Linear).WithCancellation(this.destroyCancellationToken);
                lastPosition = position;
            }
        }
    }
}

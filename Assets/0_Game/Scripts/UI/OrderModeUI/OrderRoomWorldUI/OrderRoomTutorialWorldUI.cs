using DG.Tweening;
using UnityEngine;

namespace YoyoDesign
{
    public class OrderRoomTutorialWorldUI : MonoBehaviour
    {
        [SerializeField] private Transform _tfHand;

        private void OnDisable()
        {
            _tfHand.DOKill();
        }

        public void HandDragTutorial(Vector3 startPos, Vector3 endPos)
        {
            _tfHand.position = startPos;
            _tfHand.DOKill();
            _tfHand.DOMove(endPos, 2f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
        }

        public void SetActive(bool value) => gameObject.SetActive(value);

        public void SetPosition(Vector3 position) => _tfHand.position = position;
    }
}

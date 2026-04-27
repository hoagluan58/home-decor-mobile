using Cysharp.Threading.Tasks;
using DG.Tweening;
using IsoTools;
using Redcode.Extensions;
using RotaryHeart.Lib.SerializableDictionary;
using Spine.Unity;
using System;
using UnityEngine;

namespace YoyoDesign
{
    public class TrashBoxController : MonoBehaviour
    {
        public static event Action OnTrashAdded;

        [SerializeField] private IsoObject _isoObject;
        [SerializeField] private FurnitureController _furnitureController;
        [SerializeField] private ETrashType _trashType;
        [SerializeField] private Transform _tfTrashBox;
        [SerializeField] private SkeletonAnimation _spineTrashBox;
        [SerializeField] private SerializableDictionaryBase<ETrashType, ETrashBoxSkin> _frontSkinDic;
        [SerializeField] private SerializableDictionaryBase<int, ETrashBoxAnim> _dirtAnimDic;
        [SerializeField] private Transform _tfIncorrectTrash;

        private Sequence _incorrectPopupSequence;
        private Tween _punchScaleTween;
        private int _capacity, _curTrashCount;

        public string FurId => _furnitureController.Config.Id;
        public Vector3 Size => _isoObject.size;
        public ETrashType TrashType => _trashType;
        public FurnitureController FurnitureController => _furnitureController;

        public void SetData(Vector3 position, ETrashType trashType, int capacity)
        {
            _isoObject.position = position;
            _trashType = trashType;
            _capacity = capacity;
            _curTrashCount = 0;
            ChangeTrashSkin();
            _spineTrashBox.AnimationState.SetAnimation(0, ETrashBoxAnim.open_box_frame.ToString(), false);
            DirtAnim();
        }

        public void AddTrash()
        {
            _curTrashCount++;
            PunchScale();
            DirtAnim();
            OnTrashAdded?.Invoke();
        }

        public bool IsTrashBoxFull() => _curTrashCount == _capacity;

        public void Highlight(bool isValid)
        {
            _tfTrashBox.SetLocalScale(isValid ? 1.05f : 1f);
        }

        public void ShowIncorrectTrashPanel()
        {
            _tfIncorrectTrash.gameObject.SetActive(true);
            _tfIncorrectTrash.DOKill();
            _tfIncorrectTrash.SetLocalScale(1f);
            _tfIncorrectTrash.localPosition = new Vector3(0f, 150f, 0f);
            _incorrectPopupSequence?.Kill();
            _incorrectPopupSequence = DOTween.Sequence();
            _incorrectPopupSequence.Insert(0f, _tfIncorrectTrash.DOLocalMoveY(-150f, 0.5f).SetEase(Ease.OutFlash).From());
            _incorrectPopupSequence.Insert(0f, _tfIncorrectTrash.DOScale(0f, 0.5f).SetEase(Ease.OutFlash).From());
            _incorrectPopupSequence.AppendInterval(0.5f);
            _incorrectPopupSequence.OnComplete(() => _tfIncorrectTrash.gameObject.SetActive(false));
        }

        public void Normalize()
        {
            _tfTrashBox.SetLocalScale(1f);
        }

        public async void OnRemove()
        {
            _spineTrashBox.AnimationState.SetAnimation(0, ETrashBoxAnim.close_box.ToString(), false);
            await UniTask.WaitForSeconds(0.6f, cancellationToken: destroyCancellationToken);
            await DOVirtual.Float(_isoObject.position.x, _isoObject.position.x - 30, 0.5f,
                    value => { _isoObject.position = _isoObject.position.WithX(value); })
                .OnComplete(() => FurnitureManager.Instance.Release(_furnitureController))
                .SetEase(Ease.InBack);
        }

        private void ChangeTrashSkin()
        {
            _spineTrashBox.Skeleton.SetSkin(_frontSkinDic[_trashType].ToString());
            _spineTrashBox.Skeleton.SetSlotsToSetupPose();
            _spineTrashBox.LateUpdate();
        }

        private void PunchScale()
        {
            _punchScaleTween?.Kill();
            _tfTrashBox.SetLocalScale(1f);
            _punchScaleTween = _tfTrashBox.DOPunchScale(Vector3.one * 0.1f, 0.15f);
        }

        private void DirtAnim()
        {
            if (_dirtAnimDic.TryGetValue(_curTrashCount, out var anim))
            {
                _spineTrashBox.AnimationState.SetAnimation(0, anim.ToString(), false);
            }
            else
            {
                _spineTrashBox.AnimationState.SetAnimation(0, ETrashBoxAnim.open_box_frame.ToString(), false);
            }
        }

        private enum ETrashBoxSkin
        {
            cartonbox_front_1,
            cartonbox_front_2,
            cartonbox_front_3,
            cartonbox_front_4,
        }

        private enum ETrashBoxAnim
        {
            close_box,
            dirt_fx,
            open_box1,
            open_box2,
            open_box3,
            open_box4,
            open_box_frame
        }
    }
}
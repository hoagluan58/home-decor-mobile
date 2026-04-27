using DG.Tweening;
using IsoTools;
using JSAM;
using Redcode.Extensions;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;

namespace YoyoDesign
{
    public class OrderModeDecorBox : MonoBehaviour
    {
        private const int DECOR_BOX_SORTING_ORDER = 15;
        private const int FOAM_SORTING_ORDER = 20;

        [SerializeField] private SkeletonAnimation _spineDecorBox;
        [SerializeField] private SkeletonAnimation _spineFoam;
        [SerializeField] private GameObject _goTutorialHand;

        [Title("CONFIG")]
        [SerializeField] private float _offsetBoxDropZ;

        public float DropBoxAnimTime = 0.8f;

        private int _maxItem;
        private Tween _punchScaleTween;
        private bool _isOpenBox, _isDropping;
        private IsoObject _isoObject;

        public bool IsOpenBox => _isOpenBox;

        private void Awake() => _isoObject = GetComponent<IsoObject>();

        public void DropBox(Vector3 dropPosition, int maxItem, bool isPlayTutorial = false)
        {
            _isOpenBox = false;
            _isoObject.position = new Vector3(dropPosition.x, dropPosition.y, _offsetBoxDropZ);
            _maxItem = maxItem;

            SetSortingOrder(0);
            _spineDecorBox.AnimationState.SetAnimation(0, EDecorBoxAnim.idle.ToString(), false);
            _spineFoam.gameObject.SetActive(false);
            _spineFoam.timeScale = 0f;
            _isDropping = true;
            DOVirtual.Float(_isoObject.position.z, 0, 0.5f, value =>
            {
                _isoObject.position = new Vector3(_isoObject.position.x, _isoObject.position.y, value);
            }).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                AudioManager.PlaySound(ESound.DropBox);
                _isDropping = false;
                PlayHandTutorial(isPlayTutorial);
            });
        }

        public void OpenBox()
        {
            if (_isDropping)
                return;

            AudioManager.PlaySound(ESound.Unbox_5);
            _isOpenBox = true;
            _spineDecorBox.timeScale = 1f;
            _spineDecorBox.AnimationState.SetAnimation(0, EDecorBoxAnim.open_decor_box.ToString(), false);
            _spineFoam.gameObject.SetActive(true);
        }

        public void FoamAnim(int itemLeft)
        {
            _spineFoam.timeScale = 1f;
            _spineFoam.AnimationState.SetAnimation(0, EDecorBoxAnim.foam.ToString(), false);
            _spineDecorBox.AnimationState.SetAnimation(0, GetFoamAnim(itemLeft).ToString(), false);
        }

        public void SetSortingOrder(int additionSortingOrder)
        {
            _spineDecorBox.GetComponent<MeshRenderer>().sortingOrder = DECOR_BOX_SORTING_ORDER + additionSortingOrder;
            _spineFoam.GetComponent<MeshRenderer>().sortingOrder = FOAM_SORTING_ORDER + additionSortingOrder;
        }

        public void PunchScale()
        {
            _punchScaleTween?.Kill();
            _spineDecorBox.transform.SetLocalScale(0.7f);
            _punchScaleTween = _spineDecorBox.transform.DOPunchScale(Vector3.one * 0.1f, 0.5f).SetEase(Ease.OutBack);
        }

        public void PlayHandTutorial(bool isPlay) => _goTutorialHand.SetActive(isPlay);

        private EDecorBoxAnim GetFoamAnim(int itemLeft)
        {
            var itemRate = (float)itemLeft / _maxItem;

            if (itemRate > 5f / 6f)
            {
                return EDecorBoxAnim.foam_down_1;
            }
            else if (itemRate > 4f / 6f)
            {
                return EDecorBoxAnim.foam_down_2;
            }
            else if (itemRate > 3f / 6f)
            {
                return EDecorBoxAnim.foam_down_3;
            }
            else if (itemRate > 2f / 6f)
            {
                return EDecorBoxAnim.foam_down_4;
            }
            else
            {
                return EDecorBoxAnim.foam_down_5;
            }
        }

        public enum EDecorBoxAnim
        {
            idle,
            foam,
            foam_down_1,
            foam_down_2,
            foam_down_3,
            foam_down_4,
            foam_down_5,
            foam_down_6,
            open_decor_box,
        }
    }
}

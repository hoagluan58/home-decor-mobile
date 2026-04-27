using DG.Tweening;
using IsoTools;
using JSAM;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;

namespace YoyoDesign
{
    public class OrderModeCartonBox : MonoBehaviour
    {
        [SerializeField] private SkeletonAnimation _spineBox;
        [SerializeField] private IsoObject _isoObject;
        [SerializeField] private GameObject _goHandTutorial;

        [Title("CONFIG")]
        [SerializeField] private float _offsetBoxDropZ;

        private ECartonBoxAnim _curBoxState;

        public float DropBoxAnimTime = 0.8f;
        public Vector3 Size => _isoObject.size;
        public Vector3 Position => _isoObject.position;
        public IsoObject IsoObject => _isoObject;
        public bool CanOpenBox => _curBoxState != 0 && !_isDropping;
        public bool IsBoxOpened => _curBoxState == 0;

        private bool _isDropping;

        private void Awake() => _isoObject = GetComponent<IsoObject>();

        public void DropBox(Vector3 position, bool isPlayTutorial = false)
        {
            _isoObject.position = new Vector3(position.x, position.y, position.z + _offsetBoxDropZ);
            _spineBox.timeScale = 0f;
            _curBoxState = ECartonBoxAnim.wood_sub;

            _isDropping = true;
            DOVirtual.Float(_isoObject.position.z, position.z, DropBoxAnimTime, value =>
            {
                _isoObject.position = new Vector3(_isoObject.position.x, _isoObject.position.y, value);
            }).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                AudioManager.PlaySound(ESound.DropBox);
                _isDropping = false;
                PlayHandTutorial(isPlayTutorial);
            });
        }

        public void OpenBoxStep()
        {
            PlayOpenBoxSound();
            _curBoxState--;
            _spineBox.timeScale = 1f;
            _spineBox.AnimationState.SetAnimation(0, _curBoxState.ToString(), false);
            PlayHandTutorial(false);
        }

        private void PlayOpenBoxSound()
        {
            switch (_curBoxState)
            {
                case ECartonBoxAnim.wood_sub:
                    AudioManager.PlaySound(ESound.Unbox_1);
                    break;
                case ECartonBoxAnim.wood_main:
                    AudioManager.PlaySound(ESound.Unbox_2);
                    break;
                case ECartonBoxAnim.rope:
                    AudioManager.PlaySound(ESound.Unbox_3);
                    break;
                case ECartonBoxAnim.open_box:
                    AudioManager.PlaySound(ESound.Unbox_4);
                    break;
            }
        }

        private void PlayHandTutorial(bool isPlay) => _goHandTutorial.SetActive(isPlay);

        private enum ECartonBoxAnim
        {
            open_box = 0,
            rope = 1,
            wood_main = 2,
            wood_sub = 3,
        }
    }
}

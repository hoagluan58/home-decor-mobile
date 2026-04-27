using Cysharp.Threading.Tasks;
using DG.Tweening;
using Redcode.Extensions;
using Spine.Unity;
using UnityEngine;

namespace YoyoDesign
{
    public class HomeIntroTutorial : MonoBehaviour
    {
        [SerializeField] private GameObject _goContainer;
        [SerializeField] private SkeletonAnimation _characterSkeletonAnim;
        [SerializeField] private ParticleSystem _fxSmoke;
        [SerializeField] private SpriteRenderer _srBackground;
        [SerializeField] private SkeletonAnimation _carSkeletonAnim;

        private bool _doneCarAnim, _doneCamAnim;

        public async UniTask StartTutorial()
        {
            _goContainer.SetActive(true);
            _characterSkeletonAnim.gameObject.SetActive(false);
            CarAnim();
            CamFollowCarAnim();

            await UniTask.WaitUntil(() => _doneCamAnim && _doneCamAnim);
        }

        private void CarAnim()
        {
            _carSkeletonAnim.gameObject.SetActive(true);
            var carTransform = _carSkeletonAnim.transform;
            var carSequence = DOTween.Sequence();
            carSequence
                .AppendCallback(() =>
                {
                    _doneCarAnim = false;
                    carTransform.localPosition = new Vector3(-72, -8);
                    _carSkeletonAnim.AnimationState.SetAnimation(0, "car_run1", true);
                })
                .Append(carTransform.DOLocalMove(new Vector2(-43, -24), 3f)).SetEase(Ease.Linear)
                .AppendInterval(2f)
                .AppendCallback(() =>
                {
                    _fxSmoke.Play();
                    _characterSkeletonAnim.gameObject.SetActive(true);
                    _characterSkeletonAnim.ApplyCharacterOutfit(DressUpData.Instance.GetCharacterOutfitSaveDatas());
                })
                .AppendCallback(() => _carSkeletonAnim.AnimationState.SetAnimation(0, "car_run2", true))
                .Append(carTransform.DOLocalMove(new Vector2(-72, -38), 2f).SetEase(Ease.Linear))
                .AppendCallback(() =>
                {
                    _carSkeletonAnim.gameObject.SetActive(false);
                    _doneCarAnim = true;
                });
            carSequence.SetTarget(this);
            carSequence.Play();
        }

        private void CamFollowCarAnim()
        {
            var camController = CameraController.Instance;

            var camTransform = camController.Camera.transform;
            var carTransform = _carSkeletonAnim.transform;
            var cameraSequence = DOTween.Sequence();
            var baseOrthoSize = camController.Camera.orthographicSize;

            cameraSequence
            .AppendCallback(() =>
            {
                camTransform.SetPositionXY(new Vector2(GetBoundX(), carTransform.position.y));
                camController.SetSize(20f);
                _doneCamAnim = false;
            })
            .Append(camTransform.DOMove(new Vector2(-42f, -12f), 3f).SetEase(Ease.Linear))
            .AppendInterval(2f)
            .AppendCallback(() =>
            {
                _doneCamAnim = true;
            });

            cameraSequence.SetTarget(this);
            cameraSequence.Play();

            float GetBoundX()
            {
                // Get the bounds of the sprite renderer
                var bounds = _srBackground.bounds;

                // Calculate the orthographic bounds for the camera
                var vertExtent = camController.Camera.orthographicSize;
                var horzExtent = vertExtent * Screen.width / Screen.height;

                // Set the bounds based on the sprite renderer's bounds and camera's orthographic size
                return bounds.min.x + horzExtent;
            }
        }
    }
}

using Cysharp.Threading.Tasks;
using JSAM;
using Spine.Unity;
using System;
using UnityEngine;

namespace YoyoDesign
{
    public class UnlockNewRoomAnimator : SingletonMono<UnlockNewRoomAnimator>
    {
        [Header("NEW ROOM TUTORIAL")]
        [SerializeField] private GameObject _tutorialObj;
        [SerializeField] private GameObject _secondFloor;
        [SerializeField] private ParticleSystem _showEffect;
        [SerializeField] private SkeletonAnimation _hammerAnim;
        [SerializeField] private HomeCharController _character;
        
        public async UniTaskVoid PlayUnlockNewRoomAnimation(Action onCompleteTutorial)
        {
            _character.gameObject.SetActive(false);
            _tutorialObj.SetActive(true);
            _hammerAnim.AnimationState.SetAnimation(0, "hammer", false);
            await UniTask.WaitForSeconds(0.5f, cancellationToken: destroyCancellationToken);
            VibrationManager.Vibrate(0.1f);
            AudioManager.PlaySound(ESound.Hammer);
            _secondFloor.SetActive(true);
            await UniTask.WaitForSeconds(1f, cancellationToken: destroyCancellationToken);
            AudioManager.PlaySound(ESound.Sparkling);
            _hammerAnim.gameObject.SetActive(false);
            _showEffect.Play();
            await UniTask.WaitForSeconds(2f, cancellationToken: destroyCancellationToken);
            _tutorialObj.SetActive(false);
            //HomeSceneManager.Instance.SecondFloorRoom.UnlockRoom();
            onCompleteTutorial?.Invoke();
        }
    }
}

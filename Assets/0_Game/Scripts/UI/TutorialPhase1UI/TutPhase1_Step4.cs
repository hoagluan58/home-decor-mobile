using Cysharp.Threading.Tasks;
using DG.Tweening;
using Spine.Unity;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class TutPhase1_Step4 : MonoBehaviour
    {
        public static event Action OnNextStep;

        [SerializeField] private RectTransform _rectPopup;
        [SerializeField] private GameObject _goTutorial;
        [SerializeField] private SkeletonGraphic _characterSkeletonGraphic;
        [SerializeField] private Button _btnStart;

        private void Awake() => _btnStart.onClick.AddListener(OnButtonStartClicked);

        private void OnEnable() => SetData();

        private void OnButtonStartClicked() => OnNextStep?.Invoke();

        private async void SetData()
        {
            _rectPopup.DOKill();
            _rectPopup.PunchScalePopup(Vector3.one * 0.1f, 0.2f);
            _goTutorial.SetActive(false);
            var userOutfitData = DressUpData.Instance.GetCharacterOutfitSaveDatas();
            _characterSkeletonGraphic.ApplyCharacterOutfit(userOutfitData);

            await UniTask.WaitForSeconds(0.5f);

            _goTutorial.SetActive(true);
        }
    }
}

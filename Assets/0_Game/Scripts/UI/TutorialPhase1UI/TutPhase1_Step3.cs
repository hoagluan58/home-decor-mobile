using Spine.Unity;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class TutPhase1_Step3 : MonoBehaviour
    {
        public static event Action OnNextStep;

        [SerializeField] private Button _btnDialogCloud;
        [SerializeField] private SkeletonGraphic _characterSkeletonGraphic;
        [SerializeField] private SkeletonGraphic _npcSkeletonGraphic;

        private void Awake() => _btnDialogCloud.onClick.AddListener(OnButtonDialogCloudClicked);

        private void OnEnable() => SetData();

        private void OnButtonDialogCloudClicked() => OnNextStep?.Invoke();

        private void SetData()
        {
            var userOutfitData = DressUpData.Instance.GetCharacterOutfitSaveDatas();
            _characterSkeletonGraphic.ApplyCharacterOutfit(userOutfitData);
        }
    }
}

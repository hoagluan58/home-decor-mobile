using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class TutPhase1_Step2 : MonoBehaviour
    {
        public static event Action OnNextStep;

        [SerializeField] private TextMeshProUGUI _tmpDialog;
        [SerializeField] private Button _btnNextStep;

        private void Awake() => _btnNextStep.onClick.AddListener(OnButtonNextStepClicked);

        private void OnEnable() => SetData();

        private void OnButtonNextStepClicked() => OnNextStep?.Invoke();

        private async void SetData()
        {
            _tmpDialog.PlayDialogAnimation($"I heard that you are a talented designer, can you help me redecorate my house?");
            _btnNextStep.gameObject.SetActive(false);

            await UniTask.WaitForSeconds(Define.TimeLength.DIALOG_TIME);

            _btnNextStep.gameObject.SetActive(true);
        }
    }
}

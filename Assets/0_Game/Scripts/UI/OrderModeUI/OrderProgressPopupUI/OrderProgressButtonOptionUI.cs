using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class OrderProgressButtonOptionUI : MonoBehaviour
    {
        [SerializeField] private Button _btnTry;
        [SerializeField] private Button _btnConfirm;
        [SerializeField] private TextMeshProUGUI _txtFurId;
        private Action _onClickTry, _onClickConfirm;

        private void Awake()
        {
            _btnTry.onClick.AddListener(OnButtonTryClicked);
            _btnConfirm.onClick.AddListener(OnButtonConfirmClicked);
        }

        public void SetData(string furId, Action onClickTry = null, Action onClickConfirm = null)
        {
            _onClickTry = onClickTry;
            _onClickConfirm = onClickConfirm;
            _txtFurId.text = furId;
        }

        private void OnButtonTryClicked()
        {
            _onClickTry?.Invoke();
        }

        private void OnButtonConfirmClicked()
        {
            _onClickConfirm?.Invoke();
        }
    }
}

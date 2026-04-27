using System;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class RepaintingButton : MonoBehaviour
    {
        [SerializeField] private Button _btnRepainting;
        [SerializeField] private Button _btnDone;

        private ButtonModel _model;

        private void Awake()
        {
            _btnRepainting.onClick.AddListener(OnButtonRepaintingClicked);
            _btnDone.onClick.AddListener(OnButtonDoneClicked);
        }

        private void OnButtonDoneClicked()
        {
            _model.OnClickDone?.Invoke();
        }

        private void OnButtonRepaintingClicked()
        {
            _btnDone.gameObject.SetActive(true);
            _model.OnClickRepainting?.Invoke();
        }

        public void SetData(ButtonModel model, bool isDone = false)
        {
            _model = model;
            _btnDone.gameObject.SetActive(isDone);
        }

        public class ButtonModel
        {
            public Action OnClickRepainting;
            public Action OnClickDone;

            public ButtonModel(Action onClickRepainting = null, Action onClickDone = null)
            {
                OnClickRepainting = onClickRepainting;
                OnClickDone = onClickDone;
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NFramework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class TutorialPopupUI : BaseUIView
    {
        #region PARAM

        [Header("=====", order = 0)]
        [Header("HighLight")]
        [SerializeField] private Button _btnHighlight;
        [SerializeField] private GameObject _goMaskBtn;
        [SerializeField] private GameObject _goHandClick;

        [Header("Dialogue")]
        [SerializeField] private Button _dialogLayout;
        [SerializeField] private Text _txtDialog;
        [SerializeField] private GameObject _goDialogue;
        [SerializeField] private string[] _messageDialog;
        [SerializeField] private Action _dialogComplete;
        [SerializeField] private int _indexDialogCount;
        [SerializeField] private float _timeDialogPer = 0.04f;

        private TextWriter.TextWriterSingle textWriterSingle;
        private Coroutine _coroutineHighLight;
        private bool isDialogComplete;

        #endregion

        #region UNITY METHODS

        private void OnEnable()
        {
            _dialogLayout.onClick.AddListener(SkipDialog);
        }

        private void OnDisable()
        {
            _dialogLayout.onClick.RemoveListener(SkipDialog);
        }

        #endregion

        #region HIGHLIGHT MASK
        public void HighLight(Image image, bool isShowHandClick = false, bool isCling = true, Action onClick = null)
        {
            _goHandClick.SetActive(isShowHandClick);
            _goMaskBtn.SetActive(true);
            _goDialogue.SetActive(!_goMaskBtn.activeSelf);

            //Add Event Button
            _btnHighlight.onClick.RemoveAllListeners();
            _btnHighlight.onClick.AddListener(() =>
            {
                onClick?.Invoke();
            });

            //Set Position Btn
            RectTransform _rectPivotBtn = image.GetComponent<RectTransform>();
            _btnHighlight.GetComponent<Image>().sprite = image.sprite;
            _btnHighlight.GetComponent<RectTransform>().pivot = _rectPivotBtn.pivot;
            _btnHighlight.GetComponent<RectTransform>().sizeDelta = new Vector2(_rectPivotBtn.rect.width, _rectPivotBtn.rect.height);
            _btnHighlight.transform.position = _rectPivotBtn.position;
            _btnHighlight.transform.rotation = _rectPivotBtn.rotation;

            //Object Cling
            if (isCling)
            {
                if (_coroutineHighLight != null) StopCoroutine(_coroutineHighLight);
                _coroutineHighLight = StartCoroutine(IHighLightCling(_btnHighlight.transform, image.transform));
            }
        }

        #endregion

        #region DIALOGUE
        public void ShowDialogText(string[] text, Action onComplete = null)
        {
            isDialogComplete = false;
            _dialogComplete = onComplete;
            _messageDialog = text;

            //Active
            _goDialogue.SetActive(true);
            _goMaskBtn.gameObject.SetActive(!_goDialogue.activeSelf);

            //Add text dialog
            _indexDialogCount = 0;
            textWriterSingle = TextWriter.AddWriter_Static(_txtDialog, _messageDialog[_indexDialogCount], _timeDialogPer, true, true, () =>
            {
                if (_messageDialog.Last() == _messageDialog[_indexDialogCount])
                {
                    isDialogComplete = true;
                }
            });
        }
        private void SkipDialog()
        {
            if (isDialogComplete)
            {
                _dialogComplete?.Invoke();
            }
            else
            {
                if (textWriterSingle != null && textWriterSingle.IsActive())
                {
                    // Currently active TextWriter
                    textWriterSingle.WriteAllAndDestroy();
                }
                else
                {
                    _indexDialogCount++;
                    textWriterSingle = TextWriter.AddWriter_Static(_txtDialog, _messageDialog[_indexDialogCount], 0.05f, true, true, () =>
                    {
                        if (_messageDialog.Last() == _messageDialog[_indexDialogCount])
                        {
                            isDialogComplete = true;
                        }
                    });
                }
            }

        }

        #endregion

        #region BASEVIEW METHODS

        public override void OnOpen()
        {
            base.OnOpen();
        }

        public override void OnClose()
        {
            base.OnClose();

            StopAllCoroutines();
        }

        #endregion

        #region Coroutine

        IEnumerator IHighLightCling(Transform trans, Transform transTarget)
        {
            WaitForSeconds wait = new WaitForSeconds(0.02f);
            while (true)
            {
                yield return wait;
                trans.position = transTarget.position;
                trans.localScale = transTarget.localScale;
            }
        }

        #endregion

    }
}

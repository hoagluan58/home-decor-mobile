using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

namespace YoyoDesign
{
    [RequireComponent(typeof(Canvas))]
    public class FurnitureMessagePopup : MonoBehaviour
    {
        private Canvas _canvas;

        [SerializeField] private TextMeshProUGUI _textPopup;

        private Tween _hidePopupTween;

        public void ShowMessage(string message)
        {
            if (_textPopup.gameObject.activeSelf)
            {
                
            }
            else
            {
                
            }
            
            _textPopup.gameObject.SetActive(true);
            _textPopup.text = message;
            
        
        }
    }
}
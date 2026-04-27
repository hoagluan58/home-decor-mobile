using System;
using System.Collections.Generic;
using DG.Tweening;
using Redcode.Extensions;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;

namespace YoyoDesign
{
    public class FurnitureVisualController : MonoBehaviour
    {
        [SerializeField] private FurnitureController _controller;
        [SerializeField] private Transform _checkerContainer;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private List<SpriteRenderer> _checkerSpriteRenderers;
        [SerializeField] private SerializableDictionaryBase<EFurnitureMaterial, Material> _materialDic;

        private Tween _pickUpTween;
        private Tween _putDownTween;
        private Tween _punchScaleTween;

        public readonly Color GREEN_CHECKER_COLOR = new(0f, 1f, 0f, 0.5f);
        public readonly Color RED_CHECKER_COLOR = new(1f, 0f, 0f, 0.5f);

        private bool _isValid = true;
        public bool IsValid
        {
            get => _isValid;
            set
            {
                if (value == _isValid) return;
                _isValid = value;
                foreach (var checker in _checkerSpriteRenderers)
                {
                    checker.color = _isValid ? GREEN_CHECKER_COLOR : RED_CHECKER_COLOR;
                }
            }
        }

        public void Highlight(bool isShowChecker = true, int additionSortingOrder = 0)
        {
            // Set fade value = 1f.
            var rendererColor = _spriteRenderer.color;
            rendererColor.a = 1f;
            _spriteRenderer.color = rendererColor;

            // Set sorting order.
            _spriteRenderer.sortingOrder = Define.SortingOrder.HIGH_LIGHT + additionSortingOrder;

            // Pick-up animation.
            _putDownTween?.Kill();
            _pickUpTween?.Kill();
            _pickUpTween = _spriteRenderer.transform.DOLocalMove(Vector3.up * 0.2f, 0.1f);

            // Show checkers.
            if (isShowChecker)
            {
                foreach (var checker in _checkerSpriteRenderers)
                {
                    checker.gameObject.SetActive(true);
                    checker.sortingOrder = Define.SortingOrder.CHECKER + additionSortingOrder;
                    checker.color = GREEN_CHECKER_COLOR;
                }
            }

            // Highlight child.
            foreach (var child in _controller.NestedController.Childs)
            {
                child.VisualController.Highlight(false, additionSortingOrder);
            }
        }

        public void FadeOut()
        {
            // Set fade value = 0.5f;
            var rendererColor = _spriteRenderer.color;
            rendererColor.a = 0.5f;
            _spriteRenderer.color = rendererColor;

            // Fade childs
            foreach (var child in _controller.NestedController.Childs)
            {
                child.VisualController.FadeOut();
            }
        }

        public void Normalize(bool isAnimation = true)
        {
            // Set sorting order
            SetScale(1);
            _spriteRenderer.sortingOrder = _controller.Config.IsCarpet
                ? Define.SortingOrder.CARPET
                : _controller.Config.IsWindow
                    ? Define.SortingOrder.WINDOW
                    : Define.SortingOrder.NORMAL;

            // Set fade value = 1;
            var rendererColor = _spriteRenderer.color;
            rendererColor.a = 1f;
            _spriteRenderer.color = rendererColor;

            // Put down fur animation.
            _putDownTween?.Kill();
            _pickUpTween?.Kill();
            if (isAnimation)
            {
                _putDownTween = _spriteRenderer.transform.DOLocalMove(Vector3.zero, 0.1f);
            }
            else
            {
                _spriteRenderer.transform.localPosition = Vector3.zero;
            }

            // Hide all checkers.
            foreach (var checker in _checkerSpriteRenderers)
            {
                checker.gameObject.SetActive(false);
            }

            // Nomalize all childs
            foreach (var child in _controller.NestedController.Childs)
            {
                child.VisualController.Normalize(isAnimation);
            }
        }

        public void OnFlip(FurnitureDirection directionToFlip, FurnitureDirection baseDirection)
        {
            _spriteRenderer.transform.SetLocalEulerAnglesY(directionToFlip == baseDirection ? 0 : 180);
            _checkerContainer.SetLocalEulerAnglesY(directionToFlip == baseDirection ? 0 : 180);
        }

        public void OnRelease()
        {
            Outline(EFurnitureMaterial.Default);
        }

        public void PunchScale()
        {
            _punchScaleTween?.Kill();
            _spriteRenderer.transform.SetLocalScale(1);
            _punchScaleTween = _spriteRenderer.transform.DOPunchScale(Vector3.one * 0.1f, 0.15f);
        }

        public void Outline(EFurnitureMaterial outlineType)
        {
            if (_materialDic.TryGetValue(outlineType, out var material))
            {
                _spriteRenderer.material = _materialDic[outlineType];
            }
        }

        public void SetScale(float value, bool isAnim = false, float animTime = 0.5f, Action onScaleComplete = null)
        {
            if (isAnim)
            {
                _spriteRenderer.DOKill();
                _spriteRenderer.transform.DOScale(value, animTime)
                    .OnComplete(() => onScaleComplete?.Invoke());
            }
            else
            {
                _spriteRenderer.transform.SetLocalScale(value);
            }
        }

        #region EDITOR

#if UNITY_EDITOR
        public void OnCreate(FurnitureController controller, SpriteRenderer spriteRenderer, Transform checkerContainer,
            List<SpriteRenderer> checkers, SerializableDictionaryBase<EFurnitureMaterial, Material> materialDic)
        {
            _controller = controller;
            _spriteRenderer = spriteRenderer;
            _checkerContainer = checkerContainer;
            _checkerSpriteRenderers = checkers;
            _materialDic = materialDic;
        }
#endif

        #endregion
    }
}
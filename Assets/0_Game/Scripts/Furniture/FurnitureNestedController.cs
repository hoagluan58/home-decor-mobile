using System;
using System.Collections.Generic;
using Redcode.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YoyoDesign
{
    public class FurnitureNestedController : MonoBehaviour
    {
        [SerializeField] private FurnitureController _controller;

        [Header("PARENT - CHILD")]
        [SerializeField, ReadOnly] private FurnitureController _parent;
        [ShowInInspector, ReadOnly] private HashSet<FurnitureController> _childs = new();

        [SerializeField, ReadOnly] private Vector3 _localPosition;
        public FurnitureController Parent => _parent;
        public HashSet<FurnitureController> Childs => _childs;
        public Vector3 LocalPosition => _localPosition;

#if UNITY_EDITOR
        public void OnCreate(FurnitureController furnitureController)
        {
            _controller = furnitureController;
        }
#endif

        public void UpdateLocalPosition()
        {
            _localPosition = _parent == null ? Vector3.zero : _parent.Position - _controller.Position;
        }

        public void SetParent(FurnitureController furParent)
        {
            _parent = furParent;
        }

        public void RemoveParent()
        {
            _parent = null;
            _localPosition = Vector3.zero;
        }

        public void AddChild(FurnitureController furChild)
        {
            _childs.Add(furChild);
        }

        public void RemoveChild(FurnitureController furRemove)
        {
            _childs.Remove(furRemove);
        }

        public void FlipLocalPosition()
        {
            _localPosition = _localPosition.GetYXZ();
        }

        public void OnRelease()
        {
            _parent = null;
            _childs.Clear();
        }

        public bool HasChild() => _childs.Count > 0;

        public bool HasParent() => _parent != null;
    }
}
using IsoTools;
using Redcode.Extensions;
using RotaryHeart.Lib.SerializableDictionary;
using System.Collections.Generic;
using UnityEngine;

namespace YoyoDesign
{
    public class FurnitureController : MonoBehaviour
    {
        [Header("CONFIG")]
        [SerializeField] private FurnitureConfigSO _configSO;

        [Header("COMPONENTS", order = 0)]
        [Header("Isometric", order = 1)]
        [SerializeField] private IsoObject _isoObject;
        [Header("Controllers")]
        [SerializeField] private FurnitureVisualController _visualController;
        [SerializeField] private FurnitureFlipController _flipController;
        [SerializeField] private FurnitureMoveController _moveController;
        [SerializeField] private FurnitureNestedController _nestedController;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        // VALUE PROPERTIES
        public FurnitureDirection CurDirection => FlipController.CurDirection;
        public Vector3 Position => IsoObject.position;
        public Vector3 Size => IsoObject.size;
        public Vector3 SurfacePos => Position + (FlipController.IsDefault
            ? Config.SurfaceOffset
            : Config.SurfaceOffset.GetYXZ());
        public Vector3 SurfaceSize => FlipController.IsDefault
            ? Config.SurfaceSize
            : Config.SurfaceSize.GetYXZ();
        public Vector3 LocalPosition => _nestedController.LocalPosition;

        // COMPONENT PROPERTIES
        public FurnitureConfigData Config => _configSO.Config;
        public IsoObject IsoObject => _isoObject;
        public FurnitureMoveController MoveController => _moveController;
        public FurnitureFlipController FlipController => _flipController;
        public FurnitureVisualController VisualController => _visualController;
        public FurnitureNestedController NestedController => _nestedController;
        public SpriteRenderer SpriteRenderer => _spriteRenderer;

#if UNITY_EDITOR
        public void OnCreate(FurnitureConfigSO config, SpriteRenderer spriteRenderer, Transform checkerContainer,
            List<SpriteRenderer> checkers, SerializableDictionaryBase<EFurnitureMaterial, Material> materialDic)
        {
            _configSO = config;
            _spriteRenderer = spriteRenderer;

            _isoObject = gameObject.GetOrAddComponent<IsoObject>();

            _moveController = gameObject.GetOrAddComponent<FurnitureMoveController>();
            _moveController.OnCreate(this);

            _visualController = gameObject.GetOrAddComponent<FurnitureVisualController>();
            _visualController.OnCreate(this, spriteRenderer, checkerContainer, checkers, materialDic);

            _flipController = gameObject.GetOrAddComponent<FurnitureFlipController>();
            _flipController.OnCreate(this);

            _nestedController = gameObject.GetOrAddComponent<FurnitureNestedController>();
            _nestedController.OnCreate(this);

        }
#endif
    }
}
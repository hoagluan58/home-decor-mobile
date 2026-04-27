using NFramework;
using System;
using UnityEngine;

namespace YoyoDesign
{
    [CreateAssetMenu(menuName = "ScriptableObject/FurnitureConfig", fileName = "Fur_")]
    public class FurnitureConfigSO : ScriptableObject
    {
        public FurnitureConfigData Config;
    }

    [Serializable]
    public class FurnitureConfigData
    {
        [Header("BASE")]
        public string Id;
        public Vector3 Size;
        public string FurnitureName;
        public Sprite Sprite;
        public EFurnitureCategory Category;
        public EFurnitureSubCategory SubCategory;
        public ERoomConceptName Concept;
        public FurnitureUnlockType UnlockType;
        public float Price;

        [ConditionalField(nameof(IsMessy), compareValues: true)]
        public ETrashType TrashType;

        [Header("MOVE")]
        public FurnitureMoveType FurnitureMoveType;

        [Header("ATTACHED")]
        public bool CanPlaceOnOthers;
        public bool CanBePlaceOn;
        public Vector3 SurfaceSize;
        public Vector3 SurfaceOffset;

        [Header("FLIP")]
        public FurnitureDirection BaseDirection;
        public bool CanFlip;
        public bool IsAutoFlip;

        [Header("SPECIAL")]
        public bool IsWindow;
        public bool IsCarpet;
        public bool IsSpecial;
        public bool IsMessy;

        [Header("PREFAB")]
        public FurnitureController Prefab;

#if UNITY_EDITOR
        [HideInInspector] public string SizeString;
        [HideInInspector] public string SurfaceSizeString;
        [HideInInspector] public string SurfaceOffsetString;
#endif
    }

    public enum FurnitureMovePlace
    {
        Floor,
        LeftWall,
        RightWall
    }

    public enum FurnitureDirection
    {
        Left,
        Right,
    }

    public enum FurnitureMoveType
    {
        /// <summary>
        /// Heavy furniture
        /// </summary>
        Floor,
        Wall,
        /// <summary>
        /// Decor
        /// </summary>
        Flexible
    }

    public enum EFurnitureCategory
    {
        None,
        Basic,
        Wall,
        Furniture,
        Accessory,
        Ornament,
        Other,
    }

    public enum EFurnitureSubCategory
    {
        None,
        Basic_Wall,
        Basic_Ground,
        Wall_Window,
        Wall_Accessory,
        Wall_Shelf,
        Fur_Bed,
        Fur_Table,
        Fur_Sofa,
        Fur_Cabinet,
        Acc_Carpet,
        Acc_Tree,
        Acc_Gear,
        Orna_Electric,
        Orna_IndooPlants,
        Orna_Other,
        Other_FoodDrink,
        Other_DiningSubtances,
        Other_Bathroom,
    }

    public enum ERoomConceptName
    {
        None,
        Bedroom1,
        MessyRoom,
        Bedroom2,
        Normal,
        Bedroom3,
        Kitchen1,
        Kitchen2,
        Kitchen3,
        Livingroom1,
        Livingroom2,
        Bathroom1,
        Livingroom3,
        Bathroom2,
        Bathroom3,
    }

    public enum FurnitureUnlockType
    {
        Free,
        Ads,
        Coin,
    }

    public enum ETrashType
    {
        None,
        EWaste,
        Paper,
        Organic,
        Clothes,
    }
}
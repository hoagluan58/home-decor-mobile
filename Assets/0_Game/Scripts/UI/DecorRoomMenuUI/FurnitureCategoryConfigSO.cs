using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YoyoDesign
{
    public class FurnitureCategoryConfigSO : ScriptableObject
    {
        public List<FurnitureCategoryConfigData> Config;

        public FurnitureSubCategoryConfigData[] GetSubcategory(EFurnitureCategory mainCategory)
        {
            var categoryConfigData = Config.FirstOrDefault(c => c.MainCategory == mainCategory);
            return categoryConfigData.SubCategories;
        }
    }

    [Serializable]
    public class FurnitureCategoryConfigData
    {
        public EFurnitureCategory MainCategory;
        public string MainCategoryName;
        public Sprite CategoryIcon;
        public Sprite SelectCategoryIcon;
        public FurnitureSubCategoryConfigData[] SubCategories;
    }

    [Serializable]
    public class FurnitureSubCategoryConfigData
    {
        public EFurnitureSubCategory SubCategory;
        public string SubCategoryName;
        public Sprite IconSprite;
    }
}
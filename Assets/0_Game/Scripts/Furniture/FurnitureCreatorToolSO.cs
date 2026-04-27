using IsoTools;
using NFramework;
using RotaryHeart.Lib.SerializableDictionary;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace YoyoDesign
{
    public class FurnitureCreatorToolSO : GoogleSheetConfigSO<FurnitureConfigData>
    {
        [Button]
        protected override void Sync()
        {
            base.Sync();
        }

#if UNITY_EDITOR

        [Header("PATH")]
        [SerializeField] private ERoomConceptName Category;
        [SerializeField] private string ScriptableObjectPath;
        [SerializeField] private string PrefabPath;

        [Header("CHECKER PREFABS")]
        public Transform FloorChecker;
        public Transform LeftWallChecker;
        public Transform RightWallChecker;

        [Header("MATERIAL")]
        [SerializeField] private SerializableDictionaryBase<EFurnitureMaterial, Material> _materialDic;

        protected override void OnSynced(List<FurnitureConfigData> googleSheetData)
        {
            base.OnSynced(googleSheetData);

            DeleteAllFilesInFolder(ScriptableObjectPath + "/" + Category);
            DeleteAllFilesInFolder(PrefabPath + "/" + Category);

            foreach (var data in _datas)
            {
                data.Size = Utilities.ParseStringToVector3(data.SizeString);
                data.SurfaceSize = Utilities.ParseStringToVector3(data.SurfaceSizeString);
                data.SurfaceOffset = Utilities.ParseStringToVector3(data.SurfaceOffsetString);
                data.Sprite = FileUtils.GetAssetByName<Sprite>(data.Id);
                if (data.Sprite == null)
                {
                    this.LogError("Sprite invalid " + data.Id);
                }

                if (data.IsCarpet)
                {
                    data.Size.z = 0.1f;
                }

                var configSO = CreateSO(data);
                var prefab = CreatePrefab(configSO);

                configSO.Config.Prefab = prefab;

                EditorUtility.SetDirty(configSO);
                EditorUtility.SetDirty(prefab);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private void DeleteAllFilesInFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            else
            {
                foreach (string file in Directory.GetFiles(folderPath))
                {
                    File.Delete(file);
                }
            }
        }

        public FurnitureConfigSO CreateSO(FurnitureConfigData configData)
        {
            // Save the asset as a .asset file
            var scriptableObject = CreateInstance(typeof(FurnitureConfigSO)) as FurnitureConfigSO;
            scriptableObject.Config = configData;
            AssetDatabase.CreateAsset(scriptableObject, $"{ScriptableObjectPath}/{Category}/{configData.Id}.asset");

            return scriptableObject;
        }

        public FurnitureController CreatePrefab(FurnitureConfigSO configSO)
        {
            // Create object
            var instance = new GameObject();
            instance.name = configSO.Config.FurnitureName;
            instance.tag = "Furniture";

            // Generate iso object controller
            var isoObject = instance.AddComponent<IsoObject>();
            isoObject.size = configSO.Config.Size;
            isoObject.renderersMode = IsoObject.RenderersMode.Mode3d;

            // Generate checkers
            var checkers = new List<SpriteRenderer>();
            var checkerContainer = new GameObject();
            checkerContainer.name = "Checker";
            checkerContainer.transform.SetParent(instance.transform);
            if (configSO.Config.FurnitureMoveType == FurnitureMoveType.Wall)
            {
                if (configSO.Config.BaseDirection == FurnitureDirection.Left)
                {
                    for (int x = 0; x < configSO.Config.Size.x; x++)
                    {
                        for (int z = 0; z < configSO.Config.Size.z; z++)
                        {
                            var newTile = Instantiate(LeftWallChecker, checkerContainer.transform);

                            /*var localPosX = ((x - 1) * 0.589f);
                            var localPosY = ((x + 1) * 0.336f) + (z * 2 * 0.336f);*/

                            var localPosX = (x - configSO.Config.Size.y) * 0.59f;
                            var localPosY = (x + configSO.Config.Size.y + 2 * z) * 0.3363f;

                            newTile.localPosition = new Vector3(localPosX, localPosY, 0);
                            checkers.Add(newTile.GetComponent<SpriteRenderer>());
                            newTile.gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    for (int y = 0; y < configSO.Config.Size.y; y++)
                    {
                        for (int z = 0; z < configSO.Config.Size.z; z++)
                        {
                            var newTile = Instantiate(RightWallChecker, checkerContainer.transform);

                            /*var localPosX = ((y - 1) * -0.589f);
                            var localPosY = (z * 2 * 0.336f) + ((y + 1) * 0.336f);*/

                            var localPosX = (configSO.Config.Size.x - y) * 0.59f;
                            var localPosY = (configSO.Config.Size.x + y + 2 * z) * 0.3363f;

                            newTile.localPosition = new Vector3(localPosX, localPosY, 0);
                            checkers.Add(newTile.GetComponent<SpriteRenderer>());
                            newTile.gameObject.SetActive(false);
                        }
                    }
                }
            }
            else
            {
                for (int x = 0; x < configSO.Config.Size.x; x++)
                {
                    for (int y = 0; y < configSO.Config.Size.y; y++)
                    {
                        var newTile = Instantiate(FloorChecker, checkerContainer.transform);

                        var tilePosX = (x - y) * 0.59f;
                        var tilePosY = (x + y) * 0.3363f;

                        newTile.localPosition = new Vector3(tilePosX, tilePosY);
                        checkers.Add(newTile.GetComponent<SpriteRenderer>());
                        newTile.gameObject.SetActive(false);
                    }
                }
            }

            // Create sprite renderer
            var rendererInstance = new GameObject();
            rendererInstance.transform.SetParent(instance.transform);
            rendererInstance.name = configSO.Config.FurnitureName + "SpriteRenderer";

            // Custom render order
            var renderer = rendererInstance.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = configSO.Config.IsCarpet
                ? Define.SortingOrder.CARPET
                : configSO.Config.IsWindow
                    ? Define.SortingOrder.WINDOW
                    : Define.SortingOrder.NORMAL;
            
            // Set pivot
            SetPivot(configSO.Config.Sprite,
                new Vector2(configSO.Config.Size.y / (configSO.Config.Size.x + configSO.Config.Size.y), 0));
            renderer.sprite = configSO.Config.Sprite;

            // Init controller
            var controller = instance.AddComponent<FurnitureController>();
            controller.OnCreate(configSO, renderer, checkerContainer.transform, checkers, _materialDic);

            // Create prefab.
            var prefab =
                PrefabUtility.SaveAsPrefabAsset(instance, $"{PrefabPath}/{Category}/{configSO.Config.Id}.prefab");
            DestroyImmediate(instance.gameObject);

            return prefab.GetComponent<FurnitureController>();
        }

        /// <summary>
        /// Set custom pivot for sprite by find it in assets.
        /// </summary>
        private void SetPivot(Sprite sprite, Vector2 pivot)
        {
            string path = AssetDatabase.GetAssetPath(sprite.texture);
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

            if (textureImporter == null) return;

            textureImporter.isReadable = true;

            TextureImporterSettings settings = new TextureImporterSettings();
            textureImporter.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = pivot;

            textureImporter.SetTextureSettings(settings);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
#endif
    }

    public enum EFurnitureMaterial
    {
        Default,
        OutlineBlue,
        OutlineGreen,
        OutlineRed,
        OutlineWhite,
        OutlineYellow,
    }
}
using DG.Tweening;
using NFramework;
using Redcode.Extensions;
using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace YoyoDesign
{
    public static class Utilities
    {
        public static Color ParseColor(string hexColor)
        {
            ColorUtility.TryParseHtmlString(hexColor, out var color);
            return color;
        }

        public static List<RewardData> ParseStringToRewardDatas(string dataString)
        {
            var result = new List<RewardData>();

            if (string.IsNullOrEmpty(dataString))
                return result;

            var datas = dataString.Split(";");
            foreach (var data in datas)
            {
                var splitDatas = data.Split("-");

                if (!Enum.TryParse<ERewardType>(splitDatas[0], out var rewardType))
                    return result;

                switch (rewardType)
                {
                    case ERewardType.Experience:
                        result.Add(new RewardData(ERewardType.Experience, int.Parse(splitDatas[2])));
                        break;
                    case ERewardType.Currency:
                        if (Enum.TryParse<ECurrencyType>(splitDatas[1], out var currencyType))
                        {
                            result.Add(new RewardData(ERewardType.Currency, currencyType, int.Parse(splitDatas[2])));
                        }
                        break;
                    default:
                        break;
                }
            }

            return result;
        }

        public static Vector3 ParseStringToVector3(string dataString)
        {
            if (string.IsNullOrEmpty(dataString))
            {
                return Vector3.zero;
            }

            var splitDatas = dataString.Split(",");
            return new Vector3(float.Parse(splitDatas[0]), float.Parse(splitDatas[1]), float.Parse(splitDatas[2]));
        }

        public static List<FurnitureOptionData> ParseStringToFurnitureOptions(string dataString, string delimeter)
        {
            var result = new List<FurnitureOptionData>();

            if (string.IsNullOrEmpty(dataString))
                return result;
            var furOptionsString = dataString.Split(delimeter);

            foreach (var data in furOptionsString)
            {
                var furnitureOptions = ParseStringToRoomFurnitureDatas(data, ";");
                var furOption = new FurnitureOptionData(furnitureOptions);
                result.Add(furOption);
            }

            return result;
        }

        public static List<RoomFurnitureData> ParseStringToRoomFurnitureDatas(string dataString, string delimeter)
        {
            var result = new List<RoomFurnitureData>();

            if (string.IsNullOrEmpty(dataString))
                return result;

            var splitData = dataString.Split(delimeter);
            foreach (var furOption in splitData)
            {
                var data = ParseStringToRoomFurnitureData(furOption);
                result.Add(data);
            }
            return result;
        }

        public static List<T> ParseStringToList<T>(string dataString, string delimeter)
        {
            var result = new List<T>();
            if (string.IsNullOrEmpty(dataString))
                return result;

            var splitData = dataString.Split(delimeter);
            foreach (var data in splitData)
            {
                result.Add((T)Convert.ChangeType(data, typeof(T)));
            }
            return result;
        }


        public static SerializableDictionaryBase<ELandNavigation, int> ParseStringToDictionary(string dataString, string delimeter)
        {
            var result = new SerializableDictionaryBase<ELandNavigation, int>();
            if (string.IsNullOrEmpty(dataString))
                return result;

            var splitData = dataString.Split(" ");
            foreach (var data in splitData)
            {
                var split = data.Split(delimeter);
                var key = (ELandNavigation)Enum.Parse(typeof(ELandNavigation), split[0]);
                var value = int.Parse(split[1]);
                result.Add(key, value);
            }
            return result;
        }

        public static RoomFurnitureData ParseStringToRoomFurnitureData(string dataString)
        {
            // Data format: mr_bed_2|5,8,0|Left|-1
            // (IdConfig)|(Vector3)|(BaseDirection)|(IndexParentInFurnitureList)

            RoomFurnitureData result = null;

            if (string.IsNullOrEmpty(dataString))
                return result;

            var delimeter = "|";
            var splitData = dataString.Split(delimeter);

            var configId = splitData.ElementAtOrDefault(0);
            var position = ParseStringToVector3(splitData.ElementAtOrDefault(1));
            var direction = Enum.TryParse<FurnitureDirection>(splitData.ElementAtOrDefault(2), out var dir) ? dir : FurnitureDirection.Left;
            var parentIndexInFurList = int.TryParse(splitData.ElementAtOrDefault(3), out var number) ? number : -1;
            result = new RoomFurnitureData(configId, position, direction, parentIndexInFurList);
            return result;
        }

        public static List<CharacterOutfitData> ParseStringToOutfitDatas(string outfit)
        {
            List<CharacterOutfitData> characterOutfitSaveDatas = new List<CharacterOutfitData>();

            string[] outfitId = outfit.Split(';');
            foreach (var item in outfitId)
            {
                CharacterOutfitData outfitSaveData = new CharacterOutfitData();
                if (Enum.TryParse(item.Split('_')[1].CapitalizeFirstChar(), out OutfitType outfitType))
                {
                    outfitSaveData.OutfitId = item;
                    outfitSaveData.OutfitType = outfitType;
                }
                characterOutfitSaveDatas.Add(outfitSaveData);
            }

            return characterOutfitSaveDatas;
        }

        public static T RandomEnumValue<T>()
        {
            var random = new System.Random();
            var values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(random.Next(values.Length));
        }

        public static void FitTo(this RectTransform child, RectTransform parent, float spacing)
        {
            Vector2 parentSize = parent.rect.size;
            // Calculate the aspect ratio of the original image
            float aspectRatio = child.sizeDelta.x / child.sizeDelta.y;

            // Calculate the new width and height based on the parent size and aspect ratio
            float newWidth = parentSize.x - spacing;
            float newHeight = newWidth / aspectRatio;

            // Check if the new height exceeds the parent height
            if (newHeight > parentSize.y - spacing)
            {
                // If so, recalculate the new height and width based on the parent height and aspect ratio
                newHeight = parentSize.y - spacing;
                newWidth = newHeight * aspectRatio;
            }

            // Update the size delta of the child RectTransform
            child.sizeDelta = new Vector2(newWidth, newHeight);
        }

        public static Vector3 WorldToCanvasPosition(this Canvas canvas, Vector3 worldPosition, Camera camera = null)
        {
            if (camera == null)
            {
                camera = Camera.main;
            }
            var viewportPosition = camera.WorldToViewportPoint(worldPosition);
            return canvas.ViewportToCanvasPosition(viewportPosition);
        }

        public static Vector3 ScreenToCanvasPosition(this Canvas canvas, Vector3 screenPosition)
        {
            var viewportPosition = new Vector3(screenPosition.x / Screen.width,
                                               screenPosition.y / Screen.height,
                                               0);
            return canvas.ViewportToCanvasPosition(viewportPosition);
        }

        public static Vector3 ViewportToCanvasPosition(this Canvas canvas, Vector3 viewportPosition)
        {
            var centerBasedViewPortPosition = viewportPosition - new Vector3(0.5f, 0.5f, 0);
            var canvasRect = canvas.GetComponent<RectTransform>();
            var scale = canvasRect.sizeDelta;
            return Vector3.Scale(centerBasedViewPortPosition, scale);
        }

        public static void PunchScalePopup(this Transform transform, Vector3 punch, float duration)
        {
            transform.SetLocalScale(1);
            transform.DOPunchScale(punch, duration);
        }

        public static void RotateScalePopup(this Transform transform, float duration)
        {
            transform.SetLocalScale(0);
            transform.SetEulerAnglesZ(-90);
            transform.DOScale(1, duration);
            transform.DORotate(Vector3.zero, duration);
        }

        public static void PlayDialogAnimation(this TextMeshProUGUI tmp, string message)
        {
            tmp.DOKill();
            tmp.text = "";
            tmp.DOText(message, Define.TimeLength.DIALOG_TIME)
                .SetEase(Ease.Linear);
        }

        public static bool TryGetKeyByValue<T, W>(this SerializableDictionaryBase<T, W> dictionary, W value, out T key)
        {
            key = default;
            foreach (KeyValuePair<T, W> pair in dictionary)
            {
                if (pair.Value.Equals(value))
                {
                    key = pair.Key;
                    return true;
                }
            }
            return false;
        }

        public static bool IsMouseWithinImage(RectTransform rectTransform, Vector3 mousePosition)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, mousePosition, null, out var localPoint);

            return rectTransform.rect.Contains(localPoint);
        }
    }
}

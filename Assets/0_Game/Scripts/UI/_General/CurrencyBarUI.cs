using DG.Tweening;
using Redcode.Extensions;
using RotaryHeart.Lib.SerializableDictionary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class CurrencyBarUI : MonoBehaviour
    {
        [SerializeField] private ECurrencyType _type;
        [SerializeField] private TextMeshProUGUI _txtAmount;
        [SerializeField] private Image _imgIcon;
        [SerializeField] private SerializableDictionaryBase<ECurrencyType, Sprite> _iconDic;
        [SerializeField] private bool _isListenToEvent = true;

        private void OnEnable()
        {
            _imgIcon.sprite = _iconDic[_type];
            UpdateAmountText();

            if (_isListenToEvent)
                UserData.OnUserCurrencyChanged += UserData_OnUserCurrencyChanged;
        }

        private void OnDisable()
        {
            if (_isListenToEvent)
                UserData.OnUserCurrencyChanged -= UserData_OnUserCurrencyChanged;
        }

        private void UserData_OnUserCurrencyChanged() => UpdateAmountText();

        private void UpdateAmountText() => _txtAmount.text = $"{UserData.Instance.GetCurrencyAmount(_type)}";

        public Vector3 GetIconPos() => _imgIcon.transform.position;
    }
}

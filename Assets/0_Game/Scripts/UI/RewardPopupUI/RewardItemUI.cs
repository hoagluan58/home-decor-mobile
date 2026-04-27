using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class RewardItemUI : MonoBehaviour
    {
        [SerializeField] private GameObject _goContainer;
        [SerializeField] private TextMeshProUGUI _tmpAmount;
        [SerializeField] private Image _imgReward;

        private RewardData _data;

        public void SetData(RewardData data)
        {
            _data = data;
            _goContainer.SetActive(_data != null);

            if (_data != null)
            {
                _imgReward.sprite = _data.icon;
                _tmpAmount.text = $"x{_data.amount}";
            }
        }

        public void SetData(string amount) => _tmpAmount.text = $"x{amount}";
    }
}

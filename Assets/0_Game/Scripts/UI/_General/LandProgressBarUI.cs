using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class LandProgressBarUI : MonoBehaviour
    {
        [SerializeField] private Image _progressBarIMG;
        [SerializeField] private TextMeshProUGUI _progressTMP;

        private LandProgress _landProgress;
        private LandConfigData _landConfig;

        private void OnEnable()
        {
            var landId = HomeLandData.Instance.CurrentLandId;
            _landProgress = HomeLandData.Instance.GetLandProgress(landId);
            if (_landProgress != null)
            {
                _landConfig = AllConfig.Instance.LandConfigDic[landId];
                UpdateProgressBar();
            }
        }

        private void UpdateProgressBar()
        {
            _progressTMP.text = $"{_landProgress.UnlockedDecorFurniture.Count}/{_landConfig.DecorFurnitureData.Count}";
            _progressBarIMG.fillAmount = (float)_landProgress.UnlockedDecorFurniture.Count / _landConfig.DecorFurnitureData.Count;
        }
    }
}

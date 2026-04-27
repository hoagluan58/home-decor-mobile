using EnhancedUI.EnhancedScroller;
using System.Collections.Generic;
using UnityEngine;

namespace YoyoDesign
{
    public class OutfitControllerView : MonoBehaviour, IEnhancedScrollerDelegate
    {
        #region PARAM

        private List<OutfitConfigData> _datas = new();
        [SerializeField] private EnhancedScroller _scroller;
        [SerializeField] private EnhancedScrollerCellView _cellViewPrefab;
        [SerializeField] private CharacterOutfitControllerUI _characterOutfit;
        [SerializeField] private int _numberOfCellsPerRow = 3;
        [SerializeField] private float _cellViewSize;
        #endregion

        #region UNITY METHODS

        void Start()
        {
            _scroller.Delegate = this;
        }

        #endregion

        #region FEATURE METHODS

        public void LoadData(List<OutfitConfigData> configDatas)
        {
            _datas.Clear();
            _datas = configDatas;
            _scroller.ReloadData();
        }

        #endregion

        #region IEnhancedScrollerDelegate METHODS
        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return Mathf.CeilToInt((float)_datas.Count / (float)_numberOfCellsPerRow);
        }
        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return 176;
        }
        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            OutfitCellView cellView = scroller.GetCellView(_cellViewPrefab) as OutfitCellView;

            cellView.SetData(_datas, dataIndex * _numberOfCellsPerRow, _numberOfCellsPerRow, _characterOutfit);

            return cellView;
        }
        #endregion
    }
}

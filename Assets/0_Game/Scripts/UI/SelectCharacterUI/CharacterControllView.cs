using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UIElements;
using Tween = EnhancedUI.EnhancedScroller.Tween;

namespace YoyoDesign
{
    public class CharacterControllView : MonoBehaviour, IEnhancedScrollerDelegate
    {
        #region PARAM

        [SerializeField] private List<OutfitConfigData> _datas = new();
        [SerializeField] private EnhancedScroller _scroller;
        public EnhancedScroller Scroller => _scroller;
        [SerializeField] private EnhancedScrollerCellView cellViewPrefab;
        [SerializeField] private int _curIndex = 0;
        public int CurIndex => _curIndex;
        #endregion

        #region UNITY METHODS
        void Start()
        {
            _scroller.Delegate = this;
            _scroller.scrollerSnapped = ScrollerSnapped;
            _curIndex = 0;
        }

        #endregion

        #region FEATURE METHODS
        public void LoadData(List<OutfitConfigData> configDatas)
        {
            _datas.Clear();
            _datas = configDatas;
            _scroller.ReloadData();
        }
        public void JumpToCharacterIndex(int index, float time = 0f)
        {
            if (_curIndex + index < 0 || _curIndex + index > 3) return;
            _scroller.JumpToDataIndex(_curIndex + index, 0.5f, 0.5f, true,
                EnhancedScroller.TweenType.easeInBack, time, () =>
                {
                    _curIndex += index;
                    Debug.Log(_curIndex);
                });
        }

        #endregion

        #region ENHANCED SCOLLER METHODS
        private void ScrollerSnapped(EnhancedScroller enhancedScroller, int cellindex, int dataindex, EnhancedScrollerCellView cellview)
        {
            _curIndex = dataindex;
            Debug.Log("snap:" + dataindex);
        }
        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return 4;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return Screen.width;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            CharacterCellView cellView = scroller.GetCellView(cellViewPrefab) as CharacterCellView;

            cellView.SetData();

            return cellView;
        }
        #endregion
    }
}

using EnhancedUI.EnhancedScroller;
using System.Collections.Generic;
using UnityEngine;

namespace YoyoDesign
{
    public class OutfitCellView : EnhancedScrollerCellView
    {
        #region PARAM

        [SerializeField] private OutfitItemUI _outfitItem;
        [SerializeField] private List<OutfitItemUI> _listOutfitItem = new();

        #endregion

        #region FEATURE METHODS

        public void SetData(List<OutfitConfigData> datas, int startingIndex, int maxListCount, CharacterOutfitControllerUI characterOutfit)
        {
            _listOutfitItem.ForEach(x => x.gameObject.SetActive(false));

            for(int i = 0; i < maxListCount; i++)
            {
                //IF item count equal max count => Only Init Data
                if(_listOutfitItem.Count >= maxListCount)
                {
                    if(i < _listOutfitItem.Count)
                    {
                        _listOutfitItem[i].gameObject.SetActive(startingIndex + i < datas.Count ? true : false);
                        _listOutfitItem[i].OnInit(startingIndex + i < datas.Count ? datas[startingIndex + i] : null, characterOutfit);
                    }
                }
                else
                {
                    OutfitItemUI outfitItem = Instantiate(_outfitItem, transform);
                    outfitItem.OnInit(startingIndex + i < datas.Count ? datas[startingIndex + i] : null, characterOutfit);
                    outfitItem.gameObject.SetActive(startingIndex + i < datas.Count ? true : false);
                    _listOutfitItem.Add(outfitItem);
                }
            }
        }

        #endregion

    }
}

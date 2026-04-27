using System.Collections.Generic;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;

namespace YoyoDesign
{
    public class HomeCharController : MonoBehaviour
    {
        [SerializeField] private SkeletonAnimation _charSpine;
        [SerializeField] private FurnitureController _furnitureController;

        [SerializeField] private List<AnimationReferenceAsset> _danceAnims;

        public FurnitureController FurnitureController => _furnitureController;

        public void LoadCharacter()
        {
            _charSpine.ApplyCharacterOutfit(DressUpData.Instance.GetCharacterOutfitSaveDatas());
            _charSpine.AnimationState.SetAnimation(0, _danceAnims[Random.Range(0, _danceAnims.Count)], true);
        }

        public void SetSize(Vector3 size) => _charSpine.transform.localScale = size;

        public void SetRenderOrder(int value)
        {
            _charSpine.gameObject.GetComponent<MeshRenderer>().sortingOrder = value;
        }
    }
}
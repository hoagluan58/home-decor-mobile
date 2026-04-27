using Cysharp.Threading.Tasks;
using DG.Tweening;
using JSAM;
using Redcode.Extensions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class OrderResultStarsGroupUI : MonoBehaviour
    {
        [SerializeField] private Sprite _spriteStarOn;
        [SerializeField] private Sprite _spriteStarOff;
        [SerializeField] private List<Image> _imgStarList;
        [SerializeField] private Transform _tfFlyStar;

        private Vector3 _flyStarSpawnPos;

        private void Awake() => _flyStarSpawnPos = _tfFlyStar.transform.position;

        public async void PlayAnim(int starCount)
        {
            _imgStarList.ForEach(x => x.sprite = _spriteStarOff);
            _tfFlyStar.gameObject.SetActive(true);

            for (var i = 0; i < starCount; i++)
            {
                var star = _imgStarList[i];

                _tfFlyStar.DOKill();
                _tfFlyStar.position = _flyStarSpawnPos;
                _tfFlyStar.SetLocalScaleZ(star.transform.localScale.z);

                await _tfFlyStar.DOMove(star.transform.position, 0.25f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    star.sprite = _spriteStarOn;
                    AudioManager.PlaySound(ESound.ReachStar);
                });
            }

            _tfFlyStar.gameObject.SetActive(false);
        }
    }
}

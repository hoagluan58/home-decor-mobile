using DG.Tweening;
using JSAM;
using NFramework;
using Redcode.Extensions;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class BotOrderNode : MonoBehaviour
    {
        [SerializeField] private GameObject _goContainer;
        [SerializeField] private Button _btnNode;
        [SerializeField] private TextMeshProUGUI _txtName;
        [SerializeField] private SkeletonGraphic _spineGraphicAvatar;

        public int OrderId => _botOrderData.OrderId;

        private BotConfigData _botData;
        private BotOrderData _botOrderData;

        private void Awake() => _btnNode.onClick.AddListener(OnButtonNodeClicked);

        private void OnDisable()
        {
            _goContainer.transform.DOKill();
        }

        private void OnButtonNodeClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            UIManager.Instance.Open<OrderNotePopupUI>(Define.UIName.ORDER_NOTE_POPUP).SetData(_botOrderData);
        }

        public void SetData(BotOrderData data)
        {
            _botOrderData = data;
            _goContainer.SetActive(_botOrderData != null);

            if (_botOrderData == null)
                return;

            this.gameObject.SetActive(_botOrderData.IsDone == false);
            _botData = BotManager.Instance.GetBotConfigData(data.BotId);
            _txtName.text = _botData.Name;
            _spineGraphicAvatar.ApplyCharacterOutfit(data.BotData.OutfitDatas);
            _goContainer.transform.SetLocalScale(1f);
            _goContainer.transform.DOScale(1.1f, 1).SetEase(Ease.OutQuad).SetLoops(-1, LoopType.Yoyo);
        }
    }
}

using JSAM;
using NFramework;
using UnityEngine;

namespace YoyoDesign
{
    public class TutorialPhase1PopupUI : BaseUIView
    {
        [SerializeField] private GameObject _goStep1Container;
        [SerializeField] private GameObject _goStep2Container;
        [SerializeField] private GameObject _goStep4Container;

        public override void OnOpen()
        {
            base.OnOpen();
            SetData();
            TutPhase1_Step1.OnNextStep += TutPhase1_Step1_OnNextStep;
            TutPhase1_Step2.OnNextStep += TutPhase1_Step2_OnNextStep;
            TutPhase1_Step4.OnNextStep += TutPhase1_Step4_OnNextStep;
        }

        public override void OnClose()
        {
            base.OnClose();
            TutPhase1_Step1.OnNextStep -= TutPhase1_Step1_OnNextStep;
            TutPhase1_Step2.OnNextStep -= TutPhase1_Step2_OnNextStep;
            TutPhase1_Step4.OnNextStep -= TutPhase1_Step4_OnNextStep;
        }

        private void TutPhase1_Step1_OnNextStep()
        {
            _goStep1Container.SetActive(false);
            _goStep2Container.SetActive(true);
        }

        private void TutPhase1_Step2_OnNextStep()
        {
            _goStep2Container.SetActive(false);
            _goStep4Container.SetActive(true);
        }

        private void TutPhase1_Step4_OnNextStep()
        {
            AudioManager.PlaySound(ESound.Click);
            _goStep4Container.SetActive(false);
            CloseSelf();
            var botData = new BotOrderData()
            {
                OrderId = Define.DefaultId.TUTORIAL_ORDER_ID,
                BotId = Define.DefaultId.TUTORIAL_BOT_ID,
                BotData = new BotConfigData()
                {
                    Id = Define.DefaultId.TUTORIAL_BOT_ID,
                    Name = "Jessica"
                },
                IsDone = false,
                NeedReloadRoomSprite = false,
                RoomId = Define.DefaultId.TUTORIAL_ORDER_ROOM_ID,
                RoomType = EBotRoomType.Bedroom,
            };
            OrderModeManager.Instance.TakeOrder(botData);
            if (UserData.Instance.GetCurrencyAmount(ECurrencyType.OrderCleanTrash) == 0)
            {
                UserData.Instance.ModifyCurrencyDic(ECurrencyType.OrderCleanTrash, 2);
            }
            FeatureNavigator.Instance.Go(EGameFeature.Order).Forget();
        }

        private void SetData()
        {
            _goStep1Container.SetActive(true);
            _goStep2Container.SetActive(false);
            _goStep4Container.SetActive(false);
        }
    }
}
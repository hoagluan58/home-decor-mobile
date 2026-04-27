using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using NFramework;

namespace YoyoDesign
{
    public class HomeV2SceneManager : MonoBehaviour
    {
        [Header("NPC")]
        [SerializeField] private List<HomeNPCController> _npcs;

        private void Start() => Init();

        private async void Init()
        {
            await UniTask.Yield();

            // Init npc
            foreach (var npc in _npcs)
            {
                npc.SetReward(false);
            }
            if (UserData.Instance.CanClaimNPCReward)
            {
                var randomNPc = _npcs.RandomItem();
                randomNPc.SetReward(true);
            }

            // Init order mode
            OrderModeManager.Instance.TryLoadMap();

            await UniTask.Yield();

            await ShowPopup();

            var isPlayTutorial = HomeV2TutorialController.Instance.CheckTutorial();

            if (!isPlayTutorial)
            {
                await UniTask.WaitUntil(() => LandController.Instance != null);
                LandController.Instance.OnSceneLoadCameraMovement();
            }
        }

        public async UniTask ShowPopup()
        {
            if (OrderModeManager.Instance.NeedShowRewardPopup)
            {
                var reward = OrderModeManager.Instance.LastestRewardData;
                var flyAnimationUI = UIManager.Instance.Open<FlyAnimationUI>(Define.UIName.FLY_ANIMATION);
                flyAnimationUI.PlayDiamondAnim(UserData.Instance.GetCurrencyAmount(ECurrencyType.Diamond), reward.DiamondReward);
                flyAnimationUI.PlayStarAnim(UserData.Instance.GetCurrencyAmount(ECurrencyType.Star), reward.StarReward);

                await UniTask.WaitUntil(() => flyAnimationUI == null || !flyAnimationUI.gameObject.activeSelf);

                if (reward.FurnitureReward == null)
                    return;

                var rewardData = new RewardData(ERewardType.Furniture, reward.FurnitureReward, 1);
                var popup = UIManager.Instance.Open<RewardFurniturePopupUI>(Define.UIName.REWARD_FURNITURE_POPUP);
                popup.SetData(rewardData);
                await popup.PlayAnimation();
            }
        }
    }
}

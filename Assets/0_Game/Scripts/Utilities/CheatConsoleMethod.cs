using IngameDebugConsole;
using NFramework;

namespace YoyoDesign
{
    public static class CheatConsoleMethod
    {
        [ConsoleMethod("cheat.diamond", "Cheat Diamond")]
        public static void CheatDiamond(float amount)
        {
            UserData.Instance.ModifyCurrencyDic(ECurrencyType.Diamond, amount);
            Logger.Log(null, $"Cheat Diamond: {amount}");
        }

        [ConsoleMethod("cheat.star", "Cheat Star")]
        public static void CheatStar(float amount)
        {
            UserData.Instance.ModifyCurrencyDic(ECurrencyType.Star, amount);
            Logger.Log(null, $"Cheat Star: {amount}");
        }

        [ConsoleMethod("cheat.goOrderRoom", "Go to order room")]
        public static void CheatGoOrderRoom(string roomId)
        {
            UIManager.Instance.CloseAllInLayer(EUILayer.Popup);
            UIManager.Instance.CloseAllInLayer(EUILayer.AlwaysOnTop);

            var botId = BotManager.Instance.GetRandomBotId();
            var orderRoomConfig = AllConfig.Instance.OrderModeRoomConfigDic[roomId];
            var orderData = new BotOrderData(-1, botId, orderRoomConfig.RoomType, roomId);
            OrderModeManager.Instance.TakeOrder(orderData);
            FeatureNavigator.Instance.Go(EGameFeature.Order).Forget();
            Logger.Log(null, $"Cheat Go Order Room: {roomId}");
        }

        [ConsoleMethod("cheat.skipTutorial", "Skip Tutorial")]
        public static void CheatSkipTutorial()
        {
            UserData.Instance.CurTutorialIndex = 99999;
            Logger.Log(null, "Cheat Skip Tutorial");
        }

        [ConsoleMethod("cheat.forceShowView", "Force Show UI")]
        public static void CheatForceShowView(string view)
        {
            UIManager.Instance.Open(view);
            Logger.Log(null, $"Cheat Force Show View: {view}");
        }

        [ConsoleMethod("cheat.unlockDecorLand", "Unlock Decor Land")]
        public static void CheatUnlockDecorCurLand(int landId)
        {
            HomeLandData.Instance.UnlockNewDecorFurniture(landId);
            Logger.Log(null, $"Cheat Unlock Decor Land: {landId}");
        }

        [ConsoleMethod("cheat.doneOrder", "Done Order")]
        public static void CheatDoneOrder()
        {
            UIManager.Instance.Close(Define.UIName.ORDER_PROGRESS_POPUP);
            UIManager.Instance.Open(Define.UIName.ORDER_RESULT_POPUP);
            Logger.Log(null, $"Cheat Done Order");
        }

        [ConsoleMethod("cheat.unlockAll", "Unlock All")]
        public static void CheatUnlockAll()
        {
            foreach (var landId in AllConfig.Instance.LandConfigDic.Keys)
            {
                var isHaveDecorFurniture = true;
                while (isHaveDecorFurniture)
                {
                    var fur = HomeLandData.Instance.UnlockNewDecorFurniture(landId);
                    if (fur == null)
                    {
                        isHaveDecorFurniture = false;
                    }
                }
                Logger.Log(null, $"Cheat unlock decor land: {landId}");
            }
        }
    }
}
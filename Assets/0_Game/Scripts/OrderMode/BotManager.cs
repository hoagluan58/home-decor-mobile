using Redcode.Extensions;

namespace YoyoDesign
{
    public class BotManager : Singleton<BotManager>
    {
        public BotConfigData GetBotConfigData(string id) => AllConfig.Instance.BotConfigDic[id];

        public string GetRandomBotId() => AllConfig.Instance.BotConfigDic.Keys.GetRandomElement();
    }
}

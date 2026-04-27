using System.Collections.Generic;
using System.Linq;

namespace YoyoDesign
{
    public class DressUpManager : SingletonMono<DressUpManager>
    {
        #region Get List ConfigData
        public List<OutfitConfigData> GetDresUpConfig()
        {
            return AllConfig.Instance.OutfitConfigDic.Values.ToList();
        }

        public List<OutfitConfigData> GetDressUpConfigByType(OutfitType type)
        {
            return AllConfig.Instance.OutfitConfigDic.Values.ToList().FindAll(x => x.OutfitType == type);
        }
        #endregion

        #region  Get Config Data
        public OutfitConfigData GetDressUpConfigData(string id)
        {
            AllConfig.Instance.OutfitConfigDic.TryGetValue(id, out var config);
            return config;
        }

        public OutfitConfigData GetDressUpConfigDataByType(OutfitType type, string id)
        {
            return AllConfig.Instance.OutfitConfigDic.Values.ToList().Find(x => x.OutfitType == type && x.Id == id);
        }
        #endregion
    }
}

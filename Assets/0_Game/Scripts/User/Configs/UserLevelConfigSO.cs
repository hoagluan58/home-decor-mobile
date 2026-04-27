using NFramework;
using System.Collections.Generic;

namespace YoyoDesign
{
    public class UserLevelConfigSO : GoogleSheetConfigSO<UserLevelConfigData>
    {
        private Dictionary<int, UserLevelConfigData> _userLevelConfigDic;
        public Dictionary<int, UserLevelConfigData> UserLevelConfigDic => _userLevelConfigDic;

        public void Init()
        {
            _userLevelConfigDic = new Dictionary<int, UserLevelConfigData>();

            foreach (var data in _datas)
            {
                _userLevelConfigDic[data.Level] = data;
            }
        }
    }

    [System.Serializable]
    public class UserLevelConfigData
    {
        public int Level;
        public float ExpNeed;
    }
}

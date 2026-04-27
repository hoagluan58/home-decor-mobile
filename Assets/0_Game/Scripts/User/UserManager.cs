using System.Linq;
using UnityEngine;

namespace YoyoDesign
{
    public class UserManager : Singleton<UserManager>
    {
        public UserLevelConfigData GetUserLevelConfigData(int level) => AllConfig.Instance.UserLevelConfigDic[level];

        public void CalculateLevelAndCurLevelExp(out int level, out float exp)
        {
            level = 1;
            exp = 0;

            var userTotalExp = UserData.Instance.TotalExp;
            var tempExp = 0f;

            foreach (var pair in AllConfig.Instance.UserLevelConfigDic)
            {
                tempExp += pair.Value.ExpNeed;
                if (tempExp < userTotalExp)
                {
                    continue;
                }
                else
                {
                    level = pair.Key;
                    exp = tempExp - userTotalExp == 0 ? 0 : Mathf.Abs(tempExp - userTotalExp - pair.Value.ExpNeed);
                    if (exp == 0 && !IsMaxLevel(level))
                    {
                        level++;
                    }
                    break;
                }
            }
        }

        public bool IsMaxLevel(int level)
        {
            var levelMax = AllConfig.Instance.UserLevelConfigDic.Keys.Last();
            return level >= levelMax;
        }
    }
}

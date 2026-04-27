using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YoyoDesign
{
    public class CharacterManager : Singleton<CharacterManager>
    {
        #region CONFIG DATA

        public CharacterConfigData GetCharacterData(string id)
        {
            return AllConfig.Instance.CharacterConfig[id];
        }

        #endregion

        #region CONFIG

        public List<CharacterConfigData> GetCharacterConfigDatas()
        {
            return AllConfig.Instance.CharacterConfig.Values.ToList();
        }

        #endregion
    }
}

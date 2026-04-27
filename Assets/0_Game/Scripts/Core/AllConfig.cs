using System.Collections.Generic;
using UnityEngine;

namespace YoyoDesign
{
    public class AllConfig : SingletonMono<AllConfig>
    {
        [SerializeField] private BotConfigSO _botConfigSO;
        [SerializeField] private BotHouseConfigSO _botHouseConfigSO;
        [SerializeField] private UserLevelConfigSO _userLevelConfigSO;
        [SerializeField] private LikeRewardConfigSO _likeRewardConfigSO;
        [SerializeField] private FloorConfigSO _floorConfigSO;
        [SerializeField] private WallConfigSO _wallConfigSO;
        [SerializeField] private OrderModeRoomConfigSO _orderModeRoomConfigSO;
        [SerializeField] private CharacterConfigSO _characterConfigSO;
        [SerializeField] private OutfitConfigSO _outfitConfigSO;
        [SerializeField] private LevelUpRewardConfigSO _levelUpRewardConfig;
        [SerializeField] private LandConfigSO _landConfigSO;

        public OutfitConfigSO SetOutfitConfigSo
        {
            set
            {
                _outfitConfigSO = value;
                _outfitConfigSO.Init();
            }
        }

        public Dictionary<string, BotConfigData> BotConfigDic => _botConfigSO.BotConfigDic;

        public Dictionary<string, BotHouseConfigData> BotHouseConfigDic => _botHouseConfigSO.BotHouseConfigDic;

        public Dictionary<int, UserLevelConfigData> UserLevelConfigDic => _userLevelConfigSO.UserLevelConfigDic;

        public Dictionary<string, LikeRewardConfigData> LikeRewardConfigDic => _likeRewardConfigSO.LikeRewardConfigDic;

        public Dictionary<string, FloorConfigData> FloorConfigDic => _floorConfigSO.FloorConfigDic;

        public Dictionary<string, WallConfigData> WallConfigDic => _wallConfigSO.WallConfigDic;

        public Dictionary<string, OrderModeRoomConfigData> OrderModeRoomConfigDic => _orderModeRoomConfigSO.OrderModeRoomConfigDic;

        public Dictionary<string, CharacterConfigData> CharacterConfig => _characterConfigSO.CharacterConfigDic;

        public Dictionary<string, OutfitConfigData> OutfitConfigDic => _outfitConfigSO.OutfitConfigDataDic;
        public Dictionary<int, LevelUpRewardsConfig> LevelUpRewardsConfigDic => _levelUpRewardConfig.LevelUpConfigDic;
        public Dictionary<int, LandConfigData> LandConfigDic => _landConfigSO.LandConfigDic;

        public void Init()
        {
            _botConfigSO.Init();
            _botHouseConfigSO.Init();
            _userLevelConfigSO.Init();
            _likeRewardConfigSO.Init();
            _floorConfigSO.Init();
            _wallConfigSO.Init();
            _orderModeRoomConfigSO.Init();
            _characterConfigSO.Init();
            _outfitConfigSO.Init();
            _levelUpRewardConfig.Init();
            _landConfigSO.Init();
        }
    }
}
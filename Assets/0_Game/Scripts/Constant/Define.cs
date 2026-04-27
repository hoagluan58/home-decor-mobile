using System.Collections.Generic;

public static class Define
{
    public static List<int> DEFAULT_LAND_UNLOCKED_LIST = new List<int>() { 0 };

    public class SceneName
    {
        public const string CORE = "Core";
        public const string HOME = "Home";
        public const string HOME_V2 = "HomeV2";
        public const string DECOR = "Decor";
        public const string DRESS_UP = "DressUp";
        public const string HOUSE_VISIT = "HouseVisit";
        public const string ORDER_MODE = "OrderMode";
        public const string DECOR_TUTOR = "DecorTutorial";
    }

    public class UIName
    {
        public const string LOADING = "LoadingUI";
        public const string INIT = "InitUI";
        public const string FLY_ANIMATION = "FlyAnimationUI";

        public const string HOME_MENU = "Menu/HomeMenuUI";
        public const string HOME_V2_MENU = "Menu/HomeV2MenuUI";
        public const string LAND_MENU = "Menu/LandMenuUI";
        public const string DRESS_UP = "Menu/DressUpMenuUI";
        public const string ORDER_MODE_MENU = "Menu/OrderModeMenuUI";
        public const string DECOR_MENU = "Menu/DecorMenuUI";
        public const string SELECTCHARACTER_MENU = "Menu/SelectCharacterMenuUI";

        public const string HOUSES_LIST_POPUP = "Popup/HouseListPopupUI";
        public const string HOUSE_INFO_POPUP = "Popup/HouseInfoPopupUI";
        public const string REWARD_POPUP = "Popup/RewardPopupUI";
        public const string CONFIRM_DELETE_ROOM_POPUP = "Popup/ConfirmDeleteRoomPopupUI";
        public const string CONFIRM_RETURN_HOME_POPUP = "Popup/ConfirmReturnHomePopupUI";
        public const string ORDER_NOTE_POPUP = "Popup/OrderNotePopupUI";
        public const string ORDER_PROGRESS_POPUP = "Popup/OrderProgressPopupUI";
        public const string ORDER_OPTION_POPUP = "Popup/OrderOptionPopupUI";
        public const string ORDER_RESULT_POPUP = "Popup/OrderResultPopupUI";
        public const string ORDER_HISTORY_POPUP = "Popup/OrderHistoryPopupUI";
        public const string TUTORIAL_POPUP = "Popup/TutorialPopupUI";
        public const string SETTINGS_POPUP = "Popup/SettingsPopupUI";
        public const string MESSAGE_POPUP = "Popup/MessagePopupUI";
        public const string BLOCK_POPUP = "Popup/BlockPopupUI";
        public const string LEVEL_UP_REWARD_POPUP = "Popup/LevelUpRewardPopupUI";
        public const string GAME_LOOP_POPUP = "Popup/GameLoopPopupUI";
        public const string REWARD_FURNITURE_POPUP = "Popup/RewardFurniturePopupUI";

        public const string NO_INTERNET_POPUP = "AlwaysOnTop/NoInternetPopupUI";
        public const string RATE_US_POPUP = "AlwaysOnTop/RateUsPopupUI";

        // Tutorial Phase
        public const string TUTORIAL_PHASE_1_POPUP = "Popup/TutorialPhase/TutorialPhase1PopupUI";
        public const string TUTORIAL_PHASE_2_POPUP = "Popup/TutorialPhase/TutorialPhase2PopupUI";
    }

    public class TimeLength
    {
        public const float INIT = 3f;
        public const float LOADING_ANIM = 0.3f;
        public const float UI_ANIM = 0.5f;
        public const float MOVE_FUR_DELAY = 0.2f;
        public const float DIALOG_TIME = 1.5f;
    }

    public class Size
    {
        public const float HOME_CAMERA_SIZE = 25;
        public const float LAND_CAMERA_SIZE = 12;
        public const float DECOR_CAMERA_SIZE = 19;
        public const float ORDER_CAMERA_SIZE = 17;
        public const float SQUARE_CAMERA_SIZE = 17;
        public const float ORDER_REMOVE_TRASH_SIZE = 10;
    }

    public class SortingOrder
    {
        public const int WALL = -5;
        public const int FLOOR = 0;
        public const int CARPET = 13;
        public const int WINDOW = 14;
        public const int NORMAL = 15;
        public const int CHECKER = 16;
        public const int HIGH_LIGHT = 20;
    }

    public class SortingLayerName
    {
        public const string Default = "Default";
        public const string AlwaysOnTop = "AlwaysOnTop";
    }

    public class DefaultId
    {
        public const string WALL = "nr1_wall_2";
        public const string LOCK_WALL = "lock_room";
        public const string FLOOR = "nr1_ground_5";
        public const string TRASH_BOX = "trashbox";
        public const string DECOR_BOX = "decor_furniture_box";

        // Order
        public const int TUTORIAL_ORDER_ID = -999;
        public const string TUTORIAL_BOT_ID = "-999";
        public const string TUTORIAL_ORDER_ROOM_ID = "1";
        public const int TUTORIAL_FOCUS_LAND_ID = 0;
        public const int TUTORIAL_UNLOCK_LAND_ID = 1;
    }

    public class ColorCode
    {
        public const string WHITE = "#FFFFFF";
        public const string RED = "#FF0000";
        public const string DEEP_CHESTNUT = "#501D00";
        public const string DARK_GRAY = "#323232";
    }

    public class ConstValue
    {
        public const int ORDER_REWARD_MULTIPLY = 3;
        public const int ORDER_REWARD_4_STAR = 79;
        public const int ORDER_REWARD_5_STAR = 100;
        public const int ORDER_TRASH_TYPE = 2;
        public const int ORDER_TRASH_TYPE_TUTORIAL = 1;
        public const int ORDER_TRASH_AMOUNT = 6;
        public const int ORDER_TRASH_AMOUNT_TUTORIAL = 3;
        public const int HISTORY_LIST_LENGTH = 30;
    }

    public class TutorialIndex
    {
        public const int NEW_PLAYER = 0;
        public const int DONE_INTRO = 1;
        public const int DONE_ORDER = 2;
        public const int DONE_DECOR_FURNITURE = 3;
        public const int DONE_UNLOCK_FURNITURE = 4;
        public const int DONE_UNLOCK_LAND = 5;
        public const int FUTURE_UPDATE = 6;
    }
}
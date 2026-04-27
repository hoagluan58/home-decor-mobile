using Cysharp.Threading.Tasks;
using UnityEngine;

namespace YoyoDesign
{
    public class DecorRoomManager : SingletonMono<DecorRoomManager>
    {
        [SerializeField] private DecorRoomController _firstRoom;
        [SerializeField] private DecorRoomController _secondRoom;

        private bool _canSwitch;
        private DecorRoomController _curRoom;
        private int _curRoomIndex;

        public DecorRoomController CurRoom => _curRoom;
        public int CurRoomIdnex => _curRoomIndex;

        private void Start() => Init().Forget();

        private async UniTaskVoid Init()
        {
            await UniTask.Yield();
            
            _firstRoom.Init();
            _secondRoom.Init(); 

            _curRoomIndex = 0;
            _curRoom = _firstRoom;
            _firstRoom.Enter(() => _canSwitch = true);

            HouseVisitData.Instance.ClaimBonusLike();
        }

        public void UpFloor()
        {
            if (!_canSwitch) return;
            _canSwitch = false;
            _curRoomIndex = 0;
            _curRoom = _firstRoom;
            _secondRoom.Exit();
            _firstRoom.Enter(() => _canSwitch = true);
        }

        public void DownFloor()
        {
            if (!_canSwitch) return;
            _canSwitch = false;
            _curRoomIndex = 1;
            _curRoom = _secondRoom;
            _firstRoom.Exit();
            _secondRoom.Enter(() => _canSwitch = true);
        }

        public void SaveAllRoomData()
        {
            _curRoom.DataController.SaveTemp();
            _firstRoom.DataController.Save();
            _secondRoom.DataController.Save();
        }

        public void ReleaseAllRoomFurniture()
        {
            _firstRoom.FurnitureController.ReleaseAll();
            _secondRoom.FurnitureController.ReleaseAll();
        }
    }
}
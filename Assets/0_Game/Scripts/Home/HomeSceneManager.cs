using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NFramework;
using UnityEngine;

namespace YoyoDesign
{
    public class HomeSceneManager : SingletonMono<HomeSceneManager>
    {
        [Header("ROOMS")]
        [SerializeField] private HomeRoomController _secondFloorRoom;
        [SerializeField] private HomeRoomController _firstFloorRoom;

        [Header("CHARACTER")]
        [SerializeField] private HomeCharController _character;

        [Header("NPC")]
        [SerializeField] private List<HomeNPCController> _npcs;

        public HomeRoomController FirstFloorRoom => _firstFloorRoom;
        public HomeRoomController SecondFloorRoom => _secondFloorRoom;

        private void Start() => Init().Forget();

        private async UniTaskVoid Init()
        {
            await UniTask.Yield();
            // Init room
            _firstFloorRoom.LoadRoomData();
            _secondFloorRoom.LoadRoomData();

            // Init character
            _character.LoadCharacter();
            switch (Random.Range(1, 4))
            {
                case 1:
                    PutCharacterOnFirstRoom();
                    break;
                case 2:
                    PutCharacterOnSecondRoom();
                    break;
                case 3:
                    PutCharacterOutsize();
                    break;
                default:
                    PutCharacterOutsize();
                    break;
            }

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
        }

        public void PutCharacterOnSecondRoom()
        {
            if (!UserData.Instance.IsUnlockSecondFloor || !_secondFloorRoom.PutCharacterInside(_character))
            {
                PutCharacterOnFirstRoom();
            }
        }

        public void PutCharacterOnFirstRoom()
        {
            if (!_firstFloorRoom.PutCharacterInside(_character))
            {
                PutCharacterOutsize();
            }
        }

        public void PutCharacterOutsize()
        {
            _character.transform.position = new Vector3(0, -4, 0);
            _character.SetSize(Vector3.one * 0.35f);
            _character.SetRenderOrder(200);
        }
    }
}
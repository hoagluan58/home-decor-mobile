using System.Collections.Generic;
using IsoTools;
using JSAM;
using NFramework;
using Redcode.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YoyoDesign
{
    public class DecorFurnitureController : MonoBehaviour
    {
        [Title("CONTROLLERS")]
        [SerializeField] protected IsoWorld _isoWorld;
        [SerializeField] protected DecorRoomController _decorController;
        [SerializeField] protected DecorHistoryController _historyController;

        [Title("FURNITURE")]
        [SerializeField] private FurnitureOptionsPopup _furnitureOptionsPopup;
        [SerializeField] private List<FurnitureController> _furnitureList;
        [SerializeField] private ParticleSystem _removeFx;

        [Title("CONFIG")]
        [InfoBox("Room bounds = Room Size - 1")]
        [SerializeField] private Vector3 _roomBounds;

        private FurnitureController _curFurniture;
        private float _touchDownTime;

        public List<FurnitureController> FurList => _furnitureList;

        [Title("DEBUG"), ShowInInspector, ReadOnly]
        public FurnitureController CurFurniture
        {
            get => _curFurniture;
            set
            {
                if (value == _curFurniture) return;
                if (value == null)
                {
                    _curFurniture = value;
                    _furnitureOptionsPopup.Release();
                    NormalizeAllFurniture();
                    _historyController.OnModify();
                }
                else
                {
                    _curFurniture = value;
                    _furnitureOptionsPopup.Follow(_curFurniture);
                    HighlightCurrentFurniture();
                }

                return;

                void HighlightCurrentFurniture()
                {
                    AudioManager.PlaySound(ESound.PickUp);
                    foreach (var fur in _furnitureList)
                    {
                        fur.VisualController.Normalize();
                        fur.VisualController.FadeOut();
                    }
                    _curFurniture.VisualController.Highlight();
                }

                void NormalizeAllFurniture()
                {
                    foreach (var fur in _furnitureList)
                    {
                        fur.VisualController.Normalize();
                    }
                }
            }
        }

        protected void Update()
        {
            if (UIManager.Instance.IsPopupShown()) return;
                
            if (Input.touchCount <= 0) return;
            var inputPosition = _isoWorld.TouchIsoPosition(0);

            // Begin select handle
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                _touchDownTime = 0;
                if (IsTouchOnDecorOptionButton(Input.GetTouch(0))) return;

                var furOnTouch = RoomHelper.GetFurnitureOnTouch(inputPosition, _roomBounds, _furnitureList, CurFurniture);
                
                if (CurFurniture != null && furOnTouch == null)
                {
                    VibrationManager.Vibrate(0.05f);
                    AudioManager.PlaySound(ESound.Drop);
                }
                CurFurniture = furOnTouch;
            }

            _touchDownTime += Time.deltaTime;

            if (CurFurniture == null || _touchDownTime <= Define.TimeLength.MOVE_FUR_DELAY) return;

            if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                OnMoveFurniture(inputPosition, _curFurniture);
            }

            if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                OnDropFurniture(_curFurniture);
            }
        }

        public bool IsTouchOnDecorOptionButton(Touch touch)
        {
            var pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = touch.position
            };

            var raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);

            foreach (var result in raycastResults)
            {
                switch (result.gameObject.tag)
                {
                    case "DecorMode/DeleteButton":
                        AudioManager.PlaySound(ESound.Remove);
                        _removeFx.transform.position = _isoWorld.IsoToScreen(CurFurniture.Position + CurFurniture.Size / 2);
                        _removeFx.Play();
                        ReleaseFurniture(CurFurniture);
                        return true;
                    case "DecorMode/FlipButton":
                        FlipCurrent();
                        return true;
                    case "DecorMode/ConfirmButton":
                        CurFurniture = null;
                        VibrationManager.Vibrate(0.05f);
                        AudioManager.PlaySound(ESound.Drop);
                        return true;
                }
            }

            return false;
        }

        private void OnMoveFurniture(Vector3 inputPosition, FurnitureController furMove)
        {
            RoomHelper.RemoveNested(furMove.NestedController.Parent, furMove);
            furMove.FlipController.AutoFlip(inputPosition.x > inputPosition.y ? FurnitureDirection.Right : FurnitureDirection.Left);
            furMove.MoveController.SetPosition(RoomHelper.GetMovePosition(inputPosition, furMove, _roomBounds));
            furMove.VisualController.IsValid = RoomHelper.IsPositionValid(furMove.Position, furMove.Size, furMove, _furnitureList, _roomBounds);
        }

        private void OnDropFurniture(FurnitureController furDrop)
        {
            if (furDrop.VisualController.IsValid)
            {
                // If current furniture can place other -> check if furniture is on surface of any. -> Nest them.
                if (furDrop.Config.CanPlaceOnOthers)
                {
                    var parentFur = RoomHelper.GetFurnitureOnSurface(furDrop.Position, furDrop.Size, furDrop, _furnitureList);
                    if (parentFur != null)
                    {
                        RoomHelper.NestFurniture(parentFur, furDrop, true);
                        furDrop.VisualController.IsValid = true;
                    }
                }
            }
            else
            {
                PlaceToValidPosition(furDrop);
                furDrop.VisualController.IsValid = true;
            }
        }

        private bool PlaceToValidPosition(FurnitureController curFur)
        {
            var (newPosition, newDirection, newParent) = RoomHelper.GetFurnitureValidPlace(curFur, _roomBounds, _furnitureList);

            if (newPosition == Vector3.back)
            {
                ReleaseFurniture(curFur);
                return false;
            }

            curFur.MoveController.SetPosition(newPosition);

            if (newDirection != curFur.CurDirection)
            {
                curFur.FlipController.FlipTo(newDirection);
                curFur.FlipController.FlipChild();
            }

            RoomHelper.RemoveNested(curFur.NestedController.Parent, curFur);
            if (newParent != null)
            {
                RoomHelper.NestFurniture(newParent, curFur, false);
                curFur.NestedController.UpdateLocalPosition();
            }

            curFur.VisualController.IsValid = true;
            return true;
        }

        public void ReleaseFurniture(FurnitureController furRelease, bool isShowFx = false)
        {
            if (furRelease == CurFurniture) CurFurniture = null;

            // Remove parent
            RoomHelper.RemoveNested(furRelease.NestedController.Parent, furRelease);

            // Remove all childs
            var childs = furRelease.NestedController.Childs;
            foreach (var child in childs)
            {
                _furnitureList.Remove(child);
                FurnitureManager.Instance.Release(child);
            }

            _furnitureList.Remove(furRelease);
            FurnitureManager.Instance.Release(furRelease);
        }

        public void SpawnFurniture(string furId)
        {
            CurFurniture = null;
            var furInstance = FurnitureManager.Instance.Get(furId);
            furInstance.transform.SetParent(_isoWorld.transform);
            furInstance.gameObject.SetActive(true);
            furInstance.MoveController.SetPosition(Vector3.zero);

            if (_furnitureList.Count >= 150)
            {
                AudioManager.PlaySound(ESound.Error);
                var messagePopup = UIManager.Instance.Open<MessagePopupUI>(Define.UIName.MESSAGE_POPUP);
                messagePopup.SetData("Notification", "Oops, your room seems so cramped. You should go another room to decorate.");
                FurnitureManager.Instance.Release(furInstance);
                return;
            }
            
            if (PlaceToValidPosition(furInstance))
            {
                _furnitureList.Add(furInstance);
                furInstance.VisualController.PunchScale();
                CurFurniture = furInstance;
            }
            else
            {
                AudioManager.PlaySound(ESound.Error);
                var messagePopup = UIManager.Instance.Open<MessagePopupUI>(Define.UIName.MESSAGE_POPUP);
                messagePopup.SetData("Out Of Space!", "There's no more space here. You can decorate another room.");
                FurnitureManager.Instance.Release(furInstance);
            }
        }

        #region OPTION MENU BUTTONS

        public void FlipCurrent()
        {
            if (CurFurniture == null) return;

            // If furniture has no parent.
            if (CurFurniture.NestedController.Parent == null)
            {
                var newValidPos = RoomHelper.GetValidPosition(CurFurniture.Position, CurFurniture.Size.GetYXZ(), CurFurniture.CurDirection,
                    CurFurniture, _roomBounds, _furnitureList);
                if (newValidPos != Vector3.back)
                {
                    AudioManager.PlaySound(ESound.Rotate);
                    CurFurniture.FlipController.FlipNegative();
                    PlaceToValidPosition(CurFurniture);
                }
                else
                {
                    AudioManager.PlaySound(ESound.Error);
                    CurFurniture.VisualController.PunchScale();
                }
            }
            else // If furniture has parent.
            {
                var curFurnitureParent = CurFurniture.NestedController.Parent;
                var newPos = RoomHelper.GetValidPositionOnParentSurface(
                    CurFurniture.Position,
                    CurFurniture.Size.GetYXZ(),
                    CurFurniture,
                    curFurnitureParent);

                if (newPos != Vector3.back)
                {
                    CurFurniture.FlipController.FlipNegative();
                    CurFurniture.MoveController.SetPosition(newPos);
                    CurFurniture.NestedController.UpdateLocalPosition();
                }
                else
                {
                    CurFurniture.VisualController.PunchScale();
                }
            }
        }

        #endregion


        public void ReleaseAll()
        {
            CurFurniture = null;
            foreach (var fur in _furnitureList)
            {
                fur.NestedController.OnRelease();
                FurnitureManager.Instance.Release(fur);
            }
            _furnitureList.Clear();
        }

        [Button]
        public FurnitureController SpawnFurnitureByData(RoomFurnitureData furnitureData)
        {
            var furInstance = FurnitureManager.Instance.Get(furnitureData.ConfigId);
            furInstance.gameObject.SetActive(true);
            furInstance.transform.SetParent(_isoWorld.transform);
            furInstance.FlipController.FlipTo(furnitureData.Direction);
            furInstance.MoveController.SetPosition(furnitureData.Position);
            furInstance.VisualController.Normalize(false);
            _furnitureList.Add(furInstance);
            return furInstance;
        }
    }
}

public enum RoomPlace
{
    LeftWall,
    RightWall,
    Floor
}
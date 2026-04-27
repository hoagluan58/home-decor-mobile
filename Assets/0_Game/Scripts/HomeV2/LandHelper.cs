using Redcode.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YoyoDesign
{
    public static class LandHelper
    {
        public static HomeLand GetLandOnTouch(Vector3 input, List<HomeLand> landList)
        {
            // Fix offset
            input.x -= 0.5f;
            input.y -= 0.5f;

            foreach (var land in landList)
            {
                var bounds = land.Bounds;
                var offsetRoomHeight = GetOffsetRoomHeight(bounds.Max, land.RoomSize.z);
                var isOutOfBounds = input.x < bounds.Min.x || input.y < bounds.Min.y || input.x > offsetRoomHeight.x || input.y > offsetRoomHeight.y;

                if (isOutOfBounds)
                    continue;

                return land;
            }

            return null;
        }

        public static List<Vector3> GetFurniturePositionList(Vector3 basePos, Vector3 furSize)
        {
            var result = new List<Vector3>();
            for (var x = basePos.x; x < basePos.x + furSize.x - 1; x++)
            {
                for (var y = basePos.y; y < basePos.y + furSize.y - 1; y++)
                {
                    result.Add(new Vector3(x, y, 0));
                }
            }
            return result;
        }

        public static Vector3 GetOffsetFurniturePos(Vector3 baseFurPos, Vector2 landPos)
        {
            var offsetX = baseFurPos.x + (landPos.x + landPos.y) * 0.5f;
            var offsetY = baseFurPos.y + (landPos.x - landPos.y) * -0.5f;

            return new Vector3(offsetX, offsetY, baseFurPos.z);
        }

        public static Vector3 GetBaseFurniturePos(Vector3 offsetFurPos, Vector2 landPos)
        {
            var baseX = offsetFurPos.x - (landPos.x + landPos.y) * 0.5f;
            var baseY = offsetFurPos.y - (landPos.x - landPos.y) * -0.5f;

            return new Vector3(baseX, baseY, offsetFurPos.z);
        }

        public static FurnitureController GetFurnitureOnTouch(Vector3 input, Vector3 minBounds, Vector3 maxBounds, Vector3 roomSize,
            List<FurnitureController> furnitureList, FurnitureController currentFurniture)
        {
            // Fix offset
            input.x -= 0.5f;
            input.y -= 0.5f;

            if (input.x > maxBounds.x || input.y > maxBounds.y || input.x < minBounds.x || input.y < minBounds.y)
            {
                if (input.x > maxBounds.x || input.x < minBounds.x)
                {
                    if (input.x > maxBounds.x)
                    {
                        input.z = input.x - maxBounds.x;
                        input.y -= input.z;
                        input.x = maxBounds.x;
                    }
                    else
                    {
                        input.z = minBounds.x - input.x;
                        input.y += input.z;
                        input.x = minBounds.x;
                    }
                }
                else if (input.y > maxBounds.y || input.y < minBounds.y)
                {
                    if (input.y > maxBounds.y)
                    {
                        input.z = input.y - maxBounds.y;
                        input.x -= input.z;
                        input.y = maxBounds.y;
                    }
                    else
                    {
                        input.z = minBounds.y - input.y;
                        input.x += input.z;
                        input.y = minBounds.y;
                    }
                }
            }

            var listFurOnTouch = new List<FurnitureController>();
            for (var z = input.z; z < roomSize.z; z++)
            {
                input = ConvertPosition(input, z);
                listFurOnTouch.AddRange(furnitureList.Where(fur => IsOverlap(input, Vector3.one, fur.Position, fur.Size)));
            }

            if (currentFurniture != null)
            {
                return listFurOnTouch.Contains(currentFurniture) ? currentFurniture : null;
            }
            else
            {
                return listFurOnTouch.OrderByDescending(fur => fur.Position.z).FirstOrDefault();
            }
        }

        public static Vector3 GetMovePosition(Vector3 inputPosition, FurnitureController furMove, Vector3 maxBounds,
                                              Vector3 minBounds, float snapValue = 1f, float moveOffset = 2f)
        {
            var movePosition = inputPosition;

            movePosition.x = Mathf.Round(movePosition.x / snapValue) * snapValue + moveOffset;
            movePosition.y = Mathf.Round(movePosition.y / snapValue) * snapValue + moveOffset;

            switch (furMove.Config.FurnitureMoveType)
            {
                case FurnitureMoveType.Floor:
                    {
                        movePosition.z = 0;
                    }
                    break;
                case FurnitureMoveType.Wall:
                    {
                        var normalizedPosition = NormalizedInputPositionWithBounds(movePosition, maxBounds, minBounds);
                        if (normalizedPosition.x > normalizedPosition.y)
                        {
                            movePosition.z = movePosition.x - maxBounds.x;
                            movePosition.y -= movePosition.z;
                            movePosition.x = maxBounds.x + 1 - furMove.Size.x;
                        }
                        else
                        {
                            movePosition.z = movePosition.y - maxBounds.y;
                            movePosition.x -= movePosition.z;
                            movePosition.y = maxBounds.y + 1 - furMove.Size.y;
                        }
                    }
                    break;
                case FurnitureMoveType.Flexible:
                    {
                        if (movePosition.x > maxBounds.x || movePosition.y > maxBounds.y || movePosition.x < minBounds.x || movePosition.y < minBounds.y)
                        {
                            if (Mathf.Abs(movePosition.x - minBounds.x) > Mathf.Abs(movePosition.y - minBounds.y))
                            {
                                movePosition.z = movePosition.x > maxBounds.x ? movePosition.x - maxBounds.x : movePosition.x - minBounds.x;
                                movePosition.y -= movePosition.z;
                            }
                            else
                            {
                                movePosition.z = movePosition.y > maxBounds.y ? movePosition.y - maxBounds.y : movePosition.y - minBounds.y;
                                movePosition.x -= movePosition.z;
                            }
                        }
                    }
                    break;
            }

            movePosition.x = Mathf.Clamp(movePosition.x, minBounds.x, maxBounds.x + 1 - furMove.Size.x);
            movePosition.y = Mathf.Clamp(movePosition.y, minBounds.y, maxBounds.y + 1 - furMove.Size.y);
            movePosition.z = Mathf.Clamp(movePosition.z, minBounds.z, maxBounds.z + 1 - furMove.Size.z);
            return movePosition;
        }

        public static Vector2 GetOffsetRoomHeight(Vector3 maxBounds, float roomZ) => new Vector2(maxBounds.x + roomZ, maxBounds.y + roomZ);

        public static Vector3 ConvertPosition(Vector3 oldPosition, float z)
        {
            Vector3 convertedPosition;
            if (oldPosition.z == z) return oldPosition;
            var offsetZ = Mathf.Abs(oldPosition.z - z);
            if (oldPosition.z < z)
            {
                convertedPosition = new Vector3(
                    oldPosition.x - offsetZ,
                    oldPosition.y - offsetZ,
                    z);
            }
            else
            {
                convertedPosition = new Vector3(
                    oldPosition.x + offsetZ,
                    oldPosition.y + offsetZ,
                    z);
            }
            return convertedPosition;
        }

        public static bool IsOverlap(Vector3 position, Vector3 size, FurnitureController furCheck, List<FurnitureController> furnitureList)
        {
            // Check nested first
            foreach (var otherFur in furnitureList)
            {
                // Not count current fur
                if (otherFur == furCheck) continue;


                if (furCheck.Config.CanBePlaceOn)
                {
                    if (furCheck.NestedController.Childs.Contains(otherFur)) continue;
                }

                if (furCheck.Config.CanPlaceOnOthers)
                {
                    if (otherFur.NestedController.Childs.Contains(furCheck)) continue;
                }

                // Carpet check
                if (furCheck.Config.IsCarpet != otherFur.Config.IsCarpet) continue;

                if (IsOverlap(
                        position,
                        size,
                        otherFur.Position,
                        otherFur.Size
                    )) return true;
            }
            return false;
        }

        public static bool IsOverlap(Vector3 pos1, Vector3 size1, Vector3 pos2, Vector3 size2)
        {
            return pos1.x < pos2.x + size2.x && pos1.x + size1.x > pos2.x &&
                   pos1.y < pos2.y + size2.y && pos1.y + size1.y > pos2.y &&
                   pos1.z < pos2.z + size2.z && pos1.z + size1.z > pos2.z;
        }

        public static bool IsPositionValid(Vector3 position, Vector3 size, FurnitureController furCheck,
            List<FurnitureController> furnitureList, Vector3 maxBounds, Vector3 minBounds, bool isCheckNested = true, float floorZPos = 0)
        {
            if (furCheck.Config.CanPlaceOnOthers && isCheckNested)
            {
                var parentFur = GetFurnitureOnSurface(position, size, furCheck, furnitureList);
                if (parentFur != null)
                {
                    var convertedPos = ConvertPosition(position, parentFur.SurfacePos.z);
                    // Check is overlap with any child or any other fur
                    var isOverlapWithAnyChildOfParent = parentFur.NestedController.Childs.Any(child =>
                        IsOverlap(convertedPos, size, child.Position, child.Size));
                    var isOverlapWithAnyOtherFur = IsOverlap(convertedPos, size, furCheck, furnitureList.Where(f => !f.Equals(parentFur)).ToList());

                    return !isOverlapWithAnyChildOfParent && !isOverlapWithAnyOtherFur;
                }
            }

            // Check is childs overlap any other fur
            if (furCheck.Config.CanBePlaceOn)
            {
                if (furCheck.NestedController.Childs.Any(child => IsOverlap(position - child.LocalPosition, child.Size, child, furnitureList)))
                {
                    return false;
                }
            }

            // Check overlap any fur.
            if (IsOverlap(position, size, furCheck, furnitureList))
                return false;


            // Check position
            if (furCheck.Config.FurnitureMoveType == FurnitureMoveType.Wall)
            {
                var limit = maxBounds + Vector3.one - size;
                return position.x <= limit.x && position.y <= limit.y && position.z <= limit.z;
            }
            else
            {
                var limit = maxBounds + Vector3.one - size;
                limit.z = floorZPos;
                return position.x <= limit.x && position.y <= limit.y && position.z <= limit.z;
            }
        }

        public static Vector3 NormalizedInputPositionWithBounds(Vector3 inputPosition, Vector3 maxBounds, Vector3 minBounds)
            => new((inputPosition.x - minBounds.x) / (maxBounds.x - minBounds.x),
                   (inputPosition.y - minBounds.y) / (maxBounds.y - minBounds.y),
                   inputPosition.z);

        public static FurnitureController GetFurnitureOnSurface(Vector3 furniturePosition, Vector3 size, FurnitureController furCheck,
            List<FurnitureController> furnitureList)
        {
            foreach (var fur in furnitureList)
            {
                if (fur == furCheck) continue;
                if (!fur.Config.CanBePlaceOn) continue;

                var convertedPos = ConvertPosition(furniturePosition, fur.SurfacePos.z);
                if (IsFullyOnTop(convertedPos, size, fur.SurfacePos, fur.SurfaceSize))
                {
                    return fur;
                }
            }
            return null;
        }

        /// <summary>
        /// Find the best place for a furniture.
        /// </summary>
        /// <returns>(New position, new direction, new parent)</returns>
        public static (Vector3, FurnitureDirection, FurnitureController) GetFurnitureValidPlace(FurnitureController fur, Vector3 minBounds, Vector3 maxBounds,
            List<FurnitureController> furnitureList, bool isFlip = true)
        {
            FurnitureController newParent = null;
            var newPosition = Vector3.back;
            var currentDirection = fur.FlipController.CurDirection;
            var newDirection = currentDirection;
            var negativeDirection = currentDirection == FurnitureDirection.Left ? FurnitureDirection.Right : FurnitureDirection.Left;

            // Check current direction is valid
            newPosition = GetValidPosition(fur.Position, fur.Size, currentDirection, fur, minBounds, maxBounds, furnitureList);

            if (newPosition == Vector3.back) // If invalid with current direction -> Negative the direction and check again.
            {
                if (isFlip)
                {
                    newDirection = negativeDirection;
                    newPosition = GetValidPosition(fur.Position, fur.Size.GetYXZ(), negativeDirection, fur, minBounds, maxBounds, furnitureList);
                }
            }

            // If current furniture can be a child -> check a surface
            if (newPosition == Vector3.back && fur.Config.CanPlaceOnOthers)
            {
                newDirection = currentDirection;
                (newPosition, newParent) = GetParentHasValidSurface(fur.Position, fur.Size, fur, furnitureList);

                // If current direction has no valid parent -> try flip and find again.
                if (newPosition == Vector3.back)
                {
                    if (isFlip)
                    {
                        newDirection = negativeDirection;
                        (newPosition, newParent) = GetParentHasValidSurface(fur.Position, fur.Size.GetYXZ(), fur, furnitureList);
                    }
                }
            }

            return (newPosition, newDirection, newParent);
        }

        public static Vector3 GetValidPosition(Vector3 furPos, Vector3 furSize, FurnitureDirection furDirection, FurnitureController fur,
            Vector3 minBounds, Vector3 maxBounds, List<FurnitureController> furList, float withZ = 0)
        {
            var floorPosition = Vector3.back;
            float minDistance = 1000;

            List<Vector3> listPositionToCheck;
            if (fur.Config.FurnitureMoveType == FurnitureMoveType.Wall)
            {
                listPositionToCheck = furDirection == FurnitureDirection.Left
                    ? GetLeftWallPositionList(minBounds, maxBounds, furSize)
                    : GetRightWallPositionList(minBounds, maxBounds, furSize);
            }
            else
            {
                listPositionToCheck = GetFloorPositionList(minBounds, maxBounds, furSize, withZ);
            }

            foreach (var pos in listPositionToCheck)
            {
                if (!IsPositionValid(pos, furSize, fur, furList, maxBounds, minBounds, false, withZ)) continue;
                var distance = Vector3.Distance(furPos, pos);
                if (distance >= minDistance) continue;
                minDistance = distance;
                floorPosition = pos;
            }

            return floorPosition;
        }

        public static (Vector3, FurnitureController) GetParentHasValidSurface(Vector3 furPos, Vector3 furSize, FurnitureController furCheck,
            List<FurnitureController> otherFur)
        {
            foreach (var parentFur in otherFur)
            {
                if (!parentFur.Config.CanBePlaceOn) continue;
                if (parentFur == furCheck) continue;
                if (parentFur.Config.IsCarpet) continue;

                var validPositionOnParentSurface = GetValidPositionOnParentSurface(furPos, furSize, furCheck, parentFur);
                if (validPositionOnParentSurface == Vector3.back) continue;

                return (validPositionOnParentSurface, parentFur);
            }

            return (Vector3.back, null);
        }

        public static Vector3 GetValidPositionOnParentSurface(Vector3 childPos, Vector3 childSize, FurnitureController child,
            FurnitureController parent)
        {
            var surfacePosList = GetSurfacePositionList(childSize, parent.SurfaceSize, parent.SurfacePos);
            var result = Vector3.back;
            var minDistance = 100f;
            // Search for list position in surface.
            foreach (var surfacePos in surfacePosList)
            {
                // If current position not overlap with any other child -> continue.s
                if (parent.NestedController.Childs.Any(otherChild =>
                        // Not current child
                        otherChild != child
                        // And overlap
                        && IsOverlap(surfacePos, childSize, otherChild.Position, otherChild.Size)
                    )) continue;

                // Return the valid position.
                var distance = Vector3.Distance(surfacePos, childPos);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    result = surfacePos;
                }
            }

            return result;
        }

        public static List<Vector3> GetFloorPositionList(Vector3 minBounds, Vector3 maxBounds, Vector3 furSize, float withZ = 0)
        {
            var result = new List<Vector3>();
            for (float x = minBounds.x; x <= (maxBounds.x + 1) - furSize.x; x++)
            {
                for (float y = minBounds.y; y <= (maxBounds.y + 1) - furSize.y; y++)
                {
                    result.Add(new Vector3(x, y, withZ));
                }
            }
            return result;
        }

        public static List<Vector3> GetLeftWallPositionList(Vector3 minBounds, Vector3 maxBounds, Vector3 furSize)
        {
            var result = new List<Vector3>();

            for (var z = 0; z <= maxBounds.z - furSize.z + 1; z++)
            {
                for (var x = maxBounds.x - furSize.x + 1; x >= minBounds.x; x--)
                {
                    result.Add(new Vector3(x, maxBounds.y - furSize.y + 1, z));
                }
            }
            return result;
        }

        public static List<Vector3> GetRightWallPositionList(Vector3 minBounds, Vector3 maxBounds, Vector3 furSize)
        {
            var result = new List<Vector3>();

            for (var z = 0; z <= maxBounds.z - furSize.z + 1; z++)
            {
                for (var y = maxBounds.y - furSize.y + 1; y >= minBounds.y; y--)
                {
                    result.Add(new Vector3(maxBounds.x - furSize.x + 1, y, z));
                }
            }

            return result;
        }

        public static List<Vector3> GetSurfacePositionList(Vector3 childSize, Vector3 parentSurfaceSize,
            Vector3 parentSurfacePosition)
        {
            var result = new List<Vector3>();
            for (float x = parentSurfacePosition.x;
                 x <= parentSurfacePosition.x + parentSurfaceSize.x - childSize.x;
                 x++)
            {
                for (float y = parentSurfacePosition.y;
                     y <= parentSurfacePosition.y + parentSurfaceSize.y - childSize.y;
                     y++)
                {
                    result.Add(new Vector3(x, y, parentSurfacePosition.z));
                }
            }
            return result;
        }

        public static bool IsFullyOnTop(Vector3 pos1, Vector3 size1, Vector3 pos2, Vector3 size2)
        {
            return pos1.x >= pos2.x && pos1.x + size1.x <= pos2.x + size2.x &&
                   pos1.y >= pos2.y && pos1.y + size1.y <= pos2.y + size2.y &&
                   pos1.z == pos2.z + size2.z;
        }
    }
}

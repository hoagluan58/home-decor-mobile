using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YoyoDesign
{
    public static class AStarPathfinding
    {
        private static Vector2Int[] directions = { new Vector2Int(1, 0),
                                            new Vector2Int(-1, 0),
                                            new Vector2Int(0, 1),
                                            new Vector2Int(0, -1)};

        public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int target, HashSet<Vector2Int> obstacles)
        {
            var openSet = new PriorityQueue<AStarNode>((a, b) => a.F.CompareTo(b.F));
            var closedSet = new HashSet<Vector2Int>();

            AStarNode startNode = new AStarNode(start);
            openSet.Enqueue(startNode);

            while (openSet.Count > 0)
            {
                AStarNode currentNode = openSet.Dequeue();

                if (currentNode.Position == target)
                {
                    return RetracePath(currentNode);
                }

                closedSet.Add(currentNode.Position);

                foreach (var direction in directions)
                {
                    Vector2Int neighborPos = currentNode.Position + direction;

                    if (closedSet.Contains(neighborPos) || obstacles.Contains(neighborPos))
                        continue;

                    int tentativeG = currentNode.G + 1;

                    AStarNode neighborNode = new AStarNode(neighborPos)
                    {
                        Parent = currentNode,
                        G = tentativeG,
                        H = Heuristic(neighborPos, target)
                    };

                    openSet.Enqueue(neighborNode);
                }
            }

            return null; // No path found
        }

        private static int Heuristic(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y); // Manhattan Distance
        }

        private static List<Vector2Int> RetracePath(AStarNode endNode)
        {
            var path = new List<Vector2Int>();
            AStarNode currentNode = endNode;

            while (currentNode != null)
            {
                path.Add(currentNode.Position);
                currentNode = currentNode.Parent;
            }

            path.Reverse();
            return path;
        }
    }
}

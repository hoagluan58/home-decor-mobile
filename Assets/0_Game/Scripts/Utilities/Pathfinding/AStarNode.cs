using System;
using System.Collections.Generic;
using UnityEngine;

namespace YoyoDesign
{
    public class AStarNode
    {
        public Vector2Int Position { get; set; }
        public AStarNode Parent { get; set; }
        public int G { get; set; } // Cost from start to current node
        public int H { get; set; } // Heuristic cost from current node to target

        public int F => G + H; // Total cost

        public AStarNode(Vector2Int position)
        {
            Position = position;
        }
    }

    public class PriorityQueue<T>
    {
        private List<T> elements = new List<T>();
        private Comparison<T> comparison;

        public PriorityQueue(Comparison<T> comparison)
        {
            this.comparison = comparison;
        }

        public void Enqueue(T item)
        {
            elements.Add(item);
            elements.Sort(comparison);
        }

        public T Dequeue()
        {
            T item = elements[0];
            elements.RemoveAt(0);
            return item;
        }

        public int Count => elements.Count;
    }
}

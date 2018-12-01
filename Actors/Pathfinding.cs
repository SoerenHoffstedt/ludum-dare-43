using Barely.Util.Priority_Queue;
using LD43.World;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD43.Actors
{
    public static class Pathfinding
    {
        public static Tile[,] Tiles;

        public static List<Point> AStar<T>(T[,] map, Point source, Point target, Func<T, bool> IsWalkable, Func<T, float> WalkCost, Func<int, int, IEnumerable<Point>> IterateNeighours, bool reversePath = true) where T : class
        {
            Dictionary<Point, Point> prev = new Dictionary<Point, Point>();
            Dictionary<Point, float> cost = new Dictionary<Point, float>();

            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    Point n = new Point(x, y);
                    if (n != source)
                    {
                        cost.Add(n, float.MaxValue);
                        prev.Add(n, new Point(-1, -1));
                    }
                }
            }

            bool targetReached = false;

            var closedList = new HashSet<Point>();
            var openList = new SimplePriorityQueue<Point>();
            openList.Enqueue(source, 0);
            cost[source] = 0;
            prev[source] = new Point(-1, -1);

            while (openList.Count > 0)
            {
                Point currentNode = openList.Dequeue();

                if (currentNode == target)
                {
                    targetReached = true;
                    break;
                }

                if (IsWalkable(map[currentNode.X, currentNode.Y]))
                {
                    closedList.Add(currentNode);
                    foreach (Point n in IterateNeighours(currentNode.X, currentNode.Y))
                    {
                        if (closedList.Contains(n))
                            continue;

                        float tenativeCost = cost[currentNode] + WalkCost(map[currentNode.X, currentNode.Y]);

                        bool contains = openList.Contains(n);
                        if (contains && tenativeCost >= cost[n])
                            continue;

                        prev[n] = currentNode;
                        cost[n] = tenativeCost;

                        tenativeCost += (target - n).ToVector2().Length();

                        if (contains)
                            openList.UpdatePriority(n, tenativeCost);
                        else
                            openList.Enqueue(n, tenativeCost);

                    }
                }

            }

            if (!targetReached)
                return null;

            List<Point> path = new List<Point>();
            Point run = target;
            path.Add(run);
            while (run != source)
            {
                run = prev[run];
                path.Add(run);
            }

            path.Reverse();
            return path;
        }

        private static bool IsInRange(int x, int y)
        {
            return x > 0 && y > 0 && x < Map.Size.X && y < Map.Size.Y;
        }

        public static IEnumerable<Point> IterateNeighboursFourDir(int x, int y)
        {
            if (IsInRange(x - 1, y))
                yield return new Point(x - 1, y);
            if (IsInRange(x + 1, y))
                yield return new Point(x + 1, y);
            if (IsInRange(x, y - 1))
                yield return new Point(x, y - 1);
            if (IsInRange(x, y + 1))
                yield return new Point(x, y + 1);
        }

    }
}

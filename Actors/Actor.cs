using Barely.Util;
using LD43.Scenes;
using LD43.World;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD43.Actors
{
    public class Actor
    {
        private static Point drawOffset = new Point(0, -4);        
        private float posX, posY;
        public float tileWalkDuration = 0.5f;
        public Task currTask = null;
        public Sprite sprite;
        public Path currPath = null;
        private int pathIndex;

        public Actor(Point pos)
        {            
            posX = pos.X;
            posY = pos.Y;
            sprite = Assets.OtherSprites["unitTemp"];
        }

        public bool IsBusy()
        {
            return currTask != null || currPath != null;
        }

        public void GoToTask(Task task)
        {
            currPath = new Path(this, task.tile, () => StartTask(task));
            StartPath();
        }

        public void StartTask(Task task)
        {
            currTask = task;
            task.Start(this);
            currPath = null;
        }

        public Point DrawPosition()
        {
            return new Point((int)(posX + 0.5f), (int)(posY + 0.5f)) + drawOffset;
        }

        public void StartPath()
        {
            List<Point> waypoints = currPath.waypoints;
            pathIndex = 0;

            if (waypoints.Count == 0)
            {
                currPath.OnReach();
                currPath = null;
                return;
            }

            if (waypoints[0] == currPath.target)
            {
                if (waypoints.Count == 1)
                {
                    currPath.OnReach();
                    currPath = null;
                    return;
                }
                else
                    pathIndex = 1;
            }
            Point t = waypoints[pathIndex] * Map.TileSize;
            GameScene.Tweener.Tween(this, new { posX = t.X, posY = t.Y }, tileWalkDuration).OnComplete(ReachedNextTile);


        }

        protected void ReachedNextTile()
        {
            var waypoints = currPath.waypoints;
            if (pathIndex == waypoints.Count - 1)
            {
                currPath.OnReach();
                currPath = null;
            }
            else
            {
                //tilePosition = currentPath[pathIndex];
                pathIndex++;
                Point target = waypoints[pathIndex] * Map.TileSize;                
                GameScene.Tweener.Tween(this, new { posX = target.X, posY= target.Y }, tileWalkDuration).OnComplete(ReachedNextTile);
            }
        }

    }
}

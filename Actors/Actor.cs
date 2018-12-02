using Barely.Util;
using LD43.Scenes;
using LD43.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD43.Actors
{
    public class Actor
    {
        GameScene game;

        private static Point drawOffset = new Point(0, -4);        
        private float posX, posY;
        public float tileWalkDuration = 0.5f;
        public Task currTask = null;        
        public Path currPath = null;
        private int pathIndex;
        public bool Dead = false;

        Sprite actorWalking;
        Sprite actorIdle;
        Sprite actorCelebrating;
        Sprite actorSelection;
        Sprite actorWorking;

        public Actor(Point pos, GameScene game)
        {
            this.game = game;
            posX = pos.X;
            posY = pos.Y;

            actorWalking        = Assets.OtherSprites["actorWalking"].CopyInstance();
            actorIdle           = Assets.OtherSprites["actorIdle"].CopyInstance();
            actorCelebrating    = Assets.OtherSprites["actorCelebrating"].CopyInstance();
            actorSelection      = Assets.OtherSprites["actorSelection"].CopyInstance();
            actorWorking        = Assets.OtherSprites["actorWorking"].CopyInstance();
        }

        public bool IsBusy()
        {
            return currTask != null || currPath != null;
        }

        public void GoToTask(Task task)
        {
            currPath = new Path(this, task.tile, task);
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

        public void Render(SpriteBatch spriteBatch, bool sacrificeSelection, bool sacrificeCelebrating)
        {
            Point drawPos = DrawPosition();

            if (sacrificeSelection)
                actorCelebrating.Render(spriteBatch, drawPos);
            else if (sacrificeCelebrating)
                actorCelebrating.Render(spriteBatch, drawPos);
            else if (currTask != null)
            {
                if (currTask.type == TaskType.Idle)
                    actorIdle.Render(spriteBatch, drawPos);
                else if (currTask.type == TaskType.Construction)
                    actorWorking.Render(spriteBatch, drawPos);
            }
            else if (currPath != null)
            {
                actorWalking.Render(spriteBatch, drawPos);
            }
            else
                actorIdle.Render(spriteBatch, drawPos);

        }

        public void StartPath()
        {
            List<Point> waypoints = currPath.waypoints;
            pathIndex = 0;

            if(waypoints == null || waypoints.Count == 0)
            {            
                //abort the task                
                game.ReQueueTask(currPath.taskToStartOnReach);
                currPath.taskToStartOnReach = null;
                currPath = null;
                return;
            }

            if (waypoints[0] == currPath.target)
            {
                if (waypoints.Count == 1)
                {
                    StartTask(currPath.taskToStartOnReach);
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
                StartTask(currPath.taskToStartOnReach);
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

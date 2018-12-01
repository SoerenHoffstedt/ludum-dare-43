using LD43.World;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD43.Actors
{
    public class Task
    {
        public Tile tile;
        public Point position;
        public Action<Actor> OnStart;
        public Action<Actor> OnCompletion;        
        public Func<bool> CheckFinish;
        private Actor workedBy = null;


        public Task(Tile tile, Point position, Func<bool> CheckFinish, Action<Actor> OnStart, Action<Actor> OnCompletion)
        {
            this.tile = tile;
            this.position       = position;
            this.CheckFinish    = CheckFinish;            
            this.OnStart        = OnStart;
            this.OnCompletion   = OnCompletion;
        }

        public void Start(Actor startedBy)
        {
            OnStart(startedBy);
            workedBy = startedBy;
        }

        public bool UpdateTask(float dt)
        {
            if (CheckFinish())
            {
                OnCompletion(workedBy);
                workedBy = null;
                return true;
            }
            return false;
        }

    }
}

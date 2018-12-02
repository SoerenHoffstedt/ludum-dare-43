using LD43.World;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD43.Actors
{

    public enum TaskType
    {
        Idle,
        Construction
    }

    public class Task
    {        
        public TaskType type; //for debug
        public Tile tile;        
        public Action<Actor> OnStart;
        public Action<Actor> OnCompletion;        
        public Func<bool> CheckFinish;
        private Actor workedBy = null;
        private HashSet<Task> activeTasks;
        public bool MarkedForDeletion { get; private set; } = false;

        public Task(Tile tile, Func<bool> CheckFinish, Action<Actor> OnStart, Action<Actor> OnCompletion, HashSet<Task> activeTasks, TaskType type)
        {
            this.type = type;
            this.tile = tile;            
            this.CheckFinish    = CheckFinish;            
            this.OnStart        = OnStart;
            this.OnCompletion   = OnCompletion;            
            this.activeTasks    = activeTasks;
        }

        public void Start(Actor startedBy)
        {
            activeTasks.Add(this);
            OnStart(startedBy);
            workedBy = startedBy;
        }

        public bool UpdateTask(float dt)
        {
            if (CheckFinish())
            {
                OnCompletion(workedBy);
                MarkedForDeletion = true;
                workedBy.currTask = null;
                workedBy = null;
                return true;
            }
            return false;
        }        

        public void StopWork()
        {
            OnCompletion(workedBy);
        }

    }
}

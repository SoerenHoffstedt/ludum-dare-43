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
        public Point position;
        public float length;
        public readonly float totalLength;
        public Action OnCompletion;
        public Action<float> OnUpdate;
        private float lastPercentage;

        public Task(Point position, float length, Action OnCompletion, Action OnUpdate)
        {
            this.position       = position;
            this.length         = length;
            this.totalLength    = length;
            this.OnCompletion   = OnCompletion;
        }

        public bool UpdateTask(float dt)
        {
            length -= dt;
            if (length <= 0f)
            {
                OnCompletion();
                return true;
            }
            if (OnUpdate != null)
            {
                //calc what percentage is finished and only call OnUpdate when its a bigger percentage than before.
                float perc = 1f -  (length / totalLength);
                if(lastPercentage < perc)
                {
                    OnUpdate(perc);
                    lastPercentage = perc;
                }
            }            
            return false;
        }

    }
}

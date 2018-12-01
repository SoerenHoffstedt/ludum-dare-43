using LD43.World;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD43.Actors
{
    public class Path
    {

        public List<Point> waypoints;
        public Point target;
        public Point source;
        public Action OnReach;
        public Actor actor;

        public int Count { get { return waypoints.Count; } }

        public Path(Actor actor, Tile target, Action OnReach)
        {
            this.actor = actor;
            this.source = actor.DrawPosition() / Map.TileSize;
            this.target = target.coord;
            this.OnReach = OnReach;
            waypoints = Pathfinding.AStar(Pathfinding.Tiles,
                                          this.source,
                                          this.target,
                                          (t) => t.IsWalkable(),
                                          (t) => 1f,
                                          Pathfinding.IterateNeighboursFourDir);
                                        

        }




    }
}

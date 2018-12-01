using Barely.Util;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD43.World
{
    public class Tile
    {
        public Point coord;
        public bool Exists = true;
        public Building building;

        public ResourceType resourceType;
        public int resourceAmount;

        public Sprite sprite;

        public Tile(Point coord)
        {            
            this.coord = coord;
            building = null;
            resourceType = ResourceType.NoneCount;
            resourceAmount = 0;
        }        

        public void PlaceBuilding(BuildingBlueprint blueprint)
        {
            building = new Building(blueprint);
        }

    }
}

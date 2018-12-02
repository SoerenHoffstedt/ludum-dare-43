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

        public Sprite spriteTile;
        public Sprite spriteResource;

        public Tile(Point coord)
        {            
            this.coord = coord;
            building = null;
            resourceType = ResourceType.NoneCount;
            resourceAmount = 0;
        }                

        public bool CanPlaceBuilding(BuildingBlueprint blueprint)
        {
            if(blueprint.type == BuildingType.Destruction)
            {
                return building != null;
            }
            if (building != null)
                return false;
            if (blueprint.type == BuildingType.Production && resourceType != blueprint.produces)
                return false;
            if (resourceType != ResourceType.NoneCount && blueprint.type != BuildingType.Production)
                return false;
            return true;
        }

        public bool IsWalkable()
        {
            return Exists && building == null;
        }

    }
}

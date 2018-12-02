using Barely.Util;
using LD43.Actors;
using LD43.Scenes;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LD43.World
{

    public enum BuildingType
    {
        Production,
        Hut,
        ReproductionCave,
        Destruction //hack to allow having a blueprint, check if this is placed and than destroy the building instead.
    }

    public class Building
    {
        public Point coord;
        public BuildingBlueprint blueprint;
        public float constructionTimer;
        public float productionTimer;
        public Actor workedBy = null;

        public Building(Point coord, BuildingBlueprint blueprint)
        {
            this.coord = coord;
            this.blueprint = blueprint;
            productionTimer = blueprint.productionTime;
            constructionTimer = blueprint.constructionTime;
        }

        public bool IsBuilt()
        {
            return constructionTimer == 0f;
        }

        internal bool IsWorked()
        {
            return workedBy != null;
        }

        public bool StartingWorkBy(Actor actor)
        {
            if (workedBy != null)
                return false;

            workedBy = actor;
            return true;
        }

        public void StopWorkBy(Actor actor)
        {
            if (actor != workedBy)
                throw new ArgumentException("Actor working this building does not equal passed worker.");
            workedBy = null;
        }

        public void FinishWork(GameScene game)
        {            
            //workedBy = null;
            if(blueprint.type == BuildingType.ReproductionCave)
            {
                game.ReproductionCaveFinished(this);
            }
        }
    }

    public class BuildingBlueprint
    {
        private const int POP_PER_HUT = 2;

        public string name;
        public BuildingType type;
        public ResourceType produces;
        public int productionAmount;
        public float productionTime;

        public float constructionTime;

        public int[] buildingCosts;

        public Sprite sprite;
        public Sprite spriteIcon;

        public BuildingBlueprint(XmlNode xmlDef)
        {
            name                = xmlDef.Attributes["name"].Value;
            type                = (BuildingType)Enum.Parse(typeof(BuildingType), xmlDef.Attributes["type"].Value);
            produces            = (ResourceType)Enum.Parse(typeof(ResourceType), xmlDef.Attributes["produces"].Value);
            productionAmount    = int.Parse(xmlDef.Attributes["productionAmount"].Value);
            productionTime      = float.Parse(xmlDef.Attributes["productionTime"].Value);
            constructionTime    = float.Parse(xmlDef.Attributes["constructionTime"].Value);
            buildingCosts       = new int[(int)ResourceType.NoneCount];
            foreach(XmlNode costNode in xmlDef.SelectNodes("buildingCosts/cost"))
            {
                ResourceType costType = (ResourceType)Enum.Parse(typeof(ResourceType), costNode.Attributes["resource"].Value);
                int costAmount = int.Parse(costNode.Attributes["amount"].Value);
                buildingCosts[(int)costType] = costAmount;
            }
            string spriteName = xmlDef.Attributes["sprite"].Value;
            sprite = Assets.OtherSprites[spriteName];
            spriteIcon = Assets.OtherSprites[spriteName + "Icon"];
        }

        public void OnPlacement(GameScene game)
        {
            for(int i = 0; i < (int)ResourceType.NoneCount; ++i)
            {                
                game.resources[i] -= buildingCosts[i];
            }
            if(type == BuildingType.Hut)
            {
                game.populationMax += POP_PER_HUT;
            }
        }

        public bool GameHasEnoughResources(GameScene game)
        {
            for (int i = 0; i < (int)ResourceType.NoneCount; ++i)
            {
                if (game.resources[i] < buildingCosts[i])
                    return false;
            }
            return true;
        }

        public void OnDestruction(GameScene game)
        {

            if (type == BuildingType.Hut)
            {
                game.populationMax -= POP_PER_HUT;
            }
        }
    }
}

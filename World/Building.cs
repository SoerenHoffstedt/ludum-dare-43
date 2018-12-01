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
        Main
    }

    public class Building
    {
        BuildingBlueprint blueprint;
        public float timer;

        public Building(BuildingBlueprint blueprint)
        {
            this.blueprint = blueprint;
        }

    }

    public class BuildingBlueprint
    {
        public string name;
        public BuildingType type;
        public ResourceType produces;
        public int productionAmount;
        public float productionTime;

        public int[] buildingCosts;

        public BuildingBlueprint(XmlNode xmlDef)
        {
            name                = xmlDef.Attributes["name"].Value;
            type                = (BuildingType)Enum.Parse(typeof(BuildingType), xmlDef.Attributes["type"].Value);
            produces            = (ResourceType)Enum.Parse(typeof(ResourceType), xmlDef.Attributes["resource"].Value);
            productionAmount    = int.Parse(xmlDef.Attributes["productionAmount"].Value);
            productionTime      = float.Parse(xmlDef.Attributes["productionTime"].Value);            
            buildingCosts       = new int[(int)ResourceType.NoneCount];
            foreach(XmlNode costNode in xmlDef.SelectNodes("buildingCosts/cost"))
            {
                ResourceType costType = (ResourceType)Enum.Parse(typeof(ResourceType), xmlDef.Attributes["resource"].Value);
                int costAmount = int.Parse(xmlDef.Attributes["amount"].Value);
                buildingCosts[(int)costType] = costAmount;
            }
        }
    }
}

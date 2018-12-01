using Barely.Util;
using LD43.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LD43
{
    public static class Assets
    {
        public static Texture2D atlas;
        public static Sprite[] TileSprites;
        public static Dictionary<ResourceType, Sprite> ResourceSprites;
        public static Dictionary<ResourceType, Sprite> ResourceIcons;

        public static Dictionary<string, Sprite> OtherSprites;

        public static void Load(ContentManager Content)
        {
            XmlDocument def = new XmlDocument();
            def.Load("Content/def.xml");

            atlas = Content.Load<Texture2D>("graphics");
            LoadTiles(def.SelectSingleNode("definitions/tiles"));
            LoadSprites(def);
        }

        private static void LoadSprites(XmlDocument def)
        {
            LoadResourceSprites(def.SelectSingleNode("definitions/graphics/resources"));
            LoadIconSprites(def.SelectSingleNode("definitions/graphics/icons"));

            var nodeList = def.SelectNodes("definitions/graphics/sprites/sprite");
            OtherSprites = new Dictionary<string, Sprite>(nodeList.Count);

            foreach(XmlNode spriteNode in nodeList)
            {
                string name = spriteNode.Attributes["name"].Value;
                int w = int.Parse(spriteNode.Attributes["w"].Value);
                int h = int.Parse(spriteNode.Attributes["h"].Value);
                int x = int.Parse(spriteNode.Attributes["x"].Value);
                int y = int.Parse(spriteNode.Attributes["y"].Value);
                Sprite sp = new Sprite(atlas, new Rectangle(x, y, w, h));
                OtherSprites.Add(name, sp);
            }

        }        

        private static void LoadResourceSprites(XmlNode node)
        {
            var nodeList = node.SelectNodes("sprite");
            ResourceSprites = new Dictionary<ResourceType, Sprite>(nodeList.Count);
            foreach (XmlNode spriteNode in nodeList)
            {
                ResourceType type = (ResourceType)Enum.Parse(typeof(ResourceType), spriteNode.Attributes["name"].Value);
                int w = int.Parse(spriteNode.Attributes["w"].Value);
                int h = int.Parse(spriteNode.Attributes["h"].Value);
                int x = int.Parse(spriteNode.Attributes["x"].Value);
                int y = int.Parse(spriteNode.Attributes["y"].Value);
                Sprite sp = new Sprite(atlas, new Rectangle(x, y, w, h));
                ResourceSprites.Add(type, sp);
            }
        }

        private static void LoadIconSprites(XmlNode node)
        {
            var nodeList = node.SelectNodes("sprite");
            ResourceIcons = new Dictionary<ResourceType, Sprite>(nodeList.Count);
            foreach (XmlNode spriteNode in nodeList)
            {
                ResourceType type = (ResourceType)Enum.Parse(typeof(ResourceType), spriteNode.Attributes["name"].Value);
                int w = int.Parse(spriteNode.Attributes["w"].Value);
                int h = int.Parse(spriteNode.Attributes["h"].Value);
                int x = int.Parse(spriteNode.Attributes["x"].Value);
                int y = int.Parse(spriteNode.Attributes["y"].Value);
                Sprite sp = new Sprite(atlas, new Rectangle(x,y,w,h));
                ResourceIcons.Add(type, sp);
            }
        }

        private static void LoadTiles(XmlNode tilesNode)
        {
            int w = int.Parse(tilesNode.Attributes["w"].Value);
            int h = int.Parse(tilesNode.Attributes["h"].Value);

            TileSprites = new Sprite[16];
            foreach(XmlNode n in tilesNode.SelectNodes("tile"))
            {
                int index = int.Parse(n.Attributes["index"].Value);
                int x = int.Parse(n.Attributes["x"].Value) * w;
                int y = int.Parse(n.Attributes["y"].Value) * h;
                TileSprites[index] = new Sprite(atlas, new Rectangle(x, y, w, h));
            }
        }

    }
}

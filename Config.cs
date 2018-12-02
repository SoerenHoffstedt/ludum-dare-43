using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LD43
{
    public static class Config
    {
        public static Point Resolution = new Point(1600, 900);
        public static bool Fullscreen = false;

        public static void LoadFromDisc(string filename)
        {
            XmlDocument xmlConfig = new XmlDocument();
            xmlConfig.Load(filename);

            var resNode = xmlConfig.SelectSingleNode("config/resolution");
            Resolution = new Point(int.Parse(resNode.Attributes["x"].Value), int.Parse(resNode.Attributes["y"].Value));
            var fullscreenNode = xmlConfig.SelectSingleNode("config/fullscreen");
            Fullscreen = bool.Parse(fullscreenNode.Attributes["val"].Value);
        }

    }
}

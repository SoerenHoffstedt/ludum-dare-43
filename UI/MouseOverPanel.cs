using Barely.Util;
using BarelyUI;
using LD43.Scenes;
using LD43.World;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD43.UI
{
    public class MouseOverPanel : VerticalLayout
    {
        GameScene game;
        Point lastMouseOver;
        Text text;
        bool textIsEmpty;

        public MouseOverPanel(GameScene game) : base()
        {
            this.game = game;

            text = new Text($"{1234}x{1234} \nResource:  {"Stone"}. Amount:  {1000} \nBuilding:  {"Reproduction Cave"} \n \n", false);

            AddChild(new UIElement[] { text });
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            Point mouseOver = game.GetMouseOverTile();
            Tile t = game.GetTile(mouseOver);

            if(t != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"{mouseOver.X}x{mouseOver.Y}");
                sb.AppendLine($"Resources:  { Texts.Get(t.resourceType.ToString())}, left: { t.resourceAmount}");
                string building = t.building != null ? Texts.Get(t.building.blueprint.name) : "None";
                sb.AppendLine($"Building:  {building}");
                if(t.building != null)
                {
                    if(t.building.constructionTimer > 0f)
                        sb.AppendLine(String.Format("Under Construction:  {0:P0}", 1f - t.building.constructionTimer / t.building.blueprint.constructionTime));
                    else if(t.building.blueprint.type == BuildingType.Production)
                        sb.AppendLine(String.Format("Production:  {0:P0}", 1f - t.building.productionTimer / t.building.blueprint.productionTime));
                }
                text.SetText(sb.ToString());
                textIsEmpty = false; 
            } else
            {
                if (!textIsEmpty)
                {
                    text.SetText("");
                    textIsEmpty = true;
                }
                
            }

            lastMouseOver = mouseOver;
        }
    }
}

using Barely.Util;
using BarelyUI;
using BarelyUI.Layouts;
using BarelyUI.Styles;
using LD43.Scenes;
using LD43.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD43.UI
{
    public class ButtonPanel : HorizontalLayout
    {
        GameScene game;
        Text textOver;
        BuildingBlueprint overBlueprint = null;
        StringBuilder s = new StringBuilder();

        public ButtonPanel(GameScene game) : base()
        {
            this.game = game;
            string longMaxText = "";
            textOver = new Text(longMaxText, false);
            SetTextToBlueprint(Assets.Blueprints["SawMill"]);

            AddChild(textOver);

            foreach (string key in Assets.Blueprints.Keys)
            {
                BuildingBlueprint b = Assets.Blueprints[key];
                Button bttn = new Button(b.spriteIcon);
                bttn.OnMouseClick = () => game.SetPlacementSelection(b);
                bttn.OnMouseEnter = () => overBlueprint = b;
                bttn.OnMouseExit = () => { if (overBlueprint == b) overBlueprint = null; };
                AddChild(bttn);
            }            
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            var b = overBlueprint;

            if(b == null)
            {
                b = game.GetPlacementSelection();
            } 

            if(b != null)
            {
                SetTextToBlueprint(b);
            }
            else
            {
                textOver.SetText("");
            }

        }

        private void SetTextToBlueprint(BuildingBlueprint b)
        {
            s.AppendLine($"{Texts.Get(b.name)}");
            s.AppendLine("Costs:");
            for (int i = 0; i < (int)ResourceType.NoneCount; i++)
            {
                s.AppendLine($"{(ResourceType)i}: {b.buildingCosts[i]}");
            }
            s.AppendLine($"Construction Time: {game.TimeAsDays(b.constructionTime)} days");            
            if (b.type == BuildingType.Production)
            {
                s.Append($"Produces: {b.produces}. {b.productionAmount} per {game.TimeAsDays(b.productionTime)} days");
            } else if(b.type == BuildingType.Hut)
            {
                s.Append(Texts.Get("HutExplain"));
            } else if(b.type == BuildingType.ReproductionCave)
            {
                s.Append(Texts.Get("ReproductionCaveExplain"));
            }
            textOver.SetText(s.ToString());
            s.Clear();
        }

    }
}

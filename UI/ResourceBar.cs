using Barely.Util;
using BarelyUI;
using BarelyUI.Layouts;
using BarelyUI.Styles;
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
    public class ResourceBar : HorizontalLayout
    {
        GameScene scene;

        Text populationCount;
        //KeyValueText woodCount;
        //KeyValueText stoneCount;
        //KeyValueText foodCount;

        Image foodIcon;
        Text foodCount;
        Image woodIcon;
        Text woodCount;
        Image stoneIcon;
        Text stoneCount;

        Text dayText;
        Text hourText;

        public ResourceBar(GameScene scene) : base()
        {
            this.scene = scene;

            

            populationCount = new Text("population 12345");
            populationCount.SetTextUpdateFunction(() => $"{Texts.Get("population")}   {scene.populationCount}");            

            foodIcon = new Image(Assets.ResourceIcons[ResourceType.Food]);
            foodCount = new Text("12345").SetTextUpdateFunction(() => scene.resources[(int)ResourceType.Food].ToString());

            woodIcon = new Image(Assets.ResourceIcons[ResourceType.Wood]);
            woodCount = new Text("12345").SetTextUpdateFunction(() => scene.resources[(int)ResourceType.Wood].ToString());

            stoneIcon = new Image(Assets.ResourceIcons[ResourceType.Stone]);
            stoneCount = new Text("12345").SetTextUpdateFunction(() => scene.resources[(int)ResourceType.Stone].ToString());

            dayText = new Text("day 12345");
            dayText.SetTextUpdateFunction(() => $"Day   {scene.dayCount}");
            hourText = new Text("hour 1234567");
            hourText.SetTextUpdateFunction(() => String.Format("Hour   {0:P2}", scene.dayTime / GameScene.DAY_LENGTH));
            
            AddChild(new UIElement[] { populationCount, woodIcon, woodCount, stoneIcon, stoneCount, foodIcon, foodCount, dayText, hourText });

            
        }


    }
}

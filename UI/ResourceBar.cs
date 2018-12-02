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
        Text sacrificeTime;

        bool soundOn = true;

        public ResourceBar(GameScene scene) : base()
        {
            this.scene = scene;

            populationCount = new Text("population 12/12");
            populationCount.SetTextUpdateFunction(() => $"{Texts.Get("population")}   {scene.populationCount}/{scene.populationMax}");

            foodIcon = new Image(Assets.ResourceIcons[ResourceType.Food]);
            foodCount = new Text("1234").SetTextUpdateFunction(() => scene.resources[(int)ResourceType.Food].ToString());

            woodIcon = new Image(Assets.ResourceIcons[ResourceType.Wood]);
            woodCount = new Text("1234").SetTextUpdateFunction(() => scene.resources[(int)ResourceType.Wood].ToString());

            stoneIcon = new Image(Assets.ResourceIcons[ResourceType.Stone]);
            stoneCount = new Text("1234").SetTextUpdateFunction(() => scene.resources[(int)ResourceType.Stone].ToString());

            dayText = new Text("day 1234");
            dayText.SetTextUpdateFunction(() => $"Day  {scene.dayCount}");
            hourText = new Text("hour 1234");
            hourText.SetTextUpdateFunction(() => $"Hour  {(int)(24 * scene.dayTime / GameScene.DAY_LENGTH)}");

            sacrificeTime = new Text("Next Sacrifice: 24 hours", false);
            sacrificeTime.SetTextUpdateFunction(() => $"Next Sacrifice: {scene.HoursUntilNextSacrifice()} hours");

            Button menuButton = new Button(Assets.OtherSprites["menuIcon"]);
            menuButton.OnMouseClick = () => scene.LeaveToMenu();

            Button soundButton = new Button(Assets.OtherSprites["soundOn"]);
            soundButton.OnMouseClick = () => {
                scene.ToggleMusic();
                soundOn = !soundOn;
                if (soundOn)
                    soundButton.sprite = Assets.OtherSprites["soundOn"];
                else
                    soundButton.sprite = Assets.OtherSprites["soundOff"];
            };

            AddChild(new UIElement[] { menuButton, soundButton, populationCount, woodIcon, woodCount, stoneIcon, stoneCount, foodIcon, foodCount, dayText, hourText, sacrificeTime });

            
        }


    }
}

using Barely.SceneManagement;
using BarelyUI;
using BarelyUI.Layouts;
using BarelyUI.Styles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD43.Scenes
{
    public class EndScreen : BarelyScene
    {
        GameEndScreenInfo info;
        Canvas canvas;

        public EndScreen(ContentManager Content, GraphicsDevice GraphicsDevice, Game game, GameEndScreenInfo info) 
            : base(Content, GraphicsDevice, game)
        {
            this.info = info;
            canvas = new Canvas(Content, Config.Resolution, GraphicsDevice);
            CreateUI();
        }

        private void CreateUI()
        {
            Game1 g = (Game1)game;

            Layout.PushLayout("endScreen");
            Style.PushStyle("endScreen");

            VerticalLayout vert = new VerticalLayout();

            Text lose       = new Text("loseMsg");
            Text days       = new Text($"You survived {info.daysLived} days.");
            Text sacrifices = new Text($"You sacrificed {info.sacrificedCount} Head Spinners.");
            Text deaths     = new Text($"You let {info.starvedCount} Head Spinners starve.");

            Layout.PushLayout("endScreenButtons");
            HorizontalLayout bttns = new HorizontalLayout();

            Button newGame1 = new Button("newGameNormal");
            newGame1.OnMouseClick = () => g.ShowNewGame(Difficulty.Normal);

            Button newGame2 = new Button("newGameHarder");
            newGame1.OnMouseClick = () => g.ShowNewGame(Difficulty.Harder);

            Button mainMenu = new Button("mainMenu");
            mainMenu.OnMouseClick = () => g.ShowMainMenu();

            Button exit = new Button("exit");
            exit.OnMouseClick = () => g.Exit();

            bttns.AddChild(newGame1, newGame2, mainMenu, exit);
            Layout.PopLayout("endScreenButtons");

            Text thanks = new Text("thanks");

            vert.AddChild(new UIElement[] { lose, days, sacrifices, deaths, bttns, thanks });



            Layout.PopLayout("endScreen");
            Style.PopStyle("endScreen");

            canvas.AddChild(new UIElement[] { vert });
            canvas.FinishCreation();
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointWrap, transformMatrix: camera.uiTransform);
            canvas.Render(spriteBatch);
            spriteBatch.End();
        }

        public override void Initialize()
        {
            
        }

        public override void Update(double deltaTime)
        {
            canvas.HandleInput();
            canvas.Update((float)deltaTime);
        }

        protected override void CameraInput(double deltaTime)
        {
            
        }
    }

    public class GameEndScreenInfo
    {
        public int daysLived;
        public int sacrificedCount;
        public int starvedCount;
        public int bornCount;

        public GameEndScreenInfo(int days, int sacrifices, int starved, int born)
        {
            daysLived = days;
            sacrificedCount = sacrifices;
            starvedCount = starved;
            bornCount = born;
        }

    }

}

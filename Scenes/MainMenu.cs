using Barely.SceneManagement;
using BarelyUI;
using BarelyUI.Layouts;
using BarelyUI.Styles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD43.Scenes
{
    public class MainMenu : BarelyScene
    {
        Canvas canvas;

        public MainMenu(ContentManager Content, GraphicsDevice GraphicsDevice, Game game) 
            : base(Content, GraphicsDevice, game)
        {
            canvas = new Canvas(Content, Config.Resolution, GraphicsDevice);
            CreateUI();
        }

        private void CreateUI()
        {
            Game1 g = (Game1)game;

            Layout.PushLayout("mainMenu");
            Style.PushStyle("mainMenu");          

            VerticalLayout menu = new VerticalLayout();

            Text name = new Text("Head Spin Builder - Sacrifice Edition!", false);            

            Button newGameNormal = new Button("newGameNormal");
            newGameNormal.OnMouseClick = () => g.ShowNewGame(Difficulty.Normal);

            Button newGameHard = new Button("newGameHarder");
            newGameHard.OnMouseClick = () => g.ShowNewGame(Difficulty.Harder);

            Button exit = new Button("exit");
            exit.OnMouseClick = () => g.Exit();

            Text ld = new Text("ld");
            Text by = new Text("by");
            Text thanks = new Text("thanks");
            Text howtoHeadline = new Text("howto");


            Style.PushStyle("tutText");
            string tutFile = File.ReadAllText("Content/tut.txt");
            Text tut = new Text(tutFile, false);
            Style.PopStyle("tutText");

            menu.AddChild(new UIElement[] { name, newGameNormal, newGameHard, exit, ld, by, thanks, new Space(15), howtoHeadline, tut });

            Layout.PopLayout("mainMenu");
            Style.PopStyle("mainMenu");

            canvas.AddChild(new UIElement[] { menu });            
            canvas.FinishCreation();
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

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointWrap, transformMatrix: camera.uiTransform);
            canvas.Render(spriteBatch);
            spriteBatch.End();
        }
    }
}

using Barely.SceneManagement;
using Barely.Util;
using BarelyUI.Layouts;
using BarelyUI.Styles;
using LD43.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Xml;

namespace LD43
{
    
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        BarelyScene currScene = null;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            IsMouseVisible = true;
            graphics.PreferredBackBufferWidth = Config.Resolution.X;
            graphics.PreferredBackBufferHeight = Config.Resolution.Y;
            graphics.IsFullScreen = false;
            Window.IsBorderless = false;
            //graphics.SynchronizeWithVerticalRetrace = false;
            //IsFixedTimeStep = false;
            graphics.ApplyChanges();
            SpriteBatchEx.GraphicsDevice = GraphicsDevice;

            Assets.Load(Content);

            XmlDocument soundsXml = new XmlDocument();
            soundsXml.Load("Content/Sounds/Sounds.xml");
            Sounds.Initialize(Content, soundsXml);

            XmlDocument lang = new XmlDocument();
            lang.Load("Content/Language/en.xml");
            Texts.SetTextFile(lang);           

            Style.InitializeStyle("Content/uiStyle.xml", Content);
            Layout.InitializeLayouts("Content/uiLayout.xml");

        }


        protected override void Initialize()
        {

            currScene = new GameScene(Content, GraphicsDevice, this);

            base.Initialize();
        }


        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

        }


        protected override void UnloadContent()
        {
        }


        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            double dt = gameTime.ElapsedGameTime.TotalSeconds;

            Input.Update();

            currScene.Update(dt);

            Animator.Update(dt);

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(61, 59, 76));
            double dt = gameTime.ElapsedGameTime.TotalMilliseconds;
            Window.Title = $"Head Spin Builder - Delta Time: {dt.ToString("0.000")} - FPS: {(1000 / dt).ToString("000.0")}";
            
            currScene.Draw(spriteBatch);

            base.Draw(gameTime);
        }
    }
}

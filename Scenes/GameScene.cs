using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Barely.Util;
using BarelyUI;
using LD43.Actors;
using LD43.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LD43.Scenes
{
    public class GameScene : Barely.SceneManagement.BarelyScene
    {
        public enum State
        {
            Normal,
            Sacrifice
        }

        private Map map;
        private List<Actor> actors;

        private State state = State.Normal;

        private int peopleCount;
        private int[] resources;

        private int dayCount;
        private float dayTime; //in seconds
        private const float DAY_LENGTH = 60; //in seconds

        private float timeToNextSacrifice;
        private const float SACRIFICE_TIME = 120;

        private bool Paused { get; set; } = false;

        private Canvas canvas;

        public GameScene(ContentManager Content, GraphicsDevice GraphicsDevice, Game game) 
                : base(Content, GraphicsDevice, game)
        {
            map = new Map();

            camera = new Barely.Util.Camera(GraphicsDevice.Viewport, 4, 1);
            camera.SetMinMaxPosition(new Vector2(0,0), new Vector2(Map.Size.X * Map.TileSize.X, Map.Size.Y * Map.TileSize.Y));

            resources = new int[(int)ResourceType.NoneCount];
            //start resources
            resources[(int)ResourceType.Food]   = 20;
            resources[(int)ResourceType.Wood]   = 10;
            resources[(int)ResourceType.Stone]  = 10;
            peopleCount                         = 5;

            dayCount = 1;
            
        }

        public override void Initialize()
        {
            
        }

        public override void Update(double deltaTime)
        {
            float dt = (float)deltaTime;

            if (!Paused)
            {
                dayTime += dt;
                if(dayTime > DAY_LENGTH)
                {
                    dayCount++;
                    dayTime -= DAY_LENGTH;
                }

                HandleInput(deltaTime);
                CameraInput(deltaTime);
            }

            
        }

        bool cameraTakesInput = true;

        private void HandleInput(double deltaTime)
        {
            bool handled = false;// canvas.HandleInput();
            cameraTakesInput = true;
            if (handled)
            {
                cameraTakesInput = false;
                if (isDragging && !Input.GetRightMousePressed() && !Input.GetMiddleMousePressed())
                    isDragging = false;
                return;
            }

            /*if (Input.GetKeyDown(Keys.F5))
            {
                GenerateNewMap();
            }*/
            

        }

        const int MIN_ZOOM = 1;
        const int MAX_ZOOM = 4;
           
        protected override void CameraInput(double deltaTime)
        {
            float dt = (float)deltaTime;

            Vector2 camMove = new Vector2();

            if (cameraTakesInput)
            {
                int zoom = (int)camera.zoom;
                int zoomChange = 0;

                int wheel = Input.GetMouseWheelDelta();
                if (wheel != 0)
                {
                    if (wheel > 0)
                        zoomChange++;
                    else
                        zoomChange--;
                }

                if (Input.GetKeyDown(Keys.Q))
                    zoomChange++;
                if (Input.GetKeyDown(Keys.E))
                    zoomChange--;

                zoom += zoomChange;
                if (zoom < MIN_ZOOM)
                    zoom = MIN_ZOOM;
                if (zoom > MAX_ZOOM)
                    zoom = MAX_ZOOM;
                camera.zoom = zoom;

                float camSpeed = 1000f;

                if (Input.GetKeyPressed(Keys.D) || Input.GetKeyPressed(Keys.Right))
                    camMove.X += camSpeed * dt;
                if (Input.GetKeyPressed(Keys.A) || Input.GetKeyPressed(Keys.Left))
                    camMove.X -= camSpeed * dt;
                if (Input.GetKeyPressed(Keys.S) || Input.GetKeyPressed(Keys.Down))
                    camMove.Y += camSpeed * dt;
                if (Input.GetKeyPressed(Keys.W) || Input.GetKeyPressed(Keys.Up))
                    camMove.Y -= camSpeed * dt;

                if (!isDragging)
                {
                    if (Input.GetRightMouseDown())
                        isDragging = true;
                    else if (Input.GetMiddleMouseDown())
                        isDragging = true;
                }

                if (isDragging)
                {
                    if (Input.GetRightMouseUp())
                        isDragging = false;
                    else if (Input.GetMiddleMouseUp())
                        isDragging = false;
                }

                if (isDragging)
                    camMove -= Input.GetMousePositionDelta().ToVector2() / camera.zoom;
            }

            camera.Update(deltaTime, camMove);
        }
     
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointWrap, transformMatrix: camera.Transform);

            map.Render(spriteBatch);

            spriteBatch.End();
        }
    }
}

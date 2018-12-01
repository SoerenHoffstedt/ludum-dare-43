using Glide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Barely.Util;
using BarelyUI;
using BarelyUI.Layouts;
using BarelyUI.Styles;
using LD43.Actors;
using LD43.UI;
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

        public static Tweener Tweener = new Tweener();

        private Map map;        
        private List<Actor> actors = new List<Actor>(32);
        private Queue<Task> taskQueue = new Queue<Task>(128);

        private State state = State.Normal;

        public int populationCount;
        public int[] resources;

        public int dayCount;
        public float dayTime; //in seconds
        public const float DAY_LENGTH = 60; //in seconds

        public float timeToNextSacrifice;
        public const float SACRIFICE_TIME = 120;

        public bool Paused { get; set; } = false;

        private Canvas canvas;

        BuildingBlueprint placementSelection = null;
        Actor mouseOverActor = null;
        Actor selectedActor = null;
        Building selectedBuilding = null;
        Point mouseOverTile = new Point(-1, -1);
        Sprite spriteSelectActor;
        Sprite spriteSelectTile;
        
        Color colorOver = Color.YellowGreen;
        Color colorSelected = Color.Yellow;        

        public GameScene(ContentManager Content, GraphicsDevice GraphicsDevice, Game game) 
                : base(Content, GraphicsDevice, game)
        {

            map = new Map(this);

            camera = new Barely.Util.Camera(GraphicsDevice.Viewport, 4, 1);
            camera.SetMinMaxPosition(new Vector2(0,0), new Vector2(Map.Size.X * Map.TileSize.X, Map.Size.Y * Map.TileSize.Y));

            resources = new int[(int)ResourceType.NoneCount];
            //start resources
            resources[(int)ResourceType.Food]   = 20;
            resources[(int)ResourceType.Wood]   = 10;
            resources[(int)ResourceType.Stone]  = 10;
            populationCount                     = 5;

            dayCount = 1;

            spriteSelectActor = Assets.OtherSprites["selectActor"];
            spriteSelectTile = Assets.OtherSprites["selectTile"];

            CreateUI();
        }

        public override void Initialize()
        {
            
        }

        public override void Update(double deltaTime)
        {
            float dt = (float)deltaTime;

            Tweener.Update(dt);
            LogicUpdate(dt);

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

            canvas.Update(dt);            
        }

        #region Input

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

            mouseOverActor = GetMouseOverActor();
            mouseOverTile = camera.ToWorld(Input.GetMousePosition()) / Map.TileSize;

            if (Input.GetLeftMouseUp())
            {                              
                if(placementSelection != null && map.IsInRange(mouseOverTile))
                {
                    Tile t = map[mouseOverTile];
                    if (t.CanPlaceBuilding(placementSelection))
                    {
                        map.PlaceBuilding(t, placementSelection);
                    } else
                    {
                        Sounds.Play("notPlaceable");
                    }
                }

                selectedActor = mouseOverActor != null ? mouseOverActor : null ;

                if (mouseOverActor == null && map.IsInRange(mouseOverTile))                
                    selectedBuilding = map[mouseOverTile].building;                
                else
                    selectedBuilding = null;
            }

            if (Input.GetKeyDown(Keys.F5))
            {
                Canvas.DRAW_DEBUG = !Canvas.DRAW_DEBUG;
            }
            

        }

        const int MIN_ZOOM = 1;
        const int MAX_ZOOM = 6;
           
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

        #endregion

        #region Game Logic
        private const int TASKS_PER_FRAME = 3;
        private void LogicUpdate(float dt)
        {
            for (int x = 0; x < Map.Size.X; x++)
            {
                for (int y = 0; y < Map.Size.Y; y++)
                {
                    Tile t = map[x, y];
                    if(t.building != null)
                    {
                        Building b = t.building;
                        BuildingBlueprint blueprint = b.blueprint;
                        if (b.IsBuilt())
                        {
                            if(blueprint.type == BuildingType.Production && t.resourceType != ResourceType.NoneCount)
                            {
                                b.productionTimer -= dt;
                                if (b.productionTimer <= 0f)
                                {                                                                        
                                    resources[(int)blueprint.produces] += blueprint.productionAmount;
                                    t.resourceAmount -= blueprint.productionAmount;
                                    Sounds.Play("resourceGained");                 
                                    if(t.resourceAmount <= 0)
                                    {
                                        t.resourceAmount = 0;
                                        t.resourceType = ResourceType.NoneCount;
                                    }                                    
                                    b.productionTimer += blueprint.productionTime;
                                }
                            }                            
                        }
                        else
                        {
                            if (b.IsWorked())
                            {
                                b.constructionTimer -= dt;
                                if (b.constructionTimer <= 0f)
                                {
                                    Sounds.Play("buildingConstructed");
                                    b.constructionTimer = 0f;
                                    b.FinishWork();
                                }
                            }                            
                        }
                    }
                }
            }


            for (int i = 0; i < TASKS_PER_FRAME && taskQueue.Count > 0; i++)
            {
                Actor freeActor = null;
             
                foreach(Actor a in actors)
                {
                    if (!a.IsBusy())
                    {
                        freeActor = a;
                        break;
                    }
                }

                if (freeActor == null)
                    break;

                //only handle on task per frame?
                Task task = taskQueue.Dequeue();
                freeActor.GoToTask(task);
            }

            foreach(Actor a in actors)
            {
                if(a.currPath != null)
                {
                    //a.WalkPath(dt);
                }
            }

        }        

        private Actor GetMouseOverActor()
        {            
            Point mousePos = camera.ToWorld(Input.GetMousePosition());            

            foreach (Actor a in actors)
            {
                Rectangle pos = new Rectangle(a.DrawPosition(), Map.TileSize);

                if (pos.Contains(mousePos))
                {
                    return a;
                }
            }

            return null;
        }

        public void AddConstructionTask(Tile t)
        {
            taskQueue.Enqueue(new Task(
                t,
                t.coord * Map.TileSize - Map.HalfTileSize, 
                () => t.building.constructionTimer == 0f, 
                (actor) => { t.building.workedBy = actor; }, 
                (actor) => { actor.currTask = null; }
            ));
        }

        private void BuildingComplete()
        {

        }

        private void SwitchToSacrifice()
        {

        }

        private void SwitchToNormal()
        {

        }

        #endregion

        #region API 

        public void SpawnActor(Tile t)
        {
            actors.Add(new Actor(t.coord * Map.TileSize));
        }

        public Point GetMouseOverTile()
        {
            return mouseOverTile;
        }

        public Tile GetTile(Point p)
        {
            return map.IsInRange(p) ? map[p] : null;
        }

        #endregion

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointWrap, transformMatrix: camera.Transform);
            map.Render(spriteBatch);

            if(selectedBuilding != null)
            {
                spriteSelectTile.Render(spriteBatch, selectedBuilding.coord * Map.TileSize, colorSelected);
            }

            if(mouseOverActor == null && map.IsInRange(mouseOverTile))
            {
                spriteSelectTile.Render(spriteBatch, mouseOverTile * Map.TileSize, colorOver);
            }

            foreach(Actor a in actors)
            {
                Point pos = a.DrawPosition();
                if (selectedActor == a)                
                    spriteSelectActor.Render(spriteBatch, pos + new Point(0,4), colorSelected);
                else if(mouseOverActor == a)
                    spriteSelectActor.Render(spriteBatch, pos + new Point(0, 4), colorOver);
                a.sprite.Render(spriteBatch, pos);
            }

            spriteBatch.End();

            spriteBatch.Begin(samplerState: SamplerState.PointWrap, transformMatrix: camera.uiTransform);
            canvas.Render(spriteBatch);
            spriteBatch.End();
        }

        #region UI

        ResourceBar resourceBar;
        SelectionPanel selectionPanel;
        ButtonPanel buttonPanel;
        MouseOverPanel mouseOverPanel;

        private void CreateUI()
        {
            Layout.InitializeLayouts("Content/uiLayout.xml");
            Style.InitializeStyle("Content/uiStyle.xml", Content);

            Style.PushStyle("standard");
            Layout.PushLayout("standard");


            canvas = new Canvas(Content, Config.Resolution, GraphicsDevice);


            Layout.PushLayout("resourceBar");
            Style.PushStyle("resourceBar");
            resourceBar = new ResourceBar(this);
            Layout.PopLayout("resourceBar");
            Style.PopStyle("resourceBar");

            Layout.PushLayout("selectionPanel");
            Style.PushStyle("selectionPanel");
            selectionPanel = new SelectionPanel(this);
            Layout.PopLayout("selectionPanel");
            Style.PopStyle("selectionPanel");

            Layout.PushLayout("buttonPanel");
            Style.PushStyle("buttonPanel");
            buttonPanel = new ButtonPanel(this);
            Layout.PopLayout("buttonPanel");
            Style.PopStyle("buttonPanel");

            Layout.PushLayout("mouseOverPanel");
            Style.PushStyle("mouseOverPanel");
            mouseOverPanel = new MouseOverPanel(this);
            Layout.PopLayout("mouseOverPanel");
            Style.PopStyle("mouseOverPanel");

            canvas.AddChild(resourceBar, selectionPanel, buttonPanel, mouseOverPanel);
            canvas.FinishCreation();
        }

        #endregion
    }
}

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
using Microsoft.Xna.Framework.Media;

namespace LD43.Scenes
{
    public class GameScene : Barely.SceneManagement.BarelyScene
    {
        public enum State
        {
            Normal,
            SacrificeSelection,
            SacrificeCelebrating,
            EndDelay
        }

        public static Tweener Tweener = new Tweener();
        public static Tweener UITweener = new Tweener();

        private Map map;        
        private List<Actor> actors = new List<Actor>(32);
        private Queue<Task> taskQueue = new Queue<Task>(128);
        private HashSet<Task> activeTasks = new HashSet<Task>();

        private State state = State.Normal;

        public int populationCount;
        public int populationMax;
        public int[] resources;

        public int dayCount;
        public float dayTime; //in seconds
        public float timeToNextSacrifice;

        public const float DAY_LENGTH = 20; //in seconds
        public const float SACRIFICE_TIME = 40;

        //stats
        private int sacrificeCount = 0;
        private int starvedCount = 0;
        private int bornCount = 0;

        public bool Paused { get; set; } = false;

        private Canvas canvas;

        BuildingBlueprint placementSelection = null;
        Actor mouseOverActor = null;
        Actor selectedActor = null;
        Building selectedBuilding = null;
        Point mouseOverTile = new Point(-1, -1);

        Sprite spriteSelectActor;
        Sprite spriteSelectTile;
        Sprite constructionSprite;

        Color colorOver = Color.YellowGreen;
        Color colorSelected = Color.Yellow;
        Color colorValidPlacement = Color.Green;
        Color colorInvalidPlacement = Color.Red;

        Song song;
        float soundVolume = 0.4f;


        public GameScene(ContentManager Content, GraphicsDevice GraphicsDevice, Game game, Difficulty diff) 
                : base(Content, GraphicsDevice, game)
        {
            resources = new int[(int)ResourceType.NoneCount];
            //start resources
            resources[(int)ResourceType.Food] = 1000;
            resources[(int)ResourceType.Wood] = 1000;
            resources[(int)ResourceType.Stone] = 1000;
            populationCount = 0;

            dayCount = 1;

            map = new Map(this);

            resources[(int)ResourceType.Food] = diff.startingFood;
            resources[(int)ResourceType.Wood] = diff.startingWood;
            resources[(int)ResourceType.Stone] = diff.startingStone;

            camera = new Barely.Util.Camera(GraphicsDevice.Viewport, 4, 1);
            camera.SetMinMaxPosition(new Vector2(0,0), new Vector2(Map.Size.X * Map.TileSize.X, Map.Size.Y * Map.TileSize.Y));            

            spriteSelectActor = Assets.OtherSprites["selectActor"];
            spriteSelectTile = Assets.OtherSprites["selectTile"];
            constructionSprite = Assets.OtherSprites["constructionSprite"];

            timeToNextSacrifice = SACRIFICE_TIME;

            song = Content.Load<Song>("Sounds/secondSong");
            MediaPlayer.Play(song);            
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = soundVolume;

            CreateUI();
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
                timeToNextSacrifice -= dt;
                if (timeToNextSacrifice <= 0f)
                {
                    SwitchToSacrifice();
                    timeToNextSacrifice += SACRIFICE_TIME;
                }
                else if (dayTime > DAY_LENGTH)
                {
                    dayCount++;
                    dayTime -= DAY_LENGTH;
                    NewDay();
                }
                
                Tweener.Update(dt);
            }
            UITweener.Update(dt);

            LogicUpdate(dt);

            HandleInput(deltaTime);
            CameraInput(deltaTime);
                            
            canvas.Update(dt);            
        }

        #region Input

        bool cameraTakesInput = true;

        private void HandleInput(double deltaTime)
        {
            bool handled = canvas.HandleInput();
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

            if(state == State.SacrificeSelection)
            {
                if (Input.GetLeftMouseUp())
                {
                    if(mouseOverActor != null)
                    {
                        selectedActor = mouseOverActor;
                        SelectSacrificeTarget(selectedActor);
                    }
                }
            }
            else
            {
                if (Input.GetLeftMouseUp())
                {
                    if (placementSelection != null && map.IsInRange(mouseOverTile))
                    {
                        Tile t = map[mouseOverTile];

                        if (IsValidPlacement())
                        {
                            map.PlaceBuilding(t, placementSelection);
                            Sounds.Play("placed");
                        }
                        else
                        {
                            //show notifications, why not placeable
                            if (placementSelection.type == BuildingType.ReproductionCave && populationMax - populationCount <= 0)
                                ShowNotification("notEnoughHuts", false);
                            else if (!placementSelection.GameHasEnoughResources(this))
                                ShowNotification("notEnoughResources", false);
                            else
                                ShowNotification("notPlaceableHere", false);
                            Sounds.Play("notPlaceable");
                        }
                    }

                    selectedActor = mouseOverActor != null ? mouseOverActor : null;

                    if (mouseOverActor == null && map.IsInRange(mouseOverTile))
                        selectedBuilding = map[mouseOverTile].building;
                    else
                        selectedBuilding = null;
                }

                if (Input.GetRightMouseUp())
                {
                    placementSelection = null;
                }
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

            if (state != State.Normal || CheckLoseCondition())
                return;

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
                                    b.FinishWork(this);
                                }
                            }                            
                        }
                    }
                }
            }

            foreach(Task activeTask in activeTasks)
            {
                activeTask.UpdateTask(dt);
            }

            activeTasks.RemoveWhere((t) => t.MarkedForDeletion);

            List<Actor> freeActors = new List<Actor>(actors.Count);
            foreach (Actor a in actors)
            {
                if (!a.IsBusy())
                    freeActors.Add(a);
            }

            for (int i = 0; i < TASKS_PER_FRAME && freeActors.Count > 0 && taskQueue.Count > 0; i++)
            {
                Actor freeActor = null;
             
                foreach(Actor a in freeActors)
                {
                    if (!a.IsBusy())
                    {
                        freeActor = a;
                        break;
                    }
                }
                freeActors.Remove(freeActor);

                if (freeActor == null)
                    break;

                Task task = taskQueue.Dequeue();
                freeActor.GoToTask(task);
            }

            if(taskQueue.Count == 0 && freeActors.Count > 0)
            {
                for (int i = 0; i < TASKS_PER_FRAME && i < freeActors.Count; i++)
                {
                    Actor a = freeActors[i];
                    Task idleTask = MakeIdleTask(a);
                    a.GoToTask(idleTask);
                }
                
            }            

        }        
       
        private void NewDay()
        {
            ShowNotification("newDay", false);
            Sounds.Play("newDay");
            int deathCount = 0;
            foreach(Actor a in actors)
            {
                if(resources[(int)ResourceType.Food] > 0)
                    resources[(int)ResourceType.Food] -= 1;
                else
                {
                    Death(a);
                    deathCount += 1;
                    starvedCount++;
                }
            }

            if(deathCount > 0)
            {
                actors.RemoveAll((a) => a.Dead);
                ShowNotification($"{Texts.Get("death")}: {deathCount}", false);
                Sounds.Play("death");
            }
        }

        private void Death(Actor a)
        {                        
            a.Dead = true;
            populationCount -= 1;
            if(a.currTask != null)
            {
                ReQueueTask(a.currTask);
                a.currTask.StopWork(); ;
            } 
            else if(a.currPath != null)
            {
                a.currPath.taskToStartOnReach.StopWork();
                ReQueueTask(a.currPath.taskToStartOnReach);
            }
        }

        private const float END_DELAY = 5f;

        private bool CheckLoseCondition()
        {
            if (actors.Count == 0)
            {
                Sounds.Play("boom");
                ShowNotification("youLost", false);
                GameEndScreenInfo info = new GameEndScreenInfo(dayCount, sacrificeCount, starvedCount, bornCount);                
                state = State.EndDelay;
                selectSacrificePanel.ChangeTextToEnd();
                selectSacrificePanel.Open();
                Timer timer = new Timer(END_DELAY, () => ((Game1)game).ShowGameEndScreen(info)).Start();
                return true;
            }
            return false;
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

        private const int MIN_DIST = 3;
        private const int MAX_DIST = 7;
        private Random random = new Random();

        private enum Direction { Up, Right, Down, Left }

        private Tile GetRandomWalkableTileNearActor(Tile from)
        {
            Point f = from.coord;
            Point selection = f;

            int dist = random.Next(MIN_DIST, MAX_DIST + 1);
            int distSquared = dist * dist;

            Direction dir1 = (Direction)random.Next(0, 4);
            Direction dir2 = dir1;
            while(dir1 == dir2)
            {
                dir2 = (Direction)random.Next(0, 4);
            }

            while((f.ToVector2() - selection.ToVector2()).LengthSquared() < distSquared)
            {
                Direction togo;
                if(random.NextDouble() < 0.5)
                {
                    togo = dir1;
                } else
                {
                    togo = dir2;
                }
                Point old = selection;
                switch (togo)
                {
                    case Direction.Up:
                        selection.Y -= 1;
                        break;
                    case Direction.Right:
                        selection.X += 1;
                        break;
                    case Direction.Down:
                        selection.Y += 1;
                        break;
                    case Direction.Left:
                        selection.X -= 1;
                        break;
                }

                if (!map.IsInRange(selection))
                {
                    if (old != from.coord)
                        return map[old]; //just return the last tile, if its not the source.
                    else
                        return GetRandomWalkableTileNearActor(from); //try again
                }
            }


            return map[selection];
        }

        private const float IDLE_TIME_MIN = 1f;
        private const float IDLE_TIME_MAX = 3f;

        private Task MakeIdleTask(Actor a)
        {
            Tile target = GetRandomWalkableTileNearActor(map[a.DrawPosition() / Map.TileSize]);
            float idleTime = (float)random.NextDouble() * (IDLE_TIME_MAX - IDLE_TIME_MIN) + IDLE_TIME_MIN;
            Timer timer = new Timer(idleTime);
            Task t = new Task(target,                              
                              () => timer.IsFinished(),
                              (actor) => { timer.Start(); },
                              (actor) => { if(actor != null) actor.currTask = null; },
                              activeTasks,
                              TaskType.Idle);
            return t;
        }

        public void AddConstructionTask(Tile t)
        {            
            taskQueue.Enqueue(new Task(
                t,                
                () => t.building.constructionTimer == 0f, 
                (actor) => { t.building.workedBy = actor; }, 
                (actor) => { t.building.workedBy = null; if(actor != null) actor.currTask = null; },
                activeTasks,
                TaskType.Construction
            ));
        }

        private void BuildingComplete()
        {

        }

        private void SelectSacrificeTarget(Actor a)
        {            
            sacrificeCount++;
            Sounds.Play("sacrificed");            
            Effects.ShowEffect("sacrifice", a.DrawPosition());
            ShowNotification("happySacrificed");
            SwitchToNormal();
            actors.Remove(a);
            Death(a);
        }

        private void SwitchToSacrifice()
        {
            Paused = true;
            state = State.SacrificeSelection;
            selectSacrificePanel.Open();
            placementSelection = null;
            selectedActor = null;
            selectedBuilding = null;
        }

        private void SwitchToNormal()
        {
            Paused = false;
            state = State.Normal;
            selectSacrificePanel.MyClose();
        }

        private bool IsValidPlacement()
        {
            if (placementSelection == null || !map.IsInRange(mouseOverTile))
                return false;
            if (!placementSelection.GameHasEnoughResources(this))
                return false;

            Tile t = map[mouseOverTile];
            if (placementSelection.type == BuildingType.ReproductionCave && populationMax - populationCount <= 0)
            {                
                return false;
            }
            return t.CanPlaceBuilding(placementSelection);
        }

        #endregion

        #region API 

        public void LeaveToMenu()
        {
            ((Game1)game).ShowMainMenu();
        }

        public void ToggleMusic()
        {
            if(MediaPlayer.Volume == 0.0f)
                MediaPlayer.Volume = soundVolume;
            else
                MediaPlayer.Volume = 0.0f;
        }

        private const float REPRODUCTION_DELAY = 3f;
        public void ReproductionCaveFinished(Building b)
        {
            Timer t = new Timer(REPRODUCTION_DELAY, () => {
                Tile tile = map[b.coord];
                SpawnActor(tile);
                Effects.ShowEffect("birth", tile.coord * Map.TileSize);
                Sounds.Play("birth");
                ShowNotification("birth", false);
                tile.building = null;
            }).Start();
        }

        public int HoursUntilNextSacrifice()
        {
            return (int)(24f * (timeToNextSacrifice / DAY_LENGTH));
        }

        public void ShowNotification(string text, bool playNotificationSound = true)
        {
            notifications.AddNotification(text);
            if(playNotificationSound)
                Sounds.Play("notification");
        }
            

        public void ReQueueTask(Task t)
        {            
            if(t.type != TaskType.Idle && !t.MarkedForDeletion)
                taskQueue.Enqueue(t);
        }

        public void SpawnActor(Tile t)
        {
            actors.Add(new Actor(t.coord * Map.TileSize, this));
            populationCount += 1;
        }

        public Point GetMouseOverTile()
        {
            return mouseOverTile;
        }

        public Tile GetTile(Point p)
        {
            return map.IsInRange(p) ? map[p] : null;
        }

        public void SetPlacementSelection(BuildingBlueprint blueprint)
        {
            if(state == State.Normal)
            {
                placementSelection = blueprint;            
            }
        }

        public BuildingBlueprint GetPlacementSelection()
        {                        
            return placementSelection;
        }

        public float TimeAsDays(float time)
        {
            return time / DAY_LENGTH;
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
                if (placementSelection != null)
                {
                    Color c = IsValidPlacement() ? colorValidPlacement : colorInvalidPlacement;
                    spriteSelectTile.Render(spriteBatch, mouseOverTile * Map.TileSize, c);
                    placementSelection.sprite.Render(spriteBatch, mouseOverTile * Map.TileSize);
                    constructionSprite.Render(spriteBatch, mouseOverTile * Map.TileSize);
                } else                
                    spriteSelectTile.Render(spriteBatch, mouseOverTile * Map.TileSize, colorOver);                
            }

            foreach(Actor a in actors)
            {
                Point pos = a.DrawPosition();
                if (selectedActor == a)                
                    spriteSelectActor.Render(spriteBatch, pos + new Point(0,4), colorSelected);
                else if(mouseOverActor == a)
                    spriteSelectActor.Render(spriteBatch, pos + new Point(0, 4), colorOver);
                a.Render(spriteBatch, state == State.SacrificeSelection, state == State.SacrificeCelebrating);
            }

            Effects.Render(spriteBatch);

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
        Notifications notifications;
        SelectSacrificePanel selectSacrificePanel;

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

            Layout.PushLayout("notifications");
            Style.PushStyle("notifications");
            notifications = new Notifications();
            Layout.PopLayout("notifications");
            Style.PopStyle("notifications");

            Layout.PushLayout("selectSacrificePanel");
            Style.PushStyle("selectSacrificePanel");
            Point size = new Point(400, 80);
            Point pos = new Point(Config.Resolution.X / 2 - size.X / 2, 80);
            selectSacrificePanel = new SelectSacrificePanel(pos, size);
            Layout.PopLayout("selectSacrificePanel");
            Style.PopStyle("selectSacrificePanel");

            canvas.AddChild(resourceBar, selectionPanel, buttonPanel, mouseOverPanel, notifications, selectSacrificePanel);
            canvas.FinishCreation();
        }

        #endregion
    }
}

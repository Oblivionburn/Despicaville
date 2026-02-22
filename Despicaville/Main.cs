using System;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Scenes;
using OP_Engine.Menus;
using OP_Engine.Inputs;
using OP_Engine.Sounds;
using OP_Engine.Characters;
using OP_Engine.Inventories;
using OP_Engine.Time;
using OP_Engine.Rendering;
using OP_Engine.Weathers;

using Despicaville.Scenes;
using Despicaville.Menus;

namespace Despicaville
{
    public class Main : Game
    {
        #region Variables

        public static OP_Game Game;

        public static BlendState AmbientBlendState = new BlendState();
        public static LightingRenderer LightingRenderer;

        public static string Version;

        public static int light_max_count = 32;
        public static int light_tile_distance = 4;

        #endregion

        #region Constructors

        public Main()
        {
            try
            {
                Game = new OP_Game
                {
                    Form = (Form)Control.FromHandle(Window.Handle),
                    Zoom = 2
                };
                Game.Init(this, Window);
            }
            catch (Exception e)
            {
                Game.CrashHandler(e);
            }
        }

        #endregion

        #region Methods

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                Version = fvi.FileVersion;

                LoadComponents();

                Game.SpriteBatch = new SpriteBatch(Game.GraphicsManager.GraphicsDevice);

                RenderingManager.InitDefaults(Game.GraphicsManager, Game.Resolution);
                RenderingManager.LightingRenderer = new LightingRenderer
                {
                    Name = "Lighting",
                    SetRenderTarget_BeforeDraw = true,
                    ClearGraphics_BeforeDraw = true,
                    ClearRenderTarget_AfterDraw = true,
                    BlendState = BlendState.Additive
                };
                RenderingManager.LightingRenderer.Init(Game.GraphicsManager, Game.Resolution);
                RenderingManager.AddLightingRenderer.RenderTarget = RenderingManager.LightingRenderer.RenderTarget;

                TimeTracker.Init();
                Handler.Init(this);

                if (!Game.GraphicsManager.IsFullScreen)
                {
                    Game.Form.WindowState = FormWindowState.Maximized;
                }

                LoadScenes();
                LoadMenus();

                SceneManager.ChangeScene("Loading");

                InputManager.MouseEnabled = true;
                InputManager.KeyboardEnabled = true;

                IsMouseVisible = true;
            }
            catch (Exception e)
            {
                Game.CrashHandler(e);
            }
        }

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            try
            {
                if (Window != null)
                {
                    if (Window.ClientBounds.Width > 0 &&
                        Window.ClientBounds.Height > 0)
                    {
                        if (!Game.Form.Focused)
                        {
                            if (Game.GameStarted &&
                                !TimeManager.Paused)
                            {
                                OpenMainMenu();
                            }

                            SoundManager.Paused = true;
                        }
                        else if (Game.Form.Focused)
                        {
                            SoundManager.Paused = false;
                            InputManager.Update();
                            MenuManager.Update(Game.Game, Content);
                            SceneManager.Update(Game.Game, Content);
                            RenderingManager.Update();
                            WeatherManager.Update(Game.Resolution, Color.White);
                        }
                    }
                    else
                    {
                        TimeManager.Paused = true;
                        SoundManager.Paused = true;
                    }

                    SoundManager.Update();
                }

                if (Game.Quit)
                {
                    SoundManager.StopAll();
                    Game.Game.Exit();
                }
            }
            catch (Exception e)
            {
                Game.CrashHandler(e);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            Game.Draw();

            if (Game.Window != null)
            {
                if (Game.Window.ClientBounds.Width > 0 &&
                    Game.Window.ClientBounds.Height > 0)
                {
                    base.Draw(gameTime);
                }
            }
        }

        private void LoadComponents()
        {
            Components.Add(new TimeManager(this));
            Components.Add(new InputManager(this));
            Components.Add(new SoundManager(this));
            Components.Add(new MenuManager(this));
            Components.Add(new SceneManager(this));
            Components.Add(new RenderingManager(this));
            Components.Add(new InventoryManager(this));
            Components.Add(new CharacterManager(this));
            Components.Add(new Handler(this));
            Components.Add(new Tasker(this));
        }

        private void LoadScenes()
        {
            SceneManager.Scenes.Add(new Logo());
            SceneManager.Scenes.Add(new Title(Content));
            SceneManager.Scenes.Add(new Loading(Content));
            SceneManager.Scenes.Add(new CharGen(Content));
            SceneManager.Scenes.Add(new Gameplay(Content));
        }

        private void LoadMenus()
        {
            MenuManager.Menus.Add(new Menu_UI(Content));
            MenuManager.Menus.Add(new Menu_Main(Content));
            MenuManager.Menus.Add(new Menu_Options(Content));
            MenuManager.Menus.Add(new Menu_Controls(Content));
            MenuManager.Menus.Add(new Menu_Inventory(Content));
            MenuManager.Menus.Add(new Menu_Interact(Content));
            MenuManager.Menus.Add(new Menu_Combat(Content));
            MenuManager.Menus.Add(new Menu_Health(Content));
            MenuManager.Menus.Add(new Menu_Wounds(Content));
        }

        public static void OpenMainMenu()
        {
            InputManager.Keyboard.Flush();

            OP_Engine.Menus.Menu menu = MenuManager.GetMenu("Main");
            if (menu != null)
            {
                OP_Engine.Menus.Menu menu_health = MenuManager.GetMenu("Health");
                if (menu_health != null)
                {
                    if (Handler.Menu_Health)
                    {
                        menu_health.Active = false;
                        menu_health.Visible = false;
                    }
                }

                OP_Engine.Menus.Menu menu_ui = MenuManager.GetMenu("UI");
                if (menu_ui != null)
                {
                    menu_ui.Active = false;
                    menu_ui.Visible = false;
                }

                menu.Open();
            }
        }

        #endregion
    }
}

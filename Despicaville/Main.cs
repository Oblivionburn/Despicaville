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
using Despicaville.Util;

namespace Despicaville
{
    public class Main : Game
    {
        #region Variables

        public static OP_Game? Game;

        public static bool LostFocus;
        public static string? Version;

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
                    Form = (Form?)Control.FromHandle(Window.Handle),
                    Zoom = 2
                };
                Game.Init(this, Window);
                Game.RenderingManager = new D_RenderingManager(this);
            }
            catch (Exception e)
            {
                Game?.CrashHandler(e);
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

                if (Game == null ||
                    Game.GraphicsManager == null ||
                    Game.RenderingManager == null)
                {
                    return;
                }

                Game.SpriteBatch = new SpriteBatch(Game.GraphicsManager.GraphicsDevice);

                Game.RenderingManager.InitDefaults(Game.GraphicsManager, Game.Resolution);

                if (Game.RenderingManager.AddLightingRenderer != null)
                {
                    Game.RenderingManager.LightingRenderer = new LightingRenderer
                    {
                        Name = "Lighting",
                        SetRenderTarget_BeforeDraw = true,
                        ClearGraphics_BeforeDraw = true,
                        ClearRenderTarget_AfterDraw = true,
                        BlendState = BlendState.Additive
                    };
                    Game.RenderingManager.LightingRenderer.Init(Game.GraphicsManager, Game.Resolution);
                    Game.RenderingManager.AddLightingRenderer.RenderTarget = Game.RenderingManager.LightingRenderer.RenderTarget;
                }

                Handler.Init(this);
                ShaderUtil.Init();
                TimeTracker.Init();

                if (!Game.GraphicsManager.IsFullScreen &&
                    Game.Form != null)
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
                Game?.CrashHandler(e);
            }
        }

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            if (Game == null ||
                Game.Form == null ||
                Game.RenderingManager == null ||
                Window == null)
            {
                return;
            }

            try
            {
                if (Game.Quit)
                {
                    SoundManager.StopAll();
                    Game.Game?.Exit();
                    return;
                }

                if (Window.ClientBounds.Width == 0 ||
                    Window.ClientBounds.Height == 0)
                {
                    if (!LostFocus)
                    {
                        LostFocus = true;
                        MenuManager.GetMenu("Main")?.Open();
                        SoundManager.Paused = true;
                    }

                    return;
                }

                if (!Game.Form.Focused)
                {
                    if (Game.GameStarted &&
                        !LostFocus &&
                        !TimeManager.Paused)
                    {
                        LostFocus = true;
                        MenuManager.GetMenu("Main")?.Open();
                    }

                    SoundManager.Paused = true;
                    return;
                }

                LostFocus = false;
                SoundManager.Paused = false;

                InputManager.Update();
                MenuManager.Update(Game.Game, Content);
                SceneManager.Update(Game.Game, Content);
                Game.RenderingManager.Update();
                WeatherManager.Update(Game.Resolution, Color.White);
                SoundManager.Update();
            }
            catch (Exception e)
            {
                Game?.CrashHandler(e);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            if (Game == null ||
                Game.RenderingManager == null)
            {
                return;
            }

            Game.RenderingManager.Draw(Window, Game.GraphicsManager, Game.SpriteBatch, new Point(Game.ScreenWidth, Game.ScreenHeight));
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
            SceneManager.Scenes.Add(new MapEditor(Content));
            SceneManager.Scenes.Add(new Loading(Content));
            SceneManager.Scenes.Add(new CharGen(Content));
            SceneManager.Scenes.Add(new Gameplay(Content));
        }

        private void LoadMenus()
        {
            MenuManager.Menus.Add(new Menu_Main(Content));
            MenuManager.Menus.Add(new Menu_Options(Content));
            MenuManager.Menus.Add(new Menu_Controls(Content));
            MenuManager.Menus.Add(new Menu_Inventory());
            MenuManager.Menus.Add(new Menu_Combat(Content));
            MenuManager.Menus.Add(new Menu_Health(Content));
            MenuManager.Menus.Add(new Menu_Wounds());
            MenuManager.Menus.Add(new Menu_UI(Content));
        }

        #endregion
    }
}

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
using OP_Engine.Utility;
using Despicaville.Scenes;
using Despicaville.Menus;
using Despicaville.Util;

namespace Despicaville
{
    public class Main : Game
    {
        #region Variables

        public static D_Game? Game;

        public static BlendState AmbientBlendState = new();
        public static LightingRenderer? LightingRenderer;

        public static Renderer? BufferRenderer;
        public static Renderer? FinalRenderer;

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
                Game = new D_Game
                {
                    Form = (Form?)Control.FromHandle(Window.Handle),
                    Zoom = 2
                };
                Game.Init(this, Window);
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

                if (Game != null &&
                    Game.GraphicsManager != null)
                {
                    Game.SpriteBatch = new SpriteBatch(Game.GraphicsManager.GraphicsDevice);

                    RenderingManager.InitDefaults(Game.GraphicsManager, Game.Resolution);

                    if (RenderingManager.AddLightingRenderer != null)
                    {
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
                    }

                    Handler.Init(this);

                    BufferRenderer = new Renderer(Handler.GetID(), "Buffer");
                    BufferRenderer.Init(Game.GraphicsManager, Game.Resolution);

                    FinalRenderer = new Renderer(Handler.GetID(), "Final");
                    FinalRenderer.Init(Game.GraphicsManager, Game.Resolution);

                    ShaderUtil.Init();
                    TimeTracker.Init();

                    if (!Game.GraphicsManager.IsFullScreen &&
                        Game.Form != null)
                    {
                        Game.Form.WindowState = FormWindowState.Maximized;
                    }
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
            try
            {
                if (Game != null)
                {
                    if (Window != null)
                    {
                        if (Window.ClientBounds.Width > 0 &&
                            Window.ClientBounds.Height > 0 &&
                            Game.Form != null)
                        {
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
                            }
                            else if (Game.Form.Focused)
                            {
                                LostFocus = false;
                                SoundManager.Paused = false;

                                InputManager.Update();
                                MenuManager.Update(Game.Game, Content);
                                SceneManager.Update(Game.Game, Content);
                                RenderingManager.Update();
                                WeatherManager.Update(Game.Resolution, Color.White);
                            }
                        }
                        else if (!LostFocus)
                        {
                            LostFocus = true;
                            MenuManager.GetMenu("Main")?.Open();
                            SoundManager.Paused = true;
                        }

                        SoundManager.Update();
                    }

                    if (Game.Quit)
                    {
                        SoundManager.StopAll();
                        Game.Game?.Exit();
                    }
                }
            }
            catch (Exception e)
            {
                Game?.CrashHandler(e);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            if (Game == null ||
                Game.Window == null ||
                Game.SpriteBatch == null ||
                Game.GraphicsManager == null ||
                RenderingManager.LightingRenderer == null ||
                RenderingManager.Lighting == null ||
                RenderingManager.AddLightingRenderer == null ||
                BufferRenderer?.RenderTarget == null ||
                FinalRenderer?.RenderTarget == null)
            {
                return;
            }

            //Don't bother drawing if the window is minimized
            if (Game.Window.ClientBounds.Width > 0 &&
                Game.Window.ClientBounds.Height > 0)
            {
                //Set ambient light in case the color changed
                RenderingManager.LightingRenderer.GraphicsClearColor = RenderingManager.Lighting.DrawColor;

                //Render lighting
                RenderingManager.LightingRenderer.Draw(Game.SpriteBatch, Game.Resolution);

                //=================================
                // Draw world to Buffer
                //---------------------------------
                Game.GraphicsManager.GraphicsDevice.SetRenderTarget(BufferRenderer.RenderTarget);
                Game.GraphicsManager.GraphicsDevice.Clear(Color.Black);

                //Render world
                Game.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
                SceneManager.Draw_WorldsOnly(Game.SpriteBatch, Game.Resolution, Color.White);
                Game.SpriteBatch.End();

                //Add lighting to world
                RenderingManager.AddLightingRenderer.Draw(Game.SpriteBatch, Game.Resolution);

                Game.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

                //Alt method with no lighting applied
                SceneManager.Draw_WorldsOnly(Game.SpriteBatch, Game.Resolution);

                Game.SpriteBatch.End();
                //---------------------------------
                // End of drawing to Buffer
                //=================================

                //Apply shaders
                if (ShaderUtil.RenderTarget_Blurred != null &&
                    TimeManager.Paused)
                {
                    if (ShaderUtil.RenderTarget_Blurred.Width != Game.Resolution.X ||
                        ShaderUtil.RenderTarget_Blurred.Height != Game.Resolution.Y)
                    {
                        ShaderUtil.RenderTarget_Blurred = new RenderTarget2D(Game.GraphicsManager.GraphicsDevice, Game.Resolution.X, Game.Resolution.Y);
                    }

                    Game.GraphicsManager.GraphicsDevice.SetRenderTarget(ShaderUtil.RenderTarget_Blurred);
                    Game.GraphicsManager.GraphicsDevice.Clear(Color.Transparent);

                    ShaderUtil.Apply_GaussianBlur(Game.SpriteBatch, 5, BufferRenderer.RenderTarget, new Region(0, 0, Game.Resolution.X, Game.Resolution.Y), false);
                    Game.GraphicsManager.GraphicsDevice.SetRenderTarget(BufferRenderer.RenderTarget);

                    ShaderUtil.Apply_GaussianBlur(Game.SpriteBatch, 5, ShaderUtil.RenderTarget_Blurred, new Region(0, 0, Game.Resolution.X, Game.Resolution.Y), true);
                }

                //Draw Buffer to Final RenderTarget
                Game.GraphicsManager.GraphicsDevice.SetRenderTarget(FinalRenderer.RenderTarget);
                Game.GraphicsManager.GraphicsDevice.Clear(Color.Black);

                Game.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);
                Game.SpriteBatch.Draw(BufferRenderer.RenderTarget, new Rectangle(0, 0, Game.Resolution.X, Game.Resolution.Y), Color.White);
                Game.SpriteBatch.End();

                //Draw Final RenderTarget to screen
                Game.GraphicsManager.GraphicsDevice.SetRenderTarget(null);
                Game.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);
                Game.SpriteBatch.Draw(FinalRenderer.RenderTarget, new Rectangle(0, 0, Game.Resolution.X, Game.Resolution.Y), Color.White);
                Game.SpriteBatch.End();

                //=================================
                // Draw menus
                //---------------------------------
                Game.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

                //Render scene specific menus
                SceneManager.Draw_MenusOnly(Game.SpriteBatch);

                //Render standalone menus
                MenuManager.Draw(Game.SpriteBatch);

                Game.SpriteBatch.End();
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

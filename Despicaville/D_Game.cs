using System;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OP_Engine.Logging;
using OP_Engine.Scenes;
using OP_Engine.Enums;
using OP_Engine.Rendering;

namespace Despicaville
{
    public class D_Game : OP_Game
    {
        #region Variables



        #endregion

        #region Constructors

        public D_Game()
        {

        }

        #endregion

        #region Methods

        public override void Init(Game game, GameWindow window)
        {
            Logger.LogFile = Environment.CurrentDirectory + @"\CrashLog.txt";

            Game = game;
            Game.Exiting += OnExit;

            Application.EnableVisualStyles();

            GraphicsManager = new GraphicsDeviceManager(game)
            {
                PreferredBackBufferWidth = ScreenWidth,
                PreferredBackBufferHeight = ScreenHeight,
                GraphicsProfile = GraphicsProfile.HiDef
            };

            MenuSize_X = ScreenWidth / 32;
            MenuSize_Y = ScreenHeight / 32;

            Window = window;
            Window.Position = new Point(0, 0);
            Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);

            //Default windowed
            ScreenType = ScreenType.Windowed;

            if (Form != null)
            {
                Form.WindowState = FormWindowState.Maximized;

                ResizeTickTimer = new System.Timers.Timer(1) { SynchronizingObject = Form, AutoReset = false };
                ResizeTickTimer.Elapsed += OnResizeTick;
                Form.ResizeBegin += GameForm_ResizeBegin;
                Form.Resize += GameForm_Resize;
                Form.ResizeEnd += GameForm_ResizeEnd;
            }
        }

        public override void ResetScreen()
        {
            if (Window == null ||
                GraphicsManager == null ||
                Form == null)
            {
                return;
            }

            if (Window.ClientBounds.Width > 0 &&
                Window.ClientBounds.Height > 0)
            {
                if (ScreenType == ScreenType.Fullscreen ||
                    ScreenType == ScreenType.BorderlessFullscreen)
                {
                    Window.Position = new Point(0, 0);

                    GraphicsManager.IsFullScreen = true;
                    GraphicsManager.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                    GraphicsManager.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

                    if (ScreenType == ScreenType.BorderlessFullscreen)
                    {
                        GraphicsManager.HardwareModeSwitch = false;
                        Form.WindowState = FormWindowState.Normal;
                        Window.AllowUserResizing = false;
                        Window.IsBorderless = true;
                    }
                    else
                    {
                        GraphicsManager.HardwareModeSwitch = true;
                    }
                }
                else if (ScreenType == ScreenType.Windowed)
                {
                    GraphicsManager.IsFullScreen = false;
                    GraphicsManager.PreferredBackBufferWidth = Window.ClientBounds.Width;
                    GraphicsManager.PreferredBackBufferHeight = Window.ClientBounds.Height;

                    Window.AllowUserResizing = true;
                    Window.IsBorderless = false;
                    GraphicsManager.HardwareModeSwitch = false;
                }

                ScreenWidth = GraphicsManager.PreferredBackBufferWidth;
                ScreenHeight = GraphicsManager.PreferredBackBufferHeight;

                GraphicsManager.ApplyChanges();

                if (RenderingManager.LightingRenderer != null &&
                    RenderingManager.AddLightingRenderer != null)
                {
                    RenderingManager.LightingRenderer.RenderTarget = new RenderTarget2D(GraphicsManager.GraphicsDevice, ScreenWidth, ScreenHeight);
                    RenderingManager.AddLightingRenderer.RenderTarget = RenderingManager.LightingRenderer.RenderTarget;
                }

                Main.BufferRenderer?.Init(GraphicsManager, new Point(ScreenWidth, ScreenHeight));
                Main.FinalRenderer?.Init(GraphicsManager, new Point(ScreenWidth, ScreenHeight));

                ResolutionChange();

                if (!Form.Visible)
                {
                    Form.Visible = true;
                }
            }
        }

        #endregion

        #region Events

        private void Window_ClientSizeChanged(object? sender, EventArgs e)
        {
            if (!IsResizeTickEnabled ||
                Form == null)
            { 
                return;
            }

            //This fires after the window has been manually resized
            //and on release of title bar being clicked and held
            if (Form.WindowState == LastWindowState)
            {
                ResetScreen();
            }
        }

        private void GameForm_ResizeBegin(object? sender, EventArgs e)
        {
            IsResizeTickEnabled = true;

            if (ResizeTickTimer != null)
            {
                ResizeTickTimer.Enabled = true;
            }
        }

        private void OnResizeTick(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (!IsResizeTickEnabled ||
                Game == null)
            {
                return;
            }

            ResizeScenes();
            ResizeMenus();
            Game.Tick();

            if (ResizeTickTimer != null)
            {
                ResizeTickTimer.Enabled = true;
            }
        }

        private void GameForm_Resize(object? sender, EventArgs e)
        {
            if (Form == null)
            {
                return;
            }

            //This is a resize that occurs from using Maximize Window button
            //and does not fire from manual resizing of the window
            if (Form.WindowState != LastWindowState)
            {
                LastWindowState = Form.WindowState;

                if (Form.WindowState == FormWindowState.Maximized ||
                    Form.WindowState == FormWindowState.Normal)
                {
                    if (SceneManager.Scenes != null)
                    {
                        ResetScreen();
                    }
                }
            }
        }

        private void GameForm_ResizeEnd(object? sender, EventArgs e)
        {
            IsResizeTickEnabled = false;

            if (ResizeTickTimer != null)
            {
                ResizeTickTimer.Enabled = false;
            }
        }

        #endregion
    }
}

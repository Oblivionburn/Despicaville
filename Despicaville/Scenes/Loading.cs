using System.Threading;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Scenes;
using OP_Engine.Sounds;
using OP_Engine.Menus;
using OP_Engine.Utility;
using OP_Engine.Tiles;
using OP_Engine.Characters;

using Despicaville.Util;

namespace Despicaville.Scenes
{
    public class Loading : Scene
    {
        #region Variables



        #endregion

        #region Constructors

        public Loading(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Loading";
            Load(content);
        }

        #endregion

        #region Methods

        public override void Update(Game gameRef, ContentManager content)
        {
            if (Visible)
            {
                if (SoundManager.NeedMusic)
                {
                    AssetManager.PlayMusic_Random("Loading", true);
                }

                ProgressBar bar = Menu.GetProgressBar("Loading1");
                bar.Max_Value = 1;

                UpdateMessagebar();

                if (Handler.Loading_Stage == 0)
                {
                    #region Load Textures

                    if (Handler.Loading_Percent == 0 &&
                        Handler.LoadingTask == null)
                    {
                        Handler.LoadingTokenSource = new CancellationTokenSource();
                        Handler.LoadingTask = Task.Factory.StartNew(() => Handler.LoadWorldTextures(), Handler.LoadingTokenSource.Token);
                    }

                    if (Handler.LoadingTask != null)
                    {
                        if (Handler.LoadingTask.Status == TaskStatus.RanToCompletion)
                        {
                            Handler.LoadingTask = null;
                            Handler.LoadingTokenSource.Dispose();
                            Handler.Loading_Percent = 0;
                            Handler.Loading_Stage++;
                        }
                    }

                    UpdateLoadbar();

                    #endregion
                }
                else if (Handler.Loading_Stage == 1)
                {
                    #region Load Assets

                    if (Handler.Loading_Percent == 0 &&
                        Handler.LoadingTask == null)
                    {
                        Handler.LoadingTokenSource = new CancellationTokenSource();
                        Handler.LoadingTask = Task.Factory.StartNew(() => Handler.LoadAssets(), Handler.LoadingTokenSource.Token);
                    }

                    if (Handler.LoadingTask != null)
                    {
                        if (Handler.LoadingTask.Status == TaskStatus.RanToCompletion)
                        {
                            Handler.LoadingTask = null;
                            Handler.LoadingTokenSource.Dispose();
                            Handler.Loading_Percent = 0;
                            Handler.Loading_Stage++;
                        }
                    }

                    UpdateLoadbar();

                    #endregion
                }
                else if (Handler.Loading_Stage == 2)
                {
                    #region Get Blocks

                    if (Handler.Loading_Percent == 0 &&
                        Handler.LoadingTask == null)
                    {
                        Handler.LoadingTokenSource = new CancellationTokenSource();
                        Handler.LoadingTask = Task.Factory.StartNew(() => WorldGen.GetBlocks(), Handler.LoadingTokenSource.Token);
                    }

                    if (Handler.LoadingTask != null)
                    {
                        if (Handler.LoadingTask.Status == TaskStatus.RanToCompletion)
                        {
                            Handler.LoadingTask = null;
                            Handler.LoadingTokenSource.Dispose();
                            Handler.Loading_Percent = 0;
                            Handler.Loading_Stage++;
                        }
                    }

                    UpdateLoadbar();

                    #endregion
                }
                else if (Handler.Loading_Stage == 3)
                {
                    #region Logo

                    Handler.Loading_Stage++;
                    SceneManager.ChangeScene("Logo");
                    AssetManager.PlaySound("Logo");

                    #endregion
                }
                else if (Handler.Loading_Stage == 4)
                {
                    #region GenMap

                    if (Handler.Loading_Percent == 0 &&
                        Handler.LoadingTask == null)
                    {
                        Handler.LoadingTokenSource = new CancellationTokenSource();
                        Handler.LoadingTask = Task.Factory.StartNew(() => WorldGen.GenMap(), Handler.LoadingTokenSource.Token);
                    }

                    if (Handler.LoadingTask != null)
                    {
                        if (Handler.LoadingTask.Status == TaskStatus.RanToCompletion)
                        {
                            Handler.LoadingTask = null;
                            Handler.LoadingTokenSource.Dispose();
                            Handler.Loading_Percent = 0;
                            Handler.Loading_Stage++;
                        }
                    }

                    UpdateLoadbar();

                    #endregion
                }
                else if (Handler.Loading_Stage == 5)
                {
                    #region GenTown

                    if (Handler.Loading_Percent == 0 &&
                        Handler.LoadingTask == null)
                    {
                        Handler.LoadingTokenSource = new CancellationTokenSource();
                        Handler.LoadingTask = Task.Factory.StartNew(() => WorldGen.GenTown(), Handler.LoadingTokenSource.Token);
                    }

                    if (Handler.LoadingTask != null)
                    {
                        if (Handler.LoadingTask.Status == TaskStatus.RanToCompletion)
                        {
                            Handler.LoadingTask = null;
                            Handler.LoadingTokenSource.Dispose();
                            Handler.Loading_Percent = 0;
                            Handler.Loading_Stage++;
                        }
                    }

                    UpdateLoadbar();

                    #endregion
                }
                else if (Handler.Loading_Stage == 6)
                {
                    #region GenLoot

                    if (Handler.Loading_Percent == 0 &&
                        Handler.LoadingTask == null)
                    {
                        Handler.LoadingTokenSource = new CancellationTokenSource();
                        Handler.LoadingTask = Task.Factory.StartNew(() => WorldGen.GenLoot(), Handler.LoadingTokenSource.Token);
                    }

                    if (Handler.LoadingTask != null)
                    {
                        if (Handler.LoadingTask.Status == TaskStatus.RanToCompletion)
                        {
                            Handler.LoadingTask = null;
                            Handler.LoadingTokenSource.Dispose();
                            Handler.Loading_Percent = 0;
                            Handler.Loading_Stage++;
                        }
                    }

                    UpdateLoadbar();

                    #endregion
                }
                else if (Handler.Loading_Stage == 7)
                {
                    #region Ready

                    AssetManager.PlaySound("Ready");

                    Menu.GetButton("Next").Visible = true;

                    Handler.Loading_Percent = 100;
                    Handler.Loading_Message = "Ready!";
                    UpdateLoadbar();

                    Handler.Loading_Stage++;

                    #endregion
                }

                UpdateControls();
                base.Update(gameRef, content);
            }
        }

        private void UpdateControls()
        {
            bool found = false;

            foreach (Button button in Menu.Buttons)
            {
                if (button.Visible &&
                    button.Enabled)
                {
                    if (InputManager.MouseWithin(button.Region.ToRectangle))
                    {
                        found = true;
                        if (button.HoverText != null)
                        {
                            GameUtil.Examine(Menu, button.HoverText);
                        }

                        button.Opacity = 1;
                        button.Selected = true;

                        if (InputManager.Mouse_LB_Pressed)
                        {
                            found = false;
                            CheckClick(button);
                            break;
                        }
                    }
                    else if (InputManager.Mouse_Moved)
                    {
                        button.Opacity = 0.8f;
                        button.Selected = false;
                    }
                }
            }

            if (!found)
            {
                Menu.GetLabel("Examine").Visible = false;
            }
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");

            if (button.Name == "Back")
            {
                Back();
            }
            else if (button.Name == "Next")
            {
                Finish();
            }

            button.Opacity = 0.8f;
            button.Selected = false;
        }

        private void Back()
        {
            if (Handler.LoadingTask != null)
            {
                Handler.LoadingTokenSource.Cancel();
                Handler.LoadingTask = null;
            }

            Handler.Loading_Stage = 3;
            Handler.Loading_Percent = 0;
            SceneManager.GetScene("Gameplay").World = null;

            SoundManager.StopMusic();
            SoundManager.NeedMusic = true;

            SceneManager.ChangeScene("Title");

            Menu main = MenuManager.GetMenu("Main");
            main.Active = true;
            main.Visible = true;
        }

        private void UpdateMessagebar()
        {
            ProgressBar bar = Menu.GetProgressBar("Loading1");
            Label label = Menu.GetLabel("Loading1");

            bar.Value = Handler.Loading_Stage;
            label.Text = Handler.Loading_Message;

            float CurrentVal = ((float)bar.Bar_Texture.Width / 100) * bar.Value;
            bar.Bar_Image = new Rectangle(bar.Bar_Image.X, bar.Bar_Image.Y, (int)CurrentVal, bar.Bar_Image.Height);

            CurrentVal = ((float)bar.Base_Region.Width / 100) * bar.Value;
            bar.Bar_Region = new Region(bar.Base_Region.X, bar.Base_Region.Y, (int)CurrentVal, bar.Base_Region.Height);
        }

        private void UpdateLoadbar()
        {
            ProgressBar bar = Menu.GetProgressBar("Loading2");
            Label label = Menu.GetLabel("Loading2");

            bar.Value = Handler.Loading_Percent;
            label.Text = Handler.Loading_Percent.ToString() + "%";

            float CurrentVal = ((float)bar.Bar_Texture.Width / 100) * bar.Value;
            bar.Bar_Image = new Rectangle(bar.Bar_Image.X, bar.Bar_Image.Y, (int)CurrentVal, bar.Bar_Image.Height);

            CurrentVal = ((float)bar.Base_Region.Width / 100) * bar.Value;
            bar.Bar_Region = new Region(bar.Base_Region.X, bar.Base_Region.Y, (int)CurrentVal, bar.Base_Region.Height);
        }

        private void Finish()
        {
            Character player = Handler.GetPlayer();
            World world = SceneManager.GetScene("Gameplay").World;

            WorldUtil.AssignPlayerBed(world, player);
            if (player.Location == null)
            {
                player.Location = new Location(0, 0, 0);
            }

            if (player.Location.X == 0 &&
                player.Location.Y == 0)
            {
                Map map = world.Maps[0];
                Layer bottom_tiles = map.GetLayer("BottomTiles");

                Tile center = bottom_tiles.GetTile(new Vector2(bottom_tiles.Columns / 2, bottom_tiles.Rows / 2));
                player.Location = new Location(center.Location.X, center.Location.Y, 0);
            }

            WorldUtil.SetCurrentMap(player);

            Menu.GetButton("Next").Visible = false;

            GameUtil.Start();
        }

        public override void Load(ContentManager content)
        {
            Menu.Clear();

            Menu.AddPicture(Handler.GetID(), "Loading", AssetManager.Textures["Loading"], new Region(0, 0, Main.Game.ScreenWidth, Main.Game.ScreenHeight), Color.White, true);

            Menu.AddButton(Handler.GetID(), "Next", AssetManager.Textures["Button_Next"], AssetManager.Textures["Button_Next_Hover"], AssetManager.Textures["Button_Next_Disabled"],
                new Region(0, 0, 0, 0), Color.White, false);
            Menu.GetButton("Next").HoverText = "Next";

            Menu.AddProgressBar(Handler.GetID(), "Loading1", 100, 0, 1, AssetManager.Textures["ProgressBase_Large"], AssetManager.Textures["ProgressBar_Large"],
                new Region(0, 0, 0, 0), new Color(100, 0, 0, 255), true);
            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Loading1", "Initializing...", Color.White, new Region(0, 0, 0, 0), true);

            Menu.AddProgressBar(Handler.GetID(), "Loading2", 100, 0, 1, AssetManager.Textures["ProgressBase_Large"], AssetManager.Textures["ProgressBar_Large"],
                new Region(0, 0, 0, 0), new Color(100, 0, 0, 255), true);
            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Loading2", "0%", Color.White, new Region(0, 0, 0, 0), true);

            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["Frame"], new Region(0, 0, 0, 0), false);

            Menu.Visible = true;

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            Menu.GetPicture("Loading").Region = new Region(0, 0, Main.Game.ScreenWidth, Main.Game.ScreenHeight);

            int Width = Main.Game.MenuSize_X;
            int Height = Main.Game.MenuSize_Y;

            int X = Main.Game.ScreenWidth / 2;
            int Y = (Main.Game.ScreenHeight / 20) * 12;

            ProgressBar bar = Menu.GetProgressBar("Loading1");
            bar.Base_Region = new Region(X - (Width * 8), Y, Width * 16, Height);
            Menu.GetLabel("Loading1").Region = new Region(bar.Base_Region.X, bar.Base_Region.Y, bar.Base_Region.Width, bar.Base_Region.Height);

            ProgressBar bar2 = Menu.GetProgressBar("Loading2");
            bar2.Base_Region = new Region(X - (Width * 8), Y + Height, Width * 16, Height);
            Menu.GetLabel("Loading2").Region = new Region(bar2.Base_Region.X, bar2.Base_Region.Y, bar2.Base_Region.Width, bar2.Base_Region.Height);

            Menu.GetButton("Next").Region = new Region(X - (Width / 2), Y + (Height * 3), Width, Height);
        }

        #endregion
    }
}

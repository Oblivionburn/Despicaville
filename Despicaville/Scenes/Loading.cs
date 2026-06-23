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
using Despicaville.Util;
using OP_Engine.Characters;

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

        public override void Update(Game? gameRef, ContentManager? content)
        {
            if (Visible)
            {
                if (SoundManager.NeedMusic)
                {
                    AssetManager.PlayMusic_Random("Loading", true);
                }

                ProgressBar? bar = Menu?.GetProgressBar("Loading1");
                if (bar != null)
                {
                    bar.Max_Value = 1;
                }

                UpdateMessagebar();

                //if (Handler.Loading_Stage == 0)
                //{
                //    #region Load Textures
                //
                //    if (Handler.Loading_Percent == 0 &&
                //        Handler.LoadingTask == null)
                //    {
                //        Handler.LoadingTokenSource = new CancellationTokenSource();
                //        Handler.LoadingTask = Task.Factory.StartNew(() => Handler.LoadWorldTextures(), Handler.LoadingTokenSource.Token);
                //    }
                //
                //    if (Handler.LoadingTask != null)
                //    {
                //        if (Handler.LoadingTask.Status == TaskStatus.RanToCompletion)
                //        {
                //            Handler.LoadingTask = null;
                //            Handler.LoadingTokenSource.Dispose();
                //            Handler.Loading_Percent = 0;
                //            Handler.Loading_Stage++;
                //        }
                //    }
                //
                //    UpdateLoadbar();
                //
                //    #endregion
                //}
                if (Handler.Loading_Stage == 0)
                {
                    #region Load Mods

                    if (Handler.Loading_Percent == 0 &&
                        Handler.LoadingTask == null)
                    {
                        Handler.LoadingTokenSource = new CancellationTokenSource();
                        Handler.LoadingTask = Task.Factory.StartNew(() => ModUtil.LoadMods(), Handler.LoadingTokenSource.Token);
                    }

                    if (Handler.LoadingTask != null)
                    {
                        if (Handler.LoadingTask.Status == TaskStatus.RanToCompletion)
                        {
                            Handler.LoadingTask = null;
                            Handler.LoadingTokenSource?.Dispose();
                            Handler.Loading_Percent = 0;
                            Handler.Loading_Stage++;
                        }
                    }

                    UpdateLoadbar();

                    #endregion
                }
                else if (Handler.Loading_Stage == 1)
                {
                    #region Logo

                    Handler.Loading_Stage++;
                    SceneManager.ChangeScene("Logo");
                    AssetManager.PlaySound("Logo");

                    #endregion
                }
                else if (Handler.Loading_Stage == 2)
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
                            Handler.LoadingTokenSource?.Dispose();
                            Handler.Loading_Percent = 0;
                            Handler.Loading_Stage++;
                        }
                    }

                    UpdateLoadbar();

                    #endregion
                }
                else if (Handler.Loading_Stage == 3)
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
                            Handler.LoadingTokenSource?.Dispose();
                            Handler.Loading_Percent = 0;
                            Handler.Loading_Stage++;
                        }
                    }

                    UpdateLoadbar();

                    #endregion
                }
                else if (Handler.Loading_Stage == 4)
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
                            Handler.LoadingTokenSource?.Dispose();
                            Handler.Loading_Percent = 0;
                            Handler.Loading_Stage++;
                        }
                    }

                    UpdateLoadbar();

                    #endregion
                }
                else if (Handler.Loading_Stage == 5)
                {
                    #region Ready

                    AssetManager.PlaySound("Ready");

                    Button? next = Menu?.GetButton("Next");
                    if (next != null)
                    {
                        next.Visible = true;
                    }

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

            if (Menu == null)
            {
                return;
            }

            foreach (Button button in Menu.Buttons)
            {
                if (button.Visible &&
                    button.Enabled)
                {
                    if (button.Region != null &&
                        InputManager.MouseWithin(button.Region.ToRectangle))
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
                Label? examine = Menu.GetLabel("Examine");
                if (examine != null)
                {
                    examine.Visible = false;
                }
            }
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");
            InputManager.Mouse?.Flush();

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
                Handler.LoadingTokenSource?.Cancel();
                Handler.LoadingTask = null;
            }

            Handler.Loading_Stage = 3;
            Handler.Loading_Percent = 0;

            Scene? gameplay = SceneManager.GetScene("Gameplay");
            if (gameplay != null)
            {
                gameplay.World = new();
            }

            SoundManager.StopMusic();
            SoundManager.NeedMusic = true;

            SceneManager.ChangeScene("Title");

            Menu? main = MenuManager.GetMenu("Main");
            if (main != null)
            {
                main.Active = true;
                main.Visible = true;
            }
        }

        private void UpdateMessagebar()
        {
            Label? label = Menu?.GetLabel("Loading1");
            if (label != null)
            {
                label.Text = Handler.Loading_Message;
            }

            ProgressBar? bar = Menu?.GetProgressBar("Loading1");
            if (bar != null &&
                bar.Bar_Texture != null &&
                bar.Base_Region != null)
            {
                bar.Value = Handler.Loading_Stage;

                float CurrentVal = ((float)bar.Bar_Texture.Width / 100) * bar.Value;
                bar.Bar_Image = new Rectangle(bar.Bar_Image.X, bar.Bar_Image.Y, (int)CurrentVal, bar.Bar_Image.Height);

                CurrentVal = ((float)bar.Base_Region.Width / 100) * bar.Value;
                bar.Bar_Region = new Region(bar.Base_Region.X, bar.Base_Region.Y, (int)CurrentVal, bar.Base_Region.Height);
            }
        }

        private void UpdateLoadbar()
        {
            Label? label = Menu?.GetLabel("Loading2");
            if (label != null)
            {
                label.Text = Handler.Loading_Percent.ToString() + "%";
            }

            ProgressBar? bar = Menu?.GetProgressBar("Loading2");
            if (bar != null &&
                bar.Bar_Texture != null &&
                bar.Base_Region != null)
            {
                bar.Value = Handler.Loading_Percent;

                float CurrentVal = ((float)bar.Bar_Texture.Width / 100) * bar.Value;
                bar.Bar_Image = new Rectangle(bar.Bar_Image.X, bar.Bar_Image.Y, (int)CurrentVal, bar.Bar_Image.Height);

                CurrentVal = ((float)bar.Base_Region.Width / 100) * bar.Value;
                bar.Bar_Region = new Region(bar.Base_Region.X, bar.Base_Region.Y, (int)CurrentVal, bar.Base_Region.Height);
            }
        }

        private void Finish()
        {
            Scene? gameplay = SceneManager.GetScene("Gameplay");
            if (gameplay != null)
            {
                World? world = gameplay.World;
                if (world != null)
                {
                    WorldUtil.AssignPlayerBed(world);
                }

                Character? player = Handler.Player;
                if (player != null)
                {
                    if (player.Location == null)
                    {
                        player.Location = new Location(0, 0, 0);
                    }
                    else if (player.Location.X == 0 &&
                             player.Location.Y == 0)
                    {
                        Map? map = world?.Maps[0];
                        Layer? bottom_tiles = map?.GetLayer("BottomTiles");

                        Tile? center = bottom_tiles?.GetTile(new Vector2(bottom_tiles.Columns / 2, bottom_tiles.Rows / 2));
                        if (center != null)
                        {
                            player.Location = new Location(center.Location.X, center.Location.Y, 0);
                        }
                    }

                    WorldUtil.SetCurrentMap(player);

                    Button? next = Menu?.GetButton("Next");
                    if (next != null)
                    {
                        next.Visible = false;
                    }

                    GameUtil.Start();
                }
            }
        }

        public override void Load(ContentManager content)
        {
            if (Main.Game == null)
            {
                return;
            }
            if (Menu == null)
            {
                return;
            }

            Menu.Clear();

            Menu.AddPicture(Handler.GetID(), "Loading", Handler.GetTexture("Loading"), new Region(0, 0, Main.Game.ScreenWidth, Main.Game.ScreenHeight), Color.White, true);

            Menu.AddButton(Handler.GetID(), "Next", Handler.GetTexture("Button_Next"), Handler.GetTexture("Button_Next_Hover"), Handler.GetTexture("Button_Next_Disabled"),
                new Region(0, 0, 0, 0), Color.White, false);

            Button? next = Menu.GetButton("Next");
            if (next != null)
            {
                next.HoverText = "Next";
            }

            Menu.AddProgressBar(Handler.GetID(), "Loading1", 100, 0, 1, Handler.GetTexture("ProgressBase_Large"), Handler.GetTexture("ProgressBar_Large"),
                new Region(0, 0, 0, 0), new Color(100, 0, 0, 255), true);
            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Loading1", "Initializing...", Color.White, new Region(0, 0, 0, 0), true);

            Menu.AddProgressBar(Handler.GetID(), "Loading2", 100, 0, 1, Handler.GetTexture("ProgressBase_Large"), Handler.GetTexture("ProgressBar_Large"),
                new Region(0, 0, 0, 0), new Color(100, 0, 0, 255), true);
            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Loading2", "0%", Color.White, new Region(0, 0, 0, 0), true);

            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, Handler.GetTexture("Frame"), new Region(0, 0, 0, 0), false);

            Menu.Visible = true;

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            if (Main.Game == null)
            {
                return;
            }
            if (Menu == null)
            {
                return;
            }

            Picture? loading = Menu.GetPicture("Loading");
            if (loading != null)
            {
                loading.Region = new Region(0, 0, Main.Game.ScreenWidth, Main.Game.ScreenHeight);
            }

            int Width = (int)Main.Game.MenuSize_X;
            int Height = (int)Main.Game.MenuSize_Y;

            int X = Main.Game.ScreenWidth / 2;
            int Y = (Main.Game.ScreenHeight / 20) * 12;

            ProgressBar? bar = Menu.GetProgressBar("Loading1");
            if (bar != null)
            {
                bar.Base_Region = new Region(X - (Width * 8), Y, Width * 16, Height);

                Label? loading1 = Menu.GetLabel("Loading1");
                if (loading1 != null)
                {
                    loading1.Region = new Region(bar.Base_Region.X, bar.Base_Region.Y, bar.Base_Region.Width, bar.Base_Region.Height);
                }
            }

            ProgressBar? bar2 = Menu.GetProgressBar("Loading2");
            if (bar2 != null)
            {
                bar2.Base_Region = new Region(X - (Width * 8), Y + Height, Width * 16, Height);

                Label? loading2 = Menu.GetLabel("Loading2");
                if (loading2 != null)
                {
                    loading2.Region = new Region(bar2.Base_Region.X, bar2.Base_Region.Y, bar2.Base_Region.Width, bar2.Base_Region.Height);
                }
            }

            Button? next = Menu.GetButton("Next");
            if (next != null)
            {
                next.Region = new Region(X - (Width / 2), Y + (Height * 3), Width, Height);
            }
        }

        #endregion
    }
}

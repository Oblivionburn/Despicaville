using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Menus;
using OP_Engine.Characters;
using OP_Engine.Utility;
using OP_Engine.Time;

using Despicaville.Util;

namespace Despicaville.Menus
{
    public class Menu_UI : Menu
    {
        #region Variables

        private int InitBody;
        private bool ShowHealth;

        #endregion

        #region Constructor

        public Menu_UI(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "UI";
            Load(content);
        }

        #endregion

        #region Methods

        public override void Update(Game gameRef, ContentManager content)
        {
            if (Visible ||
                Active)
            {
                if (!TimeManager.Paused)
                {
                    UpdateControls();
                    UpdateTime();

                    if (InitBody == 0)
                    {
                        InitBodyDisplay();
                        InitBody = 1;
                    }
                    
                    UpdateStats();

                    base.Update(gameRef, content);
                }
            }
        }

        private void UpdateControls()
        {
            bool found = false;

            foreach (Button button in Buttons)
            {
                if (button.Visible &&
                    button.Enabled)
                {
                    if (InputManager.MouseWithin(button.Region.ToRectangle))
                    {
                        found = true;
                        if (button.HoverText != null)
                        {
                            GameUtil.Examine(this, button.HoverText);
                        }

                        button.Opacity = 1;
                        button.Selected = true;

                        if (InputManager.Mouse_LB_Pressed)
                        {
                            found = false;
                            CheckClick(button);

                            button.Opacity = 0.8f;
                            button.Selected = false;

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

            foreach (Label label in Labels)
            {
                if (label.Visible)
                {
                    if (label.Name == "Crouching")
                    {
                        if (InputManager.KeyDown("Crouch"))
                        {
                            label.Opacity = 1;
                            label.TextColor = Color.LimeGreen;
                        }
                        else
                        {
                            label.Opacity = 0.6f;
                            label.TextColor = Color.White;
                        }
                    }
                    else if (label.Name == "Running")
                    {
                        if (InputManager.KeyDown("Run"))
                        {
                            label.Opacity = 1;
                            label.TextColor = Color.LimeGreen;
                        }
                        else
                        {
                            label.Opacity = 0.6f;
                            label.TextColor = Color.White;
                        }
                    }

                    if (InputManager.MouseWithin(label.Region.ToRectangle))
                    {
                        if (label.HoverText != null)
                        {
                            found = true;
                            Examine(label.HoverText);
                        }

                        break;
                    }
                }
            }

            foreach (Picture picture in Pictures)
            {
                if (picture.Visible &&
                    picture.Name.Contains("Paperdoll"))
                {
                    if (InputManager.MouseWithin(picture.Region.ToRectangle))
                    {
                        if (picture.Value == 1)
                        {
                            GameUtil.ResetHover(picture);
                            break;
                        }
                    }
                }
            }

            foreach (Picture picture in Pictures)
            {
                if (picture.Visible &&
                    picture.Name.Contains("Paperdoll"))
                {
                    if (InputManager.MouseWithin(picture.Region.ToRectangle))
                    {
                        if (GameUtil.MouseOnPixel(picture))
                        {
                            found = true;

                            if (picture.HoverText != null)
                            {
                                GameUtil.Examine(this, picture.HoverText);
                            }

                            if (InputManager.Mouse_LB_Pressed)
                            {
                                found = false;
                                GameUtil.ResetHover(picture);
                                CheckClick(picture);
                            }

                            break;
                        }
                    }
                }
            }

            if (!found)
            {
                GetLabel("Examine").Visible = false;
            }
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");

            if (button.Name == "Main")
            {
                TimeManager.Paused = true;

                Menu main = MenuManager.GetMenu(button.Name);
                main.Active = true;
                main.Visible = true;
            }
            else if (button.Name == "Inventory")
            {
                TimeManager.Paused = true;

                Menu main = MenuManager.GetMenu(button.Name);
                main.Load();
                main.Active = true;
                main.Visible = true;
            }
            else if (button.Name == "Health")
            {
                if (ShowHealth)
                {
                    GetPicture("Paperdoll_Right_Foot").Visible = false;
                    GetPicture("Paperdoll_Left_Foot").Visible = false;
                    GetPicture("Paperdoll_Right_Leg").Visible = false;
                    GetPicture("Paperdoll_Left_Leg").Visible = false;
                    GetPicture("Paperdoll_Right_Hand").Visible = false;
                    GetPicture("Paperdoll_Left_Hand").Visible = false;
                    GetPicture("Paperdoll_Right_Arm").Visible = false;
                    GetPicture("Paperdoll_Left_Arm").Visible = false;
                    GetPicture("Paperdoll_Groin").Visible = false;
                    GetPicture("Paperdoll_Torso").Visible = false;
                    GetPicture("Paperdoll_Neck").Visible = false;
                    GetPicture("Paperdoll_Head").Visible = false;

                    ShowHealth = false;
                }
                else
                {
                    GetPicture("Paperdoll_Right_Foot").Visible = true;
                    GetPicture("Paperdoll_Left_Foot").Visible = true;
                    GetPicture("Paperdoll_Right_Leg").Visible = true;
                    GetPicture("Paperdoll_Left_Leg").Visible = true;
                    GetPicture("Paperdoll_Right_Hand").Visible = true;
                    GetPicture("Paperdoll_Left_Hand").Visible = true;
                    GetPicture("Paperdoll_Right_Arm").Visible = true;
                    GetPicture("Paperdoll_Left_Arm").Visible = true;
                    GetPicture("Paperdoll_Groin").Visible = true;
                    GetPicture("Paperdoll_Torso").Visible = true;
                    GetPicture("Paperdoll_Neck").Visible = true;
                    GetPicture("Paperdoll_Head").Visible = true;

                    ShowHealth = true;
                }
            }
        }

        private void CheckClick(Picture picture)
        {
            AssetManager.PlaySound_Random("Click");

            if (picture.Name.Contains("Paperdoll"))
            {
                Handler.Selected_BodyPart = picture.Name.Substring(10);

                TimeManager.Paused = true;

                Menu menu = MenuManager.GetMenu("Wounds");
                menu.Load();
                menu.Active = true;
                menu.Visible = true;
            }
        }

        private void UpdateTime()
        {
            Label time = GetLabel("Time");
            if (time != null)
            {
                if (TimeManager.Now != null)
                {
                    string[] time_array = TimeManager.Now.ToString(false, true, true).Split(' ');
                    time.Text = time_array[0] + "\n" + time_array[1] + " " + time_array[2];
                }
            }
        }

        private void UpdateStats()
        {
            Character player = Handler.GetPlayer();
            if (player != null)
            {
                string[] stats = { "Hunger", "Thirst", "Bladder", "Grime", "Pain", "Paranoia", "Blood", "Consciousness", "Stamina", "Comfort" };
                foreach (string stat_name in stats)
                {
                    Something stat = player.GetStat(stat_name);
                    if (stat != null)
                    {
                        ProgressBar bar = GetProgressBar(stat_name);
                        if (bar != null)
                        {
                            bar.Value = stat.Value;
                            bar.Update();
                        }

                        Label label = GetLabel(stat_name);
                        if (label != null)
                        {
                            label.HoverText = stat.Description;
                            label.Text = stat_name + ": " + stat.Value.ToString("0.##") + "/" + stat.Max_Value + "%";
                        }
                    }
                }

                foreach (string body_part in Handler.BodyParts)
                {
                    Picture picture = GetPicture("Paperdoll_" + body_part);
                    if (picture != null)
                    {
                        BodyPart bodyPart = player.GetBodyPart(body_part);
                        if (bodyPart != null)
                        {
                            Something hp = bodyPart.GetStat("HP");
                            if (hp != null)
                            {
                                picture.HoverText = hp.Name + ": " + hp.Value.ToString("0.##") + "/" + (int)hp.Max_Value + "%";
                            }
                        }
                    }
                }
            }
        }

        private void InitBodyDisplay()
        {
            Character player = Handler.GetPlayer();

            foreach (string body_part in Handler.BodyParts)
            {
                CombatUtil.Update_Player_BodyStat(player, body_part);
            }
        }

        public override void Load(ContentManager content)
        {
            Clear();

            AddPicture(Handler.GetID(), "Panel_Upper_Left", AssetManager.Textures["Frame_Large"], new Region(0, 0, 0, 0), Color.White, false);
            AddPicture(Handler.GetID(), "Panel_Upper_Right", AssetManager.Textures["Frame_Large"], new Region(0, 0, 0, 0), Color.White, false);
            AddPicture(Handler.GetID(), "Panel_Upper_Center", AssetManager.Textures["Frame_Wide"], new Region(0, 0, 0, 0), Color.White * 0f, false);
            AddPicture(Handler.GetID(), "Panel_Lower_Left", AssetManager.Textures["Frame_Large"], new Region(0, 0, 0, 0), Color.White, false);
            AddPicture(Handler.GetID(), "Panel_Lower_Right", AssetManager.Textures["Frame_Large"], new Region(0, 0, 0, 0), Color.White, false);
            AddPicture(Handler.GetID(), "Panel_Lower_Center", AssetManager.Textures["Frame_Wide"], new Region(0, 0, 0, 0), Color.White * 0.6f, true);

            AddPicture(Handler.GetID(), "Paperdoll_Right_Foot", AssetManager.Textures["Paperdoll_Right_Foot"], new Region(0, 0, 0, 0), Color.White, false);
            AddPicture(Handler.GetID(), "Paperdoll_Left_Foot", AssetManager.Textures["Paperdoll_Left_Foot"], new Region(0, 0, 0, 0), Color.White, false);
            AddPicture(Handler.GetID(), "Paperdoll_Right_Leg", AssetManager.Textures["Paperdoll_Right_Leg"], new Region(0, 0, 0, 0), Color.White, false);
            AddPicture(Handler.GetID(), "Paperdoll_Left_Leg", AssetManager.Textures["Paperdoll_Left_Leg"], new Region(0, 0, 0, 0), Color.White, false);
            AddPicture(Handler.GetID(), "Paperdoll_Right_Hand", AssetManager.Textures["Paperdoll_Right_Hand"], new Region(0, 0, 0, 0), Color.White, false);
            AddPicture(Handler.GetID(), "Paperdoll_Left_Hand", AssetManager.Textures["Paperdoll_Left_Hand"], new Region(0, 0, 0, 0), Color.White, false);
            AddPicture(Handler.GetID(), "Paperdoll_Right_Arm", AssetManager.Textures["Paperdoll_Right_Arm"], new Region(0, 0, 0, 0), Color.White, false);
            AddPicture(Handler.GetID(), "Paperdoll_Left_Arm", AssetManager.Textures["Paperdoll_Left_Arm"], new Region(0, 0, 0, 0), Color.White, false);
            AddPicture(Handler.GetID(), "Paperdoll_Groin", AssetManager.Textures["Paperdoll_Groin"], new Region(0, 0, 0, 0), Color.White, false);
            AddPicture(Handler.GetID(), "Paperdoll_Torso", AssetManager.Textures["Paperdoll_Torso"], new Region(0, 0, 0, 0), Color.White, false);
            AddPicture(Handler.GetID(), "Paperdoll_Neck", AssetManager.Textures["Paperdoll_Neck"], new Region(0, 0, 0, 0), Color.White, false);
            AddPicture(Handler.GetID(), "Paperdoll_Head", AssetManager.Textures["Paperdoll_Head"], new Region(0, 0, 0, 0), Color.White, false);
            
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Time", "", Color.White, AssetManager.Textures["Frame_Wide"], new Region(0, 0, 0, 0), Color.White * 0f, true);

            AddProgressBar(Handler.GetID(), "Hunger", 100, 0, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                new Region(0, 0, 0, 0), new Color(0, 100, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Hunger", "Hunger: 0%", Color.White, new Region(0, 0, 0, 0), true);

            AddProgressBar(Handler.GetID(), "Thirst", 100, 0, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                new Region(0, 0, 0, 0), new Color(0, 0, 100), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Thirst", "Thirst: 0%", Color.White, new Region(0, 0, 0, 0), true);

            AddProgressBar(Handler.GetID(), "Bladder", 100, 0, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                new Region(0, 0, 0, 0), new Color(100, 100, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Bladder", "Bladder: 0%", Color.White, new Region(0, 0, 0, 0), true);

            AddProgressBar(Handler.GetID(), "Grime", 100, 0, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                new Region(0, 0, 0, 0), new Color(50, 40, 30), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Grime", "Grime: 0%", Color.White, new Region(0, 0, 0, 0), true);

            AddProgressBar(Handler.GetID(), "Pain", 100, 0, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                new Region(0, 0, 0, 0), Color.Red, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Pain", "Pain: 0%", Color.White, new Region(0, 0, 0, 0), true);

            AddProgressBar(Handler.GetID(), "Paranoia", 100, 0, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                new Region(0, 0, 0, 0), new Color(60, 0, 100), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Paranoia", "Paranoia: 0%", Color.White, new Region(0, 0, 0, 0), true);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Crouching", "Quiet | Crouch", Color.White, new Region(0, 0, 0, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Running", "Loud | Run", Color.White, new Region(0, 0, 0, 0), true);

            AddProgressBar(Handler.GetID(), "Blood", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                new Region(0, 0, 0, 0), new Color(100, 0, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Blood", "Blood: 100%", Color.White, new Region(0, 0, 0, 0), true);

            AddProgressBar(Handler.GetID(), "Consciousness", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                new Region(0, 0, 0, 0), new Color(128, 64, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Consciousness", "Consciousness: 100%", Color.White, new Region(0, 0, 0, 0), true);

            AddProgressBar(Handler.GetID(), "Stamina", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                new Region(0, 0, 0, 0), new Color(0, 64, 128), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Stamina", "Stamina: 100%", Color.White, new Region(0, 0, 0, 0), true);

            AddProgressBar(Handler.GetID(), "Comfort", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                new Region(0, 0, 0, 0), new Color(64, 128, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Comfort", "Comfort: 100%", Color.White, new Region(0, 0, 0, 0), true);

            AddButton(Handler.GetID(), "Main", AssetManager.Textures["Button_Menu"], AssetManager.Textures["Button_Menu_Hover"], null,
                new Region(0, 0, 0, 0), Color.White, true);
            GetButton("Main").HoverText = "System";

            AddButton(Handler.GetID(), "Inventory", AssetManager.Textures["Button_Inventory"], AssetManager.Textures["Button_Inventory_Hover"], null,
                new Region(0, 0, 0, 0), Color.White, true);
            GetButton("Inventory").HoverText = "Inventory";

            AddButton(Handler.GetID(), "Health", AssetManager.Textures["Button_Health"], AssetManager.Textures["Button_Health_Hover"], null,
                new Region(0, 0, 0, 0), Color.White, true);
            GetButton("Health").HoverText = "Health";

            AddButton(Handler.GetID(), "Stats", AssetManager.Textures["Button_Stats"], AssetManager.Textures["Button_Stats_Hover"], null,
                new Region(0, 0, 0, 0), Color.White, false);
            GetButton("Stats").HoverText = "Stats";

            AddButton(Handler.GetID(), "Skills", AssetManager.Textures["Button_Skills"], AssetManager.Textures["Button_Skills_Hover"], null,
                new Region(0, 0, 0, 0), Color.White, false);
            GetButton("Skills").HoverText = "Skills";

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["Frame"], new Region(0, 0, 0, 0), false);

            for (int i = 0; i < Handler.MessageMax; i++)
            {
                AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Message" + i.ToString(), "", Color.Red, new Region(0, 0, 0, 0), true);

                Label label = GetLabel("Message" + i.ToString());
                label.Alignment_Horizontal = Alignment.Left;
                label.AutoScale = false;
                label.Scale = 0.9f;
            }

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            GetLabel("Examine").Region = new Region(0, 0, 0, 0);

            //Hidden Panels
            int panel_width = Main.Game.MenuSize_X * 5;
            int upper_panel_height = Main.Game.MenuSize_Y * 12;
            int lower_panel_height = Main.Game.MenuSize_Y * 3;

            GetPicture("Panel_Upper_Left").Region = new Region(0, 0, panel_width, upper_panel_height);
            GetPicture("Panel_Lower_Left").Region = new Region(0, upper_panel_height, panel_width, lower_panel_height);
            GetPicture("Panel_Upper_Right").Region = new Region(Main.Game.ScreenWidth - panel_width, 0, panel_width, upper_panel_height);
            GetPicture("Panel_Lower_Right").Region = new Region(Main.Game.ScreenWidth - panel_width, upper_panel_height, panel_width, lower_panel_height);
            GetPicture("Panel_Upper_Center").Region = new Region(panel_width, 0, Main.Game.ScreenWidth - (panel_width * 2), upper_panel_height);

            //Message Panel
            int Y = Main.Game.ScreenHeight - lower_panel_height;
            GetPicture("Panel_Lower_Center").Region = new Region(panel_width, Y, Main.Game.ScreenWidth - (panel_width * 2), lower_panel_height);

            int message_height = lower_panel_height / Handler.MessageMax;
            for (int i = 0; i < Handler.MessageMax; i++)
            {
                Label message = GetLabel("Message" + i.ToString());
                message.Region = new Region(panel_width, Y, Main.Game.ScreenWidth - (panel_width * 2), message_height);
                message.Scale = (float)lower_panel_height / 212;
                Y += message_height;
            }

            //Upper Left
            int paperdoll_x = 0;
            int paperdoll_y = 0;
            int paperdoll_width = Main.Game.MenuSize_X * 5;
            int paperdoll_height = Main.Game.MenuSize_X * 10;
            GetPicture("Paperdoll_Head").Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            GetPicture("Paperdoll_Neck").Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            GetPicture("Paperdoll_Torso").Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            GetPicture("Paperdoll_Right_Arm").Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            GetPicture("Paperdoll_Right_Hand").Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            GetPicture("Paperdoll_Left_Arm").Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            GetPicture("Paperdoll_Left_Hand").Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            GetPicture("Paperdoll_Groin").Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            GetPicture("Paperdoll_Right_Leg").Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            GetPicture("Paperdoll_Right_Foot").Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            GetPicture("Paperdoll_Left_Leg").Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            GetPicture("Paperdoll_Left_Foot").Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);

            //Upper Center
            GetLabel("Time").Region = new Region(Main.Game.ScreenWidth - panel_width, 0, panel_width, Main.Game.MenuSize_Y * 2);

            //Upper Right
            int x = Main.Game.ScreenWidth - panel_width;
            int y = Main.Game.MenuSize_Y * 2;
            int height = (Main.Game.MenuSize_Y / 4) * 2;

            GetProgressBar("Hunger").Base_Region = new Region(x, y, panel_width, height);
            GetLabel("Hunger").Region = new Region(x, y, panel_width, height);

            GetProgressBar("Thirst").Base_Region = new Region(x, y + height, panel_width, height);
            GetLabel("Thirst").Region = new Region(x, y + height, panel_width, height);

            GetProgressBar("Bladder").Base_Region = new Region(x, y + (height * 2), panel_width, height);
            GetLabel("Bladder").Region = new Region(x, y + (height * 2), panel_width, height);

            GetProgressBar("Grime").Base_Region = new Region(x, y + (height * 3), panel_width, height);
            GetLabel("Grime").Region = new Region(x, y + (height * 3), panel_width, height);

            GetProgressBar("Pain").Base_Region = new Region(x, y + (height * 4), panel_width, height);
            GetLabel("Pain").Region = new Region(x, y + (height * 4), panel_width, height);

            GetProgressBar("Paranoia").Base_Region = new Region(x, y + (height * 5), panel_width, height);
            GetLabel("Paranoia").Region = new Region(x, y + (height * 5), panel_width, height);

            GetLabel("Crouching").Region = new Region(x, y + (height * 6), panel_width, height);
            GetLabel("Running").Region = new Region(x, y + (height * 7), panel_width, height);

            GetProgressBar("Blood").Base_Region = new Region(x, y + (height * 8), panel_width, height);
            GetLabel("Blood").Region = new Region(x, y + (height * 8), panel_width, height);

            GetProgressBar("Consciousness").Base_Region = new Region(x, y + (height * 9), panel_width, height);
            GetLabel("Consciousness").Region = new Region(x, y + (height * 9), panel_width, height);

            GetProgressBar("Stamina").Base_Region = new Region(x, y + (height * 10), panel_width, height);
            GetLabel("Stamina").Region = new Region(x, y + (height * 10), panel_width, height);

            GetProgressBar("Comfort").Base_Region = new Region(x, y + (height * 11), panel_width, height);
            GetLabel("Comfort").Region = new Region(x, y + (height * 11), panel_width, height);

            //Lower Right
            GetButton("Main").Region = new Region(Main.Game.ScreenWidth - Main.Game.MenuSize_X, Main.Game.ScreenHeight - Main.Game.MenuSize_Y, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
            GetButton("Inventory").Region = new Region(Main.Game.ScreenWidth - Main.Game.MenuSize_X, Main.Game.ScreenHeight - (Main.Game.MenuSize_Y * 2), Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
            GetButton("Health").Region = new Region(Main.Game.ScreenWidth - Main.Game.MenuSize_X, Main.Game.ScreenHeight - (Main.Game.MenuSize_Y * 3), Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
            GetButton("Stats").Region = new Region(Main.Game.ScreenWidth - Main.Game.MenuSize_X, Main.Game.ScreenHeight - (Main.Game.MenuSize_Y * 4), Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
            GetButton("Skills").Region = new Region(Main.Game.ScreenWidth - Main.Game.MenuSize_X, Main.Game.ScreenHeight - (Main.Game.MenuSize_Y * 5), Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
        }

        private void Examine(string text)
        {
            Label examine = GetLabel("Examine");
            examine.Text = text;

            int width = Main.Game.MenuSize_X * 7;
            int height = Main.Game.MenuSize_Y;

            int X = InputManager.Mouse.X - (width / 2);
            if (X < 0)
            {
                X = 0;
            }
            else if (X > Main.Game.Resolution.X - width)
            {
                X = Main.Game.Resolution.X - width;
            }

            int Y = InputManager.Mouse.Y + 20;
            if (Y < 0)
            {
                Y = 0;
            }
            else if (Y > Main.Game.Resolution.Y - height)
            {
                Y = Main.Game.Resolution.Y - height;
            }

            examine.Region = new Region(X, Y, width, height);
            examine.Visible = true;
        }

        #endregion
    }
}

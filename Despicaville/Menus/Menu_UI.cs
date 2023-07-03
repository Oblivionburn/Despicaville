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
        }

        private void CheckClick(Picture picture)
        {
            AssetManager.PlaySound_Random("Click");

            if (picture.Name.Contains("Paperdoll"))
            {

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
                string[] needs_stats = { "Hunger", "Thirst", "Bladder", "Grime", "Pain", "Paranoia" };
                foreach (string stat_name in needs_stats)
                {
                    Label label = GetLabel(stat_name);
                    if (label != null)
                    {
                        Something stat = player.GetStat(stat_name);
                        if (stat != null)
                        {
                            label.HoverText = stat.Description;

                            if (stat.Value <= 20)
                            {
                                label.TextColor = new Color(0, 200, 0);
                            }
                            else if (stat.Value <= 40)
                            {
                                label.TextColor = new Color(200, 200, 0);
                            }
                            else if (stat.Value <= 60)
                            {
                                label.TextColor = new Color(200, 100, 0);
                            }
                            else if (stat.Value <= 80)
                            {
                                label.TextColor = new Color(200, 0, 0);
                            }
                            else if (stat.Value <= 100)
                            {
                                label.TextColor = new Color(100, 0, 100);
                            }
                            label.Text = stat_name + ": " + stat.Value.ToString("0.##") + "/" + (int)stat.Max_Value + "%";
                        }
                    }
                }

                string[] stats = { "Blood", "Consciousness", "Stamina" };
                foreach (string stat_name in stats)
                {
                    Label label = GetLabel(stat_name);
                    if (label != null)
                    {
                        Something stat = player.GetStat(stat_name);
                        if (stat != null)
                        {
                            label.HoverText = stat.Description;

                            if (stat.Value >= 80)
                            {
                                label.TextColor = new Color(0, 200, 0);
                            }
                            else if (stat.Value >= 60)
                            {
                                label.TextColor = new Color(200, 200, 0);
                            }
                            else if (stat.Value >= 40)
                            {
                                label.TextColor = new Color(200, 100, 0);
                            }
                            else if (stat.Value >= 20)
                            {
                                label.TextColor = new Color(200, 0, 0);
                            }
                            else if (stat.Value >= 0)
                            {
                                label.TextColor = new Color(100, 0, 100);
                            }

                            label.Text = stat_name + ": " + stat.Value.ToString("0.##") + "/" + (int)stat.Max_Value + "%";
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

            AddPicture(Handler.GetID(), "Panel_Upper_Left", AssetManager.Textures["Frame_Large"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Panel_Upper_Right", AssetManager.Textures["Frame_Large"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Panel_Upper_Center", AssetManager.Textures["Frame_Wide"], new Region(0, 0, 0, 0), Color.White * 0f, true);
            AddPicture(Handler.GetID(), "Panel_Lower_Left", AssetManager.Textures["Frame_Large"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Panel_Lower_Right", AssetManager.Textures["Frame_Large"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Panel_Lower_Center", AssetManager.Textures["Frame_Wide"], new Region(0, 0, 0, 0), Color.White, true);

            AddPicture(Handler.GetID(), "Paperdoll_Right_Foot", AssetManager.Textures["Paperdoll_Right_Foot"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Paperdoll_Left_Foot", AssetManager.Textures["Paperdoll_Left_Foot"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Paperdoll_Right_Leg", AssetManager.Textures["Paperdoll_Right_Leg"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Paperdoll_Left_Leg", AssetManager.Textures["Paperdoll_Left_Leg"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Paperdoll_Right_Hand", AssetManager.Textures["Paperdoll_Right_Hand"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Paperdoll_Left_Hand", AssetManager.Textures["Paperdoll_Left_Hand"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Paperdoll_Right_Arm", AssetManager.Textures["Paperdoll_Right_Arm"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Paperdoll_Left_Arm", AssetManager.Textures["Paperdoll_Left_Arm"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Paperdoll_Groin", AssetManager.Textures["Paperdoll_Groin"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Paperdoll_Torso", AssetManager.Textures["Paperdoll_Torso"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Paperdoll_Neck", AssetManager.Textures["Paperdoll_Neck"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Paperdoll_Head", AssetManager.Textures["Paperdoll_Head"], new Region(0, 0, 0, 0), Color.White, true);
            
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Time", "", Color.White, AssetManager.Textures["Frame_Wide"], new Region(0, 0, 0, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Hunger", "Hunger: 0%", Color.White, new Region(0, 0, 0, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Thirst", "Thirst: 0%", Color.White, new Region(0, 0, 0, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Bladder", "Bladder: 0%", Color.White, new Region(0, 0, 0, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Grime", "Grime: 0%", Color.White, new Region(0, 0, 0, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Pain", "Pain: 0%", Color.White, new Region(0, 0, 0, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Paranoia", "Paranoia: 0%", Color.White, new Region(0, 0, 0, 0), true);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Crouching", "Quiet | Crouch", Color.White, new Region(0, 0, 0, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Running", "Loud | Run", Color.White, new Region(0, 0, 0, 0), true);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Blood", "Blood: 100%", Color.White, new Region(0, 0, 0, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Consciousness", "Consciousness: 100%", Color.White, new Region(0, 0, 0, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Stamina", "Stamina: 100%", Color.White, new Region(0, 0, 0, 0), true);

            AddButton(Handler.GetID(), "Main", AssetManager.Textures["Button_Menu"], AssetManager.Textures["Button_Menu_Hover"], null,
                new Region(0, 0, 0, 0), Color.White, true);
            GetButton("Main").HoverText = "System";

            AddButton(Handler.GetID(), "Inventory", AssetManager.Textures["Button_Inventory"], AssetManager.Textures["Button_Inventory_Hover"], null,
                new Region(0, 0, 0, 0), Color.White, true);
            GetButton("Inventory").HoverText = "Inventory";

            AddButton(Handler.GetID(), "Stats", AssetManager.Textures["Button_Stats"], AssetManager.Textures["Button_Stats_Hover"], null,
                new Region(0, 0, 0, 0), Color.White, true);
            GetButton("Stats").HoverText = "Stats";

            AddButton(Handler.GetID(), "Skills", AssetManager.Textures["Button_Skills"], AssetManager.Textures["Button_Skills_Hover"], null,
                new Region(0, 0, 0, 0), Color.White, true);
            GetButton("Skills").HoverText = "Skills";

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["Frame"], new Region(0, 0, 0, 0), false);

            for (int i = 0; i < Handler.MessageMax; i++)
            {
                AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Message" + i.ToString(), "", Color.Red, new Region(0, 0, 0, 0), true);

                Label label = GetLabel("Message" + i.ToString());
                label.Alignment_Horizontal = Alignment.Left;
                label.AutoScale = false;
                label.Scale = 1;
            }

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            GetLabel("Examine").Region = new Region(0, 0, 0, 0);

            //Panels
            int panel_width = Main.Game.MenuSize_X * 5;
            int upper_panel_height = Main.Game.MenuSize_X * 12;
            int lower_panel_height = Main.Game.ScreenHeight - upper_panel_height;

            GetPicture("Panel_Upper_Left").Region = new Region(0, 0, panel_width, upper_panel_height);
            GetPicture("Panel_Lower_Left").Region = new Region(0, upper_panel_height, panel_width, lower_panel_height);
            GetPicture("Panel_Upper_Right").Region = new Region(Main.Game.ScreenWidth - panel_width, 0, panel_width, upper_panel_height);
            GetPicture("Panel_Lower_Right").Region = new Region(Main.Game.ScreenWidth - panel_width, upper_panel_height, panel_width, lower_panel_height);
            GetPicture("Panel_Upper_Center").Region = new Region(panel_width, 0, Main.Game.ScreenWidth - (panel_width * 2), upper_panel_height);
            GetPicture("Panel_Lower_Center").Region = new Region(panel_width, upper_panel_height, Main.Game.ScreenWidth - (panel_width * 2), lower_panel_height);

            int Y = upper_panel_height;
            int message_height = lower_panel_height / Handler.MessageMax;
            for (int i = 0; i < Handler.MessageMax; i++)
            {
                GetLabel("Message" + i.ToString()).Region = new Region(panel_width, Y, Main.Game.ScreenWidth - (panel_width * 2), message_height);
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

            GetLabel("Blood").Region = new Region(0, paperdoll_height, panel_width, (Main.Game.MenuSize_Y / 2));
            GetLabel("Consciousness").Region = new Region(0, paperdoll_height + (Main.Game.MenuSize_Y / 2), panel_width, (Main.Game.MenuSize_Y / 2));
            GetLabel("Stamina").Region = new Region(0, paperdoll_height + Main.Game.MenuSize_Y, panel_width, (Main.Game.MenuSize_Y / 2));

            //Lower Left
            GetButton("Stats").Region = new Region(panel_width - (Main.Game.MenuSize_X * 2), upper_panel_height, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
            GetButton("Skills").Region = new Region(panel_width - Main.Game.MenuSize_X, upper_panel_height, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);

            int x = Main.Game.ScreenWidth - panel_width;

            //Upper Right
            GetLabel("Time").Region = new Region(x, 0, panel_width, Main.Game.MenuSize_Y * 2);

            int y = Main.Game.MenuSize_Y * 2;
            int height = (Main.Game.MenuSize_Y / 4) *3;
            GetLabel("Hunger").Region = new Region(x, y, panel_width, height);
            GetLabel("Thirst").Region = new Region(x, y + height, panel_width, height);
            GetLabel("Bladder").Region = new Region(x, y + (height * 2), panel_width, height);
            GetLabel("Grime").Region = new Region(x, y + (height * 3), panel_width, height);
            GetLabel("Pain").Region = new Region(x, y + (height * 4), panel_width, height);
            GetLabel("Paranoia").Region = new Region(x, y + (height * 5), panel_width, height);

            Region panel_upper_right = GetPicture("Panel_Upper_Right").Region;
            GetLabel("Crouching").Region = new Region(x, panel_upper_right.Height - (height * 2), panel_width, height);
            GetLabel("Running").Region = new Region(x, panel_upper_right.Height - height, panel_width, height);

            //Lower Right
            GetButton("Inventory").Region = new Region(x, upper_panel_height, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
            GetButton("Main").Region = new Region(x + Main.Game.MenuSize_X, upper_panel_height, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
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

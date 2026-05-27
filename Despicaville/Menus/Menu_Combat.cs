using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Menus;
using OP_Engine.Characters;
using OP_Engine.Utility;
using OP_Engine.Time;
using OP_Engine.Inventories;
using Despicaville.Util;
using Despicaville.JobTasks;

namespace Despicaville.Menus
{
    public class Menu_Combat : Menu
    {
        #region Variables

        

        #endregion

        #region Constructors

        public Menu_Combat(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Combat";
            Load(content);
        }

        #endregion

        #region Methods

        public override void Update(Game gameRef, ContentManager content)
        {
            if (Visible ||
                Active)
            {
                UpdateControls();

                base.Update(gameRef, content);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
                foreach (string body_part in Handler.BodyParts)
                {
                    BodyPart part = Handler.Interaction_Character.GetBodyPart(body_part);
                    if (part == null)
                    {
                        continue;
                    }

                    Property hp = part.GetStat("HP");
                    if (hp == null)
                    {
                        continue;
                    }

                    if (part.GetWounds("Sever").Count > 0)
                    {
                        GetPicture(body_part).Visible = false;
                        GetButton(body_part).Visible = false;
                        GetProgressBar(body_part).Visible = false;
                        GetLabel(body_part).Visible = false;
                    }
                    else
                    {
                        GetButton(body_part).Visible = hp.Value > 0;
                        GetProgressBar(body_part).Visible = hp.Value > 0;
                    }
                }

                foreach (Picture picture in Pictures)
                {
                    picture.Draw(spriteBatch);
                }

                foreach (Button button in Buttons)
                {
                    button.Draw(spriteBatch);
                }

                foreach (ProgressBar bar in ProgressBars)
                {
                    bar.Draw(spriteBatch);
                }

                foreach (Label label in Labels)
                {
                    if (label.Name != "Examine")
                    {
                        label.Draw(spriteBatch);
                    }
                }

                GetLabel("Examine")?.Draw(spriteBatch);
            }
        }

        private void UpdateControls()
        {
            bool hoveringButton = HoveringButton();

            if (!hoveringButton)
            {
                GetLabel("Examine").Visible = false;
            }
        }

        private bool HoveringButton()
        {
            foreach (string body_part in Handler.BodyParts)
            {
                Label label = GetLabel(body_part);
                label.Opacity = 0.8f;

                Button button = GetButton(body_part);
                button.Opacity = 0.8f;
                button.Selected = false;

                ProgressBar bar = GetProgressBar(body_part);
                bar.Opacity = 0.8f;
            }

            foreach (Button button in Buttons)
            {
                if (button.Visible &&
                    button.Enabled)
                {
                    if (InputManager.MouseWithin(button.Region.ToRectangle))
                    {
                        if (button.HoverText != null)
                        {
                            GameUtil.Examine(this, button.HoverText);
                        }

                        BodyPart part = Handler.Interaction_Character.GetBodyPart(button.Name);
                        if (part != null)
                        {
                            Property hp = part.GetStat("HP");
                            if (hp != null &&
                                hp.Value > 0)
                            {
                                button.Opacity = 1;
                                button.Selected = true;

                                Label label = GetLabel(button.Name);
                                label.Opacity = 1;

                                ProgressBar bar = GetProgressBar(button.Name);
                                bar.Opacity = 1;

                                if (InputManager.Mouse_LB_Pressed)
                                {
                                    CheckClick(button);

                                    button.Opacity = 0.8f;
                                    button.Selected = false;
                                }
                            }
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");
            InputManager.Mouse.Flush();
            Close();

            Dictionary<string, string> AttackingWith = CombatUtil.AttackChoice(Handler.Player);
            string weapon = AttackingWith.ElementAt(0).Key;
            string action = AttackingWith.ElementAt(0).Value;

            int attackTime = CombatUtil.AttackTime(Handler.Player, action);

            ProgressBar bar = GetProgressBar(button.Name);
            if (Utility.RandomPercent(bar.Value))
            {
                Handler.Selected_BodyPart = button.Name;

                Handler.Player.Job.Tasks.Add(new Attack
                {
                    Name = "Attack",
                    OwnerID = Handler.Player.ID,
                    Location = Handler.Interaction_Character.Location,
                    Direction = Handler.Player.Direction,
                    StartTime = new TimeHandler(TimeManager.Now),
                    EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(attackTime)),
                    TaskBar = CharacterUtil.GenTaskbar(Handler.Player, attackTime)
                });
            }
            else
            {
                //CombatUtil.AttackSound_Hit(Handler.Interaction_Character, null, weapon, action);

                Item weaponItem = null;
                for (int i = 0; i < Handler.Player.Inventory.Items.Count; i++)
                {
                    Item item = Handler.Player.Inventory.Items[i];
                    if (item.Name == weapon)
                    {
                        weaponItem = item;
                        break;
                    }
                }

                if (weaponItem != null)
                {
                    AssetManager.PlaySound_Random_AtDistance(weaponItem.Sound, Handler.Player.Location.ToVector2,
                        Handler.Interaction_Character.Location.ToVector2, weaponItem.SoundRange);
                }
                else
                {
                    AssetManager.PlaySound_Random_AtDistance("Punch", Handler.Player.Location.ToVector2, Handler.Interaction_Character.Location.ToVector2, 2);
                }

                GameUtil.AddMessage("You missed the shot at their " + CharacterUtil.BodyPartToName(button.Name).ToLower() + ".");

                Handler.Player.Job.Tasks.Add(new Wait
                {
                    Name = "Wait",
                    OwnerID = Handler.Player.ID,
                    StartTime = new TimeHandler(TimeManager.Now),
                    EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(attackTime))
                });

                TimeTracker.Tick(attackTime);
            }
        }

        public override void Close()
        {
            TimeManager.Paused = false;
            Active = false;
            Visible = false;
        }

        public override void Open()
        {
            TimeManager.Paused = true;
            Active = true;
            Visible = true;
            Load();
        }

        private void Update_BodyStats()
        {
            foreach (string body_part in Handler.BodyParts)
            {
                BodyPart part = Handler.Interaction_Character.GetBodyPart(body_part);
                if (part == null)
                {
                    continue;
                }

                Property hp = part.GetStat("HP");
                if (hp == null)
                {
                    continue;
                }

                Picture picture = GetPicture(body_part);
                if (picture != null)
                {
                    if (hp.Value >= 80)
                    {
                        picture.DrawColor = new Color(0, 200, 0);
                    }
                    else if (hp.Value >= 60)
                    {
                        picture.DrawColor = new Color(200, 200, 0);
                    }
                    else if (hp.Value >= 40)
                    {
                        picture.DrawColor = new Color(200, 100, 0);
                    }
                    else if (hp.Value >= 20)
                    {
                        picture.DrawColor = new Color(200, 0, 0);
                    }
                    else if (hp.Value > 0)
                    {
                        picture.DrawColor = new Color(100, 0, 100);
                    }
                    else if (hp.Value <= 0)
                    {
                        picture.DrawColor = new Color(12, 12, 12);
                    }
                }

                ProgressBar bar = GetProgressBar(body_part);
                if (bar != null)
                {
                    float hitChance = CombatUtil.ChanceToHitBodyPart(Handler.Player, Handler.Interaction_Character, body_part);
                    bar.Value = hitChance;

                    if (hitChance >= 80)
                    {
                        bar.DrawColor = new Color(0, 200, 0);
                    }
                    else if (hitChance >= 60)
                    {
                        bar.DrawColor = new Color(200, 200, 0);
                    }
                    else if (hitChance >= 40)
                    {
                        bar.DrawColor = new Color(200, 100, 0);
                    }
                    else if (hitChance >= 20)
                    {
                        bar.DrawColor = new Color(200, 0, 0);
                    }
                    else if (hitChance > 0)
                    {
                        bar.DrawColor = new Color(100, 0, 100);
                    }
                    else if (hitChance <= 0)
                    {
                        bar.DrawColor = new Color(12, 12, 12);
                    }
                }
            }
        }

        public override void Load()
        {
            if (Handler.Interaction_Character != null)
            {
                GetLabel("Name_Frame").Text = Handler.Interaction_Character.Name;
                Update_BodyStats();
            }
        }

        public override void Load(ContentManager content)
        {
            Clear();

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Name_Frame", "", Color.White, AssetManager.Textures["Frame"], new Region(0, 0, 0, 0), true);
            AddPicture(Handler.GetID(), "Body_Frame", AssetManager.Textures["Frame_Large"], new Region(0, 0, 0, 0), Color.White, false);

            AddPicture(Handler.GetID(), "Head", AssetManager.Textures["Paperdoll_Head"], new Region(0, 0, 0, 0), Color.White, true);
            AddButton(Handler.GetID(), "Head", AssetManager.Textures["Frame_Small"], AssetManager.Textures["Frame_Small"], null,
                new Region(0, 0, 0, 0), Color.White, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Head", "Head", Color.White, new Region(0, 0, 0, 0), true);
            AddProgressBar(Handler.GetID(), "Head", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                new Region(0, 0, 0, 0), Color.LimeGreen, true);

            AddPicture(Handler.GetID(), "Neck", AssetManager.Textures["Paperdoll_Neck"], new Region(0, 0, 0, 0), Color.White, true);
            AddButton(Handler.GetID(), "Neck", AssetManager.Textures["Frame_Small"], AssetManager.Textures["Frame_Small"], null,
                new Region(0, 0, 0, 0), Color.White, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Neck", "Neck", Color.White, new Region(0, 0, 0, 0), true);
            AddProgressBar(Handler.GetID(), "Neck", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                new Region(0, 0, 0, 0), Color.LimeGreen, true);

            AddPicture(Handler.GetID(), "Torso", AssetManager.Textures["Paperdoll_Torso"], new Region(0, 0, 0, 0), Color.White, true);
            AddButton(Handler.GetID(), "Torso", AssetManager.Textures["Frame_Small"], AssetManager.Textures["Frame_Small"], null,
                new Region(0, 0, 0, 0), Color.White, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Torso", "Torso", Color.White, new Region(0, 0, 0, 0), true);
            AddProgressBar(Handler.GetID(), "Torso", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                new Region(0, 0, 0, 0), Color.LimeGreen, true);

            AddPicture(Handler.GetID(), "Left_Arm", AssetManager.Textures["Paperdoll_Left_Arm"], new Region(0, 0, 0, 0), Color.White, true);
            AddButton(Handler.GetID(), "Left_Arm", AssetManager.Textures["Frame_Small"], AssetManager.Textures["Frame_Small"], null,
                new Region(0, 0, 0, 0), Color.White, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Left_Arm", "Left Arm", Color.White, new Region(0, 0, 0, 0), true);
            AddProgressBar(Handler.GetID(), "Left_Arm", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                new Region(0, 0, 0, 0), Color.LimeGreen, true);

            AddPicture(Handler.GetID(), "Right_Arm", AssetManager.Textures["Paperdoll_Right_Arm"], new Region(0, 0, 0, 0), Color.White, true);
            AddButton(Handler.GetID(), "Right_Arm", AssetManager.Textures["Frame_Small"], AssetManager.Textures["Frame_Small"], null,
                new Region(0, 0, 0, 0), Color.White, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Right_Arm", "Right Arm", Color.White, new Region(0, 0, 0, 0), true);
            AddProgressBar(Handler.GetID(), "Right_Arm", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                new Region(0, 0, 0, 0), Color.LimeGreen, true);

            AddPicture(Handler.GetID(), "Left_Hand", AssetManager.Textures["Paperdoll_Left_Hand"], new Region(0, 0, 0, 0), Color.White, true);
            AddButton(Handler.GetID(), "Left_Hand", AssetManager.Textures["Frame_Small"], AssetManager.Textures["Frame_Small"], null,
                new Region(0, 0, 0, 0), Color.White, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Left_Hand", "Left Hand", Color.White, new Region(0, 0, 0, 0), true);
            AddProgressBar(Handler.GetID(), "Left_Hand", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                new Region(0, 0, 0, 0), Color.LimeGreen, true);

            AddPicture(Handler.GetID(), "Right_Hand", AssetManager.Textures["Paperdoll_Right_Hand"], new Region(0, 0, 0, 0), Color.White, true);
            AddButton(Handler.GetID(), "Right_Hand", AssetManager.Textures["Frame_Small"], AssetManager.Textures["Frame_Small"], null,
                new Region(0, 0, 0, 0), Color.White, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Right_Hand", "Right Hand", Color.White, new Region(0, 0, 0, 0), true);
            AddProgressBar(Handler.GetID(), "Right_Hand", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                new Region(0, 0, 0, 0), Color.LimeGreen, true);

            AddPicture(Handler.GetID(), "Left_Leg", AssetManager.Textures["Paperdoll_Left_Leg"], new Region(0, 0, 0, 0), Color.White, true);
            AddButton(Handler.GetID(), "Left_Leg", AssetManager.Textures["Frame_Small"], AssetManager.Textures["Frame_Small"], null,
                new Region(0, 0, 0, 0), Color.White, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Left_Leg", "Left Leg", Color.White, new Region(0, 0, 0, 0), true);
            AddProgressBar(Handler.GetID(), "Left_Leg", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                new Region(0, 0, 0, 0), Color.LimeGreen, true);

            AddPicture(Handler.GetID(), "Right_Leg", AssetManager.Textures["Paperdoll_Right_Leg"], new Region(0, 0, 0, 0), Color.White, true);
            AddButton(Handler.GetID(), "Right_Leg", AssetManager.Textures["Frame_Small"], AssetManager.Textures["Frame_Small"], null,
                new Region(0, 0, 0, 0), Color.White, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Right_Leg", "Right Leg", Color.White, new Region(0, 0, 0, 0), true);
            AddProgressBar(Handler.GetID(), "Right_Leg", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                new Region(0, 0, 0, 0), Color.LimeGreen, true);

            AddPicture(Handler.GetID(), "Groin", AssetManager.Textures["Paperdoll_Groin"], new Region(0, 0, 0, 0), Color.White, true);
            AddButton(Handler.GetID(), "Groin", AssetManager.Textures["Frame_Small"], AssetManager.Textures["Frame_Small"], null,
                new Region(0, 0, 0, 0), Color.White, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Groin", "Groin", Color.White, new Region(0, 0, 0, 0), true);
            AddProgressBar(Handler.GetID(), "Groin", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                new Region(0, 0, 0, 0), Color.LimeGreen, true);

            AddPicture(Handler.GetID(), "Left_Foot", AssetManager.Textures["Paperdoll_Left_Foot"], new Region(0, 0, 0, 0), Color.White, true);
            AddButton(Handler.GetID(), "Left_Foot", AssetManager.Textures["Frame_Small"], AssetManager.Textures["Frame_Small"], null,
                new Region(0, 0, 0, 0), Color.White, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Left_Foot", "Left Foot", Color.White, new Region(0, 0, 0, 0), true);
            AddProgressBar(Handler.GetID(), "Left_Foot", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                new Region(0, 0, 0, 0), Color.LimeGreen, true);

            AddPicture(Handler.GetID(), "Right_Foot", AssetManager.Textures["Paperdoll_Right_Foot"], new Region(0, 0, 0, 0), Color.White, true);
            AddButton(Handler.GetID(), "Right_Foot", AssetManager.Textures["Frame_Small"], AssetManager.Textures["Frame_Small"], null,
                new Region(0, 0, 0, 0), Color.White, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Right_Foot", "Right Foot", Color.White, new Region(0, 0, 0, 0), true);
            AddProgressBar(Handler.GetID(), "Right_Foot", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                new Region(0, 0, 0, 0), Color.LimeGreen, true);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["Frame"], new Region(0, 0, 0, 0), false);

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            GetLabel("Examine").Region = new Region(0, 0, 0, 0);

            float x = (Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 4) - 5;
            float y = Main.Game.MenuSize_Y;
            float width = Main.Game.MenuSize_X * 8;
            float height = Main.Game.MenuSize_Y * 14;

            Picture frame = GetPicture("Body_Frame");
            frame.Region = new Region(x, y, width, height);

            GetLabel("Name_Frame").Region = new Region(x, y - Main.Game.MenuSize_Y, width, Main.Game.MenuSize_Y);

            float label_x = (Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X;
            float label_y = y + Main.Game.MenuSize_Y + (Main.Game.MenuSize_Y / 4);
            float label_width = Main.Game.MenuSize_X * 2;
            float label_height = Main.Game.MenuSize_Y / 2;
            float prog_height = label_height / 2;
            float button_height = label_height + prog_height;

            float Y = label_y;
            float X = label_x;
            GetPicture("Head").Region = new Region(x, y, width, height);
            GetButton("Head").Region = new Region(X, Y, label_width, button_height);
            GetLabel("Head").Region = new Region(X, Y, label_width, label_height);
            GetProgressBar("Head").Base_Region = new Region(X, label_y + label_height, label_width, prog_height);

            Y = label_y + Main.Game.MenuSize_Y;
            GetPicture("Neck").Region = new Region(x, y, width, height);
            GetButton("Neck").Region = new Region(X, Y, label_width, button_height);
            GetLabel("Neck").Region = new Region(X, Y, label_width, label_height);
            GetProgressBar("Neck").Base_Region = new Region(X, Y + label_height, label_width, prog_height);

            Y = label_y + (Main.Game.MenuSize_Y * 2) + (Main.Game.MenuSize_Y / 2);
            GetPicture("Torso").Region = new Region(x, y, width, height);
            GetButton("Torso").Region = new Region(X, Y, label_width, button_height);
            GetLabel("Torso").Region = new Region(X, Y, label_width, label_height);
            GetProgressBar("Torso").Base_Region = new Region(X, Y + label_height, label_width, prog_height);

            Y = label_y + (Main.Game.MenuSize_Y * 3) + (Main.Game.MenuSize_Y / 2);
            X = label_x - (Main.Game.MenuSize_X * 2) - (Main.Game.MenuSize_X / 4);
            GetPicture("Right_Arm").Region = new Region(x, y, width, height);
            GetButton("Right_Arm").Region = new Region(X, Y, label_width, button_height);
            GetLabel("Right_Arm").Region = new Region(X, Y, label_width, label_height);
            GetProgressBar("Right_Arm").Base_Region = new Region(X, Y + label_height, label_width, prog_height);

            X = label_x + label_width + Main.Game.MenuSize_X - (Main.Game.MenuSize_X / 2) - (Main.Game.MenuSize_X / 4);
            GetPicture("Left_Arm").Region = new Region(x, y, width, height);
            GetButton("Left_Arm").Region = new Region(X, Y, label_width, button_height);
            GetLabel("Left_Arm").Region = new Region(X, Y, label_width, label_height);
            GetProgressBar("Left_Arm").Base_Region = new Region(X, Y + label_height, label_width, prog_height);

            Y = label_y + (Main.Game.MenuSize_Y * 5) + (Main.Game.MenuSize_Y / 2);
            X = label_x - (Main.Game.MenuSize_X * 2) - Main.Game.MenuSize_X;
            GetPicture("Right_Hand").Region = new Region(x, y, width, height);
            GetButton("Right_Hand").Region = new Region(X, Y, label_width, button_height);
            GetLabel("Right_Hand").Region = new Region(X, Y, label_width, label_height);
            GetProgressBar("Right_Hand").Base_Region = new Region(X, Y + label_height, label_width, prog_height);

            X = label_x + label_width + Main.Game.MenuSize_X + (Main.Game.MenuSize_X / 2) - (Main.Game.MenuSize_X / 4);
            GetPicture("Left_Hand").Region = new Region(x, y, width, height);
            GetButton("Left_Hand").Region = new Region(X, Y, label_width, button_height);
            GetLabel("Left_Hand").Region = new Region(X, Y, label_width, label_height);
            GetProgressBar("Left_Hand").Base_Region = new Region(X, Y + label_height, label_width, prog_height);

            Y = label_y + (Main.Game.MenuSize_Y * 5) + (Main.Game.MenuSize_Y / 2);
            X = label_x;
            GetPicture("Groin").Region = new Region(x, y, width, height);
            GetButton("Groin").Region = new Region(X, Y, label_width, button_height);
            GetLabel("Groin").Region = new Region(X, Y, label_width, label_height);
            GetProgressBar("Groin").Base_Region = new Region(X, Y + label_height, label_width, prog_height);

            Y = label_y + (Main.Game.MenuSize_Y * 8);
            X = label_x - Main.Game.MenuSize_X - (Main.Game.MenuSize_X / 4);
            GetPicture("Right_Leg").Region = new Region(x, y, width, height);
            GetButton("Right_Leg").Region = new Region(X, Y, label_width, button_height);
            GetLabel("Right_Leg").Region = new Region(X, Y, label_width, label_height);
            GetProgressBar("Right_Leg").Base_Region = new Region(X, Y + label_height, label_width, prog_height);

            X = label_x + label_width - (Main.Game.MenuSize_X / 2);
            GetPicture("Left_Leg").Region = new Region(x, y, width, height);
            GetButton("Left_Leg").Region = new Region(X, Y, label_width, button_height);
            GetLabel("Left_Leg").Region = new Region(X, Y, label_width, label_height);
            GetProgressBar("Left_Leg").Base_Region = new Region(X, Y + label_height, label_width, prog_height);

            Y = label_y + (Main.Game.MenuSize_Y * 11) + (Main.Game.MenuSize_Y / 2);
            X = label_x - Main.Game.MenuSize_X - (Main.Game.MenuSize_X / 4);
            GetPicture("Right_Foot").Region = new Region(x, y, width, height);
            GetButton("Right_Foot").Region = new Region(X, Y, label_width, button_height);
            GetLabel("Right_Foot").Region = new Region(X, Y, label_width, label_height);
            GetProgressBar("Right_Foot").Base_Region = new Region(X, Y + label_height, label_width, prog_height);

            X = label_x + label_width - (Main.Game.MenuSize_X / 2);
            GetPicture("Left_Foot").Region = new Region(x, y, width, height);
            GetButton("Left_Foot").Region = new Region(X, Y, label_width, button_height);
            GetLabel("Left_Foot").Region = new Region(X, Y, label_width, label_height);
            GetProgressBar("Left_Foot").Base_Region = new Region(X, Y + label_height, label_width, prog_height);
        }

        #endregion
    }
}

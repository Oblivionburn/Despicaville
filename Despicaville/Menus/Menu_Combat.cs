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

        public override void Update(Game? gameRef, ContentManager? content)
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
                    BodyPart? part = Handler.Interaction_Character?.GetBodyPart(body_part);
                    if (part == null)
                    {
                        continue;
                    }

                    Property? hp = part.GetStat("HP");
                    if (hp == null)
                    {
                        continue;
                    }

                    Button? button = GetButton(body_part);
                    ProgressBar? progressBar = GetProgressBar(body_part);

                    if (part.GetWounds("Sever").Count > 0)
                    {
                        Picture? picture = GetPicture(body_part);
                        if (picture != null)
                        {
                            picture.Visible = false;
                        }

                        if (button != null)
                        {
                            button.Visible = false;
                        }

                        if (progressBar != null)
                        {
                            progressBar.Visible = false;
                        }

                        Label? label = GetLabel(body_part);
                        if (label != null)
                        {
                            label.Visible = false;
                        }
                    }
                    else
                    {
                        if (button != null)
                        {
                            button.Visible = hp.Value > 0;
                        }

                        if (progressBar != null)
                        {
                            progressBar.Visible = hp.Value > 0;
                        }
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
                Label? examine = GetLabel("Examine");
                if (examine != null)
                {
                    examine.Visible = false;
                }
            }
        }

        private bool HoveringButton()
        {
            foreach (string body_part in Handler.BodyParts)
            {
                Label? label = GetLabel(body_part);
                if (label != null)
                {
                    label.Opacity = 0.8f;
                }

                Button? button = GetButton(body_part);
                if (button != null)
                {
                    button.Opacity = 0.8f;
                    button.Selected = false;
                }

                ProgressBar? progressBar = GetProgressBar(body_part);
                if (progressBar != null)
                {
                    progressBar.Opacity = 0.8f;
                }
            }

            foreach (Button button in Buttons)
            {
                if (button.Visible &&
                    button.Enabled &&
                    button.Region != null)
                {
                    if (InputManager.MouseWithin(button.Region.ToRectangle) &&
                        button.Name != null)
                    {
                        if (button.HoverText != null)
                        {
                            GameUtil.Examine(this, button.HoverText);
                        }

                        BodyPart? part = Handler.Interaction_Character?.GetBodyPart(button.Name);
                        if (part != null)
                        {
                            Property? hp = part.GetStat("HP");
                            if (hp != null &&
                                hp.Value > 0)
                            {
                                button.Opacity = 1;
                                button.Selected = true;

                                Label? label = GetLabel(button.Name);
                                if (label != null)
                                {
                                    label.Opacity = 1;
                                }

                                ProgressBar? progressBar = GetProgressBar(button.Name);
                                if (progressBar != null)
                                {
                                    progressBar.Opacity = 1;
                                }

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
            if (Handler.Player == null ||
                Handler.Interaction_Character?.Location == null ||
                TimeManager.Now == null ||
                button.Name == null)
            {
                return;
            }

            AssetManager.PlaySound_Random("Click");
            InputManager.Mouse?.Flush();
            Close();

            Dictionary<string, string> AttackingWith = CombatUtil.AttackChoice(Handler.Player);
            string weapon = AttackingWith.ElementAt(0).Key;
            string action = AttackingWith.ElementAt(0).Value;

            int attackTime = CombatUtil.AttackTime(Handler.Player, action);

            ProgressBar? bar = GetProgressBar(button.Name);
            if (bar != null &&
                Utility.RandomPercent(bar.Value))
            {
                Handler.Selected_BodyPart = button.Name;

                Handler.Player.Job.Tasks.Add(new Attack
                {
                    Name = "Attack",
                    Owner_Character = Handler.Player,
                    Location = Handler.Interaction_Character.Location,
                    Direction = Handler.Player.Direction,
                    StartTime = new TimeHandler(TimeManager.Now),
                    EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(attackTime)),
                    TaskBar = CharacterUtil.GenTaskbar(Handler.Player, attackTime)
                });
            }
            else
            {
                Item? weaponItem = null;
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
                    if (weaponItem.Sound != null &&
                        Handler.Player.Location != null)
                    {
                        AssetManager.PlaySound_Random_AtDistance(weaponItem.Sound, Handler.Player.Location.ToVector2,
                        Handler.Interaction_Character.Location.ToVector2, weaponItem.SoundRange);
                    }
                }
                else if (Handler.Player.Location != null)
                {
                    AssetManager.PlaySound_Random_AtDistance("Punch", Handler.Player.Location.ToVector2, Handler.Interaction_Character.Location.ToVector2, 2);
                }

                GameUtil.AddMessage("You missed the shot at their " + CharacterUtil.BodyPartToName(button.Name)?.ToLower() + ".");

                Handler.Player.Job.Tasks.Add(new Wait
                {
                    Name = "Wait",
                    Owner_Character = Handler.Player,
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
            if (Handler.Player == null ||
                Handler.Interaction_Character == null)
            {
                return;
            }

            foreach (string body_part in Handler.BodyParts)
            {
                BodyPart? part = Handler.Interaction_Character.GetBodyPart(body_part);
                if (part == null)
                {
                    continue;
                }

                Property? hp = part.GetStat("HP");
                if (hp == null)
                {
                    continue;
                }

                Picture? picture = GetPicture(body_part);
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

                ProgressBar? bar = GetProgressBar(body_part);
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
                Label? name_Frame = GetLabel("Name_Frame");
                if (name_Frame != null)
                {
                    name_Frame.Text = Handler.Interaction_Character.Name;
                }
                
                Update_BodyStats();
            }
        }

        public override void Load(ContentManager content)
        {
            Clear();

            Texture2D? frame = Handler.GetTexture("Frame");
            Texture2D? frame_Small = Handler.GetTexture("Frame_Small");
            Texture2D? frame_Large = Handler.GetTexture("Frame_Large");

            Texture2D? progressBase = Handler.GetTexture("ProgressBase");
            Texture2D? progressBar = Handler.GetTexture("ProgressBar");

            Texture2D? paperdoll_Head = Handler.GetTexture("Paperdoll_Head");
            Texture2D? paperdoll_Neck = Handler.GetTexture("Paperdoll_Neck");
            Texture2D? paperdoll_Torso = Handler.GetTexture("Paperdoll_Torso");
            Texture2D? paperdoll_Left_Arm = Handler.GetTexture("Paperdoll_Left_Arm");
            Texture2D? paperdoll_Right_Arm = Handler.GetTexture("Paperdoll_Right_Arm");
            Texture2D? paperdoll_Left_Hand = Handler.GetTexture("Paperdoll_Left_Hand");
            Texture2D? paperdoll_Right_Hand = Handler.GetTexture("Paperdoll_Right_Hand");
            Texture2D? paperdoll_Left_Leg = Handler.GetTexture("Paperdoll_Left_Leg");
            Texture2D? paperdoll_Right_Leg = Handler.GetTexture("Paperdoll_Right_Leg");
            Texture2D? paperdoll_Groin = Handler.GetTexture("Paperdoll_Groin");
            Texture2D? paperdoll_Left_Foot = Handler.GetTexture("Paperdoll_Left_Foot");
            Texture2D? paperdoll_Right_Foot = Handler.GetTexture("Paperdoll_Right_Foot");

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Name_Frame", "", Color.White, frame, new Region(0, 0, 0, 0), true);
            AddPicture(Handler.GetID(), "Body_Frame", frame_Large, new Region(0, 0, 0, 0), Color.White, false);

            AddPicture(Handler.GetID(), "Head", paperdoll_Head, new Region(0, 0, 0, 0), Color.White, true);
            AddButton(Handler.GetID(), "Head", frame_Small, frame_Small, null,
                new Region(0, 0, 0, 0), Color.White, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Head", "Head", Color.White, new Region(0, 0, 0, 0), true);
            AddProgressBar(Handler.GetID(), "Head", 100, 100, 1, progressBase, progressBar,
                new Region(0, 0, 0, 0), Color.LimeGreen, true);

            AddPicture(Handler.GetID(), "Neck", paperdoll_Neck, new Region(0, 0, 0, 0), Color.White, true);
            AddButton(Handler.GetID(), "Neck", frame_Small, frame_Small, null,
                new Region(0, 0, 0, 0), Color.White, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Neck", "Neck", Color.White, new Region(0, 0, 0, 0), true);
            AddProgressBar(Handler.GetID(), "Neck", 100, 100, 1, progressBase, progressBar,
                new Region(0, 0, 0, 0), Color.LimeGreen, true);

            AddPicture(Handler.GetID(), "Torso", paperdoll_Torso, new Region(0, 0, 0, 0), Color.White, true);
            AddButton(Handler.GetID(), "Torso", frame_Small, frame_Small, null,
                new Region(0, 0, 0, 0), Color.White, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Torso", "Torso", Color.White, new Region(0, 0, 0, 0), true);
            AddProgressBar(Handler.GetID(), "Torso", 100, 100, 1, progressBase, progressBar,
                new Region(0, 0, 0, 0), Color.LimeGreen, true);

            AddPicture(Handler.GetID(), "Left_Arm", paperdoll_Left_Arm, new Region(0, 0, 0, 0), Color.White, true);
            AddButton(Handler.GetID(), "Left_Arm", frame_Small, frame_Small, null,
                new Region(0, 0, 0, 0), Color.White, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Left_Arm", "Left Arm", Color.White, new Region(0, 0, 0, 0), true);
            AddProgressBar(Handler.GetID(), "Left_Arm", 100, 100, 1, progressBase, progressBar,
                new Region(0, 0, 0, 0), Color.LimeGreen, true);

            AddPicture(Handler.GetID(), "Right_Arm", paperdoll_Right_Arm, new Region(0, 0, 0, 0), Color.White, true);
            AddButton(Handler.GetID(), "Right_Arm", frame_Small, frame_Small, null,
                new Region(0, 0, 0, 0), Color.White, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Right_Arm", "Right Arm", Color.White, new Region(0, 0, 0, 0), true);
            AddProgressBar(Handler.GetID(), "Right_Arm", 100, 100, 1, progressBase, progressBar,
                new Region(0, 0, 0, 0), Color.LimeGreen, true);

            AddPicture(Handler.GetID(), "Left_Hand", paperdoll_Left_Hand, new Region(0, 0, 0, 0), Color.White, true);
            AddButton(Handler.GetID(), "Left_Hand", frame_Small, frame_Small, null,
                new Region(0, 0, 0, 0), Color.White, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Left_Hand", "Left Hand", Color.White, new Region(0, 0, 0, 0), true);
            AddProgressBar(Handler.GetID(), "Left_Hand", 100, 100, 1, progressBase, progressBar,
                new Region(0, 0, 0, 0), Color.LimeGreen, true);

            AddPicture(Handler.GetID(), "Right_Hand", paperdoll_Right_Hand, new Region(0, 0, 0, 0), Color.White, true);
            AddButton(Handler.GetID(), "Right_Hand", frame_Small, frame_Small, null,
                new Region(0, 0, 0, 0), Color.White, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Right_Hand", "Right Hand", Color.White, new Region(0, 0, 0, 0), true);
            AddProgressBar(Handler.GetID(), "Right_Hand", 100, 100, 1, progressBase, progressBar,
                new Region(0, 0, 0, 0), Color.LimeGreen, true);

            AddPicture(Handler.GetID(), "Left_Leg", paperdoll_Left_Leg, new Region(0, 0, 0, 0), Color.White, true);
            AddButton(Handler.GetID(), "Left_Leg", frame_Small, frame_Small, null,
                new Region(0, 0, 0, 0), Color.White, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Left_Leg", "Left Leg", Color.White, new Region(0, 0, 0, 0), true);
            AddProgressBar(Handler.GetID(), "Left_Leg", 100, 100, 1, progressBase, progressBar,
                new Region(0, 0, 0, 0), Color.LimeGreen, true);

            AddPicture(Handler.GetID(), "Right_Leg", paperdoll_Right_Leg, new Region(0, 0, 0, 0), Color.White, true);
            AddButton(Handler.GetID(), "Right_Leg", frame_Small, frame_Small, null,
                new Region(0, 0, 0, 0), Color.White, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Right_Leg", "Right Leg", Color.White, new Region(0, 0, 0, 0), true);
            AddProgressBar(Handler.GetID(), "Right_Leg", 100, 100, 1, progressBase, progressBar,
                new Region(0, 0, 0, 0), Color.LimeGreen, true);

            AddPicture(Handler.GetID(), "Groin", paperdoll_Groin, new Region(0, 0, 0, 0), Color.White, true);
            AddButton(Handler.GetID(), "Groin", frame_Small, frame_Small, null,
                new Region(0, 0, 0, 0), Color.White, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Groin", "Groin", Color.White, new Region(0, 0, 0, 0), true);
            AddProgressBar(Handler.GetID(), "Groin", 100, 100, 1, progressBase, progressBar,
                new Region(0, 0, 0, 0), Color.LimeGreen, true);

            AddPicture(Handler.GetID(), "Left_Foot", paperdoll_Left_Foot, new Region(0, 0, 0, 0), Color.White, true);
            AddButton(Handler.GetID(), "Left_Foot", frame_Small, frame_Small, null,
                new Region(0, 0, 0, 0), Color.White, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Left_Foot", "Left Foot", Color.White, new Region(0, 0, 0, 0), true);
            AddProgressBar(Handler.GetID(), "Left_Foot", 100, 100, 1, progressBase, progressBar,
                new Region(0, 0, 0, 0), Color.LimeGreen, true);

            AddPicture(Handler.GetID(), "Right_Foot", paperdoll_Right_Foot, new Region(0, 0, 0, 0), Color.White, true);
            AddButton(Handler.GetID(), "Right_Foot", frame_Small, frame_Small, null,
                new Region(0, 0, 0, 0), Color.White, true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Right_Foot", "Right Foot", Color.White, new Region(0, 0, 0, 0), true);
            AddProgressBar(Handler.GetID(), "Right_Foot", 100, 100, 1, progressBase, progressBar,
                new Region(0, 0, 0, 0), Color.LimeGreen, true);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, frame, new Region(0, 0, 0, 0), false);

            if (Main.Game != null)
            {
                Resize(Main.Game.Resolution);
            }
        }

        public override void Resize(Point point)
        {
            if (Main.Game == null)
            {
                return;
            }

            Label? examine = GetLabel("Examine");
            if (examine != null)
            {
                examine.Region = new Region(0, 0, 0, 0);
            }

            float x = (Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize_X * 4) - 5;
            float y = Main.Game.MenuSize_Y;
            float width = Main.Game.MenuSize_X * 8;
            float height = Main.Game.MenuSize_Y * 14;

            Picture? body_Frame = GetPicture("Body_Frame");
            if (body_Frame != null)
            {
                body_Frame.Region = new Region(x, y, width, height);
            }

            Label? name_Frame = GetLabel("Name_Frame");
            if (name_Frame != null)
            {
                name_Frame.Region = new Region(x, y - Main.Game.MenuSize_Y, width, Main.Game.MenuSize_Y);
            }

            float label_x = (Main.Game.ScreenWidth / 2) - Main.Game.MenuSize_X;
            float label_y = y + Main.Game.MenuSize_Y + (Main.Game.MenuSize_Y / 4);
            float label_width = Main.Game.MenuSize_X * 2;
            float label_height = Main.Game.MenuSize_Y / 2;
            float prog_height = label_height / 2;
            float button_height = label_height + prog_height;

            float Y = label_y;
            float X = label_x;

            Picture? head_picture = GetPicture("Head");
            if (head_picture != null)
            {
                head_picture.Region = new Region(x, y, width, height);
            }

            Button? head_button = GetButton("Head");
            if (head_button != null)
            {
                head_button.Region = new Region(X, Y, label_width, button_height);
            }

            Label? head_label = GetLabel("Head");
            if (head_label != null)
            {
                head_label.Region = new Region(X, Y, label_width, label_height);
            }

            ProgressBar? head_bar = GetProgressBar("Head");
            if (head_bar != null)
            {
                head_bar.Base_Region = new Region(X, label_y + label_height, label_width, prog_height);
            }

            Y = label_y + Main.Game.MenuSize_Y;

            Picture? neck_picture = GetPicture("Neck");
            if (neck_picture != null)
            {
                neck_picture.Region = new Region(x, y, width, height);
            }

            Button? neck_button = GetButton("Neck");
            if (neck_button != null)
            {
                neck_button.Region = new Region(X, Y, label_width, button_height);
            }

            Label? neck_label = GetLabel("Neck");
            if (neck_label != null)
            {
                neck_label.Region = new Region(X, Y, label_width, label_height);
            }

            ProgressBar? neck_bar = GetProgressBar("Neck");
            if (neck_bar != null)
            {
                neck_bar.Base_Region = new Region(X, Y + label_height, label_width, prog_height);
            }

            Y = label_y + (Main.Game.MenuSize_Y * 2) + (Main.Game.MenuSize_Y / 2);

            Picture? torso_picture = GetPicture("Torso");
            if (torso_picture != null)
            {
                torso_picture.Region = new Region(x, y, width, height);
            }

            Button? torso_button = GetButton("Torso");
            if (torso_button != null)
            {
                torso_button.Region = new Region(X, Y, label_width, button_height);
            }

            Label? torso_label = GetLabel("Torso");
            if (torso_label != null)
            {
                torso_label.Region = new Region(X, Y, label_width, label_height);
            }

            ProgressBar? torso_bar = GetProgressBar("Torso");
            if (torso_bar != null)
            {
                torso_bar.Base_Region = new Region(X, Y + label_height, label_width, prog_height);
            }

            Y = label_y + (Main.Game.MenuSize_Y * 3) + (Main.Game.MenuSize_Y / 2);
            X = label_x - (Main.Game.MenuSize_X * 2) - (Main.Game.MenuSize_X / 4);

            Picture? right_arm_picture = GetPicture("Right_Arm");
            if (right_arm_picture != null)
            {
                right_arm_picture.Region = new Region(x, y, width, height);
            }

            Button? right_arm_button = GetButton("Right_Arm");
            if (right_arm_button != null)
            {
                right_arm_button.Region = new Region(X, Y, label_width, button_height);
            }

            Label? right_arm_label = GetLabel("Right_Arm");
            if (right_arm_label != null)
            {
                right_arm_label.Region = new Region(X, Y, label_width, label_height);
            }

            ProgressBar? right_arm_bar = GetProgressBar("Right_Arm");
            if (right_arm_bar != null)
            {
                right_arm_bar.Base_Region = new Region(X, Y + label_height, label_width, prog_height);
            }

            X = label_x + label_width + Main.Game.MenuSize_X - (Main.Game.MenuSize_X / 2) - (Main.Game.MenuSize_X / 4);

            Picture? left_arm_picture = GetPicture("Left_Arm");
            if (left_arm_picture != null)
            {
                left_arm_picture.Region = new Region(x, y, width, height);
            }

            Button? left_arm_button = GetButton("Left_Arm");
            if (left_arm_button != null)
            {
                left_arm_button.Region = new Region(X, Y, label_width, button_height);
            }

            Label? left_arm_label = GetLabel("Left_Arm");
            if (left_arm_label != null)
            {
                left_arm_label.Region = new Region(X, Y, label_width, label_height);
            }

            ProgressBar? left_arm_bar = GetProgressBar("Left_Arm");
            if (left_arm_bar != null)
            {
                left_arm_bar.Base_Region = new Region(X, Y + label_height, label_width, prog_height);
            }

            Y = label_y + (Main.Game.MenuSize_Y * 5) + (Main.Game.MenuSize_Y / 2);
            X = label_x - (Main.Game.MenuSize_X * 2) - Main.Game.MenuSize_X;

            Picture? right_hand_picture = GetPicture("Right_Hand");
            if (right_hand_picture != null)
            {
                right_hand_picture.Region = new Region(x, y, width, height);
            }

            Button? right_hand_button = GetButton("Right_Hand");
            if (right_hand_button != null)
            {
                right_hand_button.Region = new Region(X, Y, label_width, button_height);
            }

            Label? right_hand_label = GetLabel("Right_Hand");
            if (right_hand_label != null)
            {
                right_hand_label.Region = new Region(X, Y, label_width, label_height);
            }

            ProgressBar? right_hand_bar = GetProgressBar("Right_Hand");
            if (right_hand_bar != null)
            {
                right_hand_bar.Base_Region = new Region(X, Y + label_height, label_width, prog_height);
            }

            X = label_x + label_width + Main.Game.MenuSize_X + (Main.Game.MenuSize_X / 2) - (Main.Game.MenuSize_X / 4);

            Picture? left_hand_picture = GetPicture("Left_Hand");
            if (left_hand_picture != null)
            {
                left_hand_picture.Region = new Region(x, y, width, height);
            }

            Button? left_hand_button = GetButton("Left_Hand");
            if (left_hand_button != null)
            {
                left_hand_button.Region = new Region(X, Y, label_width, button_height);
            }

            Label? left_hand_label = GetLabel("Left_Hand");
            if (left_hand_label != null)
            {
                left_hand_label.Region = new Region(X, Y, label_width, label_height);
            }

            ProgressBar? left_hand_bar = GetProgressBar("Left_Hand");
            if (left_hand_bar != null)
            {
                left_hand_bar.Base_Region = new Region(X, Y + label_height, label_width, prog_height);
            }

            Y = label_y + (Main.Game.MenuSize_Y * 5) + (Main.Game.MenuSize_Y / 2);
            X = label_x;

            Picture? groin_picture = GetPicture("Groin");
            if (groin_picture != null)
            {
                groin_picture.Region = new Region(x, y, width, height);
            }

            Button? groin_button = GetButton("Groin");
            if (groin_button != null)
            {
                groin_button.Region = new Region(X, Y, label_width, button_height);
            }

            Label? groin_label = GetLabel("Groin");
            if (groin_label != null)
            {
                groin_label.Region = new Region(X, Y, label_width, label_height);
            }

            ProgressBar? groin_bar = GetProgressBar("Groin");
            if (groin_bar != null)
            {
                groin_bar.Base_Region = new Region(X, Y + label_height, label_width, prog_height);
            }

            Y = label_y + (Main.Game.MenuSize_Y * 8);
            X = label_x - Main.Game.MenuSize_X - (Main.Game.MenuSize_X / 4);

            Picture? right_leg_picture = GetPicture("Right_Leg");
            if (right_leg_picture != null)
            {
                right_leg_picture.Region = new Region(x, y, width, height);
            }

            Button? right_leg_button = GetButton("Right_Leg");
            if (right_leg_button != null)
            {
                right_leg_button.Region = new Region(X, Y, label_width, button_height);
            }

            Label? right_leg_label = GetLabel("Right_Leg");
            if (right_leg_label != null)
            {
                right_leg_label.Region = new Region(X, Y, label_width, label_height);
            }

            ProgressBar? right_leg_bar = GetProgressBar("Right_Leg");
            if (right_leg_bar != null)
            {
                right_leg_bar.Base_Region = new Region(X, Y + label_height, label_width, prog_height);
            }

            X = label_x + label_width - (Main.Game.MenuSize_X / 2);

            Picture? left_leg_picture = GetPicture("Left_Leg");
            if (left_leg_picture != null)
            {
                left_leg_picture.Region = new Region(x, y, width, height);
            }

            Button? left_leg_button = GetButton("Left_Leg");
            if (left_leg_button != null)
            {
                left_leg_button.Region = new Region(X, Y, label_width, button_height);
            }

            Label? left_leg_label = GetLabel("Left_Leg");
            if (left_leg_label != null)
            {
                left_leg_label.Region = new Region(X, Y, label_width, label_height);
            }

            ProgressBar? left_leg_bar = GetProgressBar("Left_Leg");
            if (left_leg_bar != null)
            {
                left_leg_bar.Base_Region = new Region(X, Y + label_height, label_width, prog_height);
            }

            Y = label_y + (Main.Game.MenuSize_Y * 11) + (Main.Game.MenuSize_Y / 2);
            X = label_x - Main.Game.MenuSize_X - (Main.Game.MenuSize_X / 4);

            Picture? right_foot_picture = GetPicture("Right_Foot");
            if (right_foot_picture != null)
            {
                right_foot_picture.Region = new Region(x, y, width, height);
            }

            Button? right_foot_button = GetButton("Right_Foot");
            if (right_foot_button != null)
            {
                right_foot_button.Region = new Region(X, Y, label_width, button_height);
            }

            Label? right_foot_label = GetLabel("Right_Foot");
            if (right_foot_label != null)
            {
                right_foot_label.Region = new Region(X, Y, label_width, label_height);
            }

            ProgressBar? right_foot_bar = GetProgressBar("Right_Foot");
            if (right_foot_bar != null)
            {
                right_foot_bar.Base_Region = new Region(X, Y + label_height, label_width, prog_height);
            }

            X = label_x + label_width - (Main.Game.MenuSize_X / 2);

            Picture? left_foot_picture = GetPicture("Left_Foot");
            if (left_foot_picture != null)
            {
                left_foot_picture.Region = new Region(x, y, width, height);
            }

            Button? left_foot_button = GetButton("Left_Foot");
            if (left_foot_button != null)
            {
                left_foot_button.Region = new Region(X, Y, label_width, button_height);
            }

            Label? left_foot_label = GetLabel("Left_Foot");
            if (left_foot_label != null)
            {
                left_foot_label.Region = new Region(X, Y, label_width, label_height);
            }

            ProgressBar? left_foot_bar = GetProgressBar("Left_Foot");
            if (left_foot_bar != null)
            {
                left_foot_bar.Base_Region = new Region(X, Y + label_height, label_width, prog_height);
            }
        }

        #endregion
    }
}

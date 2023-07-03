using System;
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
using OP_Engine.Jobs;

using Despicaville.Util;

namespace Despicaville.Menus
{
    public class Menu_Combat : Menu
    {
        #region Variables

        List<string> AttackingWith = new List<string>();
        bool AttackingWith_Extended;

        List<string> AttackType = new List<string>();
        bool AttackType_Extended;

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
                UpdateStats();

                base.Update(gameRef, content);
            }
        }

        private void UpdateControls()
        {
            bool found = false;

            foreach (Button button in Buttons)
            {
                if (button.Visible &&
                    !string.IsNullOrEmpty(button.Text))
                {
                    if (button.Enabled)
                    {
                        bool okay = false;

                        if (AttackingWith_Extended)
                        {
                            if (button.Name == "AttackingWith_Option")
                            {
                                okay = true;
                            }
                        }
                        else if (AttackType_Extended)
                        {
                            if (button.Name == "AttackType_Option")
                            {
                                okay = true;
                            }
                        }
                        else
                        {
                            okay = true;
                        }

                        if (InputManager.MouseWithin(button.Region.ToRectangle) &&
                            okay)
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

                                button.Selected = false;

                                break;
                            }
                        }
                        else if (InputManager.Mouse_Moved)
                        {
                            button.Selected = false;
                        }
                    }
                    else
                    {
                        button.Opacity = 0.8f;
                        button.Selected = false;
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
                                Examine(picture.HoverText);
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

            if (button.Name == "Cancel")
            {
                Close();
            }
            else if (button.Name == "Attack")
            {
                Attack();
            }
            else if (button.Name == "AttackingWith_Value")
            {
                if (AttackingWith_Extended)
                {
                    Retract_AttackingWith();
                }
                else
                {
                    Extend_AttackingWith();
                }
            }
            else if (button.Name.Contains("AttackingWith_Option"))
            {
                Button base_button = GetButton("AttackingWith_Value");
                base_button.Text = button.Text;

                Retract_AttackingWith();

                GetButton("AttackType_Value").Text = "<Click Here>";
                GetButton("Attack").Enabled = EnableAttack();
            }
            else if (button.Name == "AttackType_Value")
            {
                Button base_button = GetButton("AttackingWith_Value");
                if (!string.IsNullOrEmpty(base_button.Text))
                {
                    if (AttackType_Extended)
                    {
                        Retract_AttackType();
                    }
                    else
                    {
                        Extend_AttackType();
                    }
                }
            }
            else if (button.Name.Contains("AttackType_Option"))
            {
                Button base_button = GetButton("AttackType_Value");
                base_button.Text = button.Text;

                Retract_AttackType();

                GetButton("Attack").Enabled = EnableAttack();
            }
        }

        private void CheckClick(Picture picture)
        {
            AssetManager.PlaySound_Random("Click");

            if (picture.Name.Contains("Paperdoll"))
            {
                int index = picture.Name.IndexOf("_");
                string body_part = picture.Name.Substring(index + 1, picture.Name.Length - (index + 1));
                GetLabel("PartTarget_Value").Text = CharacterUtil.BodyPartToName(body_part);
            }

            GetButton("Attack").Enabled = EnableAttack();
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

        private void Attack()
        {
            Character player = Handler.GetPlayer();
            Character target = Handler.Interaction_Character;
            player.Target_ID = target.ID;

            string attacking_with = GetButton("AttackingWith_Value").Text;
            string attack_type = GetButton("AttackType_Value").Text;
            string part_target = GetLabel("PartTarget_Value").Text;

            Task task = new Task();
            task.Name = attack_type;
            task.Assignment = part_target;
            task.Type = attacking_with;
            task.OwnerIDs.Add(player.ID);
            task.Keep_On_Completed = true;
            task.StartTime = new TimeHandler(TimeManager.Now);
            task.EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CombatUtil.AttackTime(player, attack_type)));
            task.Location = new Vector3(target.Location.X, target.Location.Y, 0);

            player.Job.Tasks.Add(task);

            GameUtil.AddMessage("You " + task.Name.ToLower() + " your " + task.Type.ToLower() + " at the " + task.Assignment.ToLower() + " of " + target.Name + ".");

            Close();
        }

        private bool EnableAttack()
        {
            Character player = Handler.GetPlayer();

            string attack_type = GetButton("AttackType_Value").Text;

            if (GetLabel("PartTarget_Value").Text != "<Click Body>" &&
                GetButton("AttackingWith_Value").Text != "<Click Here>" &&
                GetButton("AttackType_Value").Text != "<Click Here>" &&
                !string.IsNullOrEmpty(attack_type))
            {
                GetButton("Attack").Opacity = 1;
                GetLabel("AttackTime_Value").Text = CombatUtil.AttackTime(player, attack_type) + "ms";
                return true;
            }

            GetButton("Attack").Opacity = 0.8f;
            GetLabel("AttackTime_Value").Text = "0ms";

            return false;
        }

        private void UpdateStats()
        {
            Character player = Handler.GetPlayer();
            Character target = Handler.Interaction_Character;

            if (player != null &&
                target != null)
            {
                foreach (string body_part in Handler.BodyParts)
                {
                    Picture picture = GetPicture("Paperdoll_" + body_part);
                    if (picture != null)
                    {
                        BodyPart bodyPart = target.GetBodyPart(body_part);
                        if (bodyPart != null)
                        {
                            Something hp = bodyPart.GetStat("HP");
                            if (hp != null)
                            {
                                string hp_value = hp.Name + ": " + hp.Value.ToString("0.##") + "/" + (int)hp.Max_Value + "%";
                                string attack_type = GetButton("AttackType_Value").Text;
                                string hit_chance = "Hit Chance: " + CombatUtil.ChanceToHitBodyPart(player, target, body_part, attack_type).ToString("0.##") + "%";
                                picture.HoverText = hp_value + "\n" + hit_chance;
                            }
                        }
                    }
                }
            }
        }

        private void InitBodyDisplay()
        {
            foreach (string body_part in Handler.BodyParts)
            {
                CombatUtil.Update_Citizen_BodyStat(Handler.Interaction_Character, body_part);
            }
        }

        public override void Load()
        {
            if (Handler.Interaction_Character != null)
            {
                GetLabel("Name_Frame").Text = Handler.Interaction_Character.Name;
                InitBodyDisplay();
            }
        }

        private void Extend_AttackType()
        {
            AttackType.Clear();

            Button attackingWithValue = GetButton("AttackingWith_Value");
            if (attackingWithValue.Text.Contains("Hand"))
            {
                //AttackType.Add("Grab");
                AttackType.Add("Punch");
            }
            else
            {
                Character player = Handler.GetPlayer();
                Inventory inventory = player.Inventory;
                Item item = inventory.GetItem(attackingWithValue.Text);
                if (item != null)
                {
                    AttackType.Add("Swing");
                    AttackType.Add("Throw");

                    if (InventoryUtil.IsWeapon(item) &&
                        !AttackType.Contains(item.Task))
                    {
                        AttackType.Add(item.Task);
                    }
                }
            }

            if (AttackType.Count > 0)
            {
                Button base_button = GetButton("AttackType_Value");
                base_button.Opacity = 0.8f;
                base_button.Selected = false;

                int x = (int)base_button.Region.X;
                int y = (int)base_button.Region.Y + Main.Game.MenuSize_Y;
                int width = (int)base_button.Region.Width;
                int height = (int)base_button.Region.Height;

                for (int i = 0; i < AttackType.Count; i++)
                {
                    AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "AttackType_Option", AttackType[i], Color.White, Color.Red, AssetManager.Textures["ButtonFrame"], AssetManager.Textures["ButtonFrame_Highlight"], null,
                        new Region(x, y + (Main.Game.MenuSize_Y * i), width, height), false, true);
                }

                AttackType_Extended = true;
            }
        }

        private void Retract_AttackType()
        {
            Button base_button = GetButton("AttackType_Value");
            base_button.Opacity = 1;
            base_button.Selected = false;

            for (int i = 0; i < Buttons.Count; i++)
            {
                Button button = Buttons[i];
                if (button.Name.Contains("AttackType_Option"))
                {
                    Buttons.Remove(button);
                    i--;
                }
            }

            AttackType_Extended = false;
        }

        private void Extend_AttackingWith()
        {
            AttackingWith.Clear();

            Character player = Handler.GetPlayer();
            if (player != null)
            {
                Item leftHandItem = InventoryUtil.Get_EquippedItem(player, "Left Weapon Slot");
                if (leftHandItem != null)
                {
                    AttackingWith.Add(leftHandItem.Name);
                }
                else if (CombatUtil.InRange(player, Handler.Interaction_Character, "Punch"))
                {
                    AttackingWith.Add("Left Hand");
                }

                Item rightHandItem = InventoryUtil.Get_EquippedItem(player, "Right Weapon Slot");
                if (rightHandItem != null)
                {
                    AttackingWith.Add(rightHandItem.Name);
                }
                else if (CombatUtil.InRange(player, Handler.Interaction_Character, "Punch"))
                {
                    AttackingWith.Add("Right Hand");
                }
            }

            if (AttackingWith.Count > 0)
            {
                Button base_button = GetButton("AttackingWith_Value");
                base_button.Opacity = 0.8f;
                base_button.Selected = false;

                int x = (int)base_button.Region.X;
                int y = (int)base_button.Region.Y + Main.Game.MenuSize_Y;
                int width = (int)base_button.Region.Width;
                int height = (int)base_button.Region.Height;

                for (int i = 0; i < AttackingWith.Count; i++)
                {
                    AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "AttackingWith_Option", AttackingWith[i], Color.White, Color.Red, AssetManager.Textures["ButtonFrame"], AssetManager.Textures["ButtonFrame_Highlight"], null,
                        new Region(x, y + (Main.Game.MenuSize_Y * i), width, height), false, true);
                }

                AttackingWith_Extended = true;
            }
        }

        private void Retract_AttackingWith()
        {
            Button base_button = GetButton("AttackingWith_Value");
            base_button.Opacity = 1;
            base_button.Selected = false;

            for (int i = 0; i < Buttons.Count; i++)
            {
                Button button = Buttons[i];
                if (button.Name.Contains("AttackingWith_Option"))
                {
                    Buttons.Remove(button);
                    i--;
                }
            }

            AttackingWith_Extended = false;
        }

        public override void Load(ContentManager content)
        {
            Clear();

            GraphicsDevice graphics = Main.Game.GraphicsManager.GraphicsDevice;

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Name_Frame", "", Color.White, AssetManager.Textures["Frame"], new Region(0, 0, 0, 0), true);
            AddPicture(Handler.GetID(), "Body_Frame", AssetManager.GetTextureCopy(graphics, "Frame_Large"), new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Paperdoll_Right_Foot", AssetManager.GetTextureCopy(graphics, "Paperdoll_Right_Foot"), new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Paperdoll_Left_Foot", AssetManager.GetTextureCopy(graphics, "Paperdoll_Left_Foot"), new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Paperdoll_Right_Leg", AssetManager.GetTextureCopy(graphics, "Paperdoll_Right_Leg"), new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Paperdoll_Left_Leg", AssetManager.GetTextureCopy(graphics, "Paperdoll_Left_Leg"), new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Paperdoll_Right_Hand", AssetManager.GetTextureCopy(graphics, "Paperdoll_Right_Hand"), new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Paperdoll_Left_Hand", AssetManager.GetTextureCopy(graphics, "Paperdoll_Left_Hand"), new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Paperdoll_Right_Arm", AssetManager.GetTextureCopy(graphics, "Paperdoll_Right_Arm"), new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Paperdoll_Left_Arm", AssetManager.GetTextureCopy(graphics, "Paperdoll_Left_Arm"), new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Paperdoll_Groin", AssetManager.GetTextureCopy(graphics, "Paperdoll_Groin"), new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Paperdoll_Torso", AssetManager.GetTextureCopy(graphics, "Paperdoll_Torso"), new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Paperdoll_Neck", AssetManager.GetTextureCopy(graphics, "Paperdoll_Neck"), new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Paperdoll_Head", AssetManager.GetTextureCopy(graphics, "Paperdoll_Head"), new Region(0, 0, 0, 0), Color.White, true);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "PartTarget", "Target:", Color.White, new Region(0, 0, 0, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "PartTarget_Value", "<Click Body>", Color.White, AssetManager.Textures["Frame"], new Region(0, 0, 0, 0), true);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "AttackingWith", "Attacking With:", Color.White, new Region(0, 0, 0, 0), true);
            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "AttackingWith_Value", "<Click Here>", Color.White, Color.Red, AssetManager.Textures["ButtonFrame"], AssetManager.Textures["ButtonFrame_Highlight"], null,
                new Region(0, 0, 0, 0), false, true);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "AttackType", "Attack Type:", Color.White, new Region(0, 0, 0, 0), true);
            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "AttackType_Value", "", Color.White, Color.Red, AssetManager.Textures["ButtonFrame"], AssetManager.Textures["ButtonFrame_Highlight"], null,
                new Region(0, 0, 0, 0), false, true);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "AttackTime", "Attack Time:", Color.White, new Region(0, 0, 0, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "AttackTime_Value", "0ms", Color.White, AssetManager.Textures["ButtonFrame"], new Region(0, 0, 0, 0), true);
            GetLabel("AttackTime_Value").Opacity = 0.8f;

            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Cancel", "Cancel", Color.White, Color.Red, AssetManager.Textures["ButtonFrame"], AssetManager.Textures["ButtonFrame_Highlight"], null,
                new Region(0, 0, 0, 0), false, true);
            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Attack", "Attack", Color.White, Color.Red, Color.DarkGray, AssetManager.Textures["ButtonFrame"], AssetManager.Textures["ButtonFrame_Highlight"], AssetManager.Textures["ButtonFrame"],
                new Region(0, 0, 0, 0), false, true);
            GetButton("Attack").Enabled = false;

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["Frame"], new Region(0, 0, 0, 0), false);

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            GetLabel("Examine").Region = new Region(0, 0, 0, 0);

            int x = Main.Game.MenuSize_X * 9;
            int y = Main.Game.MenuSize_Y;
            int width = Main.Game.MenuSize_X * 5;
            int height = Main.Game.MenuSize_X * 10;

            Picture frame = GetPicture("Body_Frame");
            frame.Region = new Region(x, y, width, height);

            GetLabel("Name_Frame").Region = new Region(x, y - Main.Game.MenuSize_Y, width, Main.Game.MenuSize_Y);

            x += width + Main.Game.MenuSize_X;
            //y += Main.Game.MenuSize_Y * 3;
            width = Main.Game.MenuSize_X * 4;
            height = Main.Game.MenuSize_Y;
            GetLabel("PartTarget").Region = new Region(x, y, width, height);
            GetLabel("PartTarget_Value").Region = new Region(x + width, y, width, height);

            y += Main.Game.MenuSize_Y * 2;
            GetLabel("AttackingWith").Region = new Region(x, y, width, height);
            GetButton("AttackingWith_Value").Region = new Region(x + width, y, width, height);

            y += Main.Game.MenuSize_Y * 2;
            GetLabel("AttackType").Region = new Region(x, y, width, height);
            GetButton("AttackType_Value").Region = new Region(x + width, y, width, height);

            y += Main.Game.MenuSize_Y * 4;
            GetLabel("AttackTime").Region = new Region(x, y, width, height);
            GetLabel("AttackTime_Value").Region = new Region(x + width, y, width, height);

            y += Main.Game.MenuSize_Y;
            GetButton("Cancel").Region = new Region(x, y, width, height);
            GetButton("Attack").Region = new Region(x + width, y, width, height);

            int paperdoll_x = (int)frame.Region.X;
            int paperdoll_y = (int)frame.Region.Y;
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
        }

        private void Examine(string text)
        {
            Label examine = GetLabel("Examine");
            examine.Text = text;

            int width = Main.Game.MenuSize_X * 5;
            int height = (Main.Game.MenuSize_Y / 4) * 6;

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

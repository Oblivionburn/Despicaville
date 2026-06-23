using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Menus;
using OP_Engine.Characters;
using OP_Engine.Utility;
using OP_Engine.Time;
using Despicaville.Util;

namespace Despicaville.Menus
{
    public class Menu_Health : Menu
    {
        #region Variables



        #endregion

        #region Constructor

        public Menu_Health(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Health";
            Load(content);
        }

        #endregion

        #region Methods

        public override void Update(Game? gameRef, ContentManager? content)
        {
            if (Visible ||
                Active)
            {
                if (!TimeManager.Paused)
                {
                    Update_BodyStats();
                    UpdateStats();
                    UpdateControls();

                    base.Update(gameRef, content);
                }
            }
        }

        private void UpdateControls()
        {
            bool found = false;

            foreach (Picture picture in Pictures)
            {
                picture.Opacity = 0.8f;
            }

            foreach (Picture picture in Pictures)
            {
                if (picture.Visible)
                {
                    if (picture.Region != null &&
                        InputManager.MouseWithin(picture.Region.ToRectangle))
                    {
                        if (GameUtil.MouseOnPixel(picture))
                        {
                            found = true;

                            picture.Opacity = 1;

                            if (picture.HoverText != null)
                            {
                                GameUtil.Examine(this, picture.HoverText);
                            }

                            if (InputManager.Mouse_LB_Pressed)
                            {
                                found = false;
                                CheckClick(picture);
                            }

                            break;
                        }
                    }
                }
            }

            if (!found)
            {
                Label? examine = GetLabel("Examine");
                if (examine != null)
                {
                    examine.Visible = false;
                }
            }
        }

        private void CheckClick(Picture picture)
        {
            AssetManager.PlaySound_Random("Click");
            InputManager.Mouse?.Flush();

            Handler.Selected_BodyPart = picture.Name;

            TimeManager.Paused = true;

            Menu? menu = MenuManager.GetMenu("Wounds");
            if (menu != null)
            {
                menu.Load();
                menu.Active = true;
                menu.Visible = true;
            }
        }

        private void UpdateStats()
        {
            foreach (string body_part in Handler.BodyParts)
            {
                Picture? picture = GetPicture(body_part);
                if (picture != null)
                {
                    BodyPart? bodyPart = Handler.Player?.GetBodyPart(body_part);
                    if (bodyPart != null)
                    {
                        Property? hp = bodyPart.GetStat("HP");
                        if (hp != null)
                        {
                            picture.HoverText = hp.Name + ": " + hp.Value.ToString("0.##") + "/" + (int)hp.Max_Value + "%";
                        }
                    }
                }
            }
        }

        private void Update_BodyStats()
        {
            foreach (string body_part in Handler.BodyParts)
            {
                Picture? picture = GetPicture(body_part);
                if (picture != null)
                {
                    BodyPart? part = Handler.Player?.GetBodyPart(body_part);
                    if (part != null)
                    {
                        Property? hp = part.GetStat("HP");
                        if (hp != null)
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
                    }
                }
            }
        }

        public override void Load()
        {
            Clear();

            Texture2D? frame = Handler.GetTexture("Frame");
            Texture2D? frame_Large = Handler.GetTexture("Frame_Large");

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

            AddPicture(Handler.GetID(), "Panel", frame_Large, new Region(0, 0, 0, 0), Color.White, false);

            AddPicture(Handler.GetID(), "Right_Foot", paperdoll_Right_Foot, new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Left_Foot", paperdoll_Left_Foot, new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Right_Leg", paperdoll_Right_Leg, new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Left_Leg", paperdoll_Left_Leg, new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Right_Hand", paperdoll_Right_Hand, new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Left_Hand", paperdoll_Left_Hand, new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Right_Arm", paperdoll_Right_Arm, new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Left_Arm", paperdoll_Left_Arm, new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Groin", paperdoll_Groin, new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Torso", paperdoll_Torso, new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Neck", paperdoll_Neck, new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Head", paperdoll_Head, new Region(0, 0, 0, 0), Color.White, true);

            Update_BodyStats();

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, frame, new Region(0, 0, 0, 0), false);

            if (Main.Game == null)
            {
                return;
            }
            Resize(Main.Game.Resolution);
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

            Picture? panel = GetPicture("Panel");
            if (panel != null)
            {
                panel.Region = new Region(0, Main.Game.MenuSize_Y, Main.Game.MenuSize_X * 5, Main.Game.MenuSize_Y * 12);
            }

            float paperdoll_x = 0;
            float paperdoll_y = Main.Game.MenuSize_Y;
            float paperdoll_width = Main.Game.MenuSize_X * 5;
            float paperdoll_height = Main.Game.MenuSize_X * 10;

            Picture? head = GetPicture("Head");
            if (head != null)
            {
                head.Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            }

            Picture? neck = GetPicture("Neck");
            if (neck != null)
            {
                neck.Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            }

            Picture? torso = GetPicture("Torso");
            if (torso != null)
            {
                torso.Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            }

            Picture? right_Arm = GetPicture("Right_Arm");
            if (right_Arm != null)
            {
                right_Arm.Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            }

            Picture? right_Hand = GetPicture("Right_Hand");
            if (right_Hand != null)
            {
                right_Hand.Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            }

            Picture? left_Arm = GetPicture("Left_Arm");
            if (left_Arm != null)
            {
                left_Arm.Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            }

            Picture? left_Hand = GetPicture("Left_Hand");
            if (left_Hand != null)
            {
                left_Hand.Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            }

            Picture? groin = GetPicture("Groin");
            if (groin != null)
            {
                groin.Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            }

            Picture? right_Leg = GetPicture("Right_Leg");
            if (right_Leg != null)
            {
                right_Leg.Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            }

            Picture? right_Foot = GetPicture("Right_Foot");
            if (right_Foot != null)
            {
                right_Foot.Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            }

            Picture? left_Leg = GetPicture("Left_Leg");
            if (left_Leg != null)
            {
                left_Leg.Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            }

            Picture? left_Foot = GetPicture("Left_Foot");
            if (left_Foot != null)
            {
                left_Foot.Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            }
        }

        #endregion
    }
}

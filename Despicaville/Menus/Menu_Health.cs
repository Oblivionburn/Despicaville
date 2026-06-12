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

        public override void Update(Game gameRef, ContentManager content)
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
                    if (InputManager.MouseWithin(picture.Region.ToRectangle))
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
                GetLabel("Examine").Visible = false;
            }
        }

        private void CheckClick(Picture picture)
        {
            AssetManager.PlaySound_Random("Click");
            InputManager.Mouse.Flush();

            Handler.Selected_BodyPart = picture.Name;

            TimeManager.Paused = true;

            Menu menu = MenuManager.GetMenu("Wounds");
            menu.Load();
            menu.Active = true;
            menu.Visible = true;
        }

        private void UpdateStats()
        {
            foreach (string body_part in Handler.BodyParts)
            {
                Picture picture = GetPicture(body_part);
                if (picture != null)
                {
                    BodyPart bodyPart = Handler.Player.GetBodyPart(body_part);
                    if (bodyPart != null)
                    {
                        Property hp = bodyPart.GetStat("HP");
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
                Picture picture = GetPicture(body_part);
                if (picture != null)
                {
                    BodyPart part = Handler.Player.GetBodyPart(body_part);
                    if (part != null)
                    {
                        Property hp = part.GetStat("HP");
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

            AddPicture(Handler.GetID(), "Panel", AssetManager.Textures["Frame_Large"], new Region(0, 0, 0, 0), Color.White, false);

            AddPicture(Handler.GetID(), "Right_Foot", AssetManager.Textures["Paperdoll_Right_Foot"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Left_Foot", AssetManager.Textures["Paperdoll_Left_Foot"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Right_Leg", AssetManager.Textures["Paperdoll_Right_Leg"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Left_Leg", AssetManager.Textures["Paperdoll_Left_Leg"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Right_Hand", AssetManager.Textures["Paperdoll_Right_Hand"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Left_Hand", AssetManager.Textures["Paperdoll_Left_Hand"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Right_Arm", AssetManager.Textures["Paperdoll_Right_Arm"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Left_Arm", AssetManager.Textures["Paperdoll_Left_Arm"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Groin", AssetManager.Textures["Paperdoll_Groin"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Torso", AssetManager.Textures["Paperdoll_Torso"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Neck", AssetManager.Textures["Paperdoll_Neck"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "Head", AssetManager.Textures["Paperdoll_Head"], new Region(0, 0, 0, 0), Color.White, true);

            Update_BodyStats();

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["Frame"], new Region(0, 0, 0, 0), false);

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            GetLabel("Examine").Region = new Region(0, 0, 0, 0);
            GetPicture("Panel").Region = new Region(0, Main.Game.MenuSize_Y, Main.Game.MenuSize_X * 5, Main.Game.MenuSize_Y * 12);

            float paperdoll_x = 0;
            float paperdoll_y = Main.Game.MenuSize_Y;
            float paperdoll_width = Main.Game.MenuSize_X * 5;
            float paperdoll_height = Main.Game.MenuSize_X * 10;

            GetPicture("Head").Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            GetPicture("Neck").Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            GetPicture("Torso").Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            GetPicture("Right_Arm").Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            GetPicture("Right_Hand").Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            GetPicture("Left_Arm").Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            GetPicture("Left_Hand").Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            GetPicture("Groin").Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            GetPicture("Right_Leg").Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            GetPicture("Right_Foot").Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            GetPicture("Left_Leg").Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
            GetPicture("Left_Foot").Region = new Region(paperdoll_x, paperdoll_y, paperdoll_width, paperdoll_height);
        }

        #endregion
    }
}

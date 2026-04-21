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

        private int InitBody;

        #endregion

        #region Constructor

        public Menu_Health(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Health";
            Load();
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

        private void UpdateStats()
        {
            foreach (string body_part in Handler.BodyParts)
            {
                Picture picture = GetPicture("Paperdoll_" + body_part);
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

        private void InitBodyDisplay()
        {
            foreach (string body_part in Handler.BodyParts)
            {
                CombatUtil.Update_Player_BodyStat(Handler.Player, body_part);
            }
        }

        public override void Load()
        {
            Clear();

            AddPicture(Handler.GetID(), "Panel", AssetManager.Textures["Frame_Large"], new Region(0, 0, 0, 0), Color.White, false);

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

        #endregion
    }
}

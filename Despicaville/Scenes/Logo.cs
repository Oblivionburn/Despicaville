using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Scenes;
using OP_Engine.Menus;
using OP_Engine.Inputs;
using OP_Engine.Sounds;
using OP_Engine.Controls;
using OP_Engine.Utility;

namespace Despicaville.Scenes
{
    public class Logo : Scene
    {
        #region Variables

        public float value;
        public bool Increasing;
        public int delay = 5;

        #endregion

        #region Constructor

        public Logo()
        {
            ID = Handler.GetID();
            Name = "Logo";
            Load();
        }

        #endregion

        #region Methods

        public override void Update(Game gameRef, ContentManager content)
        {
            if (Visible)
            {
                if (InputManager.Mouse_LB_Pressed)
                {
                    Increasing = false;
                    value = 0;
                    SoundManager.StopSound();
                }

                if (Increasing)
                {
                    delay--;
                    if (delay == 0)
                    {
                        value += 1.25f;

                        if (value >= 255)
                        {
                            Increasing = false;
                        }

                        delay = 1;
                    }
                }
                else
                {
                    value -= 1.25f;

                    if (value <= 0)
                    {
                        Visible = false;

                        SceneManager.ChangeScene("Title");

                        Menu main = MenuManager.GetMenu("Main");
                        main.Visible = true;
                        main.Active = true;

                        SoundManager.NeedMusic = true;
                    }
                }
            }
        }

        public override void DrawMenu(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
                foreach (Picture picture in Menu.Pictures)
                {
                    picture.DrawColor.R = (byte)value;
                    picture.DrawColor.G = (byte)value;
                    picture.DrawColor.B = (byte)value;

                    picture.Draw(spriteBatch);
                }
            }
        }

        public override void Load()
        {
            Increasing = true;
            value = 0;

            Menu.AddPicture(0, "Logo", AssetManager.Textures["Logo"], new Region(0, 0, 0, 0), new Color(0, 0, 0, 255), true);
            Menu.AddPicture(0, "fmod", AssetManager.Textures["fmod"], new Region(0, 0, 0, 0), new Color(0, 0, 0, 255), true);
            Menu.Visible = true;

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            Menu.GetPicture("Logo").Region = new Region(0, 0, Main.Game.ScreenWidth, Main.Game.ScreenHeight);

            Texture2D fmod = AssetManager.Textures["fmod"];
            int width = (fmod.Width / 5);
            int height = (fmod.Height / 5);
            Menu.GetPicture("fmod").Region = new Region(Main.Game.ScreenWidth - width, Main.Game.ScreenHeight - height, width, height);
        }

        #endregion
    }
}

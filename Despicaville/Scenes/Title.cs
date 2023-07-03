using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Scenes;
using OP_Engine.Menus;
using OP_Engine.Sounds;
using OP_Engine.Controls;
using OP_Engine.Utility;

namespace Despicaville.Scenes
{
    public class Title : Scene
    {
        #region Variables



        #endregion

        #region Constructor

        public Title(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Title";
            Load(content);
        }

        #endregion

        #region Methods

        public override void Update(Game gameRef, ContentManager content)
        {
            if (Visible)
            {
                if (SoundManager.NeedMusic)
                {
                    AssetManager.PlayMusic_Random("Title", true);
                }
            }
        }

        public override void DrawMenu(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
                foreach (Picture existing in Menu.Pictures)
                {
                    existing.Draw(spriteBatch);
                }

                foreach (Label label in Menu.Labels)
                {
                    label.Draw(spriteBatch);
                }
            }
        }

        public override void Load(ContentManager content)
        {
            Menu.Clear();

            Menu.AddPicture(0, "Title", AssetManager.Textures["Title"], new Region(0, 0, 0, 0), Color.White, true);

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            Menu.GetPicture("Title").Region = new Region(0, 0, Main.Game.ScreenWidth, Main.Game.ScreenHeight);
        }

        #endregion
    }
}

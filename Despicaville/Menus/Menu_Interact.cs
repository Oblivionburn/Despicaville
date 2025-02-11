using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

using OP_Engine.Menus;
using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Utility;
using OP_Engine.Time;
using OP_Engine.Characters;
using OP_Engine.Tiles;

using Despicaville.Util;

namespace Despicaville.Menus
{
    public class Menu_Interact : Menu
    {
        #region Variables

        //private Direction expanding;
        private Direction listing;

        private int x;
        private int y;
        private int width;
        private int height;

        public bool Loaded;

        #endregion

        #region Constructors

        public Menu_Interact(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Interact";
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

        public void UpdateControls()
        {
            foreach (Button button in Buttons)
            {
                if (button.Visible &&
                    button.Enabled)
                {
                    if (InputManager.MouseWithin(button.Region.ToRectangle))
                    {
                        button.Opacity = 1;
                        button.Selected = true;

                        if (InputManager.Mouse_LB_Pressed_Flush)
                        {
                            CheckClick(button);
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
        }

        public void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");

            Character player = Handler.GetPlayer();

            if (button.Name == "Cancel")
            {
                Close();
            }
            else if (button.Name == "Examine")
            {
                Tasker.AddTask(player, "Examine", true, true, TimeSpan.FromMilliseconds(Handler.ActionRate), default, player.Direction);
                Close();
            }
            else if (button.Name == "Use" ||
                     button.Name == "Search")
            {
                Tasker.Interact(Handler.Interaction_Tile, player);
                Close();
            }
            else if (button.Name == "Attack")
            {
                MenuManager.GetMenu("Combat").Open();
                Visible = false;
                Active = false;
            }
        }

        public override void Open()
        {
            TimeManager.Paused = true;
            Visible = true;
            Active = true;
        }

        public override void Close()
        {
            TimeManager.Paused = false;
            Visible = false;
            Active = false;
        }

        public override void Load()
        {
            Loaded = false;
            Character player = Handler.GetPlayer();

            Clear();

            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Cancel", "Cancel", Color.White, Color.Red, AssetManager.Textures["Frame"], AssetManager.Textures["Frame"], null,
                new Region(0, 0, 0, 0), false, true);
            AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "Examine", Color.White, Color.Red, AssetManager.Textures["Frame"], AssetManager.Textures["Frame"], null,
                new Region(0, 0, 0, 0), false, true);

            if (Handler.Interaction_Character != null)
            {
                if (WorldUtil.NextTo(Handler.Interaction_Character.Location, player.Location))
                {
                    AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Talk", "Talk", Color.White, Color.Red, AssetManager.Textures["Frame"], AssetManager.Textures["Frame"], null,
                        new Region(0, 0, 0, 0), false, false);
                    AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Attack", "Attack", Color.White, Color.Red, AssetManager.Textures["Frame"], AssetManager.Textures["Frame"], null,
                        new Region(0, 0, 0, 0), false, false);
                }
                else if (CombatUtil.CanAttack_Ranged(player) &&
                         CombatUtil.InRange(player, Handler.Interaction_Character, "Shoot"))
                {
                    AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Attack", "Attack", Color.White, Color.Red, AssetManager.Textures["Frame"], AssetManager.Textures["Frame"], null,
                        new Region(0, 0, 0, 0), false, false);
                }
            }
            else if (Handler.Interaction_Tile != null &&
                     WorldUtil.NextTo(Handler.Interaction_Tile.Location, player.Location))
            {
                AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Use", "Use", Color.White, Color.Red, AssetManager.Textures["Frame"], AssetManager.Textures["Frame"], null,
                    new Region(0, 0, 0, 0), false, false);
                AddButton(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Search", "Search", Color.White, Color.Red, AssetManager.Textures["Frame"], AssetManager.Textures["Frame"], null,
                    new Region(0, 0, 0, 0), false, false);
            }

            Loaded = true;
            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            if (Loaded)
            {
                width = Main.Game.MenuSize_X * 3;
                height = Main.Game.MenuSize_Y;

                #region Get Position

                Character player = Handler.GetPlayer();
                if (player != null)
                {
                    Region region = null;
                    List<Tile> visible = Handler.VisibleTiles[player.ID];
                    foreach (Tile tile in visible)
                    {
                        if (tile.Visible)
                        {
                            region = tile.Region;
                            break;
                        }
                    }

                    if (region != null)
                    {
                        y = (int)region.Y;

                        if (region.X >= player.Region.X)
                        {
                            x = (int)(region.X + region.Width);
                            //expanding = Direction.Right;
                        }
                        else
                        {
                            x = (int)region.X - width;
                            //expanding = Direction.Left;
                        }

                        if (region.Y >= player.Region.Y)
                        {
                            listing = Direction.Down;
                        }
                        else
                        {
                            listing = Direction.Up;
                        }
                    }
                }

                #endregion

                GetButton("Cancel").Region = new Region(x, y, width, height);

                int Y = listing == Direction.Up ? y - height : y + height;

                GetButton("Examine").Region = new Region(x, Y, width, height);

                Y = listing == Direction.Up ? Y - height : Y + height;

                if (Handler.Interaction_Tile != null)
                {
                    if (WorldUtil.CanSearch(Handler.Interaction_Tile.Name))
                    {
                        Button search = GetButton("Search");
                        if (search != null)
                        {
                            search.Region = new Region(x, Y, width, height);
                            search.Visible = true;
                        }
                    }
                    else
                    {
                        Button use = GetButton("Use");
                        if (use != null)
                        {
                            use.Region = new Region(x, Y, width, height);
                            use.Visible = true;
                        }
                    }
                }
                else if (Handler.Interaction_Character != null)
                {
                    Button talk = GetButton("Talk");
                    if (talk != null)
                    {
                        talk.Region = new Region(x, Y, width, height);
                        talk.Visible = true;
                    }

                    Y = listing == Direction.Up ? Y - height : Y + height;

                    Button attack = GetButton("Attack");
                    if (attack != null)
                    {
                        attack.Region = new Region(x, Y, width, height);
                        attack.Visible = true;
                    }
                }
            }
        }

        #endregion
    }
}

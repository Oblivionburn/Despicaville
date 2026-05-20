using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OP_Engine.Scenes;
using OP_Engine.Sounds;
using OP_Engine.Characters;
using OP_Engine.Inputs;
using OP_Engine.Tiles;
using OP_Engine.Menus;
using OP_Engine.Utility;
using OP_Engine.Jobs;
using OP_Engine.Controls;
using OP_Engine.Time;
using OP_Engine.Inventories;
using OP_Engine.Rendering;
using OP_Engine.Enums;
using Despicaville.Util;
using Despicaville.Tasks;

namespace Despicaville.Scenes
{
    public class Gameplay : Scene
    {
        #region Variables



        #endregion

        #region Constructor

        public Gameplay(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Gameplay";
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
                    if (TimeManager.Now.Hours >= 5 && TimeManager.Now.Hours <= 21)
                    {
                        AssetManager.PlayMusic_Random("Day", false);
                    }
                    else
                    {
                        AssetManager.PlayMusic_Random("Night", false);
                    }
                }

                if (!TimeManager.Paused)
                {
                    WorldUtil.UpdateWorldMap();

                    if (InputManager.KeyPressed("Map"))
                    {
                        Handler.WorldMap_Visible = !Handler.WorldMap_Visible;
                    }

                    if (!Handler.Player.Dead)
                    {
                        if (!Handler.Player.Moving &&
                            !Handler.Player.Unconscious)
                        {
                            UpdatePlayerControls();
                        }

                        UpdatePlayer();
                    }
                }
            }
        }

        public override void DrawWorld(SpriteBatch spriteBatch, Point resolution, Color color)
        {
            if (Visible)
            {
                if (World.Visible)
                {
                    Army characters = CharacterManager.GetArmy("Characters");

                    List<Tile> trees = new List<Tile>();

                    Map map = World.Maps[0];

                    Layer bottom_tiles = map.GetLayer("BottomTiles");
                    Layer room_tiles = map.GetLayer("RoomTiles");
                    //Layer middle_tiles = map.GetLayer("MiddleTiles");
                    //Layer top_tiles = map.GetLayer("TopTiles");
                    Layer effect_tiles = map.GetLayer("EffectTiles");

                    int bottom_count = bottom_tiles.Tiles.Count;
                    for (int i = 0; i < bottom_count; i++)
                    {
                        Tile bottom_tile = bottom_tiles.Tiles[i];
                        Vector2 location = bottom_tile.Location.ToVector2;

                        bottom_tile.Update(resolution);

                        if (bottom_tile.InView)
                        {
                            if (bottom_tile.InSight)
                            {
                                bottom_tile.Draw(spriteBatch, resolution, color);
                            }
                            else
                            {
                                bottom_tile.Draw(spriteBatch, resolution, color * 0.95f);
                            }

                            if (Main.Game.Debugging)
                            {
                                Tile room_tile = room_tiles.GetTile(location);
                                if (room_tile != null)
                                {
                                    if (room_tile.Texture != null)
                                    {
                                        room_tile.Update(resolution);

                                        if (room_tile.InView)
                                        {
                                            room_tile.Draw(spriteBatch, resolution, color);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    for (int i = 0; i < effect_tiles.Tiles.Count; i++)
                    {
                        Tile effect_tile = effect_tiles.Tiles[i];
                        if (effect_tile.Location.Z == 0)
                        {
                            if (effect_tile.Visible)
                            {
                                if (effect_tile.Texture != null)
                                {
                                    effect_tile.Update(resolution);

                                    if (effect_tile.InView)
                                    {
                                        if (effect_tile.Animated)
                                        {
                                            WorldUtil.Animate_Effect(spriteBatch, effect_tile);
                                        }
                                        else if (effect_tile.InSight)
                                        {
                                            effect_tile.Draw(spriteBatch, resolution, color);
                                        }
                                        else
                                        {
                                            effect_tile.Draw(spriteBatch, resolution, color * 0.95f);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                effect_tiles.Tiles.Remove(effect_tile);
                                i--;
                            }
                        }
                    }

                    int middle_count = Handler.MiddleFurniture.Count;
                    for (int i = 0; i < middle_count; i++)
                    {
                        Tile middle_tile = Handler.MiddleFurniture[i];
                        middle_tile.Update(resolution);

                        if (middle_tile.InView)
                        {
                            if (middle_tile.Name.Contains("Tree"))
                            {
                                trees.Add(middle_tile);
                            }
                            else
                            {
                                if (middle_tile.InSight)
                                {
                                    middle_tile.Draw(spriteBatch, resolution, color);
                                }
                                else
                                {
                                    middle_tile.Draw(spriteBatch, resolution, color * 0.95f);
                                }
                            }
                        }
                    }

                    int top_count = Handler.TopFurniture.Count;
                    for (int i = 0; i < top_count; i++)
                    {
                        Tile top_tile = Handler.TopFurniture[i];
                        top_tile.Update(resolution);

                        if (top_tile.InView)
                        {
                            if (top_tile.InSight)
                            {
                                top_tile.Draw(spriteBatch, resolution, color);
                            }
                            else
                            {
                                top_tile.Draw(spriteBatch, resolution, color * 0.95f);
                            }
                        }
                    }

                    for (float i = -(Main.Game.TileSize.X * 3); i < resolution.Y + (Main.Game.TileSize.X * 3); i += 0.5f)
                    {
                        int squadCount = characters.Squads.Count;
                        for (int s = 0; s < squadCount; s++)
                        {
                            Squad squad = characters.Squads[s];

                            int charCount = squad.Characters.Count;
                            for (int c = 0; c < charCount; c++)
                            {
                                Character character = squad.Characters[c];
                                if (character.Region.Y == i)
                                {
                                    if (character.Type == "Player" ||
                                        character.InSight)
                                    {
                                        if (character.Unconscious ||
                                            character.Laying ||
                                            character.Dead)
                                        {
                                            Texture2D body_texture = AssetManager.Textures["Down_" + character.Texture.Name];
                                            if (body_texture != null)
                                            {
                                                Rectangle image = new Rectangle();

                                                if (character.Direction == Direction.Up)
                                                {
                                                    image = new Rectangle(0, 0, 128, 128);
                                                }
                                                else if (character.Direction == Direction.Right)
                                                {
                                                    image = new Rectangle(128, 0, 128, 128);
                                                }
                                                else if (character.Direction == Direction.Down)
                                                {
                                                    image = new Rectangle(256, 0, 128, 128);
                                                }
                                                else if (character.Direction == Direction.Left)
                                                {
                                                    image = new Rectangle(384, 0, 128, 128);
                                                }

                                                Rectangle region = character.Region.ToRectangle;
                                                spriteBatch.Draw(AssetManager.Textures["Down_" + character.Texture.Name], region, image, Color.White);

                                                foreach (Item item in character.Inventory.Items)
                                                {
                                                    string name = item.Type + "_Down";
                                                    if (AssetManager.Textures.ContainsKey(name))
                                                    {
                                                        Texture2D texture = AssetManager.Textures[name];
                                                        if (texture != null)
                                                        {
                                                            spriteBatch.Draw(texture, region, image, item.DrawColor);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            character.Draw(spriteBatch, resolution, color);

                                            Task task = character.Job.CurrentTask;
                                            task?.TaskBar?.Draw(spriteBatch);
                                        }
                                    }
                                    else if (WorldUtil.InRange(character.Location, Handler.Player.Location, Handler.HearingDistance) &&
                                             character.Moving)
                                    {
                                        Rectangle region = character.Region.ToRectangle;
                                        spriteBatch.Draw(character.Texture, region, character.Image, Color.Black * 0.25f);

                                        foreach (Item item in character.Inventory.Items)
                                        {
                                            if (item.Visible &&
                                                item.Texture != null)
                                            {
                                                spriteBatch.Draw(item.Texture, region, character.Image, Color.Black * 0.25f);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    int trees_count = trees.Count;
                    for (int i = 0; i < trees_count; i++)
                    {
                        Tile tile = trees[i];
                        tile.Update(resolution);

                        if (tile.InView)
                        {
                            if (tile.InSight)
                            {
                                tile.Draw(spriteBatch, resolution, color);
                            }
                            else
                            {
                                tile.Draw(spriteBatch, resolution, color * 0.95f);
                            }
                        }
                    }

                    for (int i = 0; i < effect_tiles.Tiles.Count; i++)
                    {
                        Tile effect_tile = effect_tiles.Tiles[i];
                        if (effect_tile.Location.Z == 1)
                        {
                            if (effect_tile.Visible)
                            {
                                if (effect_tile.Texture != null)
                                {
                                    effect_tile.Update(resolution);

                                    if (effect_tile.InView)
                                    {
                                        if (effect_tile.Animated)
                                        {
                                            WorldUtil.Animate_Effect(spriteBatch, effect_tile);
                                        }
                                        else if (effect_tile.InSight)
                                        {
                                            effect_tile.Draw(spriteBatch, resolution, color);
                                        }
                                        else
                                        {
                                            effect_tile.Draw(spriteBatch, resolution, color * 0.95f);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                effect_tiles.Tiles.Remove(effect_tile);
                                i--;
                            }
                        }
                    }
                }
            }
        }

        public override void DrawWorld(SpriteBatch spriteBatch, Point resolution)
        {
            if (Visible)
            {
                if (World.Maps.Count > 0)
                {
                    Map map = World.Maps[0];
                    Layer roof_tiles = map.GetLayer("RoofTiles");

                    if (!Main.Game.Debugging)
                    {
                        int roof_count = roof_tiles.Tiles.Count;
                        for (int i = 0; i < roof_count; i++)
                        {
                            Tile tile = roof_tiles.Tiles[i];
                            tile.Update(resolution);

                            if (tile.InView)
                            {
                                if (!tile.InSight)
                                {
                                    tile.Draw(spriteBatch, resolution, RenderingManager.Lighting.DrawColor);
                                }
                            }
                        }
                    }
                }

                List<Tile> visible = new List<Tile>();

                if (Handler.VisibleTiles.ContainsKey(Handler.Player.ID))
                {
                    visible = Handler.VisibleTiles[Handler.Player.ID];
                }

                foreach (Tile tile in visible)
                {
                    tile.Draw(spriteBatch, resolution);
                }

                if (Handler.WorldMap_Visible)
                {
                    foreach (Tile tile in WorldGen.Worldmap)
                    {
                        tile.Draw(spriteBatch, resolution, Color.White);
                    }

                    Handler.Player.Draw(spriteBatch, resolution, Color.White);
                }
            }
        }

        private void UpdatePlayerControls()
        {
            bool mouse_in_view = false;

            Menu ui = MenuManager.GetMenu("UI");
            Picture world_view = ui.GetPicture("Panel_Upper_Center");
            if (InputManager.MouseWithin(world_view.Region.ToRectangle))
            {
                mouse_in_view = true;
            }

            if (Handler.Player.Job.Tasks.Count == 0)
            {
                #region Holding

                if (!Handler.Combat)
                {
                    if (InputManager.Mouse_RB_Held)
                    {
                        if (!Handler.Holding &&
                            InventoryUtil.HasEmptyHand(Handler.Player))
                        {
                            Location location = new Location();
                            if (Handler.Player.Direction == Direction.Up)
                            {
                                location = new Location(Handler.Player.Location.X, Handler.Player.Location.Y - 1, 0);
                            }
                            else if (Handler.Player.Direction == Direction.Right)
                            {
                                location = new Location(Handler.Player.Location.X + 1, Handler.Player.Location.Y, 0);
                            }
                            else if (Handler.Player.Direction == Direction.Down)
                            {
                                location = new Location(Handler.Player.Location.X, Handler.Player.Location.Y + 1, 0);
                            }
                            else if (Handler.Player.Direction == Direction.Left)
                            {
                                location = new Location(Handler.Player.Location.X - 1, Handler.Player.Location.Y, 0);
                            }

                            bool holding = false;

                            Army army = CharacterManager.GetArmy("Characters");
                            Squad citizens = army.GetSquad("Citizens");

                            Character character = WorldUtil.GetCharacter(citizens.Characters, location);
                            if (character != null)
                            {
                                Map map = World.Maps[0];
                                Layer bottom_tiles = map.GetLayer("BottomTiles");
                                Tile tile = bottom_tiles.GetTile(character.Location.ToVector2);
                                character.Region = new Region(tile.Region.X, tile.Region.Y, tile.Region.Width, tile.Region.Height);

                                CharacterUtil.UpdateGear(character);

                                Handler.Holding = true;
                                Handler.Holding_ID = character.ID;
                                Handler.Holding_Character = character;

                                Tasker.AbortTask(character);
                                character.Moved = 0;
                                character.Moving = false;

                                Label label = ui.GetLabel("Holding");
                                label.Opacity = 1;
                                label.TextColor = Color.Lime;
                            }

                            if (!holding)
                            {
                                Tile middle_tile = WorldUtil.GetFurniture_Movable(Handler.MiddleFurniture, location);
                                if (middle_tile != null)
                                {
                                    if (middle_tile.CanMove)
                                    {
                                        Handler.Holding = true;
                                        Handler.Holding_ID = middle_tile.ID;
                                        Handler.Holding_Tile = middle_tile;

                                        Label label = ui.GetLabel("Holding");
                                        label.Opacity = 1;
                                        label.TextColor = Color.Red;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Handler.Holding)
                        {
                            Handler.Holding = false;
                            Handler.Holding_ID = 0;
                            Handler.Holding_Tile = null;
                            Handler.Holding_Character = null;

                            Label label = ui.GetLabel("Holding");
                            label.Opacity = 0.6f;
                            label.TextColor = Color.White;
                        }
                    }
                }
                else if (Handler.Holding &&
                         !Handler.Player.Moving)
                {
                    Handler.Holding = false;
                    Handler.Holding_ID = 0;
                    Handler.Holding_Tile = null;
                    Handler.Holding_Character = null;

                    Label label = ui.GetLabel("Holding");
                    label.Opacity = 0.6f;
                    label.TextColor = Color.White;
                }

                #endregion

                #region Run

                if (InputManager.KeyDown("Run"))
                {
                    if (!Handler.Player.Running &&
                        !Handler.Holding)
                    {
                        Handler.Player.Running = true;

                        Label label = ui.GetLabel("Running");
                        label.Opacity = 1;
                        label.TextColor = Color.Red;
                    }
                }
                else
                {
                    if (Handler.Player.Running)
                    {
                        Handler.Player.Running = false;

                        Label label = ui.GetLabel("Running");
                        label.Opacity = 0.6f;
                        label.TextColor = Color.White;
                    }
                }

                #endregion

                #region Crouch

                if (InputManager.KeyDown("Crouch"))
                {
                    if (!Handler.Player.Crouching)
                    {
                        Handler.Player.Crouching = true;

                        Label label = ui.GetLabel("Crouching");
                        label.Opacity = 1;
                        label.TextColor = Color.Red;
                    }
                }
                else
                {
                    if (Handler.Player.Crouching)
                    {
                        Handler.Player.Crouching = false;

                        Label label = ui.GetLabel("Crouching");
                        label.Opacity = 0.6f;
                        label.TextColor = Color.White;
                    }
                }

                #endregion

                #region Move

                if (InputManager.KeyDown("Up"))
                {
                    if (Handler.Player.Running)
                    {
                        Handler.Player.Job.Tasks.Add(new Move
                        {
                            Name = "Run",
                            OwnerID = Handler.Player.ID,
                            StartTime = new TimeHandler(TimeManager.Now),
                            Direction = Direction.Up
                        });
                    }
                    else if (Handler.Player.Crouching)
                    {
                        Handler.Player.Job.Tasks.Add(new Move
                        {
                            Name = "Sneak",
                            OwnerID = Handler.Player.ID,
                            StartTime = new TimeHandler(TimeManager.Now),
                            Direction = Direction.Up
                        });
                    }
                    else
                    {
                        Handler.Player.Job.Tasks.Add(new Move
                        {
                            Name = "Walk",
                            OwnerID = Handler.Player.ID,
                            StartTime = new TimeHandler(TimeManager.Now),
                            Direction = Direction.Up
                        });
                    }
                }
                else if (InputManager.KeyDown("Right"))
                {
                    if (Handler.Player.Running)
                    {
                        Handler.Player.Job.Tasks.Add(new Move
                        {
                            Name = "Run",
                            OwnerID = Handler.Player.ID,
                            StartTime = new TimeHandler(TimeManager.Now),
                            Direction = Direction.Right
                        });
                    }
                    else if (Handler.Player.Crouching)
                    {
                        Handler.Player.Job.Tasks.Add(new Move
                        {
                            Name = "Sneak",
                            OwnerID = Handler.Player.ID,
                            StartTime = new TimeHandler(TimeManager.Now),
                            Direction = Direction.Right
                        });
                    }
                    else
                    {
                        Handler.Player.Job.Tasks.Add(new Move
                        {
                            Name = "Walk",
                            OwnerID = Handler.Player.ID,
                            StartTime = new TimeHandler(TimeManager.Now),
                            Direction = Direction.Right
                        });
                    }
                }
                else if (InputManager.KeyDown("Down"))
                {
                    if (Handler.Player.Running)
                    {
                        Handler.Player.Job.Tasks.Add(new Move
                        {
                            Name = "Run",
                            OwnerID = Handler.Player.ID,
                            StartTime = new TimeHandler(TimeManager.Now),
                            Direction = Direction.Down
                        });
                    }
                    else if (Handler.Player.Crouching)
                    {
                        Handler.Player.Job.Tasks.Add(new Move
                        {
                            Name = "Sneak",
                            OwnerID = Handler.Player.ID,
                            StartTime = new TimeHandler(TimeManager.Now),
                            Direction = Direction.Down
                        });
                    }
                    else
                    {
                        Handler.Player.Job.Tasks.Add(new Move
                        {
                            Name = "Walk",
                            OwnerID = Handler.Player.ID,
                            StartTime = new TimeHandler(TimeManager.Now),
                            Direction = Direction.Down
                        });
                    }
                }
                else if (InputManager.KeyDown("Left"))
                {
                    if (Handler.Player.Running)
                    {
                        Handler.Player.Job.Tasks.Add(new Move
                        {
                            Name = "Run",
                            OwnerID = Handler.Player.ID,
                            StartTime = new TimeHandler(TimeManager.Now),
                            Direction = Direction.Left
                        });
                    }
                    else if (Handler.Player.Crouching)
                    {
                        Handler.Player.Job.Tasks.Add(new Move
                        {
                            Name = "Sneak",
                            OwnerID = Handler.Player.ID,
                            StartTime = new TimeHandler(TimeManager.Now),
                            Direction = Direction.Left
                        });
                    }
                    else
                    {
                        Handler.Player.Job.Tasks.Add(new Move
                        {
                            Name = "Walk",
                            OwnerID = Handler.Player.ID,
                            StartTime = new TimeHandler(TimeManager.Now),
                            Direction = Direction.Left
                        });
                    }
                }

                #endregion

                if (mouse_in_view)
                {
                    if (InputManager.Mouse.LB_Pressed)
                    {
                        #region Turn

                        bool turning = false;

                        Direction direction = Direction.Nowhere;

                        int x_diff = (int)Math.Abs(InputManager.Mouse.X - (Handler.Player.Region.X + (Handler.Player.Region.Width / 2)));
                        int y_diff = (int)Math.Abs(InputManager.Mouse.Y - (Handler.Player.Region.Y + (Handler.Player.Region.Height / 2)));

                        if (x_diff > y_diff)
                        {
                            if (InputManager.Mouse.X <= Handler.Player.Region.X + (Handler.Player.Region.Width / 2))
                            {
                                if (Handler.Player.Direction != Direction.Left)
                                {
                                    direction = Direction.Left;
                                }
                            }
                            else if (InputManager.Mouse.X > Handler.Player.Region.X + (Handler.Player.Region.Width / 2))
                            {
                                if (Handler.Player.Direction != Direction.Right)
                                {
                                    direction = Direction.Right;
                                }
                            }
                        }
                        else
                        {
                            if (InputManager.Mouse.Y <= Handler.Player.Region.Y + (Handler.Player.Region.Height / 2))
                            {
                                if (Handler.Player.Direction != Direction.Up)
                                {
                                    direction = Direction.Up;
                                }
                            }
                            else if (InputManager.Mouse.Y > Handler.Player.Region.Y + (Handler.Player.Region.Height / 2))
                            {
                                if (Handler.Player.Direction != Direction.Down)
                                {
                                    direction = Direction.Down;
                                }
                            }
                        }

                        if (direction != Direction.Nowhere)
                        {
                            Handler.Player.Job.Tasks.Add(new Turn
                            {
                                Name = "Turn",
                                OwnerID = Handler.Player.ID,
                                StartTime = new TimeHandler(TimeManager.Now),
                                EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(CharacterUtil.GetTurnTime(Handler.Player))),
                                Direction = direction
                            });

                            turning = true;
                        }

                        #endregion

                        if (!turning)
                        {
                            if (Handler.Combat)
                            {
                                #region Attack

                                Location location = new Location();

                                if (CombatUtil.CanAttack_Ranged(Handler.Player))
                                {
                                    List<Tile> visible = Handler.VisibleTiles[Handler.Player.ID];

                                    int count = visible.Count;
                                    for (int i = 0; i < count; i++)
                                    {
                                        Tile tile = visible[i];
                                        if (tile.Visible)
                                        {
                                            location = new Location(tile.Location.X, tile.Location.Y, 1);
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    if (Handler.Player.Direction == Direction.Up)
                                    {
                                        location = new Location(Handler.Player.Location.X, Handler.Player.Location.Y - 1, 1);
                                    }
                                    else if (Handler.Player.Direction == Direction.Right)
                                    {
                                        location = new Location(Handler.Player.Location.X + 1, Handler.Player.Location.Y, 1);
                                    }
                                    else if (Handler.Player.Direction == Direction.Down)
                                    {
                                        location = new Location(Handler.Player.Location.X, Handler.Player.Location.Y + 1, 1);
                                    }
                                    else if (Handler.Player.Direction == Direction.Left)
                                    {
                                        location = new Location(Handler.Player.Location.X - 1, Handler.Player.Location.Y, 1);
                                    }
                                }

                                Dictionary<string, string> AttackingWith = CombatUtil.AttackChoice(Handler.Player);
                                string action = AttackingWith.ElementAt(0).Value;

                                int attackTime = CombatUtil.AttackTime(Handler.Player, action);
                                Handler.Player.Job.Tasks.Add(new Attack
                                {
                                    Name = "Attack",
                                    OwnerID = Handler.Player.ID,
                                    Location = location,
                                    Direction = Handler.Player.Direction,
                                    StartTime = new TimeHandler(TimeManager.Now),
                                    EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(attackTime)),
                                    TaskBar = CharacterUtil.GenTaskbar(Handler.Player, attackTime)
                                });

                                #endregion
                            }
                            else
                            {
                                #region Interact

                                Location location = null;

                                List<Tile> visible = Handler.VisibleTiles[Handler.Player.ID];

                                int count = visible.Count;
                                for (int i = 0; i < count; i++)
                                {
                                    Tile tile = visible[i];
                                    if (tile.Visible)
                                    {
                                        location = tile.Location;
                                        break;
                                    }
                                }

                                if (location != null &&
                                    location.X == -1 &&
                                    location.Y == -1)
                                {
                                    if (Handler.Player.Direction == Direction.Up)
                                    {
                                        location = new Location(Handler.Player.Location.X, Handler.Player.Location.Y - 1);
                                    }
                                    else if (Handler.Player.Direction == Direction.Right)
                                    {
                                        location = new Location(Handler.Player.Location.X + 1, Handler.Player.Location.Y);
                                    }
                                    else if (Handler.Player.Direction == Direction.Down)
                                    {
                                        location = new Location(Handler.Player.Location.X, Handler.Player.Location.Y + 1);
                                    }
                                    else if (Handler.Player.Direction == Direction.Left)
                                    {
                                        location = new Location(Handler.Player.Location.X - 1, Handler.Player.Location.Y);
                                    }
                                }

                                Character target = WorldUtil.GetCharacter(location);
                                if (target != null)
                                {
                                    WorldUtil.GenDescription(target);
                                }
                                else
                                {
                                    Tile interaction_tile = null;

                                    Map map = World.Maps[0];

                                    Layer top_tiles = map.GetLayer("TopTiles");
                                    Tile top_tile = top_tiles.GetTile(location.ToVector2);
                                    if (top_tile?.Texture != null)
                                    {
                                        interaction_tile = top_tile;
                                    }

                                    if (interaction_tile == null)
                                    {
                                        Tile middle_tile = WorldUtil.GetFurniture(Handler.MiddleFurniture, new Location(location.X, location.Y, 0));
                                        if (middle_tile?.Texture != null)
                                        {
                                            interaction_tile = middle_tile;
                                        }
                                    }

                                    if (interaction_tile == null)
                                    {
                                        Layer bottom_tiles = map.GetLayer("BottomTiles");
                                        Tile bottom_tile = bottom_tiles.GetTile(location.ToVector2);
                                        if (bottom_tile != null)
                                        {
                                            interaction_tile = bottom_tile;
                                        }
                                    }

                                    if (interaction_tile != null)
                                    {
                                        if (WorldUtil.NextTo(location, Handler.Player.Location))
                                        {
                                            Tasker.Interact(interaction_tile);
                                        }
                                        else
                                        {
                                            WorldUtil.GenDescription(interaction_tile);
                                        }
                                    }                                        
                                }

                                #endregion
                            }
                        }
                    }
                    else if (Handler.Combat &&
                             InputManager.Mouse.RB_Pressed)
                    {
                        #region Get Ranged Target

                        Character target = null;

                        if (CombatUtil.CanAttack_Ranged(Handler.Player))
                        {
                            List<Tile> visible = Handler.VisibleTiles[Handler.Player.ID];

                            int count = visible.Count;
                            for (int i = 0; i < count; i++)
                            {
                                Tile tile = visible[i];
                                if (tile.Visible)
                                {
                                    target = WorldUtil.GetCharacter(tile.Location);
                                    break;
                                }
                            }
                        }

                        #endregion

                        if (target != null &&
                            !WorldUtil.NextTo(target.Location, Handler.Player.Location))
                        {
                            #region Aimed Attack

                            Handler.Interaction_Character = target;
                            MenuManager.GetMenu("Combat").Open();

                            #endregion
                        }
                        else
                        {
                            #region Push

                            Location location = null;

                            if (Handler.Player.Direction == Direction.Up)
                            {
                                location = new Location(Handler.Player.Location.X, Handler.Player.Location.Y - 1, 1);
                            }
                            else if (Handler.Player.Direction == Direction.Right)
                            {
                                location = new Location(Handler.Player.Location.X + 1, Handler.Player.Location.Y, 1);
                            }
                            else if (Handler.Player.Direction == Direction.Down)
                            {
                                location = new Location(Handler.Player.Location.X, Handler.Player.Location.Y + 1, 1);
                            }
                            else if (Handler.Player.Direction == Direction.Left)
                            {
                                location = new Location(Handler.Player.Location.X - 1, Handler.Player.Location.Y, 1);
                            }

                            Handler.Player.Job.Tasks.Add(new Push
                            {
                                Name = "Push",
                                OwnerID = Handler.Player.ID,
                                StartTime = new TimeHandler(TimeManager.Now),
                                Location = location,
                                Direction = Handler.Player.Direction
                            });

                            #endregion
                        }
                    }
                }

                #region Wait

                if (InputManager.KeyDown("Wait"))
                {
                    long time = Handler.ActionRate;

                    if (Handler.Player.Running)
                    {
                        time = Handler.ActionRate * 5;
                    }
                    else if (Handler.Player.Crouching)
                    {
                        time = Handler.ActionRate / 5;
                    }

                    Handler.Player.Job.Tasks.Add(new Wait
                    {
                        Name = "Wait",
                        OwnerID = Handler.Player.ID,
                        StartTime = new TimeHandler(TimeManager.Now),
                        EndTime = new TimeHandler(TimeManager.Now, TimeSpan.FromMilliseconds(time))
                    });

                    TimeTracker.Tick(time);
                }

                #endregion

                #region Combat

                if (InputManager.KeyPressed("Combat"))
                {
                    InputManager.Keyboard.Flush();
                    Handler.Combat = !Handler.Combat;

                    Label label = ui.GetLabel("Combat");

                    if (Handler.Combat)
                    {
                        label.Opacity = 1;
                        label.TextColor = Color.Red;
                    }
                    else
                    {
                        label.Opacity = 0.6f;
                        label.TextColor = Color.White;
                    }
                }

                #endregion

                #region MainMenu

                if (InputManager.KeyPressed("Cancel"))
                {
                    InputManager.Keyboard.Flush();
                    MenuManager.GetMenu("Main").Open();
                }

                #endregion

                #region Inventory

                if (InputManager.KeyPressed("Inventory"))
                {
                    InputManager.Keyboard.Flush();

                    Handler.Trading = false;
                    Handler.Trading_InventoryID.Clear();

                    MenuManager.GetMenu("Inventory").Open();
                }

                #endregion

                #region Debug

                if (InputManager.KeyPressed("Debug"))
                {
                    InputManager.Keyboard.Flush();
                    Main.Game.Debugging = !Main.Game.Debugging;
                }

                #endregion
            }
        }

        private void UpdatePlayer()
        {
            if (Handler.Player != null)
            {
                if (Handler.Player.Stats.Blood <= 0)
                {
                    Handler.Player.Dead = true;
                    //Game Over
                }

                if (!Handler.Player.Dead)
                {
                    if (Handler.Player.Unconscious)
                    {
                        TimeTracker.Tick(Handler.ActionRate * 5);
                        CharacterUtil.Sleep(Handler.Player);
                    }
                    else
                    {
                        Task task = Handler.Player.Job.CurrentTask;
                        if (task != null)
                        {
                            if (task.Name == "Sneak" ||
                                task.Name == "Walk" ||
                                task.Name == "Run" ||
                                task.Name == "Push")
                            {
                                Handler.Player.Job.Update(TimeManager.Now);
                            }
                            else
                            {
                                if (!task.Completed)
                                {
                                    long milliseconds = task.EndTime.TotalMilliseconds - TimeManager.Now.TotalMilliseconds;

                                    if (task.Name == "Attack")
                                    {
                                        if (milliseconds > 60000)
                                        {
                                            //1m
                                            TimeTracker.Tick(60000);
                                        }
                                        else if (milliseconds > 10000)
                                        {
                                            //10s
                                            TimeTracker.Tick(10000);
                                        }
                                        else if (milliseconds > 1000)
                                        {
                                            //1s
                                            TimeTracker.Tick(1000);
                                        }
                                        else if (milliseconds > 100)
                                        {
                                            //100ms
                                            TimeTracker.Tick(100);
                                        }
                                        else if (milliseconds > 10)
                                        {
                                            //10ms
                                            TimeTracker.Tick(10);
                                        }
                                        else if (milliseconds > 0)
                                        {
                                            //1ms
                                            TimeTracker.Tick(milliseconds);
                                        }
                                    }
                                    else
                                    {
                                        if (milliseconds >= 60000)
                                        {
                                            //1m
                                            TimeTracker.Tick(60000);
                                        }
                                        else if (milliseconds >= 10000)
                                        {
                                            //10s
                                            TimeTracker.Tick(10000);
                                        }
                                        else if (milliseconds >= 1000)
                                        {
                                            //1s
                                            TimeTracker.Tick(1000);
                                        }
                                        else if (milliseconds >= 100)
                                        {
                                            //100ms
                                            TimeTracker.Tick(100);
                                        }
                                        else if (milliseconds >= 10)
                                        {
                                            //10ms
                                            TimeTracker.Tick(10);
                                        }
                                        else if (milliseconds > 0)
                                        {
                                            //1ms
                                            TimeTracker.Tick(milliseconds);
                                        }
                                        else
                                        {
                                            Handler.Player.Job.Update(TimeManager.Now);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    WorldUtil.UpdateWorld(World);
                    GameUtil.CenterToPlayer_OnFrame();
                }
            }
        }

        public override void Load(ContentManager content)
        {
            
        }

        public override void Resize(Point point)
        {
            GameUtil.CenterToPlayer_OnStart();
        }

        #endregion
    }
}

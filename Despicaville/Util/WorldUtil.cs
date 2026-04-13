using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OP_Engine.Characters;
using OP_Engine.Inventories;
using OP_Engine.Tiles;
using OP_Engine.Utility;
using OP_Engine.Scenes;
using OP_Engine.Enums;
using OP_Engine.Time;

namespace Despicaville.Util
{
    public static class WorldUtil
    {
        public static Map GetMap()
        {
            Scene scene = SceneManager.GetScene("Gameplay");
            return scene.World.Maps[0];
        }

        public static bool CanMove(Character character, Map map, Location destination)
        {
            Layer bottom_tiles = map.GetLayer("BottomTiles");

            //Check bottom tiles
            if (destination.X < bottom_tiles.Columns && destination.X >= 0 &&
                destination.Y < bottom_tiles.Rows && destination.Y >= 0)
            {
                Tile current = bottom_tiles.GetTile(destination.ToVector2);
                if (current != null)
                {
                    if (current.BlocksMovement)
                    {
                        return false;
                    }
                }
            }
            else if (character.Type == "Player" ||
                     character.Type == "Citizen")
            {
                return false;
            }

            //Check middle tiles
            Tile furniture = GetFurniture(Handler.MiddleFurniture, destination);
            if (furniture != null)
            {
                if (!furniture.Name.Contains("Open"))
                {
                    if (Handler.Holding_ID != furniture.ID &&
                        furniture.BlocksMovement)
                    {
                        return false;
                    }
                    else if (furniture.Name.Contains("Window") &&
                             character.Type != "Player")
                    {
                        return false;
                    }
                }
            }

            //Check other characters
            Character other = GetCharacter(destination);
            if (other != null)
            {
                return false;
            }

            if (Handler.Holding)
            {
                Location newLocation = null;
                Region size = null;

                //Check for held thing colliding when pushed
                if (Handler.Holding_Character != null)
                {
                    size = new Region(Handler.Holding_Character.Location.X, Handler.Holding_Character.Location.Y, 0, 0);
                    newLocation = new Location(Handler.Holding_Character.Location.X, Handler.Holding_Character.Location.Y, 0);

                    Direction direction = GetDirection(character.Destination, character.Location, false);
                    if (direction == Direction.Up)
                    {
                        if (Handler.Holding_Character.Location.X == character.Location.X)
                        {
                            newLocation.Y--;
                        }
                        else if (Handler.Holding_Character.Location.X < character.Location.X)
                        {
                            newLocation.X++;
                        }
                        else if (Handler.Holding_Character.Location.X > character.Location.X)
                        {
                            newLocation.X--;
                        }
                    }
                    else if (direction == Direction.Right)
                    {
                        if (Handler.Holding_Character.Location.Y == character.Location.Y)
                        {
                            newLocation.X++;
                        }
                        else if (Handler.Holding_Character.Location.Y < character.Location.Y)
                        {
                            newLocation.Y++;
                        }
                        else if (Handler.Holding_Character.Location.Y > character.Location.Y)
                        {
                            newLocation.Y--;
                        }
                    }
                    else if (direction == Direction.Down)
                    {
                        if (Handler.Holding_Character.Location.X == character.Location.X)
                        {
                            newLocation.Y++;
                        }
                        else if (Handler.Holding_Character.Location.X < character.Location.X)
                        {
                            newLocation.X++;
                        }
                        else if (Handler.Holding_Character.Location.X > character.Location.X)
                        {
                            newLocation.X--;
                        }
                    }
                    else if (direction == Direction.Left)
                    {
                        if (Handler.Holding_Character.Location.Y == character.Location.Y)
                        {
                            newLocation.X--;
                        }
                        else if (Handler.Holding_Character.Location.Y < character.Location.Y)
                        {
                            newLocation.Y++;
                        }
                        else if (Handler.Holding_Character.Location.Y > character.Location.Y)
                        {
                            newLocation.Y--;
                        }
                    }
                }
                else if (Handler.Holding_Tile != null)
                {
                    size = GetSize(Handler.Holding_Tile);
                    newLocation = new Location(Handler.Holding_Tile.Location.X, Handler.Holding_Tile.Location.Y, 0);

                    Direction direction = GetDirection(character.Destination, character.Location, false);
                    if (direction == Direction.Up)
                    {
                        if (Handler.Holding_Tile.Location.X == character.Location.X)
                        {
                            newLocation.Y--;
                        }
                        else if (Handler.Holding_Tile.Location.X < character.Location.X)
                        {
                            newLocation.X++;
                        }
                        else if (Handler.Holding_Tile.Location.X > character.Location.X)
                        {
                            newLocation.X--;
                        }
                    }
                    else if (direction == Direction.Right)
                    {
                        if (Handler.Holding_Tile.Location.Y == character.Location.Y)
                        {
                            newLocation.X++;
                        }
                        else if (Handler.Holding_Tile.Location.Y < character.Location.Y)
                        {
                            newLocation.Y++;
                        }
                        else if (Handler.Holding_Tile.Location.Y > character.Location.Y)
                        {
                            newLocation.Y--;
                        }
                    }
                    else if (direction == Direction.Down)
                    {
                        if (Handler.Holding_Tile.Location.X == character.Location.X)
                        {
                            newLocation.Y++;
                        }
                        else if (Handler.Holding_Tile.Location.X < character.Location.X)
                        {
                            newLocation.X++;
                        }
                        else if (Handler.Holding_Tile.Location.X > character.Location.X)
                        {
                            newLocation.X--;
                        }
                    }
                    else if (direction == Direction.Left)
                    {
                        if (Handler.Holding_Tile.Location.Y == character.Location.Y)
                        {
                            newLocation.X--;
                        }
                        else if (Handler.Holding_Tile.Location.Y < character.Location.Y)
                        {
                            newLocation.Y++;
                        }
                        else if (Handler.Holding_Tile.Location.Y > character.Location.Y)
                        {
                            newLocation.Y--;
                        }
                    }
                }
                
                if (size != null &&
                    newLocation != null)
                {
                    for (int y = 0; y <= size.Height; y++)
                    {
                        float Y = newLocation.Y + y;

                        for (int x = 0; x <= size.Width; x++)
                        {
                            float X = newLocation.X + x;

                            Tile bottom = bottom_tiles.GetTile(new Vector2(X, Y));
                            if (bottom != null)
                            {
                                if (bottom.BlocksMovement)
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                return false;
                            }

                            Tile middle = GetFurniture(Handler.MiddleFurniture, new Location(X, Y, 0));
                            if (middle != null &&
                                middle.ID != Handler.Holding_ID &&
                                !middle.Name.Contains("Open") &&
                                !middle.Name.Contains("Broken"))
                            {
                                if (Handler.Holding_Character != null)
                                {
                                    if (middle.BlocksMovement)
                                    {
                                        return false;
                                    }
                                }
                                else if (Handler.Holding_Tile != null)
                                {
                                    return false;
                                }
                            }

                            other = GetCharacter(new Location(X, Y, 0));
                            if (other != null &&
                                other.ID != character.ID)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        public static bool Blocked(Map map, Location location, bool forTile)
        {
            Layer bottom_tiles = map.GetLayer("BottomTiles");

            //Check bottom tiles
            if (location.X < bottom_tiles.Columns && location.X >= 0 &&
                location.Y < bottom_tiles.Rows && location.Y >= 0)
            {
                Tile current = bottom_tiles.GetTile(location.ToVector2);
                if (current != null)
                {
                    if (current.BlocksMovement)
                    {
                        return true;
                    }
                }
            }

            //Check for furniture
            Tile furniture = GetFurniture(Handler.MiddleFurniture, location);
            if (furniture != null)
            {
                if (!furniture.Name.Contains("Open") &&
                    !furniture.Name.Contains("Broken"))
                {
                    if (forTile)
                    {
                        return true;
                    }
                    else if (furniture.BlocksMovement)
                    {
                        return true;
                    }
                }
            }

            //Check for characters
            Character character = GetCharacter(location);
            if (character != null)
            {
                return true;
            }

            return false;
        }

        public static bool InRange(Location location, Location source, int distance)
        {
            float x_diff = location.X - source.X;
            if (x_diff < 0)
            {
                x_diff *= -1;
            }

            float y_diff = location.Y - source.Y;
            if (y_diff < 0)
            {
                y_diff *= -1;
            }

            float total_distance = x_diff + y_diff;

            if (total_distance <= distance)
            {
                return true;
            }

            return false;
        }

        public static Direction GetDirection(Location target, Location source, bool compass_directions)
        {
            if (compass_directions)
            {
                if (target.X > source.X)
                {
                    if (target.Y > source.Y)
                    {
                        return Direction.SouthEast;
                    }
                    else if (target.Y < source.Y)
                    {
                        return Direction.NorthEast;
                    }
                    else if (target.Y == source.Y)
                    {
                        return Direction.East;
                    }
                }
                else if (target.X < source.X)
                {
                    if (target.Y > source.Y)
                    {
                        return Direction.SouthWest;
                    }
                    else if (target.Y < source.Y)
                    {
                        return Direction.NorthWest;
                    }
                    else if (target.Y == source.Y)
                    {
                        return Direction.West;
                    }
                }
                else if (target.X == source.X)
                {
                    if (target.Y > source.Y)
                    {
                        return Direction.South;
                    }
                    else if (target.Y < source.Y)
                    {
                        return Direction.North;
                    }
                }
            }
            else
            {
                if (target.X > source.X)
                {
                    return Direction.Right;
                }
                else if (target.X < source.X)
                {
                    return Direction.Left;
                }
                else if (target.X == source.X)
                {
                    if (target.Y > source.Y)
                    {
                        return Direction.Down;
                    }
                    else if (target.Y < source.Y)
                    {
                        return Direction.Up;
                    }
                }
            }

            return Direction.Nowhere;
        }

        public static int GetDistance(Location origin, Location location)
        {
            int x_diff = (int)origin.X - (int)location.X;
            if (x_diff < 0)
            {
                x_diff *= -1;
            }

            int y_diff = (int)origin.Y - (int)location.Y;
            if (y_diff < 0)
            {
                y_diff *= -1;
            }

            return x_diff + y_diff;
        }

        public static bool Location_IsVisible(long character_id, Location location)
        {
            if (location != null)
            {
                List<Tile> tiles = new List<Tile>();
                if (Handler.VisibleTiles.ContainsKey(character_id))
                {
                    tiles = Handler.VisibleTiles[character_id];
                }

                for (int i = 0; i < tiles.Count; i++)
                {
                    Location tile_location = tiles[i].Location;
                    if (tile_location.X == location.X &&
                        tile_location.Y == location.Y)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool BlocksSight(string name)
        {
            int count = Handler.SeeThrough.Length;
            for (int i = 0; i < count; i++)
            {
                string furniture = Handler.SeeThrough[i];
                if (name.Contains(furniture))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CanSearch(string name)
        {
            for (int i = 0; i < Handler.Searchable.Length; i++)
            {
                if (name.Contains(Handler.Searchable[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public static void PullHeld_Tile(Character character, Direction direction, bool behind)
        {
            if (Handler.Holding_Tile != null)
            {
                Location location = new Location(Handler.Holding_Tile.Location.X, Handler.Holding_Tile.Location.Y, 0);
                if (direction == Direction.North)
                {
                    if (Handler.Holding_Tile.IsLightSource)
                    {
                        for (int l = 0; l < Handler.light_sources.Count; l++)
                        {
                            Point source = Handler.light_sources[l];
                            if (source.X == location.X &&
                                source.Y == location.Y)
                            {
                                source.X = (int)character.Location.X;

                                if (behind)
                                {
                                    source.Y = (int)character.Location.Y + 1;
                                }
                                else
                                {
                                    source.Y = (int)character.Location.Y - 1;
                                }

                                Handler.light_sources[l] = new Point(source.X, source.Y);
                                break;
                            }
                        }
                    }

                    location.X = (int)character.Location.X;

                    if (behind)
                    {
                        location.Y = (int)character.Location.Y + 1;
                    }
                    else
                    {
                        location.Y = (int)character.Location.Y - 1;
                    }
                }
                else if (direction == Direction.East)
                {
                    if (Handler.Holding_Tile.IsLightSource)
                    {
                        for (int l = 0; l < Handler.light_sources.Count; l++)
                        {
                            Point source = Handler.light_sources[l];
                            if (source.X == location.X &&
                                source.Y == location.Y)
                            {
                                source.Y = (int)character.Location.Y;

                                if (behind)
                                {
                                    source.X = (int)character.Location.X - 1;
                                }
                                else
                                {
                                    source.X = (int)character.Location.X + 1;
                                }

                                Handler.light_sources[l] = new Point(source.X, source.Y);
                                break;
                            }
                        }
                    }

                    location.Y = (int)character.Location.Y;

                    if (behind)
                    {
                        location.X = (int)character.Location.X - 1;
                    }
                    else
                    {
                        location.X = (int)character.Location.X + 1;
                    }
                }
                else if (direction == Direction.South)
                {
                    if (Handler.Holding_Tile.IsLightSource)
                    {
                        for (int l = 0; l < Handler.light_sources.Count; l++)
                        {
                            Point source = Handler.light_sources[l];
                            if (source.X == location.X &&
                                source.Y == location.Y)
                            {
                                source.X = (int)character.Location.X;

                                if (behind)
                                {
                                    source.Y = (int)character.Location.Y - 1;
                                }
                                else
                                {
                                    source.Y = (int)character.Location.Y + 1;
                                }

                                Handler.light_sources[l] = new Point(source.X, source.Y);
                                break;
                            }
                        }
                    }

                    location.X = (int)character.Location.X;

                    if (behind)
                    {
                        location.Y = (int)character.Location.Y - 1;
                    }
                    else
                    {
                        location.Y = (int)character.Location.Y + 1;
                    }
                }
                else if (direction == Direction.West)
                {
                    if (Handler.Holding_Tile.IsLightSource)
                    {
                        for (int l = 0; l < Handler.light_sources.Count; l++)
                        {
                            Point source = Handler.light_sources[l];
                            if (source.X == location.X &&
                                source.Y == location.Y)
                            {
                                source.Y = (int)character.Location.Y;

                                if (behind)
                                {
                                    source.X = (int)character.Location.X + 1;
                                }
                                else
                                {
                                    source.X = (int)character.Location.X - 1;
                                }

                                Handler.light_sources[l] = new Point(source.X, source.Y);
                                break;
                            }
                        }
                    }

                    location.Y = (int)character.Location.Y;

                    if (behind)
                    {
                        location.X = (int)character.Location.X + 1;
                    }
                    else
                    {
                        location.X = (int)character.Location.X - 1;
                    }
                }

                SwapTiles(GetMap().GetLayer("MiddleTiles"), Handler.Holding_Tile, location);
            }
        }

        public static void PullHeld_Character(Character character, Direction direction, bool behind)
        {
            if (Handler.Holding_Character != null)
            {
                if (direction == Direction.North)
                {
                    Handler.Holding_Character.Location.X = (int)character.Location.X;

                    if (behind)
                    {
                        Handler.Holding_Character.Location.Y = (int)character.Location.Y + 1;
                    }
                    else
                    {
                        Handler.Holding_Character.Location.Y = (int)character.Location.Y - 1;
                    }
                }
                else if (direction == Direction.East)
                {
                    Handler.Holding_Character.Location.Y = (int)character.Location.Y;

                    if (behind)
                    {
                        Handler.Holding_Character.Location.X = (int)character.Location.X - 1;
                    }
                    else
                    {
                        Handler.Holding_Character.Location.X = (int)character.Location.X + 1;
                    }
                }
                else if (direction == Direction.South)
                {
                    Handler.Holding_Character.Location.X = (int)character.Location.X;

                    if (behind)
                    {
                        Handler.Holding_Character.Location.Y = (int)character.Location.Y - 1;
                    }
                    else
                    {
                        Handler.Holding_Character.Location.Y = (int)character.Location.Y + 1;
                    }
                }
                else if (direction == Direction.West)
                {
                    Handler.Holding_Character.Location.Y = (int)character.Location.Y;

                    if (behind)
                    {
                        Handler.Holding_Character.Location.X = (int)character.Location.X + 1;
                    }
                    else
                    {
                        Handler.Holding_Character.Location.X = (int)character.Location.X - 1;
                    }
                }

                CharacterUtil.UpdateGear(Handler.Holding_Character);
            }
        }

        public static void Push_Tile(Tile tile, Location newLocation)
        {
            if (tile.IsLightSource)
            {
                for (int l = 0; l < Handler.light_sources.Count; l++)
                {
                    Point source = Handler.light_sources[l];
                    if (source.X == tile.Location.X &&
                        source.Y == tile.Location.Y)
                    {
                        Handler.light_sources[l] = new Point((int)newLocation.X, (int)newLocation.Y);
                        break;
                    }
                }
            }

            SwapTiles(GetMap().GetLayer("MiddleTiles"), tile, newLocation);
        }

        public static void Push_Character(Character character, Location newLocation)
        {
            if (character != null)
            {
                character.Location = new Location(newLocation.X, newLocation.Y, 0);
                CharacterUtil.UpdateGear(character);
            }
        }

        public static void SwapTiles(Layer layer, Tile tile, Location newLocation)
        {
            int oldIndex = ((int)tile.Location.Y * layer.Columns) + (int)tile.Location.X;
            int newIndex = ((int)newLocation.Y * layer.Columns) + (int)newLocation.X;

            Tile temp = layer.Tiles[newIndex];
            layer.Tiles[newIndex] = layer.Tiles[oldIndex];
            layer.Tiles[oldIndex] = temp;

            tile.Location = new Location(newLocation.X, newLocation.Y, 0);
        }

        public static Tile GetClosestTile(List<Tile> tiles, Character character)
        {
            Tile[] tilesArray = tiles.ToArray();
            int count = tilesArray.Length;

            if (count > 0)
            {
                if (count > 1)
                {
                    Tile closest = tilesArray[0];
                    int nearest = GetDistance(character.Location, closest.Location);

                    for (int i = 1; i < count; i++)
                    {
                        Tile tile = tilesArray[i];

                        int distance = GetDistance(character.Location, tile.Location);
                        if (distance < nearest)
                        {
                            nearest = distance;
                            closest = tile;
                        }
                    }

                    return closest;
                }
                else
                {
                    return tilesArray[0];
                }
            }

            return null;
        }

        public static List<Character> GetNearbyCharacters(long ID, Location location)
        {
            List<Character> result = new List<Character>();

            Army army = CharacterManager.Armies[0];

            Squad players = army.Squads[0];
            Character player = players.Characters[0];
            if (player.ID != ID)
            {
                if (NextTo(player.Location, location))
                {
                    result.Add(player);
                }
            }

            Squad citizens = army.Squads[1];
            for (int i = 0; i < citizens.Characters.Count; i++)
            {
                Character existing = citizens.Characters[i];
                if (existing.ID != ID)
                {
                    if (NextTo(existing.Location, location))
                    {
                        result.Add(existing);
                    }
                }
            }

            return result;
        }

        public static Character GetCharacter(Location location)
        {
            if (location != null)
            {
                Map map = GetMap();
                Layer bottom_tiles = map.GetLayer("BottomTiles");

                Tile tile = bottom_tiles.GetTile(location.ToVector2);
                if (tile != null)
                {
                    float center_x = Handler.Player.Region.X + (Handler.Player.Region.Width / 2);
                    float center_y = Handler.Player.Region.Y + (Handler.Player.Region.Height / 2);

                    if (center_x >= tile.Region.X && center_x < tile.Region.X + tile.Region.Width &&
                        center_y >= tile.Region.Y && center_y < tile.Region.Y + tile.Region.Height)
                    {
                        return Handler.Player;
                    }

                    Army army = CharacterManager.GetArmy("Characters");
                    Squad citizens = army.GetSquad("Citizens");

                    Character character = GetCharacter(citizens.Characters, tile);
                    if (character != null)
                    {
                        return character;
                    }
                }
            }

            return null;
        }

        public static Character GetCharacter(List<Character> characters, Location location)
        {
            Map map = GetMap();
            Layer bottom_tiles = map.GetLayer("BottomTiles");

            Tile tile = bottom_tiles.GetTile(location.ToVector2);
            if (tile != null)
            {
                Character character = GetCharacter(characters, tile);
                if (character != null)
                {
                    return character;
                }
            }

            return null;
        }

        public static Character GetCharacter(List<Character> characters, Tile tile)
        {
            int count = characters.Count;
            for (int i = 0; i < count; i++)
            {
                Character character = characters[i];

                float center_x = character.Region.X + (character.Region.Width / 2);
                float center_y = character.Region.Y + (character.Region.Height / 2);

                if (center_x >= tile.Region.X && center_x < tile.Region.X + tile.Region.Width &&
                    center_y >= tile.Region.Y && center_y < tile.Region.Y + tile.Region.Height)
                {
                    return character;
                }
            }

            return null;
        }

        public static Character GetCharacter_Target(Character character)
        {
            Army army = CharacterManager.GetArmy("Characters");
            foreach (Squad squad in army.Squads)
            {
                foreach (Character existing in squad.Characters)
                {
                    if (existing.ID == character.Target_ID)
                    {
                        return existing;
                    }
                }
            }

            return null;
        }

        public static List<Character> GetAllCharacters(Point map_coords)
        {
            List<Character> characters = new List<Character>();

            int world_x = map_coords.X * 20;
            int world_y = map_coords.Y * 20;

            Army army = CharacterManager.GetArmy("Characters");
            Squad squad = army.GetSquad("Citizens");

            foreach (Character character in squad.Characters)
            {
                if (character.Location.X >= world_x && character.Location.X < world_x + 20 &&
                    character.Location.Y >= world_y && character.Location.Y < world_y + 20)
                {
                    characters.Add(character);
                }
            }

            return characters;
        }

        public static Map GetRoom(Character character)
        {
            if (character.Map == null)
            {
                SetCurrentMap(character);
            }

            if (WorldGen.Rooms.ContainsKey(character.Map.ID))
            {
                List<Map> rooms = WorldGen.Rooms[character.Map.ID];

                int mapCount = rooms.Count;
                for (int m = 0; m < mapCount; m++)
                {
                    Map room = rooms[m];

                    int layerCount = room.Layers.Count;
                    for (int l = 0; l < layerCount; l++)
                    {
                        Layer layer = room.Layers[l];
                        if (layer.Name == "Tiles")
                        {
                            int tileCount = layer.Tiles.Count;
                            for (int t = 0; t < tileCount; t++)
                            {
                                Tile tile = layer.Tiles[t];

                                int world_x = (int)((room.Location.X * 20) + tile.Location.X);
                                int world_y = (int)((room.Location.Y * 20) + tile.Location.Y);

                                if (world_x == character.Location.X &&
                                    world_y == character.Location.Y)
                                {
                                    return room;
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        public static Tile GetFurniture(List<Tile> furniture, Location location)
        {
            int width = (int)Main.Game.TileSize_X;
            int width_double = width * 2;
            int width_triple = width * 3;

            float dest_x = location.X;
            float dest_y = location.Y;

            Tile[] tiles = furniture.ToArray();

            int count = tiles.Length;
            for (int i = 0; i < count; i++)
            {
                Tile existing = tiles[i];

                float x = existing.Location.X;
                float y = existing.Location.Y;

                if (dest_x == x &&
                    dest_y == y)
                {
                    return existing;
                }
                else if (existing.Region.Height == width)
                {
                    if (existing.Direction == Direction.Right)
                    {
                        if (existing.Region.Width == width_double)
                        {
                            if (dest_x >= x && dest_x <= x + 1 &&
                                dest_y == y)
                            {
                                return existing;
                            }
                        }
                    }
                    else if (existing.Direction == Direction.Left)
                    {
                        if (existing.Region.Width == width_double)
                        {
                            if (dest_x >= x - 1 && dest_x <= x &&
                                dest_y == y)
                            {
                                return existing;
                            }
                        }
                    }
                    else if (existing.Direction == Direction.Up ||
                             existing.Direction == Direction.Down)
                    {
                        if (existing.Region.Width == width_double)
                        {
                            if (dest_x >= x && dest_x <= x + 1 &&
                                dest_y == y)
                            {
                                return existing;
                            }
                        }
                        else if (existing.Region.Width == width_triple)
                        {
                            if (dest_x >= x - 1 && dest_x <= x + 1 &&
                                dest_y == y)
                            {
                                return existing;
                            }
                        }
                    }
                }
                else if (existing.Region.Width == width)
                {
                    if (existing.Direction == Direction.Up)
                    {
                        if (existing.Region.Height == width_double)
                        {
                            if (dest_x == x &&
                                dest_y >= y && dest_y <= y - 1)
                            {
                                return existing;
                            }
                        }
                    }
                    else if (existing.Direction == Direction.Down)
                    {
                        if (existing.Region.Height == width_double)
                        {
                            if (dest_x == x &&
                                dest_y >= y && dest_y <= y + 1)
                            {
                                return existing;
                            }
                        }
                    }
                    else if (existing.Direction == Direction.Left ||
                             existing.Direction == Direction.Right)
                    {
                        if (existing.Region.Height == width_double)
                        {
                            if (dest_x == x &&
                                dest_y >= y && dest_y <= y + 1)
                            {
                                return existing;
                            }
                        }
                        else if (existing.Region.Height == width_triple)
                        {
                            if (dest_x == x &&
                                dest_y >= y - 1 && dest_y <= y + 1)
                            {
                                return existing;
                            }
                        }
                    }
                }
                else if (existing.Region.Width == width_double &&
                         existing.Region.Height == width_double)
                {
                    if (dest_x >= x && dest_x <= x + 1 &&
                        dest_y >= y && dest_y <= y + 1)
                    {
                        return existing;
                    }
                }
            }

            return null;
        }

        public static Tile GetFurniture_Movable(List<Tile> furniture, Location location)
        {
            int width = (int)Main.Game.TileSize_X;
            int width_double = width * 2;
            int width_triple = width * 3;

            float dest_x = location.X;
            float dest_y = location.Y;

            Tile[] tiles = furniture.ToArray();

            int count = tiles.Length;
            for (int i = 0; i < count; i++)
            {
                Tile existing = tiles[i];

                if (!existing.CanMove)
                {
                    continue;
                }

                float x = existing.Location.X;
                float y = existing.Location.Y;

                if (dest_x == x &&
                    dest_y == y)
                {
                    return existing;
                }
                else if (existing.Region.Height == width)
                {
                    if (existing.Direction == Direction.Right)
                    {
                        if (existing.Region.Width == width_double)
                        {
                            if (dest_x >= x && dest_x <= x + 1 &&
                                dest_y == y)
                            {
                                return existing;
                            }
                        }
                    }
                    else if (existing.Direction == Direction.Left)
                    {
                        if (existing.Region.Width == width_double)
                        {
                            if (dest_x >= x - 1 && dest_x <= x &&
                                dest_y == y)
                            {
                                return existing;
                            }
                        }
                    }
                    else if (existing.Direction == Direction.Up ||
                             existing.Direction == Direction.Down)
                    {
                        if (existing.Region.Width == width_double)
                        {
                            if (dest_x >= x && dest_x <= x + 1 &&
                                dest_y == y)
                            {
                                return existing;
                            }
                        }
                        else if (existing.Region.Width == width_triple)
                        {
                            if (dest_x >= x - 1 && dest_x <= x + 1 &&
                                dest_y == y)
                            {
                                return existing;
                            }
                        }
                    }
                }
                else if (existing.Region.Width == width)
                {
                    if (existing.Direction == Direction.Up)
                    {
                        if (existing.Region.Height == width_double)
                        {
                            if (dest_x == x &&
                                dest_y >= y && dest_y <= y - 1)
                            {
                                return existing;
                            }
                        }
                    }
                    else if (existing.Direction == Direction.Down)
                    {
                        if (existing.Region.Height == width_double)
                        {
                            if (dest_x == x &&
                                dest_y >= y && dest_y <= y + 1)
                            {
                                return existing;
                            }
                        }
                    }
                    else if (existing.Direction == Direction.Left ||
                             existing.Direction == Direction.Right)
                    {
                        if (existing.Region.Height == width_double)
                        {
                            if (dest_x == x &&
                                dest_y >= y && dest_y <= y + 1)
                            {
                                return existing;
                            }
                        }
                        else if (existing.Region.Height == width_triple)
                        {
                            if (dest_x == x &&
                                dest_y >= y - 1 && dest_y <= y + 1)
                            {
                                return existing;
                            }
                        }
                    }
                }
                else if (existing.Region.Width == width_double &&
                         existing.Region.Height == width_double)
                {
                    if (dest_x >= x && dest_x <= x + 1 &&
                        dest_y >= y && dest_y <= y + 1)
                    {
                        return existing;
                    }
                }
            }

            return null;
        }

        public static Region GetSize(Tile tile)
        {
            Region size = new Region
            {
                X = tile.Location.X,
                Y = tile.Location.Y
            };

            int width = (int)Main.Game.TileSize_X;
            int width_double = width * 2;
            int width_triple = width * 3;

            if (tile.Region.Height == width)
            {
                if (tile.Region.Width == width_double)
                {
                    size.Width = 1;
                }
                else if (tile.Region.Width == width_triple)
                {
                    size.Width = 2;
                }
            }
            else if (tile.Region.Width == width)
            {
                if (tile.Region.Height == width_double)
                {
                    size.Height = 1;
                }
                else if (tile.Region.Height == width_triple)
                {
                    size.Height = 2;
                }
            }
            else if (tile.Region.Width == width_double &&
                     tile.Region.Height == width_double)
            {
                size.Width = 1;
                size.Height = 1;
            }

            return size;
        }

        public static List<Tile> GetAllFurniture(Layer layer, Point map_coords)
        {
            List<Tile> furniture = new List<Tile>();

            if (layer != null)
            {
                int world_x = map_coords.X * 20;
                int world_y = map_coords.Y * 20;

                foreach (Tile tile in layer.Tiles)
                {
                    if (tile.Texture != null)
                    {
                        if (tile.Location.X >= world_x && tile.Location.X < world_x + 20 &&
                            tile.Location.Y >= world_y && tile.Location.Y < world_y + 20)
                        {
                            furniture.Add(tile);
                        }
                    }
                }
            }

            return furniture;
        }

        public static List<Tile> GetOwned_Furniture(Character character, string name)
        {
            List<Tile> tiles = new List<Tile>();

            Tile[] furniture = Handler.OwnedFurniture[character.ID].ToArray();

            int count = furniture.Length;
            for (int i = 0; i < count; i++)
            {
                Tile tile = furniture[i];
                if (tile.Name.Contains(name))
                {
                    tiles.Add(tile);
                }
            }

            return tiles;
        }

        public static Direction GetFurnitureDirection(Tile tile, Character character)
        {
            if (tile.Location.X > character.Location.X)
            {
                return Direction.Right;
            }
            else if (tile.Location.X < character.Location.X)
            {
                return Direction.Left;
            }
            else if (tile.Location.X == character.Location.X)
            {
                if (tile.Location.Y > character.Location.Y)
                {
                    return Direction.Down;
                }
                else if (tile.Location.Y < character.Location.Y)
                {
                    return Direction.Up;
                }
            }

            return Direction.Nowhere;
        }

        public static bool Furniture_InRoom(Tile furniture, Character character)
        {
            Map room = GetRoom(character);
            if (room != null)
            {
                int layerCount = room.Layers.Count;
                for (int l = 0; l < layerCount; l++)
                {
                    Layer layer = room.Layers[l];
                    if (layer.Name == "Tiles")
                    {
                        int tileCount = layer.Tiles.Count;
                        for (int i = 0; i < tileCount; i++)
                        {
                            Tile tile = layer.Tiles[i];

                            int world_x = (int)((room.Location.X * 20) + tile.Location.X);
                            int world_y = (int)((room.Location.Y * 20) + tile.Location.Y);

                            if (world_x == furniture.Location.X &&
                                world_y == furniture.Location.Y)
                            {
                                return true;
                            }
                        }

                        break;
                    }
                }
            }

            return false;
        }

        public static Tile GetNearestExit_ToFurniture(Character character, Layer middle_tiles, Tile furniture)
        {
            Map room = GetRoom(character);
            if (room != null)
            {
                int layerCount = room.Layers.Count;
                for (int l = 0; l < layerCount; l++)
                {
                    Layer layer = room.Layers[l];
                    if (layer.Name == "Exits")
                    {
                        Tile nearest_exit = layer.Tiles[0];

                        int nearest_world_x = (int)((room.Location.X * 20) + nearest_exit.Location.X);
                        int nearest_world_y = (int)((room.Location.Y * 20) + nearest_exit.Location.Y);

                        int distance = GetDistance(new Location(nearest_world_x, nearest_world_y, 0), furniture.Location);

                        int tileCount = layer.Tiles.Count;
                        for (int t = 0; t < tileCount; t++)
                        {
                            Tile exit = layer.Tiles[t];

                            int exit_world_x = (int)((room.Location.X * 20) + exit.Location.X);
                            int exit_world_y = (int)((room.Location.Y * 20) + exit.Location.Y);

                            int new_distance = GetDistance(new Location(exit_world_x, exit_world_y, 0), furniture.Location);
                            if (new_distance < distance)
                            {
                                nearest_exit = exit;
                                distance = new_distance;
                            }
                        }

                        nearest_world_x = (int)((room.Location.X * 20) + nearest_exit.Location.X);
                        nearest_world_y = (int)((room.Location.Y * 20) + nearest_exit.Location.Y);

                        Tile door = middle_tiles.GetTile(new Vector2(nearest_world_x, nearest_world_y));
                        if (door != null &&
                            door.Name.Contains("Door"))
                        {
                            return door;
                        }
                        else
                        {
                            return new Tile
                            {
                                Location = new Location(nearest_world_x, nearest_world_y, 0)
                            };
                        }
                    }
                }
            }

            return null;
        }

        public static string GetTile_Name(Tile tile)
        {
            if (tile.Name.Contains("Bath"))
            {
                return "bathtub";
            }
            else if (tile.Name.Contains("Bed"))
            {
                return "bed";
            }
            else if (tile.Name.Contains("Bench"))
            {
                return "bench";
            }
            else if (tile.Name.Contains("Bookshelf"))
            {
                return "bookshelf";
            }
            else if (tile.Name.Contains("Chair"))
            {
                return "chair";
            }
            else if (tile.Name.Contains("Desk"))
            {
                return "desk";
            }
            else if (tile.Name.Contains("Couch"))
            {
                return "couch";
            }
            else if (tile.Name.Contains("Counter"))
            {
                return "cupboard";
            }
            else if (tile.Name.Contains("Door"))
            {
                return "door";
            }
            else if (tile.Name.Contains("Dresser"))
            {
                return "dresser";
            }
            else if (tile.Name.Contains("Fence"))
            {
                return "fence";
            }
            else if (tile.Name.Contains("Fridge"))
            {
                return "fridge";
            }
            else if (tile.Name.Contains("Lamp"))
            {
                return "lamp";
            }
            else if (tile.Name.Contains("StreetLight"))
            {
                return "street light";
            }
            else if (tile.Name.Contains("Loveseat"))
            {
                return "loveseat";
            }
            else if (tile.Name.Contains("Phone"))
            {
                return "phone";
            }
            else if (tile.Name.Contains("Register"))
            {
                return "cash register";
            }
            else if (tile.Name.Contains("Shower"))
            {
                return "shower";
            }
            else if (tile.Name.Contains("Sink"))
            {
                return "sink";
            }
            else if (tile.Name.Contains("Stove"))
            {
                return "stove";
            }
            else if (tile.Name.Contains("Table"))
            {
                return "table";
            }
            else if (tile.Name.Contains("Toilet"))
            {
                return "toilet";
            }
            else if (tile.Name.Contains("TV"))
            {
                return "television";
            }
            else if (tile.Name.Contains("Window"))
            {
                if (tile.Name.Contains("Broken"))
                {
                    return "broken window";
                }
                else
                {
                    return "window";
                }
            }
            else if (tile.Name.Contains("Glass"))
            {
                return "some shattered glass";
            }
            else if (tile.Name.Contains("Blood"))
            {
                if (tile.Name.Contains("Puddle"))
                {
                    return "huge pool of blood";
                }
                else if (tile.Name.Contains("Pool"))
                {
                    return "pool of blood";
                }
                else if (tile.Name.Contains("Large"))
                {
                    return "large pool of blood";
                }
                else if (tile.Name.Contains("Medium"))
                {
                    return "small pool of blood";
                }
                else if (tile.Name.Contains("Small"))
                {
                    return "tiny pool of blood";
                }
                else if (tile.Name.Contains("Tiny"))
                {
                    return "some drops of blood";
                }
                else if (tile.Name.Contains("Splat"))
                {
                    return "gorey mess";
                }
                else if (tile.Name.Contains("Prints"))
                {
                    return "some bloody foot prints";
                }
                else if (tile.Name.Contains("Trail"))
                {
                    return "trail of blood";
                }
                else
                {
                    return "some blood";
                }
            }
            else if (tile.Name.Contains("Tree"))
            {
                return "tree";
            }
            else if (tile.Name.Contains("Police"))
            {
                return "police car";
            }
            else if (tile.Name.Contains("Wall"))
            {
                return "wall";
            }
            else if (tile.Name.Contains("Carpet"))
            {
                return "carpet";
            }
            else if (tile.Name.Contains("Parking"))
            {
                return "parking lot";
            }
            else if (tile.Name.Contains("Road"))
            {
                return "road";
            }
            else if (tile.Name.Contains("Tile"))
            {
                return "tile floor";
            }
            else if (tile.Name.Contains("Wood"))
            {
                return "wood floor";
            }
            else if (tile.Name.Contains("Dirt"))
            {
                return "dirt";
            }
            else if (tile.Name.Contains("Grass"))
            {
                return "grass";
            }
            else if (tile.Name.Contains("Sidewalk"))
            {
                return "sidewalk";
            }
            else if (tile.Name.Contains("Wall"))
            {
                return "wall";
            }

            return null;
        }

        public static void GenDescription()
        {
            Something stat = Handler.Player.GetStat("Perception");
            int perception = (int)stat.Value;

            string description = "You see ";
            string he_she = "He";
            string his_her = "His";
            string him_her = "Him";

            List<string> details = new List<string>();
            if (Handler.Interaction_Character != null)
            {
                Character character = Handler.Interaction_Character;
                if (character.Gender == "Male")
                {
                    description += "a man. ";
                }
                else
                {
                    he_she = "She";
                    his_her = "Her";
                    him_her = "Her";

                    description += "a woman. ";
                }

                description += his_her + " name is " + character.Name + ".";

                if (Handler.Player.Relationships.ContainsKey(character.ID))
                {
                    description += " " + he_she + " is your " + Handler.Player.Relationships[character.ID].ToLower() + ".";
                }

                if (character.Unconscious)
                {
                    description += he_she + " is unconscious.";
                }

                if (character.Dead)
                {
                    description += he_she + " is dead.";
                }

                #region Left Hand

                Item left_hand_item = InventoryUtil.Get_EquippedItem(character, "Left Weapon Slot");
                if (left_hand_item != null)
                {
                    string item_description;
                    if (GameUtil.NameStartsWithVowel(left_hand_item.Name))
                    {
                        item_description = "an " + left_hand_item.Name.ToLower();
                    }
                    else
                    {
                        item_description = "a " + left_hand_item.Name.ToLower();
                    }

                    CryptoRandom random = new CryptoRandom();
                    int variation = random.Next(1, 4);
                    if (variation == 0)
                    {
                        details.Add(he_she + " is holding " + item_description + " in " + his_her.ToLower() + " left hand.");
                    }
                    else if (variation == 1)
                    {
                        details.Add("There is " + item_description + " in " + his_her.ToLower() + " left hand.");
                    }
                    else if (variation == 2)
                    {
                        details.Add(his_her + " left hand is holding " + item_description + ".");
                    }
                    else if (variation == 3)
                    {
                        details.Add("You see " + item_description + " in " + his_her.ToLower() + " left hand.");
                    }
                }

                #endregion

                #region Right Hand

                Item right_hand_item = InventoryUtil.Get_EquippedItem(character, "Right Weapon Slot");
                if (right_hand_item != null)
                {
                    string item_description;
                    if (GameUtil.NameStartsWithVowel(right_hand_item.Name))
                    {
                        item_description = "an " + right_hand_item.Name.ToLower();
                    }
                    else
                    {
                        item_description = "a " + right_hand_item.Name.ToLower();
                    }

                    CryptoRandom random = new CryptoRandom();
                    int variation = random.Next(1, 4);
                    if (variation == 0)
                    {
                        details.Add(he_she + " is holding " + item_description + " in " + his_her.ToLower() + " right hand.");
                    }
                    else if (variation == 1)
                    {
                        details.Add("There is " + item_description + " in " + his_her.ToLower() + " right hand.");
                    }
                    else if (variation == 2)
                    {
                        details.Add(his_her + " right hand is holding " + item_description + ".");
                    }
                    else if (variation == 3)
                    {
                        details.Add("You see " + item_description + " in " + his_her.ToLower() + " right hand.");
                    }
                }

                #endregion

                #region Mask

                Item mask_item = InventoryUtil.Get_EquippedItem(character, "Mask Slot");
                if (mask_item != null)
                {
                    int words = mask_item.Name.Split(' ').Length;

                    string item_description;
                    if (words > 1)
                    {
                        if (GameUtil.NameStartsWithVowel(mask_item.Name))
                        {
                            item_description = "an " + mask_item.Name.ToLower();
                        }
                        else
                        {
                            item_description = "a " + mask_item.Name.ToLower();
                        }
                    }
                    else
                    {
                        item_description = mask_item.Name.ToLower();
                    }

                    CryptoRandom random = new CryptoRandom();
                    int variation = random.Next(1, 6);
                    if (variation == 0)
                    {
                        details.Add(he_she + " is wearing " + item_description + " on " + his_her.ToLower() + " face.");
                    }
                    else if (variation == 1)
                    {
                        details.Add("There is " + item_description + " on " + his_her.ToLower() + " face.");
                    }
                    else if (variation == 2)
                    {
                        details.Add(his_her + " face has " + item_description + " on it.");
                    }
                    else if (variation == 3)
                    {
                        details.Add(he_she + " is wearing " + item_description + ".");
                    }
                    else if (variation == 4)
                    {
                        details.Add("You see " + item_description + " on " + him_her.ToLower() + ".");
                    }
                    else if (variation == 5)
                    {
                        details.Add(he_she + " has " + item_description + " on.");
                    }
                }

                #endregion

                #region Backpack

                Item backpack_item = InventoryUtil.Get_EquippedItem(character, "Backpack Slot");
                if (backpack_item != null)
                {
                    string item_description;
                    if (GameUtil.NameStartsWithVowel(backpack_item.Name))
                    {
                        item_description = "an " + backpack_item.Name.ToLower();
                    }
                    else
                    {
                        item_description = "a " + backpack_item.Name.ToLower();
                    }

                    CryptoRandom random = new CryptoRandom();
                    int variation = random.Next(1, 10);
                    if (variation == 0)
                    {
                        details.Add(he_she + " has " + item_description + " on.");
                    }
                    else if (variation == 1)
                    {
                        details.Add(he_she + " is wearing " + item_description + ".");
                    }
                    else if (variation == 2)
                    {
                        details.Add("You see " + item_description + " on " + him_her.ToLower() + ".");
                    }
                    else if (variation == 3)
                    {
                        details.Add("There is " + item_description + " on " + him_her.ToLower() + ".");
                    }
                    else if (variation == 4)
                    {
                        details.Add(he_she + " has " + item_description + ".");
                    }
                    else if (variation == 5)
                    {
                        details.Add("You see " + item_description + ".");
                    }
                    else if (variation == 6)
                    {
                        details.Add(he_she + " has " + item_description + " on " + his_her.ToLower() + " back.");
                    }
                    else if (variation == 7)
                    {
                        details.Add(he_she + " is wearing " + item_description + " on " + his_her.ToLower() + " back.");
                    }
                    else if (variation == 8)
                    {
                        details.Add("You see " + item_description + " on " + his_her.ToLower() + " back.");
                    }
                    else if (variation == 9)
                    {
                        details.Add("There is " + item_description + " on " + his_her.ToLower() + " back.");
                    }
                }

                #endregion

                foreach (string detail in details)
                {
                    if (Utility.RandomPercent(perception))
                    {
                        description += " " + detail;
                    }
                }
            }
            else if (Handler.Interaction_Tile != null)
            {
                bool plural = false;

                string name = GetTile_Name(Handler.Interaction_Tile);
                if (name.Contains(" "))
                {
                    if (name.Split(' ')[0] == "some")
                    {
                        plural = true;
                    }
                }

                if (plural)
                {
                    description += name + ".";
                }
                else
                {
                    if (GameUtil.NameStartsWithVowel(name))
                    {
                        description += "an " + name + ".";
                    }
                    else
                    {
                        description += "a " + name + ".";
                    }
                }
            }

            GameUtil.AddMessage(description);
        }

        public static void AssignPlayerBed(World world, Character player)
        {
            player.Relationships.Clear();

            Army army = CharacterManager.GetArmy("Characters");
            Squad squad = army.GetSquad("Citizens");

            Map map = world.Maps[0];

            Layer middle_tiles = map.GetLayer("MiddleTiles");
            Layer top_tiles = map.GetLayer("TopTiles");

            CryptoRandom random = new CryptoRandom();
            List<Map> homes = WorldGen.Residential.OrderBy(a => random.Next()).ToList();

            Tile bed = null;
            Map newHome = null;
            Character replacement = null;
            Character mate = null;

            List<Tile> middle_furniture = new List<Tile>();

            //Start player in single home
            foreach (Map home in homes)
            {
                Point map_coords = new Point((int)home.Location.X, (int)home.Location.Y);

                middle_furniture = GetAllFurniture(middle_tiles, map_coords);
                
                int bed_count = 0;
                Tile possibleBed = null;

                foreach (Tile tile in middle_furniture)
                {
                    if (tile.Name.Contains("Bed"))
                    {
                        possibleBed = tile;
                        bed_count++;
                    }
                }

                if (bed_count == 1)
                {
                    Location bed_location = null;
                    if (possibleBed.Direction == Direction.Up)
                    {
                        bed_location = new Location(possibleBed.Location.X, possibleBed.Location.Y + 1, 0);
                    }
                    else if (possibleBed.Direction == Direction.Right ||
                             possibleBed.Direction == Direction.Down)
                    {
                        bed_location = new Location(possibleBed.Location.X, possibleBed.Location.Y, 0);
                    }
                    else if (possibleBed.Direction == Direction.Left)
                    {
                        bed_location = new Location(possibleBed.Location.X + 1, possibleBed.Location.Y, 0);
                    }

                    Character character = GetCharacter(bed_location);
                    if (character != null)
                    {
                        replacement = character;
                        bed = possibleBed;
                        newHome = home;
                        break;
                    }
                }
            }

            if (bed == null)
            {
                //Start player next to an NPC
                foreach (Map home in homes)
                {
                    Point map_coords = new Point((int)home.Location.X, (int)home.Location.Y);

                    middle_furniture = GetAllFurniture(middle_tiles, map_coords);
                    List<Tile> top_furniture = GetAllFurniture(top_tiles, map_coords);

                    int bed_count = 0;
                    Tile possibleBed = null;

                    foreach (Tile tile in middle_furniture)
                    {
                        if (tile.Name.Contains("DoubleBed"))
                        {
                            possibleBed = tile;
                            bed_count++;
                        }
                    }

                    if (bed_count >= 1)
                    {
                        Location bed_location = default;
                        if (possibleBed.Direction == Direction.Up)
                        {
                            bed_location = new Location(possibleBed.Location.X, possibleBed.Location.Y + 1, 0);
                        }
                        else if (possibleBed.Direction == Direction.Right ||
                                 possibleBed.Direction == Direction.Down)
                        {
                            bed_location = new Location(possibleBed.Location.X, possibleBed.Location.Y, 0);
                        }
                        else if (possibleBed.Direction == Direction.Left)
                        {
                            bed_location = new Location(possibleBed.Location.X + 1, possibleBed.Location.Y, 0);
                        }

                        Character character = GetCharacter(bed_location);
                        if (character != null)
                        {
                            bed = possibleBed;
                            newHome = home;
                            mate = character;
                            break;
                        }
                    }
                }
            }

            if (bed == null)
            {
                //Start player in any open bed
                foreach (Map home in homes)
                {
                    Point map_coords = new Point((int)home.Location.X, (int)home.Location.Y);

                    middle_furniture = GetAllFurniture(middle_tiles, map_coords);

                    foreach (Tile tile in middle_furniture)
                    {
                        if (tile.Name.Contains("DoubleBed"))
                        {
                            bed = tile;
                            break;
                        }
                    }

                    if (bed == null)
                    {
                        foreach (Tile tile in middle_furniture)
                        {
                            if (tile.Name.Contains("Bed"))
                            {
                                bed = tile;
                                break;
                            }
                        }
                    }

                    if (bed != null)
                    {
                        newHome = home;

                        Location bed_location = default;
                        if (bed.Direction == Direction.Up)
                        {
                            bed_location = new Location(bed.Location.X, bed.Location.Y + 1, 0);
                        }
                        else if (bed.Direction == Direction.Right ||
                                 bed.Direction == Direction.Down)
                        {
                            bed_location = new Location(bed.Location.X, bed.Location.Y, 0);
                        }
                        else if (bed.Direction == Direction.Left)
                        {
                            bed_location = new Location(bed.Location.X + 1, bed.Location.Y, 0);
                        }

                        Character character = GetCharacter(bed_location);
                        if (character != null)
                        {
                            replacement = character;
                            break;
                        }
                    }
                }
            }

            if (bed != null)
            {
                Point map_coords = new Point((int)newHome.Location.X, (int)newHome.Location.Y);

                List<Tile> top_furniture = GetAllFurniture(top_tiles, map_coords);

                if (replacement != null)
                {
                    Handler.OwnedFurniture.Remove(replacement.ID);
                    squad.Characters.Remove(replacement);

                    player.Location = new Location(replacement.Location.X, replacement.Location.Y, 0);
                }
                else if (mate != null)
                {
                    if (mate.Direction == Direction.Up ||
                        mate.Direction == Direction.Down)
                    {
                        player.Location = new Location(mate.Location.X + 1, mate.Location.Y, 0);
                    }
                    else if (mate.Direction == Direction.Right ||
                             mate.Direction == Direction.Left)
                    {
                        player.Location = new Location(mate.Location.X, mate.Location.Y + 1, 0);
                    }
                }
                else
                {
                    if (bed.Direction == Direction.Up)
                    {
                        player.Location = new Location(bed.Location.X, bed.Location.Y + 1, 0);
                    }
                    else if (bed.Direction == Direction.Right ||
                             bed.Direction == Direction.Down)
                    {
                        player.Location = new Location(bed.Location.X, bed.Location.Y, 0);
                    }
                    else if (bed.Direction == Direction.Left)
                    {
                        player.Location = new Location(bed.Location.X + 1, bed.Location.Y, 0);
                    }
                }

                if (!Handler.OwnedFurniture.ContainsKey(player.ID))
                {
                    Handler.OwnedFurniture.Add(player.ID, new List<Tile>());
                }

                foreach (Tile tile in middle_furniture)
                {
                    Handler.OwnedFurniture[player.ID].Add(tile);
                }

                foreach (Tile tile in top_furniture)
                {
                    Handler.OwnedFurniture[player.ID].Add(tile);
                }

                string last_name = player.Name.Split(' ')[1].Trim();

                List<Character> characters = GetAllCharacters(map_coords);
                foreach (Character existing in characters)
                {
                    if (!player.Relationships.ContainsKey(existing.ID))
                    {
                        string first_name = existing.Name.Split(' ')[0].Trim();
                        existing.Name = first_name + " " + last_name;

                        random = new CryptoRandom();
                        int relative = random.Next(0, 2);

                        if (existing.ID == mate?.ID)
                        {
                            if (existing.Gender == "Male")
                            {
                                if (relative == 0)
                                {
                                    player.Relationships.Add(existing.ID, "Husband");
                                }
                                else
                                {
                                    player.Relationships.Add(existing.ID, "Boyfriend");
                                }
                            }
                            else
                            {
                                if (relative == 0)
                                {
                                    player.Relationships.Add(existing.ID, "Wife");
                                }
                                else
                                {
                                    player.Relationships.Add(existing.ID, "Girlfriend");
                                }
                            }
                        }
                        else
                        {
                            if (existing.Gender == "Male")
                            {
                                if (relative == 0)
                                {
                                    player.Relationships.Add(existing.ID, "Father");
                                }
                                else
                                {
                                    player.Relationships.Add(existing.ID, "Brother");
                                }
                            }
                            else
                            {
                                if (relative == 0)
                                {
                                    player.Relationships.Add(existing.ID, "Mother");
                                }
                                else
                                {
                                    player.Relationships.Add(existing.ID, "Sister");
                                }
                            }
                        }
                    }
                }
            }
        }

        public static bool NextTo(Location target, Location source)
        {
            if (target.X == source.X)
            {
                if (target.Y == source.Y - 1 ||
                    target.Y == source.Y + 1)
                {
                    return true;
                }
            }
            else if (target.Y == source.Y)
            {
                if (target.X == source.X - 1 ||
                    target.X == source.X + 1)
                {
                    return true;
                }
            }

            return false;
        }

        public static Tile StandingByFurniture(Layer tiles, Location location, string type)
        {
            foreach (Tile tile in tiles.Tiles)
            {
                if (tile.Name.Contains(type))
                {
                    if (NextTo(tile.Location, location))
                    {
                        return tile;
                    }
                }
            }

            return null;
        }

        public static bool NextToFence(Layer middle_tiles, Location location)
        {
            foreach (Tile tile in middle_tiles.Tiles)
            {
                if (!string.IsNullOrEmpty(tile.Name))
                {
                    if (tile.Name.Contains("Fence"))
                    {
                        if (tile.Location.X >= location.X - 1 && tile.Location.X <= location.X + 1 &&
                            tile.Location.Y >= location.Y - 1 && tile.Location.Y <= location.Y + 1)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static bool PassedOpenDoor(Layer middle_tiles, Character character)
        {
            Vector2 tile_location = character.Location.ToVector2;
            if (character.Direction == Direction.Up)
            {
                tile_location.Y++;
            }
            else if (character.Direction == Direction.Right)
            {
                tile_location.X--;
            }
            else if (character.Direction == Direction.Down)
            {
                tile_location.Y--;
            }
            else if (character.Direction == Direction.Left)
            {
                tile_location.X++;
            }

            Tile tile = middle_tiles.GetTile(tile_location);
            if (tile != null)
            {
                if (tile.Name.Contains("Open") &&
                    tile.Name.Contains("Door"))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool PassedOpenWindow(Layer middle_tiles, Character character)
        {
            Vector2 tile_location = character.Location.ToVector2;
            if (character.Direction == Direction.Up)
            {
                tile_location.Y++;
            }
            else if (character.Direction == Direction.Right)
            {
                tile_location.X--;
            }
            else if (character.Direction == Direction.Down)
            {
                tile_location.Y--;
            }
            else if (character.Direction == Direction.Left)
            {
                tile_location.X++;
            }

            Tile tile = middle_tiles.GetTile(tile_location);
            if (tile != null)
            {
                if (tile.Name.Contains("Open") &&
                    tile.Name.Contains("Window"))
                {
                    return true;
                }
            }

            return false;
        }

        public static Map GetCurrentMap(Character character)
        {
            if (character.Map == null)
            {
                SetCurrentMap(character);
            }

            foreach (Map map in WorldGen.Residential)
            {
                if (map.ID == character.Map.ID)
                {
                    return map;
                }
            }

            foreach (Map map in WorldGen.Commercial)
            {
                if (map.ID == character.Map.ID)
                {
                    return map;
                }
            }

            foreach (Map map in WorldGen.Parks)
            {
                if (map.ID == character.Map.ID)
                {
                    return map;
                }
            }

            foreach (Map map in WorldGen.Roads)
            {
                if (map.ID == character.Map.ID)
                {
                    return map;
                }
            }

            return null;
        }

        public static void SetCurrentMap(Character character)
        {
            int block_x = (int)character.Location.X / 20;
            int block_y = (int)character.Location.Y / 20;

            int count = WorldGen.Residential.Count;
            for (int i = 0; i < count; ++i)
            {
                Map map = WorldGen.Residential[i];
                if (map.Location.X == block_x &&
                    map.Location.Y == block_y)
                {
                    character.Map = map;
                    return;
                }
            }

            count = WorldGen.Commercial.Count;
            for (int i = 0; i < count; ++i)
            {
                Map map = WorldGen.Commercial[i];
                if (map.Location.X == block_x &&
                    map.Location.Y == block_y)
                {
                    character.Map = map;
                    return;
                }
            }

            count = WorldGen.Parks.Count;
            for (int i = 0; i < count; ++i)
            {
                Map map = WorldGen.Parks[i];
                if (map.Location.X == block_x &&
                    map.Location.Y == block_y)
                {
                    character.Map = map;
                    return;
                }
            }

            count = WorldGen.Roads.Count;
            for (int i = 0; i < count; ++i)
            {
                Map map = WorldGen.Roads[i];
                if (map.Location.X == block_x &&
                    map.Location.Y == block_y)
                {
                    character.Map = map;
                    return;
                }
            }
        }

        public static void UpdateWorldMap()
        {
            if (Handler.WorldMap_Visible)
            {
                int world_x = (int)Handler.Player.Location.X / 20;
                int world_y = (int)Handler.Player.Location.Y / 20;

                Tile center = null;
                foreach (Tile tile in WorldGen.Worldmap)
                {
                    if (tile.Location.X == world_x &&
                        tile.Location.Y == world_y)
                    {
                        center = tile;
                        break;
                    }
                }

                if (center != null)
                {
                    center.Region.X = Handler.Player.Region.X + (Handler.Player.Region.Width / 2) - (Main.Game.MenuSize_X / 2);
                    center.Region.Y = Handler.Player.Region.Y + (Handler.Player.Region.Height / 2) - (Main.Game.MenuSize_Y / 2);
                    center.Region.Width = Main.Game.MenuSize_X;
                    center.Region.Height = Main.Game.MenuSize_Y;

                    foreach (Tile tile in WorldGen.Worldmap)
                    {
                        tile.Region.Width = Main.Game.MenuSize_X;
                        tile.Region.Height = Main.Game.MenuSize_Y;

                        int world_x_diff = (int)tile.Location.X - world_x;
                        if (world_x_diff < 0)
                        {
                            world_x_diff *= -1;
                        }

                        int world_y_diff = (int)tile.Location.Y - world_y;
                        if (world_y_diff < 0)
                        {
                            world_y_diff *= -1;
                        }

                        if (tile.Location.X < world_x)
                        {
                            tile.Region.X = center.Region.X - (world_x_diff * Main.Game.MenuSize_X);
                        }
                        else if (tile.Location.X > world_x)
                        {
                            tile.Region.X = center.Region.X + (world_x_diff * Main.Game.MenuSize_X);
                        }
                        else if (tile.Location.X == world_x)
                        {
                            tile.Region.X = center.Region.X;
                        }

                        if (tile.Location.Y < world_y)
                        {
                            tile.Region.Y = center.Region.Y - (world_y_diff * Main.Game.MenuSize_Y);
                        }
                        else if (tile.Location.Y > world_y)
                        {
                            tile.Region.Y = center.Region.Y + (world_y_diff * Main.Game.MenuSize_Y);
                        }
                        else if (tile.Location.Y == world_y)
                        {
                            tile.Region.Y = center.Region.Y;
                        }
                    }
                }
            }
        }

        public static void AddEffect(Vector3 location, string name, string texture)
        {
            Map map = GetMap();
            if (map != null)
            {
                Layer bottom_tiles = map.GetLayer("BottomTiles");

                Tile bottom_tile = bottom_tiles.GetTile(new Vector2(location.X, location.Y));
                if (bottom_tile != null)
                {
                    Layer effect_tiles = map.GetLayer("EffectTiles");

                    Texture2D texture2D = AssetManager.Textures[texture];

                    effect_tiles.Tiles.Add(new Tile
                    {
                        ID = Handler.GetID(),
                        Map = map,
                        Layer = effect_tiles,
                        Name = name,
                        Location = new Location(location.X, location.Y, location.Z),
                        Region = bottom_tile.Region,
                        Texture = texture2D,
                        Image = new Rectangle(0, 0, texture2D.Width, texture2D.Height),
                        Visible = true
                    });
                }
            }
        }

        public static void AddEffect_Animated(Vector3 location, Direction direction, string texture, TimeSpan start_time, int duration)
        {
            Scene scene = SceneManager.GetScene("Gameplay");
            Map map = scene.World.Maps[0];
            if (map != null)
            {
                Layer bottom_tiles = map.GetLayer("BottomTiles");

                Tile bottom_tile = bottom_tiles.GetTile(new Vector2(location.X, location.Y));
                if (bottom_tile != null)
                {
                    Layer effect_tiles = map.GetLayer("EffectTiles");

                    Texture2D texture2D = AssetManager.Textures[texture];

                    effect_tiles.Tiles.Add(new Tile
                    {
                        ID = Handler.GetID(),
                        Map = map,
                        Layer = effect_tiles,
                        Name = texture,
                        Direction = direction,
                        Location = new Location(location.X, location.Y, location.Z),
                        Region = bottom_tile.Region,
                        Texture = texture2D,
                        Image = new Rectangle(0, 0, texture2D.Height, texture2D.Height),
                        Animated = true,
                        Time = start_time,
                        Duration = duration,
                        Visible = true
                    });
                }
            }
        }

        public static void Animate_Effect(SpriteBatch spriteBatch, Tile tile)
        {
            int frames = tile.Texture.Width / tile.Texture.Height;
            float frame_duration = tile.Duration / frames;

            int time = (int)(TimeManager.Now.TotalMilliseconds - tile.Time.TotalMilliseconds);
            if (time > 0)
            {
                int frame = (int)(time / frame_duration);
                tile.Image = new Rectangle(tile.Texture.Height * frame, tile.Image.Y, tile.Image.Width, tile.Image.Height);
            }

            if (tile.Image.X >= tile.Texture.Width - tile.Texture.Height)
            {
                tile.Visible = false;
            }

            if (tile.Visible)
            {
                float rotation;

                switch (tile.Direction)
                {
                    case Direction.Up:
                    default:
                        rotation = 0f;
                        break;

                    case Direction.Right:
                        rotation = 1.5f;
                        break;

                    case Direction.Down:
                        rotation = 3f;
                        break;

                    case Direction.Left:
                        rotation = 4.5f;
                        break;
                }

                Rectangle region = new Rectangle((int)(tile.Region.X + (tile.Region.Width / 2)), (int)(tile.Region.Y + (tile.Region.Height / 2)), (int)tile.Region.Width, (int)tile.Region.Height);
                spriteBatch.Draw(tile.Texture, region, tile.Image, Color.White, rotation, new Vector2(tile.Texture.Height / 2, tile.Texture.Height / 2), SpriteEffects.None, 0);
            }
        }
    }
}

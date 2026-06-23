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
using OP_Engine.Inputs;

namespace Despicaville.Util
{
    public static class WorldUtil
    {
        public static Map? GetMap()
        {
            Scene? scene = SceneManager.GetScene("Gameplay");
            if (scene != null &&
                scene.World != null &&
                scene.World.Maps.Count > 0)
            {
                return scene.World.Maps[0];
            }

            return null;
        }

        public static bool CanMove(Character character, Map map, Location destination)
        {
            Layer? bottom_tiles = map.GetLayer("BottomTiles");
            if (bottom_tiles == null)
            {
                return false;
            }

            //Check bottom tiles
            if (destination.X < bottom_tiles.Columns && destination.X >= 0 &&
                destination.Y < bottom_tiles.Rows && destination.Y >= 0)
            {
                Tile? current = bottom_tiles.GetTile(destination.ToVector2);
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
            Layer? middle_tiles = map.GetLayer("MiddleTiles");
            if (middle_tiles == null)
            {
                return false;
            }

            List<Tile> furniture = GetFurniture_Nearby(middle_tiles, destination);

            Tile? middle_tile = GetFurniture(furniture, destination);
            if (middle_tile != null &&
                middle_tile.Name != null)
            {
                if (!middle_tile.Name.Contains("Open"))
                {
                    if (Handler.Pull_ID != middle_tile.ID &&
                        middle_tile.BlocksMovement)
                    {
                        return false;
                    }
                    else if (middle_tile.Name.Contains("Window") &&
                             character.Type != "Player")
                    {
                        return false;
                    }
                }
            }

            //Check other characters
            Character? other = GetCharacter(destination);
            if (other != null)
            {
                if (Handler.Pull_Character != null)
                {
                    if (Handler.Pull_Character.ID != other.ID)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            if (Handler.Pull &&
                character.Location != null &&
                character.Destination != null)
            {
                #region Pull Collision Check

                Location? newLocation = null;
                Region? size = null;

                if (Handler.Pull_Character != null &&
                    Handler.Pull_Character.Location != null)
                {
                    #region Character

                    size = new Region(Handler.Pull_Character.Location.X, Handler.Pull_Character.Location.Y, 0, 0);
                    newLocation = new Location(Handler.Pull_Character.Location.X, Handler.Pull_Character.Location.Y, 0);

                    Direction direction = GetDirection(character.Location, character.Destination);
                    if (direction == Direction.North)
                    {
                        newLocation.Y--;
                    }
                    else if (direction == Direction.East)
                    {
                        newLocation.X++;
                    }
                    else if (direction == Direction.South)
                    {
                        newLocation.Y++;
                    }
                    else if (direction == Direction.West)
                    {
                        newLocation.X--;
                    }

                    #endregion
                }
                else if (Handler.Pull_Tile != null)
                {
                    #region Tile

                    size = GetSize(Handler.Pull_Tile);
                    newLocation = new Location(Handler.Pull_Tile.Location.X, Handler.Pull_Tile.Location.Y, 0);

                    Direction direction = GetDirection(character.Location, character.Destination);
                    if (direction == Direction.North)
                    {
                        newLocation.Y--;
                    }
                    else if (direction == Direction.East)
                    {
                        newLocation.X++;
                    }
                    else if (direction == Direction.South)
                    {
                        newLocation.Y++;
                    }
                    else if (direction == Direction.West)
                    {
                        newLocation.X--;
                    }

                    #endregion
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

                            Tile? bottom = bottom_tiles.GetTile(new Vector2(X, Y));
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

                            Tile? middle = GetFurniture(furniture, new Location(X, Y, 0));
                            if (middle != null &&
                                middle.Name != null &&
                                middle.ID != Handler.Pull_ID &&
                                !middle.Name.Contains("Open") &&
                                !middle.Name.Contains("Broken"))
                            {
                                if (Handler.Pull_Character != null)
                                {
                                    if (middle.BlocksMovement)
                                    {
                                        return false;
                                    }
                                }
                                else if (Handler.Pull_Tile != null)
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

                #endregion
            }

            return true;
        }

        public static Tile? GetTile(Location location)
        {
            Map? map = GetMap();
            if (map == null)
            {
                return null;
            }

            Layer? bottom_tiles = map.GetLayer("BottomTiles");
            if (bottom_tiles == null)
            {
                return null;
            }

            //Check bottom tiles
            if (location.X < bottom_tiles.Columns && location.X >= 0 &&
                location.Y < bottom_tiles.Rows && location.Y >= 0)
            {
                Tile? current = bottom_tiles.GetTile(location.ToVector2);
                if (current != null)
                {
                    if (current.BlocksMovement)
                    {
                        return current;
                    }
                }
            }

            //Check for furniture
            Tile? furniture = GetFurniture(Handler.MiddleFurniture, location);
            if (furniture != null &&
                furniture.Name != null)
            {
                if (!furniture.Name.Contains("Open") &&
                    !furniture.Name.Contains("Broken") &&
                    !furniture.Name.Contains("Window"))
                {
                    return furniture;
                }
            }

            return null;
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

        public static Direction GetDirection(Location source, Location target)
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

        public static void UpdateWorld(World world)
        {
            if (Handler.Player == null ||
                Handler.Player.Location == null ||
                !world.Visible)
            {
                return;
            }

            Army? characters = CharacterManager.GetArmy("Characters");
            if (characters == null)
            {
                return;
            }

            List<Tile> visible = [];
            if (Handler.VisibleTiles.TryGetValue(Handler.Player.ID, out List<Tile>? value))
            {
                visible = value;
            }

            Map map = world.Maps[0];

            Layer? bottom_tiles = map.GetLayer("BottomTiles");
            Layer? middle_tiles = map.GetLayer("MiddleTiles");
            Layer? top_tiles = map.GetLayer("TopTiles");
            Layer? roof_tiles = map.GetLayer("RoofTiles");

            int start_y = (int)Handler.Player.Location.Y - Handler.SightDistance - 1;
            int end_y = (int)Handler.Player.Location.Y + Handler.SightDistance + 1;
            int start_x = (int)Handler.Player.Location.X - Handler.SightDistance - 1;
            int end_x = (int)Handler.Player.Location.X + Handler.SightDistance + 1;

            if (!Handler.Player.Unconscious)
            {
                for (int y = start_y; y <= end_y; y++)
                {
                    for (int x = start_x; x <= end_x; x++)
                    {
                        Vector2 location = new(x, y);

                        Tile? tile = bottom_tiles?.GetTile(location);
                        if (tile != null)
                        {
                            tile.InSight = Location_IsVisible(Handler.Player.ID, tile.Location);

                            Tile? middle_tile = middle_tiles?.GetTile(location);
                            if (middle_tile != null)
                            {
                                middle_tile.InSight = tile.InSight;
                            }

                            Tile? top_tile = top_tiles?.GetTile(location);
                            if (top_tile != null)
                            {
                                top_tile.InSight = tile.InSight;
                            }

                            Tile? roof_tile = roof_tiles?.GetTile(location);
                            if (roof_tile != null)
                            {
                                roof_tile.InSight = tile.InSight;
                            }
                        }
                    }
                }

                foreach (Tile tile in visible)
                {
                    tile.Visible = InputManager.MouseWithin(tile.Region.ToRectangle);
                }

                foreach (Squad squad in characters.Squads)
                {
                    foreach (Character character in squad.Characters)
                    {
                        if (Location_IsVisible(Handler.Player.ID, character.Location) ||
                            Location_IsVisible(Handler.Player.ID, character.Destination))
                        {
                            character.InSight = true;
                        }
                        else
                        {
                            character.InSight = false;
                        }
                    }
                }
            }
            else
            {
                for (int y = start_y; y <= end_y; y++)
                {
                    for (int x = start_x; x <= end_x; x++)
                    {
                        Vector2 location = new(x, y);

                        Tile? tile = bottom_tiles?.GetTile(location);
                        if (tile != null)
                        {
                            tile.InSight = false;

                            Tile? middle_tile = middle_tiles?.GetTile(location);
                            if (middle_tile != null)
                            {
                                middle_tile.InSight = false;
                            }

                            Tile? top_tile = top_tiles?.GetTile(location);
                            if (top_tile != null)
                            {
                                top_tile.InSight = false;
                            }

                            Tile? roof_tile = roof_tiles?.GetTile(location);
                            if (roof_tile != null)
                            {
                                roof_tile.InSight = false;
                            }
                        }
                    }
                }

                foreach (Tile tile in visible)
                {
                    tile.Visible = false;
                }

                foreach (Squad squad in characters.Squads)
                {
                    foreach (Character character in squad.Characters)
                    {
                        character.InSight = false;
                    }
                }
            }
        }

        public static bool Location_IsVisible(long character_id, Location? location)
        {
            if (location != null)
            {
                List<Tile> tiles = [];
                if (Handler.VisibleTiles.TryGetValue(character_id, out List<Tile>? value))
                {
                    tiles = value;
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

        public static void PullTile_SetLocation(Direction direction)
        {
            if (Handler.Pull_Tile != null)
            {
                Map? map = GetMap();
                Layer? middle_tiles = map?.GetLayer("MiddleTiles");
                if (middle_tiles == null)
                {
                    return;
                }

                Location newLocation = new(Handler.Pull_Tile.Location.X, Handler.Pull_Tile.Location.Y);
                switch (direction)
                {
                    case Direction.North:
                        if (Handler.Pull_Tile.IsLightSource)
                        {
                            for (int l = 0; l < Handler.light_sources.Count; l++)
                            {
                                Point source = Handler.light_sources[l];
                                if (source.X == Handler.Pull_Tile.Location.X &&
                                    source.Y == Handler.Pull_Tile.Location.Y)
                                {
                                    source.Y--;

                                    Handler.light_sources[l] = new Point(source.X, source.Y);
                                    break;
                                }
                            }
                        }

                        newLocation.Y--;
                        SwapTiles(middle_tiles, Handler.Pull_Tile, newLocation);
                        break;

                    case Direction.East:
                        if (Handler.Pull_Tile.IsLightSource)
                        {
                            for (int l = 0; l < Handler.light_sources.Count; l++)
                            {
                                Point source = Handler.light_sources[l];
                                if (source.X == Handler.Pull_Tile.Location.X &&
                                    source.Y == Handler.Pull_Tile.Location.Y)
                                {
                                    source.X++;

                                    Handler.light_sources[l] = new Point(source.X, source.Y);
                                    break;
                                }
                            }
                        }

                        newLocation.X++;
                        SwapTiles(middle_tiles, Handler.Pull_Tile, newLocation);
                        break;

                    case Direction.South:
                        if (Handler.Pull_Tile.IsLightSource)
                        {
                            for (int l = 0; l < Handler.light_sources.Count; l++)
                            {
                                Point source = Handler.light_sources[l];
                                if (source.X == Handler.Pull_Tile.Location.X &&
                                    source.Y == Handler.Pull_Tile.Location.Y)
                                {
                                    source.Y++;

                                    Handler.light_sources[l] = new Point(source.X, source.Y);
                                    break;
                                }
                            }
                        }

                        newLocation.Y++;
                        SwapTiles(middle_tiles, Handler.Pull_Tile, newLocation);
                        break;

                    case Direction.West:
                        if (Handler.Pull_Tile.IsLightSource)
                        {
                            for (int l = 0; l < Handler.light_sources.Count; l++)
                            {
                                Point source = Handler.light_sources[l];
                                if (source.X == Handler.Pull_Tile.Location.X &&
                                    source.Y == Handler.Pull_Tile.Location.Y)
                                {
                                    source.X--;

                                    Handler.light_sources[l] = new Point(source.X, source.Y);
                                    break;
                                }
                            }
                        }

                        newLocation.X--;
                        SwapTiles(middle_tiles, Handler.Pull_Tile, newLocation);
                        break;
                }
            }
        }

        public static void Push_Tile(Tile tile, Location newLocation)
        {
            Map? map = GetMap();
            Layer? middle_tiles = map?.GetLayer("MiddleTiles");
            if (middle_tiles == null)
            {
                return;
            }

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

            SwapTiles(middle_tiles, tile, newLocation);
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

        public static Tile? GetClosestTile(List<Tile> tiles, Character character)
        {
            if (character.Location == null)
            {
                return null;
            }

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
            List<Character> result = [];

            if (Handler.Player?.Location == null)
            {
                return result;
            }

            if (NextTo(Handler.Player.Location, location))
            {
                result.Add(Handler.Player);
            }

            Army army = CharacterManager.Armies[0];
            Squad citizens = army.Squads[1];

            int count = citizens.Characters.Count;
            for (int i = 0; i < count; i++)
            {
                Character citizen = citizens.Characters[i];
                if (citizen.ID != ID &&
                    citizen.Location != null)
                {
                    if (NextTo(citizen.Location, location))
                    {
                        result.Add(citizen);
                    }
                }
            }

            return result;
        }

        public static Character? GetCharacter(Location location)
        {
            Map? map = GetMap();
            Layer? bottom_tiles = map?.GetLayer("BottomTiles");

            Tile? tile = bottom_tiles?.GetTile(location.ToVector2);
            if (tile != null)
            {
                Army army = CharacterManager.Armies[0];
                Squad citizens = army.Squads[1];

                Character? character = GetCharacter(citizens.Characters, tile);
                if (character != null)
                {
                    return character;
                }

                if (Handler.Player != null &&
                    Handler.Player.Region != null)
                {
                    float center_x = Handler.Player.Region.X + (Handler.Player.Region.Width / 2);
                    float center_y = Handler.Player.Region.Y + (Handler.Player.Region.Height / 2);

                    if (center_x >= tile.Region.X && center_x < tile.Region.X + tile.Region.Width &&
                        center_y >= tile.Region.Y && center_y < tile.Region.Y + tile.Region.Height)
                    {
                        return Handler.Player;
                    }
                }
            }

            return null;
        }

        public static Character? GetCharacter(List<Character> characters, Location location)
        {
            Map? map = GetMap();
            Layer? bottom_tiles = map?.GetLayer("BottomTiles");

            Tile? tile = bottom_tiles?.GetTile(location.ToVector2);
            if (tile != null)
            {
                Character? character = GetCharacter(characters, tile);
                if (character != null)
                {
                    return character;
                }
            }

            return null;
        }

        public static Character? GetCharacter(List<Character> characters, Tile tile)
        {
            int count = characters.Count;
            for (int i = 0; i < count; i++)
            {
                Character character = characters[i];
                if (character.Region != null)
                {
                    float center_x = character.Region.X + (character.Region.Width / 2);
                    float center_y = character.Region.Y + (character.Region.Height / 2);

                    if (center_x >= tile.Region.X && center_x < tile.Region.X + tile.Region.Width &&
                        center_y >= tile.Region.Y && center_y < tile.Region.Y + tile.Region.Height)
                    {
                        return character;
                    }
                }
            }

            return null;
        }

        public static Character? GetCharacter_Target(Character character)
        {
            Army? army = CharacterManager.GetArmy("Characters");
            if (army != null)
            {
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
            }

            return null;
        }

        public static List<Character> GetAllCharacters(Point map_coords)
        {
            List<Character> characters = [];

            int world_x = map_coords.X * 20;
            int world_y = map_coords.Y * 20;

            Army? army = CharacterManager.GetArmy("Characters");
            Squad? squad = army?.GetSquad("Citizens");

            if (squad != null)
            {
                foreach (Character character in squad.Characters)
                {
                    if (character.Location != null &&
                        character.Location.X >= world_x && character.Location.X < world_x + 20 &&
                        character.Location.Y >= world_y && character.Location.Y < world_y + 20)
                    {
                        characters.Add(character);
                    }
                }
            }

            return characters;
        }

        public static Map? GetRoom(Character character)
        {
            if (character.Location == null)
            {
                return null;
            }

            if (character.Map == null)
            {
                SetCurrentMap(character);
            }

            if (character?.Map != null &&
                WorldGen.Rooms.TryGetValue(character.Map.ID, out List<Map>? rooms))
            {
                int mapCount = rooms.Count;
                for (int m = 0; m < mapCount; m++)
                {
                    Map room = rooms[m];

                    int layerCount = room.Layers.Count;
                    for (int l = 0; l < layerCount; l++)
                    {
                        Layer layer = room.Layers[l];
                        if (layer.Name == "Tiles" &&
                            room.Location != null)
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

        public static List<Tile> GetFurniture_Nearby(Layer middle_tiles, Location location)
        {
            List<Tile> furniture = [];

            int min_y = (int)location.Y - 3;
            int max_y = (int)location.Y + 3;
            int min_x = (int)location.X - 3;
            int max_x = (int)location.X + 3;

            for (int y = min_y; y < max_y; y++)
            {
                for (int x = min_x; x < max_x; x++)
                {
                    Vector2 loc = new(x, y);

                    Tile? tile = middle_tiles.GetTile(loc);
                    if (tile?.Texture != null)
                    {
                        furniture.Add(tile);
                    }
                }
            }

            return furniture;
        }

        public static Tile? GetFurniture(List<Tile> furniture, Location location)
        {
            if (Main.Game == null)
            {
                return null;
            }

            int width = (int)Main.Game.TileSize_X;
            int width_double = width * 2;
            int width_triple = width * 3;

            float loc_x = location.X;
            float loc_y = location.Y;

            Tile[] tiles = furniture.ToArray();

            int count = tiles.Length;
            for (int i = 0; i < count; i++)
            {
                Tile tile = tiles[i];

                Location tile_location = tile.Location;

                float x = tile_location.X;
                float y = tile_location.Y;

                if (loc_x == x &&
                    loc_y == y)
                {
                    return tile;
                }
                else if (tile.Region.Height == width)
                {
                    if (tile.Direction == Direction.East ||
                        tile.Direction == Direction.West)
                    {
                        if (tile.Region.Width == width_double)
                        {
                            if (loc_x >= x && loc_x <= x + 1 &&
                                loc_y == y)
                            {
                                return tile;
                            }
                        }
                    }
                    else if (tile.Direction == Direction.North ||
                             tile.Direction == Direction.South)
                    {
                        if (tile.Region.Width == width_double)
                        {
                            if (loc_x >= x && loc_x <= x + 1 &&
                                loc_y == y)
                            {
                                return tile;
                            }
                        }
                        else if (tile.Region.Width == width_triple)
                        {
                            if (loc_x >= x - 1 && loc_x <= x + 1 &&
                                loc_y == y)
                            {
                                return tile;
                            }
                        }
                    }
                }
                else if (tile.Region.Width == width)
                {
                    if (tile.Direction == Direction.North)
                    {
                        if (tile.Region.Height == width_double)
                        {
                            if (loc_x == x &&
                                loc_y >= y && loc_y <= y - 1)
                            {
                                return tile;
                            }
                        }
                    }
                    else if (tile.Direction == Direction.South)
                    {
                        if (tile.Region.Height == width_double)
                        {
                            if (loc_x == x &&
                                loc_y >= y && loc_y <= y + 1)
                            {
                                return tile;
                            }
                        }
                    }
                    else if (tile.Direction == Direction.West ||
                             tile.Direction == Direction.East)
                    {
                        if (tile.Region.Height == width_double)
                        {
                            if (loc_x == x &&
                                loc_y >= y && loc_y <= y + 1)
                            {
                                return tile;
                            }
                        }
                        else if (tile.Region.Height == width_triple)
                        {
                            if (loc_x == x &&
                                loc_y >= y - 1 && loc_y <= y + 1)
                            {
                                return tile;
                            }
                        }
                    }
                }
                else if (tile.Region.Width == width_double &&
                         tile.Region.Height == width_double)
                {
                    if (loc_x >= x && loc_x <= x + 1 &&
                        loc_y >= y && loc_y <= y + 1)
                    {
                        return tile;
                    }
                }
            }

            return null;
        }

        public static Tile? GetFurniture_Movable(List<Tile> furniture, Location location)
        {
            if (Main.Game == null)
            {
                return null;
            }

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
                    if (existing.Direction == Direction.East)
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
                    else if (existing.Direction == Direction.West)
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
                    else if (existing.Direction == Direction.North ||
                             existing.Direction == Direction.South)
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
                    if (existing.Direction == Direction.North)
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
                    else if (existing.Direction == Direction.South)
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
                    else if (existing.Direction == Direction.West ||
                             existing.Direction == Direction.East)
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

        public static Region? GetSize(Tile tile)
        {
            if (Main.Game == null)
            {
                return null;
            }

            Region size = new()
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
            List<Tile> furniture = [];

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

            return furniture;
        }

        public static List<Tile> GetOwned_Furniture(Character character, string name)
        {
            List<Tile> tiles = [];

            Tile[] furniture = Handler.OwnedFurniture[character.ID].ToArray();

            int count = furniture.Length;
            for (int i = 0; i < count; i++)
            {
                Tile tile = furniture[i];
                if (tile.Name != null &&
                    tile.Name.Contains(name))
                {
                    tiles.Add(tile);
                }
            }

            return tiles;
        }

        public static Direction GetFurnitureDirection(Tile tile, Character character)
        {
            if (tile.Location == null ||
                character.Location == null)
            {
                return Direction.Nowhere;
            }

            if (tile.Location.X > character.Location.X)
            {
                return Direction.East;
            }
            else if (tile.Location.X < character.Location.X)
            {
                return Direction.West;
            }
            else if (tile.Location.X == character.Location.X)
            {
                if (tile.Location.Y > character.Location.Y)
                {
                    return Direction.South;
                }
                else if (tile.Location.Y < character.Location.Y)
                {
                    return Direction.North;
                }
            }

            return Direction.Nowhere;
        }

        public static bool Furniture_InRoom(Tile furniture, Character character)
        {
            Map? room = GetRoom(character);
            if (room?.Location == null)
            {
                return false;
            }

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

                        if (world_x == furniture?.Location.X &&
                            world_y == furniture.Location.Y)
                        {
                            return true;
                        }
                    }

                    break;
                }
            }

            return false;
        }

        public static Tile? GetNearestExit_ToFurniture(Character character, Layer middle_tiles, Tile furniture)
        {
            Map? room = GetRoom(character);
            if (room?.Location == null)
            {
                return null;
            }

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

                    Tile? door = middle_tiles?.GetTile(new Vector2(nearest_world_x, nearest_world_y));
                    if (door?.Name != null &&
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

            return null;
        }

        public static string? GetTile_Name(Tile tile)
        {
            if (tile.Name == null)
            {
                return null;
            }

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

        public static void GenDescription(Character character)
        {
            if (Handler.Player == null)
            {
                return;
            }

            string description = "You see ";
            string he_she = "He";
            string his_her = "His";
            string him_her = "Him";

            List<string> details = [];

            if (character.Gender == "Male")
            {
                description += "a man.";
            }
            else
            {
                he_she = "She";
                his_her = "Her";
                him_her = "Her";

                description += "a woman.";
            }

            description += " " + his_her + " name is " + character.Name + ".";

            if (Handler.Player.Relationships.TryGetValue(character.ID, out string? value))
            {
                description += " " + he_she + " is your " + value.ToLower() + ".";
            }

            if (character.Unconscious)
            {
                description += " " + he_she + " is unconscious.";
            }

            if (character.Dead)
            {
                description += " " + he_she + " is dead.";
            }

            #region Hair

            Item? hair = InventoryUtil.Get_EquippedItem(character, "Hair");
            if (hair?.Name != null)
            {
                details.Add(he_she + " has " + hair.Name.ToLower() + ".");
            }
            else
            {
                details.Add(he_she + " is bald.");
            }

            #endregion

            #region Shoes

            Item? shoes_item = InventoryUtil.Get_EquippedItem(character, "Shoes Slot");
            if (shoes_item?.Name != null)
            {
                string item_description = shoes_item.Name.ToLower();

                CryptoRandom random = new CryptoRandom();
                int variation = random.Next(0, 10);
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
                    details.Add("There are " + item_description + " on " + him_her.ToLower() + ".");
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
                    details.Add(he_she + " has " + item_description + " on " + his_her.ToLower() + " feet.");
                }
                else if (variation == 7)
                {
                    details.Add(he_she + " is wearing " + item_description + " on " + his_her.ToLower() + " feet.");
                }
                else if (variation == 8)
                {
                    details.Add("You see " + item_description + " on " + his_her.ToLower() + " feet.");
                }
                else if (variation == 9)
                {
                    details.Add("There is " + item_description + " on " + his_her.ToLower() + " feet.");
                }
            }

            #endregion

            #region Pants

            Item? pants_item = InventoryUtil.Get_EquippedItem(character, "Pants Slot");
            if (pants_item?.Name != null)
            {
                string item_description = pants_item.Name.ToLower();

                CryptoRandom random = new();
                int variation = random.Next(0, 3);
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
                    details.Add(he_she + " has " + item_description + ".");
                }
            }
            else
            {
                details.Add(he_she + " is not wearing pants.");
            }

            #endregion

            #region Shirt

            Item? shirt_item = InventoryUtil.Get_EquippedItem(character, "Shirt Slot");
            if (shirt_item?.Name != null)
            {
                string item_description;
                if (GameUtil.NameStartsWithVowel(shirt_item.Name))
                {
                    item_description = "an " + shirt_item.Name.ToLower();
                }
                else
                {
                    item_description = "a " + shirt_item.Name.ToLower();
                }

                CryptoRandom random = new();
                int variation = random.Next(0, 6);
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
            }
            else
            {
                details.Add(he_she + " is not wearing a shirt.");
            }

            #endregion

            #region Hat

            Item? hat_item = InventoryUtil.Get_EquippedItem(character, "Hat Slot");
            if (hat_item?.Name != null)
            {
                string item_description;
                if (GameUtil.NameStartsWithVowel(hat_item.Name))
                {
                    item_description = "an " + hat_item.Name.ToLower();
                }
                else
                {
                    item_description = "a " + hat_item.Name.ToLower();
                }

                CryptoRandom random = new();
                int variation = random.Next(0, 10);
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
                    details.Add(he_she + " has " + item_description + " on " + his_her.ToLower() + " head.");
                }
                else if (variation == 7)
                {
                    details.Add(he_she + " is wearing " + item_description + " on " + his_her.ToLower() + " head.");
                }
                else if (variation == 8)
                {
                    details.Add("You see " + item_description + " on " + his_her.ToLower() + " head.");
                }
                else if (variation == 9)
                {
                    details.Add("There is " + item_description + " on " + his_her.ToLower() + " head.");
                }
            }

            #endregion

            #region Left Hand

            Item? left_hand_item = InventoryUtil.Get_EquippedItem(character, "Left Weapon Slot");
            if (left_hand_item?.Name != null)
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

                CryptoRandom random = new();
                int variation = random.Next(0, 4);
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

            Item? right_hand_item = InventoryUtil.Get_EquippedItem(character, "Right Weapon Slot");
            if (right_hand_item?.Name != null)
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

                CryptoRandom random = new();
                int variation = random.Next(0, 4);
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

            Item? mask_item = InventoryUtil.Get_EquippedItem(character, "Mask Slot");
            if (mask_item?.Name != null)
            {
                string item_description;
                if (GameUtil.NameStartsWithVowel(mask_item.Name))
                {
                    item_description = "an " + mask_item.Name.ToLower();
                }
                else
                {
                    item_description = "a " + mask_item.Name.ToLower();
                }

                CryptoRandom random = new();
                int variation = random.Next(0, 6);
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

            Item? backpack_item = InventoryUtil.Get_EquippedItem(character, "Backpack Slot");
            if (backpack_item?.Name != null)
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

                CryptoRandom random = new();
                int variation = random.Next(0, 10);
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
                if (Utility.RandomPercent(character.Stats.Perception))
                {
                    description += " " + detail;
                }
            }

            GameUtil.AddMessage(description);
        }

        public static void GenDescription(Tile tile)
        {
            string description = "You see ";

            bool plural = false;

            string? name = GetTile_Name(tile);
            if (name != null &&
                name.Contains(' '))
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

            GameUtil.AddMessage(description);
        }

        public static void AssignPlayerBed(World world)
        {
            if (Handler.Player == null)
            {
                return;
            }

            Handler.Player.Relationships.Clear();

            Army? army = CharacterManager.GetArmy("Characters");
            Squad? squad = army?.GetSquad("Citizens");

            Map map = world.Maps[0];

            Layer? middle_tiles = map.GetLayer("MiddleTiles");
            if (middle_tiles == null)
            {
                return;
            }

            Layer? top_tiles = map.GetLayer("TopTiles");
            if (top_tiles == null)
            {
                return;
            }

            CryptoRandom random = new();
            List<Map> homes = WorldGen.Residential.OrderBy(a => random.Next()).ToList();

            Tile? bed = null;
            Map? newHome = null;
            Character? replacement = null;
            Character? mate = null;

            List<Tile> middle_furniture = [];

            //Start player in single home
            foreach (Map home in homes)
            {
                if (home.Location == null)
                {
                    continue;
                }

                Point map_coords = new((int)home.Location.X, (int)home.Location.Y);

                middle_furniture = GetAllFurniture(middle_tiles, map_coords);

                int bed_count = 0;
                Tile? possibleBed = null;

                foreach (Tile tile in middle_furniture)
                {
                    if (tile.Name != null &&
                        tile.Name.Contains("Bed"))
                    {
                        possibleBed = tile;
                        bed_count++;
                    }
                }

                if (bed_count == 1)
                {
                    Location? bed_location = null;

                    if (possibleBed?.Direction == Direction.North)
                    {
                        bed_location = new Location(possibleBed.Location.X, possibleBed.Location.Y + 1, 0);
                    }
                    else if (possibleBed?.Direction == Direction.East ||
                             possibleBed?.Direction == Direction.South)
                    {
                        bed_location = new Location(possibleBed.Location.X, possibleBed.Location.Y, 0);
                    }
                    else if (possibleBed?.Direction == Direction.West)
                    {
                        bed_location = new Location(possibleBed.Location.X + 1, possibleBed.Location.Y, 0);
                    }

                    if (bed_location != null)
                    {
                        Character? character = GetCharacter(bed_location);
                        if (character != null)
                        {
                            replacement = character;
                            bed = possibleBed;
                            newHome = home;
                            break;
                        }
                    }
                }
            }

            if (bed == null)
            {
                //Start player next to an NPC
                foreach (Map home in homes)
                {
                    if (home.Location == null)
                    {
                        continue;
                    }

                    Point map_coords = new((int)home.Location.X, (int)home.Location.Y);

                    middle_furniture = GetAllFurniture(middle_tiles, map_coords);
                    List<Tile> top_furniture = GetAllFurniture(top_tiles, map_coords);

                    int bed_count = 0;
                    Tile? possibleBed = null;

                    foreach (Tile tile in middle_furniture)
                    {
                        if (tile.Name != null &&
                            tile.Name.Contains("DoubleBed"))
                        {
                            possibleBed = tile;
                            bed_count++;
                        }
                    }

                    if (bed_count >= 1)
                    {
                        Location? bed_location = null;

                        if (possibleBed?.Direction == Direction.North)
                        {
                            bed_location = new Location(possibleBed.Location.X, possibleBed.Location.Y + 1, 0);
                        }
                        else if (possibleBed?.Direction == Direction.East ||
                                 possibleBed?.Direction == Direction.South)
                        {
                            bed_location = new Location(possibleBed.Location.X, possibleBed.Location.Y, 0);
                        }
                        else if (possibleBed?.Direction == Direction.West)
                        {
                            bed_location = new Location(possibleBed.Location.X + 1, possibleBed.Location.Y, 0);
                        }

                        if (bed_location != null)
                        {
                            Character? character = GetCharacter(bed_location);
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
            }

            if (bed == null)
            {
                //Start player in any open bed
                foreach (Map home in homes)
                {
                    if (home.Location == null)
                    {
                        continue;
                    }

                    Point map_coords = new((int)home.Location.X, (int)home.Location.Y);

                    middle_furniture = GetAllFurniture(middle_tiles, map_coords);

                    foreach (Tile tile in middle_furniture)
                    {
                        if (tile.Name != null &&
                            tile.Name.Contains("DoubleBed"))
                        {
                            bed = tile;
                            break;
                        }
                    }

                    if (bed == null)
                    {
                        foreach (Tile tile in middle_furniture)
                        {
                            if (tile.Name != null &&
                                tile.Name.Contains("Bed"))
                            {
                                bed = tile;
                                break;
                            }
                        }
                    }

                    if (bed != null)
                    {
                        newHome = home;

                        Location? bed_location = null;

                        if (bed.Direction == Direction.North)
                        {
                            bed_location = new Location(bed.Location.X, bed.Location.Y + 1, 0);
                        }
                        else if (bed.Direction == Direction.East ||
                                 bed.Direction == Direction.South)
                        {
                            bed_location = new Location(bed.Location.X, bed.Location.Y, 0);
                        }
                        else if (bed.Direction == Direction.West)
                        {
                            bed_location = new Location(bed.Location.X + 1, bed.Location.Y, 0);
                        }

                        if (bed_location != null)
                        {
                            Character? character = GetCharacter(bed_location);
                            if (character != null)
                            {
                                replacement = character;
                                break;
                            }
                        }
                    }
                }
            }

            if (bed != null)
            {
                if (replacement != null &&
                    replacement.Location != null)
                {
                    Handler.OwnedFurniture.Remove(replacement.ID);
                    squad?.Characters.Remove(replacement);

                    Handler.Player.Location = new Location(replacement.Location.X, replacement.Location.Y, 0);
                }
                else if (mate != null &&
                         mate.Location != null)
                {
                    if (mate.Direction == Direction.North ||
                        mate.Direction == Direction.South)
                    {
                        Handler.Player.Location = new Location(mate.Location.X + 1, mate.Location.Y, 0);
                    }
                    else if (mate.Direction == Direction.East ||
                             mate.Direction == Direction.West)
                    {
                        Handler.Player.Location = new Location(mate.Location.X, mate.Location.Y + 1, 0);
                    }
                }
                else
                {
                    if (bed.Direction == Direction.North)
                    {
                        Handler.Player.Location = new Location(bed.Location.X, bed.Location.Y + 1, 0);
                    }
                    else if (bed.Direction == Direction.East ||
                             bed.Direction == Direction.South)
                    {
                        Handler.Player.Location = new Location(bed.Location.X, bed.Location.Y, 0);
                    }
                    else if (bed.Direction == Direction.West)
                    {
                        Handler.Player.Location = new Location(bed.Location.X + 1, bed.Location.Y, 0);
                    }
                }

                Handler.Player.Direction = bed.Direction;
                switch (bed.Direction)
                {
                    case Direction.North:
                        Handler.Player.FaceNorth();
                        break;

                    case Direction.East:
                        Handler.Player.FaceEast();
                        break;

                    case Direction.South:
                        Handler.Player.FaceSouth();
                        break;

                    case Direction.West:
                        Handler.Player.FaceWest();
                        break;
                }
                CharacterUtil.UpdateGear(Handler.Player);

                if (!Handler.OwnedFurniture.TryGetValue(Handler.Player.ID, out List<Tile>? ownedFurniture))
                {
                    ownedFurniture = [];
                    Handler.OwnedFurniture.Add(Handler.Player.ID, ownedFurniture);
                }

                foreach (Tile tile in middle_furniture)
                {
                    ownedFurniture.Add(tile);
                }

                string? last_name = Handler.Player.Name?.Split(' ')[1].Trim();

                if (newHome?.Location != null)
                {
                    Point map_coords = new((int)newHome.Location.X, (int)newHome.Location.Y);
                    List<Tile> top_furniture = GetAllFurniture(top_tiles, map_coords);

                    foreach (Tile tile in top_furniture)
                    {
                        ownedFurniture.Add(tile);
                    }

                    List<Character> characters = GetAllCharacters(map_coords);
                    foreach (Character existing in characters)
                    {
                        if (!Handler.Player.Relationships.ContainsKey(existing.ID) &&
                            existing.Name != null)
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
                                        Handler.Player.Relationships.Add(existing.ID, "Husband");
                                    }
                                    else
                                    {
                                        Handler.Player.Relationships.Add(existing.ID, "Boyfriend");
                                    }
                                }
                                else
                                {
                                    if (relative == 0)
                                    {
                                        Handler.Player.Relationships.Add(existing.ID, "Wife");
                                    }
                                    else
                                    {
                                        Handler.Player.Relationships.Add(existing.ID, "Girlfriend");
                                    }
                                }
                            }
                            else
                            {
                                if (existing.Gender == "Male")
                                {
                                    if (relative == 0)
                                    {
                                        Handler.Player.Relationships.Add(existing.ID, "Father");
                                    }
                                    else
                                    {
                                        Handler.Player.Relationships.Add(existing.ID, "Brother");
                                    }
                                }
                                else
                                {
                                    if (relative == 0)
                                    {
                                        Handler.Player.Relationships.Add(existing.ID, "Mother");
                                    }
                                    else
                                    {
                                        Handler.Player.Relationships.Add(existing.ID, "Sister");
                                    }
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

        public static Tile? StandingByFurniture(Layer tiles, Location location, string type)
        {
            foreach (Tile tile in tiles.Tiles)
            {
                if (tile.Name != null &&
                    tile.Name.Contains(type))
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
            if (character.Location == null)
            {
                return false;
            }

            Vector2 tile_location = character.Location.ToVector2;
            if (character.Direction == Direction.North)
            {
                tile_location.Y++;
            }
            else if (character.Direction == Direction.East)
            {
                tile_location.X--;
            }
            else if (character.Direction == Direction.South)
            {
                tile_location.Y--;
            }
            else if (character.Direction == Direction.West)
            {
                tile_location.X++;
            }

            Tile? tile = middle_tiles.GetTile(tile_location);
            if (tile != null &&
                tile.Name != null)
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
            if (character.Location == null)
            {
                return false;
            }

            Vector2 tile_location = character.Location.ToVector2;
            if (character.Direction == Direction.North)
            {
                tile_location.Y++;
            }
            else if (character.Direction == Direction.East)
            {
                tile_location.X--;
            }
            else if (character.Direction == Direction.South)
            {
                tile_location.Y--;
            }
            else if (character.Direction == Direction.West)
            {
                tile_location.X++;
            }

            Tile? tile = middle_tiles.GetTile(tile_location);
            if (tile != null &&
                tile.Name != null)
            {
                if (tile.Name.Contains("Open") &&
                    tile.Name.Contains("Window"))
                {
                    return true;
                }
            }

            return false;
        }

        public static Map? GetCurrentMap(Character character)
        {
            if (character.Map == null)
            {
                SetCurrentMap(character);
            }

            foreach (Map map in WorldGen.Residential)
            {
                if (map.ID == character.Map?.ID)
                {
                    return map;
                }
            }

            foreach (Map map in WorldGen.Commercial)
            {
                if (map.ID == character.Map?.ID)
                {
                    return map;
                }
            }

            foreach (Map map in WorldGen.Parks)
            {
                if (map.ID == character.Map?.ID)
                {
                    return map;
                }
            }

            foreach (Map map in WorldGen.Roads)
            {
                if (map.ID == character.Map?.ID)
                {
                    return map;
                }
            }

            return null;
        }

        public static void SetCurrentMap(Character character)
        {
            if (character.Location == null)
            {
                return;
            }

            int block_x = (int)character.Location.X / 20;
            int block_y = (int)character.Location.Y / 20;

            int count = WorldGen.Residential.Count;
            for (int i = 0; i < count; ++i)
            {
                Map map = WorldGen.Residential[i];
                if (map.Location != null &&
                    map.Location.X == block_x &&
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
                if (map.Location != null &&
                    map.Location.X == block_x &&
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
                if (map.Location != null &&
                    map.Location.X == block_x &&
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
                if (map.Location != null &&
                    map.Location.X == block_x &&
                    map.Location.Y == block_y)
                {
                    character.Map = map;
                    return;
                }
            }
        }

        public static void UpdateWorldMap()
        {
            if (Main.Game == null ||
                !Handler.WorldMap_Visible ||
                Handler.Player?.Location == null ||
                Handler.Player.Region == null)
            {
                return;
            }

            int world_x = (int)Handler.Player.Location.X / 20;
            int world_y = (int)Handler.Player.Location.Y / 20;

            Tile? center = null;
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

        public static void AddEffect(Vector3 location, string name, string? texture)
        {
            Map? map = GetMap();
            if (map != null)
            {
                Layer? bottom_tiles = map.GetLayer("BottomTiles");

                Tile? bottom_tile = bottom_tiles?.GetTile(new Vector2(location.X, location.Y));
                if (bottom_tile != null)
                {
                    Layer? effect_tiles = map?.GetLayer("EffectTiles");

                    Texture2D? texture2D = null;
                    if (texture != null)
                    {
                        texture2D = Handler.GetTexture(texture);
                    }

                    if (texture2D != null)
                    {
                        effect_tiles?.Tiles.Add(new Tile
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
                    else
                    {
                        effect_tiles?.Tiles.Add(new Tile
                        {
                            ID = Handler.GetID(),
                            Map = map,
                            Layer = effect_tiles,
                            Name = name,
                            Location = new Location(location.X, location.Y, location.Z),
                            Region = bottom_tile.Region
                        });
                    }
                }
            }
        }

        public static void AddEffect_Animated(Vector3 location, Direction direction, string texture, TimeSpan start_time, int duration)
        {
            Scene? scene = SceneManager.GetScene("Gameplay");
            Map? map = scene?.World?.Maps[0];
            if (map != null)
            {
                Layer? bottom_tiles = map.GetLayer("BottomTiles");

                Tile? bottom_tile = bottom_tiles?.GetTile(new Vector2(location.X, location.Y));
                if (bottom_tile != null)
                {
                    Layer? effect_tiles = map.GetLayer("EffectTiles");

                    Texture2D? texture2D = null;
                    if (texture != null)
                    {
                        texture2D = Handler.GetTexture(texture);
                    }

                    if (texture2D == null)
                    {
                        return;
                    }

                    effect_tiles?.Tiles.Add(new Tile
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
            if (TimeManager.Now == null ||
                tile.Texture == null)
            {
                return;
            }

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
                var rotation = tile.Direction switch
                {
                    Direction.East => 1.5f,
                    Direction.South => 3f,
                    Direction.West => 4.5f,
                    _ => 0f,
                };

                Rectangle region = new((int)(tile.Region.X + (tile.Region.Width / 2)), (int)(tile.Region.Y + (tile.Region.Height / 2)), (int)tile.Region.Width, (int)tile.Region.Height);
                spriteBatch.Draw(tile.Texture, region, tile.Image, Color.White, rotation, new Vector2(tile.Texture.Height / 2, tile.Texture.Height / 2), SpriteEffects.None, 0);
            }
        }
    }
}

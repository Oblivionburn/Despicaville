using System.Collections.Generic;
using Microsoft.Xna.Framework;
using OP_Engine.Characters;
using OP_Engine.Utility;
using OP_Engine.Tiles;
using OP_Engine.Enums;

namespace Despicaville.Util
{
    public static class Pathing
    {
        private static List<Character> nearby_characters = [];

        public static List<ALocation> GetPath(Layer bottom_tiles, Layer middle_tiles, Character character, Location target, int? max_distance, bool stop_next_to_tile)
        {
            List<ALocation> result = [];

            if (character.Location == null)
            {
                return result;
            }

            List<ALocation> blocks = GetPath_Blocks(character, target);
            if (blocks.Count > 1)
            {
                //Pathing to next block
                ALocation block = blocks[0];
                ALocation next_block = blocks[1];

                List<Map> rooms = GetPath_Rooms_NextBlock(block, next_block, character.Location);
                if (rooms.Count > 1)
                {
                    //Pathing to next room
                    Map room = rooms[0];
                    Map next_room = rooms[1];

                    List<Location> exits = GetExits_ToRoom(room, next_room);
                    if (exits.Count > 0)
                    {
                        Location? exit = WorldUtil.GetClosestLocation(exits, character.Location);
                        if (exit != null)
                        {
                            result = GetPath_Tiles_NextRoom(bottom_tiles, middle_tiles, character, room, next_room, exit, max_distance, false);
                        }
                    }
                }
                else
                {
                    //Pathing to room exit in next block
                    List<Location> exits = GetExits_ToBlock(block, next_block);
                    if (exits.Count > 0)
                    {
                        Location? exit = WorldUtil.GetClosestLocation(exits, character.Location);
                        if (exit != null)
                        {
                            result = GetPath_Tiles(bottom_tiles, middle_tiles, character, exit, max_distance, false);
                        }
                    }
                }
            }
            else
            {
                //Pathing in current block
                List<Map> rooms = GetPath_Rooms_SameBlock(character, target);
                if (rooms.Count > 1)
                {
                    //Pathing to next room
                    Map room = rooms[0];
                    Map next_room = rooms[1];

                    List<Location> exits = GetExits_ToRoom(room, next_room);
                    if (exits.Count > 0)
                    {
                        Location? exit = WorldUtil.GetClosestLocation(exits, character.Location);
                        if (exit != null)
                        {
                            result = GetPath_Tiles_NextRoom(bottom_tiles, middle_tiles, character, room, next_room, exit, max_distance, false);
                        }
                    }
                }
                else
                {
                    //Pathing in current room
                    result = GetPath_Tiles(bottom_tiles, middle_tiles, character, target, max_distance, stop_next_to_tile);
                }
            }

            return result;
        }

        #region Block Pathing

        private static List<ALocation> GetPath_Blocks(Character character, Location target_location)
        {
            if (character.Location == null)
            {
                return [];
            }

            int start_block_x = (int)character.Location.X / 20;
            int start_block_y = (int)character.Location.Y / 20;

            int target_block_x = (int)target_location.X / 20;
            int target_block_y = (int)target_location.Y / 20;

            ALocation target = new(target_block_x, target_block_y);

            ALocation start = new(start_block_x, start_block_y);
            List<ALocation> path = [start];
            
            List<ALocation> open = [];

            ALocation? previous_block = null;
            ALocation current_block = start;

            for (int i = 0; i < 10; i++)
            {
                if (ReachedTarget_Block(current_block, target))
                {
                    return path;
                }

                if (current_block != null)
                {
                    List<ALocation> locations = GetBlocks(previous_block, current_block, target, character);

                    int count = locations.Count;
                    for (int l = 0; l < count; l++)
                    {
                        ALocation location = locations[l];

                        if (!HasLocation(path, location))
                        {
                            int? distance_ToStart = WorldUtil.GetDistance(new Location(location.X, location.Y, 0), new Location(start.X, start.Y, 0));
                            if (distance_ToStart != null)
                            {
                                location.Distance_ToStart = (int)distance_ToStart;
                            }

                            int? distance_ToDestination = WorldUtil.GetDistance(new Location(location.X, location.Y, 0), new Location(target.X, target.Y, 0));
                            if (distance_ToDestination != null)
                            {
                                location.Distance_ToDestination = (int)distance_ToDestination;
                            }
                            
                            location.Parent = current_block;
                            open.Add(location);
                        }
                    }

                    if (open.Count > 0)
                    {
                        ALocation? next_block = Get_MinLocation_Target(open);
                        if (next_block != null)
                        {
                            open.Clear();
                            path.Add(next_block);

                            previous_block = current_block;
                            current_block = next_block;
                        }
                    }
                    else if (current_block.Parent != null)
                    {
                        current_block = current_block.Parent;
                    }
                }
                else
                {
                    break;
                }
            }

            return [];
        }

        private static List<ALocation> GetBlocks(ALocation? previous_block, ALocation current_block, ALocation target, Character character)
        {
            List<ALocation> result = [];

            if (CanExitBlock(previous_block, current_block, target, character, Direction.North))
            {
                result.Add(new ALocation(current_block.X, current_block.Y - 1));
            }

            if (CanExitBlock(previous_block, current_block, target, character, Direction.East))
            {
                result.Add(new ALocation(current_block.X + 1, current_block.Y));
            }

            if (CanExitBlock(previous_block, current_block, target, character, Direction.South))
            {
                result.Add(new ALocation(current_block.X, current_block.Y + 1));
            }

            if (CanExitBlock(previous_block, current_block, target, character, Direction.West))
            {
                result.Add(new ALocation(current_block.X - 1, current_block.Y));
            }

            return result;
        }

        private static bool CanExitBlock(ALocation? previous_block, ALocation current_block, ALocation target_block, Character character, Direction direction)
        {
            Map? worldTile = WorldUtil.GetWorldTile(new Location(current_block.X, current_block.Y));
            if (worldTile?.Location == null ||
                worldTile.Type == null)
            {
                return false;
            }

            ALocation? next_location = null;

            switch (direction)
            {
                case Direction.North:
                    next_location = new(current_block.X, current_block.Y - 1);
                    break;
                case Direction.East:
                    next_location = new(current_block.X + 1, current_block.Y);
                    break;
                case Direction.South:
                    next_location = new(current_block.X, current_block.Y + 1);
                    break;
                case Direction.West:
                    next_location = new(current_block.X - 1, current_block.Y);
                    break;
            }

            if (next_location == null)
            {
                return false;
            }

            Map? next_block = null;

            int count = WorldGen.Worldmap.Count;
            for (int i = 0; i < count; i++)
            {
                Map map = WorldGen.Worldmap[i];
                if (map.Location != null &&
                    map.Location.X == next_location.X &&
                    map.Location.Y == next_location.Y)
                {
                    next_block = map;
                    break;
                }
            }

            if (next_block?.Type == null)
            {
                return false;
            }

            if (!next_block.Type.Contains("Road"))
            {
                //Stick to the roads unless we reached the destination
                if (next_location.X != target_block.X ||
                    next_location.Y != target_block.Y)
                {
                    return false;
                }
            }

            Location? location;

            if (previous_block != null)
            {
                //Get an entrance point for the block
                location = GetExit_ToBlock(previous_block, current_block);
            }
            else
            {
                location = character.Location;
            }

            if (location != null)
            {
                //Can we traverse across the block to an exit?
                List<Map> path = GetPath_Rooms_NextBlock(current_block, next_location, location);
                if (path.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ReachedTarget_Block(ALocation location, ALocation target)
        {
            if (location.X == target.X &&
                location.Y == target.Y)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Room Pathing

        private static List<Map> GetPath_Rooms_SameBlock(Character character, Location target_location)
        {
            if (character.Location == null)
            {
                return [];
            }

            List<Map> path = [];
            ALocation target = new((int)target_location.X, (int)target_location.Y);

            Map? start = WorldUtil.GetRoom(character);
            if (start == null)
            {
                return [];
            }
            path.Add(start);
            Map last_min = start;

            List<Map> open = [];

            for (int i = 0; i < 10; i++)
            {
                if (ReachedTarget_Room(last_min, target))
                {
                    return path;
                }

                if (last_min != null)
                {
                    List<Map> rooms = GetRooms(last_min);

                    int count = rooms.Count;
                    for (int r = 0; r < count; r++)
                    {
                        Map room = rooms[r];

                        if (!HasRoom(path, room))
                        {
                            open.Add(room);
                        }
                    }

                    if (open.Count > 0)
                    {
                        Map? min = Get_MinRoom(open, target_location);
                        if (min != null)
                        {
                            open.Clear();
                            path.Add(min);
                            last_min = min;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            return [];
        }

        private static List<Map> GetPath_Rooms_NextBlock(ALocation block, ALocation next_block, Location start_location)
        {
            ALocation target = new((int)start_location.X, (int)start_location.Y);

            Map? start = GetRoom_ToBlock(block, next_block);
            if (start == null)
            {
                return [];
            }
            List<Map> path = [start];

            List<Map> open = [];
            Map last_min = start;

            for (int i = 0; i < 100; i++)
            {
                if (ReachedTarget_Room(last_min, target))
                {
                    path.Reverse();
                    return path;
                }

                List<Map> rooms = GetRooms(last_min);

                int count = rooms.Count;
                for (int r = 0; r < count; r++)
                {
                    Map room = rooms[r];

                    if (!HasRoom(path, room))
                    {
                        open.Add(room);
                    }
                }

                if (open.Count > 0)
                {
                    Map? min = Get_MinRoom(open, start_location);
                    if (min != null)
                    {
                        open.Clear();
                        path.Add(min);
                        last_min = min;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            return [];
        }

        private static List<Map> GetRooms(Map room)
        {
            List<Map> result = [];

            if (room.World == null)
            {
                return result;
            }

            List<Map> rooms = WorldGen.Rooms[room.World.ID];
            int roomCount = rooms.Count;

            //Check for other rooms connected to current room
            for (int r = 0; r < roomCount; r++)
            {
                Map next_room = rooms[r];
                if (next_room.ID != room.ID)
                {
                    List<Location> exits = GetExits_ToRoom(room, next_room);
                    if (exits.Count > 0)
                    {
                        result.Add(next_room);
                    }
                }
            }

            return result;
        }

        private static List<Location> GetExits_ToRoom(Map room, Map next_room)
        {
            List<Location> result = [];

            Layer? exits = room.GetLayer("Exits");
            if (exits != null &&
                room.Location != null)
            {
                int count = exits.Tiles.Count;
                for (int i = 0; i < count; i++)
                {
                    Tile exit = exits.Tiles[i];
                    if (exit.Location == null)
                    {
                        continue;
                    }

                    //Convert exit location to world coordinates
                    int x = (int)(exit.Location.X + (room.Location.X * 20));
                    int y = (int)(exit.Location.Y + (room.Location.Y * 20));

                    //Is the current exit tile overlapping some other room's exit tile?
                    Layer? other_exits = next_room.GetLayer("Exits");
                    if (other_exits != null)
                    {
                        int exitCount = other_exits.Tiles.Count;
                        for (int e = 0; e < exitCount; e++)
                        {
                            Tile other_exit = other_exits.Tiles[e];
                            if (other_exit.Location == null ||
                                next_room.Location == null)
                            {
                                continue;
                            }

                            //Convert other exit location to world coordinates
                            int other_x = (int)(other_exit.Location.X + (next_room.Location.X * 20));
                            int other_y = (int)(other_exit.Location.Y + (next_room.Location.Y * 20));

                            if (x == other_x &&
                                y == other_y)
                            {
                                result.Add(new Location(other_x, other_y));
                            }
                        }
                    }

                    //Is the current exit tile overlapping some other room's tile?
                    Layer? tiles = next_room.GetLayer("Tiles");
                    if (tiles != null)
                    {
                        int tileCount = tiles.Tiles.Count;
                        for (int e = 0; e < tileCount; e++)
                        {
                            Tile tile = tiles.Tiles[e];
                            if (tile.Location == null ||
                                next_room.Location == null)
                            {
                                continue;
                            }

                            //Convert tile location to world coordinates
                            int other_x = (int)(tile.Location.X + (next_room.Location.X * 20));
                            int other_y = (int)(tile.Location.Y + (next_room.Location.Y * 20));

                            if (x == other_x &&
                                y == other_y)
                            {
                                result.Add(new Location(other_x, other_y));
                            }
                        }
                    }
                }
            }

            return result;
        }

        private static Location? GetExit_ToBlock(ALocation block, ALocation next_block)
        {
            //Convert next block location to world coordinates
            int next_block_min_x = next_block.X * 20;
            int next_block_min_y = next_block.Y * 20;
            int next_block_max_x = next_block_min_x + 20;
            int next_block_max_y = next_block_min_y + 20;

            Map? worldTile = WorldUtil.GetWorldTile(new Location(block.X, block.Y));
            if (worldTile == null)
            {
                return null;
            }

            List<Map> rooms = WorldGen.Rooms[worldTile.ID];
            int count = rooms.Count;

            for (int i = 0; i < count; i++)
            {
                Map room = rooms[i];
                if (room.Location == null)
                {
                    continue;
                }

                Layer? exits = room.GetLayer("Exits");
                if (exits == null)
                {
                    continue;
                }

                int exitCount = exits.Tiles.Count;
                for (int e = 0; e < exitCount; e++)
                {
                    Tile exit = exits.Tiles[e];
                    if (exit.Location == null)
                    {
                        continue;
                    }

                    //Convert exit location to world coordinates
                    int x = (int)(exit.Location.X + (room.Location.X * 20));
                    int y = (int)(exit.Location.Y + (room.Location.Y * 20));

                    //Is the room exit within the next block?
                    if (x >= next_block_min_x && x < next_block_max_x &&
                        y >= next_block_min_y && y < next_block_max_y)
                    {
                        return new Location(x, y);
                    }
                }
            }

            return null;
        }

        private static List<Location> GetExits_ToBlock(ALocation block, ALocation next_block)
        {
            List<Location> result = [];

            //Convert next block location to world coordinates
            int next_block_min_x = next_block.X * 20;
            int next_block_min_y = next_block.Y * 20;
            int next_block_max_x = next_block_min_x + 20;
            int next_block_max_y = next_block_min_y + 20;

            Map? worldTile = WorldUtil.GetWorldTile(new Location(block.X, block.Y));
            if (worldTile == null)
            {
                return result;
            }

            List<Map> rooms = WorldGen.Rooms[worldTile.ID];
            int count = rooms.Count;

            for (int i = 0; i < count; i++)
            {
                Map room = rooms[i];
                if (room.Location == null)
                {
                    continue;
                }

                Layer? exits = room.GetLayer("Exits");
                if (exits == null)
                {
                    continue;
                }

                int exitCount = exits.Tiles.Count;
                for (int e = 0; e < exitCount; e++)
                {
                    Tile exit = exits.Tiles[e];
                    if (exit.Location == null)
                    {
                        continue;
                    }

                    //Convert exit location to world coordinates
                    int x = (int)(exit.Location.X + (room.Location.X * 20));
                    int y = (int)(exit.Location.Y + (room.Location.Y * 20));

                    //Is the room exit within the next block?
                    if (x >= next_block_min_x && x < next_block_max_x &&
                        y >= next_block_min_y && y < next_block_max_y)
                    {
                        result.Add(new Location(x, y));
                    }
                }
            }

            return result;
        }

        private static Map? GetRoom_ToBlock(ALocation block, ALocation next_block)
        {
            //Convert next block location to world coordinates
            int next_block_min_x = next_block.X * 20;
            int next_block_min_y = next_block.Y * 20;
            int next_block_max_x = next_block_min_x + 20;
            int next_block_max_y = next_block_min_y + 20;

            Map? worldTile = WorldUtil.GetWorldTile(new Location(block.X, block.Y));
            if (worldTile == null)
            {
                return null;
            }

            List<Map> rooms = WorldGen.Rooms[worldTile.ID];
            int count = rooms.Count;

            for (int i = 0; i < count; i++)
            {
                Map room = rooms[i];
                if (room.Location == null)
                {
                    continue;
                }

                Layer? exits = room.GetLayer("Exits");
                if (exits == null)
                {
                    continue;
                }

                int exitCount = exits.Tiles.Count;
                for (int e = 0; e < exitCount; e++)
                {
                    Tile exit = exits.Tiles[e];
                    if (exit.Location == null)
                    {
                        continue;
                    }

                    //Convert exit location to world coordinates
                    int x = (int)(exit.Location.X + (room.Location.X * 20));
                    int y = (int)(exit.Location.Y + (room.Location.Y * 20));

                    //Is the room exit within the next block?
                    if (x >= next_block_min_x && x < next_block_max_x &&
                        y >= next_block_min_y && y < next_block_max_y)
                    {
                        return room;
                    }
                }
            }

            return null;
        }

        private static bool HasRoom(List<Map> rooms, Map room)
        {
            int count = rooms.Count;
            for (int i = 0; i < count; i++)
            {
                Map existing = rooms[i];
                if (existing.ID == room.ID)
                {
                    return true;
                }
            }

            return false;
        }

        private static Map? Get_MinRoom(List<Map> rooms, Location target)
        {
            Map room = rooms[0];

            Layer? exits = room.GetLayer("Exits");
            if (exits == null)
            {
                return null;
            }

            Tile? exit = WorldUtil.GetClosestTile(exits.Tiles, target, true);
            if (exit?.Location == null ||
                room.Location == null)
            {
                return null;
            }

            float x = exit.Location.X + (room.Location.X * 20);
            float y = exit.Location.Y + (room.Location.Y * 20);

            int? distance = WorldUtil.GetDistance(new Location(x, y), target);

            int count = rooms.Count;
            for (int i = 0; i < count; i++)
            {
                Map other_room = rooms[i];
                if (other_room.Location == null)
                {
                    continue;
                }

                if (other_room.ID != room.ID)
                {
                    Layer? other_exits = other_room.GetLayer("Exits");
                    if (other_exits == null)
                    {
                        return null;
                    }

                    Tile? other_exit = WorldUtil.GetClosestTile(other_exits.Tiles, target, true);
                    if (other_exit?.Location == null)
                    {
                        return null;
                    }

                    float other_x = other_exit.Location.X + (other_room.Location.X * 20);
                    float other_y = other_exit.Location.Y + (other_room.Location.Y * 20);

                    int? other_distance = WorldUtil.GetDistance(new Location(other_x, other_y), target);

                    if (other_distance < distance)
                    {
                        room = other_room;
                        distance = other_distance;
                    }
                }
            }

            return room;
        }

        private static bool ReachedTarget_Room(Map room, ALocation target)
        {
            if (room.Location == null)
            {
                return false;
            }

            Layer? tiles = room.GetLayer("Tiles");
            if (tiles == null)
            {
                return false;
            }

            int count = tiles.Tiles.Count;
            for (int i = 0; i < count; i++)
            {
                Tile tile = tiles.Tiles[i];
                if (tile.Location == null)
                {
                    continue;
                }

                float x = tile.Location.X + (room.Location.X * 20);
                float y = tile.Location.Y + (room.Location.Y * 20);

                if (x == target.X &&
                    y == target.Y)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Tile Pathing

        private static List<ALocation> GetPath_Tiles(Layer bottom_tiles, Layer middle_tiles, Character character, Location target_location, int? max_distance, bool stop_next_to_tile)
        {
            List<ALocation> result = [];

            if (character?.Location == null)
            {
                return result;
            }

            ALocation target = new((int)target_location.X, (int)target_location.Y);

            ALocation start = new((int)character.Location.X, (int)character.Location.Y);
            List<ALocation> path = [start];

            List<ALocation> open = [];
            ALocation last_min = start;

            bool reached = false;
            for (int i = 0; i < max_distance; i++)
            {
                if (last_min != null)
                {
                    if (DestinationReached(last_min, target_location, stop_next_to_tile))
                    {
                        reached = true;
                        break;
                    }

                    if (i == 0)
                    {
                        nearby_characters = WorldUtil.GetNearbyCharacters(character.ID, new Location(last_min.X, last_min.Y, 0));
                    }
                    else
                    {
                        nearby_characters.Clear();
                    }

                    List<ALocation> locations = GetLocations(bottom_tiles, middle_tiles, last_min);

                    int count = locations.Count;
                    for (int l = 0; l < count; l++)
                    {
                        ALocation location = locations[l];

                        if (!HasLocation(path, location))
                        {
                            int? distance_ToStart = WorldUtil.GetDistance(new Location(location.X, location.Y, 0), new Location(start.X, start.Y, 0));
                            if (distance_ToStart != null)
                            {
                                location.Distance_ToStart = (int)distance_ToStart;
                            }

                            int? distance_ToDestination = WorldUtil.GetDistance(new Location(location.X, location.Y, 0), new Location(target.X, target.Y, 0));
                            if (distance_ToDestination != null)
                            {
                                location.Distance_ToDestination = (int)distance_ToDestination;
                            }

                            location.Parent = last_min;
                            open.Add(location);
                        }
                    }

                    if (open.Count > 0)
                    {
                        ALocation? min = Get_MinLocation_Target(open);
                        if (min != null)
                        {
                            open.Clear();
                            path.Add(min);
                            last_min = min;
                        }
                    }
                    else if (last_min.Parent != null)
                    {
                        last_min = last_min.Parent;
                    }
                }
                else
                {
                    break;
                }
            }

            if (reached)
            {
                result = Optimize_Path(path, start);
                result.Reverse();
            }

            return result;
        }

        private static List<ALocation> GetPath_Tiles_NextRoom(Layer bottom_tiles, Layer middle_tiles, Character character, Map room, Map next_room, Location exit_location, int? max_distance, bool stop_next_to_tile)
        {
            List<ALocation> result = [];

            if (character?.Location == null)
            {
                return result;
            }

            ALocation target = new((int)exit_location.X, (int)exit_location.Y);

            ALocation start = new((int)character.Location.X, (int)character.Location.Y);
            List<ALocation> path = [start];

            List<ALocation> open = [];
            ALocation last_min = start;

            bool reached = false;
            for (int i = 0; i < max_distance; i++)
            {
                if (last_min != null)
                {
                    if (DestinationReached(last_min, exit_location, stop_next_to_tile))
                    {
                        reached = true;
                        break;
                    }

                    if (i == 0)
                    {
                        nearby_characters = WorldUtil.GetNearbyCharacters(character.ID, new Location(last_min.X, last_min.Y, 0));
                    }
                    else
                    {
                        nearby_characters.Clear();
                    }

                    List<ALocation> locations = GetLocations_NextRoom(room, next_room, bottom_tiles, middle_tiles, target, last_min);

                    int count = locations.Count;
                    for (int l = 0; l < count; l++)
                    {
                        ALocation location = locations[l];

                        if (!HasLocation(path, location))
                        {
                            int? distance_ToStart = WorldUtil.GetDistance(new Location(location.X, location.Y, 0), new Location(start.X, start.Y, 0));
                            if (distance_ToStart != null)
                            {
                                location.Distance_ToStart = (int)distance_ToStart;
                            }

                            int? distance_ToDestination = WorldUtil.GetDistance(new Location(location.X, location.Y, 0), new Location(target.X, target.Y, 0));
                            if (distance_ToDestination != null)
                            {
                                location.Distance_ToDestination = (int)distance_ToDestination;
                            }

                            location.Parent = last_min;
                            open.Add(location);
                        }
                    }

                    if (open.Count > 0)
                    {
                        ALocation? min = Get_MinLocation_Target(open);
                        if (min != null)
                        {
                            open.Clear();
                            path.Add(min);
                            last_min = min;
                        }
                    }
                    else if (last_min.Parent != null)
                    {
                        last_min = last_min.Parent;
                    }
                }
                else
                {
                    break;
                }
            }

            if (reached)
            {
                result = Optimize_Path(path, start);
                result.Reverse();
            }

            return result;
        }

        private static List<ALocation> GetLocations(Layer bottom_tiles, Layer middle_tiles, ALocation location)
        {
            List<ALocation> locations = [];

            if (location == null)
            {
                return locations;
            }

            ALocation North = new(location.X, location.Y - 1);
            if (Walkable(bottom_tiles, middle_tiles, North))
            {
                locations.Add(North);
            }

            ALocation East = new(location.X + 1, location.Y);
            if (Walkable(bottom_tiles, middle_tiles, East))
            {
                locations.Add(East);
            }

            ALocation South = new(location.X, location.Y + 1);
            if (Walkable(bottom_tiles, middle_tiles, South))
            {
                locations.Add(South);
            }

            ALocation West = new(location.X - 1, location.Y);
            if (Walkable(bottom_tiles, middle_tiles, West))
            {
                locations.Add(West);
            }

            return locations;
        }

        private static List<ALocation> GetLocations_NextRoom(Map room, Map next_room, Layer bottom_tiles, Layer middle_tiles, ALocation target, ALocation location)
        {
            List<ALocation> locations = [];

            if (location == null)
            {
                return locations;
            }

            ALocation North = new(location.X, location.Y - 1);
            if (Walkable_NextRoom(room, next_room, bottom_tiles, middle_tiles, target, North))
            {
                locations.Add(North);
            }

            ALocation East = new(location.X + 1, location.Y);
            if (Walkable_NextRoom(room, next_room, bottom_tiles, middle_tiles, target, East))
            {
                locations.Add(East);
            }

            ALocation South = new(location.X, location.Y + 1);
            if (Walkable_NextRoom(room, next_room, bottom_tiles, middle_tiles, target, South))
            {
                locations.Add(South);
            }

            ALocation West = new(location.X - 1, location.Y);
            if (Walkable_NextRoom(room, next_room, bottom_tiles, middle_tiles, target, West))
            {
                locations.Add(West);
            }

            return locations;
        }

        private static List<ALocation> Optimize_Path(List<ALocation> possible, ALocation start)
        {
            List<ALocation> path = [];

            ALocation min = possible[possible.Count - 1];
            List<ALocation> open = [];
            path.Add(min);
            ALocation last_min = min;

            bool reached = false;
            int path_max = possible.Count;
            for (int i = 0; i < path_max; i++)
            {
                List<ALocation> locations = Get_ClosedLocations(possible, last_min);

                int count = locations.Count;
                for (int l = 0; l < count; l++)
                {
                    ALocation location = locations[l];

                    if (!HasLocation(path, location))
                    {
                        open.Add(location);
                    }
                }

                if (open.Count > 0)
                {
                    min = Get_MinLocation_Start(open);
                    open.Clear();
                    path.Add(min);
                    last_min = min;

                    if (min.X == start.X &&
                        min.Y == start.Y)
                    {
                        reached = true;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            if (reached)
            {
                return path;
            }

            return [];
        }

        private static List<ALocation> Get_ClosedLocations(List<ALocation> possible, ALocation location)
        {
            List<ALocation> locations = [];

            ALocation North = new(location.X, location.Y - 1);

            int northCount = possible.Count;
            for (int i = 0; i < northCount; i++)
            {
                ALocation existing = possible[i];
                if (existing.X == North.X &&
                    existing.Y == North.Y)
                {
                    locations.Add(existing);
                    break;
                }
            }

            ALocation East = new(location.X + 1, location.Y);
            int eastCount = possible.Count;
            for (int i = 0; i < eastCount; i++)
            {
                ALocation existing = possible[i];
                if (existing.X == East.X &&
                    existing.Y == East.Y)
                {
                    locations.Add(existing);
                    break;
                }
            }

            ALocation South = new(location.X, location.Y + 1);
            int southCount = possible.Count;
            for (int i = 0; i < southCount; i++)
            {
                ALocation existing = possible[i];
                if (existing.X == South.X &&
                    existing.Y == South.Y)
                {
                    locations.Add(existing);
                    break;
                }
            }

            ALocation West = new(location.X - 1, location.Y);
            int westCount = possible.Count;
            for (int i = 0; i < westCount; i++)
            {
                ALocation existing = possible[i];
                if (existing.X == West.X &&
                    existing.Y == West.Y)
                {
                    locations.Add(existing);
                    break;
                }
            }

            return locations;
        }

        private static bool HasLocation(List<ALocation> locations, ALocation location)
        {
            int count = locations.Count;
            for (int i = 0; i < count; i++)
            {
                ALocation existing = locations[i];
                if (existing.X == location.X &&
                    existing.Y == location.Y)
                {
                    return true;
                }
            }

            return false;
        }

        private static ALocation? Get_MinLocation_Target(List<ALocation> locations)
        {
            if (locations.Count == 0)
            {
                return null;
            }

            ALocation current = locations[0];
            if (current.Distance_ToDestination == 0)
            {
                return current;
            }

            float current_near = current.Distance_ToDestination;
            float current_far = current.Distance_ToStart;

            int count = locations.Count;
            for (int i = 0; i < count; i++)
            {
                ALocation location = locations[i];

                float pref_near = location.Distance_ToDestination;
                float pref_far = location.Distance_ToStart;

                if ((pref_near <= current_near && pref_far > current_far) ||
                    pref_near < current_near)
                {
                    current = location;
                    current_near = pref_near;
                    current_far = pref_far;
                }
            }

            return current;
        }

        private static ALocation Get_MinLocation_Start(List<ALocation> locations)
        {
            ALocation current = locations[0];
            if (current.Distance_ToStart == 0)
            {
                return current;
            }

            float current_near = current.Distance_ToDestination;
            float current_far = current.Distance_ToStart;

            int count = locations.Count;
            for (int i = 0; i < count; i++)
            {
                ALocation location = locations[i];

                float pref_near = location.Distance_ToDestination;
                float pref_far = location.Distance_ToStart;

                if ((pref_far <= current_far && pref_near > current_near) ||
                    pref_far < current_far)
                {
                    current = location;
                    current_near = pref_near;
                    current_far = pref_far;
                }
            }

            return current;
        }

        private static bool DestinationReached(ALocation location, Location target, bool stop_next_to_tile)
        {
            if (stop_next_to_tile)
            {
                if ((location.X == target.X - 1 && location.Y == target.Y) ||
                    (location.X == target.X + 1 && location.Y == target.Y) ||
                    (location.X == target.X && location.Y == target.Y - 1) ||
                    (location.X == target.X && location.Y == target.Y + 1))
                {
                    return true;
                }
            }
            else
            {
                if (location.X == target.X &&
                    location.Y == target.Y)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Walkable(Layer? bottom_tiles, Layer? middle_tiles, ALocation? location)
        {
            if (location == null)
            {
                return false;
            }

            Tile? bottom_tile = bottom_tiles?.GetTile(new Vector2(location.X, location.Y));
            if (bottom_tile == null)
            {
                return false;
            }

            if (bottom_tile.BlocksMovement)
            {
                return false;
            }

            Tile? middle_tile = middle_tiles?.GetTile(new Vector2(location.X, location.Y));
            if (middle_tile != null)
            {
                if (middle_tile.BlocksMovement)
                {
                    if (middle_tile.Name != null &&
                        !middle_tile.Name.Contains("Door"))
                    {
                        return false;
                    }
                }
                else if (middle_tile.Name != null &&
                         middle_tile.Name.Contains("Window"))
                {
                    return false;
                }
            }

            if (nearby_characters.Count > 0)
            {
                Character? other = WorldUtil.GetCharacter(nearby_characters, new Location(location.X, location.Y));
                if (other != null)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool Walkable_NextRoom(Map room, Map next_room, Layer bottom_tiles, Layer middle_tiles, ALocation target, ALocation location)
        {
            Tile? bottom_tile = bottom_tiles?.GetTile(new Vector2(location.X, location.Y));
            if (bottom_tile == null)
            {
                return false;
            }

            if (bottom_tile.BlocksMovement)
            {
                return false;
            }

            Tile? middle_tile = middle_tiles?.GetTile(new Vector2(location.X, location.Y));
            if (middle_tile != null)
            {
                if (middle_tile.BlocksMovement)
                {
                    if (middle_tile.Name != null &&
                        !middle_tile.Name.Contains("Door"))
                    {
                        return false;
                    }
                }
                else if (middle_tile.Name != null &&
                         middle_tile.Name.Contains("Window"))
                {
                    return false;
                }
            }

            bool inCurrentRoom = InRoom(room, location);
            if (!inCurrentRoom)
            {
                bool inNextRoom = InRoom(next_room, location);
                if (!inNextRoom)
                {
                    if (location.X == target.X &&
                        location.Y == target.Y)
                    {
                        return true;
                    }

                    return false;
                }
            }

            if (nearby_characters.Count > 0)
            {
                Character? other = WorldUtil.GetCharacter(nearby_characters, new Location(location.X, location.Y));
                if (other != null)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool InRoom(Map room, ALocation location)
        {
            if (room.Location == null)
            {
                return false;
            }

            Layer? tiles = room.GetLayer("Tiles");
            if (tiles == null)
            {
                return false;
            }

            int count = tiles.Tiles.Count;
            for (int i = 0; i < count; i++)
            {
                Tile tile = tiles.Tiles[i];
                if (tile.Location == null)
                {
                    continue;
                }

                float x = tile.Location.X + (room.Location.X * 20);
                float y = tile.Location.Y + (room.Location.Y * 20);

                if (x == location.X &&
                    y == location.Y)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}

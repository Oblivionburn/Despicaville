using System.Collections.Generic;

using Microsoft.Xna.Framework;

using OP_Engine.Characters;
using OP_Engine.Utility;
using OP_Engine.Tiles;

namespace Despicaville.Util
{
    public static class DPathing
    {
        private static List<Character> nearby_characters;
        public static Dictionary<long, List<Tile>> attempted_pathing = new Dictionary<long, List<Tile>>();

        public static List<ALocation> GetPath(Layer bottom_tiles, Layer middle_tiles, Character character, Tile tile, int max_distance, bool stop_next_to_tile)
        {
            List<ALocation> result = new List<ALocation>();

            List<ALocation> path = new List<ALocation>();
            ALocation target = new ALocation((int)tile.Location.X, (int)tile.Location.Y);

            ALocation start = new ALocation((int)character.Location.X, (int)character.Location.Y);
            List<ALocation> open = new List<ALocation>();
            path.Add(start);
            ALocation last_min = start;

            bool reached = false;
            for (int i = 0; i < max_distance; i++)
            {
                if (last_min != null)
                {
                    nearby_characters = WorldUtil.GetNearbyCharacters(character.ID, new Vector3(last_min.X, last_min.Y, 0));

                    List<ALocation> locations = GetLocations(bottom_tiles, middle_tiles, last_min);

                    int count = locations.Count;
                    for (int l = 0; l < count; l++)
                    {
                        ALocation location = locations[l];

                        if (!HasLocation(path, location))
                        {
                            location.Distance_ToStart = WorldUtil.GetDistance(new Vector3(location.X, location.Y, 0), new Vector3(last_min.X, last_min.Y, 0));
                            location.Distance_ToDestination = WorldUtil.GetDistance(new Vector3(location.X, location.Y, 0), new Vector3(target.X, target.Y, 0));
                            location.Parent = last_min;
                            open.Add(location);
                        }
                    }

                    if (open.Count > 0)
                    {
                        ALocation min = Get_MinLocation_Target(open, target, last_min);
                        open.Clear();
                        path.Add(min);
                        last_min = min;

                        if (DestinationReached(min, tile, stop_next_to_tile))
                        {
                            reached = true;

                            if (attempted_pathing.ContainsKey(character.ID))
                            {
                                attempted_pathing[character.ID].Clear();
                            }

                            break;
                        }
                    }
                    else
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
            }

            return result;
        }

        public static List<ALocation> GetLocations(Layer bottom_tiles, Layer middle_tiles, ALocation location)
        {
            List<ALocation> locations = new List<ALocation>();

            ALocation North = new ALocation(location.X, location.Y - 1);
            if (Walkable(bottom_tiles, middle_tiles, North))
            {
                locations.Add(North);
            }

            ALocation East = new ALocation(location.X + 1, location.Y);
            if (Walkable(bottom_tiles, middle_tiles, East))
            {
                locations.Add(East);
            }

            ALocation South = new ALocation(location.X, location.Y + 1);
            if (Walkable(bottom_tiles, middle_tiles, South))
            {
                locations.Add(South);
            }

            ALocation West = new ALocation(location.X - 1, location.Y);
            if (Walkable(bottom_tiles, middle_tiles, West))
            {
                locations.Add(West);
            }

            return locations;
        }

        public static List<ALocation> Optimize_Path(List<ALocation> possible, ALocation start)
        {
            List<ALocation> path = new List<ALocation>();

            ALocation min = possible[possible.Count - 1];
            List<ALocation> open = new List<ALocation>();
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
                    possible = Path_RemoveTile(possible, min);
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

            return null;
        }

        public static List<ALocation> Get_ClosedLocations(List<ALocation> possible, ALocation location)
        {
            List<ALocation> locations = new List<ALocation>();

            ALocation North = new ALocation(location.X, location.Y - 1);

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

            ALocation East = new ALocation(location.X + 1, location.Y);
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

            ALocation South = new ALocation(location.X, location.Y + 1);
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

            ALocation West = new ALocation(location.X - 1, location.Y);
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

        public static bool HasLocation(List<ALocation> locations, ALocation location)
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

        public static ALocation Get_MinLocation_Target(List<ALocation> locations, ALocation target, ALocation previous)
        {
            ALocation current = locations[0];

            int current_near = current.Distance_ToDestination;
            int current_far = current.Distance_ToStart;

            int count = locations.Count;
            for (int i = 0; i < count; i++)
            {
                ALocation location = locations[i];

                int pref_near = location.Distance_ToDestination;
                int pref_far = location.Distance_ToStart;

                bool equal = false;
                if (pref_near == current_near &&
                    pref_far == current_far)
                {
                    equal = true;

                    bool preferred = false;
                    if (target.X < previous.X)
                    {
                        if (target.Y < previous.Y)
                        {
                            //NorthWest
                            if (location.X <= current.X ||
                                location.Y <= current.Y)
                            {
                                preferred = true;
                            }
                        }
                        else if (target.Y > previous.Y)
                        {
                            //SouthWest
                            if (location.X <= current.X ||
                                location.Y >= current.Y)
                            {
                                preferred = true;
                            }
                        }
                        else if (target.Y == previous.Y)
                        {
                            //West
                            if (location.X <= current.X)
                            {
                                preferred = true;
                            }
                        }
                    }
                    else if (target.X > previous.X)
                    {
                        if (target.Y < previous.Y)
                        {
                            //NorthEast
                            if (location.X >= current.X ||
                                location.Y <= current.Y)
                            {
                                preferred = true;
                            }
                        }
                        else if (target.Y > previous.Y)
                        {
                            //SouthEast
                            if (location.X >= current.X ||
                                location.Y >= current.Y)
                            {
                                preferred = true;
                            }
                        }
                        else if (target.Y == previous.Y)
                        {
                            //East
                            if (location.X >= current.X)
                            {
                                preferred = true;
                            }
                        }
                    }
                    else if (target.X == previous.X)
                    {
                        if (target.Y < previous.Y)
                        {
                            //North
                            if (location.Y <= current.Y)
                            {
                                preferred = true;
                            }
                        }
                        else if (target.Y > previous.Y)
                        {
                            //South
                            if (location.Y >= current.Y)
                            {
                                preferred = true;
                            }
                        }
                    }

                    if (preferred)
                    {
                        current = location;
                        current_near = pref_near;
                        current_far = pref_far;
                    }
                }

                if (!equal)
                {
                    if ((pref_near <= current_near && pref_far > current_far) ||
                         pref_near < current_near)
                    {
                        current = location;
                        current_near = pref_near;
                        current_far = pref_far;
                    }
                }
            }

            return current;
        }

        public static ALocation Get_MinLocation_Start(List<ALocation> locations)
        {
            ALocation current = locations[0];

            int current_far = current.Distance_ToStart;

            int count = locations.Count;
            for (int i = 0; i < count; i++)
            {
                ALocation location = locations[i];

                int pref_far = location.Distance_ToStart;
                if (pref_far < current_far)
                {
                    current = location;
                    current_far = pref_far;
                }
            }

            return current;
        }

        public static List<ALocation> Path_RemoveTile(List<ALocation> path, ALocation tile)
        {
            int count = path.Count;
            for (int i = 0; i < count; i++)
            {
                ALocation location = path[i];
                if (location.X == tile.X &&
                    location.Y == tile.Y)
                {
                    path.Remove(location);
                    break;
                }
            }

            return path;
        }

        public static bool DestinationReached(ALocation location, Tile target_tile, bool stop_next_to_tile)
        {
            if (stop_next_to_tile)
            {
                if ((location.X == target_tile.Location.X - 1 && location.Y == target_tile.Location.Y) ||
                    (location.X == target_tile.Location.X + 1 && location.Y == target_tile.Location.Y) ||
                    (location.X == target_tile.Location.X && location.Y == target_tile.Location.Y - 1) ||
                    (location.X == target_tile.Location.X && location.Y == target_tile.Location.Y + 1))
                {
                    return true;
                }
            }
            else
            {
                if (location.X == target_tile.Location.X &&
                    location.Y == target_tile.Location.Y)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool Walkable(Layer bottom_tiles, Layer middle_tiles, ALocation location)
        {
            Tile bottom_tile = bottom_tiles.GetTile(new Vector2(location.X, location.Y));
            if (bottom_tile != null)
            {
                if (bottom_tile.BlocksMovement)
                {
                    return false;
                }

                Tile middle_tile = middle_tiles.GetTile(new Vector2(location.X, location.Y));
                if (middle_tile != null)
                {
                    if (middle_tile.BlocksMovement)
                    {
                        if (!middle_tile.Name.Contains("Door") &&
                            !middle_tile.Name.Contains("Window"))
                        {
                            return false;
                        }
                    }
                }

                if (nearby_characters.Count > 0)
                {
                    Character other = WorldUtil.GetCharacter(nearby_characters, new Vector3(location.X, location.Y, 0));
                    if (other != null)
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        public static void AddAttemptedTile(Character character, Tile tile)
        {
            if (attempted_pathing.ContainsKey(character.ID))
            {
                if (!attempted_pathing[character.ID].Contains(tile))
                {
                    tile.Value = 1000;
                    attempted_pathing[character.ID].Add(tile);
                }
            }
            else
            {
                tile.Value = 1000;
                attempted_pathing.Add(character.ID, new List<Tile> { tile });
            }
        }

        public static bool SkipAttemptedTile(Character character, Tile tile)
        {
            if (attempted_pathing.ContainsKey(character.ID))
            {
                if (attempted_pathing[character.ID].Contains(tile))
                {
                    tile.DecreaseValue(1);
                    if (tile.Value == 0)
                    {
                        attempted_pathing[character.ID].Remove(tile);
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}

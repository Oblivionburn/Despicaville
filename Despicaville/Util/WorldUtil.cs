using System.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using OP_Engine.Characters;
using OP_Engine.Inventories;
using OP_Engine.Tiles;
using OP_Engine.Utility;
using OP_Engine.Scenes;

namespace Despicaville.Util
{
    public static class WorldUtil
    {
        public static bool InRange(Vector3 location, Vector3 source, int distance)
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

        public static Direction GetDirection(Vector3 target, Vector3 source, bool compass_directions)
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

        public static int GetDistance(Vector3 origin, Vector3 location)
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

        public static bool Location_IsVisible(long character_id, Vector3 location)
        {
            List<Tile> tiles = new List<Tile>();
            if (Handler.VisibleTiles.ContainsKey(character_id))
            {
                tiles = Handler.VisibleTiles[character_id];
            }

            for (int i = 0; i < tiles.Count; i++)
            {
                Vector3 tile_location = tiles[i].Location;
                if (tile_location.X == location.X &&
                    tile_location.Y == location.Y)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool BlocksSight(string name)
        {
            for (int i = 0; i < Handler.SeeThrough.Length; i++)
            {
                if (name.Contains(Handler.SeeThrough[i]))
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

        public static Tile GetNearestExit_ToFurniture(Character character, Layer bottom_tiles, Layer middle_tiles, Layer room_tiles, Tile furniture)
        {
            List<Tile> exits = GetExits(bottom_tiles, middle_tiles, room_tiles, character.Location);
            if (exits.Count > 0)
            {
                Tile nearest_exit = exits[0];
                int distance = GetDistance(nearest_exit.Location, furniture.Location);

                foreach (Tile exit in exits)
                {
                    int new_distance = GetDistance(exit.Location, furniture.Location);
                    if (new_distance < distance)
                    {
                        nearest_exit = exit;
                        distance = new_distance;
                    }
                }

                return nearest_exit;
            }

            return null;
        }

        public static Tile GetClosestTile(List<Tile> tiles, Character character)
        {
            if (tiles.Count > 0)
            {
                if (tiles.Count > 1)
                {
                    Tile closest = tiles[0];
                    int nearest = GetDistance(character.Location, closest.Location);

                    for (int i = 1; i < tiles.Count; i++)
                    {
                        Tile tile = tiles[i];

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
                    return tiles[0];
                }
            }

            return null;
        }

        public static List<Character> GetNearbyCharacters(long ID, Vector3 location)
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

        public static Character GetCharacter(List<Character> characters, Vector3 destination)
        {
            for (int i = 0; i < characters.Count; i++)
            {
                Character existing = characters[i];
                if (existing.Moving)
                {
                    if (existing.Destination.X == destination.X &&
                        existing.Destination.Y == destination.Y)
                    {
                        return existing;
                    }
                }
                else
                {
                    if (existing.Location.X == destination.X &&
                        existing.Location.Y == destination.Y)
                    {
                        return existing;
                    }
                }
            }

            return null;
        }

        public static Character MouseGetCharacter(List<Character> characters, Vector3 destination)
        {
            for (int i = 0; i < characters.Count; i++)
            {
                Character existing = characters[i];
                if ((existing.Destination.X == destination.X &&
                     existing.Destination.Y == destination.Y) ||
                    (existing.Location.X == destination.X &&
                     existing.Location.Y == destination.Y))
                {
                    return existing;
                }
            }

            return null;
        }

        public static Character GetCharacter(Vector3 destination)
        {
            Army army = CharacterManager.GetArmy("Characters");

            Character player = Handler.GetPlayer();
            if (player.Moving)
            {
                if (player.Destination.X == destination.X &&
                    player.Destination.Y == destination.Y)
                {
                    return player;
                }
            }
            else
            {
                if (player.Location.X == destination.X &&
                    player.Location.Y == destination.Y)
                {
                    return player;
                }
            }

            Squad citizens = army.GetSquad("Citizens");
            int count = citizens.Characters.Count;
            for (int i = 0; i < count; i++)
            {
                Character existing = citizens.Characters[i];
                if (existing.Moving)
                {
                    if (existing.Destination.X == destination.X &&
                        existing.Destination.Y == destination.Y)
                    {
                        return existing;
                    }
                }
                else
                {
                    if (existing.Location.X == destination.X &&
                        existing.Location.Y == destination.Y)
                    {
                        return existing;
                    }
                }
            }

            return null;
        }

        public static bool IsCharacter_AtLocation(long ID, Vector3 location)
        {
            Character player = Handler.GetPlayer();
            if (ID == player.ID)
            {
                if (player.Moving)
                {
                    if (player.Destination.X == location.X &&
                        player.Destination.Y == location.Y)
                    {
                        return true;
                    }
                }
                else
                {
                    if (player.Location.X == location.X &&
                        player.Location.Y == location.Y)
                    {
                        return true;
                    }
                }
            }
            else
            {
                Army army = CharacterManager.GetArmy("Characters");
                Squad citizens = army.GetSquad("Citizens");
                Character existing = citizens.GetCharacter(ID);
                if (existing != null)
                {
                    if (existing.Moving)
                    {
                        if (existing.Destination.X == location.X &&
                            existing.Destination.Y == location.Y)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (existing.Location.X == location.X &&
                            existing.Location.Y == location.Y)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
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

        public static Tile GetFurniture(Layer middle_tiles, Vector3 destination)
        {
            if (middle_tiles != null)
            {
                foreach (Tile existing in middle_tiles.Tiles)
                {
                    if (existing.Texture != null)
                    {
                        if (destination.X == existing.Location.X &&
                            destination.Y == existing.Location.Y)
                        {
                            return existing;
                        }
                        else if (existing.Region.Height == Main.Game.TileSize.Y)
                        {
                            if (existing.Direction == Direction.Right)
                            {
                                if (existing.Region.Width == (Main.Game.TileSize.X * 2))
                                {
                                    if (destination.X >= existing.Location.X && destination.X <= existing.Location.X + 1 &&
                                        destination.Y == existing.Location.Y)
                                    {
                                        return existing;
                                    }
                                }
                            }
                            else if (existing.Direction == Direction.Left)
                            {
                                if (existing.Region.Width == (Main.Game.TileSize.X * 2))
                                {
                                    if (destination.X >= existing.Location.X - 1 && destination.X <= existing.Location.X &&
                                        destination.Y == existing.Location.Y)
                                    {
                                        return existing;
                                    }
                                }
                            }
                            else if (existing.Direction == Direction.Up ||
                                     existing.Direction == Direction.Down)
                            {
                                if (existing.Region.Width == (Main.Game.TileSize.X * 2))
                                {
                                    if (destination.X >= existing.Location.X && destination.X <= existing.Location.X + 1 &&
                                        destination.Y == existing.Location.Y)
                                    {
                                        return existing;
                                    }
                                }
                                else if (existing.Region.Width == (Main.Game.TileSize.X * 3))
                                {
                                    if (destination.X >= existing.Location.X - 1 && destination.X <= existing.Location.X + 1 &&
                                        destination.Y == existing.Location.Y)
                                    {
                                        return existing;
                                    }
                                }
                            }
                        }
                        else if (existing.Region.Width == Main.Game.TileSize.X)
                        {
                            if (existing.Direction == Direction.Up)
                            {
                                if (existing.Region.Height == (Main.Game.TileSize.Y * 2))
                                {
                                    if (destination.X == existing.Location.X &&
                                        destination.Y >= existing.Location.Y && destination.Y <= existing.Location.Y - 1)
                                    {
                                        return existing;
                                    }
                                }
                            }
                            else if (existing.Direction == Direction.Down)
                            {
                                if (existing.Region.Height == (Main.Game.TileSize.Y * 2))
                                {
                                    if (destination.X == existing.Location.X &&
                                        destination.Y >= existing.Location.Y && destination.Y <= existing.Location.Y + 1)
                                    {
                                        return existing;
                                    }
                                }
                            }
                            else if (existing.Direction == Direction.Left ||
                                     existing.Direction == Direction.Right)
                            {
                                if (existing.Region.Height == (Main.Game.TileSize.Y * 2))
                                {
                                    if (destination.X == existing.Location.X &&
                                        destination.Y >= existing.Location.Y && destination.Y <= existing.Location.Y + 1)
                                    {
                                        return existing;
                                    }
                                }
                                else if (existing.Region.Height == (Main.Game.TileSize.Y * 3))
                                {
                                    if (destination.X == existing.Location.X &&
                                        destination.Y >= existing.Location.Y - 1 && destination.Y <= existing.Location.Y + 1)
                                    {
                                        return existing;
                                    }
                                }
                            }
                        }
                        else if (existing.Region.Width == (Main.Game.TileSize.X * 2) &&
                                 existing.Region.Height == (Main.Game.TileSize.Y * 2))
                        {
                            if (destination.X >= existing.Location.X && destination.X <= existing.Location.X + 1 &&
                                destination.Y >= existing.Location.Y && destination.Y <= existing.Location.Y + 1)
                            {
                                return existing;
                            }
                        }
                    }
                }
            }

            return null;
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

        public static bool Furniture_InRoom(Layer room_tiles, Tile furniture, Character character)
        {
            Layer room = GetRoom(room_tiles, character.Location);
            if (room != null)
            {
                foreach (Tile tile in room.Tiles)
                {
                    if (tile.Location.X == furniture.Location.X &&
                        tile.Location.Y == furniture.Location.Y)
                    {
                        return true;
                    }
                }
            }

            return false;
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
            Character player = Handler.GetPlayer();
            Something stat = player.GetStat("Perception");
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

                if (player.Relationships.ContainsKey(character.ID))
                {
                    description += " " + he_she + " is your " + player.Relationships[character.ID].ToLower() + ".";
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

        public static Layer GetRoom(Layer room_tiles, Vector3 location)
        {
            Tile room_tile = room_tiles.GetTile(new Vector2(location.X, location.Y));
            if (room_tile != null &&
                room_tile.Texture != null)
            {
                Layer room = new Layer();

                GetRoomTiles(room_tiles, room, room_tile);

                if (room.Tiles.Count > 0)
                {
                    return room;
                }
            }

            return null;
        }

        public static void GetRoomTiles(Layer room_tiles, Layer room, Tile room_tile)
        {
            room.Tiles.Add(room_tile);

            Tile north = room_tiles.GetTile(new Vector2(room_tile.Location.X, room_tile.Location.Y - 1));
            if (north != null &&
                north.Texture != null &&
                north.Name == room_tile.Name &&
                !room.Tiles.Contains(north))
            {
                GetRoomTiles(room_tiles, room, north);
            }

            Tile east = room_tiles.GetTile(new Vector2(room_tile.Location.X + 1, room_tile.Location.Y));
            if (east != null &&
                east.Texture != null &&
                east.Name == room_tile.Name &&
                !room.Tiles.Contains(east))
            {
                GetRoomTiles(room_tiles, room, east);
            }

            Tile south = room_tiles.GetTile(new Vector2(room_tile.Location.X, room_tile.Location.Y + 1));
            if (south != null &&
                south.Texture != null &&
                south.Name == room_tile.Name &&
                !room.Tiles.Contains(south))
            {
                GetRoomTiles(room_tiles, room, south);
            }

            Tile west = room_tiles.GetTile(new Vector2(room_tile.Location.X - 1, room_tile.Location.Y));
            if (west != null &&
                west.Texture != null &&
                west.Name == room_tile.Name &&
                !room.Tiles.Contains(west))
            {
                GetRoomTiles(room_tiles, room, west);
            }
        }

        public static List<Tile> GetExits(Layer bottom_tiles, Layer middle_tiles, Layer room_tiles, Vector3 location)
        {
            List<Tile> exits = new List<Tile>();

            Layer room = GetRoom(room_tiles, location);
            if (room != null)
            {
                foreach (Tile tile in room.Tiles)
                {
                    Vector2 north = new Vector2(tile.Location.X, tile.Location.Y - 1);

                    Tile north_bottom = bottom_tiles.GetTile(north);
                    if (north_bottom != null &&
                        !north_bottom.Name.Contains("Wall"))
                    {
                        Tile north_middle = middle_tiles.GetTile(north);
                        if (north_middle.Name.Contains("Door"))
                        {
                            if (!exits.Contains(north_middle))
                            {
                                exits.Add(north_middle);
                            }
                        }
                        else
                        {
                            Tile north_room = room_tiles.GetTile(north);
                            if (north_room != null &&
                                north_room.Texture != null &&
                                north_room.Name != tile.Name)
                            {
                                if (!exits.Contains(north_room))
                                {
                                    exits.Add(north_room);
                                }
                            }
                        }
                    }

                    Vector2 east = new Vector2(tile.Location.X + 1, tile.Location.Y);

                    Tile east_bottom = bottom_tiles.GetTile(east);
                    if (east_bottom != null &&
                        !east_bottom.Name.Contains("Wall"))
                    {
                        Tile east_middle = middle_tiles.GetTile(east);
                        if (east_middle.Name.Contains("Door"))
                        {
                            if (!exits.Contains(east_middle))
                            {
                                exits.Add(east_middle);
                            }
                        }
                        else
                        {
                            Tile east_room = room_tiles.GetTile(east);
                            if (east_room != null &&
                                east_room.Texture != null &&
                                east_room.Name != tile.Name)
                            {
                                if (!exits.Contains(east_room))
                                {
                                    exits.Add(east_room);
                                }
                            }
                        }
                    }

                    Vector2 south = new Vector2(tile.Location.X, tile.Location.Y + 1);

                    Tile south_bottom = bottom_tiles.GetTile(south);
                    if (south_bottom != null &&
                        !south_bottom.Name.Contains("Wall"))
                    {
                        Tile south_middle = middle_tiles.GetTile(south);
                        if (south_middle.Name.Contains("Door"))
                        {
                            if (!exits.Contains(south_middle))
                            {
                                exits.Add(south_middle);
                            }
                        }
                        else
                        {
                            Tile south_room = room_tiles.GetTile(south);
                            if (south_room != null &&
                                south_room.Texture != null &&
                                south_room.Name != tile.Name)
                            {
                                if (!exits.Contains(south_room))
                                {
                                    exits.Add(south_room);
                                }
                            }
                        }
                    }

                    Vector2 west = new Vector2(tile.Location.X - 1, tile.Location.Y);

                    Tile west_bottom = bottom_tiles.GetTile(west);
                    if (west_bottom != null &&
                        !west_bottom.Name.Contains("Wall"))
                    {
                        Tile west_middle = middle_tiles.GetTile(west);
                        if (west_middle.Name.Contains("Door"))
                        {
                            if (!exits.Contains(west_middle))
                            {
                                exits.Add(west_middle);
                            }
                        }
                        else
                        {
                            Tile west_room = room_tiles.GetTile(west);
                            if (west_room != null &&
                                west_room.Texture != null &&
                                west_room.Name != tile.Name)
                            {
                                if (!exits.Contains(west_room))
                                {
                                    exits.Add(west_room);
                                }
                            }
                        }
                    }
                }
            }

            return exits;
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

            foreach (Tile tile in Handler.OwnedFurniture[character.ID])
            {
                if (tile.Name.Contains(name))
                {
                    tiles.Add(tile);
                }
            }

            return tiles;
        }

        public static void AssignPlayerBed(World world, Character player)
        {
            Army army = CharacterManager.GetArmy("Characters");
            Squad squad = army.GetSquad("Citizens");
            bool found_bed = false;

            Map map = world.Maps[0];

            CryptoRandom random = new CryptoRandom();
            List<Map> homes = WorldGen.Residential.OrderBy(a => random.Next()).ToList();

            foreach (Map home in homes)
            {
                Point map_coords = new Point((int)home.Location.X, (int)home.Location.Y);

                Layer middle_tiles = map.GetLayer("MiddleTiles");
                Layer top_tiles = map.GetLayer("TopTiles");

                List<Tile> middle_furniture = GetAllFurniture(middle_tiles, map_coords);
                List<Tile> top_furniture = GetAllFurniture(top_tiles, map_coords);

                int bed_count = 0;
                Tile bed = null;
                foreach (Tile tile in middle_furniture)
                {
                    if (tile.Name.Contains("Bed"))
                    {
                        bed = tile;
                        bed_count++;
                    }
                }

                if (bed_count == 1)
                {
                    Vector3 bed_location = default;
                    if (bed.Direction == Direction.Up)
                    {
                        bed_location = new Vector3(bed.Location.X, bed.Location.Y + 1, 0);
                    }
                    else if (bed.Direction == Direction.Right ||
                             bed.Direction == Direction.Down)
                    {
                        bed_location = new Vector3(bed.Location.X, bed.Location.Y, 0);
                    }
                    else if (bed.Direction == Direction.Left)
                    {
                        bed_location = new Vector3(bed.Location.X + 1, bed.Location.Y, 0);
                    }

                    Character character = GetCharacter(bed_location);
                    if (character != null)
                    {
                        found_bed = true;

                        player.Location = new Vector3(character.Location.X, character.Location.Y, 0);

                        if (!Handler.OwnedFurniture.ContainsKey(player.ID))
                        {
                            Handler.OwnedFurniture.Add(player.ID, new List<Tile>());
                        }

                        foreach (Tile tile in middle_furniture)
                        {
                            tile.OwnerIDs.Add(player.ID);
                            tile.OwnerIDs.Remove(character.ID);

                            Handler.OwnedFurniture[player.ID].Add(tile);
                        }

                        foreach (Tile tile in top_furniture)
                        {
                            tile.OwnerIDs.Add(player.ID);
                            tile.OwnerIDs.Remove(character.ID);

                            Handler.OwnedFurniture[player.ID].Add(tile);
                        }

                        Handler.OwnedFurniture.Remove(character.ID);
                        squad.Characters.Remove(character);

                        break;
                    }
                }
            }

            if (!found_bed)
            {
                foreach (Map home in homes)
                {
                    Point map_coords = new Point((int)home.Location.X, (int)home.Location.Y);

                    Layer middle_tiles = map.GetLayer("MiddleTiles");
                    Layer top_tiles = map.GetLayer("TopTiles");

                    List<Tile> middle_furniture = GetAllFurniture(middle_tiles, map_coords);
                    List<Tile> top_furniture = GetAllFurniture(top_tiles, map_coords);

                    int bed_count = 0;
                    Tile bed = null;
                    foreach (Tile tile in middle_furniture)
                    {
                        if (tile.Name.Contains("DoubleBed"))
                        {
                            bed = tile;
                            bed_count++;
                        }
                    }

                    if (bed_count >= 1)
                    {
                        Vector3 bed_location = default;
                        if (bed.Direction == Direction.Up)
                        {
                            bed_location = new Vector3(bed.Location.X, bed.Location.Y + 1, 0);
                        }
                        else if (bed.Direction == Direction.Right ||
                                 bed.Direction == Direction.Down)
                        {
                            bed_location = new Vector3(bed.Location.X, bed.Location.Y, 0);
                        }
                        else if (bed.Direction == Direction.Left)
                        {
                            bed_location = new Vector3(bed.Location.X + 1, bed.Location.Y, 0);
                        }

                        Character character = GetCharacter(bed_location);
                        if (character != null)
                        {
                            if (character.Direction == Direction.Up ||
                                character.Direction == Direction.Down)
                            {
                                player.Location = new Vector3(character.Location.X + 1, character.Location.Y, 0);
                            }
                            else if (character.Direction == Direction.Right ||
                                     character.Direction == Direction.Left)
                            {
                                player.Location = new Vector3(character.Location.X, character.Location.Y + 1, 0);
                            }

                            if (!Handler.OwnedFurniture.ContainsKey(player.ID))
                            {
                                Handler.OwnedFurniture.Add(player.ID, new List<Tile>());
                            }

                            foreach (Tile tile in middle_furniture)
                            {
                                tile.OwnerIDs.Add(player.ID);
                                Handler.OwnedFurniture[player.ID].Add(tile);
                            }

                            foreach (Tile tile in top_furniture)
                            {
                                tile.OwnerIDs.Add(player.ID);
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

                                    if (existing.ID == character.ID)
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
                }
            }
        }

        public static bool NextTo(Vector3 target, Vector3 source)
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

        public static Tile StandingByFurniture(Layer middle_tiles, Vector3 location, string type)
        {
            foreach (Tile tile in middle_tiles.Tiles)
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

        public static bool FacingFurniture(Tile tile, Character character)
        {
            if (tile.Location.X > character.Location.X)
            {
                if (character.Direction == Direction.Right)
                {
                    return true;
                }
            }
            else if (tile.Location.X < character.Location.X)
            {
                if (character.Direction == Direction.Left)
                {
                    return true;
                }
            }
            else if (tile.Location.X == character.Location.X)
            {
                if (tile.Location.Y > character.Location.Y)
                {
                    if (character.Direction == Direction.Down)
                    {
                        return true;
                    }
                }
                else if (tile.Location.Y < character.Location.Y)
                {
                    if (character.Direction == Direction.Up)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool NextToFence(Layer middle_tiles, Vector3 location)
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
            Vector2 tile_location = new Vector2(character.Location.X, character.Location.Y);
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
            Vector2 tile_location = new Vector2(character.Location.X, character.Location.Y);
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
            foreach (Map map in WorldGen.Residential)
            {
                if (map.ID == character.MapID)
                {
                    return map;
                }
            }

            foreach (Map map in WorldGen.Commercial)
            {
                if (map.ID == character.MapID)
                {
                    return map;
                }
            }

            foreach (Map map in WorldGen.Parks)
            {
                if (map.ID == character.MapID)
                {
                    return map;
                }
            }

            foreach (Map map in WorldGen.Roads)
            {
                if (map.ID == character.MapID)
                {
                    return map;
                }
            }

            return null;
        }

        public static Map GetCurrentMap(Vector2 location)
        {
            int world_x = (int)location.X / 20;
            int world_y = (int)location.Y / 20;

            foreach (Map map in WorldGen.Residential)
            {
                if (map.Location.X == world_x &&
                    map.Location.Y == world_y)
                {
                    return map;
                }
            }

            foreach (Map map in WorldGen.Commercial)
            {
                if (map.Location.X == world_x &&
                    map.Location.Y == world_y)
                {
                    return map;
                }
            }

            foreach (Map map in WorldGen.Parks)
            {
                if (map.Location.X == world_x &&
                    map.Location.Y == world_y)
                {
                    return map;
                }
            }

            foreach (Map map in WorldGen.Roads)
            {
                if (map.Location.X == world_x &&
                    map.Location.Y == world_y)
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

            bool found = false;

            foreach (Map map in WorldGen.Residential)
            {
                if (map.Location.X == block_x &&
                    map.Location.Y == block_y)
                {
                    found = true;
                    character.MapID = map.ID;
                    break;
                }
            }

            if (!found)
            {
                foreach (Map map in WorldGen.Commercial)
                {
                    if (map.Location.X == block_x &&
                        map.Location.Y == block_y)
                    {
                        found = true;
                        character.MapID = map.ID;
                        break;
                    }
                }
            }

            if (!found)
            {
                foreach (Map map in WorldGen.Parks)
                {
                    if (map.Location.X == block_x &&
                        map.Location.Y == block_y)
                    {
                        found = true;
                        character.MapID = map.ID;
                        break;
                    }
                }
            }

            if (!found)
            {
                foreach (Map map in WorldGen.Roads)
                {
                    if (map.Location.X == block_x &&
                        map.Location.Y == block_y)
                    {
                        character.MapID = map.ID;
                        break;
                    }
                }
            }
        }

        public static void UpdateWorldMap(Character player)
        {
            if (Handler.WorldMap_Visible)
            {
                int world_x = (int)player.Location.X / 20;
                int world_y = (int)player.Location.Y / 20;

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
                    center.Region.X = player.Region.X + (player.Region.Width / 2) - (Main.Game.MenuSize_X / 2);
                    center.Region.Y = player.Region.Y + (player.Region.Height / 2) - (Main.Game.MenuSize_Y / 2);
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

        public static void AddEffect(Layer bottom_tiles, Vector2 location, string name, string texture)
        {
            Scene scene = SceneManager.GetScene("Gameplay");
            Map map = scene.World.Maps[0];
            if (map != null)
            {
                Tile bottom_tile = bottom_tiles.GetTile(location);
                if (bottom_tile != null)
                {
                    Layer effect_tiles = map.GetLayer("EffectTiles");
                    if (effect_tiles != null)
                    {
                        Tile new_tile = new Tile();
                        new_tile.ID = Handler.GetID();
                        new_tile.MapID = map.ID;
                        new_tile.LayerID = effect_tiles.ID;
                        new_tile.Name = name;
                        new_tile.Location = new Vector3(location.X, location.Y, 0);
                        new_tile.Region = bottom_tile.Region;
                        new_tile.Visible = true;
                        effect_tiles.Tiles.Add(new_tile);

                        if (!string.IsNullOrEmpty(texture))
                        {
                            new_tile.Texture = AssetManager.Textures[texture];
                            new_tile.Image = new Rectangle(0, 0, new_tile.Texture.Width, new_tile.Texture.Height);
                        }
                    }
                }
            }
        }
    }
}

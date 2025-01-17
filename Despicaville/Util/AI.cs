using System.Collections.Generic;

using Microsoft.Xna.Framework;

using OP_Engine.Characters;
using OP_Engine.Inventories;
using OP_Engine.Tiles;
using OP_Engine.Utility;

namespace Despicaville.Util
{
    public static class AI
    {
        public static bool CanMove(Character character, Map map, Location destination)
        {
            Layer bottom_tiles = map.GetLayer("BottomTiles");
            Layer middle_tiles = map.GetLayer("MiddleTiles");

            //Check bottom tiles
            if (destination.X < bottom_tiles.Columns && destination.X >= 0 &&
                destination.Y < bottom_tiles.Rows && destination.Y >= 0)
            {
                Tile current = bottom_tiles.GetTile(new Vector2(destination.X, destination.Y));
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

            //Check furniture on current block
            Map block_map = WorldUtil.GetCurrentMap(character);
            Layer block_middle_tiles = block_map.GetLayer("MiddleTiles");
            Tile furniture = WorldUtil.GetFurniture(block_middle_tiles, new Location((int)destination.X, (int)destination.Y, (int)destination.Z));
            if (furniture != null)
            {
                if (furniture.Texture != null)
                {
                    if (!furniture.Name.Contains("Open"))
                    {
                        if (furniture.BlocksMovement)
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
            }

            //Check middle tiles for edge pieces of a nearby block (e.g. fence)
            Tile tile = middle_tiles.GetTile(new Vector2(destination.X, destination.Y));
            if (tile != null)
            {
                if (tile.Texture != null)
                {
                    if (!tile.Name.Contains("Open"))
                    {
                        if (tile.BlocksMovement)
                        {
                            return false;
                        }
                        else if (tile.Name.Contains("Window") &&
                                 character.Type != "Player")
                        {
                            return false;
                        }
                    }
                }
            }

            //Check other characters
            Character other = WorldUtil.GetCharacter(destination);
            if (other != null)
            {
                return false;
            }

            return true;
        }

        public static string ReactToAttack(Character attacker, Character defender)
        {
            defender.Target_ID = attacker.ID;
            return "Attacking";
        }

        public static Dictionary<string, string> AttackChoice(Character character)
        {
            Dictionary<string, string> attack = new Dictionary<string, string>();

            Item rightHandItem = InventoryUtil.Get_EquippedItem(character, "Right Weapon Slot");
            if (rightHandItem != null)
            {
                if (CombatUtil.IsAttack(rightHandItem.Task))
                {
                    attack.Add(rightHandItem.Name, rightHandItem.Task);
                }
            }

            if (attack.Count == 0)
            {
                Item leftHandItem = InventoryUtil.Get_EquippedItem(character, "Left Weapon Slot");
                if (leftHandItem != null)
                {
                    if (CombatUtil.IsAttack(leftHandItem.Task))
                    {
                        attack.Add(leftHandItem.Name, rightHandItem.Task);
                    }
                }
            }

            if (attack.Count == 0)
            {
                string attacking_with;

                CryptoRandom random = new CryptoRandom();
                int hand_choice = random.Next(0, 2);
                if (hand_choice == 0)
                {
                    attacking_with = "Right Hand";
                }
                else
                {
                    attacking_with = "Right Hand";
                }

                attack.Add(attacking_with, "Punch");
            }

            return attack;
        }

        public static void ReactToNoise(object sender, ReactionEventArgs e)
        {

        }

        public static void ReactToMovement(object sender, ReactionEventArgs e)
        {

        }

        public static void ReactToSmell(object sender, ReactionEventArgs e)
        {

        }
    }
}

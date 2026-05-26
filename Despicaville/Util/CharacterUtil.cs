using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OP_Engine.Characters;
using OP_Engine.Controls;
using OP_Engine.Inventories;
using OP_Engine.Scenes;
using OP_Engine.Tiles;
using OP_Engine.Utility;
using OP_Engine.Enums;

namespace Despicaville.Util
{
    public static class CharacterUtil
    {
        public static Character GenCharacter(string last_name)
        {
            Character character = new Character
            {
                ID = Handler.GetID(),
                Type = "Citizen",
                Frames = 4,
                MoveSpeed = 1,
                Move_TotalDistance = Main.Game.TileSize.X,
                Direction = Direction.South,
                Visible = true
            };
            character.Inventory.ID = Handler.GetID();
            character.Job.OwnerID = character.ID;
            InventoryManager.Inventories.Add(character.Inventory);

            ResetStats(character);
            GenStats(character);

            //Get body parts
            foreach (BodyPart part in BodyParts())
            {
                character.BodyParts.Add(part);
            }

            //Get personality traits
            foreach (Property trait in Traits(character))
            {
                character.Traits.Add(trait);
            }

            //Get Gender
            CryptoRandom random = new CryptoRandom();
            int gender = random.Next(0, 2);
            if (gender == 0)
            {
                character.Gender = "Male";
            }
            else if (gender == 1)
            {
                character.Gender = "Female";
            }

            //Get Name
            string first_name = "";
            random = new CryptoRandom();
            if (gender == 0)
            {
                first_name = CharacterManager.FirstNames_Male[random.Next(0, CharacterManager.FirstNames_Male.Count)];
            }
            else if (gender == 1)
            {
                first_name = CharacterManager.FirstNames_Female[random.Next(0, CharacterManager.FirstNames_Female.Count)];
            }
            character.Name = first_name + " " + last_name;
            character.Inventory.Name = character.Name;

            //Get Skin
            random = new CryptoRandom();
            int skin_color = random.Next(0, 3);
            if (skin_color == 0)
            {
                character.Texture = AssetManager.Textures["Naked_Dark"];
            }
            else if (skin_color == 1)
            {
                character.Texture = AssetManager.Textures["Naked_Light"];
            }
            else if (skin_color == 2)
            {
                character.Texture = AssetManager.Textures["Naked_Tan"];
            }
            character.Image = new Rectangle(0, 0, character.Texture.Width / 4, character.Texture.Height / 4);

            //Get Hair
            random = new CryptoRandom();
            int hair_length = random.Next(0, 3);
            
            if (hair_length > 0)
            {
                random = new CryptoRandom();
                int hair_color = random.Next(0, Handler.HairColor.Length);

                string hairType = "Hair_" + Handler.HairLength[hair_length] + "_" + Handler.HairColor[hair_color];

                Item hair = new Item
                {
                    Name = Handler.HairLength[hair_length] + " " + Handler.HairColor[hair_color] + " Hair",
                    Type = hairType,
                    Equipped = true,
                    Assignment = "Hair",
                    Texture = AssetManager.Textures[hairType],
                    Image = character.Image,
                    Region = character.Region,
                    DrawColor = Color.White,
                    Visible = true
                };
                character.Inventory.Items.Add(hair);
            }

            Inventory assets = InventoryManager.GetInventory("Assets");

            if (Utility.RandomPercent(10))
            {
                random = new CryptoRandom();
                int hat_color = random.Next(0, Handler.Colors.Length);

                Item hat = InventoryUtil.NewItem(assets.GetItem(Handler.Colors[hat_color] + " Hat"));
                hat.Equipped = true;
                hat.Assignment = "Hat Slot";
                hat.Image = character.Image;
                hat.Region = character.Region;
                character.Inventory.Items.Add(hat);
            }

            //Get Shirt
            random = new CryptoRandom();
            int shirt_color = random.Next(0, Handler.Colors.Length);

            Item shirt = InventoryUtil.NewItem(assets.GetItem(Handler.Colors[shirt_color] + " Shirt"));
            shirt.Equipped = true;
            shirt.Assignment = "Shirt Slot";
            shirt.Image = character.Image;
            shirt.Region = character.Region;
            character.Inventory.Items.Add(shirt);

            //Get Pants
            random = new CryptoRandom();
            int pants_color = random.Next(0, Handler.Colors.Length);

            Item pants = InventoryUtil.NewItem(assets.GetItem(Handler.Colors[pants_color] + " Pants"));
            pants.Equipped = true;
            pants.Assignment = "Pants Slot";
            pants.Image = character.Image;
            pants.Region = character.Region;
            pants.Visible = false;
            InventoryManager.Inventories.Add(pants.Inventory);
            character.Inventory.Items.Add(pants);

            //Get Shoes
            random = new CryptoRandom();
            int shoes_color = random.Next(0, Handler.Colors.Length);

            Item shoes = InventoryUtil.NewItem(assets.GetItem(Handler.Colors[shoes_color] + " Shoes"));
            shoes.Equipped = true;
            shoes.Assignment = "Shoes Slot";
            shoes.Image = character.Image;
            shoes.Region = character.Region;
            shoes.Visible = false;
            character.Inventory.Items.Add(shoes);

            return character;
        }

        public static void ResetStats(Character character)
        {
            character.Stats.Strength = 50;
            character.Stats.Vitality = 50;
            character.Stats.Endurance = 50;
            character.Stats.Agility = 50;
            character.Stats.Intelligence = 50;
            character.Stats.Perception = 50;
            character.Stats.Charisma = 50;
            character.Stats.Willpower = 50;
            character.Stats.Sanity = 50;
            character.Stats.Luck = 50;

            character.Stats.Blood = 100;
            character.Stats.Consciousness = 100;
            character.Stats.Stamina = 100;
            character.Stats.Comfort = 100;

            character.Stats.Hunger = 60;
            character.Stats.Thirst = 60;
            character.Stats.Boredom = 0;
            character.Stats.Bladder = 0;
            character.Stats.Grime = 0;
            character.Stats.Pain = 0;
            character.Stats.Paranoia = 0;

            character.Stats.Disposition = 0;
        }

        public static void GenStats(Character character)
        {
            CryptoRandom random;

            int pool = 50;

            for (int i = 0; i < pool; i++)
            {
                random = new CryptoRandom();
                int stat = random.Next(0, 10);

                switch (stat)
                {
                    case 0:
                        if (character.Stats.Strength < 100)
                        {
                            character.Stats.Strength++;
                            pool--;
                        }
                        break;

                    case 1:
                        if (character.Stats.Vitality < 100)
                        {
                            character.Stats.Vitality++;
                            pool--;
                        }
                        break;

                    case 2:
                        if (character.Stats.Endurance < 100)
                        {
                            character.Stats.Endurance++;
                            pool--;
                        }
                        break;

                    case 3:
                        if (character.Stats.Agility < 100)
                        {
                            character.Stats.Agility++;
                            pool--;
                        }
                        break;

                    case 4:
                        if (character.Stats.Intelligence < 100)
                        {
                            character.Stats.Intelligence++;
                            pool--;
                        }
                        break;

                    case 5:
                        if (character.Stats.Perception < 100)
                        {
                            character.Stats.Perception++;
                            pool--;
                        }
                        break;

                    case 6:
                        if (character.Stats.Charisma < 100)
                        {
                            character.Stats.Charisma++;
                            pool--;
                        }
                        break;

                    case 7:
                        if (character.Stats.Willpower < 100)
                        {
                            character.Stats.Willpower++;
                            pool--;
                        }
                        break;

                    case 8:
                        if (character.Stats.Sanity < 100)
                        {
                            character.Stats.Sanity++;
                            pool--;
                        }
                        break;

                    case 9:
                        if (character.Stats.Luck < 100)
                        {
                            character.Stats.Luck++;
                            pool--;
                        }
                        break;
                }
            }
        }

        public static List<BodyPart> BodyParts()
        {
            List<BodyPart> parts = new List<BodyPart>
            {
                GenBodyPart("Head", "Head"),
                GenBodyPart("Neck", "Neck"),
                GenBodyPart("Torso", "Torso"),
                GenBodyPart("Right_Arm", "Right Arm"),
                GenBodyPart("Right_Hand", "Right Hand"),
                GenBodyPart("Left_Arm", "Left Arm"),
                GenBodyPart("Left_Hand", "Left Hand"),
                GenBodyPart("Groin", "Groin"),
                GenBodyPart("Right_Leg", "Right Leg"),
                GenBodyPart("Right_Foot", "Right Foot"),
                GenBodyPart("Left_Leg", "Left Leg"),
                GenBodyPart("Left_Foot", "Left Foot")
            };

            return parts;
        }

        public static BodyPart GenBodyPart(string name, string description)
        {
            return new BodyPart
            {
                Name = name,
                Description = description,
                Stats = new List<Property>
                {
                    new Property
                    {
                        Name = "HP",
                        Max_Value = 100,
                        Value = 100
                    },
                    new Property
                    {
                        Name = "Pain",
                        Max_Value = 100,
                        Value = 0
                    },
                    new Property
                    {
                        Name = "Blood Loss",
                        Max_Value = 100,
                        Value = 0
                    }
                }
            };
        }

        public static string BodyPartToName(string body_part)
        {
            if (body_part == "Head")
            {
                return "Head";
            }
            else if (body_part == "Neck")
            {
                return "Neck";
            }
            else if (body_part == "Torso")
            {
                return "Torso";
            }
            else if (body_part == "Groin")
            {
                return "Groin";
            }
            else if (body_part == "Right_Arm")
            {
                return "Right Arm";
            }
            else if (body_part == "Right_Hand")
            {
                return "Right Hand";
            }
            else if (body_part == "Left_Arm")
            {
                return "Left Arm";
            }
            else if (body_part == "Left_Hand")
            {
                return "Left Hand";
            }
            else if (body_part == "Right_Leg")
            {
                return "Right Leg";
            }
            else if (body_part == "Right_Foot")
            {
                return "Right Foot";
            }
            else if (body_part == "Left_Leg")
            {
                return "Left Leg";
            }
            else if (body_part == "Left_Foot")
            {
                return "Left Foot";
            }

            return null;
        }

        public static string BodyPartFromName(string body_part)
        {
            if (body_part == "Head")
            {
                return "Head";
            }
            else if (body_part == "Neck")
            {
                return "Neck";
            }
            else if (body_part == "Torso")
            {
                return "Torso";
            }
            else if (body_part == "Groin")
            {
                return "Groin";
            }
            else if (body_part == "Right Arm")
            {
                return "Right_Arm";
            }
            else if (body_part == "Right Hand")
            {
                return "Right_Hand";
            }
            else if (body_part == "Left Arm")
            {
                return "Left_Arm";
            }
            else if (body_part == "Left Hand")
            {
                return "Left_Hand";
            }
            else if (body_part == "Right Leg")
            {
                return "Right_Leg";
            }
            else if (body_part == "Right Foot")
            {
                return "Right_Foot";
            }
            else if (body_part == "Left Leg")
            {
                return "Left_Leg";
            }
            else if (body_part == "Left Foot")
            {
                return "Left_Foot";
            }

            return null;
        }

        public static List<Property> Traits(Character character)
        {
            List<Property> traits = new List<Property>();

            int STR = (int)character.Stats.Strength;
            int END = (int)character.Stats.Endurance;
            int AGI = (int)character.Stats.Agility;
            int INT = (int)character.Stats.Intelligence;
            int PER = (int)character.Stats.Perception;
            int CHA = (int)character.Stats.Charisma;
            int WIL = (int)character.Stats.Willpower;
            int SAN = (int)character.Stats.Sanity;
            int LUK = (int)character.Stats.Luck;

            //Fight or flight?
            if (STR > 50)
            {
                if (INT > 50)
                {
                    //Grab nearest weapon and defend
                    traits.Add(new Property() { Name = "Assertive" });

                    if (SAN <= 50)
                    {
                        //They already knew where the nearest weapon was
                        traits.Add(new Property() { Name = "Prepared" });
                    }
                    else if (SAN > 50)
                    {
                        //Anything nearby will do
                        traits.Add(new Property() { Name = "Adaptive" });
                    }
                }
                else if (INT <= 50)
                {
                    //Attack without warning or preparation
                    traits.Add(new Property() { Name = "Aggressive" });

                    if (SAN <= 50)
                    {
                        //They always have a weapon to threaten others with
                        traits.Add(new Property() { Name = "Bully" });
                    }
                    else if (SAN > 50)
                    {
                        //Unlikely to have a weapon, just reckless abandon
                        traits.Add(new Property() { Name = "Guardian" });
                    }
                }
            }
            else if (STR <= 50)
            {
                if (INT > 50)
                {
                    //Flee, find a weapon, and attack
                    traits.Add(new Property() { Name = "Passive-Aggressive" });

                    if (SAN <= 50)
                    {
                        //They'll just keep coming after you
                        traits.Add(new Property() { Name = "Vengeful" });
                    }
                    else if (SAN > 50)
                    {
                        //They just want you to leave
                        traits.Add(new Property() { Name = "Forgiving" });
                    }
                }
                else if (INT <= 50)
                {
                    //Flee and defend
                    traits.Add(new Property() { Name = "Passive" });

                    if (SAN <= 50)
                    {
                        //They're probably hiding in the closet
                        traits.Add(new Property() { Name = "Coward" });
                    }
                    else if (SAN > 50)
                    {
                        //They're probably hiding in the closet where they keep the shotgun
                        traits.Add(new Property() { Name = "Survivalist" });
                    }
                }
            }

            //How open are they to conversation?
            if (CHA > 50)
            {
                //They're fine with speaking

                if (INT > 50)
                {
                    //They speak when spoken to, otherwise very quiet
                    traits.Add(new Property() { Name = "Low Introvert" });
                }
                else if (INT <= 50)
                {
                    //They never seem to shut up
                    traits.Add(new Property() { Name = "High Extrovert" });
                }
            }
            else if (CHA <= 50)
            {
                //They prefer not to speak

                if (INT > 50)
                {
                    //Very shy, takes a long time to open up
                    traits.Add(new Property() { Name = "High Introvert" });
                }
                else if (INT <= 50)
                {
                    //Quiet until they start rambling
                    traits.Add(new Property() { Name = "Low Extrovert" });
                }
            }

            //How quickly do they notice the weapon in your hand?
            if (AGI > 50)
            {
                if (PER > 50)
                {
                    //They notice and react immediately
                    traits.Add(new Property() { Name = "Observant" });
                }
                else if (PER <= 50)
                {
                    //Slow to notice, but reacts quickly
                    traits.Add(new Property() { Name = "Reactive" });
                }
            }
            else if (AGI <= 50)
            {
                if (PER > 50)
                {
                    //They'll see it, but slow to react
                    traits.Add(new Property() { Name = "Focused" });
                }
                else if (PER <= 50)
                {
                    //Never sees it coming
                    traits.Add(new Property() { Name = "Clumsy" });
                }
            }

            //How long will they put up a fight?
            if (END > 50)
            {
                if (WIL > 50)
                {
                    //They won't stop coming at you
                    traits.Add(new Property() { Name = "Heroic" });
                }
                else if (WIL <= 50)
                {
                    //Quickly concedes from any wounds/pain
                    traits.Add(new Property() { Name = "Quitter" });
                }
            }
            else if (END <= 50)
            {
                if (WIL > 50)
                {
                    //Will fight back until they run out of steam
                    traits.Add(new Property() { Name = "Defiant" });
                }
                else if (WIL <= 50)
                {
                    //Won't even bother trying, just accepts their fate
                    traits.Add(new Property() { Name = "Accepting" });
                }
            }

            //Will they take a risk?
            if (INT > 50)
            {
                if (LUK <= 50)
                {
                    //Doesn't take risks
                    traits.Add(new Property() { Name = "Conservative" });
                }
                else if (LUK > 50)
                {
                    //Will take a risk when the reward chance seems high
                    traits.Add(new Property() { Name = "Opportunist" });
                }
            }
            else if (INT <= 50)
            {
                if (LUK <= 50)
                {
                    //Takes risks despite failing often... they must enjoy failing?
                    traits.Add(new Property() { Name = "Masochist" });
                }
                else if (LUK > 50)
                {
                    //Risky and reckless, because they usually win
                    traits.Add(new Property() { Name = "Gambler" });
                }
            }

            return traits;
        }

        public static void LoadStats(Character character, Dictionary<string, int> stats)
        {
            ResetStats(character);

            foreach (var stat in stats)
            {
                switch(stat.Key)
                {
                    case "Strength":
                        character.Stats.Strength = stat.Value;
                        break;

                    case "Vitality":
                        character.Stats.Vitality = stat.Value;
                        break;

                    case "Endurance":
                        character.Stats.Endurance = stat.Value;
                        break;

                    case "Agility":
                        character.Stats.Agility = stat.Value;
                        break;

                    case "Intelligence":
                        character.Stats.Intelligence = stat.Value;
                        break;

                    case "Perception":
                        character.Stats.Perception = stat.Value;
                        break;

                    case "Charisma":
                        character.Stats.Charisma = stat.Value;
                        break;

                    case "Willpower":
                        character.Stats.Willpower = stat.Value;
                        break;

                    case "Sanity":
                        character.Stats.Sanity = stat.Value;
                        break;

                    case "Luck":
                        character.Stats.Luck = stat.Value;
                        break;
                }
            }

            foreach (var part in BodyParts())
            {
                character.BodyParts.Add(part);
            }
        }

        public static void UpdateSight(Character character)
        {
            Handler.VisibleTiles.Remove(character.ID);
            List<Vector2> locations = new List<Vector2>();

            Point starting = new Point((int)character.Location.X, (int)character.Location.Y);

            Scene gameplay = SceneManager.GetScene("Gameplay");
            Map map = gameplay.World.Maps[0];
            Layer bottom_tiles = map.GetLayer("BottomTiles");
            Layer middle_tiles = map.GetLayer("MiddleTiles");

            Vector2 left_corner;
            Vector2 right_corner;

            if (character.Direction == Direction.North)
            {
                #region North

                left_corner = new Vector2(character.Location.X - (Handler.SightDistance * 2), character.Location.Y - Handler.SightDistance);
                right_corner = new Vector2(character.Location.X + (Handler.SightDistance * 2) + 2, character.Location.Y - Handler.SightDistance);

                for (int x = (int)left_corner.X; x <= right_corner.X; x++)
                {
                    Point dest = new Point(x, (int)left_corner.Y);

                    List<Point> points = Utility.GetLine(starting, dest);

                    bool reverse = points[0] != starting;

                    int pointCount = points.Count;
                    for (int i = 0; i < pointCount; i++)
                    {
                        //Reverse direction if first point is not where we started
                        int index = reverse ? (pointCount - 1) - i : i;

                        Point point = points[index];
                        int X = point.X;
                        int Y = point.Y;

                        Vector2 current_location = new Vector2(X, Y);
                        Tile bottom_tile = bottom_tiles.GetTile(current_location);

                        if (!locations.Contains(current_location))
                        {
                            locations.Add(current_location);
                        }

                        if (bottom_tile != null &&
                            bottom_tile.BlocksMovement)
                        {
                            break;
                        }
                        else
                        {
                            Tile middle_tile = middle_tiles.GetTile(current_location);
                            if (middle_tile != null &&
                                middle_tile.BlocksSight &&
                                !middle_tile.Name.Contains("Open"))
                            {
                                break;
                            }
                        }
                    }
                }

                #endregion
            }
            else if (character.Direction == Direction.East)
            {
                #region East

                left_corner = new Vector2(character.Location.X + Handler.SightDistance, character.Location.Y - (Handler.SightDistance * 2));
                right_corner = new Vector2(character.Location.X + Handler.SightDistance, character.Location.Y + (Handler.SightDistance * 2) + 2);

                for (int y = (int)left_corner.Y; y <= right_corner.Y; y++)
                {
                    Point dest = new Point((int)left_corner.X, y);

                    List<Point> points = Utility.GetLine(starting, dest);

                    bool reverse = points[0] != starting;

                    int pointCount = points.Count;
                    for (int i = 0; i < pointCount; i++)
                    {
                        //Reverse direction if first point is not where we started
                        int index = reverse ? (pointCount - 1) - i : i;

                        Point point = points[index];
                        int X = point.X;
                        int Y = point.Y;

                        Vector2 current_location = new Vector2(X, Y);
                        Tile bottom_tile = bottom_tiles.GetTile(current_location);

                        if (!locations.Contains(current_location))
                        {
                            locations.Add(current_location);
                        }

                        if (bottom_tile != null &&
                            bottom_tile.BlocksMovement)
                        {
                            break;
                        }
                        else
                        {
                            Tile middle_tile = middle_tiles.GetTile(current_location);
                            if (middle_tile != null &&
                                middle_tile.BlocksSight &&
                                !middle_tile.Name.Contains("Open"))
                            {
                                break;
                            }
                        }
                    }
                }

                #endregion
            }
            else if (character.Direction == Direction.South)
            {
                #region South

                left_corner = new Vector2(character.Location.X - (Handler.SightDistance * 2), character.Location.Y + Handler.SightDistance);
                right_corner = new Vector2(character.Location.X + (Handler.SightDistance * 2) + 2, character.Location.Y + Handler.SightDistance);

                for (int x = (int)left_corner.X; x <= right_corner.X; x++)
                {
                    Point dest = new Point(x, (int)left_corner.Y);

                    List<Point> points = Utility.GetLine(starting, dest);

                    bool reverse = points[0] != starting;

                    int pointCount = points.Count;
                    for (int i = 0; i < pointCount; i++)
                    {
                        //Reverse direction if first point is not where we started
                        int index = reverse ? (pointCount - 1) - i : i;

                        Point point = points[index];
                        int X = point.X;
                        int Y = point.Y;

                        Vector2 current_location = new Vector2(X, Y);
                        Tile bottom_tile = bottom_tiles.GetTile(current_location);

                        if (!locations.Contains(current_location))
                        {
                            locations.Add(current_location);
                        }

                        if (bottom_tile != null &&
                            bottom_tile.BlocksMovement)
                        {
                            break;
                        }
                        else
                        {
                            Tile middle_tile = middle_tiles.GetTile(current_location);
                            if (middle_tile != null &&
                                middle_tile.BlocksSight &&
                                !middle_tile.Name.Contains("Open"))
                            {
                                break;
                            }
                        }
                    }
                }

                #endregion
            }
            else if (character.Direction == Direction.West)
            {
                #region West

                left_corner = new Vector2(character.Location.X - Handler.SightDistance, character.Location.Y + (Handler.SightDistance * 2) + 2);
                right_corner = new Vector2(character.Location.X - Handler.SightDistance, character.Location.Y - (Handler.SightDistance * 2));

                for (int y = (int)right_corner.Y; y <= left_corner.Y; y++)
                {
                    Point dest = new Point((int)left_corner.X, y);

                    List<Point> points = Utility.GetLine(starting, dest);

                    bool reverse = points[0] != starting;

                    int pointCount = points.Count;
                    for (int i = 0; i < pointCount; i++)
                    {
                        //Reverse direction if first point is not where we started
                        int index = reverse ? (pointCount - 1) - i : i;

                        Point point = points[index];
                        int X = point.X;
                        int Y = point.Y;

                        Vector2 current_location = new Vector2(X, Y);
                        Tile bottom_tile = bottom_tiles.GetTile(current_location);

                        if (!locations.Contains(current_location))
                        {
                            locations.Add(current_location);
                        }

                        if (bottom_tile != null &&
                            bottom_tile.BlocksMovement)
                        {
                            break;
                        }
                        else
                        {
                            Tile middle_tile = middle_tiles.GetTile(current_location);
                            if (middle_tile != null &&
                                middle_tile.BlocksSight &&
                                !middle_tile.Name.Contains("Open"))
                            {
                                break;
                            }
                        }
                    }
                }

                #endregion
            }

            List<Tile> tiles = new List<Tile>();
            Texture2D selection = AssetManager.Textures["Selection"];

            int count = locations.Count;
            for (int i = 0; i < count; i++)
            {
                Vector2 location = locations[i];
                Location tileLocation = new Location(location.X, location.Y, 0);

                Tile new_tile = new Tile
                {
                    Location = tileLocation,
                    Texture = selection,
                    Image = new Rectangle(0, 0, selection.Width, selection.Height)
                };

                Tile existing = bottom_tiles.GetTile(location);
                if (existing != null)
                {
                    new_tile.Region = existing.Region;
                }

                new_tile.DrawColor = Color.White;
                tiles.Add(new_tile);
            }

            Handler.VisibleTiles.Add(character.ID, tiles);
        }

        public static void UpdateGear(Character character)
        {
            foreach (Item item in character.Inventory.Items)
            {
                if (item.Texture != null)
                {
                    item.Image = character.Image;
                    item.Region = character.Region;
                }
            }
        }

        public static ProgressBar GenTaskbar(Character character, int max_value)
        {
            Texture2D baseTexture = AssetManager.Textures["ProgressBase"];

            return new ProgressBar
            {
                Max_Value = max_value,
                Value = 0,
                Rate = 1,
                Base_Texture = baseTexture,
                Bar_Texture = AssetManager.Textures["ProgressBar"],
                Bar_Image = new Rectangle(0, 0, 0, baseTexture.Height),
                Base_Region = new Region(character.Region.X + (Main.Game.TileSize.X / 8), character.Region.Y + character.Region.Height - (Main.Game.TileSize.Y / 4), Main.Game.TileSize.X - (Main.Game.TileSize.X / 4), Main.Game.TileSize.Y / 8),
                DrawColor = new Color(0, 255, 0, 255),
                Visible = true
            };
        }

        public static string HisHer(Character character)
        {
            string his_her = "His";
            if (character.Gender == "Female")
            {
                his_her = "Her";
            }

            return his_her;
        }

        public static bool PulledByPlayer(Character character)
        {
            if (Handler.Pull &&
                Handler.Pull_ID == character.ID)
            {
                return true;
            }

            return false;
        }

        public static void UpdateConsciousness(Character character)
        {
            if (character.Stats.Stamina <= 0)
            {
                character.Stats.Consciousness++;
                if (character.Stats.Consciousness > 100)
                {
                    character.Stats.Consciousness = 100;
                }
            }
            if (character.Stats.Pain >= 100)
            {
                character.Stats.Consciousness -= 5;
                if (character.Stats.Consciousness < 0)
                {
                    character.Stats.Consciousness = 0;
                }
            }

            if (character.Stats.Pain < 100 &&
                character.Stats.Stamina > 0)
            {
                character.Stats.Consciousness++;
                if (character.Stats.Consciousness > 100)
                {
                    character.Stats.Consciousness = 100;
                }
            }

            if (character.Stats.Consciousness <= 0 &&
                !character.Unconscious)
            {
                character.Unconscious = true;

                if (character.Type == "Player")
                {
                    GameUtil.AddMessage("You fell unconscious.");
                }
            }
            else if (character.Stats.Consciousness >= 20 &&
                     character.Unconscious)
            {
                character.Unconscious = false;

                if (character.Type == "Player")
                {
                    GameUtil.AddMessage("You regained consciousness.");
                }
            }
        }

        public static void UpdatePain(Character character)
        {
            float total = 0;

            foreach (BodyPart part in character.BodyParts)
            {
                Property hp = part.GetStat("HP");

                Property pain = part.GetStat("Pain");
                pain.Value = 0;
                       
                foreach (Wound wound in part.Wounds)
                {
                    if (wound.Name == "Break")
                    {
                        if (part.Name == "Head")
                        {
                            pain.Value += 50;
                        }
                        else
                        {
                            pain.Value += 20;
                        }
                    }
                    else if (wound.Name == "Covered")
                    {
                        pain.Value += 2;
                    }
                    else if (wound.Name == "Cut")
                    {
                        pain.Value += 4;
                    }
                    else if (wound.Name == "Fracture")
                    {
                        if (part.Name == "Head")
                        {
                            pain.Value += 25;
                        }
                        else
                        {
                            pain.Value += 10;
                        }
                    }
                    else if (wound.Name == "Gunshot")
                    {
                        if (part.Name == "Head")
                        {
                            pain.Value += 100;
                        }
                        else
                        {
                            pain.Value += 20;
                        }
                    }
                    else if (wound.Name == "Stab")
                    {
                        if (part.Name == "Head")
                        {
                            pain.Value += 50;
                        }
                        else
                        {
                            pain.Value += 20;
                        }
                    }
                    else if (wound.Name == "Stitched")
                    {
                        pain.Value += 4;
                    }
                    else if (wound.Name == "Set")
                    {
                        pain.Value += 10;
                    }
                    else if (wound.Name == "Burn")
                    {
                        pain.Value += 50;
                    }
                    else if (wound.Name == "Sever")
                    {
                        pain.Value += 80;
                    }
                    else if (wound.Name == "Bruise")
                    {
                        if (part.Name == "Head")
                        {
                            pain.Value += 20;
                        }
                        else
                        {
                            pain.Value += 5;
                        }
                    }
                }

                if (pain.Value > pain.Max_Value)
                {
                    pain.Value = pain.Max_Value;
                }

                hp.Value = 100 - pain.Value;
                if (part.Name == "Head" &&
                    hp.Value <= 0)
                {
                    character.Stats.Consciousness = 0;
                }

                total += pain.Value;
            }

            Property painKiller = character.GetStatusEffect("Painkillers");
            if (painKiller != null)
            {
                total -= painKiller.Value;
            }

            Property adrenaline = character.GetStatusEffect("Adrenaline");
            if (adrenaline != null)
            {
                total -= painKiller.Value;
            }

            character.Stats.Pain = total;
            if (character.Stats.Pain > 100)
            {
                character.Stats.Pain = 100;
            }
        }

        public static void UpdateWounds(Character character)
        {
            int partCount = character.BodyParts.Count;
            for (int b = 0; b < partCount; b++)
            { 
                BodyPart part = character.BodyParts[b];

                for (int w = 0; w < part.Wounds.Count; w++)
                {
                    Wound wound = part.Wounds[w];
                    if (wound != null)
                    {
                        if (wound.Name != "Sever")
                        {
                            float heal_rate = character.Stats.Vitality / 50;
                            wound.Value -= heal_rate;

                            if (wound.Value <= 0)
                            {
                                part.Wounds.Remove(wound);
                            }
                        }
                    }
                }
            }
        }

        public static void UpdateBloodLoss(Character character)
        {
            float total = 0;

            foreach (BodyPart part in character.BodyParts)
            {
                if (total < 100)
                {
                    foreach (Wound wound in part.Wounds)
                    {
                        if (wound.Name == "Stitched")
                        {
                            total += 0.000024f; //2 per day
                        }
                        else if(wound.Name == "Covered")
                        {
                            total += 0.000058f; //5 per day
                        }
                        else if (wound.Name == "Cut")
                        {
                            total += 0.00012f; //10 per day
                        }
                        else if (wound.Name == "Stab")
                        {
                            total += 0.0029f; //10 per hour
                        }
                        else if (wound.Name == "Gunshot")
                        {
                            total += 0.00579f; //20 per hour
                        }
                        else if (wound.Name == "Sever")
                        {
                            if (part.Name == "Head")
                            {
                                character.Dead = true;
                                total = 100;
                                break;
                            }
                            else
                            {
                                total += 0.01389f; //50 per hour
                            }
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            if (total > 0)
            {
                character.Stats.Blood -= total;
                if (character.Stats.Blood < 0)
                {
                    character.Stats.Blood = 0;
                }

                Map map = WorldUtil.GetMap();
                if (map != null)
                {
                    Layer bottom_tiles = map.GetLayer("BottomTiles");

                    Layer effect_tiles = map.GetLayer("EffectTiles");
                    if (effect_tiles != null)
                    {
                        Tile blood = null;

                        bool trail_north_found = false;
                        bool trail_east_found = false;
                        bool trail_south_found = false;
                        bool trail_west_found = false;

                        foreach (Tile existing in effect_tiles.Tiles)
                        {
                            if (existing.Location.X == character.Location.X &&
                                existing.Location.Y == character.Location.Y)
                            {
                                if (!string.IsNullOrEmpty(existing.Name))
                                {
                                    if (existing.Name.Contains("Blood"))
                                    {
                                        existing.Value += total;
                                        if (existing.Value > 1)
                                        {
                                            existing.Value = 1;
                                        }

                                        if (!existing.Name.Contains("Trail") &&
                                            !existing.Name.Contains("Prints"))
                                        {
                                            blood = existing;
                                        }
                                        else if (existing.Name.Contains("Trail"))
                                        {
                                            if (existing.Name.Contains("North"))
                                            {
                                                trail_north_found = true;
                                            }
                                            else if (existing.Name.Contains("East"))
                                            {
                                                trail_east_found = true;
                                            }
                                            else if (existing.Name.Contains("South"))
                                            {
                                                trail_south_found = true;
                                            }
                                            else if (existing.Name.Contains("West"))
                                            {
                                                trail_west_found = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (blood != null)
                        {
                            if (blood.Value > 0.05f)
                            {             
                                if (blood.Value < 0.1f)
                                {
                                    blood.Name = "Small Blood";
                                    blood.Texture = AssetManager.Textures["Blood_Small"];
                                }
                                else if (blood.Value < 1)
                                {
                                    blood.Name = "Medium Blood";
                                    blood.Texture = AssetManager.Textures["Blood_Medium"];
                                }
                                else if (blood.Value < 10)
                                {
                                    blood.Name = "Large Blood";
                                    blood.Texture = AssetManager.Textures["Blood_Large"];
                                }
                                else if (blood.Value < 100)
                                {
                                    blood.Name = "Pool Blood";
                                    blood.Texture = AssetManager.Textures["Blood_Pool"];
                                }
                                else if (blood.Value >= 1000)
                                {
                                    blood.Name = "Puddle Blood";
                                    blood.Texture = AssetManager.Textures["Blood_Puddle"];
                                }

                                if (blood.Texture != null)
                                {
                                    blood.Image = new Rectangle(0, 0, blood.Texture.Width, blood.Texture.Height);
                                    blood.Visible = true;
                                }
                            }
                        }
                        else
                        {
                            WorldUtil.AddEffect(character.Location.ToVector3, "Tiny Blood", null);
                        }

                        if (character.Moving &&
                            total >= 0.001f)
                        {
                            if (character.Direction == Direction.North &&
                                !trail_north_found)
                            {
                                WorldUtil.AddEffect(character.Location.ToVector3, "Trail of Blood North", "Blood_Trail_North");
                            }
                            else if (character.Direction == Direction.East &&
                                     !trail_east_found)
                            {
                                WorldUtil.AddEffect(character.Location.ToVector3, "Trail of Blood East", "Blood_Trail_East");
                            }
                            else if (character.Direction == Direction.South &&
                                     !trail_south_found)
                            {
                                WorldUtil.AddEffect(character.Location.ToVector3, "Trail of Blood South", "Blood_Trail_South");
                            }
                            else if (character.Direction == Direction.West &&
                                     !trail_west_found)
                            {
                                WorldUtil.AddEffect(character.Location.ToVector3, "Trail of Blood West", "Blood_Trail_West");
                            }
                        }
                    }
                }
            }
        }

        public static float GetTurnTime(Character character)
        {
            return 5000 / character.Stats.Agility;
        }

        public static float GetStandTime(Character character)
        {
            float baseTime = 20000; //Default 20 seconds
            float painTime = character.Stats.Pain * 200; //Extra 20 seconds max
            float fatigueTime = (100 - character.Stats.Stamina) * 200; //Extra 20 seconds max

            return baseTime + painTime + fatigueTime; //60 seconds max
        }

        public static void Sleep(Character character)
        {
            if (character.Stats.Stamina < 100)
            {
                //Per second
                character.Stats.Stamina += 0.00348f;
                if (character.Stats.Stamina > 100)
                {
                    character.Stats.Stamina = 100;
                }
            }
        }

        public static void Rest(Character character)
        {
            if (character.Stats.Stamina < 100)
            {
                //Per millisecond
                character.Stats.Stamina += 0.00002f;
                if (character.Stats.Stamina > 100)
                {
                    character.Stats.Stamina = 100;
                }
            }
        }

        public static void Kill(Character character)
        {
            character.Unconscious = false;
            character.Dead = true;
        }
    }
}

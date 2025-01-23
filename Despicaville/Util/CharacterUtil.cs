using System.Collections.Generic;

using Microsoft.Xna.Framework;

using OP_Engine.Characters;
using OP_Engine.Controls;
using OP_Engine.Inventories;
using OP_Engine.Jobs;
using OP_Engine.Scenes;
using OP_Engine.Tiles;
using OP_Engine.Utility;

namespace Despicaville.Util
{
    public static class CharacterUtil
    {
        public static Character GenCharacter(string last_name)
        {
            Character character = new Character();
            character.ID = Handler.GetID();
            character.Inventory.ID = Handler.GetID();
            character.Type = "Citizen";
            character.Animator.Frames = 4;
            character.Speed = 1;
            character.Move_TotalDistance = Main.Game.TileSize.X;
            character.Direction = Direction.Down;
            character.Visible = true;
            character.Job.OwnerIDs.Add(character.ID);
            JobManager.Jobs.Add(character.Job);
            InventoryManager.Inventories.Add(character.Inventory);

            //Get Stats
            foreach (var stat in GenStats())
            {
                character.Stats.Add(stat);
            }

            //Get body parts
            foreach (var part in BodyParts())
            {
                character.BodyParts.Add(part);
            }

            //Get personality traits
            foreach (var trait in Traits(character))
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
                Item hair = new Item();
                hair.Equipped = true;
                hair.Image = character.Image;
                hair.Region = character.Region;
                hair.Visible = true;

                random = new CryptoRandom();
                int hair_color = random.Next(0, Handler.HairColor.Length);
                hair.DrawColor = GameUtil.ColorFromName(Handler.HairColor[hair_color]);

                hair.Type = "Hair_" + Handler.HairLength[hair_length];
                hair.Texture = AssetManager.Textures[hair.Type];

                hair.Name = Handler.HairLength[hair_length] + " " + Handler.HairColor[hair_color] + " Hair";

                character.Inventory.Items.Add(hair);
            }

            if (Utility.RandomPercent(10))
            {
                Item hat = new Item();
                hat.ID = Handler.GetID();
                hat.Type = "Hat";
                hat.Equipped = true;
                hat.Assignment = "Hat Slot";
                hat.Icon = AssetManager.Textures["Hat_Cap"];
                hat.Icon_Image = new Rectangle(0, 0, hat.Icon.Width, hat.Icon.Height);
                hat.Icon_Visible = true;
                hat.Texture = AssetManager.Textures["Hat"];
                hat.Image = character.Image;
                hat.Region = character.Region;
                hat.Visible = true;
                character.Inventory.Items.Add(hat);

                random = new CryptoRandom();
                int hat_color = random.Next(0, Handler.Colors.Length);
                hat.DrawColor = GameUtil.ColorFromName(Handler.Colors[hat_color]);
                hat.Icon_DrawColor = GameUtil.ColorFromName(Handler.Colors[hat_color]);
                hat.Name = Handler.Colors[hat_color] + " Hat";
            }

            //Get Shirt
            Item shirt = new Item();
            shirt.ID = Handler.GetID();
            shirt.Type = "Shirt";
            shirt.Icon = AssetManager.Textures["Shirt_T"];
            shirt.Icon_Image = new Rectangle(0, 0, shirt.Icon.Width, shirt.Icon.Height);
            shirt.Icon_Visible = true;
            shirt.Texture = AssetManager.Textures["Shirt"];
            shirt.Image = character.Image;
            shirt.Region = character.Region;
            shirt.Equipped = true;
            shirt.Assignment = "Shirt Slot";
            shirt.Visible = true;
            character.Inventory.Items.Add(shirt);

            random = new CryptoRandom();
            int shirt_color = random.Next(0, Handler.Colors.Length);
            shirt.DrawColor = GameUtil.ColorFromName(Handler.Colors[shirt_color]);
            shirt.Icon_DrawColor = GameUtil.ColorFromName(Handler.Colors[shirt_color]);
            shirt.Name = Handler.Colors[shirt_color] + " Shirt";

            //Get Pants
            Item pants = new Item();
            pants.ID = Handler.GetID();
            pants.Type = "Pants";
            pants.Icon = AssetManager.Textures["Pants_Plain"];
            pants.Icon_Image = new Rectangle(0, 0, pants.Icon.Width, pants.Icon.Height);
            pants.Icon_Visible = true;
            pants.Region = character.Region;
            pants.Equipped = true;
            pants.Assignment = "Pants Slot";
            pants.Visible = false;
            pants.Inventory.ID = Handler.GetID();
            pants.Inventory.Name = pants.Name;
            pants.Inventory.Weight = 4;
            pants.Inventory.Max_Value = 4;
            InventoryManager.Inventories.Add(pants.Inventory);
            character.Inventory.Items.Add(pants);

            random = new CryptoRandom();
            int pants_color = random.Next(0, Handler.Colors.Length);
            pants.DrawColor = GameUtil.ColorFromName(Handler.Colors[pants_color]);
            pants.Icon_DrawColor = GameUtil.ColorFromName(Handler.Colors[pants_color]);
            pants.Name = Handler.Colors[pants_color] + " Pants";

            //Get Shoes
            Item shoes = new Item();
            shoes.ID = Handler.GetID();
            shoes.Type = "Shoes";
            shoes.Icon = AssetManager.Textures["Shoes_Plain"];
            shoes.Icon_Image = new Rectangle(0, 0, shoes.Icon.Width, shoes.Icon.Height);
            shoes.Icon_Visible = true;
            shoes.Region = character.Region;
            shoes.Equipped = true;
            shoes.Assignment = "Shoes Slot";
            shoes.Visible = false;
            character.Inventory.Items.Add(shoes);

            random = new CryptoRandom();
            int shoes_color = random.Next(0, Handler.Colors.Length);
            shoes.DrawColor = GameUtil.ColorFromName(Handler.Colors[shoes_color]);
            shoes.Icon_DrawColor = GameUtil.ColorFromName(Handler.Colors[shoes_color]);
            shoes.Name = Handler.Colors[shoes_color] + " Shoes";

            character.OnHearSomething += AI.ReactToNoise;
            character.OnSeeSomething += AI.ReactToMovement;
            character.OnSmellSomething += AI.ReactToSmell;

            return character;
        }

        public static List<Something> BaseStats()
        {
            List<Something> stats = new List<Something>();

            Something stat = new Something();
            stat.Name = "Blood";
            stat.Description = "How much blood you have left.\n0% means you are dead.";
            stat.Max_Value = 100;
            stat.Value = 100;
            stats.Add(stat);

            stat = new Something();
            stat.Name = "Consciousness";
            stat.Description = "How conscious you are.\n0% means you are unconscious.";
            stat.Max_Value = 100;
            stat.Value = 100;
            stats.Add(stat);

            stat = new Something();
            stat.Name = "Stamina";
            stat.Description = "How much energy you have.\nConsciousness -1 every second at 0%.";
            stat.Max_Value = 200;
            stat.Value = 100;
            stats.Add(stat);

            stat = new Something();
            stat.Name = "Comfort";
            stat.Description = "How comfortable you are.\nMax Stamina -1 every minute at 0%.";
            stat.Max_Value = 100;
            stat.Value = 100;
            stat.Rate = 0.00556f;
            stats.Add(stat);

            stat = new Something();
            stat.Name = "Paranoia";
            stat.Description = "How paranoid you are.\nYou will commit suicide at 100%.";
            stat.Max_Value = 100;
            stat.Value = 0;
            stats.Add(stat);

            stat = new Something();
            stat.Name = "Hunger";
            stat.Description = "How hungry you are.\nMax Stamina -1 every hour at 100%.";
            stat.Max_Value = 100;
            stat.Rate = 0.0006945f;
            stat.Value = 30;
            stats.Add(stat);

            stat = new Something();
            stat.Name = "Thirst";
            stat.Description = "How thirsty you are.\nMax Blood -1 every hour at 100%.";
            stat.Max_Value = 100;
            stat.Rate = 0.0013888f;
            stat.Value = 60;
            stats.Add(stat);

            stat = new Something();
            stat.Name = "Disposition";
            stat.Description = "How well the player is liked.\nWill follow any command at 100%.";
            stat.Max_Value = 100;
            stat.Value = 0;
            stats.Add(stat);

            stat = new Something();
            stat.Name = "Depression";
            stat.Description = "How depressed you are.\nYou will commit suicide at 100%.";
            stat.Max_Value = 100;
            stat.Value = 0;
            stats.Add(stat);

            stat = new Something();
            stat.Name = "Boredom";
            stat.Description = "How bored you are.\nDepression +1 every hour at 100%.";
            stat.Max_Value = 100;
            stat.Rate = 0.00695f;
            stat.Value = 0;
            stats.Add(stat);

            stat = new Something();
            stat.Name = "Bladder";
            stat.Description = "How urgently you need a toilet.\nYou defecate yourself at 100%.";
            stat.Max_Value = 100;
            stat.Value = 0;
            stats.Add(stat);

            stat = new Something();
            stat.Name = "Grime";
            stat.Description = "How dirty you are.\nImpacts using your Charisma.";
            stat.Max_Value = 100;
            stat.Value = 0;
            stats.Add(stat);

            stat = new Something();
            stat.Name = "Pain";
            stat.Description = "How much pain you are feeling.\nConsciousness -5 every second at 100%.";
            stat.Max_Value = 100;
            stat.Value = 0;
            stats.Add(stat);

            return stats;
        }

        public static List<Something> GenStats()
        {
            List<Something> stats = new List<Something>();

            CryptoRandom random;

            int pool = 550;
            foreach (var new_stat in Handler.Stats)
            {
                Something stat = new Something();
                stat.Name = new_stat.Key;
                stat.Description = new_stat.Value;
                stat.Max_Value = 100;
                stats.Add(stat);

                random = new CryptoRandom();
                int stat_value = random.Next(1, 51);
                stat.Value = stat_value;
                pool -= stat_value;
            }

            for (int i = 0; i < pool; i++)
            {
                random = new CryptoRandom();
                int index = random.Next(0, stats.Count);

                Something stat = stats[index];
                if (stat.Value < 100)
                {
                    stat.Value++;
                    pool--;
                    if (pool == 0)
                    {
                        break;
                    }
                }
                else
                {
                    i--;
                }
            }

            foreach (var base_stat in BaseStats())
            {
                stats.Add(base_stat);
            }

            return stats;
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
            BodyPart part = new BodyPart();
            part.Name = name;
            part.Description = description;

            Something hp = new Something();
            hp.Name = "HP";
            hp.Max_Value = 100;
            hp.Value = 100;
            part.Stats.Add(hp);

            Something pain = new Something();
            pain.Name = "Pain";
            pain.Max_Value = 100;
            pain.Value = 0;
            part.Stats.Add(pain);

            Something blood_loss = new Something();
            blood_loss.Name = "Blood Loss";
            blood_loss.Max_Value = 100;
            blood_loss.Value = 0;
            part.Stats.Add(blood_loss);

            return part;
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

        public static List<Something> Traits(Character character)
        {
            List<Something> traits = new List<Something>();

            int STR = (int)character.GetStat("Strength").Value;
            int END = (int)character.GetStat("Endurance").Value;
            int AGI = (int)character.GetStat("Agility").Value;
            int INT = (int)character.GetStat("Intelligence").Value;
            int PER = (int)character.GetStat("Perception").Value;
            int CHA = (int)character.GetStat("Charisma").Value;
            int WIL = (int)character.GetStat("Willpower").Value;
            int SAN = (int)character.GetStat("Sanity").Value;
            int LUK = (int)character.GetStat("Luck").Value;

            //Fight or flight?
            if (STR > 50)
            {
                if (INT > 50)
                {
                    //Grab nearest weapon and defend
                    traits.Add(new Something() { Name = "Assertive" });

                    if (SAN <= 50)
                    {
                        //They already knew where the nearest weapon was
                        traits.Add(new Something() { Name = "Prepared" });
                    }
                    else if (SAN > 50)
                    {
                        //Anything nearby will do
                        traits.Add(new Something() { Name = "Adaptive" });
                    }
                }
                else if (INT <= 50)
                {
                    //Attack without warning or preparation
                    traits.Add(new Something() { Name = "Aggressive" });

                    if (SAN <= 50)
                    {
                        //They always have a weapon to threaten others with
                        traits.Add(new Something() { Name = "Bully" });
                    }
                    else if (SAN > 50)
                    {
                        //Unlikely to have a weapon, just reckless abandon
                        traits.Add(new Something() { Name = "Guardian" });
                    }
                }
            }
            else if (STR <= 50)
            {
                if (INT > 50)
                {
                    //Flee, find a weapon, and attack
                    traits.Add(new Something() { Name = "Passive-Aggressive" });

                    if (SAN <= 50)
                    {
                        //They'll just keep coming after you
                        traits.Add(new Something() { Name = "Vengeful" });
                    }
                    else if (SAN > 50)
                    {
                        //They just want you to leave
                        traits.Add(new Something() { Name = "Forgiving" });
                    }
                }
                else if (INT <= 50)
                {
                    //Flee and defend
                    traits.Add(new Something() { Name = "Passive" });

                    if (SAN <= 50)
                    {
                        //They're probably hiding in the closet
                        traits.Add(new Something() { Name = "Coward" });
                    }
                    else if (SAN > 50)
                    {
                        //They're probably hiding in the closet where they keep the shotgun
                        traits.Add(new Something() { Name = "Survivalist" });
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
                    traits.Add(new Something() { Name = "Low Introvert" });
                }
                else if (INT <= 50)
                {
                    //They never seem to shut up
                    traits.Add(new Something() { Name = "High Extrovert" });
                }
            }
            else if (CHA <= 50)
            {
                //They prefer not to speak

                if (INT > 50)
                {
                    //Very shy, takes a long time to open up
                    traits.Add(new Something() { Name = "High Introvert" });
                }
                else if (INT <= 50)
                {
                    //Quiet until they start rambling
                    traits.Add(new Something() { Name = "Low Extrovert" });
                }
            }

            //How quickly do they notice the weapon in your hand?
            if (AGI > 50)
            {
                if (PER > 50)
                {
                    //They notice and react immediately
                    traits.Add(new Something() { Name = "Observant" });
                }
                else if (PER <= 50)
                {
                    //Slow to notice, but reacts quickly
                    traits.Add(new Something() { Name = "Reactive" });
                }
            }
            else if (AGI <= 50)
            {
                if (PER > 50)
                {
                    //They'll see it, but slow to react
                    traits.Add(new Something() { Name = "Focused" });
                }
                else if (PER <= 50)
                {
                    //Never sees it coming
                    traits.Add(new Something() { Name = "Clumsy" });
                }
            }

            //How long will they put up a fight?
            if (END > 50)
            {
                if (WIL > 50)
                {
                    //They won't stop coming at you
                    traits.Add(new Something() { Name = "Heroic" });
                }
                else if (WIL <= 50)
                {
                    //Quickly concedes from any wounds/pain
                    traits.Add(new Something() { Name = "Quitter" });
                }
            }
            else if (END <= 50)
            {
                if (WIL > 50)
                {
                    //Will fight back until they run out of steam
                    traits.Add(new Something() { Name = "Defiant" });
                }
                else if (WIL <= 50)
                {
                    //Won't even bother trying, just accepts their fate
                    traits.Add(new Something() { Name = "Accepting" });
                }
            }

            //Will they take a risk?
            if (INT > 50)
            {
                if (LUK <= 50)
                {
                    //Doesn't take risks
                    traits.Add(new Something() { Name = "Conservative" });
                }
                else if (LUK > 50)
                {
                    //Will take a risk when the reward chance seems high
                    traits.Add(new Something() { Name = "Opportunist" });
                }
            }
            else if (INT <= 50)
            {
                if (LUK <= 50)
                {
                    //Takes risks despite failing often... they must enjoy failing?
                    traits.Add(new Something() { Name = "Masochist" });
                }
                else if (LUK > 50)
                {
                    //Risky and reckless, because they usually win
                    traits.Add(new Something() { Name = "Gambler" });
                }
            }

            return traits;
        }

        public static void LoadStats(Character character, Dictionary<string, int> stats)
        {
            character.Stats.Clear();

            foreach (var new_stat in stats)
            {
                Something stat = new Something();
                stat.Name = new_stat.Key;
                stat.Description = Handler.Stats[new_stat.Key];
                stat.Value = new_stat.Value;
                stat.Max_Value = 100;
                character.Stats.Add(stat);
            }

            foreach (var base_stat in BaseStats())
            {
                character.Stats.Add(base_stat);
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

            if (character.Direction == Direction.Up)
            {
                #region Up

                left_corner = new Vector2(character.Location.X - (Handler.SightDistance * 2), character.Location.Y - Handler.SightDistance);
                right_corner = new Vector2(character.Location.X + (Handler.SightDistance * 2) + 2, character.Location.Y - Handler.SightDistance);

                for (int x = (int)left_corner.X; x <= right_corner.X; x++)
                {
                    Point dest = new Point(x, (int)left_corner.Y);

                    List<Point> points = Utility.GetLine(starting, dest);

                    bool reverse = points[0] != starting;

                    for (int i = 0; i < points.Count; i++)
                    {
                        //Reverse direction if first point is not where we started
                        int index = reverse ? (points.Count - 1) - i : i;

                        Vector2 current_location = new Vector2(points[index].X, points[index].Y);
                        Tile bottom_tile = bottom_tiles.GetTile(current_location);
                        Tile middle_tile = middle_tiles.GetTile(current_location);

                        if (!locations.Contains(current_location))
                        {
                            locations.Add(current_location);
                        }

                        if (bottom_tile != null &&
                            bottom_tile.BlocksMovement)
                        {
                            break;
                        }
                        else if (middle_tile != null &&
                                 middle_tile.BlocksMovement &&
                                 WorldUtil.BlocksSight(middle_tile.Name))
                        {
                            break;
                        }
                    }
                }

                #endregion
            }
            else if (character.Direction == Direction.Right)
            {
                #region Right

                left_corner = new Vector2(character.Location.X + Handler.SightDistance, character.Location.Y - (Handler.SightDistance * 2));
                right_corner = new Vector2(character.Location.X + Handler.SightDistance, character.Location.Y + (Handler.SightDistance * 2) + 2);

                for (int y = (int)left_corner.Y; y <= right_corner.Y; y++)
                {
                    Point dest = new Point((int)left_corner.X, y);

                    List<Point> points = Utility.GetLine(starting, dest);

                    bool reverse = points[0] != starting;

                    for (int i = 0; i < points.Count; i++)
                    {
                        //Reverse direction if first point is not where we started
                        int index = reverse ? (points.Count - 1) - i : i;

                        Vector2 current_location = new Vector2(points[index].X, points[index].Y);
                        Tile bottom_tile = bottom_tiles.GetTile(current_location);
                        Tile middle_tile = middle_tiles.GetTile(current_location);

                        if (!locations.Contains(current_location))
                        {
                            locations.Add(current_location);
                        }

                        if (bottom_tile != null &&
                            bottom_tile.BlocksMovement)
                        {
                            break;
                        }
                        else if (middle_tile != null &&
                                 middle_tile.BlocksMovement &&
                                 WorldUtil.BlocksSight(middle_tile.Name))
                        {
                            break;
                        }
                    }
                }

                #endregion
            }
            else if (character.Direction == Direction.Down)
            {
                #region Down

                left_corner = new Vector2(character.Location.X - (Handler.SightDistance * 2), character.Location.Y + Handler.SightDistance);
                right_corner = new Vector2(character.Location.X + (Handler.SightDistance * 2) + 2, character.Location.Y + Handler.SightDistance);

                for (int x = (int)left_corner.X; x <= right_corner.X; x++)
                {
                    Point dest = new Point(x, (int)left_corner.Y);

                    List<Point> points = Utility.GetLine(starting, dest);

                    bool reverse = points[0] != starting;

                    for (int i = 0; i < points.Count; i++)
                    {
                        //Reverse direction if first point is not where we started
                        int index = reverse ? (points.Count - 1) - i : i;

                        Vector2 current_location = new Vector2(points[index].X, points[index].Y);
                        Tile bottom_tile = bottom_tiles.GetTile(current_location);
                        Tile middle_tile = middle_tiles.GetTile(current_location);

                        if (!locations.Contains(current_location))
                        {
                            locations.Add(current_location);
                        }

                        if (bottom_tile != null &&
                            bottom_tile.BlocksMovement)
                        {
                            break;
                        }
                        else if (middle_tile != null &&
                                 middle_tile.BlocksMovement &&
                                 WorldUtil.BlocksSight(middle_tile.Name))
                        {
                            break;
                        }
                    }
                }

                #endregion
            }
            else if (character.Direction == Direction.Left)
            {
                #region Left

                left_corner = new Vector2(character.Location.X - Handler.SightDistance, character.Location.Y + (Handler.SightDistance * 2) + 2);
                right_corner = new Vector2(character.Location.X - Handler.SightDistance, character.Location.Y - (Handler.SightDistance * 2));

                for (int y = (int)right_corner.Y; y <= left_corner.Y; y++)
                {
                    Point dest = new Point((int)left_corner.X, y);

                    List<Point> points = Utility.GetLine(starting, dest);

                    bool reverse = points[0] != starting;

                    for (int i = 0; i < points.Count; i++)
                    {
                        //Reverse direction if first point is not where we started
                        int index = reverse ? (points.Count - 1) - i : i;

                        Vector2 current_location = new Vector2(points[index].X, points[index].Y);
                        Tile bottom_tile = bottom_tiles.GetTile(current_location);
                        Tile middle_tile = middle_tiles.GetTile(current_location);

                        if (!locations.Contains(current_location))
                        {
                            locations.Add(current_location);
                        }

                        if (bottom_tile != null &&
                            bottom_tile.BlocksMovement)
                        {
                            break;
                        }
                        else if (middle_tile != null &&
                                 middle_tile.BlocksMovement &&
                                 WorldUtil.BlocksSight(middle_tile.Name))
                        {
                            break;
                        }
                    }
                }

                #endregion
            }

            List<Tile> tiles = new List<Tile>();
            foreach (Vector2 location in locations)
            {
                Tile new_tile = new Tile();
                new_tile.Location = new Location(location.X, location.Y, 0);
                new_tile.Texture = AssetManager.Textures["SelectionBox"];
                new_tile.Image = new Rectangle(0, 0, new_tile.Texture.Width, new_tile.Texture.Height);

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
            ProgressBar taskBar = new ProgressBar();
            taskBar.Max_Value = max_value;
            taskBar.Value = 0;
            taskBar.Rate = 1;
            taskBar.Base_Texture = AssetManager.Textures["ProgressBase"];
            taskBar.Bar_Texture = AssetManager.Textures["ProgressBar"];
            taskBar.Bar_Image = new Rectangle(0, 0, 0, taskBar.Base_Texture.Height);
            taskBar.Base_Region = new Region(character.Region.X + (Main.Game.TileSize.X / 8), character.Region.Y + character.Region.Height - (Main.Game.TileSize.Y / 4), Main.Game.TileSize.X - (Main.Game.TileSize.X / 4), Main.Game.TileSize.Y / 8);
            taskBar.DrawColor = new Color(0, 255, 0, 255);
            taskBar.Visible = true;
            return taskBar;
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

        public static float UpdatePain(Character character)
        {
            float total = 0;

            foreach (BodyPart part in character.BodyParts)
            {
                Something hp = part.GetStat("HP");

                Something pain = part.GetStat("Pain");
                pain.Value = 0;
                       
                foreach (Something wound in part.Wounds)
                {
                    if (wound.Name == "Break")
                    {
                        if (part.Name == "Head")
                        {
                            pain.IncreaseValue(50);
                        }
                        else
                        {
                            pain.IncreaseValue(20);
                        }
                    }
                    else if (wound.Name == "Covered")
                    {
                        pain.IncreaseValue(2);
                    }
                    else if (wound.Name == "Cut")
                    {
                        pain.IncreaseValue(4);
                    }
                    else if (wound.Name == "Fracture")
                    {
                        if (part.Name == "Head")
                        {
                            pain.IncreaseValue(25);
                        }
                        else
                        {
                            pain.IncreaseValue(10);
                        }
                    }
                    else if (wound.Name == "Gunshot")
                    {
                        if (part.Name == "Head")
                        {
                            pain.IncreaseValue(100);
                        }
                        else
                        {
                            pain.IncreaseValue(20);
                        }
                    }
                    else if (wound.Name == "Stab")
                    {
                        if (part.Name == "Head")
                        {
                            pain.IncreaseValue(50);
                        }
                        else
                        {
                            pain.IncreaseValue(20);
                        }
                    }
                    else if (wound.Name == "Stitched")
                    {
                        pain.IncreaseValue(4);
                    }
                    else if (wound.Name == "Set")
                    {
                        pain.IncreaseValue(10);
                    }
                    else if (wound.Name == "Burn")
                    {
                        pain.IncreaseValue(50);
                    }
                    else if (wound.Name == "Sever")
                    {
                        pain.IncreaseValue(80);
                    }
                    else if (wound.Name == "Bruise")
                    {
                        if (part.Name == "Head")
                        {
                            pain.IncreaseValue(20);
                        }
                        else
                        {
                            pain.IncreaseValue(5);
                        }
                    }
                }

                hp.Value = 100 - pain.Value;
                if (part.Name == "Head" &&
                    hp.Value <= 0)
                {
                    character.GetStat("Consciousness").Value = 0;
                }

                total += pain.Value;
            }

            Something painKiller = character.GetStatusEffect("Painkillers");
            if (painKiller != null)
            {
                total -= painKiller.Value;
            }

            Something adrenaline = character.GetStatusEffect("Adrenaline");
            if (adrenaline != null)
            {
                total -= painKiller.Value;
            }

            Something stat = character.GetStat("Pain");
            stat.Value = 0;
            stat.IncreaseValue(total);

            return total;
        }

        public static void UpdateWounds(Character character)
        {
            Something vitality = character.GetStat("Vitality");

            int partCount = character.BodyParts.Count;
            for (int b = 0; b < partCount; b++)
            { 
                BodyPart part = character.BodyParts[b];

                for (int w = 0; w < part.Wounds.Count; w++)
                {
                    Something wound = part.Wounds[w];
                    if (wound != null)
                    {
                        if (wound.Name != "Sever")
                        {
                            float heal_rate = vitality.Value / 50;
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
                    foreach (Something wound in part.Wounds)
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
                Something stat = character.GetStat("Blood");
                stat.DecreaseValue(total);

                Scene scene = SceneManager.GetScene("Gameplay");
                Map map = scene.World.Maps[0];
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
                            WorldUtil.AddEffect(bottom_tiles, new Vector2(character.Location.X, character.Location.Y), "Tiny Blood", null);
                        }

                        if (character.Moving &&
                            total >= 0.001f)
                        {
                            if (character.Direction == Direction.Up &&
                                !trail_north_found)
                            {
                                WorldUtil.AddEffect(bottom_tiles, new Vector2(character.Location.X, character.Location.Y), "Trail of Blood North", "Blood_Trail_Up");
                            }
                            else if (character.Direction == Direction.Right &&
                                     !trail_east_found)
                            {
                                WorldUtil.AddEffect(bottom_tiles, new Vector2(character.Location.X, character.Location.Y), "Trail of Blood East", "Blood_Trail_Right");
                            }
                            else if (character.Direction == Direction.Down &&
                                     !trail_south_found)
                            {
                                WorldUtil.AddEffect(bottom_tiles, new Vector2(character.Location.X, character.Location.Y), "Trail of Blood South", "Blood_Trail_Down");
                            }
                            else if (character.Direction == Direction.Left &&
                                     !trail_west_found)
                            {
                                WorldUtil.AddEffect(bottom_tiles, new Vector2(character.Location.X, character.Location.Y), "Trail of Blood West", "Blood_Trail_Left");
                            }
                        }
                    }
                }
            }
        }

        public static float GetTurnTime(Character character)
        {
            Something agility = character.GetStat("Agility");
            return 10000 / agility.Value;
        }

        public static void Sleep(Character character)
        {
            Something stamina = character.GetStat("Stamina");
            if (stamina.Value < 100)
            {
                //Per second
                stamina.IncreaseValue(0.00348f);
                if (stamina.Value > 100)
                {
                    stamina.Value = 100;
                }
            }
        }

        public static void Rest(Character character)
        {
            Something stamina = character.GetStat("Stamina");
            if (stamina.Value < 100)
            {
                //Per millisecond
                stamina.IncreaseValue(0.00002f);
                if (stamina.Value > 100)
                {
                    stamina.Value = 100;
                }
            }
        }

        public static void Kill(Character character)
        {
            character.Unconscious = false;
            character.Dead = true;
            JobManager.Jobs.Remove(character.Job);
        }
    }
}

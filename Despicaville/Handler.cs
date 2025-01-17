using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Utility;
using OP_Engine.Sounds;
using OP_Engine.Inputs;
using OP_Engine.Characters;
using OP_Engine.Inventories;
using OP_Engine.Tiles;

using Despicaville.Util;

namespace Despicaville
{
    public class Handler : GameComponent
    {
        #region Variables

        public static long ID;
        public static int SightDistance = 10;
        public static int HearingDistance = 5;
        public static int SmellDistance = 3;
        public static int ActionRate = 320;
        public static int MapSize_X = 10;
        public static int MapSize_Y = 10;
        public static bool WorldMap_Visible;

        public static bool Trading;
        public static List<long> Trading_InventoryID = new List<long>();
        
        public static string[] SkinColors = { "Light", "Tan", "Dark" };
        public static string[] HairLength = { "Bald", "Short", "Long" };
        public static string[] HairColor = { "Black", "Blonde", "Brown", "Grey", "Red", "White" };
        public static string[] Colors = { "Black", "Blue", "Brown", "Cyan", "Green", "Grey", "Orange", "Pink", "Purple", "Red", "White", "Yellow" };
        public static string[] SeeThrough = { "Lamp", "StreetLight", "Counter", "TV", "Stove", "Table" };
        public static string[] Searchable = { "Counter", "Fridge", "Dresser", "Desk", "Bookshelf", "Grass" };
        public static string[] BodyParts = { "Head", "Neck", "Torso", "Groin", "Right_Arm", "Right_Hand", "Left_Arm", "Left_Hand", "Right_Leg", "Right_Foot", "Left_Leg", "Left_Foot" };

        public static int MessageMax = 6;
        public static List<string> Messages = new List<string>();

        public static Tile Interaction_Tile = null;
        public static Character Interaction_Character = null;

        public static int light_count = 0;
        public static int light_map_width = 0;
        public static Dictionary<Vector2, List<Something>> light_maps = new Dictionary<Vector2, List<Something>>();
        public static List<Point> light_sources = new List<Point>();

        public static Dictionary<long, List<Tile>> VisibleTiles = new Dictionary<long, List<Tile>>();
        public static Dictionary<long, List<Tile>> OwnedFurniture = new Dictionary<long, List<Tile>>();
        public static Dictionary<string, string> Stats = new Dictionary<string, string>();
        public static Dictionary<string, string> Skills = new Dictionary<string, string>();

        public static int CharGen_Stage;

        public static int Icon_Count;
        public static int Loading_Stage;
        public static int Loading_Percent;
        public static string Loading_Message;
        public static CancellationTokenSource LoadingTokenSource;
        public static Task LoadingTask;

        #endregion

        #region Constructors

        public Handler(Game game) : base(game)
        {
            GameUtil.ResetArmy();
        }

        #endregion

        #region Methods

        #region Init

        public static void Init(Game game)
        {
            AssetManager.Init(game, Environment.CurrentDirectory + @"\Content");

            Load_Init();

            string config = AssetManager.Files["Config"];
            if (!File.Exists(config))
            {
                File.Create(config).Close();
            }
            else
            {
                Load.ParseINI(config);
            }
        }

        public static void Load_Init()
        {
            AssetManager.Directories.Add("Maps", string.Concat(AssetManager.Directories["Content"], @"\Maps"));

            AssetManager.Files.Add("Config", Environment.CurrentDirectory + @"\config.ini");
            AssetManager.Files.Add("Save", Environment.CurrentDirectory + @"\save.dat");

            AssetManager.LoadFonts();

            //Textures
            string[] textures = 
            { 
                "Controls",
                "Hairs",
                "Hats",
                "Pants",
                "Paperdoll",
                "Screens",
                "Shirts",
                "Shaders",
                "Shoes",
                "Skins"
            };
            foreach (string dir in textures)
            {
                AssetManager.LoadTextures(Main.Game.GraphicsManager.GraphicsDevice, dir);
            }

            //Sounds
            AssetManager.LoadSounds();

            string[] sounds = 
            {
                "Bow",
                "Click",
                "DoorClose",
                "DoorOpen",
                "Equip",
                "Explode",
                "Flush",
                "GlassBreak",
                "Pistol",
                "Punch",
                "Purchase",
                "Rifle",
                "Shotgun",
                "Stab",
                "Swing",
                "Sword",
                "Thunder",
                "WaterRunning",
                "WindowClose",
                "WindowOpen"
            };
            foreach (string dir in sounds)
            {
                AssetManager.LoadSounds(dir);
            }
            SoundManager.SoundVolume = 0.4f;
            SoundManager.SoundEnabled = true;

            //Ambient
            string[] ambient =
            {
                "Rain",
                "Storm",
                "Wind"
            };
            foreach (string dir in sounds)
            {
                AssetManager.LoadAmbient(dir);
            }
            SoundManager.AmbientFade = 1;
            SoundManager.AmbientVolume = 0.6f;
            SoundManager.AmbientEnabled = true;

            //Music
            string[] music =
            {
                "Day",
                "Defeat",
                "Loading",
                "Night",
                "Title"
            };
            foreach (string dir in music)
            {
                AssetManager.LoadMusic(dir);
            }
            SoundManager.MusicVolume = 0.8f;
            SoundManager.MusicEnabled = true;

            LoadControls();
            LoadStats();
            LoadSkills();
        }

        #endregion

        #region Load Stuff

        private static void LoadControls()
        {
            InputManager.Keyboard.KeysMapped.Add("Cancel", Keys.Escape);
            InputManager.Keyboard.KeysMapped.Add("Debug", Keys.OemTilde);
            InputManager.Keyboard.KeysMapped.Add("Up", Keys.W);
            InputManager.Keyboard.KeysMapped.Add("Right", Keys.D);
            InputManager.Keyboard.KeysMapped.Add("Down", Keys.S);
            InputManager.Keyboard.KeysMapped.Add("Left", Keys.A);
            InputManager.Keyboard.KeysMapped.Add("Crouch", Keys.LeftControl);
            InputManager.Keyboard.KeysMapped.Add("Run", Keys.LeftShift);
            InputManager.Keyboard.KeysMapped.Add("Interact", Keys.E);
            InputManager.Keyboard.KeysMapped.Add("Inventory", Keys.I);
            InputManager.Keyboard.KeysMapped.Add("Stats", Keys.C);
            InputManager.Keyboard.KeysMapped.Add("Skills", Keys.K);
            InputManager.Keyboard.KeysMapped.Add("Map", Keys.M);
            InputManager.Keyboard.KeysMapped.Add("Wait", Keys.Space);
            InputManager.Keyboard.KeysMapped.Add("Fullscreen", Keys.F2);
            InputManager.Keyboard.KeysMapped.Add("Save", Keys.F5);
            InputManager.Keyboard.KeysMapped.Add("Space", Keys.Space);
            InputManager.Keyboard.KeysMapped.Add("Backspace", Keys.Back);

            //for (char c = 'A'; c <= 'Z'; c++)
            //{
            //    Keys key = InputManager.GetKey(c.ToString());
            //    InputManager.Keyboard.KeysMapped.Add(c.ToString(), key);
            //}
        }

        public static void LoadWorldTextures()
        {
            if (Main.Game.GraphicsManager.GraphicsDevice != null)
            {
                Loading_Percent = 0;
                Loading_Message = "Loading textures...";

                string[] dirs = { "Foliage", "Furniture", "Icons", "Particles", "Tiles", "Vehicles" };

                int current = 0;
                int total = 0;

                DirectoryInfo dir = new DirectoryInfo(AssetManager.Directories["Textures"]);
                foreach (var sub_dir in dir.GetDirectories())
                {
                    if (dirs.Contains(sub_dir.Name))
                    {
                        foreach (var file in sub_dir.GetFiles("*.png"))
                        {
                            total++;

                            if (sub_dir.Name == "Icons" &&
                                !Path.GetFileNameWithoutExtension(file.Name).Contains("Slot"))
                            {
                                Icon_Count++;
                            }
                        }
                    }
                }

                foreach (var sub_dir in dir.GetDirectories())
                {
                    if (dirs.Contains(sub_dir.Name))
                    {
                        FileInfo[] files = sub_dir.GetFiles("*.png");

                        int fileCount = files.Length;
                        for (int i = 0; i < fileCount; i++)
                        {
                            FileInfo file = files[i];

                            var name = Path.GetFileNameWithoutExtension(file.Name);
                            if (!AssetManager.Textures.ContainsKey(name))
                            {
                                using (FileStream fileStream = new FileStream(file.FullName, FileMode.Open))
                                {
                                    if (Main.Game.GraphicsManager.GraphicsDevice != null)
                                    {
                                        Texture2D texture = Texture2D.FromStream(Main.Game.GraphicsManager.GraphicsDevice, fileStream);
                                        texture.Name = name;
                                        AssetManager.Textures.Add(name, texture);

                                        current++;
                                        Loading_Percent = (current * 100) / total;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void LoadStats()
        {
            string stat = "Strength";
            string description = "How much physical force you can apply.";
            Stats.Add(stat, description);

            stat = "Vitality";
            description = "How quickly your wounds heal.";
            Stats.Add(stat, description);

            stat = "Endurance";
            description = "How long you can sustain physical exertion.";
            Stats.Add(stat, description);

            stat = "Agility";
            description = "How quickly you can move and react.";
            Stats.Add(stat, description);

            stat = "Intelligence";
            description = "How quickly you learn new things.";
            Stats.Add(stat, description);

            stat = "Perception";
            description = "How quickly you notice details.";
            Stats.Add(stat, description);

            stat = "Charisma";
            description = "How persuasive you are.";
            Stats.Add(stat, description);

            stat = "Willpower";
            description = "How much pain you can tolerate.";
            Stats.Add(stat, description);

            stat = "Sanity";
            description = "How well you manage paranoia.";
            Stats.Add(stat, description);

            stat = "Luck";
            description = "How lucky you are.";
            Stats.Add(stat, description);

            CharacterManager.LastNames.Sort();
        }

        private static void LoadSkills()
        {
            string skill = "Grab";
            string description = "How well you hold onto things you grab.";
            Skills.Add(skill, description);

            skill = "Stab";
            description = "How accurate you are with stabbing weapons.";
            Skills.Add(skill, description);

            skill = "Throw";
            description = "How accurate you are with throwing weapons.";
            Skills.Add(skill, description);

            skill = "Punch";
            description = "How accurate you are with throwing punches.";
            Skills.Add(skill, description);

            skill = "Swing";
            description = "How accurate you are with swinging weapons.";
            Skills.Add(skill, description);

            skill = "Shoot";
            description = "How accurate you are with shooting weapons.";
            Skills.Add(skill, description);
        }

        public static void LoadAssets()
        {
            Loading_Percent = 0;
            Loading_Message = "Loading assets...";

            Inventory assets = new Inventory
            {
                ID = GetID(),
                Name = "Assets"
            };
            InventoryManager.Inventories.Add(assets);

            int current = 0;

            #region Weapons

            #region Axe

            List<string> categories = new List<string>
            {
                "Bedroom"
            };

            Item item = InventoryUtil.GenAsset("Axe", "Axe", categories, 60, "Weapon", "Swing", 25, 5, "Lose Limb");
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Fork

            categories = new List<string>
            {
                "Kitchen",
                "Counter"
            };

            item = InventoryUtil.GenAsset("Fork", "Fork", categories, 0, "Weapon", "Stab", 2, 0.1f, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Baseball Bat

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Baseball Bat", "Bat_Baseball", categories, 40, "Weapon", "Swing", 10, 0, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Spiked Bat

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Spiked Bat", "Bat_Spiked", categories, 80, "Weapon", "Swing", 10, 5, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Baton

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Baton", "Baton", categories, 70, "Weapon", "Swing", 10, 0, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Bow

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Bow", "Bow", categories, 60, "Weapon", "Shoot", 10, 5, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Box Cutter

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Box Cutter", "Box_Cutter", categories, 60, "Weapon", "Stab", 4, 2, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Cane

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Cane", "Cane", categories, 30, "Weapon", "Swing", 5, 0, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Broom

            categories = new List<string>
            {
                "Kitchen"
            };

            item = InventoryUtil.GenAsset("Broom", "Broom", categories, 30, "Weapon", "Swing", 2, 0, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Chainsaw

            categories = new List<string>
            {
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Chainsaw", "Chainsaw", categories, 100, "Weapon", "Swing", 50, 20, "Lose Limb");
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Brick

            categories = new List<string>
            {
                "Outdoors"
            };

            item = InventoryUtil.GenAsset("Brick", "Brick", categories, 50, "Weapon", "Throw", 20, 0, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Meat Cleaver

            categories = new List<string>
            {
                "Kitchen"
            };

            item = InventoryUtil.GenAsset("Meat Cleaver", "Cleaver", categories, 60, "Weapon", "Swing", 20, 10, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Crossbow

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Crossbow", "Crossbow", categories, 80, "Weapon", "Shoot", 10, 5, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Crowbar

            categories = new List<string>
            {
                "Kitchen",
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Crowbar", "Crowbar", categories, 60, "Weapon", "Swing", 20, 0, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Dart

            categories = new List<string>
            {
                "Bedroom",
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Dart", "Dart", categories, 40, "Weapon", "Throw", 4, 2, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Drill

            categories = new List<string>
            {
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Drill", "Drill", categories, 60, "Weapon", "Stab", 10, 4, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Dynamite

            categories = new List<string>
            {
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Dynamite", "Dynamite", categories, 100, "Weapon", "Throw", 100, 100, "Explode");
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Timed Dynamite", "Dynamite_Timed", null, 0, "Weapon", "Throw", 100, 100, "Explode");
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Flamethrower

            categories = new List<string>
            {
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Flamethrower", "Flamethrower", categories, 100, "Weapon", "Shoot", 0, 0, "Burn");
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Grapple

            categories = new List<string>
            {
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Grapple", "Grapple", categories, 80, "Weapon", "Throw", 0, 0, "Hooked");
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Grenade

            categories = new List<string>
            {
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Grenade", "Grenade", categories, 100, "Weapon", "Throw", 100, 100, "Explode");
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Flash Grenade

            categories = new List<string>
            {
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Flash Grenade", "Grenade_Flash", categories, 90, "Weapon", "Throw", 0, 0, "Blind");
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Gas Grenade

            categories = new List<string>
            {
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Gas Grenade", "Grenade_Gas", categories, 90, "Weapon", "Throw", 0, 0, "Gas");
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Hammer

            categories = new List<string>
            {
                "Hallway",
                "Kitchen"
            };

            item = InventoryUtil.GenAsset("Hammer", "Hammer", categories, 90, "Weapon", "Swing", 10, 0, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Hatchet

            categories = new List<string>
            {
                "Hallway",
                "Kitchen",
                "Outdoors"
            };

            item = InventoryUtil.GenAsset("Hatchet", "Hatchet", categories, 90, "Weapon", "Swing", 10, 10, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Icepick

            categories = new List<string>
            {
                "Hallway",
                "Outdoors"
            };

            item = InventoryUtil.GenAsset("Icepick", "Icepick", categories, 60, "Weapon", "Swing", 10, 8, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Katana

            categories = new List<string>
            {
                "Hallway",
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Katana", "Katana", categories, 100, "Weapon", "Swing", 20, 20, "Lose Limb");
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Knife

            categories = new List<string>
            {
                "Hallway",
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Knife", "Knife", categories, 10, "Weapon", "Stab", 5, 4, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Bone Knife

            item = InventoryUtil.GenAsset("Bone Knife", "Knife_Bone", null, 0, "Weapon", "Stab", 5, 5, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Switchblade

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Switchblade", "Knife_Switchblade", categories, 60, "Weapon", "Stab", 4, 2, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Machine Gun

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Machine Gun", "Machine Gun", categories, 90, "Weapon", "Shoot", 10, 4, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Mallet

            categories = new List<string>
            {
                "Kitchen"
            };

            item = InventoryUtil.GenAsset("Mallet", "Mallet", categories, 40, "Weapon", "Swing", 8, 0, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Net

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Net", "Net", categories, 80, "Weapon", "Throw", 0, 0, "Net");
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Nunchucks

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Nunchucks", "Nunchucks", categories, 80, "Weapon", "Swing", 10, 0, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Lead Pipe

            categories = new List<string>
            {
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Lead Pipe", "Pipe_Lead", categories, 60, "Weapon", "Swing", 10, 0, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Pistol

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Pistol", "Pistol", categories, 60, "Weapon", "Shoot", 10, 4, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Rifle

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Rifle", "Rifle", categories, 80, "Weapon", "Shoot", 10, 4, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Rock

            categories = new List<string>
            {
                "Outdoors"
            };

            item = InventoryUtil.GenAsset("Rock", "Rock", categories, 0, "Weapon", "Throw", 4, 0, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Rolling Pin

            categories = new List<string>
            {
                "Kitchen"
            };

            item = InventoryUtil.GenAsset("Rolling Pin", "Rolling_Pin", categories, 40, "Weapon", "Swing", 4, 0, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Saucepan

            categories = new List<string>
            {
                "Kitchen"
            };

            item = InventoryUtil.GenAsset("Saucepan", "Saucepan", categories, 40, "Weapon", "Swing", 4, 0, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Scalpel

            categories = new List<string>
            {
                "Bathroom"
            };

            item = InventoryUtil.GenAsset("Scalpel", "Scalpel", categories, 80, "Weapon", "Stab", 2, 2, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Scythe

            categories = new List<string>
            {
                "Outdoors"
            };

            item = InventoryUtil.GenAsset("Scythe", "Scythe", categories, 100, "Weapon", "Swing", 20, 20, "Lose Limb");
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Shears

            categories = new List<string>
            {
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Shears", "Shears", categories, 60, "Weapon", "Stab", 5, 4, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Large Shears

            categories = new List<string>
            {
                "Hallway",
                "Outdoors"
            };

            item = InventoryUtil.GenAsset("Large Shears", "Shears_Large", categories, 80, "Weapon", "Chop", 20, 20, "Lose Limb");
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Shotgun

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Shotgun", "Shotgun", categories, 80, "Weapon", "Shoot", 20, 10, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Shovel

            categories = new List<string>
            {
                "Hallway",
                "Outdoors"
            };

            item = InventoryUtil.GenAsset("Shovel", "Shovel", categories, 40, "Weapon", "Swing", 10, 0, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Shuriken

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Shuriken", "Shuriken", categories, 80, "Weapon", "Throw", 5, 4, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Sickle

            categories = new List<string>
            {
                "Outdoors"
            };

            item = InventoryUtil.GenAsset("Sickle", "Sickle", categories, 80, "Weapon", "Stab", 8, 8, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Sling

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Sling", "Sling", categories, 60, "Weapon", "Shoot", 0, 0, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Spoon

            categories = new List<string>
            {
                "Kitchen"
            };

            item = InventoryUtil.GenAsset("Spoon", "Spoon", categories, 0, "Weapon", "Stab", 4, 2, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Stake

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Stake", "Stake", categories, 80, "Weapon", "Stab", 8, 8, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Sword

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Sword", "Sword", categories, 90, "Weapon", "Swing", 20, 20, "Lose Limb");
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Lug Wrench

            categories = new List<string>
            {
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Lug Wrench", "Tire_Iron", categories, 60, "Weapon", "Swing", 10, 0, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Trowel

            categories = new List<string>
            {
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Trowel", "Trowel", categories, 60, "Weapon", "Stab", 4, 2, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Pipe Wrench

            categories = new List<string>
            {
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Pipe Wrench", "Wrench", categories, 60, "Weapon", "Swing", 10, 0, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #endregion

            #region Tools

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Binoculars", "Binoculars", categories, 80, "Tool", "Longsight", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Stethoscope", "Stethoscope", categories, 80, "Tool", "Crack Safe", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Bathroom"
            };

            item = InventoryUtil.GenAsset("Towel", "Towel", categories, 20, "Tool", "Dry", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Bolt Cutter", "Bolt_Cutter", categories, 80, "Tool", "Cut", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Saw", "Saw", categories, 60, "Tool", "Saw", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Screwdriver", "Screwdriver", categories, 40, "Tool", "Dismantle", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Trap", "Trap", categories, 80, "Tool", "Trap", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Kitchen"
            };

            item = InventoryUtil.GenAsset("Cannister", "Cannister_Empty", categories, 80, "Tool", "Fill", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Cannister of Blood", "Cannister_Blood", null, 0, "Tool", "Drink", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Thirst",
                Value = -10
            });
            item.Properties.Add(new Something
            {
                Name = "Bladder",
                Value = 10
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Cannister of Poison", "Cannister_Poison", null, 0, "Tool", "Drink", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Poison",
                Value = 1440
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Cannister of Water", "Cannister_Water", null, 0, "Tool", "Drink", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Thirst",
                Value = -20
            });
            item.Properties.Add(new Something
            {
                Name = "Bladder",
                Value = 20
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Dish Rag", "Dish_Rag", categories, 20, "Tool", "Clean", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Office"
            };

            item = InventoryUtil.GenAsset("Walkie Talkie", "Walkie_Talkie", categories, 80, "Tool", "Listen", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Hallway",
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Boombox", "Boombox", categories, 60, "Tool", "Play Music", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Hallway",
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Radio", "Radio", categories, 60, "Tool", "Play Music", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Hallway",
                "Bedroom",
                "Kitchen",
                "Office",
                "Outdoors"
            };

            item = InventoryUtil.GenAsset("Bottle", "Bottle_Empty", categories, 20, "Tool", "Fill", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Bottle of Blood", "Bottle_Blood", null, 0, "Tool", "Drink", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Thirst",
                Value = -10
            });
            item.Properties.Add(new Something
            {
                Name = "Bladder",
                Value = 10
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Bottle of Poison", "Bottle_Poison", null, 0, "Tool", "Drink", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Poison",
                Value = 1440
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Bottle of Water", "Bottle_Water", null, 0, "Tool", "Drink", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Thirst",
                Value = -20
            });
            item.Properties.Add(new Something
            {
                Name = "Bladder",
                Value = 20
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Hallway",
                "Kitchen"
            };

            item = InventoryUtil.GenAsset("Bucket", "Bucket_Empty", categories, 40, "Tool", "Fill", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Bucket of Blood", "Bucket_Blood", null, 0, "Tool", "Drink", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Thirst",
                Value = -50
            });
            item.Properties.Add(new Something
            {
                Name = "Bladder",
                Value = 50
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Bucket of Poison", "Bucket_Poison", null, 0, "Tool", "Drink", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Poison",
                Value = 4320
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Bucket of Water", "Bucket_Water", null, 0, "Tool", "Drink", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Thirst",
                Value = -100
            });
            item.Properties.Add(new Something
            {
                Name = "Bladder",
                Value = 100
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Kitchen",
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Flashlight", "Flashlight", categories, 40, "Tool", "Light", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Office",
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Handcuffs", "Handcuffs", categories, 80, "Tool", "Handcuff", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Kitchen",
                "Bedroom",
                "Office",
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Lighter", "Lighter", categories, 60, "Tool", "Burn", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Bedroom",
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Lockpicks", "Lockpicks", categories, 80, "Tool", "Lockpick", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Bedroom",
                "Hallway",
                "Kitchen",
                "Office"
            };

            item = InventoryUtil.GenAsset("Magnifying Glass", "Magnifying_Glass", categories, 60, "Tool", "Search", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Bedroom",
                "Hallway",
                "Kitchen",
                "Office"
            };

            item = InventoryUtil.GenAsset("Matches", "Matches", categories, 40, "Tool", "Burn", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Bedroom",
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Rope", "Rope", categories, 60, "Tool", "Tie", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Bedroom",
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Jump Rope", "Rope_Jump", categories, 50, "Tool", "Tie", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Kitchen",
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Duct Tape", "Duct_Tape", categories, 40, "Tool", "Tape", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Back Clothing

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Cape", "Back_Cape", categories, 80, "Back", null, 0, 0, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Shoes

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Boots", "Boots", categories, 40, "Shoes", null, 0, 0, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Ninja Boots", "Boots_Ninja", categories, 90, "Shoes", null, 0, 0, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Sabatons", "Boots_Armor", categories, 100, "Shoes", null, 0, 0, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Combat Boots", "Boots_Combat", categories, 80, "Shoes", null, 0, 0, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Sandals", "Shoes_Sandals", categories, 40, "Shoes", null, 0, 0, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Shoes", "Shoes_Plain", categories, 10, "Shoes", null, 0, 0, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Bunny Slippers", "Shoes_Slippers", categories, 60, "Shoes", null, 0, 0, null);
            assets.Items.Add(item);

            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Hats

            categories = new List<string>
            {
                "Kitchen"
            };

            item = InventoryUtil.GenAsset("Chef Hat", "Hat_Chef", categories, 80, "Hat", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Bedroom",
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Football Helmet", "Hat_Football", categories, 70, "Hat", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Motorcycle Helmet", "Hat_Motorcycle", categories, 80, "Hat", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Hat", "Hat_Cap", categories, 10, "Hat", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Captain Hat", "Hat_Captain", categories, 80, "Hat", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Cowboy Hat", "Hat_Cowboy", categories, 80, "Hat", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Crown", "Hat_Crown", categories, 100, "Hat", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Fedora", "Hat_Fedora", categories, 70, "Hat", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Graduation Hat", "Hat_Graduate", categories, 70, "Hat", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Headphones", "Hat_Headphones", categories, 60, "Hat", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Pirate Hat", "Hat_Pirate", categories, 80, "Hat", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Pope Hat", "Hat_Pope", categories, 100, "Hat", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Sombrero", "Hat_Sombrero", categories, 80, "Hat", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Top Hat", "Hat_Top", categories, 80, "Hat", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Winter Hat", "Hat_Winter", categories, 40, "Hat", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Witch Hat", "Hat_Witch", categories, 80, "Hat", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Helmet", "Hat_Armor", categories, 100, "Hat", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Space Helmet", "Hat_Astronaut", categories, 100, "Hat", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Mining Helmet", "Hat_Mining", categories, 80, "Hat", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Spartan Helmet", "Hat_Spartan", categories, 100, "Hat", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Viking Helmet", "Hat_Viking", categories, 100, "Hat", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Tiara", "Tiara", categories, 80, "Hat", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Gloves

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Baseball Glove - Left", "Glove_Baseball_Left", categories, 40, "Gloves", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Baseball Glove - Right", "Glove_Baseball_Right", categories, 40, "Gloves", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Boxing Glove - Left", "Glove_Boxing_Left", categories, 80, "Gloves", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Boxing Glove - Right", "Glove_Boxing_Right", categories, 80, "Gloves", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Glove - Left", "Glove_Left", categories, 40, "Gloves", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Glove - Right", "Glove_Right", categories, 40, "Gloves", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Mitten - Left", "Glove_Left_Winter", categories, 20, "Gloves", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Mitten - Right", "Glove_Right_Winter", categories, 20, "Gloves", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Gauntlet - Left", "Glove_Left_Armor", categories, 100, "Gloves", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Gauntlet - Right", "Glove_Right_Armor", categories, 100, "Gloves", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Pants

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Pants", "Pants_Plain", categories, 10, "Pants", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Greaves", "Pants_Armor", categories, 100, "Pants", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Shorts", "Pants_Shorts", categories, 10, "Pants", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Skirt", "Pants_Skirt", categories, 20, "Pants", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Underwear", "Pants_Underwear", categories, 20, "Pants", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Coats

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Doctor Coat", "Coat_Doctor", categories, 80, "Coat", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Trench Coat", "Coat_Trench", categories, 80, "Coat", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Vest", "Coat_Vest", categories, 60, "Coat", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Shields

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Medieval Shield", "Shield_Medieval", categories, 100, "Shield", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Bedroom",
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Riot Shield", "Shield_Riot", categories, 100, "Shield", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Shirts

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Shirt", "Shirt_T", categories, 10, "Shirt", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Armor", "Shirt_Armor", categories, 100, "Shirt", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Bulletproof Vest", "Shirt_Combat", categories, 80, "Shirt", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Corset", "Shirt_Corset", categories, 60, "Shirt", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Dress", "Shirt_Dress", categories, 20, "Shirt", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Gown", "Shirt_Gown", categories, 40, "Shirt", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Hoodie", "Shirt_Hoodie", categories, 20, "Shirt", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Jersey", "Shirt_Jersey", categories, 20, "Shirt", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Kimono", "Shirt_Kimono", categories, 80, "Shirt", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Ninja Garb", "Shirt_Ninja", categories, 90, "Shirt", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Tank Top", "Shirt_Tank", categories, 10, "Shirt", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Masks

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("3D Glasses", "Mask_3D", categories, 60, "Mask", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Batman Mask", "Mask_Batman", categories, 80, "Mask", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Devil Mask", "Mask_Devil", categories, 70, "Mask", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Duality Mask", "Mask_Duality", categories, 70, "Mask", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Gas Mask", "Mask_Gas", categories, 80, "Mask", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Glasses", "Mask_Glasses", categories, 20, "Mask", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Goggles", "Mask_Goggles", categories, 40, "Mask", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Hockey Mask", "Mask_Hockey", categories, 60, "Mask", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Night Vision Goggles", "Mask_NightVision", categories, 80, "Mask", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Ninja Mask", "Mask_Ninja", categories, 90, "Mask", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Eye Patch", "Mask_Patch", categories, 40, "Mask", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Plague Doctor Mask", "Mask_Plague", categories, 80, "Mask", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Safety Glasses", "Mask_Protection", categories, 60, "Mask", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Scuba Mask", "Mask_Scuba", categories, 80, "Mask", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Ski Mask", "Mask_Ski", categories, 60, "Mask", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Skull Mask", "Mask_Skull", categories, 70, "Mask", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Spiderman Mask", "Mask_Spiderman", categories, 80, "Mask", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Tribal Mask", "Mask_Tribal", categories, 70, "Mask", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Medical

            item = InventoryUtil.GenAsset("Splint", "Splint", null, 0, "Medical", "Set Break", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Bathroom"
            };

            item = InventoryUtil.GenAsset("Bandage", "Bandage", categories, 20, "Medical", "Cover Wound", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Bandaid", "Bandaid", categories, 10, "Medical", "Cover Wound", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Blood Pack", "Blood_Pack", categories, 60, "Medical", "Drink", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Thirst",
                Value = -10
            });
            item.Properties.Add(new Something
            {
                Name = "Bladder",
                Value = 10
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Disinfectant", "Disinfectant", categories, 40, "Medical", "Clean Wound", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Needle & Thread", "Thread_Needle", categories, 80, "Medical", "Stitch Wound", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Adrenaline Pill", "Pill_Adrenaline", categories, 60, "Medical", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Stamina",
                Value = 100
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Caffeine Pill", "Pill_Caffeine", categories, 40, "Medical", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Stamina",
                Value = 50
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Focus Pill", "Pill_Focus", categories, 40, "Medical", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Consciousness",
                Value = 100
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Happy Pill", "Pill_Happy", categories, 20, "Medical", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Paranoia",
                Value = -25
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Painkiller", "Pill_Painkiller", categories, 20, "Medical", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Pain",
                Value = -25
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Soap", "Soap", categories, 10, "Medical", "Clean Wound", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Syringe", "Syringe_Empty", categories, 60, "Medical", "Fill", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Syringe of Blood", "Syringe_Blood", null, 0, "Medical", "Inject", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Blood",
                Value = 25
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Syringe of Poison", "Syringe_Poison", null, 0, "Medical", "Inject", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Poison",
                Value = 1440
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Syringe of Water", "Syringe_Water", null, 0, "Medical", "Inject", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Blood",
                Value = -20
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Syringe of Adrenaline", "Syringe_Adrenaline", categories, 80, "Medical", "Inject", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Stamina",
                Value = 200
            });
            item.Properties.Add(new Something
            {
                Name = "Consciousness",
                Value = 100
            });
            item.Properties.Add(new Something
            {
                Name = "Pain",
                Value = -160
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Bathroom",
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Thread", "Thread", categories, 20, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Bathroom",
                "Bedroom",
                "Kitchen"
            };

            item = InventoryUtil.GenAsset("Needle", "Needle", categories, 20, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Food

            item = InventoryUtil.GenAsset("Dough", "Dough", null, 0, "Food", null, 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -4
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Kitchen",
                "Office"
            };

            item = InventoryUtil.GenAsset("Coffee", "Coffee", categories, 20, "Misc", "Drink", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Thirst",
                Value = -10
            });
            item.Properties.Add(new Something
            {
                Name = "Stamina",
                Value = 25
            });
            item.Properties.Add(new Something
            {
                Name = "Bladder",
                Value = 10
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Candy", "Food_Candy", categories, 40, "Misc", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -2
            });
            item.Properties.Add(new Something
            {
                Name = "Stamina",
                Value = 5
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Chips", "Food_Chips", categories, 20, "Misc", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -5
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Popcorn", "Food_Popcorn", categories, 80, "Misc", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -5
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Pretzel", "Food_Pretzel", categories, 60, "Misc", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -8
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Cookie", "Food_Cookie", categories, 40, "Misc", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -2
            });
            item.Properties.Add(new Something
            {
                Name = "Stamina",
                Value = 5
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Kitchen"
            };

            item = InventoryUtil.GenAsset("Cooked Fish", "Fish_Cooked", categories, 60, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -10
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Raw Fish", "Fish_Raw", categories, 40, "Food", "Cook", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -5
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Flour", "Flour", categories, 10, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Apple", "Food_Apple", categories, 0, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -4
            });
            item.Properties.Add(new Something
            {
                Name = "Thirst",
                Value = -2
            });
            item.Properties.Add(new Something
            {
                Name = "Bladder",
                Value = 2
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Banana", "Food_Banana", categories, 0, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -5
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Bread", "Food_Bread", categories, 0, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -15
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Broccoli", "Food_Broccoli", categories, 0, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -5
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Cabbage", "Food_Cabbage", categories, 0, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -5
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Cake", "Food_Cake", categories, 40, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -5
            });
            item.Properties.Add(new Something
            {
                Name = "Stamina",
                Value = 10
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Carrot", "Food_Carrot", categories, 0, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -5
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Cheese", "Food_Cheese", categories, 0, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -8
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Cherries", "Food_Cherries", categories, 20, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -2
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Cooked Chicken", "Food_Chicken_Cooked", categories, 60, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -20
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Raw Chicken", "Food_Chicken_Raw", categories, 40, "Food", "Cook", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -10
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Chocolate", "Food_Chocolate", categories, 60, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -2
            });
            item.Properties.Add(new Something
            {
                Name = "Stamina",
                Value = 8
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Corn", "Food_Corn", categories, 20, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -10
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Cupcake", "Food_Cupcake", categories, 40, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -4
            });
            item.Properties.Add(new Something
            {
                Name = "Stamina",
                Value = 8
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Donut", "Food_Donut", categories, 40, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -4
            });
            item.Properties.Add(new Something
            {
                Name = "Stamina",
                Value = 8
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Egg", "Food_Egg", categories, 10, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -4
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Fries", "Food_Fries", categories, 80, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -5
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Garlic", "Food_Garlic", categories, 60, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -2
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Grapes", "Food_Grapes", categories, 0, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -4
            });
            item.Properties.Add(new Something
            {
                Name = "Thirst",
                Value = -2
            });
            item.Properties.Add(new Something
            {
                Name = "Bladder",
                Value = 2
            });

            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Cooked Ham", "Food_Ham_Cooked", categories, 60, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -20
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Raw Ham", "Food_Ham_Raw", categories, 40, "Food", "Cook", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -10
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Hamburger", "Food_Hamburger", categories, 80, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -25
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Ice Cream", "Food_IceCream", categories, 60, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -3
            });
            item.Properties.Add(new Something
            {
                Name = "Thirst",
                Value = -3
            });
            item.Properties.Add(new Something
            {
                Name = "Stamina",
                Value = 15
            });
            item.Properties.Add(new Something
            {
                Name = "Bladder",
                Value = 3
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Lemon", "Food_Lemon", categories, 20, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -4
            });
            item.Properties.Add(new Something
            {
                Name = "Thirst",
                Value = -2
            });
            item.Properties.Add(new Something
            {
                Name = "Bladder",
                Value = 2
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Milk", "Food_Milk", categories, 0, "Food", "Drink", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Thirst",
                Value = -10
            });
            item.Properties.Add(new Something
            {
                Name = "Bladder",
                Value = 10
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Orange", "Food_Orange", categories, 0, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -4
            });
            item.Properties.Add(new Something
            {
                Name = "Thirst",
                Value = -2
            });
            item.Properties.Add(new Something
            {
                Name = "Bladder",
                Value = 2
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Pear", "Food_Pear", categories, 10, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -4
            });
            item.Properties.Add(new Something
            {
                Name = "Thirst",
                Value = -2
            });
            item.Properties.Add(new Something
            {
                Name = "Bladder",
                Value = 2
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Pepper", "Food_Pepper", categories, 20, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -2
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Pickle", "Food_Pickle", categories, 0, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -4
            });
            item.Properties.Add(new Something
            {
                Name = "Thirst",
                Value = -2
            });
            item.Properties.Add(new Something
            {
                Name = "Bladder",
                Value = 2
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Pineapple", "Food_Pineapple", categories, 40, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -4
            });
            item.Properties.Add(new Something
            {
                Name = "Thirst",
                Value = -2
            });
            item.Properties.Add(new Something
            {
                Name = "Bladder",
                Value = 2
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Pizza", "Food_Pizza", categories, 60, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -30
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Potato", "Food_Potato", categories, 0, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -15
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Pumpkin", "Food_Pumpkin", categories, 60, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -15
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Sandwich", "Food_Sandwich", categories, 60, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -20
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Cooked Sausage", "Food_Sausage_Cooked", categories, 60, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -10
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Raw Sausage", "Food_Sausage_Raw", categories, 40, "Food", "Cook", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -5
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Soda", "Food_Soda", categories, 20, "Food", "Drink", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Thirst",
                Value = -10
            });
            item.Properties.Add(new Something
            {
                Name = "Stamina",
                Value = 10
            });
            item.Properties.Add(new Something
            {
                Name = "Bladder",
                Value = 10
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Cooked Steak", "Food_Steak_Cooked", categories, 60, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -20
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Raw Steak", "Food_Steak_Raw", categories, 40, "Food", "Cook", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -10
            });
            item.Properties.Add(new Something
            {
                Name = "Thirst",
                Value = -2
            });
            item.Properties.Add(new Something
            {
                Name = "Bladder",
                Value = 2
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Strawberry", "Food_Strawberry", categories, 0, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -4
            });
            item.Properties.Add(new Something
            {
                Name = "Thirst",
                Value = -2
            });
            item.Properties.Add(new Something
            {
                Name = "Bladder",
                Value = 2
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Tomato", "Food_Tomato", categories, 0, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -4
            });
            item.Properties.Add(new Something
            {
                Name = "Thirst",
                Value = -8
            });
            item.Properties.Add(new Something
            {
                Name = "Bladder",
                Value = 8
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Watermelon", "Food_Watermelon", categories, 40, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -4
            });
            item.Properties.Add(new Something
            {
                Name = "Thirst",
                Value = -8
            });
            item.Properties.Add(new Something
            {
                Name = "Bladder",
                Value = 8
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Honey", "Honey", categories, 0, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -2
            });
            item.Properties.Add(new Something
            {
                Name = "Thirst",
                Value = -2
            });
            item.Properties.Add(new Something
            {
                Name = "Stamina",
                Value = 10
            });
            item.Properties.Add(new Something
            {
                Name = "Bladder",
                Value = 2
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Ketchup", "Ketchup", categories, 0, "Food", "Eat", 0, 0, null);
            item.Properties.Add(new Something
            {
                Name = "Hunger",
                Value = -2
            });
            item.Properties.Add(new Something
            {
                Name = "Thirst",
                Value = -2
            });
            item.Properties.Add(new Something
            {
                Name = "Bladder",
                Value = 2
            });
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Containers

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Large Backpack", "Back_Backpack_Large", categories, 90, "Container", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Medium Backpack", "Back_Backpack_Medium", categories, 80, "Container", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Small Backpack", "Back_Backpack_Small", categories, 70, "Container", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Duffel Bag", "Bag_Duffel", categories, 80, "Container", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Gym Bag", "Bag_Gym", categories, 80, "Container", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Suitcase", "Bag_Suitcase", categories, 60, "Container", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Quiver", "Quiver", categories, 80, "Container", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Locked Wooden Chest", "Wood_Chest_Locked", categories, 60, "Container", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Wooden Chest", "Wood_Chest_Closed", categories, 80, "Container", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Empty Wooden Chest", "Wood_Chest_Open", null, 0, "Container", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Bedroom",
                "Kitchen",
                "Outdoors"
            };

            item = InventoryUtil.GenAsset("Basket", "Bag_Basket", categories, 60, "Container", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Office"
            };

            item = InventoryUtil.GenAsset("Briefcase", "Bag_Briefcase", categories, 80, "Container", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Office",
                "Kitchen"
            };

            item = InventoryUtil.GenAsset("Paper Bag", "Bag_Paper", categories, 60, "Container", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Kitchen"
            };

            item = InventoryUtil.GenAsset("Plastic Bag", "Bag_Plastic", categories, 40, "Container", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Bedroom",
                "Office"
            };

            item = InventoryUtil.GenAsset("Purse", "Bag_Purse", categories, 40, "Container", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Safe", "Safe", categories, 80, "Container", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Bedroom",
                "Office",
                "Kitchen",
                "Hallway",
                "Outdoors",
                "Lounge"
            };

            item = InventoryUtil.GenAsset("Cardboard Box", "Cardboard_Box_Closed", categories, 20, "Container", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Empty Cardboard Box", "Cardboard_Box_Open", null, 0, "Container", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Outdoors"
            };

            item = InventoryUtil.GenAsset("Coffin", "Coffin", categories, 100, "Container", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Outdoors",
                "Hallway",
                "Kitchen",
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Cooler", "Cooler", categories, 60, "Container", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Bathroom"
            };

            item = InventoryUtil.GenAsset("First Aid Kit", "First_Aid_Kit", categories, 80, "Container", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Outdoors"
            };

            item = InventoryUtil.GenAsset("Barrel", "Wood_Barrel", categories, 80, "Container", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Wooden Crate", "Wood_Crate", categories, 60, "Container", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Bedroom",
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Present", "Present", categories, 80, "Container", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Ammo

            categories = new List<string>
            {
                "Office",
                "Bedroom",
                "Store Counter"
            };

            item = InventoryUtil.GenAsset("Machine Gun Ammo", "Ammo_Machine_Gun", categories, 80, "Ammo", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Pistol Ammo", "Ammo_Pistol", categories, 60, "Ammo", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Shotgun Ammo", "Ammo_Shotgun", categories, 70, "Ammo", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Arrow", "Arrow", categories, 60, "Ammo", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Misc

            #region Outdoors

            categories = new List<string>
            {
                "Outdoors"
            };

            item = InventoryUtil.GenAsset("Acorn", "Acorn", categories, 20, "Misc", "Throw", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Broken Arrow", "Arrow_Broken", categories, 80, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Gas Barrel", "Gas_Barrel", categories, 90, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Gas Can", "Gas_Can", categories, 80, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Generator", "Generator", categories, 90, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Leaf", "Leaf", categories, 10, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Log", "Wood_Log", categories, 40, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Plank", "Wood_Plank", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Stick", "Wood_Stick", categories, 10, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Kitchen

            categories = new List<string>
            {
                "Kitchen"
            };

            item = InventoryUtil.GenAsset("Bowl", "Bowl", categories, 0, "Misc", "Throw", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Candelabra", "Candelabra_Empty", categories, 80, "Misc", "Throw", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Candelabra", "Candelabra", null, 0, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Lit Candelabra", "Candelabra_Lit", null, 0, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Cup", "Cup_Empty", categories, 0, "Misc", "Fill", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Cup of Blood", "Cup_Blood", null, 0, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Cup of Poison", "Cup_Poison", null, 0, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Cup of Water", "Cup_Water", null, 0, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Dish", "Dish", categories, 0, "Misc", "Throw", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Ladle", "Ladle", categories, 20, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Teapot", "Teapot", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Poison", "Poison", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Bedroom

            categories = new List<string>
            {
                "Bedroom"
            };

            item = InventoryUtil.GenAsset("Camera", "Camera", categories, 40, "Misc", "Throw", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Video Camera", "Camera_Video", categories, 60, "Misc", "Throw", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Gold Medal", "Medal_Gold", categories, 90, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Silver Medal", "Medal_Silver", categories, 80, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Bronze Medal", "Metal_Bronze", categories, 70, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Toy Car", "Toy_Car", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Toy Horse", "Toy_Horse", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Toy Puppet", "Toy_Puppet", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Gold Trophy", "Trophy_Gold", categories, 90, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Silver Trophy", "Trophy_Silver", categories, 80, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Pillow", "Pillow", categories, 40, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Tobacco Pipe", "Pipe_Smoking", categories, 80, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Ribbon", "Ribbon", categories, 40, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Scuba Tank", "Scuba_Tank", categories, 80, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Sheet", "Sheet", categories, 20, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Yarn", "Yarn", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Hallway

            categories = new List<string>
            {
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Fishing Pole", "Fishing_Pole", categories, 80, "Misc", "Fishing", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Jigsaw Piece", "Jigsaw_Piece", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Stop Watch", "Watch_Stop", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Empty Water Gun", "Water_Gun_Empty", categories, 60, "Misc", "Fill", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Water Gun of Blood", "Water_Gun_Blood", null, 0, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Water Gun of Poison", "Water_Gun_Poison", null, 0, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Water Gun", "Water_Gun_Water", null, 0, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Empty Watering Can", "Watering_Can_Empty", categories, 60, "Misc", "Fill", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Watering Can of Blood", "Watering_Can_Blood", null, 0, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Watering Can of Poison", "Watering_Can_Poison", null, 0, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Watering Can", "Watering_Can_Water", null, 0, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Whistle", "Whistle", categories, 70, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Spring", "Spring", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Umbrella", "Umbrella", categories, 80, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Bathroom

            categories = new List<string>
            {
                "Bathroom"
            };

            item = InventoryUtil.GenAsset("Plunger", "Plunger", categories, 20, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Office

            categories = new List<string>
            {
                "Office"
            };

            item = InventoryUtil.GenAsset("Stapler", "Stapler", categories, 80, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Lounge

            categories = new List<string>
            {
                "Lounge"
            };

            item = InventoryUtil.GenAsset("Remote", "Remote", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Bedroom, Bathroom

            categories = new List<string>
            {
                "Bedroom",
                "Bathroom"
            };

            item = InventoryUtil.GenAsset("Rubber Ducky", "Rubber_Ducky", categories, 80, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Bedroom, Hallway

            categories = new List<string>
            {
                "Bedroom",
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Cassette", "Cassette", categories, 20, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("CD", "CD", categories, 40, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Paint Brush", "Paint_Brush", categories, 70, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Large Paint Brush", "Paint_Brush_Large", categories, 80, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #region Chess Pieces

            item = InventoryUtil.GenAsset("Black Bishop", "Chess_Bishop_Black", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("White Bishop", "Chess_Bishop_White", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Black King", "Chess_King_Black", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("White King", "Chess_King_White", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Black Knight", "Chess_Knight_Black", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("White Knight", "Chess_Knight_White", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Black Pawn", "Chess_Pawn_Black", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("White Pawn", "Chess_Pawn_White", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Black Queen", "Chess_Queen_Black", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("White Queen", "Chess_Queen_White", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Black Rook", "Chess_Rook_Black", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("White Rook", "Chess_Rook_White", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #endregion

            #region Bedroom, Office

            categories = new List<string>
            {
                "Bedroom",
                "Office"
            };

            item = InventoryUtil.GenAsset("Disc", "Disc", categories, 50, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("ID Card", "ID_Card", categories, 80, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Lipstick", "Lipstick", categories, 40, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Money", "Money", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Ticket", "Ticket", categories, 80, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Pocket Watch", "Watch_Pocket", categories, 80, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Hallway, Office

            categories = new List<string>
            {
                "Hallway",
                "Office"
            };

            item = InventoryUtil.GenAsset("Envelope", "Envelope", categories, 40, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Film", "Film", categories, 80, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Folder", "Folder", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Hallway, Outdoors

            categories = new List<string>
            {
                "Hallway",
                "Outdoors"
            };

            item = InventoryUtil.GenAsset("Nail", "Nail", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Padlock", "Padlock", categories, 80, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Kitchen, Hallway

            categories = new List<string>
            {
                "Kitchen",
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Candle", "Candle", categories, 20, "Misc", "Light", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Lit Candle", "Candle_Lit", null, 0, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Bedroom, Bathroom, Office

            categories = new List<string>
            {
                "Bedroom",
                "Bathroom",
                "Office"
            };

            item = InventoryUtil.GenAsset("Ring", "Ring", categories, 80, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Diamond Ring", "Ring_Diamond", categories, 100, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Bedroom, Hallway, Lounge

            categories = new List<string>
            {
                "Bedroom",
                "Hallway",
                "Lounge"
            };

            item = InventoryUtil.GenAsset("Dice D6", "Dice_D6", categories, 40, "Misc", "Throw", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Dice D8", "Dice_D8", categories, 80, "Misc", "Throw", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Dice D10", "Dice_D10", categories, 80, "Misc", "Throw", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Dice D20", "Dice_D20", categories, 80, "Misc", "Throw", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Gamepad", "Gamepad", categories, 80, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Bedroom, Hallway, Outdoors

            categories = new List<string>
            {
                "Bedroom",
                "Hallway",
                "Outdoors"
            };

            #region Balls

            item = InventoryUtil.GenAsset("Basketball", "Ball_Basketball", categories, 60, "Misc", "Throw", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Beach Ball", "Ball_Beach", categories, 60, "Misc", "Throw", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Golf Ball", "Ball_Golf", categories, 60, "Misc", "Throw", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Tennis Ball", "Ball_Tennis", categories, 60, "Misc", "Throw", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Volleyball", "Ball_Volleyball", categories, 60, "Misc", "Throw", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Football", "Football", categories, 60, "Misc", "Throw", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            item = InventoryUtil.GenAsset("Marbles", "Marbles", categories, 40, "Misc", "Throw", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Bedroom, Kitchen, Office

            categories = new List<string>
            {
                "Bedroom",
                "Kitchen",
                "Office"
            };

            item = InventoryUtil.GenAsset("Battery", "Battery", categories, 20, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Safety Pin", "Safety_Pin", categories, 70, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Bedroom, Office, Hallway

            categories = new List<string>
            {
                "Bedroom",
                "Office",
                "Hallway"
            };

            item = InventoryUtil.GenAsset("Pencil", "Pencil", categories, 40, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Puzzle Box", "Puzzle_Box", categories, 80, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Bedroom, Office, Outdoors

            categories = new List<string>
            {
                "Bedroom",
                "Office",
                "Outdoors"
            };

            item = InventoryUtil.GenAsset("Amethyst", "Gem_Amethyst", categories, 90, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Diamond", "Gem_Diamond", categories, 100, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Ruby", "Gem_Ruby", categories, 90, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Sapphire", "Gem_Sapphire", categories, 90, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Topaz", "Gem_Topaz", categories, 90, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Hallway, Lounge, Office

            categories = new List<string>
            {
                "Hallway",
                "Lounge",
                "Office"
            };

            #region Cards

            #region 2

            item = InventoryUtil.GenAsset("2 of Clubs", "Card_2_Clubs", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("2 of Diamonds", "Card_2_Diamonds", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("2 of Hearts", "Card_2_Hearts", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("2 of Spades", "Card_2_Spades", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region 3

            item = InventoryUtil.GenAsset("3 of Clubs", "Card_3_Clubs", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("3 of Diamonds", "Card_3_Diamonds", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("3 of Hearts", "Card_3_Hearts", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("3 of Spades", "Card_3_Spades", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region 4

            item = InventoryUtil.GenAsset("4 of Clubs", "Card_4_Clubs", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("4 of Diamonds", "Card_4_Diamonds", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("4 of Hearts", "Card_4_Hearts", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("4 of Spades", "Card_4_Spades", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region 5

            item = InventoryUtil.GenAsset("5 of Clubs", "Card_5_Clubs", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("5 of Diamonds", "Card_5_Diamonds", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("5 of Hearts", "Card_5_Hearts", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("5 of Spades", "Card_5_Spades", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region 6

            item = InventoryUtil.GenAsset("6 of Clubs", "Card_6_Clubs", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("6 of Diamonds", "Card_6_Diamonds", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("6 of Hearts", "Card_6_Hearts", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("6 of Spades", "Card_6_Spades", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region 7

            item = InventoryUtil.GenAsset("7 of Clubs", "Card_7_Clubs", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("7 of Diamonds", "Card_7_Diamonds", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("7 of Hearts", "Card_7_Hearts", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("7 of Spades", "Card_7_Spades", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region 8

            item = InventoryUtil.GenAsset("8 of Clubs", "Card_8_Clubs", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("8 of Diamonds", "Card_8_Diamonds", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("8 of Hearts", "Card_8_Hearts", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("8 of Spades", "Card_8_Spades", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region 9

            item = InventoryUtil.GenAsset("9 of Clubs", "Card_9_Clubs", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("9 of Diamonds", "Card_9_Diamonds", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("9 of Hearts", "Card_9_Hearts", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("9 of Spades", "Card_9_Spades", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region 10

            item = InventoryUtil.GenAsset("10 of Clubs", "Card_10_Clubs", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("10 of Diamonds", "Card_10_Diamonds", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("10 of Hearts", "Card_10_Hearts", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("10 of Spades", "Card_10_Spades", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Jack

            item = InventoryUtil.GenAsset("Jack of Clubs", "Card_Jack_Clubs", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Jack of Diamonds", "Card_Jack_Diamonds", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Jack of Hearts", "Card_Jack_Hearts", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Jack of Spades", "Card_Jack_Spades", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Queen

            item = InventoryUtil.GenAsset("Queen of Clubs", "Card_Queen_Clubs", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Queen of Diamonds", "Card_Queen_Diamonds", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Queen of Hearts", "Card_Queen_Hearts", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Queen of Spades", "Card_Queen_Spades", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region King

            item = InventoryUtil.GenAsset("King of Clubs", "Card_King_Clubs", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("King of Diamonds", "Card_King_Diamonds", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("King of Hearts", "Card_King_Hearts", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("King of Spades", "Card_King_Spades", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Ace

            item = InventoryUtil.GenAsset("Ace of Clubs", "Card_Ace_Clubs", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Ace of Diamonds", "Card_Ace_Diamonds", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Ace of Hearts", "Card_Ace_Hearts", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Ace of Spades", "Card_Ace_Spades", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Joker

            item = InventoryUtil.GenAsset("Joker", "Card_Joker", categories, 80, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #endregion

            #endregion

            #region Hallway, Kitchen, Outdoors

            categories = new List<string>
            {
                "Hallway",
                "Kitchen",
                "Outdoors"
            };

            item = InventoryUtil.GenAsset("Clothespin", "Clothespin", categories, 40, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Screw", "Screw", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Hallway, Office, Outdoors

            categories = new List<string>
            {
                "Hallway",
                "Office",
                "Outdoors"
            };

            item = InventoryUtil.GenAsset("Newspaper", "Newspaper", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Hallway, Kitchen, Office

            categories = new List<string>
            {
                "Hallway",
                "Kitchen",
                "Office"
            };

            item = InventoryUtil.GenAsset("Stamp", "Stamp", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Bedroom, Hallway, Lounge, Office

            categories = new List<string>
            {
                "Bedroom",
                "Hallway",
                "Lounge",
                "Office"
            };

            item = InventoryUtil.GenAsset("Book", "Book", categories, 60, "Misc", "Read", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Cigar", "Cigar", categories, 60, "Misc", "Light", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Lit Cigar", "Cigar_Lit", null, 0, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Cigarette", "Cigarette", categories, 40, "Misc", "Light", 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Lit Cigarette", "Cigarette_Lit", null, 0, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("VHS Tape", "VHS", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Watch", "Watch", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Bedroom, Kitchen, Office, Hallway, Outdoors

            categories = new List<string>
            {
                "Bedroom",
                "Kitchen",
                "Office",
                "Hallway",
                "Outdoors"
            };

            item = InventoryUtil.GenAsset("Paper", "Paper", categories, 40, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Paperclip", "Paperclip", categories, 40, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region All

            categories = new List<string>
            {
                "Bathroom",
                "Bedroom",
                "Driveway",
                "Grocery Store",
                "Hallway",
                "Kitchen",
                "Lounge",
                "Office",
                "Outdoors",
                "Store Counter"
            };

            item = InventoryUtil.GenAsset("Key", "Key", categories, 60, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region None

            item = InventoryUtil.GenAsset("Golden Ticket", "Ticket_Golden", null, 0, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Finger Print", "Finger_Print", null, 0, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Tire Tracks", "Tire_Tracks", null, 0, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Torch", "Torch", null, 0, "Misc", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #endregion

            #region Body Parts

            item = InventoryUtil.GenAsset("Dead Body", "Body_Dead", null, 0, "Body", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Skeleton", "Body_Skeleton", null, 0, "Body", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Wrapped Body", "Body_Wrapped", null, 0, "Body", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Severed Whole Arm", "BodyPart_Arm", null, 0, "Body", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Severed Arm", "BodyPart_Arm_NoHand", null, 0, "Body", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Brain", "BodyPart_Brain", null, 0, "Body", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Ear", "BodyPart_Ear", null, 0, "Body", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Eye", "BodyPart_Eye", null, 0, "Body", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Severed Foot", "BodyPart_Foot", null, 0, "Body", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Severed Hand", "BodyPart_Hand", null, 0, "Body", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Head", "BodyPart_Head", null, 0, "Body", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Heart", "BodyPart_Heart", null, 0, "Body", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Intestines", "BodyPart_Intestines", null, 0, "Body", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Kidneys", "BodyPart_Kidneys", null, 0, "Body", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Severed Whole Leg", "BodyPart_Leg", null, 0, "Body", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Severed Leg", "BodyPart_Leg_NoFoot", null, 0, "Body", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Liver", "BodyPart_Liver", null, 0, "Body", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Lungs", "BodyPart_Lungs", null, 0, "Body", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Nose", "BodyPart_Nose", null, 0, "Body", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Skull", "BodyPart_Skull", null, 0, "Body", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Stomach", "BodyPart_Stomach", null, 0, "Body", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Tooth", "BodyPart_Tooth", null, 0, "Body", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Torso", "BodyPart_Torso", null, 0, "Body", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Bone", "Bone", null, 0, "Body", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion

            #region Wounds

            item = InventoryUtil.GenAsset("Broken Bone", "Wound_Break", null, 0, "Wound", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Covered Wound", "Wound_Covered", null, 0, "Wound", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Cut Wound", "Wound_Cut", null, 0, "Wound", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Fractured Bone", "Wound_Fracture", null, 0, "Wound", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Gunshot Wound", "Wound_Gunshot", null, 0, "Wound", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Stab Wound", "Wound_Stab", null, 0, "Wound", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            item = InventoryUtil.GenAsset("Stitched Wound", "Wound_Stitched", null, 0, "Wound", null, 0, 0, null);
            assets.Items.Add(item);
            current++;
            Loading_Percent = (current * 100) / Icon_Count;

            #endregion
        }

        #endregion

        #region Get Stuff

        public static long GetID()
        {
            ID++;
            return ID;
        }

        public static Character GetPlayer()
        {
            Army army = CharacterManager.GetArmy("Characters");
            if (army != null)
            {
                Squad squad = army.GetSquad("Players");
                if (squad != null)
                {
                    if (squad.Characters.Count > 0)
                    {
                        return squad.Characters[0];
                    }
                }
            }

            return null;
        }

        #endregion

        #endregion
    }
}

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
        public static int MapSize_X = 10;
        public static int MapSize_Y = 10;
        public static int MaxPop = 1000;

        public static int ActionRate = 320;
        public static int TimeToAction;
        public static bool Action;

        public static bool Menu_Health;
        public static bool WorldMap_Visible;
        public static bool Combat;

        public static bool Holding;
        public static long Holding_ID;
        public static Tile Holding_Tile;
        public static Character Holding_Character;

        public static bool Trading;
        public static List<long> Trading_InventoryID = new List<long>();
        
        public static string[] SkinColors = { "Light", "Tan", "Dark" };
        public static string[] HairLength = { "Bald", "Short", "Long" };
        public static string[] HairColor = { "Black", "Blonde", "Brown", "Grey", "Red", "White" };
        public static string[] Colors = { "Black", "Blue", "Brown", "Cyan", "Green", "Grey", "Orange", "Pink", "Purple", "Red", "White", "Yellow" };
        public static string[] SeeThrough = { "Lamp", "StreetLight", "Counter", "TV", "Stove", "Table", "Dresser", "Bookshelf", "ComputerDesk" };
        public static string[] Searchable = { "Counter", "Fridge", "Dresser", "Desk", "Bookshelf", "Grass" };
        public static string[] BodyParts = { "Head", "Neck", "Torso", "Groin", "Right_Arm", "Right_Hand", "Left_Arm", "Left_Hand", "Right_Leg", "Right_Foot", "Left_Leg", "Left_Foot" };

        public static int MessageMax = 6;
        public static List<string> Messages = new List<string>();

        public static Character Player = null;
        public static Tile Interaction_Tile = null;
        public static Character Interaction_Character = null;

        public static int light_count = 0;
        public static int light_map_width = 0;
        public static Dictionary<Vector2, List<Tile>> light_maps = new Dictionary<Vector2, List<Tile>>();
        public static List<Point> light_sources = new List<Point>();

        public static Dictionary<long, List<Tile>> VisibleTiles = new Dictionary<long, List<Tile>>();

        public static List<Tile> Furniture = new List<Tile>();
        public static List<Tile> TopFurniture = new List<Tile>();
        public static List<Tile> MiddleFurniture = new List<Tile>();
        public static Dictionary<long, List<Tile>> OwnedFurniture = new Dictionary<long, List<Tile>>();

        public static Dictionary<string, string> Stats = new Dictionary<string, string>();
        public static float HungerRate = 0.0006945f;
        public static float ThirstRate = 0.0013888f;
        public static float ComfortRate = 0.00556f;
        public static float BoredomRate = 0.00695f;

        public static int CharGen_Stage;
        public static string Selected_BodyPart;

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
            AssetManager.Init(game, Path.Combine(Environment.CurrentDirectory, "Content"));

            Load_Init();

            string saves = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Despicaville");
            AssetManager.Directories.Add("Saves", saves);
            if (!Directory.Exists(saves))
            {
                Directory.CreateDirectory(saves);
            }

            string config = Path.Combine(AssetManager.Directories["Saves"], "config.ini");
            AssetManager.Files.Add("Config", config);
            if (!File.Exists(config))
            {
                File.Create(config).Close();
                SaveUtil.ExportINI();
            }
            else
            {
                LoadUtil.ParseINI(config);
            }

            InventoryManager.Inventories = new List<Inventory>
            {
                new Inventory
                {
                    ID = GetID(),
                    Name = "Assets"
                }
            };

            AssetManager.Directories.Add("Mods", Path.Combine(Environment.CurrentDirectory, "Mods"));
        }

        public static void Load_Init()
        {
            AssetManager.LoadFonts();

            //Textures
            string[] textures =
            {
                "Colors",
                "Controls",
                "Foliage",
                "Furniture",
                "Hairs",
                "Hats",
                "Pants",
                "Paperdoll",
                "RoomTypes",
                "Screens",
                "Shirts",
                "Shoes",
                "Skins",
                "Tiles"
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
            //SoundManager.AmbientFade = 1;
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
            InputManager.Keyboard.KeysMapped.Add("Combat", Keys.Tab);
            InputManager.Keyboard.KeysMapped.Add("Inventory", Keys.I);
            InputManager.Keyboard.KeysMapped.Add("Map", Keys.M);
            InputManager.Keyboard.KeysMapped.Add("Wait", Keys.Space);
        }

        public static void LoadWorldTextures()
        {
            if (Main.Game.GraphicsManager.GraphicsDevice != null)
            {
                Loading_Percent = 0;
                Loading_Message = "Loading textures...";

                string[] dirs = { "Animations", "Effects", "Icons", "MapTiles", "Particles", "Shaders", "Vehicles", "Wounds" };

                int current = 0;
                int total = 0;

                DirectoryInfo dir = new DirectoryInfo(AssetManager.Directories["Textures"]);
                foreach (DirectoryInfo sub_dir in dir.GetDirectories())
                {
                    if (dirs.Contains(sub_dir.Name))
                    {
                        total += sub_dir.GetFiles("*.png").Length;
                    }
                }

                foreach (DirectoryInfo sub_dir in dir.GetDirectories())
                {
                    if (dirs.Contains(sub_dir.Name))
                    {
                        FileInfo[] files = sub_dir.GetFiles("*.png");

                        int fileCount = files.Length;
                        for (int i = 0; i < fileCount; i++)
                        {
                            FileInfo file = files[i];

                            string name = Path.GetFileNameWithoutExtension(file.Name);
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
            Stats = new Dictionary<string, string>
            {
                {
                    "Strength",
                    "How much physical force you can apply."
                },
                {
                    "Vitality",
                    "How quickly your wounds heal."
                },
                {
                    "Endurance",
                    "How long you can sustain physical exertion."
                },
                {
                    "Agility",
                    "How quickly you can move and react."
                },
                {
                    "Intelligence",
                    "How quickly you learn new things."
                },
                {
                    "Perception",
                    "How quickly you notice details."
                },
                {
                    "Charisma",
                    "How persuasive you are."
                },
                {
                    "Willpower",
                    "How much pain you can tolerate."
                },
                {
                    "Sanity",
                    "How well you manage paranoia."
                },
                {
                    "Luck",
                    "How lucky you are."
                },
            };

            CharacterManager.LastNames.Sort();
        }

        #endregion

        public static long GetID()
        {
            ID++;
            return ID;
        }

        #endregion
    }
}

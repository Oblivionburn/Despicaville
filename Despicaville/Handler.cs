using System;
using System.IO;
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
using OP_Engine.Controls;
using OP_Engine.Menus;
using OP_Engine.Jobs;
using OP_Engine.Time;
using Despicaville.Util;
using Despicaville.JobTasks;

namespace Despicaville
{
    public class Handler : GameComponent
    {
        #region Variables

        public static long ID;
        public static List<FileInfo> Textures = [];

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

        public static bool Pull;
        public static long Pull_ID;
        public static Tile? Pull_Tile;
        public static Character? Pull_Character;

        public static bool Trading;
        public static List<long> Trading_InventoryID = [];

        public static string[] SkinColors = ["Light", "Tan", "Dark"];
        public static string[] HairLength = ["Bald", "Short", "Long"];
        public static string[] HairColor = ["Black", "Blonde", "Brown", "Grey", "Red", "White"];
        public static string[] Colors = ["Black", "Blue", "Brown", "Cyan", "Green", "Grey", "Orange", "Pink", "Purple", "Red", "White", "Yellow"];
        public static string[] BodyParts = ["Head", "Neck", "Torso", "Groin", "Right_Arm", "Right_Hand", "Left_Arm", "Left_Hand", "Right_Leg", "Right_Foot", "Left_Leg", "Left_Foot"];

        public static int MessageMax = 6;
        public static List<string> Messages = [];

        public static Character? Player = null;
        public static Tile? Interaction_Tile = null;
        public static Character? Interaction_Character = null;

        public static int light_count = 0;
        public static int light_map_width = 0;
        public static Dictionary<Vector2, List<Tile>> light_maps = [];
        public static List<Point> light_sources = [];

        public static Dictionary<long, List<Tile>> VisibleTiles = [];

        public static List<Tile> Furniture = []; //List of furniture assets, not stuff in the world

        public static List<Tile> TopFurniture = [];
        public static List<Tile> MiddleFurniture = [];
        public static Dictionary<long, List<Tile>> OwnedFurniture = [];

        public static Dictionary<string, string> Stats = [];
        public static float HungerRate = 0.0006945f;
        public static float ThirstRate = 0.0013888f;
        public static float ComfortRate = 0.00556f;
        public static float BoredomRate = 0.00695f;

        public static List<Job> Jobs = [];

        public static int CharGen_Stage;
        public static string? Selected_BodyPart;

        public static int Loading_Stage;
        public static int Loading_Percent;
        public static string? Loading_Message;
        public static CancellationTokenSource? LoadingTokenSource;
        public static Task? LoadingTask;

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
            DirectoryInfo textures_dir = new(AssetManager.Directories["Textures"]);
            foreach (DirectoryInfo dir in textures_dir.GetDirectories())
            {
                FileInfo[] files = dir.GetFiles("*.png");

                int fileCount = files.Length;
                for (int i = 0; i < fileCount; i++)
                {
                    Textures.Add(files[i]);
                }
            }

            //Sounds
            AssetManager.LoadSounds();

            string[] sounds =
            [
                "Click",
                "DoorClose",
                "DoorOpen",
                "Equip",
                "GlassBreak",
                "Punch",
                "Purchase",
                "WindowClose",
                "WindowOpen"
            ];
            foreach (string dir in sounds)
            {
                AssetManager.LoadSounds(dir);
            }
            SoundManager.SoundVolume = 0.4f;
            SoundManager.SoundEnabled = true;

            //Ambient
            string[] ambient =
            [
                "Rain",
                "Storm",
                "Wind"
            ];
            foreach (string dir in ambient)
            {
                AssetManager.LoadAmbient(dir);
            }
            //SoundManager.AmbientFade = 1;
            SoundManager.AmbientVolume = 0.6f;
            SoundManager.AmbientEnabled = true;

            //Music
            string[] music =
            [
                "Day",
                "Defeat",
                "Loading",
                "Night",
                "Title"
            ];
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
            InputManager.Keyboard?.KeysMapped.Add("Cancel", Keys.Escape);
            InputManager.Keyboard?.KeysMapped.Add("Debug", Keys.OemTilde);
            InputManager.Keyboard?.KeysMapped.Add("Up", Keys.W);
            InputManager.Keyboard?.KeysMapped.Add("Right", Keys.D);
            InputManager.Keyboard?.KeysMapped.Add("Down", Keys.S);
            InputManager.Keyboard?.KeysMapped.Add("Left", Keys.A);
            InputManager.Keyboard?.KeysMapped.Add("Crouch", Keys.LeftControl);
            InputManager.Keyboard?.KeysMapped.Add("Run", Keys.LeftShift);
            InputManager.Keyboard?.KeysMapped.Add("Combat", Keys.Tab);
            InputManager.Keyboard?.KeysMapped.Add("Inventory", Keys.I);
            InputManager.Keyboard?.KeysMapped.Add("Map", Keys.M);
            InputManager.Keyboard?.KeysMapped.Add("Wait", Keys.Space);
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

        public static void LoadJobs()
        {
            Jobs =
            [
                new Job
                {
                    ID = GetID(),
                    Name = "1st-Shift_Diner_Cashier",
                    Schedule =
                    [
                        new Appointment
                        {
                            Name = "Attend_Register",
                            StartTime = new TimeHandler((long)7, 0, 0, 0),
                            EndTime = new TimeHandler((long)14, 0, 0, 0)
                        }
                    ],
                    Tasks =
                    [
                        new Cashier
                        {
                            Name = "Cashier",
                            Type = "Attend",
                            Assignment = "Register",
                            StartTime = new TimeHandler((long)7, 0, 0, 0),
                            EndTime = new TimeHandler((long)14, 0, 0, 0)
                        }
                    ]
                },
                new Job
                {
                    ID = GetID(),
                    Name = "2nd-Shift_Diner_Cashier",
                    Schedule =
                    [
                        new Appointment
                        {
                            Name = "Attend_Register",
                            StartTime = new TimeHandler((long)7, 0, 0, 0),
                            EndTime = new TimeHandler((long)14, 0, 0, 0)
                        }
                    ],
                    Tasks =
                    [
                        new Cashier
                        {
                            Name = "Cashier",
                            Type = "Attend",
                            Assignment = "Register",
                            StartTime = new TimeHandler((long)7, 0, 0, 0),
                            EndTime = new TimeHandler((long)14, 0, 0, 0)
                        }
                    ]
                },
                new Job
                {
                    ID = GetID(),
                    Name = "1st-Shift_Grocery_Cashier",
                    Schedule =
                    [
                        new Appointment
                        {
                            Name = "Attend_Register",
                            StartTime = new TimeHandler((long)7, 0, 0, 0),
                            EndTime = new TimeHandler((long)14, 0, 0, 0)
                        }
                    ],
                    Tasks =
                    [
                        new Cashier
                        {
                            Name = "Cashier",
                            Type = "Attend",
                            Assignment = "Register",
                            StartTime = new TimeHandler((long)7, 0, 0, 0),
                            EndTime = new TimeHandler((long)14, 0, 0, 0)
                        }
                    ]
                },
                new Job
                {
                    ID = GetID(),
                    Name = "2nd-Shift_Grocery_Cashier",
                    Schedule =
                    [
                        new Appointment
                        {
                            Name = "Attend_Register",
                            StartTime = new TimeHandler((long)7, 0, 0, 0),
                            EndTime = new TimeHandler((long)14, 0, 0, 0)
                        }
                    ],
                    Tasks =
                    [
                        new Cashier
                        {
                            Name = "Cashier",
                            Type = "Attend",
                            Assignment = "Register",
                            StartTime = new TimeHandler((long)7, 0, 0, 0),
                            EndTime = new TimeHandler((long)14, 0, 0, 0)
                        }
                    ]
                }
            ];
        }

        #endregion

        public static long GetID()
        {
            ID++;
            return ID;
        }

        public static Texture2D? GetTexture(string name)
        {
            if (AssetManager.Textures.TryGetValue(name, out Texture2D? value))
            {
                return value;
            }
            else
            {
                int total = Textures.Count;

                for (int i = 0; i < total; i++)
                {
                    FileInfo fileInfo = Textures[i];
                    string fileName = Path.GetFileNameWithoutExtension(fileInfo.Name);

                    if (fileName == name &&
                        Main.Game?.GraphicsManager != null)
                    {
                        using (FileStream stream = new(fileInfo.FullName, FileMode.Open))
                        {
                            Texture2D texture2D = Texture2D.FromStream(Main.Game.GraphicsManager.GraphicsDevice, stream);
                            texture2D.Name = fileName;
                            AssetManager.Textures.Add(fileName, texture2D);
                            return texture2D;
                        }
                    }
                }
            }

            return null;
        }

        public static void ResetPull()
        {
            Pull = false;
            Pull_ID = 0;
            Pull_Tile = null;
            Pull_Character = null;

            Label? label = MenuManager.GetMenu("UI")?.GetLabel("Pulling");
            if (label != null)
            {
                label.Opacity = 0.6f;
                label.TextColor = Color.White;
            }
        }

        #endregion
    }
}

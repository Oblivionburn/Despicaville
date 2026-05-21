using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OP_Engine.Enums;
using OP_Engine.Inventories;
using OP_Engine.Sounds;
using OP_Engine.Tiles;
using OP_Engine.Utility;

namespace Despicaville.Util
{
    public static class ModUtil
    {
        #region Variables

        private static List<FileInfo> Textures = new List<FileInfo>();
        private static List<FileInfo> Sounds = new List<FileInfo>();
        private static List<FileInfo> Items = new List<FileInfo>();
        private static List<FileInfo> Maps = new List<FileInfo>();
        private static List<FileInfo> Furniture = new List<FileInfo>();

        #endregion

        #region Methods

        public static void LoadMods()
        {
            Textures.Clear();
            Sounds.Clear();
            Items.Clear();
            Maps.Clear();
            Furniture.Clear();

            WorldGen.Blocks.Clear();
            Handler.Furniture.Clear();

            DirectoryInfo modsBaseDir = new DirectoryInfo(AssetManager.Directories["Mods"]);
            DirectoryInfo[] modsDirs = modsBaseDir.GetDirectories();
            foreach (DirectoryInfo modDir in modsDirs)
            {
                string modName = modDir.Name;

                DirectoryInfo[] modDirs = modDir.GetDirectories();

                //Load Textures first
                foreach (DirectoryInfo dir in modDirs)
                {
                    ScanTextures(dir);
                }
                LoadTextures(modName);

                //Load Sounds
                foreach (DirectoryInfo dir in modDirs)
                {
                    ScanSounds(dir);
                }
                LoadSounds(modName);

                //Load Items
                foreach (DirectoryInfo dir in modDirs)
                {
                    ScanItems(dir);
                }
                LoadItems(modName);

                //Load Furniture
                foreach (DirectoryInfo dir in modDirs)
                {
                    ScanFurniture(dir);
                }
                LoadFurniture(modName);

                //Load Maps
                foreach (DirectoryInfo dir in modDirs)
                {
                    ScanMaps(dir);
                }
                LoadMaps(modName);
            }
        }

        private static void ScanTextures(DirectoryInfo dir)
        {
            FileInfo[] files = dir.GetFiles("*.png");

            int count = files.Length;
            for (int i = 0; i < count; i++)
            {
                Textures.Add(files[i]);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            foreach (DirectoryInfo subDir in dirs)
            {
                ScanTextures(subDir);
            }
        }

        private static void LoadTextures(string modName)
        {
            Handler.Loading_Percent = 0;
            Handler.Loading_Message = "Loading " + modName + " Textures...";

            int current = 0;
            int total = Textures.Count;

            for (int i = 0; i < total; i++)
            {
                FileInfo fileInfo = Textures[i];

                string fileName = Path.GetFileNameWithoutExtension(fileInfo.Name);
                if (!AssetManager.Textures.ContainsKey(fileName))
                {
                    using (FileStream stream = new FileStream(fileInfo.FullName, FileMode.Open))
                    {
                        Texture2D texture2D = Texture2D.FromStream(Main.Game.GraphicsManager.GraphicsDevice, stream);
                        texture2D.Name = fileName;
                        AssetManager.Textures.Add(fileName, texture2D);
                    }

                    current++;
                    Handler.Loading_Percent = (current * 100) / total;
                }
            }
        }

        private static void ScanSounds(DirectoryInfo dir)
        {
            List<FileInfo> Files = new List<FileInfo>();

            FileInfo[] files = dir.GetFiles();
            for (int i = 0; i < files.Length; i++)
            {
                FileInfo file = files[i];

                if (AssetManager.SoundExtensions.Contains(file.Extension))
                {
                    //mp3, wma, wav, or ogg
                    Files.Add(file);
                }
            }

            int count = Files.Count;
            for (int i = 0; i < count; i++)
            {
                Sounds.Add(Files[i]);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            foreach (DirectoryInfo subDir in dirs)
            {
                ScanSounds(subDir);
            }
        }

        private static void LoadSounds(string modName)
        {
            Handler.Loading_Percent = 0;
            Handler.Loading_Message = "Loading " + modName + " Sounds...";

            int current = 0;
            int total = Sounds.Count;

            for (int i = 0; i < total; i++)
            {
                FileInfo fileInfo = Sounds[i];

                Sound sound = new Sound
                {
                    Extension = fileInfo.Extension,
                    Name = Path.GetFileNameWithoutExtension(fileInfo.FullName),
                    Directory = Path.GetDirectoryName(fileInfo.FullName)
                };

                //Assumes your sound files end with numbers for variations of each sound
                //because nobody likes hearing the same sound repeatedly for hours
                //Example: Hit1.mp3, Hit2.mp3, Hit3.mp3,etc
                sound.Type = sound.Name.Substring(0, sound.Name.Length - 1);

                if (!AssetManager.Sounds.ContainsKey(sound.Type))
                {
                    Dictionary<string, Sound> sounds = new Dictionary<string, Sound>
                    {
                        { sound.Name, sound }
                    };

                    AssetManager.Sounds.Add(sound.Type, sounds);
                }
                else if (!AssetManager.Sounds[sound.Type].ContainsKey(sound.Name))
                {
                    AssetManager.Sounds[sound.Type].Add(sound.Name, sound);
                }

                current++;
                Handler.Loading_Percent = (current * 100) / total;
            }
        }

        private static void ScanItems(DirectoryInfo dir)
        {
            FileInfo[] files = dir.GetFiles("*.item");

            int count = files.Length;
            for (int i = 0; i < count; i++)
            {
                Items.Add(files[i]);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            foreach (DirectoryInfo subDir in dirs)
            {
                ScanItems(subDir);
            }
        }

        private static void LoadItems(string modName)
        {
            Handler.Loading_Percent = 0;
            Handler.Loading_Message = "Loading " + modName + " Items...";

            int current = 0;
            int total = Items.Count;

            for (int i = 0; i < total; i++)
            {
                FileInfo fileInfo = Items[i];

                using (XmlTextReader reader = new XmlTextReader(File.OpenRead(fileInfo.FullName)))
                {
                    try
                    {
                        while (reader.Read())
                        {
                            switch (reader.Name)
                            {
                                case "Item":
                                    LoadItem(reader);

                                    current++;
                                    Handler.Loading_Percent = (current * 100) / total;
                                    break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Main.Game.CrashHandler(e);
                    }
                }
            }
        }

        private static void ScanMaps(DirectoryInfo dir)
        {
            FileInfo[] files = dir.GetFiles("*.blockmap");

            int count = files.Length;
            for (int i = 0; i < count; i++)
            {
                Maps.Add(files[i]);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            foreach (DirectoryInfo subDir in dirs)
            {
                ScanMaps(subDir);
            }
        }

        private static void LoadMaps(string modName)
        {
            Handler.Loading_Percent = 0;
            Handler.Loading_Message = "Loading " + modName + " Maps...";

            int current = 0;
            int total = Maps.Count;

            for (int i = 0; i < total; i++)
            {
                FileInfo file = Maps[i];

                WorldGen.ParseBlock(file.FullName);

                current++;
                Handler.Loading_Percent = (current * 100) / total;
            }
        }

        private static void LoadItem(XmlTextReader reader)
        {
            Inventory assets = InventoryManager.GetInventory("Assets");
            Item item = null;

            while (reader.Read())
            {
                if (reader.Name == "Item" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "Properties":
                        item = new Item();
                        LoadItemProperties(reader, item);
                        assets.Items.Add(item);
                        break;

                    case "Wounds":
                        LoadWounds(reader, item);
                        break;

                    case "Effects":
                        LoadEffects(reader, item);
                        break;

                    case "Rooms":
                        LoadRooms(reader, item);
                        break;
                }
            }
        }

        private static void LoadItemProperties(XmlTextReader reader, Item item)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "Name":
                        item.Name = reader.Value;
                        break;

                    case "Icon":
                        string icon = reader.Value;

                        try
                        {
                            item.Icon = AssetManager.Textures[icon];
                        }
                        catch (Exception ex)
                        {
                            string error = ex.Message;
                        }
                        
                        item.Icon_Image = new Rectangle(0, 0, item.Icon.Width, item.Icon.Height);
                        item.Icon_DrawColor = Color.White;
                        break;

                    case "Texture":
                        string texture = reader.Value;

                        item.Texture = AssetManager.Textures[texture];
                        item.Image = new Rectangle(0, 0, item.Texture.Width, item.Texture.Height);
                        item.DrawColor = Color.White;
                        break;

                    case "Type":
                        item.Type = reader.Value;

                        if (item.Type == "Container")
                        {
                            item.Inventory.ID = Handler.GetID();
                            item.Inventory.Name = item.Name;
                        }
                        break;

                    case "Tier":
                        item.Tier = int.Parse(reader.Value);
                        break;

                    case "InventorySlots":
                        item.Inventory.ID = Handler.GetID();
                        item.Inventory.Name = item.Name;
                        item.Inventory.Max_Value = int.Parse(reader.Value);
                        break;

                    case "Action":
                        item.Task = reader.Value;
                        break;

                    case "Sound":
                        item.Sound = reader.Value;
                        break;

                    case "SoundRange":
                        item.SoundRange = int.Parse(reader.Value);
                        break;
                }
            }
        }

        private static void LoadWounds(XmlTextReader reader, Item item)
        {
            while (reader.Read())
            {
                if (reader.Name == "Wounds" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "Wound":
                        Property property = new Property();
                        LoadWound(reader, property);
                        item.Properties.Add(property);
                        break;
                }
            }
        }

        private static void LoadWound(XmlTextReader reader, Property property)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "Name":
                        property.Name = reader.Value;
                        break;
                }
            }
        }

        private static void LoadEffects(XmlTextReader reader, Item item)
        {
            while (reader.Read())
            {
                if (reader.Name == "Effects" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "Effect":
                        Property property = new Property();
                        LoadEffect(reader, property);
                        item.Properties.Add(property);
                        break;
                }
            }
        }

        private static void LoadEffect(XmlTextReader reader, Property property)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "Name":
                        property.Name = reader.Value;
                        break;

                    case "Value":
                        property.Value = float.Parse(reader.Value);
                        break;
                }
            }
        }

        private static void LoadRooms(XmlTextReader reader, Item item)
        {
            while (reader.Read())
            {
                if (reader.Name == "Rooms" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "Room":
                        LoadRoom(reader, item);
                        break;
                }
            }
        }

        private static void LoadRoom(XmlTextReader reader, Item item)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "Name":
                    case "Container":
                        string name = reader.Value;

                        if (!item.Categories.Contains(name))
                        {
                            item.Categories.Add(name);
                        }
                        break;
                }
            }
        }

        private static void ScanFurniture(DirectoryInfo dir)
        {
            FileInfo[] files = dir.GetFiles("*.furniture");

            int count = files.Length;
            for (int i = 0; i < count; i++)
            {
                Furniture.Add(files[i]);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            foreach (DirectoryInfo subDir in dirs)
            {
                ScanFurniture(subDir);
            }
        }

        private static void LoadFurniture(string modName)
        {
            Handler.Loading_Percent = 0;
            Handler.Loading_Message = "Loading " + modName + " Furniture...";

            int current = 0;
            int total = Furniture.Count;

            for (int i = 0; i < total; i++)
            {
                FileInfo fileInfo = Furniture[i];

                using (XmlTextReader reader = new XmlTextReader(File.OpenRead(fileInfo.FullName)))
                {
                    try
                    {
                        while (reader.Read())
                        {
                            switch (reader.Name)
                            {
                                case "Furniture":
                                    LoadFurniture(reader);

                                    current++;
                                    Handler.Loading_Percent = (current * 100) / total;
                                    break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Main.Game.CrashHandler(e);
                    }
                }
            }
        }

        private static void LoadFurniture(XmlTextReader reader)
        {
            while (reader.Read())
            {
                if (reader.Name == "Furniture" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "Properties":
                        Tile tile = new Tile();
                        LoadFurnitureProperties(reader, tile);
                        Handler.Furniture.Add(tile);
                        break;
                }
            }
        }

        private static void LoadFurnitureProperties(XmlTextReader reader, Tile tile)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "Name":
                        tile.Name = reader.Value;
                        break;

                    case "Texture":
                        string texture = reader.Value;

                        tile.Texture = AssetManager.Textures[texture];
                        tile.Image = new Rectangle(0, 0, tile.Texture.Width, tile.Texture.Height);
                        tile.DrawColor = Color.White;
                        break;

                    case "Direction":
                        if (Enum.TryParse(reader.Value, out Direction direction))
                        {
                            tile.Direction = direction;
                        }
                        break;

                    case "BlocksMovement":
                        tile.BlocksMovement = reader.Value == "True";
                        break;

                    case "BlocksSight":
                        tile.BlocksSight = reader.Value == "True";
                        break;

                    case "Usable":
                        tile.CanUse = reader.Value == "True";
                        break;

                    case "Sound":
                        tile.Sound = reader.Value;
                        break;

                    case "SoundRange":
                        tile.SoundRange = int.Parse(reader.Value);
                        break;

                    case "Movable":
                        tile.CanMove = reader.Value == "True";
                        break;

                    case "LightSource":
                        tile.IsLightSource = reader.Value == "True";
                        break;

                    case "LightColor":
                        string[] values = reader.Value.Split(',');
                        int R = int.Parse(values[0]);
                        int G = int.Parse(values[1]);
                        int B = int.Parse(values[2]);

                        tile.LightColor = new Color(R, G, B, 255);
                        break;
                }
            }
        }

        #endregion
    }
}

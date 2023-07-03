﻿using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using OP_Engine.Tiles;
using OP_Engine.Utility;
using OP_Engine.Scenes;
using OP_Engine.Inventories;
using OP_Engine.Characters;

namespace Despicaville.Util
{
    public static class WorldGen
    {
        #region Variables

        public static List<Tile> Worldmap = new List<Tile>();
        private static List<Map> Blocks = new List<Map>();
        private static List<Map> Available_Commercial = new List<Map>();
        private static List<Map> ParkBlocks = new List<Map>();

        public static List<Map> Residential = new List<Map>();
        public static List<Map> Commercial = new List<Map>();
        public static List<Map> Parks = new List<Map>();
        public static List<Map> Roads = new List<Map>();

        private static int column;
        private static int row;

        private static int total;
        private static int current;

        private static string last_name;

        #endregion

        #region Methods

        public static void GetBlocks()
        {
            Handler.Loading_Percent = 0;
            Handler.Loading_Message = "Getting available blocks...";

            DirectoryInfo dir = new DirectoryInfo(AssetManager.Directories["Maps"]);
            FileInfo[] files = dir.GetFiles("*.blockmap");

            total = files.Length;
            current = 0;

            Blocks.Clear();
            for (int i = 0; i < total; i++)
            {
                FileInfo file = files[i];

                ParseBlock(file.FullName);

                current++;
                Handler.Loading_Percent = (current * 100) / total;
            }
        }

        #region GetBlocks

        private static void ParseBlock(string file)
        {
            using (XmlTextReader reader = new XmlTextReader(File.OpenRead(file)))
            {
                try
                {
                    while (reader.Read())
                    {
                        switch (reader.Name)
                        {
                            case "Map":
                                VisitBlock(reader);
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

        private static void VisitBlock(XmlTextReader reader)
        {
            Map map = null;
            Layer layer = null;

            while (reader.Read())
            {
                if (reader.Name == "Map" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "Globals":
                        map = new Map();
                        VisitGlobals(reader, map);
                        Blocks.Add(map);
                        break;

                    case "BottomTiles":
                        layer = new Layer();
                        layer.Name = "BottomTiles";

                        VisitBottomTiles(reader, layer);
                        map.Layers.Add(layer);
                        break;

                    case "RoomTiles":
                        layer = new Layer();
                        layer.Name = "RoomTiles";

                        VisitRoomTiles(reader, layer);
                        map.Layers.Add(layer);
                        break;

                    case "MiddleTiles":
                        layer = new Layer();
                        layer.Name = "MiddleTiles";

                        VisitMiddleTiles(reader, layer);
                        map.Layers.Add(layer);
                        break;

                    case "TopTiles":
                        layer = new Layer();
                        layer.Name = "TopTiles";

                        VisitTopTiles(reader, layer);
                        map.Layers.Add(layer);
                        break;
                }
            }
        }

        private static void VisitGlobals(XmlTextReader reader, Map map)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "Type":
                        map.Type = reader.Value;
                        break;

                    case "Name":
                        map.Name = Path.GetFileNameWithoutExtension(reader.Value);
                        break;

                    case "Direction":
                        string value = reader.Value;
                        if (value == "North")
                        {
                            value = "Up";
                        }
                        else if (value == "East")
                        {
                            value = "Right";
                        }
                        else if (value == "South")
                        {
                            value = "Down";
                        }
                        else if (value == "West")
                        {
                            value = "Left";
                        }

                        Direction direction;
                        if (Enum.TryParse(value, out direction))
                        {
                            map.Direction = direction;
                        }
                        break;
                }
            }
        }

        private static void VisitBottomTiles(XmlTextReader reader, Layer layer)
        {
            Tile tile = null;

            while (reader.Read())
            {
                if (reader.Name == "BottomTiles" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "BottomTile":
                        tile = new Tile();
                        VisitBottomTile(reader, tile);
                        layer.Tiles.Add(tile);
                        break;
                }
            }
        }

        private static void VisitBottomTile(XmlTextReader reader, Tile tile)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "Texture":
                        tile.Name = reader.Value;
                        tile.Texture = AssetManager.Textures[tile.Name];
                        break;

                    case "Location":
                        string[] values = reader.Value.Split(',');
                        tile.Location.X = int.Parse(values[0]);
                        tile.Location.Y = int.Parse(values[1]);
                        break;
                }
            }
        }

        private static void VisitRoomTiles(XmlTextReader reader, Layer layer)
        {
            Tile tile = null;

            while (reader.Read())
            {
                if (reader.Name == "RoomTiles" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "RoomTile":
                        tile = new Tile();
                        VisitRoomTile(reader, tile);
                        layer.Tiles.Add(tile);
                        break;
                }
            }
        }

        private static void VisitRoomTile(XmlTextReader reader, Tile tile)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "RoomType":
                        tile.Name = reader.Value;
                        tile.Texture = AssetManager.Textures[tile.Name];
                        break;

                    case "Location":
                        string[] values = reader.Value.Split(',');
                        tile.Location.X = int.Parse(values[0]);
                        tile.Location.Y = int.Parse(values[1]);
                        break;
                }
            }
        }

        private static void VisitMiddleTiles(XmlTextReader reader, Layer layer)
        {
            Tile tile = null;

            while (reader.Read())
            {
                if (reader.Name == "MiddleTiles" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "MiddleTile":
                        tile = new Tile();
                        VisitBottomTile(reader, tile);
                        layer.Tiles.Add(tile);
                        break;
                }
            }
        }

        private static void VisitTopTiles(XmlTextReader reader, Layer layer)
        {
            Tile tile = null;

            while (reader.Read())
            {
                if (reader.Name == "TopTiles" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "TopTile":
                        tile = new Tile();
                        VisitBottomTile(reader, tile);
                        layer.Tiles.Add(tile);
                        break;
                }
            }
        }

        #endregion

        public static void GenMap()
        {
            Handler.Loading_Percent = 0;
            Handler.Loading_Message = "Generating new map...";

            total = Handler.MapSize_X * Handler.MapSize_Y;
            current = 0;

            column = 0;
            row = 0;

            Worldmap.Clear();

            Available_Commercial.Clear();
            ParkBlocks.Clear();

            foreach (Map block in Blocks)
            {
                if (block.Type == "Park")
                {
                    ParkBlocks.Add(block);
                }
                else if (block.Type == "Commercial")
                {
                    Available_Commercial.Add(block);
                }
            }

            CryptoRandom random = new CryptoRandom();
            int choice = 0;

            string previous_type = "";
            string previous_w = "";

            #region First Row

            AddMapTile("Road_Corner_NW", "Map_Road_Corner_NW", 0, 0);

            column++;
            AddMapTile("Road_WE", "Map_Road_WE", column, 0);

            if (Handler.MapSize_X > 3)
            {
                if (Handler.MapSize_X > 4)
                {
                    choice = random.Next(1, 4);
                    if (choice <= 2)
                    {
                        column++;
                        AddMapTile("Road_TSection_S", "Map_Road_TSection_S", column, 0);
                    }
                    else if (choice == 3)
                    {
                        column++;
                        AddMapTile("Road_WE", "Map_Road_WE", column, 0);
                    }

                    if (Handler.MapSize_X > 5)
                    {
                        int size = Handler.MapSize_X - 5;
                        for (int i = 0; i < size; i++)
                        {
                            if (previous_w == "Road_WE")
                            {
                                choice = random.Next(1, 4);
                                if (choice <= 2)
                                {
                                    column++;
                                    AddMapTile("Road_TSection_S", "Map_Road_TSection_S", column, 0);
                                    previous_type = previous_w = "Road_TSection_S";
                                }
                                else if (choice == 3)
                                {
                                    column++;
                                    AddMapTile("Road_WE", "Map_Road_WE", column, 0);
                                    previous_type = previous_w = "Road_WE";
                                }
                            }
                            else
                            {
                                column++;
                                AddMapTile("Road_WE", "Map_Road_WE", column, 0);
                                previous_type = previous_w = "Road_WE";
                            }
                        }
                    }
                }

                column++;
                AddMapTile("Road_WE", "Map_Road_WE", column, 0);
            }

            column++;
            AddMapTile("Road_Corner_NE", "Map_Road_Corner_NE", column, 0);

            #endregion

            #region Inner Rows

            for (int y = 1; y < Handler.MapSize_Y - 1; y++)
            {
                row = y;
                column = 0;

                if (previous_w == "Road_NS" &&
                    y < Handler.MapSize_Y - 2)
                {
                    choice = random.Next(1, 4);
                    if (choice <= 2)
                    {
                        AddMapTile("Road_TSection_E", "Map_Road_TSection_E", 0, row);
                        previous_type = previous_w = "Road_TSection_E";
                    }
                    else if (choice == 3)
                    {
                        AddMapTile("Road_NS", "Map_Road_NS", 0, row);
                        previous_type = previous_w = "Road_NS";
                    }
                }
                else
                {
                    AddMapTile("Road_NS", "Map_Road_NS", 0, row);
                    previous_type = previous_w = "Road_NS";
                }

                for (int x = 1; x < Handler.MapSize_X - 1; x++)
                {
                    if (!string.IsNullOrEmpty(previous_type))
                    {
                        previous_type = AddToMap_InnerRows(previous_type, x, y);
                    }
                }

                bool Previous_Block = GetPreviousBlock(previous_type) != null ? true : false;
                if (Previous_Block)
                {
                    column++;
                    previous_type = AddMapTile("Road_NS", "Map_Road_NS", column, row);
                }
                else if (previous_type == "Road_NS" ||
                         previous_type == "Road_Corner_NE" ||
                         previous_type == "Road_Corner_SE" ||
                         previous_type == "Road_TSection_W")
                {
                    column++;
                    previous_type = AddMapTile("Road_NS", "Map_Road_NS", column, row);
                }
                else
                {
                    column++;
                    previous_type = AddMapTile("Road_TSection_W", "Map_Road_TSection_W", column, row);
                }
            }

            #endregion

            #region Last Row

            row = Handler.MapSize_Y - 1;
            column = 0;

            previous_type = AddMapTile("Road_Corner_SW", "Map_Road_Corner_SW", 0, row);

            for (int i = 1; i < Handler.MapSize_X - 1; i++)
            {
                if (!string.IsNullOrEmpty(previous_type))
                {
                    previous_type = AddToMap_LastRow(i);
                }
            }

            previous_type = AddMapTile("Road_Corner_SE", "Map_Road_Corner_SE", Handler.MapSize_X - 1, row);

            #endregion
        }

        #region GenMap

        private static string AddToMap_InnerRows(string previous, int x, int y)
        {
            CryptoRandom random = new CryptoRandom();
            string this_block = "";
            Tile upper_block = Worldmap[x + (Handler.MapSize_X * (row - 1))];

            column++;

            if (previous == "Road_NS" ||
                previous == "Road_TSection_W" ||
                previous == "Road_Corner_NE" ||
                previous == "Road_Corner_SE")
            {
                #region Road Not Coming From West

                if (upper_block.Name == "Road_Cross" ||
                    upper_block.Name == "Road_TSection_S" ||
                    upper_block.Name == "Road_TSection_W" ||
                    upper_block.Name == "Road_TSection_E" ||
                    upper_block.Name == "Road_Corner_NW" ||
                    upper_block.Name == "Road_Corner_NE")
                {
                    this_block = AddMapTile("Road_NS", "Map_Road_NS", column, row);
                }
                else if (upper_block.Name == "Road_NS")
                {
                    if (y == Handler.MapSize_Y - 2)
                    {
                        this_block = AddMapTile("Road_NS", "Map_Road_NS", column, row);
                    }
                    else
                    {
                        int type = random.Next(1, 3);
                        if (type == 1)
                        {
                            this_block = AddMapTile("Road_NS", "Map_Road_NS", column, row);
                        }
                        else if (type == 2)
                        {
                            this_block = AddMapTile("Road_TSection_E", "Map_Road_TSection_E", column, row);
                        }
                    }
                }
                else
                {
                    bool BlockAbove_South = false;
                    Map blockAbove = GetBlockAbove(upper_block);
                    if (blockAbove != null)
                    {
                        if (blockAbove.Type != "Park" &&
                            blockAbove.Direction == Direction.Down)
                        {
                            BlockAbove_South = true;
                        }
                    }

                    if (BlockAbove_South)
                    {
                        this_block = AddMapTile("Road_Corner_NW", "Map_Road_Corner_NW", column, row);
                    }
                    else
                    {
                        bool BlockAbove = GetBlockAbove(upper_block) != null;
                        if (BlockAbove)
                        {
                            if (Available_Commercial.Count > 0)
                            {
                                int type = random.Next(1, 11);
                                if (type <= 3)
                                {
                                    #region Residential

                                    if (y == Handler.MapSize_Y - 2)
                                    {
                                        List<Map> WestBlocks = new List<Map>();
                                        foreach (Map block in Blocks)
                                        {
                                            if (block.Type == "Residential" &&
                                                block.Direction == Direction.Left)
                                            {
                                                WestBlocks.Add(block);
                                            }
                                        }

                                        if (WestBlocks.Count > 0)
                                        {
                                            int choice = random.Next(0, WestBlocks.Count);
                                            this_block = AddMapTile(WestBlocks[choice].Name, "Map_Residential", column, row);
                                        }
                                        else
                                        {
                                            int choice = random.Next(0, ParkBlocks.Count);
                                            this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                        }
                                    }
                                    else
                                    {
                                        if (x == Handler.MapSize_X - 3)
                                        {
                                            List<Map> WestBlocks = new List<Map>();
                                            foreach (Map block in Blocks)
                                            {
                                                if (block.Type == "Residential" &&
                                                    block.Direction == Direction.Left)
                                                {
                                                    WestBlocks.Add(block);
                                                }
                                            }

                                            if (WestBlocks.Count > 0)
                                            {
                                                int choice = random.Next(0, WestBlocks.Count);
                                                this_block = AddMapTile(WestBlocks[choice].Name, "Map_Residential", column, row);
                                            }
                                            else
                                            {
                                                int choice = random.Next(0, ParkBlocks.Count);
                                                this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                            }
                                        }
                                        else
                                        {
                                            int direction = random.Next(1, 3);
                                            if (direction == 1)
                                            {
                                                List<Map> EastBlocks = new List<Map>();
                                                foreach (Map block in Blocks)
                                                {
                                                    if (block.Type == "Residential" &&
                                                        block.Direction == Direction.Right)
                                                    {
                                                        EastBlocks.Add(block);
                                                    }
                                                }

                                                if (EastBlocks.Count > 0)
                                                {
                                                    int choice = random.Next(0, EastBlocks.Count);
                                                    this_block = AddMapTile(EastBlocks[choice].Name, "Map_Residential", column, row);
                                                }
                                                else
                                                {
                                                    int choice = random.Next(0, ParkBlocks.Count);
                                                    this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                                }
                                            }
                                            else if (direction == 2)
                                            {
                                                List<Map> WestBlocks = new List<Map>();
                                                foreach (Map block in Blocks)
                                                {
                                                    if (block.Type == "Residential" &&
                                                        block.Direction == Direction.Left)
                                                    {
                                                        WestBlocks.Add(block);
                                                    }
                                                }

                                                if (WestBlocks.Count > 0)
                                                {
                                                    int choice = random.Next(0, WestBlocks.Count);
                                                    this_block = AddMapTile(WestBlocks[choice].Name, "Map_Residential", column, row);
                                                }
                                                else
                                                {
                                                    int choice = random.Next(0, ParkBlocks.Count);
                                                    this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                                }
                                            }
                                        }
                                    }

                                    #endregion
                                }
                                else if (type > 3)
                                {
                                    #region Commercial

                                    if (y == Handler.MapSize_Y - 2)
                                    {
                                        List<Map> WestBlocks = new List<Map>();
                                        foreach (Map block in Available_Commercial)
                                        {
                                            if (block.Type == "Commercial" &&
                                                block.Direction == Direction.Left)
                                            {
                                                WestBlocks.Add(block);
                                            }
                                        }

                                        if (WestBlocks.Count > 0)
                                        {
                                            int choice = random.Next(0, WestBlocks.Count);
                                            this_block = AddMapTile(WestBlocks[choice].Name, "Map_Commercial", column, row);
                                        }
                                        else
                                        {
                                            int choice = random.Next(0, ParkBlocks.Count);
                                            this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                        }
                                    }
                                    else
                                    {
                                        if (x == Handler.MapSize_X - 3)
                                        {
                                            List<Map> WestBlocks = new List<Map>();
                                            foreach (Map block in Available_Commercial)
                                            {
                                                if (block.Type == "Commercial" &&
                                                    block.Direction == Direction.Left)
                                                {
                                                    WestBlocks.Add(block);
                                                }
                                            }

                                            if (WestBlocks.Count > 0)
                                            {
                                                int choice = random.Next(0, WestBlocks.Count);
                                                this_block = AddMapTile(WestBlocks[choice].Name, "Map_Commercial", column, row);
                                            }
                                            else
                                            {
                                                int choice = random.Next(0, ParkBlocks.Count);
                                                this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                            }
                                        }
                                        else
                                        {
                                            int direction = random.Next(1, 3);
                                            if (direction == 1)
                                            {
                                                List<Map> EastBlocks = new List<Map>();
                                                foreach (Map block in Available_Commercial)
                                                {
                                                    if (block.Type == "Commercial" &&
                                                        block.Direction == Direction.Right)
                                                    {
                                                        EastBlocks.Add(block);
                                                    }
                                                }

                                                if (EastBlocks.Count > 0)
                                                {
                                                    int choice = random.Next(0, EastBlocks.Count);
                                                    this_block = AddMapTile(EastBlocks[choice].Name, "Map_Commercial", column, row);
                                                }
                                                else
                                                {
                                                    int choice = random.Next(0, ParkBlocks.Count);
                                                    this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                                }
                                            }
                                            else if (direction == 2)
                                            {
                                                List<Map> WestBlocks = new List<Map>();
                                                foreach (Map block in Available_Commercial)
                                                {
                                                    if (block.Type == "Commercial" &&
                                                        block.Direction == Direction.Left)
                                                    {
                                                        WestBlocks.Add(block);
                                                    }
                                                }

                                                if (WestBlocks.Count > 0)
                                                {
                                                    int choice = random.Next(0, WestBlocks.Count);
                                                    this_block = AddMapTile(WestBlocks[choice].Name, "Map_Commercial", column, row);
                                                }
                                                else
                                                {
                                                    int choice = random.Next(0, ParkBlocks.Count);
                                                    this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                                }
                                            }
                                        }
                                    }

                                    #endregion
                                }
                            }
                            else
                            {
                                int type = random.Next(1, 11);
                                if (type <= 8)
                                {
                                    #region Residential

                                    if (y == Handler.MapSize_Y - 2)
                                    {
                                        List<Map> WestBlocks = new List<Map>();
                                        foreach (Map block in Blocks)
                                        {
                                            if (block.Type == "Residential" &&
                                                block.Direction == Direction.Left)
                                            {
                                                WestBlocks.Add(block);
                                            }
                                        }

                                        if (WestBlocks.Count > 0)
                                        {
                                            int choice = random.Next(0, WestBlocks.Count);
                                            this_block = AddMapTile(WestBlocks[choice].Name, "Map_Residential", column, row);
                                        }
                                        else
                                        {
                                            int choice = random.Next(0, ParkBlocks.Count);
                                            this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                        }
                                    }
                                    else
                                    {
                                        if (x == Handler.MapSize_X - 3)
                                        {
                                            List<Map> WestBlocks = new List<Map>();
                                            foreach (Map block in Blocks)
                                            {
                                                if (block.Type == "Residential" &&
                                                    block.Direction == Direction.Left)
                                                {
                                                    WestBlocks.Add(block);
                                                }
                                            }

                                            if (WestBlocks.Count > 0)
                                            {
                                                int choice = random.Next(0, WestBlocks.Count);
                                                this_block = AddMapTile(WestBlocks[choice].Name, "Map_Residential", column, row);
                                            }
                                            else
                                            {
                                                int choice = random.Next(0, ParkBlocks.Count);
                                                this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                            }
                                        }
                                        else
                                        {
                                            int direction = random.Next(1, 3);
                                            if (direction == 1)
                                            {
                                                List<Map> EastBlocks = new List<Map>();
                                                foreach (Map block in Blocks)
                                                {
                                                    if (block.Type == "Residential" &&
                                                        block.Direction == Direction.Right)
                                                    {
                                                        EastBlocks.Add(block);
                                                    }
                                                }

                                                if (EastBlocks.Count > 0)
                                                {
                                                    int choice = random.Next(0, EastBlocks.Count);
                                                    this_block = AddMapTile(EastBlocks[choice].Name, "Map_Residential", column, row);
                                                }
                                                else
                                                {
                                                    int choice = random.Next(0, ParkBlocks.Count);
                                                    this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                                }
                                            }
                                            else if (direction == 2)
                                            {
                                                List<Map> WestBlocks = new List<Map>();
                                                foreach (Map block in Blocks)
                                                {
                                                    if (block.Type == "Residential" &&
                                                        block.Direction == Direction.Left)
                                                    {
                                                        WestBlocks.Add(block);
                                                    }
                                                }

                                                if (WestBlocks.Count > 0)
                                                {
                                                    int choice = random.Next(0, WestBlocks.Count);
                                                    this_block = AddMapTile(WestBlocks[choice].Name, "Map_Residential", column, row);
                                                }
                                                else
                                                {
                                                    int choice = random.Next(0, ParkBlocks.Count);
                                                    this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                                }
                                            }
                                        }
                                    }

                                    #endregion
                                }
                                else if (type > 8)
                                {
                                    int choice = random.Next(0, ParkBlocks.Count);
                                    this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                }
                            }
                        }
                        else
                        {
                            if (Available_Commercial.Count > 0)
                            {
                                int type = random.Next(1, 11);
                                if (type <= 3)
                                {
                                    #region Residential

                                    int direction = random.Next(1, 3);
                                    if (direction == 1)
                                    {
                                        List<Map> NorthBlocks = new List<Map>();
                                        foreach (Map block in Blocks)
                                        {
                                            if (block.Type == "Residential" &&
                                                block.Direction == Direction.Up)
                                            {
                                                NorthBlocks.Add(block);
                                            }
                                        }

                                        if (NorthBlocks.Count > 0)
                                        {
                                            int choice = random.Next(0, NorthBlocks.Count);
                                            this_block = AddMapTile(NorthBlocks[choice].Name, "Map_Residential", column, row);
                                        }
                                        else
                                        {
                                            int choice = random.Next(0, ParkBlocks.Count);
                                            this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                        }
                                    }
                                    else if (direction == 2)
                                    {
                                        List<Map> WestBlocks = new List<Map>();
                                        foreach (Map block in Blocks)
                                        {
                                            if (block.Type == "Residential" &&
                                                block.Direction == Direction.Left)
                                            {
                                                WestBlocks.Add(block);
                                            }
                                        }

                                        if (WestBlocks.Count > 0)
                                        {
                                            int choice = random.Next(0, WestBlocks.Count);
                                            this_block = AddMapTile(WestBlocks[choice].Name, "Map_Residential", column, row);
                                        }
                                        else
                                        {
                                            int choice = random.Next(0, ParkBlocks.Count);
                                            this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                        }
                                    }

                                    #endregion
                                }
                                else if (type > 3)
                                {
                                    #region Commercial

                                    int direction = random.Next(1, 3);
                                    if (direction == 1)
                                    {
                                        List<Map> NorthBlocks = new List<Map>();
                                        foreach (Map block in Available_Commercial)
                                        {
                                            if (block.Type == "Commercial" &&
                                                block.Direction == Direction.Up)
                                            {
                                                NorthBlocks.Add(block);
                                            }
                                        }

                                        if (NorthBlocks.Count > 0)
                                        {
                                            int choice = random.Next(0, NorthBlocks.Count);
                                            this_block = AddMapTile(NorthBlocks[choice].Name, "Map_Commercial", column, row);
                                        }
                                        else
                                        {
                                            int choice = random.Next(0, ParkBlocks.Count);
                                            this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                        }
                                    }
                                    else if (direction == 2)
                                    {
                                        List<Map> WestBlocks = new List<Map>();
                                        foreach (Map block in Available_Commercial)
                                        {
                                            if (block.Type == "Commercial" &&
                                                block.Direction == Direction.Left)
                                            {
                                                WestBlocks.Add(block);
                                            }
                                        }

                                        if (WestBlocks.Count > 0)
                                        {
                                            int choice = random.Next(0, WestBlocks.Count);
                                            this_block = AddMapTile(WestBlocks[choice].Name, "Map_Commercial", column, row);
                                        }
                                        else
                                        {
                                            int choice = random.Next(0, ParkBlocks.Count);
                                            this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                        }
                                    }

                                    #endregion
                                }
                            }
                            else
                            {
                                int type = random.Next(1, 11);
                                if (type <= 8)
                                {
                                    #region Residential

                                    int direction = random.Next(1, 3);
                                    if (direction == 1)
                                    {
                                        List<Map> NorthBlocks = new List<Map>();
                                        foreach (Map block in Blocks)
                                        {
                                            if (block.Type == "Residential" &&
                                                block.Direction == Direction.Up)
                                            {
                                                NorthBlocks.Add(block);
                                            }
                                        }

                                        if (NorthBlocks.Count > 0)
                                        {
                                            int choice = random.Next(0, NorthBlocks.Count);
                                            this_block = AddMapTile(NorthBlocks[choice].Name, "Map_Residential", column, row);
                                        }
                                        else
                                        {
                                            int choice = random.Next(0, ParkBlocks.Count);
                                            this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                        }
                                    }
                                    else if (direction == 2)
                                    {
                                        List<Map> WestBlocks = new List<Map>();
                                        foreach (Map block in Blocks)
                                        {
                                            if (block.Type == "Residential" &&
                                                block.Direction == Direction.Left)
                                            {
                                                WestBlocks.Add(block);
                                            }
                                        }

                                        if (WestBlocks.Count > 0)
                                        {
                                            int choice = random.Next(0, WestBlocks.Count);
                                            this_block = AddMapTile(WestBlocks[choice].Name, "Map_Residential", column, row);
                                        }
                                        else
                                        {
                                            int choice = random.Next(0, ParkBlocks.Count);
                                            this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                        }
                                    }

                                    #endregion
                                }
                                else if (type > 8)
                                {
                                    int choice = random.Next(0, ParkBlocks.Count);
                                    this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                }
                            }
                        }
                    }
                }

                #endregion
            }
            else if (previous == "Road_Corner_SW" ||
                     previous == "Road_TSection_N")
            {
                #region Road Coming From West

                if (upper_block.Name == "Road_TSection_W" ||
                    upper_block.Name == "Road_Corner_NE" ||
                    upper_block.Name == "Road_NS")
                {
                    if (y == Handler.MapSize_Y - 2)
                    {
                        this_block = AddMapTile("Road_TSection_W", "Map_Road_TSection_W", column, row);
                    }
                    else
                    {
                        int type = random.Next(1, 4);
                        if (type == 1)
                        {
                            this_block = AddMapTile("Road_Cross", "Map_Road_Cross", column, row);
                        }
                        else if (type == 2)
                        {
                            this_block = AddMapTile("Road_TSection_N", "Map_Road_TSection_N", column, row);
                        }
                        else if (type == 3)
                        {
                            this_block = AddMapTile("Road_TSection_W", "Map_Road_TSection_W", column, row);
                        }
                    }
                }
                else if (upper_block.Name == "Road_Cross" ||
                         upper_block.Name == "Road_TSection_S" ||
                         upper_block.Name == "Road_TSection_E" ||
                         upper_block.Name == "Road_Corner_NW")
                {
                    if (y == Handler.MapSize_Y - 2)
                    {
                        this_block = AddMapTile("Road_TSection_W", "Map_Road_TSection_W", column, row);
                    }
                    else
                    {
                        int type = random.Next(1, 3);
                        if (type == 1)
                        {
                            this_block = AddMapTile("Road_Corner_SE", "Map_Road_Corner_SE", column, row);
                        }
                        else if (type == 2)
                        {
                            this_block = AddMapTile("Road_TSection_W", "Map_Road_TSection_W", column, row);
                        }
                    }
                }
                else
                {
                    this_block = AddMapTile("Road_WE", "Map_Road_WE", column, row);
                }

                #endregion
            }
            else if (previous == "Road_Cross" ||
                     previous == "Road_TSection_E" ||
                     previous == "Road_TSection_S")
            {
                #region Road Coming From West

                if (upper_block.Name == "Road_Cross" ||
                    upper_block.Name == "Road_TSection_S" ||
                    upper_block.Name == "Road_TSection_E" ||
                    upper_block.Name == "Road_Corner_NW")
                {
                    if (y == Handler.MapSize_Y - 2)
                    {
                        this_block = AddMapTile("Road_TSection_W", "Map_Road_TSection_W", column, row);
                    }
                    else
                    {
                        this_block = AddMapTile("Road_Corner_SE", "Map_Road_Corner_SE", column, row);
                    }
                }
                else if (upper_block.Name == "Road_NS" ||
                         upper_block.Name == "Road_TSection_W" ||
                         upper_block.Name == "Road_Corner_NE")
                {
                    if (y == Handler.MapSize_Y - 2)
                    {
                        this_block = AddMapTile("Road_TSection_W", "Map_Road_TSection_W", column, row);
                    }
                    else
                    {
                        this_block = AddMapTile("Road_TSection_N", "Map_Road_TSection_N", column, row);
                    }
                }
                else
                {
                    if (y == Handler.MapSize_Y - 2)
                    {
                        this_block = AddMapTile("Road_Corner_NE", "Map_Road_Corner_NE", column, row);
                    }
                    else
                    {
                        this_block = AddMapTile("Road_WE", "Map_Road_WE", column, row);
                    }
                }

                #endregion
            }
            else if (previous == "Road_Corner_NW")
            {
                #region Road Coming From West

                if (upper_block.Name == "Road_Cross" ||
                    upper_block.Name == "Road_TSection_S" ||
                    upper_block.Name == "Road_TSection_E" ||
                    upper_block.Name == "Road_Corner_NW")
                {
                    if (y == Handler.MapSize_Y - 2)
                    {
                        this_block = AddMapTile("Road_TSection_W", "Map_Road_TSection_W", column, row);
                    }
                    else
                    {
                        this_block = AddMapTile("Road_Corner_SE", "Map_Road_Corner_SE", column, row);
                    }
                }
                else if (upper_block.Name == "Road_NS" ||
                         upper_block.Name == "Road_TSection_W" ||
                         upper_block.Name == "Road_Corner_NE")
                {
                    if (y == Handler.MapSize_Y - 2)
                    {
                        this_block = AddMapTile("Road_TSection_W", "Map_Road_TSection_W", column, row);
                    }
                    else
                    {
                        this_block = AddMapTile("Road_TSection_N", "Map_Road_TSection_N", column, row);
                    }
                }
                else
                {
                    if (y == Handler.MapSize_Y - 2)
                    {
                        this_block = AddMapTile("Road_Corner_NE", "Map_Road_Corner_NE", column, row);
                    }
                    else
                    {
                        this_block = AddMapTile("Road_WE", "Map_Road_WE", column, row);
                    }
                }

                #endregion
            }
            else if (previous == "Road_WE")
            {
                #region Road Coming From West

                if (upper_block.Name == "Road_TSection_W" ||
                    upper_block.Name == "Road_Corner_NE")
                {
                    if (y == Handler.MapSize_Y - 2)
                    {
                        if (x == Handler.MapSize_X - 2)
                        {
                            this_block = AddMapTile("Road_Cross", "Map_Road_Cross", column, row);
                        }
                        else
                        {
                            this_block = AddMapTile("Road_TSection_W", "Map_Road_TSection_W", column, row);
                        }
                    }
                    else
                    {
                        if (x == Handler.MapSize_X - 2)
                        {
                            this_block = AddMapTile("Road_TSection_N", "Map_Road_TSection_N", column, row);
                        }
                        else
                        {
                            int type = random.Next(1, 4);
                            if (type == 1)
                            {
                                this_block = AddMapTile("Road_Cross", "Map_Road_Cross", column, row);
                            }
                            else if (type == 2)
                            {
                                this_block = AddMapTile("Road_TSection_N", "Map_Road_TSection_N", column, row);
                            }
                            else if (type == 3)
                            {
                                this_block = AddMapTile("Road_TSection_W", "Map_Road_TSection_W", column, row);
                            }
                        }
                    }
                }
                else if (upper_block.Name == "Road_Cross" ||
                         upper_block.Name == "Road_TSection_S" ||
                         upper_block.Name == "Road_TSection_E")
                {
                    if (y == Handler.MapSize_Y - 2)
                    {
                        if (x == Handler.MapSize_X - 2)
                        {
                            this_block = AddMapTile("Road_Cross", "Map_Road_Cross", column, row);
                        }
                        else
                        {
                            this_block = AddMapTile("Road_TSection_W", "Map_Road_TSection_W", column, row);
                        }
                    }
                    else
                    {
                        if (x == Handler.MapSize_X - 2)
                        {
                            this_block = AddMapTile("Road_TSection_N", "Map_Road_TSection_N", column, row);
                        }
                        else
                        {
                            this_block = AddMapTile("Road_TSection_W", "Map_Road_TSection_W", column, row);
                        }
                    }
                }
                else if (upper_block.Name == "Road_Corner_NW")
                {
                    if (y == Handler.MapSize_Y - 2)
                    {
                        if (x == Handler.MapSize_X - 2)
                        {
                            this_block = AddMapTile("Road_Cross", "Map_Road_Cross", column, row);
                        }
                        else
                        {
                            this_block = AddMapTile("Road_TSection_W", "Map_Road_TSection_W", column, row);
                        }
                    }
                    else
                    {
                        if (x == Handler.MapSize_X - 2)
                        {
                            this_block = AddMapTile("Road_TSection_N", "Map_Road_TSection_N", column, row);
                        }
                        else
                        {
                            this_block = AddMapTile("Road_TSection_W", "Map_Road_TSection_W", column, row);
                        }
                    }
                }
                else if (upper_block.Name == "Road_NS")
                {
                    if (y == Handler.MapSize_Y - 2)
                    {
                        if (x == Handler.MapSize_X - 2)
                        {
                            this_block = AddMapTile("Road_Cross", "Map_Road_Cross", column, row);
                        }
                        else
                        {
                            this_block = AddMapTile("Road_TSection_W", "Map_Road_TSection_W", column, row);
                        }
                    }
                    else
                    {
                        if (x == Handler.MapSize_X - 2)
                        {
                            this_block = AddMapTile("Road_TSection_N", "Map_Road_TSection_N", column, row);
                        }
                        else
                        {
                            int type = random.Next(1, 7);
                            if (type <= 4)
                            {
                                this_block = AddMapTile("Road_Cross", "Map_Road_Cross", column, row);
                            }
                            else if (type == 5)
                            {
                                this_block = AddMapTile("Road_TSection_N", "Map_Road_TSection_N", column, row);
                            }
                            else if (type == 6)
                            {
                                this_block = AddMapTile("Road_TSection_W", "Map_Road_TSection_W", column, row);
                            }
                        }
                    }
                }
                else
                {
                    if (x == Handler.MapSize_X - 2)
                    {
                        this_block = AddMapTile("Road_WE", "Map_Road_WE", column, row);
                    }
                    else
                    {
                        int type = random.Next(1, 3);
                        if (type == 1)
                        {
                            this_block = AddMapTile("Road_TSection_S", "Map_Road_TSection_S", column, row);
                        }
                        else if (type == 2)
                        {
                            this_block = AddMapTile("Road_WE", "Map_Road_WE", column, row);
                        }
                    }
                }

                #endregion
            }
            else
            {
                #region West Block Not Road

                Direction previous_direction = Direction.Nowhere;
                Map previous_block = GetPreviousBlock(previous);
                if (previous_block != null)
                {
                    previous_direction = previous_block.Direction;
                }

                if (previous_direction == Direction.Up ||
                    previous_direction == Direction.Left ||
                    previous_direction == Direction.Down ||
                    previous_direction == Direction.Nowhere)
                {
                    #region West Block Not Facing Current

                    if (upper_block.Name == "Road_Cross" ||
                        upper_block.Name == "Road_TSection_S" ||
                        upper_block.Name == "Road_TSection_W" ||
                        upper_block.Name == "Road_TSection_E" ||
                        upper_block.Name == "Road_Corner_NW" ||
                        upper_block.Name == "Road_Corner_NE")
                    {
                        this_block = AddMapTile("Road_NS", "Map_Road_NS", column, row);
                    }
                    else if (upper_block.Name == "Road_NS")
                    {
                        if (y == Handler.MapSize_Y - 2)
                        {
                            this_block = AddMapTile("Road_NS", "Map_Road_NS", column, row);
                        }
                        else
                        {
                            int type = random.Next(1, 3);
                            if (type == 1)
                            {
                                this_block = AddMapTile("Road_NS", "Map_Road_NS", column, row);
                            }
                            else if (type == 2)
                            {
                                this_block = AddMapTile("Road_TSection_E", "Map_Road_TSection_E", column, row);
                            }
                        }
                    }
                    else
                    {
                        bool BlockAbove = GetBlockAbove(upper_block) != null;
                        if (BlockAbove)
                        {
                            bool BlockAbove_South = false;
                            Map blockAbove = GetBlockAbove(upper_block);
                            if (blockAbove != null)
                            {
                                if (blockAbove.Type != "Park" &&
                                    blockAbove.Direction == Direction.Down)
                                {
                                    BlockAbove_South = true;
                                }
                            }

                            if (BlockAbove_South)
                            {
                                this_block = AddMapTile("Road_Corner_NW", "Map_Road_Corner_NW", column, row);
                            }
                            else
                            {
                                if (x == Handler.MapSize_X - 2)
                                {
                                    int choice = random.Next(0, ParkBlocks.Count);
                                    this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                }
                                else
                                {
                                    if (Available_Commercial.Count > 0)
                                    {
                                        int type = random.Next(1, 11);
                                        if (type <= 3)
                                        {
                                            #region Residential

                                            if (y == Handler.MapSize_Y - 3)
                                            {
                                                if (x == Handler.MapSize_X - 3)
                                                {
                                                    int choice = random.Next(0, ParkBlocks.Count);
                                                    this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                                }
                                                else
                                                {
                                                    List<Map> EastBlocks = new List<Map>();
                                                    foreach (Map block in Blocks)
                                                    {
                                                        if (block.Type == "Residential" &&
                                                            block.Direction == Direction.Right)
                                                        {
                                                            EastBlocks.Add(block);
                                                        }
                                                    }

                                                    if (EastBlocks.Count > 0)
                                                    {
                                                        int choice = random.Next(0, EastBlocks.Count);
                                                        this_block = AddMapTile(EastBlocks[choice].Name, "Map_Residential", column, row);
                                                    }
                                                    else
                                                    {
                                                        int choice = random.Next(0, ParkBlocks.Count);
                                                        this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                                    }
                                                }
                                            }
                                            else if (y == Handler.MapSize_Y - 2)
                                            {
                                                List<Map> SouthBlocks = new List<Map>();
                                                foreach (Map block in Blocks)
                                                {
                                                    if (block.Type == "Residential" &&
                                                        block.Direction == Direction.Down)
                                                    {
                                                        SouthBlocks.Add(block);
                                                    }
                                                }

                                                if (SouthBlocks.Count > 0)
                                                {
                                                    int choice = random.Next(0, SouthBlocks.Count);
                                                    this_block = AddMapTile(SouthBlocks[choice].Name, "Map_Residential", column, row);
                                                }
                                                else
                                                {
                                                    int choice = random.Next(0, ParkBlocks.Count);
                                                    this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                                }
                                            }
                                            else
                                            {
                                                if (x == Handler.MapSize_X - 3)
                                                {
                                                    List<Map> SouthBlocks = new List<Map>();
                                                    foreach (Map block in Blocks)
                                                    {
                                                        if (block.Type == "Residential" &&
                                                            block.Direction == Direction.Down)
                                                        {
                                                            SouthBlocks.Add(block);
                                                        }
                                                    }

                                                    if (SouthBlocks.Count > 0)
                                                    {
                                                        int choice = random.Next(0, SouthBlocks.Count);
                                                        this_block = AddMapTile(SouthBlocks[choice].Name, "Map_Residential", column, row);
                                                    }
                                                    else
                                                    {
                                                        int choice = random.Next(0, ParkBlocks.Count);
                                                        this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                                    }
                                                }
                                                else
                                                {
                                                    int direction = random.Next(1, 3);
                                                    if (direction == 1)
                                                    {
                                                        List<Map> EastBlocks = new List<Map>();
                                                        foreach (Map block in Blocks)
                                                        {
                                                            if (block.Type == "Residential" &&
                                                                block.Direction == Direction.Right)
                                                            {
                                                                EastBlocks.Add(block);
                                                            }
                                                        }

                                                        if (EastBlocks.Count > 0)
                                                        {
                                                            int choice = random.Next(0, EastBlocks.Count);
                                                            this_block = AddMapTile(EastBlocks[choice].Name, "Map_Residential", column, row);
                                                        }
                                                        else
                                                        {
                                                            int choice = random.Next(0, ParkBlocks.Count);
                                                            this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                                        }
                                                    }
                                                    else if (direction == 2)
                                                    {
                                                        List<Map> SouthBlocks = new List<Map>();
                                                        foreach (Map block in Blocks)
                                                        {
                                                            if (block.Type == "Residential" &&
                                                                block.Direction == Direction.Down)
                                                            {
                                                                SouthBlocks.Add(block);
                                                            }
                                                        }

                                                        if (SouthBlocks.Count > 0)
                                                        {
                                                            int choice = random.Next(0, SouthBlocks.Count);
                                                            this_block = AddMapTile(SouthBlocks[choice].Name, "Map_Residential", column, row);
                                                        }
                                                        else
                                                        {
                                                            int choice = random.Next(0, ParkBlocks.Count);
                                                            this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                                        }
                                                    }
                                                }
                                            }

                                            #endregion
                                        }
                                        else if (type >= 4 && type <= 8)
                                        {
                                            #region Commercial

                                            if (y == Handler.MapSize_Y - 3)
                                            {
                                                if (x == Handler.MapSize_X - 3)
                                                {
                                                    int choice = random.Next(0, ParkBlocks.Count);
                                                    this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                                }
                                                else
                                                {
                                                    List<Map> EastBlocks = new List<Map>();
                                                    foreach (Map block in Available_Commercial)
                                                    {
                                                        if (block.Type == "Commercial" &&
                                                            block.Direction == Direction.Right)
                                                        {
                                                            EastBlocks.Add(block);
                                                        }
                                                    }

                                                    if (EastBlocks.Count > 0)
                                                    {
                                                        int choice = random.Next(0, EastBlocks.Count);
                                                        this_block = AddMapTile(EastBlocks[choice].Name, "Map_Commercial", column, row);
                                                    }
                                                    else
                                                    {
                                                        int choice = random.Next(0, ParkBlocks.Count);
                                                        this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                                    }
                                                }
                                            }
                                            else if (y == Handler.MapSize_Y - 2)
                                            {
                                                List<Map> SouthBlocks = new List<Map>();
                                                foreach (Map block in Available_Commercial)
                                                {
                                                    if (block.Type == "Commercial" &&
                                                        block.Direction == Direction.Down)
                                                    {
                                                        SouthBlocks.Add(block);
                                                    }
                                                }

                                                if (SouthBlocks.Count > 0)
                                                {
                                                    int choice = random.Next(0, SouthBlocks.Count);
                                                    this_block = AddMapTile(SouthBlocks[choice].Name, "Map_Commercial", column, row);
                                                }
                                                else
                                                {
                                                    int choice = random.Next(0, ParkBlocks.Count);
                                                    this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                                }
                                            }
                                            else
                                            {
                                                if (x == Handler.MapSize_X - 3)
                                                {
                                                    List<Map> SouthBlocks = new List<Map>();
                                                    foreach (Map block in Available_Commercial)
                                                    {
                                                        if (block.Type == "Commercial" &&
                                                            block.Direction == Direction.Down)
                                                        {
                                                            SouthBlocks.Add(block);
                                                        }
                                                    }

                                                    if (SouthBlocks.Count > 0)
                                                    {
                                                        int choice = random.Next(0, SouthBlocks.Count);
                                                        this_block = AddMapTile(SouthBlocks[choice].Name, "Map_Commercial", column, row);
                                                    }
                                                    else
                                                    {
                                                        int choice = random.Next(0, ParkBlocks.Count);
                                                        this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                                    }
                                                }
                                                else
                                                {
                                                    int direction = random.Next(1, 3);
                                                    if (direction == 1)
                                                    {
                                                        List<Map> EastBlocks = new List<Map>();
                                                        foreach (Map block in Available_Commercial)
                                                        {
                                                            if (block.Type == "Commercial" &&
                                                                block.Direction == Direction.Right)
                                                            {
                                                                EastBlocks.Add(block);
                                                            }
                                                        }

                                                        if (EastBlocks.Count > 0)
                                                        {
                                                            int choice = random.Next(0, EastBlocks.Count);
                                                            this_block = AddMapTile(EastBlocks[choice].Name, "Map_Commercial", column, row);
                                                        }
                                                        else
                                                        {
                                                            int choice = random.Next(0, ParkBlocks.Count);
                                                            this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                                        }
                                                    }
                                                    else if (direction == 2)
                                                    {
                                                        List<Map> SouthBlocks = new List<Map>();
                                                        foreach (Map block in Available_Commercial)
                                                        {
                                                            if (block.Type == "Commercial" &&
                                                                block.Direction == Direction.Down)
                                                            {
                                                                SouthBlocks.Add(block);
                                                            }
                                                        }

                                                        if (SouthBlocks.Count > 0)
                                                        {
                                                            int choice = random.Next(0, SouthBlocks.Count);
                                                            this_block = AddMapTile(SouthBlocks[choice].Name, "Map_Commercial", column, row);
                                                        }
                                                        else
                                                        {
                                                            int choice = random.Next(0, ParkBlocks.Count);
                                                            this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                                        }
                                                    }
                                                }
                                            }

                                            #endregion
                                        }
                                        else if (type >= 9)
                                        {
                                            int choice = random.Next(0, ParkBlocks.Count);
                                            this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                        }
                                    }
                                    else
                                    {
                                        int type = random.Next(1, 11);
                                        if (type <= 8)
                                        {
                                            #region Residential

                                            if (y == Handler.MapSize_Y - 3)
                                            {
                                                if (x == Handler.MapSize_X - 3)
                                                {
                                                    int choice = random.Next(0, ParkBlocks.Count);
                                                    this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                                }
                                                else
                                                {
                                                    List<Map> EastBlocks = new List<Map>();
                                                    foreach (Map block in Blocks)
                                                    {
                                                        if (block.Type == "Residential" &&
                                                            block.Direction == Direction.Right)
                                                        {
                                                            EastBlocks.Add(block);
                                                        }
                                                    }

                                                    if (EastBlocks.Count > 0)
                                                    {
                                                        int choice = random.Next(0, EastBlocks.Count);
                                                        this_block = AddMapTile(EastBlocks[choice].Name, "Map_Residential", column, row);
                                                    }
                                                    else
                                                    {
                                                        int choice = random.Next(0, ParkBlocks.Count);
                                                        this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                                    }
                                                }
                                            }
                                            else if (y == Handler.MapSize_Y - 2)
                                            {
                                                List<Map> SouthBlocks = new List<Map>();
                                                foreach (Map block in Blocks)
                                                {
                                                    if (block.Type == "Residential" &&
                                                        block.Direction == Direction.Down)
                                                    {
                                                        SouthBlocks.Add(block);
                                                    }
                                                }

                                                if (SouthBlocks.Count > 0)
                                                {
                                                    int choice = random.Next(0, SouthBlocks.Count);
                                                    this_block = AddMapTile(SouthBlocks[choice].Name, "Map_Residential", column, row);
                                                }
                                                else
                                                {
                                                    int choice = random.Next(0, ParkBlocks.Count);
                                                    this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                                }
                                            }
                                            else
                                            {
                                                if (x == Handler.MapSize_X - 3)
                                                {
                                                    List<Map> SouthBlocks = new List<Map>();
                                                    foreach (Map block in Blocks)
                                                    {
                                                        if (block.Type == "Residential" &&
                                                            block.Direction == Direction.Down)
                                                        {
                                                            SouthBlocks.Add(block);
                                                        }
                                                    }

                                                    if (SouthBlocks.Count > 0)
                                                    {
                                                        int choice = random.Next(0, SouthBlocks.Count);
                                                        this_block = AddMapTile(SouthBlocks[choice].Name, "Map_Residential", column, row);
                                                    }
                                                    else
                                                    {
                                                        int choice = random.Next(0, ParkBlocks.Count);
                                                        this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                                    }
                                                }
                                                else
                                                {
                                                    int direction = random.Next(1, 3);
                                                    if (direction == 1)
                                                    {
                                                        List<Map> EastBlocks = new List<Map>();
                                                        foreach (Map block in Blocks)
                                                        {
                                                            if (block.Type == "Residential" &&
                                                                block.Direction == Direction.Right)
                                                            {
                                                                EastBlocks.Add(block);
                                                            }
                                                        }

                                                        if (EastBlocks.Count > 0)
                                                        {
                                                            int choice = random.Next(0, EastBlocks.Count);
                                                            this_block = AddMapTile(EastBlocks[choice].Name, "Map_Residential", column, row);
                                                        }
                                                        else
                                                        {
                                                            int choice = random.Next(0, ParkBlocks.Count);
                                                            this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                                        }
                                                    }
                                                    else if (direction == 2)
                                                    {
                                                        List<Map> SouthBlocks = new List<Map>();
                                                        foreach (Map block in Blocks)
                                                        {
                                                            if (block.Type == "Residential" &&
                                                                block.Direction == Direction.Down)
                                                            {
                                                                SouthBlocks.Add(block);
                                                            }
                                                        }

                                                        if (SouthBlocks.Count > 0)
                                                        {
                                                            int choice = random.Next(0, SouthBlocks.Count);
                                                            this_block = AddMapTile(SouthBlocks[choice].Name, "Map_Residential", column, row);
                                                        }
                                                        else
                                                        {
                                                            int choice = random.Next(0, ParkBlocks.Count);
                                                            this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                                        }
                                                    }
                                                }
                                            }

                                            #endregion
                                        }
                                        else if (type >= 9)
                                        {
                                            int choice = random.Next(0, ParkBlocks.Count);
                                            this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            #region Road_WE Above

                            if (Available_Commercial.Count > 0)
                            {
                                int type = random.Next(1, 11);
                                if (type <= 3)
                                {
                                    #region Residential

                                    List<Map> NorthBlocks = new List<Map>();
                                    foreach (Map block in Blocks)
                                    {
                                        if (block.Type == "Residential" &&
                                            block.Direction == Direction.Up)
                                        {
                                            NorthBlocks.Add(block);
                                        }
                                    }

                                    if (NorthBlocks.Count > 0)
                                    {
                                        int choice = random.Next(0, NorthBlocks.Count);
                                        this_block = AddMapTile(NorthBlocks[choice].Name, "Map_Residential", column, row);
                                    }
                                    else
                                    {
                                        int choice = random.Next(0, ParkBlocks.Count);
                                        this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                    }

                                    #endregion
                                }
                                else if (type >= 4 && type <= 8)
                                {
                                    #region Commercial

                                    List<Map> NorthBlocks = new List<Map>();
                                    foreach (Map block in Available_Commercial)
                                    {
                                        if (block.Type == "Commercial" &&
                                            block.Direction == Direction.Up)
                                        {
                                            NorthBlocks.Add(block);
                                        }
                                    }

                                    if (NorthBlocks.Count > 0)
                                    {
                                        int choice = random.Next(0, NorthBlocks.Count);
                                        this_block = AddMapTile(NorthBlocks[choice].Name, "Map_Commercial", column, row);
                                    }
                                    else
                                    {
                                        int choice = random.Next(0, ParkBlocks.Count);
                                        this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                    }

                                    #endregion
                                }
                                else if (type >= 9)
                                {
                                    int choice = random.Next(0, ParkBlocks.Count);
                                    this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                }
                            }
                            else
                            {
                                int type = random.Next(1, 11);
                                if (type <= 8)
                                {
                                    #region Residential

                                    List<Map> NorthBlocks = new List<Map>();
                                    foreach (Map block in Blocks)
                                    {
                                        if (block.Type == "Residential" &&
                                            block.Direction == Direction.Up)
                                        {
                                            NorthBlocks.Add(block);
                                        }
                                    }

                                    if (NorthBlocks.Count > 0)
                                    {
                                        int choice = random.Next(0, NorthBlocks.Count);
                                        this_block = AddMapTile(NorthBlocks[choice].Name, "Map_Residential", column, row);
                                    }
                                    else
                                    {
                                        int choice = random.Next(0, ParkBlocks.Count);
                                        this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                    }

                                    #endregion
                                }
                                else if (type >= 9)
                                {
                                    int choice = random.Next(0, ParkBlocks.Count);
                                    this_block = AddMapTile(ParkBlocks[choice].Name, "Map_Park", column, row);
                                }
                            }

                            #endregion
                        }
                    }

                    #endregion
                }
                else if (previous_direction == Direction.Right)
                {
                    #region West Block Facing Current

                    if (upper_block.Name == "Road_Cross" ||
                        upper_block.Name == "Road_TSection_S" ||
                        upper_block.Name == "Road_TSection_W" ||
                        upper_block.Name == "Road_TSection_E" ||
                        upper_block.Name == "Road_Corner_NW" ||
                        upper_block.Name == "Road_Corner_NE" ||
                        upper_block.Name == "Road_NS")
                    {
                        this_block = AddMapTile("Road_NS", "Map_Road_NS", column, row);
                    }
                    else
                    {
                        this_block = AddMapTile("Road_Corner_NW", "Map_Road_Corner_NW", column, row);
                    }

                    #endregion
                }

                #endregion
            }

            return this_block;
        }

        private static string AddToMap_LastRow(int x)
        {
            string this_block = "";

            column++;
            Tile upper_block = Worldmap[x + (Handler.MapSize_X * (row - 1))];

            if (upper_block.Name == "Road_Cross" ||
                upper_block.Name == "Road_TSection_S" ||
                upper_block.Name == "Road_TSection_W" ||
                upper_block.Name == "Road_TSection_E" ||
                upper_block.Name == "Road_Corner_NW" ||
                upper_block.Name == "Road_Corner_NE" ||
                upper_block.Name == "Road_NS")
            {
                this_block = AddMapTile("Road_TSection_N", "Map_Road_TSection_N", column, row);
            }
            else
            {
                this_block = AddMapTile("Road_WE", "Map_Road_WE", column, row);
            }

            return this_block;
        }

        private static string AddMapTile(string name, string type, int column, int row)
        {
            Tile tile = new Tile();
            tile.ID = Handler.GetID();
            tile.Name = name;
            tile.Type = type;
            tile.Texture = AssetManager.Textures[type];
            tile.Image = new Rectangle(0, 0, tile.Texture.Width, tile.Texture.Height);
            tile.Location = new Vector3(column, row, 0);
            tile.Region = new Region(column * Main.Game.TileSize.X, row * Main.Game.TileSize.Y, Main.Game.TileSize.X, Main.Game.TileSize.Y);
            tile.Visible = true;
            Worldmap.Add(tile);

            Map map = new Map();
            map.ID = tile.ID;
            map.Name = name;
            map.Location = new Vector3(column, row, 0);

            if (type.Contains("Residential"))
            {
                Residential.Add(map);
            }
            else if (type.Contains("Commercial"))
            {
                Commercial.Add(map);

                if (name.Contains("Diner"))
                {
                    for (int a = 0; a < Available_Commercial.Count; a++)
                    {
                        Map block = Available_Commercial[a];
                        if (block.Name.Contains("Diner"))
                        {
                            Available_Commercial.Remove(block);
                            a--;
                        }
                    }
                }
                else if (name.Contains("Grocery"))
                {
                    for (int a = 0; a < Available_Commercial.Count; a++)
                    {
                        Map block = Available_Commercial[a];
                        if (block.Name.Contains("Grocery"))
                        {
                            Available_Commercial.Remove(block);
                            a--;
                        }
                    }
                }
                else if (name.Contains("Police"))
                {
                    for (int a = 0; a < Available_Commercial.Count; a++)
                    {
                        Map block = Available_Commercial[a];
                        if (block.Name.Contains("Police"))
                        {
                            Available_Commercial.Remove(block);
                            a--;
                        }
                    }
                }
            }
            else if (type.Contains("Park"))
            {
                Parks.Add(map);
            }
            else if (type.Contains("Road"))
            {
                Roads.Add(map);
            }

            Layer bottom_tiles = NewLayer(map, "BottomTiles");
            map.Layers.Add(bottom_tiles);

            Layer room_tiles = NewLayer(map, "RoomTiles");
            map.Layers.Add(room_tiles);

            Layer middle_tiles = NewLayer(map, "MiddleTiles");
            map.Layers.Add(middle_tiles);

            Layer top_tiles = NewLayer(map, "TopTiles");
            map.Layers.Add(top_tiles);

            current++;
            Handler.Loading_Percent = (current * 100) / total;

            return name;
        }

        private static Map GetBlockAbove(Tile upper_block)
        {
            foreach (Map block in Blocks)
            {
                if ((block.Type == "Residential" ||
                    block.Type == "Commercial" ||
                    block.Type == "Park") &&
                    block.Name == upper_block.Name)
                {
                    return block;
                }
            }

            return null;
        }

        private static Map GetPreviousBlock(string previous)
        {
            foreach (Map block in Blocks)
            {
                if ((block.Type == "Residential" ||
                    block.Type == "Commercial" ||
                    block.Type == "Park") &&
                    block.Name == previous)
                {
                    return block;
                }
            }

            return null;
        }

        #endregion

        public static void GenTown()
        {
            Handler.light_sources.Clear();

            Handler.Loading_Percent = 0;
            Handler.Loading_Message = "Generating new town...";

            World world = new World();
            world.ID = Handler.GetID();
            world.Visible = true;

            Map map = NewMap(world, "Despicaville");
            world.Maps.Add(map);

            Layer bottom_tiles = NewLayer(map, "BottomTiles");
            map.Layers.Add(bottom_tiles);

            Layer room_tiles = NewLayer(map, "RoomTiles");
            map.Layers.Add(room_tiles);

            Layer middle_tiles = NewLayer(map, "MiddleTiles");
            map.Layers.Add(middle_tiles);

            Layer top_tiles = NewLayer(map, "TopTiles");
            map.Layers.Add(top_tiles);

            Layer effect_tiles = NewLayer(map, "EffectTiles");
            map.Layers.Add(effect_tiles);

            Layer roof_tiles = NewLayer(map, "RoofTiles");
            map.Layers.Add(roof_tiles);

            total = 0;
            current = 0;

            total += (bottom_tiles.Columns * bottom_tiles.Rows) * 6;

            AddNewTiles(map, bottom_tiles);
            AssignOwners(middle_tiles);
            UpdateMiddleTiles(middle_tiles);
            AssignOwners(top_tiles);
            AddRoofTiles(bottom_tiles, middle_tiles, room_tiles, roof_tiles);
            AddEffectTiles(bottom_tiles, effect_tiles);

            SceneManager.GetScene("Gameplay").World = world;
        }

        private static void AddNewTiles(Map map, Layer bottom_tiles)
        {
            for (int y = 0; y < bottom_tiles.Rows; y++)
            {
                for (int x = 0; x < bottom_tiles.Columns; x++)
                {
                    Tile worldTile = null;
                    foreach (Tile existing in Worldmap)
                    {
                        if (existing.Location.X == x / 20 &&
                            existing.Location.Y == y / 20)
                        {
                            worldTile = existing;
                            break;
                        }
                    }

                    if (worldTile != null)
                    {
                        Map block = null;
                        foreach (Map existing in Blocks)
                        {
                            if (existing.Name == worldTile.Name)
                            {
                                block = existing;
                                break;
                            }
                        }

                        if (block != null)
                        {
                            CryptoRandom random = new CryptoRandom();
                            int choice = random.Next(0, CharacterManager.LastNames.Count);
                            last_name = CharacterManager.LastNames[choice];

                            Layer block_bottom_tiles = block.GetLayer("BottomTiles");
                            if (block_bottom_tiles != null)
                            {
                                foreach (Tile tile in block_bottom_tiles.Tiles)
                                {
                                    int tile_x = (int)(((tile.Location.X / 32) - 5) + (worldTile.Location.X * 20));
                                    int tile_y = (int)(((tile.Location.Y - 80) / 32) + (worldTile.Location.Y * 20));

                                    if (tile_x == x &&
                                        tile_y == y)
                                    {
                                        AddTile(map, bottom_tiles, worldTile, tile);

                                        current++;
                                        Handler.Loading_Percent = (current * 100) / total;
                                        break;
                                    }
                                }
                            }

                            bool found = false;

                            Layer middle_tiles = map.GetLayer("MiddleTiles");
                            if (middle_tiles != null)
                            {
                                Layer block_middle_tiles = block.GetLayer("MiddleTiles");
                                if (block_middle_tiles != null)
                                {
                                    foreach (Tile tile in block_middle_tiles.Tiles)
                                    {
                                        int tile_x = (int)(((tile.Location.X / 32) - 5) + (worldTile.Location.X * 20));
                                        int tile_y = (int)(((tile.Location.Y - 80) / 32) + (worldTile.Location.Y * 20));

                                        if (tile_x == x &&
                                            tile_y == y)
                                        {
                                            found = true;

                                            AddTile(map, middle_tiles, worldTile, tile);

                                            current++;
                                            Handler.Loading_Percent = (current * 100) / total;
                                            break;
                                        }
                                    }
                                }

                                if (!found)
                                {
                                    Tile middle_tile = new Tile();
                                    middle_tile.ID = Handler.GetID();
                                    middle_tile.MapID = map.ID;
                                    middle_tile.LayerID = middle_tiles.ID;
                                    middle_tile.Name = "";
                                    middle_tile.Location = new Vector3(x, y, 0);
                                    middle_tile.Region = new Region((int)middle_tile.Location.X * Main.Game.TileSize.X, (int)middle_tile.Location.Y * Main.Game.TileSize.Y, Main.Game.TileSize.X, Main.Game.TileSize.Y);
                                    middle_tiles.Tiles.Add(middle_tile);

                                    current++;
                                    Handler.Loading_Percent = (current * 100) / total;
                                }
                            }

                            found = false;

                            Layer top_tiles = map.GetLayer("TopTiles");
                            if (top_tiles != null)
                            {
                                Layer block_top_tiles = block.GetLayer("TopTiles");
                                if (block_top_tiles != null)
                                {
                                    foreach (Tile tile in block_top_tiles.Tiles)
                                    {
                                        int tile_x = (int)(((tile.Location.X / 32) - 5) + (worldTile.Location.X * 20));
                                        int tile_y = (int)(((tile.Location.Y - 80) / 32) + (worldTile.Location.Y * 20));

                                        if (tile_x == x &&
                                            tile_y == y)
                                        {
                                            found = true;

                                            AddTile(map, top_tiles, worldTile, tile);

                                            current++;
                                            Handler.Loading_Percent = (current * 100) / total;
                                            break;
                                        }
                                    }
                                }

                                if (!found)
                                {
                                    Tile top_tile = new Tile();
                                    top_tile.ID = Handler.GetID();
                                    top_tile.MapID = map.ID;
                                    top_tile.LayerID = top_tiles.ID;
                                    top_tile.Name = "";
                                    top_tile.Location = new Vector3(x, y, 0);
                                    top_tile.Region = new Region((int)top_tile.Location.X * Main.Game.TileSize.X, (int)top_tile.Location.Y * Main.Game.TileSize.Y, Main.Game.TileSize.X, Main.Game.TileSize.Y);
                                    top_tiles.Tiles.Add(top_tile);

                                    current++;
                                    Handler.Loading_Percent = (current * 100) / total;
                                }
                            }

                            found = false;

                            Layer room_tiles = map.GetLayer("RoomTiles");
                            if (room_tiles != null)
                            {
                                Layer block_room_tiles = block.GetLayer("RoomTiles");
                                if (block_room_tiles != null)
                                {
                                    foreach (Tile tile in block_room_tiles.Tiles)
                                    {
                                        int tile_x = (int)(((tile.Location.X / 32) - 5) + (worldTile.Location.X * 20));
                                        int tile_y = (int)(((tile.Location.Y - 80) / 32) + (worldTile.Location.Y * 20));

                                        if (tile_x == x &&
                                            tile_y == y)
                                        {
                                            found = true;

                                            AddTile(map, room_tiles, worldTile, tile);

                                            current++;
                                            Handler.Loading_Percent = (current * 100) / total;
                                            break;
                                        }
                                    }
                                }

                                if (!found)
                                {
                                    Tile room_tile = new Tile();
                                    room_tile.ID = Handler.GetID();
                                    room_tile.MapID = map.ID;
                                    room_tile.LayerID = room_tiles.ID;
                                    room_tile.Name = "";
                                    room_tile.Location = new Vector3(x, y, 0);
                                    room_tile.Region = new Region((int)room_tile.Location.X * Main.Game.TileSize.X, (int)room_tile.Location.Y * Main.Game.TileSize.Y, Main.Game.TileSize.X, Main.Game.TileSize.Y);
                                    room_tiles.Tiles.Add(room_tile);

                                    current++;
                                    Handler.Loading_Percent = (current * 100) / total;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void UpdateMiddleTiles(Layer middle_tiles)
        {
            for (int i = middle_tiles.Tiles.Count - 1; i >= 0; i--)
            {
                Tile middle_tile = middle_tiles.Tiles[i];
                if (!string.IsNullOrEmpty(middle_tile.Name))
                {
                    if (middle_tile.Name.Contains("Tree"))
                    {
                        int x = (int)middle_tile.Location.X + 1;
                        int y = (int)middle_tile.Location.Y + 1;

                        int index = (y * middle_tiles.Columns) + x;
                        Tile existing = middle_tiles.Tiles[index];
                        if (existing != null)
                        {
                            //Swamp tree with tile at X+1/Y+1
                            middle_tiles.Tiles[index] = middle_tile;
                            middle_tiles.Tiles[i] = existing;

                            existing.Location.X--;
                            existing.Location.Y--;

                            middle_tile.Location.X++;
                            middle_tile.Location.Y++;
                        }
                    }
                }
            }
        }

        private static void AddRoofTiles(Layer bottom_tiles, Layer middle_tiles, Layer room_tiles, Layer roof_tiles)
        {
            foreach (Tile bottom_tile in bottom_tiles.Tiles)
            {
                Tile roof_tile = new Tile();
                roof_tile.Location = new Vector3(bottom_tile.Location.X, bottom_tile.Location.Y, 0);
                roof_tile.Region = new Region((int)roof_tile.Location.X * Main.Game.TileSize.X, (int)roof_tile.Location.Y * Main.Game.TileSize.Y, Main.Game.TileSize.X, Main.Game.TileSize.Y);
                roof_tiles.Tiles.Add(roof_tile);

                if (bottom_tile.Name.Contains("Wall"))
                {
                    roof_tile.Texture = AssetManager.Textures["Roof"];
                    roof_tile.Image = new Rectangle(0, 0, roof_tile.Texture.Width, roof_tile.Texture.Height);
                    roof_tile.Visible = true;
                }

                current++;
                Handler.Loading_Percent = (current * 100) / total;
            }

            foreach (Tile middle_tile in middle_tiles.Tiles)
            {
                Tile roof_tile = roof_tiles.GetTile(new Vector2(middle_tile.Location.X, middle_tile.Location.Y));
                if (roof_tile != null)
                {
                    if (roof_tile.Texture == null)
                    {
                        if ((middle_tile.Name.Contains("Door") && !WorldUtil.NextToFence(middle_tiles, middle_tile.Location)) ||
                             middle_tile.Name.Contains("Window"))
                        {
                            roof_tile.Texture = AssetManager.Textures["Roof"];
                            roof_tile.Image = new Rectangle(0, 0, roof_tile.Texture.Width, roof_tile.Texture.Height);
                            roof_tile.Visible = true;
                        }
                    }
                }
            }

            foreach (Tile room_tile in room_tiles.Tiles)
            {
                Tile roof_tile = roof_tiles.GetTile(new Vector2(room_tile.Location.X, room_tile.Location.Y));
                if (roof_tile != null)
                {
                    if (roof_tile.Texture == null)
                    {
                        if (room_tile.Name == "RoomType_Bathroom" ||
                            room_tile.Name == "RoomType_Bedroom" ||
                            room_tile.Name == "RoomType_GroceryStore" ||
                            room_tile.Name == "RoomType_Hallway" ||
                            room_tile.Name == "RoomType_Kitchen" ||
                            room_tile.Name == "RoomType_Lounge" ||
                            room_tile.Name == "RoomType_Office" ||
                            room_tile.Name == "RoomType_StoreCounter" ||
                            room_tile.Name == "RoomType_WeaponStore")
                        {
                            roof_tile.Texture = AssetManager.Textures["Roof"];
                            roof_tile.Image = new Rectangle(0, 0, roof_tile.Texture.Width, roof_tile.Texture.Height);
                            roof_tile.Visible = true;
                        }
                    }
                }
            }
        }

        private static void AddEffectTiles(Layer bottom_tiles, Layer effect_tiles)
        {
            foreach (Tile bottom_tile in bottom_tiles.Tiles)
            {
                Tile effect_tile = new Tile();
                effect_tile.Location = new Vector3(bottom_tile.Location.X, bottom_tile.Location.Y, 0);
                effect_tile.Region = new Region((int)effect_tile.Location.X * Main.Game.TileSize.X, (int)effect_tile.Location.Y * Main.Game.TileSize.Y, Main.Game.TileSize.X, Main.Game.TileSize.Y);
                effect_tile.Visible = false;
                effect_tiles.Tiles.Add(effect_tile);

                current++;
                Handler.Loading_Percent = (current * 100) / total;
            }
        }

        #region GenTown

        public static Map NewMap(World world, string name)
        {
            Map map = new Map();
            map.ID = Handler.GetID();
            map.WorldID = world.ID;
            map.Visible = true;
            map.DrawColor = Color.White;
            map.Name = name;
            return map;
        }

        public static Layer NewLayer(Map map, string name)
        {
            Layer layer = new Layer();
            layer.ID = Handler.GetID();
            layer.WorldID = map.WorldID;
            layer.MapID = map.ID;
            layer.Visible = true;
            layer.DrawColor = Color.White;
            layer.Name = name;
            layer.Rows = Handler.MapSize_Y * 20;
            layer.Columns = Handler.MapSize_X * 20;
            return layer;
        }

        public static void AddTile(Map map, Layer layer, Tile worldTile, Tile tile)
        {
            int x = (int)(((tile.Location.X / 32) - 5) + (worldTile.Location.X * 20));
            int y = (int)(((tile.Location.Y - 80) / 32) + (worldTile.Location.Y * 20));

            Tile new_tile = new Tile();
            new_tile.ID = Handler.GetID();
            new_tile.MapID = map.ID;
            new_tile.LayerID = layer.ID;
            new_tile.Name = tile.Name;
            new_tile.Type = tile.Type;
            new_tile.Texture = AssetManager.Textures[tile.Texture.Name];
            new_tile.Image = new Rectangle(0, 0, new_tile.Texture.Width, new_tile.Texture.Height);
            new_tile.Location = new Vector3(x, y, 0);
            new_tile.Region = new Region((int)new_tile.Location.X * Main.Game.TileSize.X, (int)new_tile.Location.Y * Main.Game.TileSize.Y, Main.Game.TileSize.X, Main.Game.TileSize.Y);
            new_tile.Visible = true;

            if (new_tile.Name.Contains("North"))
            {
                new_tile.Direction = Direction.Up;
            }
            else if (new_tile.Name.Contains("East"))
            {
                new_tile.Direction = Direction.Right;
            }
            else if (new_tile.Name.Contains("South"))
            {
                new_tile.Direction = Direction.Down;
            }
            else if (new_tile.Name.Contains("West"))
            {
                new_tile.Direction = Direction.Left;
            }

            if (new_tile.Name.Contains("Wall") ||
                new_tile.Name.Contains("Fence") ||
                new_tile.Name.Contains("Fridge") ||
                new_tile.Name.Contains("Lamp") ||
                new_tile.Name.Contains("StreetLight") ||
                new_tile.Name.Contains("TV") ||
                new_tile.Name.Contains("Tree") ||
                new_tile.Name.Contains("Counter") ||
                new_tile.Name.Contains("Table") ||
                new_tile.Name.Contains("Stove"))
            {
                new_tile.BlocksMovement = true;

                if (new_tile.Name.Contains("Lamp") ||
                    new_tile.Name.Contains("StreetLight"))
                {
                    new_tile.IsLightSource = true;
                    Handler.light_sources.Add(new Point((int)new_tile.Location.X, (int)new_tile.Location.Y));
                }
                else if (new_tile.Name.Contains("TV"))
                {
                    Handler.light_sources.Add(new Point((int)new_tile.Location.X, (int)new_tile.Location.Y));
                }
            }
            else if (new_tile.Name.Contains("Door"))
            {
                new_tile.Name += "_Closed";
                new_tile.BlocksMovement = true;
            }
            else if (new_tile.Name.Contains("Window"))
            {
                new_tile.Name += "_Closed";
            }
            else if (new_tile.Name.Contains("Bed") &&
                     !new_tile.Name.Contains("RoomType"))
            {
                Character character = CharacterUtil.GenCharacter(last_name);
                character.Direction = new_tile.Direction;
                character.MapID = worldTile.ID;

                if (new_tile.Direction == Direction.Up)
                {
                    character.Animator.FaceNorth(character);
                }
                else if (new_tile.Direction == Direction.Right)
                {
                    character.Animator.FaceEast(character);
                }
                else if (new_tile.Direction == Direction.Down)
                {
                    character.Animator.FaceSouth(character);
                }
                else if (new_tile.Direction == Direction.Left)
                {
                    character.Animator.FaceWest(character);
                }

                if (new_tile.Direction == Direction.Up)
                {
                    character.Location = new Vector3(new_tile.Location.X, new_tile.Location.Y + 1, new_tile.Location.Z);
                    character.Region = new Region(new_tile.Region.X, new_tile.Region.Y + Main.Game.TileSize.Y, Main.Game.TileSize.X, Main.Game.TileSize.Y);
                }
                else if (new_tile.Direction == Direction.Left)
                {
                    character.Location = new Vector3(new_tile.Location.X + 1, new_tile.Location.Y, new_tile.Location.Z);
                    character.Region = new Region(new_tile.Region.X + Main.Game.TileSize.X, new_tile.Region.Y, Main.Game.TileSize.X, Main.Game.TileSize.Y);
                }
                else
                {
                    character.Location = new Vector3(new_tile.Location.X, new_tile.Location.Y, new_tile.Location.Z);
                    character.Region = new Region(new_tile.Region.X, new_tile.Region.Y, Main.Game.TileSize.X, Main.Game.TileSize.Y);
                }

                CharacterManager.GetArmy("Characters").GetSquad("Citizens").Characters.Add(character);
            }

            if (layer.Name == "BottomTiles" ||
                layer.Name == "MiddleTiles")
            {
                new_tile.Inventory.ID = Handler.GetID();
                new_tile.Inventory.Location = new Vector3(x, y, 0);
                InventoryManager.Inventories.Add(new_tile.Inventory);
            }

            //Add to world
            layer.Tiles.Add(new_tile);

            //Add to block list
            if (worldTile.Type.Contains("Residential"))
            {
                foreach (Map residential in Residential)
                {
                    if (residential.ID == worldTile.ID)
                    {
                        Layer map_layer = residential.GetLayer(layer.Name);
                        if (map_layer != null)
                        {
                            map_layer.Tiles.Add(new_tile);
                        }

                        break;
                    }
                }
            }
            else if (worldTile.Type.Contains("Commercial"))
            {
                foreach (Map commercial in Commercial)
                {
                    if (commercial.ID == worldTile.ID)
                    {
                        Layer map_layer = commercial.GetLayer(layer.Name);
                        if (map_layer != null)
                        {
                            map_layer.Tiles.Add(new_tile);
                        }

                        break;
                    }
                }
            }
            else if (worldTile.Type.Contains("Park"))
            {
                foreach (Map park in Parks)
                {
                    if (park.ID == worldTile.ID)
                    {
                        Layer map_layer = park.GetLayer(layer.Name);
                        if (map_layer != null)
                        {
                            map_layer.Tiles.Add(new_tile);
                        }

                        break;
                    }
                }
            }
            else if (worldTile.Type.Contains("Road"))
            {
                foreach (Map road in Roads)
                {
                    if (road.ID == worldTile.ID)
                    {
                        Layer map_layer = road.GetLayer(layer.Name);
                        if (map_layer != null)
                        {
                            map_layer.Tiles.Add(new_tile);
                        }

                        break;
                    }
                }
            }
        }

        #endregion

        private static void AssignOwners(Layer tiles)
        {
            Army army = CharacterManager.GetArmy("Characters");
            Squad squad = army.GetSquad("Citizens");

            int char_count = squad.Characters.Count;
            for (int i = 0; i < char_count; i++)
            {
                Character character = squad.Characters[i];

                int block_x = ((int)character.Location.X / 20) * 20;
                int block_y = ((int)character.Location.Y / 20) * 20;

                if (!Handler.OwnedFurniture.ContainsKey(character.ID))
                {
                    Handler.OwnedFurniture.Add(character.ID, new List<Tile>());
                }

                List<Tile> furniture = WorldUtil.GetAllFurniture(tiles, new Point(block_x, block_y));
                foreach (Tile tile in furniture)
                {
                    tile.OwnerIDs.Add(character.ID);
                    Handler.OwnedFurniture[character.ID].Add(tile);
                }
            }
        }

        public static void GenLoot()
        {
            Handler.Loading_Percent = 0;
            Handler.Loading_Message = "Generating loot...";

            World world = SceneManager.GetScene("Gameplay").World;
            Map map = world.Maps[0];

            Layer room_tiles = map.GetLayer("RoomTiles");
            if (room_tiles != null)
            {
                Layer bottom_tiles = map.GetLayer("BottomTiles");
                if (bottom_tiles != null)
                {
                    total = bottom_tiles.Tiles.Count * 2;
                    current = 0;

                    foreach (Tile bottom_tile in bottom_tiles.Tiles)
                    {
                        current++;
                        Handler.Loading_Percent = (current * 100) / total;

                        Tile room_tile = room_tiles.GetTile(new Vector2(bottom_tile.Location.X, bottom_tile.Location.Y));
                        if (room_tile != null)
                        {
                            if (!string.IsNullOrEmpty(room_tile.Name))
                            {
                                Inventory inventory = null;

                                string category = InventoryUtil.GetCategory_FromTile(room_tile);
                                if (!string.IsNullOrEmpty(category))
                                {
                                    inventory = bottom_tile.Inventory;

                                    if (bottom_tile.Name.Contains("Grass"))
                                    {
                                        inventory.Name = "Grass";
                                        inventory.Max_Value = 49;
                                    }

                                    if (inventory != null)
                                    {
                                        List<Item> loot = InventoryUtil.GetLoot(category, inventory.Name, (int)inventory.Max_Value);
                                        foreach (Item item in loot)
                                        {
                                            inventory.Items.Add(item);

                                            if (item.Type == "Container")
                                            {
                                                if (!InventoryManager.Inventories.Contains(item.Inventory))
                                                {
                                                    InventoryManager.Inventories.Add(item.Inventory);
                                                }

                                                List<Item> item_loot = InventoryUtil.GetLoot(category, inventory.Name, (int)inventory.Max_Value);
                                                foreach (Item sub_item in item_loot)
                                                {
                                                    item.Inventory.Items.Add(sub_item);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                Layer middle_tiles = map.GetLayer("MiddleTiles");
                if (middle_tiles != null)
                {
                    foreach (Tile middle_tile in middle_tiles.Tiles)
                    {
                        current++;
                        Handler.Loading_Percent = (current * 100) / total;

                        if (middle_tile.Texture != null)
                        {
                            Tile room_tile = room_tiles.GetTile(new Vector2(middle_tile.Location.X, middle_tile.Location.Y));
                            if (room_tile != null)
                            {
                                if (!string.IsNullOrEmpty(room_tile.Name))
                                {
                                    Inventory inventory = null;

                                    string category = InventoryUtil.GetCategory_FromTile(room_tile);
                                    if (!string.IsNullOrEmpty(category))
                                    {
                                        inventory = middle_tile.Inventory;

                                        if (middle_tile.Name.Contains("Counter"))
                                        {
                                            inventory.Name = "Counter";
                                            inventory.Max_Value = 21;
                                        }
                                        else if (middle_tile.Name.Contains("Fridge"))
                                        {
                                            inventory.Name = "Fridge";
                                            inventory.Max_Value = 35;
                                        }
                                        else if (middle_tile.Name.Contains("Dresser"))
                                        {
                                            inventory.Name = "Dresser";
                                            inventory.Max_Value = 28;
                                        }
                                        else if (middle_tile.Name.Contains("Desk"))
                                        {
                                            inventory.Name = "Desk";
                                            inventory.Max_Value = 21;
                                        }
                                        else if (middle_tile.Name.Contains("Bookshelf"))
                                        {
                                            inventory.Name = "Bookshelf";
                                            inventory.Max_Value = 14;
                                        }

                                        if (inventory != null)
                                        {
                                            List<Item> loot = InventoryUtil.GetLoot(category, inventory.Name, (int)inventory.Max_Value);
                                            foreach (Item item in loot)
                                            {
                                                inventory.Items.Add(item);

                                                if (item.Type == "Container")
                                                {
                                                    if (!InventoryManager.Inventories.Contains(item.Inventory))
                                                    {
                                                        InventoryManager.Inventories.Add(item.Inventory);
                                                    }

                                                    List<Item> item_loot = InventoryUtil.GetLoot(category, item.Name, (int)item.Inventory.Max_Value);
                                                    foreach (Item sub_item in item_loot)
                                                    {
                                                        item.Inventory.Items.Add(sub_item);
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
            }
        }

        #endregion
    }
}

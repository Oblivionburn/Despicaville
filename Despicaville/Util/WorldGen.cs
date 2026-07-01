using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using OP_Engine.Tiles;
using OP_Engine.Utility;
using OP_Engine.Scenes;
using OP_Engine.Inventories;
using OP_Engine.Characters;
using OP_Engine.Enums;
using OP_Engine.Jobs;

namespace Despicaville.Util
{
    public static class WorldGen
    {
        #region Variables

        public static List<Map> Worldmap = [];
        public static List<Map> Blocks = [];
        private static Dictionary<long, List<Map>> BlockRooms = [];

        private static List<Map> PoliceBlocks = [];
        private static List<Map> GroceryBlocks = [];
        private static List<Map> DinerBlocks = [];
        private static List<Map> ResidentialBlocks = [];
        private static List<Map> ParkBlocks = [];

        public static List<Map> Residential = [];
        public static Map? Police = null;
        public static Map? Grocery = null;
        public static Map? Diner = null;
        public static Dictionary<long, List<Map>> Rooms = [];

        private static int column;
        private static int row;

        private static int total;
        private static int current;

        private static string? last_name;

        #endregion

        #region Methods

        #region GetBlocks

        public static void ParseBlock(string file)
        {
            using (XmlTextReader reader = new(File.OpenRead(file)))
            {
                try
                {
                    while (reader.Read())
                    {
                        switch (reader.Name)
                        {
                            case "Map":
                                VisitBlock(reader, file);
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Main.Game?.CrashHandler(e);
                }
            }
        }

        private static void VisitBlock(XmlTextReader reader, string file)
        {
            Map? map = null;

            while (reader.Read())
            {
                if (reader.Name == "Map" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "Globals":
                        map = new Map
                        {
                            ID = Handler.GetID()
                        };

                        VisitGlobals(reader, map, file);
                        Blocks.Add(map);
                        break;

                    case "BottomTiles":
                        Layer bottomTiles = new()
                        {
                            Name = "BottomTiles"
                        };

                        VisitBottomTiles(reader, bottomTiles);
                        map?.Layers.Add(bottomTiles);
                        break;

                    case "RoomTiles":
                        Layer roomTiles = new()
                        {
                            Name = "RoomTiles"
                        };

                        VisitRoomTiles(reader, roomTiles);
                        map?.Layers.Add(roomTiles);
                        break;

                    case "MiddleTiles":
                        Layer middleTiles = new()
                        {
                            Name = "MiddleTiles"
                        };

                        VisitMiddleTiles(reader, middleTiles);
                        map?.Layers.Add(middleTiles);
                        break;

                    case "TopTiles":
                        Layer topTiles = new()
                        {
                            Name = "TopTiles"
                        };

                        VisitTopTiles(reader, topTiles);
                        map?.Layers.Add(topTiles);
                        break;

                    case "Rooms":
                        if (map != null)
                        {
                            VisitRooms(reader, map);
                        }
                        break;
                }
            }
        }

        private static void VisitGlobals(XmlTextReader reader, Map map, string file)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "Type":
                        map.Type = reader.Value;
                        map.Name = Path.GetFileNameWithoutExtension(file);
                        break;

                    case "Direction":
                        if (Enum.TryParse(reader.Value, out Direction direction))
                        {
                            map.Direction = direction;
                        }
                        break;
                }
            }
        }

        private static void VisitBottomTiles(XmlTextReader reader, Layer layer)
        {
            while (reader.Read())
            {
                if (reader.Name == "BottomTiles" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "BottomTile":
                        Tile tile = new();
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

                        if (!string.IsNullOrEmpty(tile.Name))
                        {
                            tile.Texture = Handler.GetTexture(tile.Name);
                        }
                        break;

                    case "Location":
                        string[] values = reader.Value.Split(',');
                        int x = int.Parse(values[0]);
                        int y = int.Parse(values[1]);

                        tile.Location = new Location(x, y);
                        break;
                }
            }
        }

        private static void VisitRoomTiles(XmlTextReader reader, Layer layer)
        {
            while (reader.Read())
            {
                if (reader.Name == "RoomTiles" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "RoomTile":
                        Tile tile = new();
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
                    case "Texture":
                        tile.Name = reader.Value;

                        if (!string.IsNullOrEmpty(tile.Name))
                        {
                            tile.Texture = Handler.GetTexture(tile.Name);
                        }
                        break;

                    case "Location":
                        string[] values = reader.Value.Split(',');
                        int x = int.Parse(values[0]);
                        int y = int.Parse(values[1]);

                        tile.Location = new Location(x, y);
                        break;
                }
            }
        }

        private static void VisitMiddleTiles(XmlTextReader reader, Layer layer)
        {
            while (reader.Read())
            {
                if (reader.Name == "MiddleTiles" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "MiddleTile":
                        Tile tile = new();
                        VisitBottomTile(reader, tile);
                        layer.Tiles.Add(tile);
                        break;
                }
            }
        }

        private static void VisitTopTiles(XmlTextReader reader, Layer layer)
        {
            while (reader.Read())
            {
                if (reader.Name == "TopTiles" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "TopTile":
                        Tile tile = new();
                        VisitBottomTile(reader, tile);
                        layer.Tiles.Add(tile);
                        break;
                }
            }
        }

        private static void VisitRooms(XmlTextReader reader, Map blockMap)
        {
            BlockRooms.Add(blockMap.ID, []);

            while (reader.Read())
            {
                if (reader.Name == "Rooms" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "Room":
                        Map map = new()
                        {
                            ID = blockMap.ID
                        };

                        VisitRoom(reader, map);

                        BlockRooms[blockMap.ID].Add(map);
                        break;
                }
            }
        }

        private static void VisitRoom(XmlTextReader reader, Map map)
        {
            while (reader.Read())
            {
                if (reader.Name == "Room" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "RoomProperties":
                        VisitRoomProperties(reader, map);
                        break;

                    case "Tiles":
                        Layer tiles = new()
                        {
                            Name = "Tiles"
                        };
                        VisitTiles(reader, tiles);
                        map.Layers.Add(tiles);
                        break;

                    case "Exits":
                        Layer exits = new()
                        {
                            Name = "Exits"
                        };
                        VisitExits(reader, exits);
                        map.Layers.Add(exits);
                        break;
                }
            }
        }

        private static void VisitRoomProperties(XmlTextReader reader, Map map)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "Name":
                        map.Name = reader.Value;
                        break;
                }
            }
        }

        private static void VisitTiles(XmlTextReader reader, Layer layer)
        {
            while (reader.Read())
            {
                if (reader.Name == "Tiles" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "Tile":
                        Tile tile = new();
                        VisitTile(reader, tile);
                        layer.Tiles.Add(tile);
                        break;
                }
            }
        }

        private static void VisitTile(XmlTextReader reader, Tile tile)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "Texture":
                        tile.Name = reader.Value;
                        break;

                    case "Location":
                        var parts = reader.Value.Split(',');
                        float x = float.Parse(parts[0]);
                        float y = float.Parse(parts[1]);

                        tile.Location = new(x, y, 0);
                        break;
                }
            }
        }

        private static void VisitExits(XmlTextReader reader, Layer layer)
        {
            while (reader.Read())
            {
                if (reader.Name == "Exits" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "Exit":
                        Tile tile = new();
                        VisitExit(reader, tile);
                        layer.Tiles.Add(tile);
                        break;
                }
            }
        }

        private static void VisitExit(XmlTextReader reader, Tile tile)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "Location":
                        var parts = reader.Value.Split(',');
                        float x = float.Parse(parts[0]);
                        float y = float.Parse(parts[1]);

                        tile.Location = new(x, y, 0);
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

            PoliceBlocks.Clear();
            GroceryBlocks.Clear();
            DinerBlocks.Clear();
            ParkBlocks.Clear();

            Residential.Clear();
            Police = null;
            Grocery = null;
            Diner = null;
            Rooms.Clear();

            foreach (Map block in Blocks)
            {
                if (block.Type == "Park")
                {
                    ParkBlocks.Add(block);
                }
                else if (block.Type == "Service" &&
                         block.Name != null)
                {
                    if (block.Name.Contains("Police"))
                    {
                        PoliceBlocks.Add(block);
                    }
                }
                else if (block.Type == "Commercial" &&
                         block.Name != null)
                {
                    if (block.Name.Contains("Grocery"))
                    {
                        GroceryBlocks.Add(block);
                    }
                    else if (block.Name.Contains("Diner"))
                    {
                        DinerBlocks.Add(block);
                    }
                }
                else if (block.Type == "Residential")
                {
                    ResidentialBlocks.Add(block);
                }
            }

            CryptoRandom random = new();
            int choice = 0;

            string? previous_type = "";
            string previous_w = "";

            #region First Row

            AddToWorldmap("Road_Corner_NW", "Map_Road_Corner_NW", 0, 0);

            column++;
            AddToWorldmap("Road_WE", "Map_Road_WE", column, 0);

            if (Handler.MapSize_X > 3)
            {
                if (Handler.MapSize_X > 4)
                {
                    choice = random.Next(1, 4);
                    if (choice <= 2)
                    {
                        column++;
                        AddToWorldmap("Road_TSection_S", "Map_Road_TSection_S", column, 0);
                    }
                    else if (choice == 3)
                    {
                        column++;
                        AddToWorldmap("Road_WE", "Map_Road_WE", column, 0);
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
                                    AddToWorldmap("Road_TSection_S", "Map_Road_TSection_S", column, 0);
                                    previous_type = previous_w = "Road_TSection_S";
                                }
                                else if (choice == 3)
                                {
                                    column++;
                                    AddToWorldmap("Road_WE", "Map_Road_WE", column, 0);
                                    previous_type = previous_w = "Road_WE";
                                }
                            }
                            else
                            {
                                column++;
                                AddToWorldmap("Road_WE", "Map_Road_WE", column, 0);
                                previous_type = previous_w = "Road_WE";
                            }
                        }
                    }
                }

                column++;
                AddToWorldmap("Road_WE", "Map_Road_WE", column, 0);
            }

            column++;
            AddToWorldmap("Road_Corner_NE", "Map_Road_Corner_NE", column, 0);

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
                        AddToWorldmap("Road_TSection_E", "Map_Road_TSection_E", 0, row);
                        previous_type = previous_w = "Road_TSection_E";
                    }
                    else if (choice == 3)
                    {
                        AddToWorldmap("Road_NS", "Map_Road_NS", 0, row);
                        previous_type = previous_w = "Road_NS";
                    }
                }
                else
                {
                    AddToWorldmap("Road_NS", "Map_Road_NS", 0, row);
                    previous_type = previous_w = "Road_NS";
                }

                for (int x = 1; x < Handler.MapSize_X - 1; x++)
                {
                    if (!string.IsNullOrEmpty(previous_type))
                    {
                        previous_type = AddToMap_InnerRows(previous_type, x, y);
                    }
                }

                if (previous_type == "Open")
                {
                    column++;
                    previous_type = AddToWorldmap("Road_NS", "Map_Road_NS", column, row);
                }
                else if (previous_type == "Road_NS" ||
                         previous_type == "Road_Corner_NE" ||
                         previous_type == "Road_Corner_SE" ||
                         previous_type == "Road_TSection_W")
                {
                    column++;
                    previous_type = AddToWorldmap("Road_NS", "Map_Road_NS", column, row);
                }
                else
                {
                    column++;
                    previous_type = AddToWorldmap("Road_TSection_W", "Map_Road_TSection_W", column, row);
                }
            }

            #endregion

            #region Last Row

            row = Handler.MapSize_Y - 1;
            column = 0;

            previous_type = AddToWorldmap("Road_Corner_SW", "Map_Road_Corner_SW", 0, row);

            for (int i = 1; i < Handler.MapSize_X - 1; i++)
            {
                if (!string.IsNullOrEmpty(previous_type))
                {
                    previous_type = AddToMap_LastRow(i);
                }
            }

            previous_type = AddToWorldmap("Road_Corner_SE", "Map_Road_Corner_SE", Handler.MapSize_X - 1, row);

            #endregion

            FillMap();
        }

        #region GenMap

        private static string? AddToMap_InnerRows(string previous, int x, int y)
        {
            CryptoRandom random = new();
            string? this_block = "";
            Map upper_block = Worldmap[x + (Handler.MapSize_X * (row - 1))];

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
                    this_block = AddToWorldmap("Road_NS", "Map_Road_NS", column, row);
                }
                else if (upper_block.Name == "Road_NS")
                {
                    if (y == Handler.MapSize_Y - 2)
                    {
                        this_block = AddToWorldmap("Road_NS", "Map_Road_NS", column, row);
                    }
                    else
                    {
                        int type = random.Next(1, 3);
                        if (type == 1)
                        {
                            this_block = AddToWorldmap("Road_NS", "Map_Road_NS", column, row);
                        }
                        else if (type == 2)
                        {
                            this_block = AddToWorldmap("Road_TSection_E", "Map_Road_TSection_E", column, row);
                        }
                    }
                }
                else
                {
                    this_block = AddToWorldmap("Open", "Map_Open", column, row);
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
                        this_block = AddToWorldmap("Road_TSection_W", "Map_Road_TSection_W", column, row);
                    }
                    else
                    {
                        int type = random.Next(1, 4);
                        if (type == 1)
                        {
                            this_block = AddToWorldmap("Road_Cross", "Map_Road_Cross", column, row);
                        }
                        else if (type == 2)
                        {
                            this_block = AddToWorldmap("Road_TSection_N", "Map_Road_TSection_N", column, row);
                        }
                        else if (type == 3)
                        {
                            this_block = AddToWorldmap("Road_TSection_W", "Map_Road_TSection_W", column, row);
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
                        this_block = AddToWorldmap("Road_TSection_W", "Map_Road_TSection_W", column, row);
                    }
                    else
                    {
                        int type = random.Next(1, 3);
                        if (type == 1)
                        {
                            this_block = AddToWorldmap("Road_Corner_SE", "Map_Road_Corner_SE", column, row);
                        }
                        else if (type == 2)
                        {
                            this_block = AddToWorldmap("Road_TSection_W", "Map_Road_TSection_W", column, row);
                        }
                    }
                }
                else
                {
                    this_block = AddToWorldmap("Road_WE", "Map_Road_WE", column, row);
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
                        this_block = AddToWorldmap("Road_TSection_W", "Map_Road_TSection_W", column, row);
                    }
                    else
                    {
                        this_block = AddToWorldmap("Road_Corner_SE", "Map_Road_Corner_SE", column, row);
                    }
                }
                else if (upper_block.Name == "Road_NS" ||
                         upper_block.Name == "Road_TSection_W" ||
                         upper_block.Name == "Road_Corner_NE")
                {
                    if (y == Handler.MapSize_Y - 2)
                    {
                        this_block = AddToWorldmap("Road_TSection_W", "Map_Road_TSection_W", column, row);
                    }
                    else
                    {
                        this_block = AddToWorldmap("Road_TSection_N", "Map_Road_TSection_N", column, row);
                    }
                }
                else
                {
                    if (y == Handler.MapSize_Y - 2)
                    {
                        this_block = AddToWorldmap("Road_Corner_NE", "Map_Road_Corner_NE", column, row);
                    }
                    else
                    {
                        this_block = AddToWorldmap("Road_WE", "Map_Road_WE", column, row);
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
                        this_block = AddToWorldmap("Road_TSection_W", "Map_Road_TSection_W", column, row);
                    }
                    else
                    {
                        this_block = AddToWorldmap("Road_Corner_SE", "Map_Road_Corner_SE", column, row);
                    }
                }
                else if (upper_block.Name == "Road_NS" ||
                         upper_block.Name == "Road_TSection_W" ||
                         upper_block.Name == "Road_Corner_NE")
                {
                    if (y == Handler.MapSize_Y - 2)
                    {
                        this_block = AddToWorldmap("Road_TSection_W", "Map_Road_TSection_W", column, row);
                    }
                    else
                    {
                        this_block = AddToWorldmap("Road_TSection_N", "Map_Road_TSection_N", column, row);
                    }
                }
                else
                {
                    if (y == Handler.MapSize_Y - 2)
                    {
                        this_block = AddToWorldmap("Road_Corner_NE", "Map_Road_Corner_NE", column, row);
                    }
                    else
                    {
                        this_block = AddToWorldmap("Road_WE", "Map_Road_WE", column, row);
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
                            this_block = AddToWorldmap("Road_Cross", "Map_Road_Cross", column, row);
                        }
                        else
                        {
                            this_block = AddToWorldmap("Road_TSection_W", "Map_Road_TSection_W", column, row);
                        }
                    }
                    else
                    {
                        if (x == Handler.MapSize_X - 2)
                        {
                            this_block = AddToWorldmap("Road_TSection_N", "Map_Road_TSection_N", column, row);
                        }
                        else
                        {
                            int type = random.Next(1, 4);
                            if (type == 1)
                            {
                                this_block = AddToWorldmap("Road_Cross", "Map_Road_Cross", column, row);
                            }
                            else if (type == 2)
                            {
                                this_block = AddToWorldmap("Road_TSection_N", "Map_Road_TSection_N", column, row);
                            }
                            else if (type == 3)
                            {
                                this_block = AddToWorldmap("Road_TSection_W", "Map_Road_TSection_W", column, row);
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
                            this_block = AddToWorldmap("Road_Cross", "Map_Road_Cross", column, row);
                        }
                        else
                        {
                            this_block = AddToWorldmap("Road_TSection_W", "Map_Road_TSection_W", column, row);
                        }
                    }
                    else
                    {
                        if (x == Handler.MapSize_X - 2)
                        {
                            this_block = AddToWorldmap("Road_TSection_N", "Map_Road_TSection_N", column, row);
                        }
                        else
                        {
                            this_block = AddToWorldmap("Road_TSection_W", "Map_Road_TSection_W", column, row);
                        }
                    }
                }
                else if (upper_block.Name == "Road_Corner_NW")
                {
                    if (y == Handler.MapSize_Y - 2)
                    {
                        if (x == Handler.MapSize_X - 2)
                        {
                            this_block = AddToWorldmap("Road_Cross", "Map_Road_Cross", column, row);
                        }
                        else
                        {
                            this_block = AddToWorldmap("Road_TSection_W", "Map_Road_TSection_W", column, row);
                        }
                    }
                    else
                    {
                        if (x == Handler.MapSize_X - 2)
                        {
                            this_block = AddToWorldmap("Road_TSection_N", "Map_Road_TSection_N", column, row);
                        }
                        else
                        {
                            this_block = AddToWorldmap("Road_TSection_W", "Map_Road_TSection_W", column, row);
                        }
                    }
                }
                else if (upper_block.Name == "Road_NS")
                {
                    if (y == Handler.MapSize_Y - 2)
                    {
                        if (x == Handler.MapSize_X - 2)
                        {
                            this_block = AddToWorldmap("Road_Cross", "Map_Road_Cross", column, row);
                        }
                        else
                        {
                            this_block = AddToWorldmap("Road_TSection_W", "Map_Road_TSection_W", column, row);
                        }
                    }
                    else
                    {
                        if (x == Handler.MapSize_X - 2)
                        {
                            this_block = AddToWorldmap("Road_TSection_N", "Map_Road_TSection_N", column, row);
                        }
                        else
                        {
                            int type = random.Next(1, 7);
                            if (type <= 4)
                            {
                                this_block = AddToWorldmap("Road_Cross", "Map_Road_Cross", column, row);
                            }
                            else if (type == 5)
                            {
                                this_block = AddToWorldmap("Road_TSection_N", "Map_Road_TSection_N", column, row);
                            }
                            else if (type == 6)
                            {
                                this_block = AddToWorldmap("Road_TSection_W", "Map_Road_TSection_W", column, row);
                            }
                        }
                    }
                }
                else
                {
                    if (x == Handler.MapSize_X - 2)
                    {
                        this_block = AddToWorldmap("Road_WE", "Map_Road_WE", column, row);
                    }
                    else
                    {
                        int type = random.Next(1, 3);
                        if (type == 1)
                        {
                            this_block = AddToWorldmap("Road_TSection_S", "Map_Road_TSection_S", column, row);
                        }
                        else if (type == 2)
                        {
                            this_block = AddToWorldmap("Road_WE", "Map_Road_WE", column, row);
                        }
                    }
                }

                #endregion
            }
            else
            {
                #region West Block Not Facing Current

                if (upper_block.Name == "Road_Cross" ||
                    upper_block.Name == "Road_TSection_S" ||
                    upper_block.Name == "Road_TSection_W" ||
                    upper_block.Name == "Road_TSection_E" ||
                    upper_block.Name == "Road_Corner_NW" ||
                    upper_block.Name == "Road_Corner_NE")
                {
                    this_block = AddToWorldmap("Road_NS", "Map_Road_NS", column, row);
                }
                else if (upper_block.Name == "Road_NS")
                {
                    if (y == Handler.MapSize_Y - 2)
                    {
                        this_block = AddToWorldmap("Road_NS", "Map_Road_NS", column, row);
                    }
                    else
                    {
                        int type = random.Next(1, 3);
                        if (type == 1)
                        {
                            this_block = AddToWorldmap("Road_NS", "Map_Road_NS", column, row);
                        }
                        else if (type == 2)
                        {
                            this_block = AddToWorldmap("Road_TSection_E", "Map_Road_TSection_E", column, row);
                        }
                    }
                }
                else
                {
                    this_block = AddToWorldmap("Open", "Map_Open", column, row);
                }

                #endregion
            }

            return this_block;
        }

        private static string? AddToMap_LastRow(int x)
        {
            column++;
            Map upper_block = Worldmap[x + (Handler.MapSize_X * (row - 1))];

            if (upper_block.Name == "Road_Cross" ||
                upper_block.Name == "Road_TSection_S" ||
                upper_block.Name == "Road_TSection_W" ||
                upper_block.Name == "Road_TSection_E" ||
                upper_block.Name == "Road_Corner_NW" ||
                upper_block.Name == "Road_Corner_NE" ||
                upper_block.Name == "Road_NS")
            {
                return AddToWorldmap("Road_TSection_N", "Map_Road_TSection_N", column, row);
            }
            else
            {
                return AddToWorldmap("Road_WE", "Map_Road_WE", column, row);
            }
        }

        private static string? AddToWorldmap(string name, string type, int column, int row)
        {
            if (Main.Game == null)
            {
                return null;
            }

            Map map = new()
            {
                ID = Handler.GetID(),
                Name = name,
                Type = type,
                Texture = Handler.GetTexture(type),
                Location = new Location(column, row, 0),
                Region = new Region(column * Main.Game.TileSize.X, row * Main.Game.TileSize.Y, Main.Game.TileSize.X, Main.Game.TileSize.Y),
                Visible = true
            };

            if (map.Texture != null)
            {
                map.Image = new Rectangle(0, 0, map.Texture.Width, map.Texture.Height);
            }

            Layer bottom_tiles = NewLayer_Worldmap(map, "BottomTiles");
            map.Layers.Add(bottom_tiles);

            Layer room_tiles = NewLayer_Worldmap(map, "RoomTiles");
            map.Layers.Add(room_tiles);

            Layer middle_tiles = NewLayer_Worldmap(map, "MiddleTiles");
            map.Layers.Add(middle_tiles);

            Layer top_tiles = NewLayer_Worldmap(map, "TopTiles");
            map.Layers.Add(top_tiles);

            Worldmap.Add(map);

            return name;
        }

        private static void UpdateWorldmap(Map map, string? name, string? type)
        {
            map.Name = name;
            map.Type = type;

            if (map.Type != null)
            {
                map.Texture = Handler.GetTexture(map.Type);
            }

            if (map.Texture != null)
            {
                map.Image = new Rectangle(0, 0, map.Texture.Width, map.Texture.Height);
            }
        }

        private static void FillMap()
        {
            int residential_count = 0;

            int open_count = 0;
            foreach (Map map in Worldmap)
            {
                if (map.Name == "Open")
                {
                    open_count++;
                }
            }

            CryptoRandom random;

            foreach (Map map in Worldmap)
            {
                if (map.Location == null)
                {
                    continue;
                }

                if (map.Name == "Open")
                {
                    bool force_park = false;

                    if (open_count <= 3 &&
                        PoliceBlocks.Count > 0)
                    {
                        Map? block = null;

                        //Force police
                        if (PoliceBlocks.Count > 0)
                        {
                            block = GetRandomBlock(PoliceBlocks, map.Location);
                            if (block != null)
                            {
                                PoliceBlocks.Clear();
                            }
                        }

                        if (block != null)
                        {
                            UpdateWorldmap(map, block.Name, "Map_Service");
                            Police = map;
                            open_count--;
                        }
                        else
                        {
                            force_park = true;
                        }
                    }
                    else if (open_count <= 3 &&
                             GroceryBlocks.Count > 0)
                    {
                        Map? block = null;

                        //Force commercial
                        block = GetRandomBlock(GroceryBlocks, map.Location);
                        if (block != null)
                        {
                            GroceryBlocks.Clear();

                            UpdateWorldmap(map, block.Name, "Map_Commercial");
                            Grocery = map;
                            open_count--;
                        }
                        else
                        {
                            force_park = true;
                        }
                    }
                    else if (open_count <= 3 &&
                             DinerBlocks.Count > 0)
                    {
                        Map? block = null;

                        //Force commercial
                        block = GetRandomBlock(DinerBlocks, map.Location);
                        if (block != null)
                        {
                            DinerBlocks.Clear();

                            UpdateWorldmap(map, block.Name, "Map_Commercial");
                            Diner = map;
                            open_count--;
                        }
                        else
                        {
                            force_park = true;
                        }
                    }
                    else if (open_count <= 6 &&
                             residential_count < 3)
                    {
                        //Force residential
                        Map? block = GetRandomBlock(ResidentialBlocks, map.Location);
                        if (block != null)
                        {
                            UpdateWorldmap(map, block.Name, "Map_Residential");
                            Residential.Add(map);
                            residential_count++;
                            open_count--;
                        }
                        else
                        {
                            force_park = true;
                        }
                    }
                    else
                    {
                        random = new CryptoRandom();
                        int block_choice = random.Next(0, 3);
                        if (block_choice == 0)
                        {
                            //Commercial
                            if (PoliceBlocks.Count > 0)
                            {
                                //Police
                                Map? block = GetRandomBlock(PoliceBlocks, map.Location);
                                if (block != null)
                                {
                                    UpdateWorldmap(map, block.Name, "Map_Service");
                                    Police = map;
                                    open_count--;

                                    PoliceBlocks.Clear();
                                }
                                else
                                {
                                    force_park = true;
                                }
                            }
                            else if (GroceryBlocks.Count > 0)
                            {
                                //Grocery
                                Map? block = GetRandomBlock(GroceryBlocks, map.Location);
                                if (block != null)
                                {
                                    UpdateWorldmap(map, block.Name, "Map_Commercial");
                                    Grocery = map;
                                    open_count--;

                                    GroceryBlocks.Clear();
                                }
                                else
                                {
                                    force_park = true;
                                }
                            }
                            else if (DinerBlocks.Count > 0)
                            {
                                //Diner
                                Map? block = GetRandomBlock(DinerBlocks, map.Location);
                                if (block != null)
                                {
                                    UpdateWorldmap(map, block.Name, "Map_Commercial");
                                    Diner = map;
                                    open_count--;

                                    DinerBlocks.Clear();
                                }
                                else
                                {
                                    force_park = true;
                                }
                            }
                            else
                            {
                                force_park = true;
                            }
                        }
                        else if (block_choice == 1)
                        {
                            //Residential
                            Map? block = GetRandomBlock(ResidentialBlocks, map.Location);
                            if (block != null)
                            {
                                UpdateWorldmap(map, block.Name, "Map_Residential");
                                Residential.Add(map);
                                residential_count++;
                                open_count--;
                            }
                            else
                            {
                                force_park = true;
                            }
                        }
                        else if (block_choice == 2)
                        {
                            force_park = true;
                        }
                    }

                    if (force_park)
                    {
                        //Park
                        Map? block = GetRandomBlock(ParkBlocks, map.Location);
                        if (block != null)
                        {
                            UpdateWorldmap(map, block.Name, "Map_Park");
                            open_count--;
                        }
                    }
                }
                else if (map.Type != null &&
                         map.Type.Contains("Road"))
                {
                    foreach (Map block in Blocks)
                    {
                        if (block.Name == map.Name)
                        {
                            UpdateWorldmap(map, block.Name, "Map_" + block.Name);
                            break;
                        }
                    }
                }

                current++;
                Handler.Loading_Percent = (current * 100) / total;
            }
        }

        private static Map? GetRandomBlock(List<Map> Blocks, Location location)
        {
            List<Map> possible = [];
            List<Direction> blocked_directions = [];
            CryptoRandom random = new();

            foreach (Map map in Worldmap)
            {
                if (map.Location == null ||
                    map.Type == null)
                {
                    continue;
                }

                if (map.Type.Contains("Service") || 
                    map.Type.Contains("Commercial") ||
                    map.Type.Contains("Residential") ||
                    map.Type.Contains("Park") ||
                    map.Type.Contains("Open"))
                {
                    if (map.Location.Y == location.Y - 1 &&
                        map.Location.X == location.X)
                    {
                        blocked_directions.Add(Direction.North);
                    }
                    else if (map.Location.Y == location.Y &&
                             map.Location.X == location.X + 1)
                    {
                        blocked_directions.Add(Direction.East);
                    }
                    else if (map.Location.Y == location.Y + 1 &&
                             map.Location.X == location.X)
                    {
                        blocked_directions.Add(Direction.South);
                    }
                    else if (map.Location.Y == location.Y &&
                             map.Location.X == location.X - 1)
                    {
                        blocked_directions.Add(Direction.West);
                    }
                }
            }

            if (blocked_directions.Count > 0)
            {
                foreach (Map map in Blocks)
                {
                    if (!blocked_directions.Contains(map.Direction))
                    {
                        possible.Add(map);
                    }
                }
            }
            else
            {
                return Blocks[random.Next(0, Blocks.Count)];
            }

            if (possible.Count > 0)
            {
                return possible[random.Next(0, possible.Count)];
            }

            return null;
        }

        private static Layer NewLayer_Worldmap(Map map, string name)
        {
            return new Layer
            {
                ID = Handler.GetID(),
                Name = name,
                Map = map,
                Rows = 20,
                Columns = 20
            };
        }

        #endregion

        public static void GenTown()
        {
            Handler.MiddleFurniture.Clear();
            Handler.TopFurniture.Clear();
            Handler.OwnedFurniture.Clear();
            Handler.light_sources.Clear();

            Handler.Loading_Percent = 0;
            Handler.Loading_Message = "Generating new town...";

            World world = new()
            {
                ID = Handler.GetID(),
                Visible = true
            };

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

            total += (bottom_tiles.Columns * bottom_tiles.Rows) * 5;

            AddNewTiles(map, bottom_tiles);
            UpdateMiddleTiles(middle_tiles);

            AssignOwners(middle_tiles);
            AssignOwners(top_tiles);

            AddRoofTiles(bottom_tiles, middle_tiles, room_tiles, roof_tiles);
            AddRooms();
            AssignJobs();

            Scene? gameplay = SceneManager.GetScene("Gameplay");
            if (gameplay != null)
            {
                gameplay.World = world;
            }
        }

        #region GenTown

        private static Map NewMap(World world, string name)
        {
            return new Map
            {
                World = world,
                Visible = true,
                DrawColor = Color.White,
                Name = name
            };
        }

        private static Layer NewLayer(Map map, string name)
        {
            return new Layer
            {
                ID = Handler.GetID(),
                Name = name,
                World = map.World,
                Map = map,
                Rows = Handler.MapSize_Y * 20,
                Columns = Handler.MapSize_X * 20,
                Visible = true
            };
        }

        private static void AddNewTiles(Map map, Layer bottom_tiles)
        {
            for (int y = 0; y < bottom_tiles.Rows; y++)
            {
                for (int x = 0; x < bottom_tiles.Columns; x++)
                {
                    Map? worldTile = null;
                    foreach (Map existing in Worldmap)
                    {
                        if (existing.Location == null)
                        {
                            continue;
                        }

                        if (existing.Location.X == x / 20 &&
                            existing.Location.Y == y / 20)
                        {
                            worldTile = existing;
                            break;
                        }
                    }

                    if (worldTile?.Location != null)
                    {
                        Map? block = null;
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
                            CryptoRandom random = new();
                            int choice = random.Next(0, CharacterManager.LastNames.Count);
                            last_name = CharacterManager.LastNames[choice];

                            #region BottomTiles

                            bool found = false;

                            Layer? block_bottom_tiles = block.GetLayer("BottomTiles");
                            if (block_bottom_tiles != null)
                            {
                                foreach (Tile tile in block_bottom_tiles.Tiles)
                                {
                                    if (tile.Location == null)
                                    {
                                        continue;
                                    }

                                    int tile_x = -1;
                                    int tile_y = -1;

                                    tile_x = (int)(tile.Location.X + (worldTile.Location.X * 20));
                                    tile_y = (int)(tile.Location.Y + (worldTile.Location.Y * 20));

                                    if (tile_x == x &&
                                        tile_y == y &&
                                        tile.Texture != null &&
                                        block.Name != null)
                                    {
                                        found = true;

                                        AddTile(block.Name, map, bottom_tiles, worldTile, tile);

                                        current++;
                                        Handler.Loading_Percent = (current * 100) / total;
                                        break;
                                    }
                                }

                                if (!found)
                                {
                                    AddEmptyTile(map, bottom_tiles, worldTile, new Location(x, y, 0));

                                    current++;
                                    Handler.Loading_Percent = (current * 100) / total;
                                }
                            }

                            #endregion

                            #region MiddleTiles

                            found = false;

                            Layer? middle_tiles = map.GetLayer("MiddleTiles");
                            if (middle_tiles != null)
                            {
                                Layer? block_middle_tiles = block.GetLayer("MiddleTiles");
                                if (block_middle_tiles != null)
                                {
                                    foreach (Tile tile in block_middle_tiles.Tiles)
                                    {
                                        if (tile.Location == null)
                                        {
                                            continue;
                                        }

                                        int tile_x = -1;
                                        int tile_y = -1;

                                        tile_x = (int)(tile.Location.X + (worldTile.Location.X * 20));
                                        tile_y = (int)(tile.Location.Y + (worldTile.Location.Y * 20));

                                        if (tile_x == x &&
                                            tile_y == y &&
                                            tile.Texture != null &&
                                            block.Name != null)
                                        {
                                            found = true;

                                            AddTile(block.Name, map, middle_tiles, worldTile, tile);

                                            current++;
                                            Handler.Loading_Percent = (current * 100) / total;
                                            break;
                                        }
                                    }
                                }

                                if (!found)
                                {
                                    AddEmptyTile(map, middle_tiles, worldTile, new Location(x, y, 0));

                                    current++;
                                    Handler.Loading_Percent = (current * 100) / total;
                                }
                            }

                            #endregion

                            #region TopTiles

                            found = false;

                            Layer? top_tiles = map.GetLayer("TopTiles");
                            if (top_tiles != null)
                            {
                                Layer? block_top_tiles = block.GetLayer("TopTiles");
                                if (block_top_tiles != null)
                                {
                                    foreach (Tile tile in block_top_tiles.Tiles)
                                    {
                                        if (tile.Location == null)
                                        {
                                            continue;
                                        }

                                        int tile_x = -1;
                                        int tile_y = -1;

                                        tile_x = (int)(tile.Location.X + (worldTile.Location.X * 20));
                                        tile_y = (int)(tile.Location.Y + (worldTile.Location.Y * 20));

                                        if (tile_x == x &&
                                            tile_y == y &&
                                            tile.Texture != null &&
                                            block.Name != null)
                                        {
                                            found = true;

                                            AddTile(block.Name, map, top_tiles, worldTile, tile);

                                            current++;
                                            Handler.Loading_Percent = (current * 100) / total;
                                            break;
                                        }
                                    }
                                }

                                if (!found)
                                {
                                    AddEmptyTile(map, top_tiles, worldTile, new Location(x, y, 0));

                                    current++;
                                    Handler.Loading_Percent = (current * 100) / total;
                                }
                            }

                            #endregion

                            #region RoomTiles

                            found = false;

                            Layer? room_tiles = map.GetLayer("RoomTiles");
                            if (room_tiles != null)
                            {
                                Layer? block_room_tiles = block.GetLayer("RoomTiles");
                                if (block_room_tiles != null)
                                {
                                    foreach (Tile tile in block_room_tiles.Tiles)
                                    {
                                        if (tile.Location == null)
                                        {
                                            continue;
                                        }

                                        int tile_x = -1;
                                        int tile_y = -1;

                                        tile_x = (int)(tile.Location.X + (worldTile.Location.X * 20));
                                        tile_y = (int)(tile.Location.Y + (worldTile.Location.Y * 20));

                                        if (tile_x == x &&
                                            tile_y == y &&
                                            tile.Texture != null &&
                                            block.Name != null)
                                        {
                                            found = true;

                                            AddTile(block.Name, map, room_tiles, worldTile, tile);

                                            current++;
                                            Handler.Loading_Percent = (current * 100) / total;
                                            break;
                                        }
                                    }
                                }

                                if (!found)
                                {
                                    AddEmptyTile(map, room_tiles, worldTile, new Location(x, y, 0));

                                    current++;
                                    Handler.Loading_Percent = (current * 100) / total;
                                }
                            }

                            #endregion
                        }
                    }
                }
            }
        }

        private static void AddTile(string blockName, Map map, Layer layer, Map worldTile, Tile tile)
        {
            if (Main.Game == null ||
                worldTile.Location == null ||
                tile.Location == null ||
                tile.Texture == null)
            {
                return;
            }

            int x = (int)(tile.Location.X + (worldTile.Location.X * 20));
            int y = (int)(tile.Location.Y + (worldTile.Location.Y * 20));

            Tile new_tile = new()
            {
                ID = Handler.GetID(),
                Map = map,
                Layer = layer,
                World = layer.World,
                Name = tile.Name,
                Type = tile.Type,
                Texture = Handler.GetTexture(tile.Texture.Name),
                Location = new Location(x, y, 0),
                Visible = true
            };

            if (new_tile.Texture != null)
            {
                new_tile.Image = new Rectangle(0, 0, new_tile.Texture.Width, new_tile.Texture.Height);
            }
            
            new_tile.Region = new Region((int)new_tile.Location.X * Main.Game.TileSize.X, (int)new_tile.Location.Y * Main.Game.TileSize.Y, Main.Game.TileSize.X, Main.Game.TileSize.Y);

            Tile? existing = null;

            int count = Handler.Furniture.Count;
            for (int i = 0; i < count; i++)
            {
                Tile furniture = Handler.Furniture[i];
                if (furniture.Texture != null &&
                    new_tile.Texture != null &&
                    furniture.Texture.Name == new_tile.Texture.Name)
                {
                    existing = furniture;
                    break;
                }
            }

            if (existing != null &&
                existing.Name != null)
            {
                new_tile.Name = existing.Name;
                if (new_tile.Name.Contains("Door") ||
                    new_tile.Name.Contains("Window"))
                {
                    new_tile.Name += "_Closed";
                }

                new_tile.Direction = existing.Direction;
                new_tile.BlocksMovement = existing.BlocksMovement;
                new_tile.BlocksSight = existing.BlocksSight;
                new_tile.CanMove = existing.CanMove;

                new_tile.CanUse = existing.CanUse;
                new_tile.Sound = existing.Sound;
                new_tile.SoundRange = existing.SoundRange;

                new_tile.IsLightSource = existing.IsLightSource;
                if (new_tile.IsLightSource)
                {
                    Handler.light_sources.Add(new Point((int)new_tile.Location.X, (int)new_tile.Location.Y));
                }

                new_tile.LightColor = existing.LightColor;

                if (existing.Inventory != null)
                {
                    new_tile.Inventory = new Inventory()
                    {
                        ID = Handler.GetID(),
                        Name = existing.Inventory.Name,
                        Max_Value = existing.Inventory.Max_Value
                    };
                }
            }
            else if (new_tile.Name != null &&
                     new_tile.Name.Contains("Wall"))
            {
                new_tile.BlocksMovement = true;
                new_tile.BlocksSight = true;
            }

            if (new_tile.Name != null &&
                new_tile.Name.Contains("Bed") &&
                !new_tile.Name.Contains("RoomType") &&
                !blockName.Contains("Police"))
            {
                AddCharacter(new_tile);
            }

            if (layer.Name == "BottomTiles" ||
                layer.Name == "MiddleTiles")
            {
                if (layer.Name == "MiddleTiles")
                {
                    Handler.MiddleFurniture.Add(new_tile);
                }

                if (new_tile.Inventory != null)
                {
                    new_tile.Inventory.Location = new Location(x, y, 0);
                    InventoryManager.Inventories.Add(new_tile.Inventory);
                }
            }

            if (layer.Name == "TopTiles")
            {
                Handler.TopFurniture.Add(new_tile);
            }

            //Add to world
            layer.Tiles.Add(new_tile);

            //Add to Worldmap
            if (layer.Name != null)
            {
                Layer? map_layer = worldTile.GetLayer(layer.Name);
                map_layer?.Tiles.Add(new_tile);
            }
        }

        private static void AddEmptyTile(Map map, Layer layer, Map worldTile, Location location)
        {
            if (Main.Game == null)
            {
                return;
            }

            Tile new_tile = new()
            {
                ID = Handler.GetID(),
                Map = map,
                Layer = layer,
                World = layer.World,
                Name = "",
                Location = location
            };
            new_tile.Region = new Region((int)new_tile.Location.X * Main.Game.TileSize.X, (int)new_tile.Location.Y * Main.Game.TileSize.Y, Main.Game.TileSize.X, Main.Game.TileSize.Y);

            //Add to world
            layer.Tiles.Add(new_tile);

            //Add to Worldmap
            if (layer.Name != null)
            {
                Layer? map_layer = worldTile.GetLayer(layer.Name);
                map_layer?.Tiles.Add(new_tile);
            }
        }

        private static void UpdateMiddleTiles(Layer middle_tiles)
        {
            for (int i = middle_tiles.Tiles.Count - 1; i >= 0; i--)
            {
                Tile middle_tile = middle_tiles.Tiles[i];
                if (middle_tile.Location == null)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(middle_tile.Name))
                {
                    if (middle_tile.Name.Contains("Tree"))
                    {
                        int x = (int)middle_tile.Location.X + 1;
                        int y = (int)middle_tile.Location.Y + 1;

                        int index = (y * middle_tiles.Columns) + x;
                        Tile existing = middle_tiles.Tiles[index];
                        if (existing?.Location != null)
                        {
                            //Swap tree with tile at X+1/Y+1
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
            if (Main.Game == null)
            {
                return;
            }

            foreach (Tile bottom_tile in bottom_tiles.Tiles)
            {
                if (bottom_tile.Location == null)
                {
                    continue;
                }

                Tile roof_tile = new()
                {
                    Location = new Location(bottom_tile.Location.X, bottom_tile.Location.Y, 0)
                };
                roof_tile.Region = new Region((int)roof_tile.Location.X * Main.Game.TileSize.X, (int)roof_tile.Location.Y * Main.Game.TileSize.Y, Main.Game.TileSize.X, Main.Game.TileSize.Y);
                roof_tiles.Tiles.Add(roof_tile);

                if (bottom_tile.Name != null &&
                    bottom_tile.Name.Contains("Wall"))
                {
                    roof_tile.Texture = Handler.GetTexture("Roof");
                    if (roof_tile.Texture != null)
                    {
                        roof_tile.Image = new Rectangle(0, 0, roof_tile.Texture.Width, roof_tile.Texture.Height);
                        roof_tile.Visible = true;
                    }
                }

                current++;
                Handler.Loading_Percent = (current * 100) / total;
            }

            foreach (Tile middle_tile in middle_tiles.Tiles)
            {
                if (middle_tile.Location == null)
                {
                    continue;
                }

                Tile? roof_tile = roof_tiles.GetTile(middle_tile.Location.ToVector2);
                if (roof_tile != null)
                {
                    if (roof_tile.Texture == null &&
                        middle_tile.Name != null)
                    {
                        if ((middle_tile.Name.Contains("Door") && !WorldUtil.NextToFence(middle_tiles, middle_tile.Location)) ||
                             middle_tile.Name.Contains("Window"))
                        {
                            roof_tile.Texture = Handler.GetTexture("Roof");
                            if (roof_tile.Texture != null)
                            {
                                roof_tile.Image = new Rectangle(0, 0, roof_tile.Texture.Width, roof_tile.Texture.Height);
                                roof_tile.Visible = true;
                            }
                        }
                    }
                }
            }

            foreach (Tile room_tile in room_tiles.Tiles)
            {
                if (room_tile.Location == null)
                {
                    continue;
                }

                Tile? roof_tile = roof_tiles.GetTile(room_tile.Location.ToVector2);
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
                            roof_tile.Texture = Handler.GetTexture("Roof");
                            if (roof_tile.Texture != null)
                            {
                                roof_tile.Image = new Rectangle(0, 0, roof_tile.Texture.Width, roof_tile.Texture.Height);
                                roof_tile.Visible = true;
                            }
                        }
                    }
                }
            }
        }

        private static void AddRooms()
        {
            foreach (Map worldTile in Worldmap)
            {
                if (worldTile.Location == null)
                {
                    continue;
                }

                foreach (Map block in Blocks)
                {
                    if (block.Name == worldTile.Name)
                    {
                        if (BlockRooms.TryGetValue(block.ID, out List<Map>? blockRooms))
                        {
                            World world = new()
                            {
                                ID = worldTile.ID
                            };

                            foreach (Map blockRoom in blockRooms)
                            {
                                Map room = new()
                                {
                                    ID = Handler.GetID(),
                                    Name = blockRoom.Name,
                                    Location = new Location(worldTile.Location.X, worldTile.Location.Y, 0),
                                    World = world
                                };

                                foreach (Layer blockLayer in blockRoom.Layers)
                                {
                                    Layer layer = new()
                                    {
                                        Name = blockLayer.Name,
                                        Map = room,
                                        World = world
                                    };
                                    room.Layers.Add(layer);

                                    foreach (Tile blockTile in blockLayer.Tiles)
                                    {
                                        if (blockTile.Location == null)
                                        {
                                            continue;
                                        }

                                        Tile tile = new()
                                        {
                                            Name = blockTile.Name,
                                            Location = new Location(blockTile.Location.X, blockTile.Location.Y, 0),
                                            Layer = layer,
                                            Map = room,
                                            World = world
                                        };
                                        layer.Tiles.Add(tile);
                                    }
                                }

                                if (Rooms.TryGetValue(worldTile.ID, out List<Map>? rooms))
                                {
                                    rooms.Add(room);
                                }
                                else
                                {
                                    Rooms.Add(worldTile.ID, [room]);
                                }
                            }
                        }

                        break;
                    }
                }
            }
        }

        private static void AssignOwners(Layer layer)
        {
            Army? army = CharacterManager.GetArmy("Characters");
            Squad? squad = army?.GetSquad("Citizens");

            if (squad == null)
            {
                return;
            }

            int char_count = squad.Characters.Count;
            for (int i = 0; i < char_count; i++)
            {
                Character character = squad.Characters[i];
                if (character.Location == null)
                {
                    continue;
                }

                int block_x = (int)character.Location.X / 20;
                int block_y = (int)character.Location.Y / 20;

                List<Tile> tiles = WorldUtil.GetFurniture_All(layer, new Point(block_x, block_y));
                foreach (Tile furniture in tiles)
                {
                    if (furniture.Location != null)
                    {
                        bool addFurniture = false;

                        if (furniture.Name != null &&
                            furniture.Name.Contains("Bed"))
                        {
                            //Don't grant ownership of someone else's bed

                            Location? sleepSpot = null;
                            if (furniture.Direction == Direction.North)
                            {
                                sleepSpot = new Location(furniture.Location.X, furniture.Location.Y + 1);
                            }
                            else if (furniture.Direction == Direction.West)
                            {
                                sleepSpot = new Location(furniture.Location.X + 1, furniture.Location.Y);
                            }
                            else
                            {
                                sleepSpot = new Location(furniture.Location.X, furniture.Location.Y);
                            }

                            if (sleepSpot != null)
                            {
                                if (character.Location.X == sleepSpot.X &&
                                    character.Location.Y == sleepSpot.Y)
                                {
                                    addFurniture = true;
                                }
                            }
                        }
                        else
                        {
                            addFurniture = true;
                        }

                        if (addFurniture)
                        {
                            if (!Handler.OwnedFurniture.TryGetValue(character.ID, out List<Tile>? value))
                            {
                                Handler.OwnedFurniture.Add(character.ID, [furniture]);
                            }
                            else
                            {
                                value.Add(furniture);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        private static void AddCharacter(Tile bed)
        {
            if (Main.Game == null ||
                bed.Location == null ||
                bed.Region == null)
            {
                return;
            }

            Squad? citizens = CharacterManager.GetArmy("Characters")?.GetSquad("Citizens");
            if (citizens?.Characters.Count < Handler.MaxPop)
            {
                Character? character = CharacterUtil.GenCharacter(last_name);
                if (character != null)
                {
                    character.Direction = bed.Direction;

                    if (bed.Direction == Direction.North)
                    {
                        character.FaceNorth();
                    }
                    else if (bed.Direction == Direction.East)
                    {
                        character.FaceEast();
                    }
                    else if (bed.Direction == Direction.South)
                    {
                        character.FaceSouth();
                    }
                    else if (bed.Direction == Direction.West)
                    {
                        character.FaceWest();
                    }

                    if (bed.Direction == Direction.North)
                    {
                        character.Location = new Location(bed.Location.X, bed.Location.Y + 1);
                        character.Region = new Region(bed.Region.X, bed.Region.Y + Main.Game.TileSize.Y, Main.Game.TileSize.X, Main.Game.TileSize.Y);
                    }
                    else if (bed.Direction == Direction.West)
                    {
                        character.Location = new Location(bed.Location.X + 1, bed.Location.Y);
                        character.Region = new Region(bed.Region.X + Main.Game.TileSize.X, bed.Region.Y, Main.Game.TileSize.X, Main.Game.TileSize.Y);
                    }
                    else
                    {
                        character.Location = new Location(bed.Location.X, bed.Location.Y);
                        character.Region = new Region(bed.Region.X, bed.Region.Y, Main.Game.TileSize.X, Main.Game.TileSize.Y);
                    }

                    CharacterUtil.UpdateGear(character);

                    citizens.Characters.Add(character);
                }
            }
        }

        private static void AssignJobs()
        {
            Handler.LoadJobs();

            List<Job> OpenJobs = [];

            int jobCount = Handler.Jobs.Count;
            for (int i = 0; i < jobCount; i++)
            {
                Job job = Handler.Jobs[i];
                if (job.Name == null)
                {
                    continue;
                }

                OpenJobs.Add(job);

                string[] jobNameParts = job.Name.Split('_');
                
                string place = jobNameParts[1];

                Layer? middle_tiles = null;
                Layer? top_tiles = null;

                if (place == "Police")
                {
                    middle_tiles = Police?.GetLayer("MiddleTiles");
                    top_tiles = Police?.GetLayer("TopTiles");
                }
                else if (place == "Grocery")
                {
                    middle_tiles = Grocery?.GetLayer("MiddleTiles");
                    top_tiles = Grocery?.GetLayer("TopTiles");
                }
                else if (place == "Diner")
                {
                    middle_tiles = Diner?.GetLayer("MiddleTiles");
                    top_tiles = Diner?.GetLayer("TopTiles");
                }

                for (int jt = 0; jt < job.Tasks.Count; jt++)
                {
                    JobTask task = job.Tasks[jt];

                    if (task.Name == "Cashier")
                    {
                        if (top_tiles == null)
                        {
                            continue;
                        }

                        int tileCount = top_tiles.Tiles.Count;
                        for (int t = 0; t < tileCount; t++)
                        {
                            Tile tile = top_tiles.Tiles[t];
                            if (tile.Location == null ||
                                tile.Name == null)
                            {
                                continue;
                            }

                            if (tile.Name.Contains("Register"))
                            {
                                task.Owner_Tile = tile;

                                if (tile.Direction == Direction.North)
                                {
                                    task.Location = new Location(tile.Location.X, tile.Location.Y - 1);
                                    task.Direction = Direction.South;
                                }
                                else if (tile.Direction == Direction.East)
                                {
                                    task.Location = new Location(tile.Location.X + 1, tile.Location.Y);
                                    task.Direction = Direction.West;
                                }
                                else if (tile.Direction == Direction.South)
                                {
                                    task.Location = new Location(tile.Location.X, tile.Location.Y + 1);
                                    task.Direction = Direction.North;
                                }
                                else if (tile.Direction == Direction.West)
                                {
                                    task.Location = new Location(tile.Location.X - 1, tile.Location.Y);
                                    task.Direction = Direction.East;
                                }

                                break;
                            }
                        }
                    }
                }
            }

            Squad? citizens = CharacterManager.GetArmy("Characters")?.GetSquad("Citizens");
            if (citizens == null)
            {
                return;
            }

            CryptoRandom random;

            int count = citizens.Characters.Count;
            for (int i = 0; i < count; i++)
            {
                if (OpenJobs.Count == 0)
                {
                    break;
                }

                random = new();
                int job_choice = random.Next(0, OpenJobs.Count);
                Job openJob = OpenJobs[job_choice];

                Character character = citizens.Characters[i];
                character.Job.ID = openJob.ID;
                character.Job.Name = openJob.Name;

                for (int j = 0; j < jobCount; j++)
                {
                    Job job = Handler.Jobs[j];
                    if (job.ID == openJob.ID)
                    {
                        job.Owner_Character = character;

                        for (int t = 0; t < job.Tasks.Count; t++)
                        {
                            JobTask task = job.Tasks[t];
                            task.Owner_Character = character;
                        }
                        break;
                    }
                }

                OpenJobs.Remove(openJob);
            }
        }

        public static void GenLoot()
        {
            Handler.Loading_Percent = 0;
            Handler.Loading_Message = "Generating loot...";

            Map? map = WorldUtil.GetMap();

            Layer? room_tiles = map?.GetLayer("RoomTiles");
            if (room_tiles != null)
            {
                Layer? middle_tiles = map?.GetLayer("MiddleTiles");
                if (middle_tiles != null)
                {
                    foreach (Tile middle_tile in middle_tiles.Tiles)
                    {
                        current++;
                        Handler.Loading_Percent = (current * 100) / total;

                        if (middle_tile.Location != null &&
                            middle_tile.Texture != null)
                        {
                            Tile? room_tile = room_tiles.GetTile(middle_tile.Location.ToVector2);
                            if (room_tile != null)
                            {
                                if (!string.IsNullOrEmpty(room_tile.Name))
                                {
                                    string? category = InventoryUtil.GetCategory_FromTile(room_tile);
                                    if (!string.IsNullOrEmpty(category) &&
                                        category != "Outdoors")
                                    {
                                        if (middle_tile.Inventory?.Name != null)
                                        {
                                            string container = middle_tile.Inventory.Name;
                                            if (!string.IsNullOrEmpty(container))
                                            {
                                                List<Item> loot = InventoryUtil.GenLoot(category, container, (int)middle_tile.Inventory.Max_Value);
                                                foreach (Item item in loot)
                                                {
                                                    if (item.Name == null)
                                                    {
                                                        continue;
                                                    }

                                                    middle_tile.Inventory.Items.Add(item);

                                                    if (item.Type == "Container")
                                                    {
                                                        if (!InventoryManager.Inventories.Contains(item.Inventory))
                                                        {
                                                            InventoryManager.Inventories.Add(item.Inventory);
                                                        }

                                                        List<Item> item_loot = InventoryUtil.GenLoot(category, item.Name, (int)item.Inventory.Max_Value);
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
        }

        #endregion
    }
}

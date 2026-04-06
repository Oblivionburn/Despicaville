using System.IO;
using System.Xml;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OP_Engine.Scenes;
using OP_Engine.Menus;
using OP_Engine.Tiles;
using OP_Engine.Inputs;
using OP_Engine.Controls;
using OP_Engine.Utility;
using OP_Engine.Enums;
using Despicaville.Util;

namespace Despicaville.Menus
{
    public class Menu_MapEditor : Menu
    {
        #region Variables

        string current_file = "";
        bool convert_coords;

        bool TileToggle;
        bool RemovingTiles;

        Layer BottomTiles = new Layer();
        Layer MiddleTiles = new Layer();
        Layer TopTiles = new Layer();
        Layer RoomTiles = new Layer();

        bool Selecting_Layer;
        string[] Layers = { "Bottom", "Middle", "Top", "Room" };
        List<Button> Buttons_Layer = new List<Button>();
        
        bool Selecting_RoomType;
        List<string> RoomTypes;
        List<Button> Buttons_RoomType = new List<Button>();

        bool Selecting_MapType;
        string[] MapTypes = { "Residential", "Commercial", "Road", "Park" };
        List<Button> Buttons_MapType = new List<Button>();

        bool Selecting_MapFacing;
        string[] MapFacings = { "North", "East", "South", "West" };
        List<Button> Buttons_MapFacing = new List<Button>();

        Tile SelectedTile;

        int Tiles_TopY;
        int Tiles_BottomY;
        List<Tile> Tiles = new List<Tile>();

        int Furniture_TopY;
        int Furniture_BottomY;
        List<Tile> Furniture = new List<Tile>();

        Stream SaveStream;
        XmlWriter Writer;

        #endregion

        #region Constructor

        public Menu_MapEditor(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "MapEditor";
            Load(content);
        }

        #endregion

        #region Methods

        public override void Update(Game gameRef, ContentManager content)
        {
            if (Visible ||
                Active)
            {
                UpdateControls();

                foreach (Button button in Buttons_Layer)
                {
                    button.Update();
                }

                foreach (Button button in Buttons_RoomType)
                {
                    button.Update();
                }

                foreach (Button button in Buttons_MapType)
                {
                    button.Update();
                }

                foreach (Button button in Buttons_MapFacing)
                {
                    button.Update();
                }

                base.Update(gameRef, content);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
                foreach (Picture picture in Pictures)
                {
                    if (picture.Name != "Selected" &&
                        picture.Name != "Highlight")
                    {
                        picture.Draw(spriteBatch);
                    }
                }

                foreach (Tile tile in BottomTiles.Tiles)
                {
                    tile.Draw(spriteBatch, Main.Game.Resolution);
                }

                switch (GetButton("Layer").Text)
                {
                    case "Middle":
                        foreach (Tile tile in MiddleTiles.Tiles)
                        {
                            tile.Draw(spriteBatch, Main.Game.Resolution);
                        }
                        break;

                    case "Top":
                        foreach (Tile tile in MiddleTiles.Tiles)
                        {
                            tile.Draw(spriteBatch, Main.Game.Resolution);
                        }

                        foreach (Tile tile in TopTiles.Tiles)
                        {
                            tile.Draw(spriteBatch, Main.Game.Resolution);
                        }
                        break;

                    case "Room":
                        foreach (Tile tile in MiddleTiles.Tiles)
                        {
                            tile.Draw(spriteBatch, Main.Game.Resolution);
                        }

                        foreach (Tile tile in RoomTiles.Tiles)
                        {
                            tile.Draw(spriteBatch, Main.Game.Resolution);
                        }
                        break;
                }

                foreach (Tile tile in Tiles)
                {
                    if (tile.Visible)
                    {
                        tile.Draw(spriteBatch, Main.Game.Resolution);
                    }
                }

                foreach (Tile tile in Furniture)
                {
                    if (tile.Visible)
                    {
                        tile.Draw(spriteBatch, Main.Game.Resolution);
                    }
                }

                foreach (Picture picture in Pictures)
                {
                    if (picture.Name == "Selected")
                    {
                        picture.Draw(spriteBatch);
                        break;
                    }
                }

                foreach (Picture picture in Pictures)
                {
                    if (picture.Name == "Highlight")
                    {
                        picture.Draw(spriteBatch);
                        break;
                    }
                }

                foreach (Button button in Buttons)
                {
                    button.Draw(spriteBatch);
                }

                foreach (Label label in Labels)
                {
                    if (label.Name != "Examine")
                    {
                        label.Draw(spriteBatch);
                    }
                }

                foreach (Button button in Buttons_Layer)
                {
                    button.Draw(spriteBatch);
                }

                foreach (Button button in Buttons_RoomType)
                {
                    button.Draw(spriteBatch);
                }

                foreach (Button button in Buttons_MapType)
                {
                    button.Draw(spriteBatch);
                }

                foreach (Button button in Buttons_MapFacing)
                {
                    button.Draw(spriteBatch);
                }

                foreach (Label label in Labels)
                {
                    if (label.Name == "Examine")
                    {
                        label.Draw(spriteBatch);
                        break;
                    }
                }
            }
        }

        private void UpdateControls()
        {
            bool hoveringTile = false;
            if (!Selecting_Layer &&
                !Selecting_RoomType &&
                !Selecting_MapType &&
                !Selecting_MapFacing)
            {
                hoveringTile = HoveringTile();
            }

            bool hoveringFurniture = false;
            if (!Selecting_Layer &&
                !Selecting_RoomType &&
                !Selecting_MapType &&
                !Selecting_MapFacing)
            {
                hoveringFurniture = HoveringFurniture();
            }

            bool hoveringMapTile = false;
            if (!Selecting_Layer &&
                !Selecting_RoomType &&
                !Selecting_MapType &&
                !Selecting_MapFacing)
            {
                hoveringMapTile = HoveringMapTile();
            }

            bool hoveringButton = false;
            if (!Selecting_Layer &&
                !Selecting_RoomType &&
                !Selecting_MapType &&
                !Selecting_MapFacing)
            {
                hoveringButton = HoveringButton();
            }

            bool hoveringButtonLayer = HoveringButton_Layer();
            if (!hoveringButtonLayer &&
                Selecting_Layer)
            {
                if (InputManager.Mouse_LB_Pressed)
                {
                    HideLayers();
                }
            }

            bool hoveringButtonRoomType = HoveringButton_RoomType();
            if (!hoveringButtonRoomType &&
                Selecting_RoomType)
            {
                if (InputManager.Mouse_LB_Pressed)
                {
                    HideRoomTypes();
                }
            }

            bool hoveringButtonMapType = HoveringButton_MapType();
            if (!hoveringButtonMapType &&
                Selecting_MapType)
            {
                if (InputManager.Mouse_LB_Pressed)
                {
                    HideMapTypes();
                }
            }

            bool hoveringButtonMapFacing = HoveringButton_MapFacing();
            if (!hoveringButtonMapFacing &&
                Selecting_MapFacing)
            {
                if (InputManager.Mouse_LB_Pressed)
                {
                    HideMapFacings();
                }
            }

            if (!hoveringButton)
            { 
                GetLabel("Examine").Visible = false;
            }

            if (!hoveringTile &&
                !hoveringFurniture &&
                !hoveringMapTile)
            {
                GetPicture("Highlight").Visible = false;
            }

            Picture tileWindow = GetPicture("TileWindow");
            if (InputManager.MouseWithin(tileWindow.Region.ToRectangle))
            {
                if (InputManager.Mouse_ScrolledUp)
                {
                    if (Tiles_TopY > 0)
                    {
                        Tiles_TopY--;

                        if (Tiles_TopY == 0)
                        {
                            GetPicture("TileWindow_ArrowUp").Visible = false;
                        }
                        GetPicture("TileWindow_ArrowDown").Visible = true;

                        ScrollTiles_Up(tileWindow);
                    }
                }
                else if (InputManager.Mouse_ScrolledDown)
                {
                    if (Tiles_TopY < Tiles_BottomY)
                    {
                        Tiles_TopY++;

                        if (Tiles_TopY == Tiles_BottomY)
                        {
                            GetPicture("TileWindow_ArrowDown").Visible = false;
                        }
                        GetPicture("TileWindow_ArrowUp").Visible = true;

                        ScrollTiles_Down(tileWindow);
                    }
                }
            }

            Picture objectWindow = GetPicture("ObjectWindow");
            if (InputManager.MouseWithin(objectWindow.Region.ToRectangle))
            {
                if (InputManager.Mouse_ScrolledUp)
                {
                    if (Furniture_TopY > 0)
                    {
                        Furniture_TopY--;

                        if (Furniture_TopY == 0)
                        {
                            GetPicture("ObjectWindow_ArrowUp").Visible = false;
                        }
                        GetPicture("ObjectWindow_ArrowDown").Visible = true;

                        ScrollFurniture_Up(objectWindow);
                    }
                }
                else if (InputManager.Mouse_ScrolledDown)
                {
                    if (Furniture_TopY < Furniture_BottomY)
                    {
                        Furniture_TopY++;

                        if (Furniture_TopY == Furniture_BottomY)
                        {
                            GetPicture("ObjectWindow_ArrowDown").Visible = false;
                        }
                        GetPicture("ObjectWindow_ArrowUp").Visible = true;

                        ScrollFurniture_Down(objectWindow);
                    }
                }
            }
        }

        private bool HoveringButton()
        {
            foreach (Button button in Buttons)
            {
                button.Opacity = 0.8f;
                button.Selected = false;
            }

            foreach (Button button in Buttons)
            {
                if (button.Visible &&
                    button.Enabled)
                {
                    if (InputManager.MouseWithin(button.Region.ToRectangle))
                    {
                        if (button.HoverText != null)
                        {
                            GameUtil.Examine(this, button.HoverText);
                        }

                        button.Opacity = 1;
                        button.Selected = true;

                        if (InputManager.Mouse_LB_Pressed)
                        {
                            CheckClick(button);

                            button.Opacity = 0.8f;
                            button.Selected = false;
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        private bool HoveringButton_Layer()
        {
            foreach (Button button in Buttons_Layer)
            {
                button.Opacity = 0.8f;
                button.Selected = false;
            }

            foreach (Button button in Buttons_Layer)
            {
                if (button.Visible &&
                    button.Enabled)
                {
                    if (InputManager.MouseWithin(button.Region.ToRectangle))
                    {
                        if (button.HoverText != null)
                        {
                            GameUtil.Examine(this, button.HoverText);
                        }

                        button.Opacity = 1;
                        button.Selected = true;

                        if (InputManager.Mouse_LB_Pressed)
                        {
                            CheckClick_Layer(button);

                            button.Opacity = 0.8f;
                            button.Selected = false;
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        private bool HoveringButton_RoomType()
        {
            foreach (Button button in Buttons_RoomType)
            {
                button.Opacity = 0.8f;
                button.Selected = false;
            }

            foreach (Button button in Buttons_RoomType)
            {
                if (button.Visible &&
                    button.Enabled)
                {
                    if (InputManager.MouseWithin(button.Region.ToRectangle))
                    {
                        if (button.HoverText != null)
                        {
                            GameUtil.Examine(this, button.HoverText);
                        }

                        button.Opacity = 1;
                        button.Selected = true;

                        if (InputManager.Mouse_LB_Pressed)
                        {
                            CheckClick_RoomType(button);

                            button.Opacity = 0.8f;
                            button.Selected = false;
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        private bool HoveringButton_MapType()
        {
            foreach (Button button in Buttons_MapType)
            {
                button.Opacity = 0.8f;
                button.Selected = false;
            }

            foreach (Button button in Buttons_MapType)
            {
                if (button.Visible &&
                    button.Enabled)
                {
                    if (InputManager.MouseWithin(button.Region.ToRectangle))
                    {
                        if (button.HoverText != null)
                        {
                            GameUtil.Examine(this, button.HoverText);
                        }

                        button.Opacity = 1;
                        button.Selected = true;

                        if (InputManager.Mouse_LB_Pressed)
                        {
                            CheckClick_MapType(button);

                            button.Opacity = 0.8f;
                            button.Selected = false;
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        private bool HoveringButton_MapFacing()
        {
            foreach (Button button in Buttons_MapFacing)
            {
                button.Opacity = 0.8f;
                button.Selected = false;
            }

            foreach (Button button in Buttons_MapFacing)
            {
                if (button.Visible &&
                    button.Enabled)
                {
                    if (InputManager.MouseWithin(button.Region.ToRectangle))
                    {
                        if (button.HoverText != null)
                        {
                            GameUtil.Examine(this, button.HoverText);
                        }

                        button.Opacity = 1;
                        button.Selected = true;

                        if (InputManager.Mouse_LB_Pressed)
                        {
                            CheckClick_MapFacing(button);

                            button.Opacity = 0.8f;
                            button.Selected = false;
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        private bool HoveringTile()
        {
            foreach (Tile tile in Tiles)
            {
                if (tile.Visible)
                {
                    if (InputManager.MouseWithin(tile.Region.ToRectangle))
                    {
                        Picture highlight = GetPicture("Highlight");
                        highlight.Region = tile.Region;
                        highlight.Visible = true;

                        if (InputManager.Mouse_LB_Pressed)
                        {
                            AssetManager.PlaySound_Random("Click");
                            InputManager.Mouse.Flush();

                            SelectedTile = tile;

                            Picture selected = GetPicture("Selected");
                            selected.Region = tile.Region;
                            selected.Visible = true;
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        private bool HoveringFurniture()
        {
            foreach (Tile tile in Furniture)
            {
                if (tile.Visible)
                {
                    if (InputManager.MouseWithin(tile.Region.ToRectangle))
                    {
                        Picture highlight = GetPicture("Highlight");
                        highlight.Region = tile.Region;
                        highlight.Visible = true;

                        if (InputManager.Mouse_LB_Pressed)
                        {
                            AssetManager.PlaySound_Random("Click");
                            InputManager.Mouse.Flush();

                            SelectedTile = tile;

                            Picture selected = GetPicture("Selected");
                            selected.Region = tile.Region;
                            selected.Visible = true;
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        private bool HoveringMapTile()
        {
            Button layer = GetButton("Layer");
            Tile tile = null;

            switch (layer.Text)
            {
                case "Bottom":
                    foreach (Tile existing in BottomTiles.Tiles)
                    {
                        if (InputManager.MouseWithin(existing.Region.ToRectangle))
                        {
                            tile = existing;
                            break;
                        }
                    }
                    break;

                case "Middle":
                    foreach (Tile existing in MiddleTiles.Tiles)
                    {
                        if (InputManager.MouseWithin(existing.Region.ToRectangle))
                        {
                            tile = existing;
                            break;
                        }
                    }
                    break;

                case "Top":
                    foreach (Tile existing in TopTiles.Tiles)
                    {
                        if (InputManager.MouseWithin(existing.Region.ToRectangle))
                        {
                            tile = existing;
                            break;
                        }
                    }
                    break;

                case "Room":
                    foreach (Tile existing in RoomTiles.Tiles)
                    {
                        if (InputManager.MouseWithin(existing.Region.ToRectangle))
                        {
                            tile = existing;
                            break;
                        }
                    }
                    break;
            }
            
            if (tile != null)
            {
                Picture highlight = GetPicture("Highlight");
                highlight.Region = tile.Region;
                highlight.Visible = true;

                if (InputManager.Mouse_LB_Held)
                {
                    if (!TileToggle &&
                        layer.Text != "Bottom")
                    {
                        TileToggle = true;
                
                        if (!RemovingTiles &&
                            tile.Texture != null)
                        {
                            RemovingTiles = true;
                        }
                        else if (RemovingTiles &&
                                 tile.Texture == null)
                        {
                            RemovingTiles = false;
                        }
                    }
                
                    ToggleTile(tile, layer.Text);
                }
                else if (InputManager.Mouse_LB_Pressed)
                {
                    InputManager.Mouse.Flush();
                    TileToggle = false;
                }

                return true;
            }

            return false;
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");
            InputManager.Mouse.Flush();

            if (button.Name == "Exit")
            {
                Close();
            }
            else if (button.Name == "New")
            {
                NewMap();
            }
            else if (button.Name == "Open")
            {
                OpenMap();
            }
            else if (button.Name == "Save")
            {
                SaveMap();
            }
            else if (button.Name == "Layer")
            {
                if (!Selecting_Layer)
                {
                    Selecting_Layer = true;
                    ShowLayers(button);
                }
                else
                {
                    HideLayers();
                }
            }
            else if (button.Name == "RoomType")
            {
                if (!Selecting_RoomType)
                {
                    Selecting_RoomType = true;
                    ShowRoomTypes(button);
                }
                else
                {
                    HideRoomTypes();
                }
            }
            else if (button.Name == "MapType")
            {
                if (!Selecting_MapType)
                {
                    Selecting_MapType = true;
                    ShowMapTypes(button);
                }
                else
                {
                    HideMapTypes();
                }
            }
            else if (button.Name == "MapFacing")
            {
                if (!Selecting_MapFacing)
                {
                    Selecting_MapFacing = true;
                    ShowMapFacings(button);
                }
                else
                {
                    HideMapFacings();
                }
            }
        }

        private void CheckClick_Layer(Button button)
        {
            AssetManager.PlaySound_Random("Click");
            InputManager.Mouse.Flush();

            Button layer = GetButton("Layer");
            layer.Text = button.Text;
            layer.Enabled = true;

            if (button.Text == "Room")
            {
                GetLabel("RoomType").Visible = true;
                GetButton("RoomType").Visible = true;
            }
            else
            {
                GetLabel("RoomType").Visible = false;
                GetButton("RoomType").Visible = false;
            }

            HideLayers();
        }

        private void CheckClick_RoomType(Button button)
        {
            AssetManager.PlaySound_Random("Click");
            InputManager.Mouse.Flush();

            Button roomType = GetButton("RoomType");
            roomType.Text = button.Text;
            roomType.Enabled = true;

            HideRoomTypes();
        }

        private void CheckClick_MapType(Button button)
        {
            AssetManager.PlaySound_Random("Click");
            InputManager.Mouse.Flush();

            Button mapType = GetButton("MapType");
            mapType.Text = button.Text;
            mapType.Enabled = true;

            if (button.Text == "Residential" ||
                button.Text == "Commercial")
            {
                GetLabel("MapFacing").Visible = true;
                GetButton("MapFacing").Visible = true;
            }
            else
            {
                GetLabel("MapFacing").Visible = false;
                GetButton("MapFacing").Visible = false;
            }

            HideMapTypes();
        }

        private void CheckClick_MapFacing(Button button)
        {
            AssetManager.PlaySound_Random("Click");
            InputManager.Mouse.Flush();

            Button mapFacing = GetButton("MapFacing");
            mapFacing.Text = button.Text;
            mapFacing.Enabled = true;

            HideMapFacings();
        }

        private void ToggleTile(Tile tile, string layer)
        {
            Picture mapWindow = GetPicture("MapWindow");
            float width = mapWindow.Region.Width / 20;
            float height = mapWindow.Region.Height / 20;

            if (RemovingTiles)
            {
                if (tile.Texture != null)
                {
                    tile.Name = "";
                    tile.Texture = null;
                    tile.Region = new Region(tile.Region.X, tile.Region.Y, width, height);
                }
            }
            else
            {
                bool okay = false;

                if (layer == "Bottom")
                {
                    if (SelectedTile?.Type == "Tile")
                    {
                        okay = true;
                    }
                }
                else if (SelectedTile?.Type == "Furniture")
                {
                    okay = true;
                }
                else if (layer == "Room")
                {
                    okay = true;
                }

                if (okay)
                {
                    if (layer != "Room")
                    {
                        tile.Name = SelectedTile?.Name;
                        tile.Texture = SelectedTile?.Texture;
                    }
                    else
                    {
                        string name = "RoomType_" + GetButton("RoomType").Text;
                        tile.Name = name;
                        tile.Texture = AssetManager.Textures[name];
                    }

                    tile.Image = new Rectangle(0, 0, tile.Texture.Width, tile.Texture.Height);

                    if (SelectedTile != null)
                    {
                        tile.Region = new Region(tile.Region.X, tile.Region.Y, width * SelectedTile.Dimensions.Width, height * SelectedTile.Dimensions.Height);
                    }
                }
            }
        }

        public override void Close()
        {
            Visible = false;
            Active = false;

            SceneManager.GetScene("Title").Menu.GetPicture("Title").Visible = true;

            Menu main = MenuManager.GetMenu("Main");
            main.Active = true;
            main.Visible = true;
        }

        private void NewMap()
        {
            GenMap();

            GetButton("Layer").Text = "Bottom";
            GetButton("MapType").Text = "Park";

            Button roomType = GetButton("RoomType");
            roomType.Text = "Bathroom";
            roomType.Visible = false;
            
            Button mapFacing = GetButton("MapFacing");
            mapFacing.Text = "North";
            mapFacing.Visible = false;

            current_file = "";
            GetLabel("MapFile").Text = "Map File: New";
        }

        private void OpenMap()
        {
            System.Windows.Forms.OpenFileDialog openFile = new System.Windows.Forms.OpenFileDialog();
            openFile.InitialDirectory = AssetManager.Directories["Maps"];
            openFile.Filter = "Despicaville Map | *.blockmap";
            openFile.DefaultExt = ".blockmap";
            openFile.Title = "Open Despicaville Map";
            openFile.FileName = "";

            if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                BottomTiles = new Layer();
                MiddleTiles = new Layer();
                TopTiles = new Layer();
                RoomTiles = new Layer();

                ParseTiles(openFile.FileName);

                current_file = Path.GetFileNameWithoutExtension(openFile.FileName);
                GetLabel("MapFile").Text = "Map File: " + current_file;
            }
        }

        private void SaveMap()
        {
            string fileName = "";

            System.Windows.Forms.SaveFileDialog saveFile = new System.Windows.Forms.SaveFileDialog();
            saveFile.InitialDirectory = AssetManager.Directories["Maps"];
            saveFile.Filter = "Despicaville Map | *.blockmap";
            saveFile.DefaultExt = ".blockmap";
            saveFile.Title = "Save Despicaville Map";
            saveFile.FileName = current_file + ".blockmap";

            if (saveFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fileName = saveFile.FileName;
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                WriteStream(fileName);

                if (BottomTiles.Tiles.Count > 0)
                {
                    EnterNode("Map");

                    EnterNode("Globals");
                    Writer.WriteAttributeString("Type", GetButton("MapType").Text);
                    Writer.WriteAttributeString("Direction", GetButton("MapFacing").Text);
                    ExitNode();

                    EnterNode("BottomTiles");
                    foreach (Tile tile in BottomTiles.Tiles)
                    {
                        EnterNode("BottomTile");
                        Writer.WriteAttributeString("Texture", tile.Name);
                        Writer.WriteAttributeString("Location", tile.Location.X.ToString() + "," + tile.Location.Y.ToString());
                        ExitNode();
                    }
                    ExitNode();

                    EnterNode("MiddleTiles");
                    foreach (Tile tile in MiddleTiles.Tiles)
                    {
                        EnterNode("MiddleTile");
                        Writer.WriteAttributeString("Texture", tile.Name);
                        Writer.WriteAttributeString("Location", tile.Location.X.ToString() + "," + tile.Location.Y.ToString());
                        ExitNode();
                    }
                    ExitNode();

                    EnterNode("TopTiles");
                    foreach (Tile tile in TopTiles.Tiles)
                    {
                        EnterNode("TopTile");
                        Writer.WriteAttributeString("Texture", tile.Name);
                        Writer.WriteAttributeString("Location", tile.Location.X.ToString() + "," + tile.Location.Y.ToString());
                        ExitNode();
                    }
                    ExitNode();

                    EnterNode("RoomTiles");
                    foreach (Tile tile in RoomTiles.Tiles)
                    {
                        EnterNode("RoomTile");
                        Writer.WriteAttributeString("Texture", tile.Name);
                        Writer.WriteAttributeString("Location", tile.Location.X.ToString() + "," + tile.Location.Y.ToString());
                        ExitNode();
                    }
                    ExitNode();

                    Map rooms = new Map
                    {
                        Name = "Rooms"
                    };

                    int roomCount = RoomTiles.Tiles.Count;
                    for (int i = 0; i < roomCount; i++)
                    {
                        Tile tile = RoomTiles.Tiles[i];
                        if (!string.IsNullOrEmpty(tile.Name))
                        {
                            bool hasRoom = HasRoom(rooms, tile);
                            if (!hasRoom)
                            {
                                Layer room = new Layer
                                {
                                    Name = tile.Name,
                                    Columns = 20,
                                    Rows = 20
                                };

                                GetRoomTiles(RoomTiles, room, tile);
                                rooms.Layers.Add(room);
                            }
                        }
                    }

                    if (rooms.Layers.Count > 0)
                    {
                        EnterNode("Rooms");
                        foreach (Layer layer in rooms.Layers)
                        {
                            EnterNode("Room");

                            EnterNode("RoomProperties");
                            Writer.WriteAttributeString("Name", layer.Name);
                            ExitNode();

                            EnterNode("Tiles");
                            foreach (Tile tile in layer.Tiles)
                            {
                                EnterNode("Tile");
                                Writer.WriteAttributeString("Texture", tile.Name);
                                Writer.WriteAttributeString("Location", tile.Location.X.ToString() + "," + tile.Location.Y.ToString());
                                ExitNode();
                            }
                            ExitNode();

                            List<Tile> exits = GetExits(layer, BottomTiles, MiddleTiles, RoomTiles);
                            if (exits.Count > 0)
                            {
                                EnterNode("Exits");
                                foreach (Tile tile in exits)
                                {
                                    EnterNode("Exit");
                                    Writer.WriteAttributeString("Location", tile.Location.X.ToString() + "," + tile.Location.Y.ToString());
                                    ExitNode();
                                }
                                ExitNode();
                            }

                            ExitNode();
                        }
                        ExitNode();
                    }
                }

                FinalizeWriting();
            }
        }

        private void ShowLayers(Button layer)
        {
            layer.Enabled = false;

            Texture2D frame = AssetManager.Textures["ButtonFrame"];

            for (int i = 0; i < Layers.Length; i++)
            {
                Buttons_Layer.Add(new Button
                {
                    ID = Handler.GetID(),
                    Font = AssetManager.Fonts["ControlFont"],
                    Name = "Layer_" + Layers[i],
                    Texture = frame,
                    Texture_Highlight = AssetManager.Textures["ButtonFrame_Highlight"],
                    Image = new Rectangle(0, 0, frame.Width, frame.Height),
                    Region = new Region(layer.Region.X, layer.Region.Y + (layer.Region.Height * (i + 1)), layer.Region.Width, layer.Region.Height),
                    DrawColor = Color.White,
                    DrawColor_Selected = Color.White,
                    Text = Layers[i],
                    TextColor = Color.White,
                    TextColor_Selected = Color.Red,
                    Enabled = true,
                    Visible = true
                });
            }
        }

        private void HideLayers()
        {
            InputManager.Mouse.Flush();
            GetButton("Layer").Enabled = true;
            Buttons_Layer.Clear();
            Selecting_Layer = false;
        }

        private void ShowRoomTypes(Button roomType)
        {
            roomType.Enabled = false;

            Texture2D frame = AssetManager.Textures["ButtonFrame"];

            for (int i = 0; i < RoomTypes.Count; i++)
            {
                Buttons_RoomType.Add(new Button
                {
                    ID = Handler.GetID(),
                    Font = AssetManager.Fonts["ControlFont"],
                    Name = "RoomType_" + RoomTypes[i],
                    Texture = frame,
                    Texture_Highlight = AssetManager.Textures["ButtonFrame_Highlight"],
                    Image = new Rectangle(0, 0, frame.Width, frame.Height),
                    Region = new Region(roomType.Region.X, roomType.Region.Y + (roomType.Region.Height * (i + 1)), roomType.Region.Width, roomType.Region.Height),
                    DrawColor = Color.White,
                    DrawColor_Selected = Color.White,
                    Text = RoomTypes[i],
                    TextColor = Color.White,
                    TextColor_Selected = Color.Red,
                    Enabled = true,
                    Visible = true
                });
            }
        }

        private void HideRoomTypes()
        {
            InputManager.Mouse.Flush();
            GetButton("RoomType").Enabled = true;
            Buttons_RoomType.Clear();
            Selecting_RoomType = false;
        }

        private void ShowMapTypes(Button mapType)
        {
            mapType.Enabled = false;

            Texture2D frame = AssetManager.Textures["ButtonFrame"];

            for (int i = 0; i < MapTypes.Length; i++)
            {
                Buttons_MapType.Add(new Button
                {
                    ID = Handler.GetID(),
                    Font = AssetManager.Fonts["ControlFont"],
                    Name = "MapType_" + MapTypes[i],
                    Texture = frame,
                    Texture_Highlight = AssetManager.Textures["ButtonFrame_Highlight"],
                    Image = new Rectangle(0, 0, frame.Width, frame.Height),
                    Region = new Region(mapType.Region.X, mapType.Region.Y + (mapType.Region.Height * (i + 1)), mapType.Region.Width, mapType.Region.Height),
                    DrawColor = Color.White,
                    DrawColor_Selected = Color.White,
                    Text = MapTypes[i],
                    TextColor = Color.White,
                    TextColor_Selected = Color.Red,
                    Enabled = true,
                    Visible = true
                });
            }
        }

        private void HideMapTypes()
        {
            InputManager.Mouse.Flush();
            GetButton("MapType").Enabled = true;
            Buttons_MapType.Clear();
            Selecting_MapType = false;
        }

        private void ShowMapFacings(Button mapFacing)
        {
            mapFacing.Enabled = false;

            Texture2D frame = AssetManager.Textures["ButtonFrame"];

            for (int i = 0; i < MapFacings.Length; i++)
            {
                Buttons_MapFacing.Add(new Button
                {
                    ID = Handler.GetID(),
                    Font = AssetManager.Fonts["ControlFont"],
                    Name = "MapFacing_" + MapFacings[i],
                    Texture = frame,
                    Texture_Highlight = AssetManager.Textures["ButtonFrame_Highlight"],
                    Image = new Rectangle(0, 0, frame.Width, frame.Height),
                    Region = new Region(mapFacing.Region.X, mapFacing.Region.Y + (mapFacing.Region.Height * (i + 1)), mapFacing.Region.Width, mapFacing.Region.Height),
                    DrawColor = Color.White,
                    DrawColor_Selected = Color.White,
                    Text = MapFacings[i],
                    TextColor = Color.White,
                    TextColor_Selected = Color.Red,
                    Enabled = true,
                    Visible = true
                });
            }
        }

        private void HideMapFacings()
        {
            InputManager.Mouse.Flush();
            GetButton("MapFacing").Enabled = true;
            Buttons_MapFacing.Clear();
            Selecting_MapFacing = false;
        }

        private void ScrollTiles_Up(Picture tileWindow)
        {
            Picture selected = GetPicture("Selected");

            float width = tileWindow.Region.Width / 10;

            int count = Tiles.Count;
            for (int i = 0; i < count; i++)
            {
                Tile tile = Tiles[i];

                tile.Region.Y += width;

                if (tile.Region.Y < tileWindow.Region.Y ||
                    tile.Region.X + width > tileWindow.Region.X + tileWindow.Region.Width ||
                    tile.Region.Y + width > tileWindow.Region.Y + tileWindow.Region.Height)
                {
                    tile.Visible = false;
                }
                else
                {
                    tile.Visible = true;
                }

                if (selected.Region.X == tile.Region.X &&
                    selected.Region.Y == tile.Region.Y)
                {
                    selected.Visible = tile.Visible;
                }
            }
        }

        private void ScrollTiles_Down(Picture tileWindow)
        {
            Picture selected = GetPicture("Selected");

            float width = tileWindow.Region.Width / 10;

            int count = Tiles.Count;
            for (int i = 0; i < count; i++)
            {
                Tile tile = Tiles[i];

                tile.Region.Y -= width;

                if (tile.Region.Y < tileWindow.Region.Y ||
                    tile.Region.X + width > tileWindow.Region.X + tileWindow.Region.Width ||
                    tile.Region.Y + width > tileWindow.Region.Y + tileWindow.Region.Height)
                {
                    tile.Visible = false;
                }
                else
                {
                    tile.Visible = true;
                }

                if (selected.Region.X == tile.Region.X &&
                    selected.Region.Y == tile.Region.Y)
                {
                    selected.Visible = tile.Visible;
                }
            }
        }

        private void ScrollFurniture_Up(Picture objectWindow)
        {
            Picture selected = GetPicture("Selected");

            float width = objectWindow.Region.Width / 10;

            int count = Furniture.Count;
            for (int i = 0; i < count; i++)
            {
                Tile tile = Furniture[i];

                tile.Region.Y += width;

                if (tile.Region.Y < objectWindow.Region.Y ||
                    tile.Region.X + tile.Region.Width > objectWindow.Region.X + objectWindow.Region.Width ||
                    tile.Region.Y + tile.Region.Height > objectWindow.Region.Y + objectWindow.Region.Height)
                {
                    tile.Visible = false;
                }
                else
                {
                    tile.Visible = true;
                }

                if (selected.Region.X == tile.Region.X &&
                    selected.Region.Y == tile.Region.Y)
                {
                    selected.Visible = tile.Visible;
                }
            }
        }

        private void ScrollFurniture_Down(Picture objectWindow)
        {
            Picture selected = GetPicture("Selected");

            float width = objectWindow.Region.Width / 10;

            int count = Furniture.Count;
            for (int i = 0; i < count; i++)
            {
                Tile tile = Furniture[i];

                tile.Region.Y -= width;

                if (tile.Region.Y < objectWindow.Region.Y ||
                    tile.Region.X + tile.Region.Width > objectWindow.Region.X + objectWindow.Region.Width ||
                    tile.Region.Y + tile.Region.Height > objectWindow.Region.Y + objectWindow.Region.Height)
                {
                    tile.Visible = false;
                }
                else
                {
                    tile.Visible = true;
                }

                if (selected.Region.X == tile.Region.X &&
                    selected.Region.Y == tile.Region.Y)
                {
                    selected.Visible = tile.Visible;
                }
            }
        }

        private void GenMap()
        {
            Picture mapWindow = GetPicture("MapWindow");
            float X = mapWindow.Region.X;
            float Y = mapWindow.Region.Y;
            float width = mapWindow.Region.Width / 20;
            float height = mapWindow.Region.Height / 20;

            BottomTiles = new Layer
            {
                Name = "BottomTiles",
                Columns = 20,
                Rows = 20
            };
            MiddleTiles = new Layer
            {
                Name = "MiddleTiles",
                Columns = 20,
                Rows = 20
            };
            TopTiles = new Layer
            {
                Name = "TopTiles",
                Columns = 20,
                Rows = 20
            };
            RoomTiles = new Layer
            {
                Name = "RoomTiles",
                Columns = 20,
                Rows = 20
            };

            Texture2D texture = AssetManager.Textures["Grass"];

            for (int y = 0; y < 20; y++)
            {
                for (int x = 0; x < 20; x++)
                {
                    BottomTiles.Tiles.Add(new Tile
                    {
                        Name = "Grass",
                        Location = new Location(x, y, 0),
                        Texture = texture,
                        Image = new Rectangle(0, 0, texture.Width, texture.Height),
                        Region = new Region(X + (x * width), Y + (y * height), width, height),
                        DrawColor = Color.White,
                        Visible = true
                    });

                    MiddleTiles.Tiles.Add(new Tile
                    {
                        Location = new Location(x, y, 0),
                        Region = new Region(X + (x * width), Y + (y * height), width, height),
                        DrawColor = Color.White,
                        Visible = true
                    });

                    TopTiles.Tiles.Add(new Tile
                    {
                        Location = new Location(x, y, 0),
                        Region = new Region(X + (x * width), Y + (y * height), width, height),
                        DrawColor = Color.White,
                        Visible = true
                    });

                    RoomTiles.Tiles.Add(new Tile
                    {
                        Location = new Location(x, y, 0),
                        Region = new Region(X + (x * width), Y + (y * height), width, height),
                        DrawColor = Color.White,
                        Visible = true
                    });
                }
            }
        }

        public override void Load()
        {
            DirectoryInfo dir = new DirectoryInfo(AssetManager.Directories["Textures"]);

            NewMap();
            LoadTiles(dir);
            LoadFurniture(dir);
            LoadRoomTypes(dir);
        }

        private void LoadTiles(DirectoryInfo TexturesDir)
        {
            Picture tileWindow = GetPicture("TileWindow");

            int x = 0;
            int y = 0;
            float X = tileWindow.Region.X;
            float Y = tileWindow.Region.Y;
            float tileWidth = tileWindow.Region.Width / 10;

            foreach (DirectoryInfo sub_dir in TexturesDir.GetDirectories())
            {
                if (sub_dir.Name == "Tiles")
                {
                    foreach (FileInfo file in sub_dir.GetFiles("*.png"))
                    {
                        string name = Path.GetFileNameWithoutExtension(file.FullName);
                        Texture2D texture = AssetManager.Textures[name];

                        Tile tile = new Tile
                        {
                            Name = name,
                            Type = "Tile",
                            Location = new Location(x, y, 0),
                            Texture = texture,
                            Image = new Rectangle(0, 0, texture.Width, texture.Height),
                            Region = new Region(X, Y, tileWidth, tileWidth),
                            Dimensions = new Dimension3(1, 1, 0),
                            DrawColor = Color.White,
                            Visible = true
                        };

                        if (Y + tileWidth > tileWindow.Region.Y + tileWindow.Region.Height)
                        {
                            tile.Visible = false;
                        }

                        Tiles.Add(tile);

                        x++;
                        X += tileWidth;
                        if (X + tileWidth > tileWindow.Region.X + tileWindow.Region.Width)
                        {
                            x = 0;
                            X = tileWindow.Region.X;

                            y++;
                            Y += tileWidth;
                        }
                    }

                    break;
                }
            }

            Tiles_BottomY = y - 9;
            if (Tiles_BottomY < 0)
            {
                GetPicture("TileWindow_ArrowDown").Visible = false;
            }
        }

        private void LoadFurniture(DirectoryInfo TexturesDir)
        {
            Picture objectWindow = GetPicture("ObjectWindow");

            int loc_x = 0;
            int loc_y = 0;
            float X = objectWindow.Region.X;
            float Y = objectWindow.Region.Y;
            float tileWidth = objectWindow.Region.Width / 10;

            List<string> Files = new List<string>();

            foreach (DirectoryInfo sub_dir in TexturesDir.GetDirectories())
            {
                if (sub_dir.Name == "Furniture" ||
                    sub_dir.Name == "Foliage")
                {
                    FileInfo[] files = sub_dir.GetFiles("*.png");
                    for (int i = 0; i < files.Length; i++)
                    {
                        FileInfo file = files[i];
                        string name = Path.GetFileNameWithoutExtension(file.FullName);
                        Files.Add(name);
                    }
                }
            }

            for (int i = 0; i < Files.Count; i++)
            {
                string name = Files[i];
                Texture2D texture = AssetManager.Textures[name];

                if (!name.Contains("Used"))
                {
                    float width_scale = 1;
                    if (texture.Width > 128)
                    {
                        width_scale = texture.Width / 128;
                    }

                    float height_scale = 1;
                    if (texture.Height > 128)
                    {
                        height_scale = texture.Height / 128;
                    }

                    if (i > 0)
                    {
                        loc_x++;
                        X += tileWidth;
                        if (X + tileWidth > objectWindow.Region.X + objectWindow.Region.Width)
                        {
                            loc_x = 0;
                            X = objectWindow.Region.X;

                            loc_y++;
                            Y += tileWidth;
                        }
                    }

                    Region region = new Region(X, Y, tileWidth * width_scale, tileWidth * height_scale);

                    bool okay = false;
                    while (!okay)
                    {
                        okay = true;

                        if (region.X >= objectWindow.Region.X + objectWindow.Region.Width ||
                            region.X + region.Width > objectWindow.Region.X + objectWindow.Region.Width)
                        {
                            okay = false;
                        }

                        foreach (Tile tile in Furniture)
                        {
                            for (int y = (int)region.Y; y < region.Y + region.Width; y++)
                            {
                                for (int x = (int)region.X; x < region.X + region.Width; x++)
                                {
                                    if (x >= tile.Region.X && x < tile.Region.X + tile.Region.Width &&
                                        y >= tile.Region.Y && y < tile.Region.Y + tile.Region.Height)
                                    {
                                        okay = false;
                                        break;
                                    }
                                }

                                if (!okay)
                                {
                                    break;
                                }
                            }
                        }

                        if (!okay)
                        {
                            loc_x++;
                            X += tileWidth;
                            if (X + tileWidth > objectWindow.Region.X + objectWindow.Region.Width)
                            {
                                loc_x = 0;
                                X = objectWindow.Region.X;

                                loc_y++;
                                Y += tileWidth;
                            }

                            region = new Region(X, Y, tileWidth * width_scale, tileWidth * height_scale);
                        }
                    }

                    if (okay)
                    {
                        Tile tile = new Tile
                        {
                            Name = name,
                            Type = "Furniture",
                            Location = new Location(loc_x, loc_y, 0),
                            Texture = texture,
                            Image = new Rectangle(0, 0, texture.Width, texture.Height),
                            Region = region,
                            Dimensions = new Dimension3((int)width_scale, (int)height_scale, 0),
                            DrawColor = Color.White,
                            Visible = true
                        };

                        if (tile.Name.Contains("Wall") ||
                            tile.Name.Contains("Fence") ||
                            tile.Name.Contains("Fridge") ||
                            tile.Name.Contains("Lamp") ||
                            tile.Name.Contains("StreetLight") ||
                            tile.Name.Contains("TV") ||
                            tile.Name.Contains("Tree") ||
                            tile.Name.Contains("Counter") ||
                            tile.Name.Contains("Table") ||
                            tile.Name.Contains("Stove") ||
                            tile.Name.Contains("Door"))
                        {
                            tile.BlocksMovement = true;
                        }

                        if (Y + tileWidth > objectWindow.Region.Y + objectWindow.Region.Height)
                        {
                            tile.Visible = false;
                        }

                        Furniture.Add(tile);
                    }
                }
            }

            Furniture_BottomY = loc_y - 7;
            if (Furniture_BottomY < 0)
            {
                GetPicture("ObjectWindow_ArrowDown").Visible = false;
            }
        }

        private void LoadRoomTypes(DirectoryInfo TexturesDir)
        {
            RoomTypes = new List<string>();

            foreach (DirectoryInfo sub_dir in TexturesDir.GetDirectories())
            {
                if (sub_dir.Name == "RoomTypes")
                {
                    FileInfo[] files = sub_dir.GetFiles("*.png");
                    for (int i = 0; i < files.Length; i++)
                    {
                        FileInfo file = files[i];
                        string name = Path.GetFileNameWithoutExtension(file.FullName);

                        string[] nameParts = name.Split('_');
                        RoomTypes.Add(nameParts[1]);
                    }

                    break;
                }
            }
        }

        public override void Load(ContentManager content)
        {
            Clear();

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                font = AssetManager.Fonts["ControlFont"],
                name = "New",
                texture = AssetManager.Textures["ButtonFrame"],
                texture_highlight = AssetManager.Textures["ButtonFrame_Highlight"],
                draw_color = Color.White,
                draw_color_selected = Color.White,
                text = "New",
                text_color = Color.White,
                text_selected_color = Color.Red,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                font = AssetManager.Fonts["ControlFont"],
                name = "Open",
                texture = AssetManager.Textures["ButtonFrame"],
                texture_highlight = AssetManager.Textures["ButtonFrame_Highlight"],
                draw_color = Color.White,
                draw_color_selected = Color.White,
                text = "Open",
                text_color = Color.White,
                text_selected_color = Color.Red,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                font = AssetManager.Fonts["ControlFont"],
                name = "Save",
                texture = AssetManager.Textures["ButtonFrame"],
                texture_highlight = AssetManager.Textures["ButtonFrame_Highlight"],
                draw_color = Color.White,
                draw_color_selected = Color.White,
                text = "Save",
                text_color = Color.White,
                text_selected_color = Color.Red,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                font = AssetManager.Fonts["ControlFont"],
                name = "Exit",
                texture = AssetManager.Textures["ButtonFrame"],
                texture_highlight = AssetManager.Textures["ButtonFrame_Highlight"],
                draw_color = Color.White,
                draw_color_selected = Color.White,
                text = "Exit",
                text_color = Color.White,
                text_selected_color = Color.Red,
                enabled = true,
                visible = true
            });

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Layer", "Layer:", Color.White, new Region(0, 0, 0, 0), true);
            GetLabel("Layer").Alignment_Horizontal = Alignment.Right;
            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                font = AssetManager.Fonts["ControlFont"],
                name = "Layer",
                texture = AssetManager.Textures["ButtonFrame"],
                texture_highlight = AssetManager.Textures["ButtonFrame_Highlight"],
                texture_disabled = AssetManager.Textures["ButtonFrame_Highlight"],
                draw_color = Color.White,
                draw_color_selected = Color.White,
                draw_color_disabled = new Color(32, 32, 32),
                text = "Bottom",
                text_color = Color.White,
                text_selected_color = Color.Red,
                text_disabled_color = new Color(32, 32, 32),
                enabled = true,
                visible = true
            });

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "RoomType", "Room Type:", Color.White, new Region(0, 0, 0, 0), false);
            GetLabel("RoomType").Alignment_Horizontal = Alignment.Right;
            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                font = AssetManager.Fonts["ControlFont"],
                name = "RoomType",
                texture = AssetManager.Textures["ButtonFrame"],
                texture_highlight = AssetManager.Textures["ButtonFrame_Highlight"],
                texture_disabled = AssetManager.Textures["ButtonFrame_Highlight"],
                draw_color = Color.White,
                draw_color_selected = Color.White,
                draw_color_disabled = new Color(32, 32, 32),
                text = "Bathroom",
                text_color = Color.White,
                text_selected_color = Color.Red,
                text_disabled_color = new Color(32, 32, 32),
                enabled = true,
                visible = false
            });

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "MapType", "Map Type:", Color.White, new Region(0, 0, 0, 0), true);
            GetLabel("MapType").Alignment_Horizontal = Alignment.Right;
            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                font = AssetManager.Fonts["ControlFont"],
                name = "MapType",
                texture = AssetManager.Textures["ButtonFrame"],
                texture_highlight = AssetManager.Textures["ButtonFrame_Highlight"],
                texture_disabled = AssetManager.Textures["ButtonFrame_Highlight"],
                draw_color = Color.White,
                draw_color_selected = Color.White,
                draw_color_disabled = new Color(32, 32, 32),
                text = "Park",
                text_color = Color.White,
                text_selected_color = Color.Red,
                text_disabled_color = new Color(32, 32, 32),
                enabled = true,
                visible = true
            });

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "MapFacing", "Map Facing:", Color.White, new Region(0, 0, 0, 0), false);
            GetLabel("MapFacing").Alignment_Horizontal = Alignment.Right;
            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                font = AssetManager.Fonts["ControlFont"],
                name = "MapFacing",
                texture = AssetManager.Textures["ButtonFrame"],
                texture_highlight = AssetManager.Textures["ButtonFrame_Highlight"],
                texture_disabled = AssetManager.Textures["ButtonFrame_Highlight"],
                draw_color = Color.White,
                draw_color_selected = Color.White,
                draw_color_disabled = new Color(32, 32, 32),
                text = "North",
                text_color = Color.White,
                text_selected_color = Color.Red,
                text_disabled_color = new Color(32, 32, 32),
                enabled = true,
                visible = false
            });

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Tiles", "Tiles:", Color.White, new Region(0, 0, 0, 0), true);
            GetLabel("Tiles").Alignment_Horizontal = Alignment.Left;
            AddPicture(Handler.GetID(), "TileWindow", AssetManager.Textures["Frame_Full"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "TileWindow_ArrowDown", AssetManager.Textures["ArrowIcon_Down"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "TileWindow_ArrowUp", AssetManager.Textures["ArrowIcon_Up"], new Region(0, 0, 0, 0), Color.White, false);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Objects", "Objects:", Color.White, new Region(0, 0, 0, 0), true);
            GetLabel("Objects").Alignment_Horizontal = Alignment.Left;
            AddPicture(Handler.GetID(), "ObjectWindow", AssetManager.Textures["Frame_Full"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "ObjectWindow_ArrowDown", AssetManager.Textures["ArrowIcon_Down"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "ObjectWindow_ArrowUp", AssetManager.Textures["ArrowIcon_Up"], new Region(0, 0, 0, 0), Color.White, false);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "MapFile", "Map File: New", Color.White, new Region(0, 0, 0, 0), true);
            GetLabel("MapFile").Alignment_Horizontal = Alignment.Left;
            AddPicture(Handler.GetID(), "MapWindow", AssetManager.Textures["Frame_Full"], new Region(0, 0, 0, 0), Color.White, true);

            AddPicture(Handler.GetID(), "Highlight", AssetManager.Textures["Grid_Hover"], new Region(0, 0, 0, 0), Color.White, false);
            AddPicture(Handler.GetID(), "Selected", AssetManager.Textures["Selection"], new Region(0, 0, 0, 0), Color.White, false);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["Frame"], new Region(0, 0, 0, 0), false);

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            float buttonHeight = Main.Game.MenuSize_Y / 2;

            GetButton("New").Region = new Region(0, 0, Main.Game.MenuSize_X * 2, buttonHeight);
            GetButton("Open").Region = new Region(Main.Game.MenuSize_X * 2, 0, Main.Game.MenuSize_X * 2, buttonHeight);
            GetButton("Save").Region = new Region(Main.Game.MenuSize_X * 4, 0, Main.Game.MenuSize_X * 2, buttonHeight);
            GetButton("Exit").Region = new Region(Main.Game.MenuSize_X * 6, 0, Main.Game.MenuSize_X * 2, buttonHeight);

            GetLabel("Layer").Region = new Region(Main.Game.MenuSize_X * 9, 0, Main.Game.MenuSize_X * 2, buttonHeight);
            GetButton("Layer").Region = new Region(Main.Game.MenuSize_X * 11, 0, Main.Game.MenuSize_X * 3, buttonHeight);

            GetLabel("RoomType").Region = new Region(Main.Game.MenuSize_X * 15, 0, Main.Game.MenuSize_X * 2, buttonHeight);
            GetButton("RoomType").Region = new Region(Main.Game.MenuSize_X * 17, 0, Main.Game.MenuSize_X * 3, buttonHeight);

            GetLabel("MapType").Region = new Region(Main.Game.MenuSize_X * 21, 0, Main.Game.MenuSize_X * 2, buttonHeight);
            GetButton("MapType").Region = new Region(Main.Game.MenuSize_X * 23, 0, Main.Game.MenuSize_X * 3, buttonHeight);

            GetLabel("MapFacing").Region = new Region(Main.Game.MenuSize_X * 27, 0, Main.Game.MenuSize_X * 2, buttonHeight);
            GetButton("MapFacing").Region = new Region(Main.Game.MenuSize_X * 29, 0, Main.Game.MenuSize_X * 3, buttonHeight);

            float Y = Main.Game.MenuSize_Y;
            int windowHeight = (int)(((Main.Game.ScreenHeight - Main.Game.MenuSize_Y) / 2) - Main.Game.MenuSize_Y);
            for (int i = 0; i < windowHeight; i++)
            {
                if (windowHeight % 10 != 0)
                {
                    windowHeight--;
                }
                else
                {
                    break;
                }
            }

            int tileWidth = windowHeight / 10;

            GetLabel("Tiles").Region = new Region(0, Y, Main.Game.MenuSize_X * 4, buttonHeight);

            Picture tileWindow = GetPicture("TileWindow");
            tileWindow.Region = new Region(0, Y + buttonHeight, windowHeight, windowHeight);

            GetPicture("TileWindow_ArrowUp").Region = new Region(tileWindow.Region.X + tileWindow.Region.Width, tileWindow.Region.Y, tileWidth, tileWidth);
            GetPicture("TileWindow_ArrowDown").Region = new Region(tileWindow.Region.X + tileWindow.Region.Width, tileWindow.Region.Y + tileWindow.Region.Height - tileWidth, tileWidth, tileWidth);

            Y = Main.Game.MenuSize_Y + windowHeight + (buttonHeight * 2);
            GetLabel("Objects").Region = new Region(0, Y, Main.Game.MenuSize_X * 4, buttonHeight);

            Picture objectWindow = GetPicture("ObjectWindow");
            objectWindow.Region = new Region(0, Y + buttonHeight, windowHeight, windowHeight);

            GetPicture("ObjectWindow_ArrowUp").Region = new Region(objectWindow.Region.X + objectWindow.Region.Width, objectWindow.Region.Y, tileWidth, tileWidth);
            GetPicture("ObjectWindow_ArrowDown").Region = new Region(objectWindow.Region.X + objectWindow.Region.Width, objectWindow.Region.Y + objectWindow.Region.Height - tileWidth, tileWidth, tileWidth);

            GetLabel("MapFile").Region = new Region(Main.Game.MenuSize_X * 10, Main.Game.MenuSize_Y, Main.Game.MenuSize_X * 4, buttonHeight);

            int mapHeight = (int)(Main.Game.ScreenHeight - Main.Game.MenuSize_Y - buttonHeight);
            for (int i = 0; i < mapHeight; i++)
            {
                if (mapHeight % 20 != 0)
                {
                    mapHeight--;
                }
                else
                {
                    break;
                }
            }
            GetPicture("MapWindow").Region = new Region(Main.Game.MenuSize_X * 10, Main.Game.MenuSize_Y + buttonHeight, mapHeight, mapHeight);
        }

        #endregion

        #region XML Methods

        private void WriteStream(string path)
        {
            SaveStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.IndentChars = "\t";
            Writer = XmlWriter.Create(SaveStream, xmlWriterSettings);
            Writer.WriteStartDocument();
        }

        private void EnterNode(string elementName)
        {
            Writer.WriteStartElement(elementName);
        }

        private void ExitNode()
        {
            Writer.WriteEndElement();
        }

        private void FinalizeWriting()
        {
            Writer.WriteEndDocument();
            Writer.Close();
            SaveStream.Close();
        }

        private void ParseTiles(string file)
        {
            using (XmlTextReader reader = new XmlTextReader(File.OpenRead(file)))
            {
                while (reader.Read())
                {
                    switch (reader.Name)
                    {
                        case "Map":
                            VisitMap(reader);
                            break;
                    }
                }
            }
        }

        private void VisitMap(XmlTextReader reader)
        {
            while (reader.Read())
            {
                if (reader.Name == "Map" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "Globals":
                        VisitMapGlobals(reader);
                        break;

                    case "BottomTiles":
                        GenMap();
                        VisitBottomTiles(reader);
                        break;

                    case "MiddleTiles":
                        VisitMiddleTiles(reader);
                        break;

                    case "TopTiles":
                        VisitTopTiles(reader);
                        break;

                    case "RoomTiles":
                        VisitRoomTiles(reader);
                        break;
                }
            }
        }

        private void VisitMapGlobals(XmlTextReader reader)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "Type":
                        convert_coords = false;
                        GetButton("MapType").Text = reader.Value;
                        break;

                    case "Name":
                        convert_coords = true;
                        break;

                    case "Direction":
                        GetButton("MapFacing").Text = reader.Value;
                        break;
                }
            }
        }

        private void VisitBottomTiles(XmlTextReader reader)
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
                        break;
                }
            }
        }

        private void VisitBottomTile(XmlTextReader reader, Tile tile)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "Texture":
                        tile.Name = reader.Value;

                        if (!string.IsNullOrEmpty(tile.Name))
                        {
                            tile.Texture = AssetManager.Textures[reader.Value];
                            tile.Image = new Rectangle(0, 0, tile.Texture.Width, tile.Texture.Height);
                        }
                        
                        tile.DrawColor = Color.White;
                        break;

                    case "Location":
                        var parts = reader.Value.Split(',');
                        float x = float.Parse(parts[0]);
                        float y = float.Parse(parts[1]);

                        if (convert_coords)
                        {
                            int converted_x = (int)((x / 32) - 5);
                            int converted_y = (int)((y - 80) / 32);

                            tile.Location = new Location(converted_x, converted_y, 0);
                        }
                        else
                        {
                            tile.Location = new Location(x, y, 0);
                        }

                        Picture mapWindow = GetPicture("MapWindow");
                        float X = mapWindow.Region.X;
                        float Y = mapWindow.Region.Y;
                        float width = mapWindow.Region.Width / 20;
                        float height = mapWindow.Region.Height / 20;

                        tile.Region = new Region(X + (tile.Location.X * width), Y + (tile.Location.Y * height), width, height);
                        tile.Visible = true;

                        for (int i = 0; i < BottomTiles.Tiles.Count; i++)
                        {
                            Tile existing = BottomTiles.Tiles[i];
                            if (existing.Location.X == tile.Location.X &&
                                existing.Location.Y == tile.Location.Y)
                            {
                                BottomTiles.Tiles[i] = tile;
                                break;
                            }
                        }
                        break;
                }
            }
        }

        private void VisitMiddleTiles(XmlTextReader reader)
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
                        VisitMiddleTile(reader, tile);
                        break;
                }
            }
        }

        private void VisitMiddleTile(XmlTextReader reader, Tile tile)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "Texture":
                        tile.Name = reader.Value;

                        if (!string.IsNullOrEmpty(tile.Name))
                        {
                            tile.Texture = AssetManager.Textures[reader.Value];
                            tile.Image = new Rectangle(0, 0, tile.Texture.Width, tile.Texture.Height);

                            if (tile.Name.Contains("Wall") ||
                                tile.Name.Contains("Fence") ||
                                tile.Name.Contains("Fridge") ||
                                tile.Name.Contains("Lamp") ||
                                tile.Name.Contains("StreetLight") ||
                                tile.Name.Contains("TV") ||
                                tile.Name.Contains("Tree") ||
                                tile.Name.Contains("Counter") ||
                                tile.Name.Contains("Table") ||
                                tile.Name.Contains("Stove") ||
                                tile.Name.Contains("Door"))
                            {
                                tile.BlocksMovement = true;
                            }
                        }

                        tile.DrawColor = Color.White;
                        break;

                    case "Location":
                        var parts = reader.Value.Split(',');
                        float x = float.Parse(parts[0]);
                        float y = float.Parse(parts[1]);

                        if (convert_coords)
                        {
                            int converted_x = (int)((x / 32) - 5);
                            int converted_y = (int)((y - 80) / 32);

                            tile.Location = new Location(converted_x, converted_y, 0);
                        }
                        else
                        {
                            tile.Location = new Location(x, y, 0);
                        }

                        Picture mapWindow = GetPicture("MapWindow");
                        float X = mapWindow.Region.X;
                        float Y = mapWindow.Region.Y;
                        float width = mapWindow.Region.Width / 20;
                        float height = mapWindow.Region.Height / 20;

                        tile.Region = new Region(X + (tile.Location.X * width), Y + (tile.Location.Y * height), width, height);
                        tile.Visible = true;

                        for (int i = 0; i < Furniture.Count; i++)
                        {
                            Tile furniture = Furniture[i];
                            if (furniture.Name == tile.Name)
                            {
                                tile.Region = new Region(tile.Region.X, tile.Region.Y, width * furniture.Dimensions.Width, height * furniture.Dimensions.Height);
                                break;
                            }
                        }

                        for (int i = 0; i < MiddleTiles.Tiles.Count; i++)
                        {
                            Tile existing = MiddleTiles.Tiles[i];
                            if (existing.Location.X == tile.Location.X &&
                                existing.Location.Y == tile.Location.Y)
                            {
                                MiddleTiles.Tiles[i] = tile;
                                break;
                            }
                        }
                        break;
                }
            }
        }

        private void VisitTopTiles(XmlTextReader reader)
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
                        VisitTopTile(reader, tile);
                        break;
                }
            }
        }

        private void VisitTopTile(XmlTextReader reader, Tile tile)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "Texture":
                        tile.Name = reader.Value;

                        if (!string.IsNullOrEmpty(tile.Name))
                        {
                            tile.Texture = AssetManager.Textures[reader.Value];
                            tile.Image = new Rectangle(0, 0, tile.Texture.Width, tile.Texture.Height);
                        }

                        tile.DrawColor = Color.White;
                        break;

                    case "Location":
                        var parts = reader.Value.Split(',');
                        float x = float.Parse(parts[0]);
                        float y = float.Parse(parts[1]);

                        if (convert_coords)
                        {
                            int converted_x = (int)((x / 32) - 5);
                            int converted_y = (int)((y - 80) / 32);

                            tile.Location = new Location(converted_x, converted_y, 0);
                        }
                        else
                        {
                            tile.Location = new Location(x, y, 0);
                        }

                        Picture mapWindow = GetPicture("MapWindow");
                        float X = mapWindow.Region.X;
                        float Y = mapWindow.Region.Y;
                        float width = mapWindow.Region.Width / 20;
                        float height = mapWindow.Region.Height / 20;

                        tile.Region = new Region(X + (tile.Location.X * width), Y + (tile.Location.Y * height), width, height);
                        tile.Visible = true;

                        for (int i = 0; i < TopTiles.Tiles.Count; i++)
                        {
                            Tile existing = TopTiles.Tiles[i];
                            if (existing.Location.X == tile.Location.X &&
                                existing.Location.Y == tile.Location.Y)
                            {
                                TopTiles.Tiles[i] = tile;
                                break;
                            }
                        }
                        break;
                }
            }
        }

        private void VisitRoomTiles(XmlTextReader reader)
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
                        break;
                }
            }
        }

        private void VisitRoomTile(XmlTextReader reader, Tile tile)
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
                            tile.Texture = AssetManager.Textures[reader.Value];
                            tile.Image = new Rectangle(0, 0, tile.Texture.Width, tile.Texture.Height);
                        }

                        tile.DrawColor = Color.White;
                        break;

                    case "Location":
                        var parts = reader.Value.Split(',');
                        float x = float.Parse(parts[0]);
                        float y = float.Parse(parts[1]);

                        if (convert_coords)
                        {
                            int converted_x = (int)((x / 32) - 5);
                            int converted_y = (int)((y - 80) / 32);

                            tile.Location = new Location(converted_x, converted_y, 0);
                        }
                        else
                        {
                            tile.Location = new Location(x, y, 0);
                        }

                        Picture mapWindow = GetPicture("MapWindow");
                        float X = mapWindow.Region.X;
                        float Y = mapWindow.Region.Y;
                        float width = mapWindow.Region.Width / 20;
                        float height = mapWindow.Region.Height / 20;

                        tile.Region = new Region(X + (tile.Location.X * width), Y + (tile.Location.Y * height), width, height);
                        tile.Visible = true;

                        for (int i = 0; i < RoomTiles.Tiles.Count; i++)
                        {
                            Tile existing = RoomTiles.Tiles[i];
                            if (existing.Location.X == tile.Location.X &&
                                existing.Location.Y == tile.Location.Y)
                            {
                                RoomTiles.Tiles[i] = tile;
                                break;
                            }
                        }
                        break;
                }
            }
        }

        #endregion

        #region Rooms Methods

        private bool HasRoom(Map rooms, Tile tile)
        {
            int layerCount = rooms.Layers.Count;
            for (int l = 0; l < layerCount; l++)
            {
                Layer layer = rooms.Layers[l];

                int tileCount = layer.Tiles.Count;
                for (int t = 0; t < tileCount; t++)
                {
                    Tile existing = layer.Tiles[t];

                    if (existing.Name == tile.Name &&
                        existing.Location.X == tile.Location.X &&
                        existing.Location.Y == tile.Location.Y)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool HasTile(Layer room, Tile tile)
        {
            int tileCount = room.Tiles.Count;
            for (int t = 0; t < tileCount; t++)
            {
                Tile existing = room.Tiles[t];

                if (existing.Location.X == tile.Location.X &&
                    existing.Location.Y == tile.Location.Y)
                {
                    return true;
                }
            }

            return false;
        }

        private void GetRoomTiles(Layer room_tiles, Layer room, Tile room_tile)
        {
            room.Tiles.Add(room_tile);

            Tile north = room_tiles.GetTile(new Vector2(room_tile.Location.X, room_tile.Location.Y - 1));
            if (north != null &&
                !string.IsNullOrEmpty(north.Name) &&
                north.Name == room_tile.Name)
            {
                bool hasTile = HasTile(room, north);
                if (!hasTile)
                {
                    GetRoomTiles(room_tiles, room, north);
                }
            }

            Tile east = room_tiles.GetTile(new Vector2(room_tile.Location.X + 1, room_tile.Location.Y));
            if (east != null &&
                !string.IsNullOrEmpty(east.Name) &&
                east.Name == room_tile.Name &&
                !HasTile(room, east))
            {
                bool hasTile = HasTile(room, east);
                if (!hasTile)
                {
                    GetRoomTiles(room_tiles, room, east);
                }
            }

            Tile south = room_tiles.GetTile(new Vector2(room_tile.Location.X, room_tile.Location.Y + 1));
            if (south != null &&
                !string.IsNullOrEmpty(south.Name) &&
                south.Name == room_tile.Name &&
                !HasTile(room, south))
            {
                bool hasTile = HasTile(room, south);
                if (!hasTile)
                {
                    GetRoomTiles(room_tiles, room, south);
                }
            }

            Tile west = room_tiles.GetTile(new Vector2(room_tile.Location.X - 1, room_tile.Location.Y));
            if (west != null &&
                !string.IsNullOrEmpty(west.Name) &&
                west.Name == room_tile.Name &&
                !HasTile(room, west))
            {
                bool hasTile = HasTile(room, west);
                if (!hasTile)
                {
                    GetRoomTiles(room_tiles, room, west);
                }
            }
        }

        private List<Tile> GetExits(Layer room, Layer bottom_tiles, Layer middle_tiles, Layer room_tiles)
        {
            Layer exits = new Layer();

            if (room != null)
            {
                foreach (Tile tile in room.Tiles)
                {
                    Tile middle = middle_tiles.GetTile(new Vector2(tile.Location.X, tile.Location.Y));
                    if (!middle.BlocksMovement)
                    {
                        Vector3 north = new Vector3(tile.Location.X, tile.Location.Y - 1, 0);

                        Tile north_bottom = bottom_tiles.GetTile(north);
                        if (north_bottom != null &&
                            !north_bottom.Name.Contains("Wall"))
                        {
                            Tile north_middle = middle_tiles.GetTile(north);
                            if (north_middle.Name.Contains("Door"))
                            {
                                bool hasTile = HasTile(exits, north_middle);
                                if (!hasTile)
                                {
                                    exits.Tiles.Add(north_middle);
                                }
                            }
                            else
                            {
                                Tile north_room = room_tiles.GetTile(north);
                                if (north_room != null &&
                                    !string.IsNullOrEmpty(north_room.Name) &&
                                    north_room.Name != tile.Name &&
                                    !north_middle.BlocksMovement)
                                {
                                    bool hasTile = HasTile(exits, north_room);
                                    if (!hasTile)
                                    {
                                        exits.Tiles.Add(north_room);
                                    }
                                }
                            }
                        }

                        Vector3 east = new Vector3(tile.Location.X + 1, tile.Location.Y, 0);

                        Tile east_bottom = bottom_tiles.GetTile(east);
                        if (east_bottom != null &&
                            !east_bottom.Name.Contains("Wall"))
                        {
                            Tile east_middle = middle_tiles.GetTile(east);
                            if (east_middle.Name.Contains("Door"))
                            {
                                bool hasTile = HasTile(exits, east_middle);
                                if (!hasTile)
                                {
                                    exits.Tiles.Add(east_middle);
                                }
                            }
                            else
                            {
                                Tile east_room = room_tiles.GetTile(east);
                                if (east_room != null &&
                                    !string.IsNullOrEmpty(east_room.Name) &&
                                    east_room.Name != tile.Name &&
                                    !east_middle.BlocksMovement)
                                {
                                    bool hasTile = HasTile(exits, east_room);
                                    if (!hasTile)
                                    {
                                        exits.Tiles.Add(east_room);
                                    }
                                }
                            }
                        }

                        Vector3 south = new Vector3(tile.Location.X, tile.Location.Y + 1, 0);

                        Tile south_bottom = bottom_tiles.GetTile(south);
                        if (south_bottom != null &&
                            !south_bottom.Name.Contains("Wall"))
                        {
                            Tile south_middle = middle_tiles.GetTile(south);
                            if (south_middle.Name.Contains("Door"))
                            {
                                bool hasTile = HasTile(exits, south_middle);
                                if (!hasTile)
                                {
                                    exits.Tiles.Add(south_middle);
                                }
                            }
                            else
                            {
                                Tile south_room = room_tiles.GetTile(south);
                                if (south_room != null &&
                                    !string.IsNullOrEmpty(south_room.Name) &&
                                    south_room.Name != tile.Name &&
                                    !south_middle.BlocksMovement)
                                {
                                    bool hasTile = HasTile(exits, south_room);
                                    if (!hasTile)
                                    {
                                        exits.Tiles.Add(south_room);
                                    }
                                }
                            }
                        }

                        Vector3 west = new Vector3(tile.Location.X - 1, tile.Location.Y, 0);

                        Tile west_bottom = bottom_tiles.GetTile(west);
                        if (west_bottom != null &&
                            !west_bottom.Name.Contains("Wall"))
                        {
                            Tile west_middle = middle_tiles.GetTile(west);
                            if (west_middle.Name.Contains("Door"))
                            {
                                bool hasTile = HasTile(exits, west_middle);
                                if (!hasTile)
                                {
                                    exits.Tiles.Add(west_middle);
                                }
                            }
                            else
                            {
                                Tile west_room = room_tiles.GetTile(west);
                                if (west_room != null &&
                                    !string.IsNullOrEmpty(west_room.Name) &&
                                    west_room.Name != tile.Name &&
                                    !west_middle.BlocksMovement)
                                {
                                    bool hasTile = HasTile(exits, west_room);
                                    if (!hasTile)
                                    {
                                        exits.Tiles.Add(west_room);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return exits.Tiles;
        }

        #endregion
    }
}

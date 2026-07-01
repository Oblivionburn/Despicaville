using System;
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

namespace Despicaville.Scenes
{
    public class MapEditor : Scene
    {
        #region Variables

        string current_file = "";
        bool convert_coords;

        bool TileToggle;
        bool RemovingTiles;

        Layer BottomTiles = new();
        Layer MiddleTiles = new();
        Layer TopTiles = new();
        Layer RoomTiles = new();

        bool Selecting_Layer;
        string[] Layers = ["Bottom", "Middle", "Top", "Room"];
        List<Button> Buttons_Layer = [];

        bool Selecting_RoomType;
        List<string> RoomTypes = [];
        List<Button> Buttons_RoomType = [];

        bool Selecting_MapType;
        string[] MapTypes = ["Residential", "Commercial", "Service", "Road", "Park"];
        List<Button> Buttons_MapType = [];

        bool Selecting_MapFacing;
        string[] MapFacings = ["North", "East", "South", "West"];
        List<Button> Buttons_MapFacing = [];

        Tile? SelectedTile;

        int Tiles_TopY;
        int Tiles_BottomY;
        List<Tile> Tiles = [];

        int Furniture_TopY;
        int Furniture_BottomY;
        List<Tile> Furniture = [];

        Stream? SaveStream;
        XmlWriter? Writer;

        #endregion

        #region Constructors

        public MapEditor(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "MapEditor";
            Load(content);
        }

        #endregion

        #region Methods

        public override void Update(Game? gameRef, ContentManager? content)
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

        public override void DrawMenu(SpriteBatch spriteBatch)
        {
            if (Main.Game == null ||
                Menu == null)
            {
                return;
            }

            if (Visible)
            {
                foreach (Picture picture in Menu.Pictures)
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

                Button? layer = Menu.GetButton("Layer");
                if (layer?.Text != null)
                {
                    switch (layer.Text)
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

                            foreach (Tile tile in TopTiles.Tiles)
                            {
                                tile.Draw(spriteBatch, Main.Game.Resolution);
                            }

                            foreach (Tile tile in RoomTiles.Tiles)
                            {
                                tile.Draw(spriteBatch, Main.Game.Resolution);
                            }
                            break;
                    }
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

                foreach (Picture picture in Menu.Pictures)
                {
                    if (picture.Name == "Selected")
                    {
                        picture.Draw(spriteBatch);
                        break;
                    }
                }

                foreach (Picture picture in Menu.Pictures)
                {
                    if (picture.Name == "Highlight")
                    {
                        picture.Draw(spriteBatch);
                        break;
                    }
                }

                foreach (Button button in Menu.Buttons)
                {
                    button.Draw(spriteBatch);
                }

                foreach (Label label in Menu.Labels)
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

                Menu.GetLabel("Examine")?.Draw(spriteBatch);
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

            if (!hoveringButton &&
                !hoveringTile &&
                !hoveringFurniture)
            {
                Label? examine = Menu?.GetLabel("Examine");
                if (examine != null)
                {
                    examine.Visible = false;
                }
            }

            if (!hoveringTile &&
                !hoveringFurniture &&
                !hoveringMapTile)
            {
                Picture? highlight = Menu?.GetPicture("Highlight");
                if (highlight != null)
                {
                    highlight.Visible = false;
                }
            }

            Picture? tileWindow = Menu?.GetPicture("TileWindow");
            Picture? tileWindow_ArrowUp = Menu?.GetPicture("TileWindow_ArrowUp");
            Picture? tileWindow_ArrowDown = Menu?.GetPicture("TileWindow_ArrowDown");

            if (tileWindow?.Region != null &&
                InputManager.MouseWithin(tileWindow.Region.ToRectangle))
            {
                if (InputManager.Mouse_ScrolledUp)
                {
                    if (Tiles_TopY > 0)
                    {
                        Tiles_TopY--;

                        if (Tiles_TopY == 0)
                        {
                            if (tileWindow_ArrowUp != null)
                            {
                                tileWindow_ArrowUp.Visible = false;
                            }
                        }

                        if (tileWindow_ArrowDown != null)
                        {
                            tileWindow_ArrowDown.Visible = true;
                        }

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
                            if (tileWindow_ArrowDown != null)
                            {
                                tileWindow_ArrowDown.Visible = false;
                            }
                        }

                        if (tileWindow_ArrowUp != null)
                        {
                            tileWindow_ArrowUp.Visible = true;
                        }

                        ScrollTiles_Down(tileWindow);
                    }
                }
            }

            Picture? objectWindow = Menu?.GetPicture("ObjectWindow");
            Picture? objectWindow_ArrowUp = Menu?.GetPicture("ObjectWindow_ArrowUp");
            Picture? objectWindow_ArrowDown = Menu?.GetPicture("ObjectWindow_ArrowDown");

            if (objectWindow?.Region != null &&
                InputManager.MouseWithin(objectWindow.Region.ToRectangle))
            {
                if (InputManager.Mouse_ScrolledUp)
                {
                    if (Furniture_TopY > 0)
                    {
                        Furniture_TopY--;

                        if (Furniture_TopY == 0)
                        {
                            if (objectWindow_ArrowUp != null)
                            {
                                objectWindow_ArrowUp.Visible = false;
                            }
                        }

                        if (objectWindow_ArrowDown != null)
                        {
                            objectWindow_ArrowDown.Visible = true;
                        }

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
                            if (objectWindow_ArrowDown != null)
                            {
                                objectWindow_ArrowDown.Visible = false;
                            }
                        }

                        if (objectWindow_ArrowUp != null)
                        {
                            objectWindow_ArrowUp.Visible = true;
                        }

                        ScrollFurniture_Down(objectWindow);
                    }
                }
            }
        }

        private bool HoveringButton()
        {
            if (Menu == null)
            {
                return false;
            }

            foreach (Button button in Menu.Buttons)
            {
                button.Opacity = 0.8f;
                button.Selected = false;
            }

            foreach (Button button in Menu.Buttons)
            {
                if (button.Visible &&
                    button.Enabled)
                {
                    if (button.Region != null &&
                        InputManager.MouseWithin(button.Region.ToRectangle))
                    {
                        if (button.HoverText != null)
                        {
                            GameUtil.Examine(Menu, button.HoverText);
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
                    if (button.Region != null &&
                        InputManager.MouseWithin(button.Region.ToRectangle))
                    {
                        if (button.HoverText != null)
                        {
                            GameUtil.Examine(Menu, button.HoverText);
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
                    if (button.Region != null &&
                        InputManager.MouseWithin(button.Region.ToRectangle))
                    {
                        if (button.HoverText != null)
                        {
                            GameUtil.Examine(Menu, button.HoverText);
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
                    if (button.Region != null &&
                        InputManager.MouseWithin(button.Region.ToRectangle))
                    {
                        if (button.HoverText != null)
                        {
                            GameUtil.Examine(Menu, button.HoverText);
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
                    if (button.Region != null &&
                        InputManager.MouseWithin(button.Region.ToRectangle))
                    {
                        if (button.HoverText != null)
                        {
                            GameUtil.Examine(Menu, button.HoverText);
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
                if (tile.Visible &&
                    tile.Region != null)
                {
                    if (InputManager.MouseWithin(tile.Region.ToRectangle))
                    {
                        Picture? highlight = Menu?.GetPicture("Highlight");
                        if (highlight != null)
                        {
                            highlight.Region = tile.Region;
                            highlight.DrawColor = Color.Blue;
                            highlight.Visible = true;
                        }

                        GameUtil.Examine(Menu, tile.Texture?.Name);

                        if (InputManager.Mouse_LB_Pressed)
                        {
                            AssetManager.PlaySound_Random("Click");
                            InputManager.Mouse?.Flush();

                            SelectedTile = tile;

                            Picture? selected = Menu?.GetPicture("Selected");
                            if (selected != null)
                            {
                                selected.Region = tile.Region;
                                selected.Visible = true;
                            }
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
                if (tile.Visible &&
                    tile.Region != null)
                {
                    if (InputManager.MouseWithin(tile.Region.ToRectangle))
                    {
                        Picture? highlight = Menu?.GetPicture("Highlight");
                        if (highlight != null)
                        {
                            highlight.Region = tile.Region;
                            highlight.DrawColor = Color.Blue;
                            highlight.Visible = true;
                        }

                        GameUtil.Examine(Menu, tile.Texture?.Name);

                        if (InputManager.Mouse_LB_Pressed)
                        {
                            AssetManager.PlaySound_Random("Click");
                            InputManager.Mouse?.Flush();

                            SelectedTile = tile;

                            Picture? selected = Menu?.GetPicture("Selected");
                            if (selected != null)
                            {
                                selected.Region = tile.Region;
                                selected.Visible = true;
                            }
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        private bool HoveringMapTile()
        {
            Button? layer = Menu?.GetButton("Layer");
            Tile? tile = null;

            switch (layer?.Text)
            {
                case "Bottom":
                    foreach (Tile existing in BottomTiles.Tiles)
                    {
                        if (existing.Region != null &&
                            InputManager.MouseWithin(existing.Region.ToRectangle))
                        {
                            tile = existing;
                            break;
                        }
                    }
                    break;

                case "Middle":
                    foreach (Tile existing in MiddleTiles.Tiles)
                    {
                        if (existing.Region != null &&
                            InputManager.MouseWithin(existing.Region.ToRectangle))
                        {
                            tile = existing;
                            break;
                        }
                    }
                    break;

                case "Top":
                    foreach (Tile existing in TopTiles.Tiles)
                    {
                        if (existing.Region != null &&
                            InputManager.MouseWithin(existing.Region.ToRectangle))
                        {
                            tile = existing;
                            break;
                        }
                    }
                    break;

                case "Room":
                    foreach (Tile existing in RoomTiles.Tiles)
                    {
                        if (existing.Region != null &&
                            InputManager.MouseWithin(existing.Region.ToRectangle))
                        {
                            tile = existing;
                            break;
                        }
                    }
                    break;
            }

            if (tile != null)
            {
                Picture? highlight = Menu?.GetPicture("Highlight");
                if (highlight != null)
                {
                    highlight.Region = tile.Region;
                    highlight.DrawColor = Color.White;
                    highlight.Visible = true;
                }

                if (InputManager.Mouse_LB_Held)
                {
                    if (layer?.Text == "Bottom")
                    {
                        RemovingTiles = false;
                    }
                    else if (!TileToggle)
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

                    ToggleTile(tile, layer?.Text);
                }
                else if (InputManager.Mouse_LB_Pressed)
                {
                    InputManager.Mouse?.Flush();
                    TileToggle = false;
                }

                return true;
            }

            return false;
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");
            InputManager.Mouse?.Flush();

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
            InputManager.Mouse?.Flush();

            Button? layer = Menu?.GetButton("Layer");
            if (layer != null)
            {
                layer.Text = button.Text;
                layer.Enabled = true;
            }

            if (button.Text == "Room")
            {
                Label? roomType_Label = Menu?.GetLabel("RoomType");
                if (roomType_Label != null)
                {
                    roomType_Label.Visible = true;
                }

                Button? roomType_Button = Menu?.GetButton("RoomType");
                if (roomType_Button != null)
                {
                    roomType_Button.Visible = true;
                }
            }
            else
            {
                Label? roomType_Label = Menu?.GetLabel("RoomType");
                if (roomType_Label != null)
                {
                    roomType_Label.Visible = false;
                }

                Button? roomType_Button = Menu?.GetButton("RoomType");
                if (roomType_Button != null)
                {
                    roomType_Button.Visible = false;
                }
            }

            HideLayers();
        }

        private void CheckClick_RoomType(Button button)
        {
            AssetManager.PlaySound_Random("Click");
            InputManager.Mouse?.Flush();

            Button? roomType = Menu?.GetButton("RoomType");
            if (roomType != null)
            {
                roomType.Text = button.Text;
                roomType.Enabled = true;
            }

            HideRoomTypes();
        }

        private void CheckClick_MapType(Button button)
        {
            AssetManager.PlaySound_Random("Click");
            InputManager.Mouse?.Flush();

            Button? mapType = Menu?.GetButton("MapType");
            if (mapType != null)
            {
                mapType.Text = button.Text;
                mapType.Enabled = true;
            }

            if (button.Text == "Residential" ||
                button.Text == "Commercial" ||
                button.Text == "Service")
            {
                Label? mapFacing_Label = Menu?.GetLabel("MapFacing");
                if (mapFacing_Label != null)
                {
                    mapFacing_Label.Visible = true;
                }

                Button? mapFacing_Button = Menu?.GetButton("MapFacing");
                if (mapFacing_Button != null)
                {
                    mapFacing_Button.Visible = true;
                }
            }
            else
            {
                Label? mapFacing_Label = Menu?.GetLabel("MapFacing");
                if (mapFacing_Label != null)
                {
                    mapFacing_Label.Visible = false;
                }

                Button? mapFacing_Button = Menu?.GetButton("MapFacing");
                if (mapFacing_Button != null)
                {
                    mapFacing_Button.Visible = false;
                }
            }

            HideMapTypes();
        }

        private void CheckClick_MapFacing(Button button)
        {
            AssetManager.PlaySound_Random("Click");
            InputManager.Mouse?.Flush();

            Button? mapFacing = Menu?.GetButton("MapFacing");
            if (mapFacing != null)
            {
                mapFacing.Text = button.Text;
                mapFacing.Enabled = true;
            }

            HideMapFacings();
        }

        private void ToggleTile(Tile? tile, string? layer)
        {
            Picture? mapWindow = Menu?.GetPicture("MapWindow");
            if (mapWindow?.Region == null ||
                tile == null)
            {
                return;
            }

            float width = mapWindow.Region.Width / 20;
            float height = mapWindow.Region.Height / 20;

            if (RemovingTiles)
            {
                if (tile.Texture != null &&
                    tile.Region != null)
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
                        Button? roomType = Menu?.GetButton("RoomType");
                        if (roomType != null)
                        {
                            string name = "RoomType_" + roomType.Text;
                            tile.Name = name;
                            tile.Texture = Handler.GetTexture(name);
                        }
                    }

                    if (tile.Texture != null)
                    {
                        tile.Image = new Rectangle(0, 0, tile.Texture.Width, tile.Texture.Height);
                    }

                    if (SelectedTile != null &&
                        tile.Region != null)
                    {
                        tile.Region = new Region(tile.Region.X, tile.Region.Y, width * SelectedTile.Dimensions.Width, height * SelectedTile.Dimensions.Height);
                    }
                }
            }
        }

        public void Close()
        {
            SceneManager.ChangeScene("Title");

            Menu? main = MenuManager.GetMenu("Main");
            if (main != null)
            {
                main.Active = true;
                main.Visible = true;
            }
        }

        private void NewMap()
        {
            GenMap();

            Button? layer = Menu?.GetButton("Layer");
            if (layer != null)
            {
                layer.Text = "Bottom";
            }

            Button? mapType = Menu?.GetButton("MapType");
            if (mapType != null)
            {
                mapType.Text = "Park";
            }

            Label? roomType_Label = Menu?.GetLabel("RoomType");
            if (roomType_Label != null)
            {
                roomType_Label.Visible = false;
            }

            Button? roomType = Menu?.GetButton("RoomType");
            if (roomType != null)
            {
                roomType.Text = "Bathroom";
                roomType.Visible = false;
            }

            Label? mapFacing_Label = Menu?.GetLabel("MapFacing");
            if (mapFacing_Label != null)
            {
                mapFacing_Label.Visible = false;
            }

            Button? mapFacing = Menu?.GetButton("MapFacing");
            if (mapFacing != null)
            {
                mapFacing.Text = "North";
                mapFacing.Visible = false;
            }

            current_file = "";

            Label? mapFile = Menu?.GetLabel("MapFile");
            if (mapFile != null)
            {
                mapFile.Text = "Map File: New";
            }
        }

        private void OpenMap()
        {
            System.Windows.Forms.OpenFileDialog openFile = new()
            {
                InitialDirectory = Path.Combine(AssetManager.Directories["Mods"], "Core", "Maps"),
                Filter = "Despicaville Map | *.blockmap",
                DefaultExt = ".blockmap",
                Title = "Open Despicaville Map",
                FileName = ""
            };

            if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                BottomTiles = new Layer();
                MiddleTiles = new Layer();
                TopTiles = new Layer();
                RoomTiles = new Layer();

                ParseTiles(openFile.FileName);

                current_file = Path.GetFileNameWithoutExtension(openFile.FileName);

                Label? mapFile = Menu?.GetLabel("MapFile");
                if (mapFile != null)
                {
                    mapFile.Text = "Map File: " + current_file;
                }
            }
        }

        private void SaveMap()
        {
            string fileName = "";

            System.Windows.Forms.SaveFileDialog saveFile = new()
            {
                InitialDirectory = Path.Combine(AssetManager.Directories["Mods"], "Core", "Maps"),
                Filter = "Despicaville Map | *.blockmap",
                DefaultExt = ".blockmap",
                Title = "Save Despicaville Map",
                FileName = current_file + ".blockmap"
            };

            if (saveFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fileName = saveFile.FileName;
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                WriteStream(fileName);

                if (Writer == null)
                {
                    return;
                }

                if (BottomTiles.Tiles.Count > 0)
                {
                    EnterNode("Map");

                    Button? mapType = Menu?.GetButton("MapType");
                    Button? mapFacing = Menu?.GetButton("MapFacing");

                    if (mapType != null &&
                        mapFacing != null)
                    {
                        EnterNode("Globals");
                        Writer.WriteAttributeString("Type", mapType.Text);
                        Writer.WriteAttributeString("Direction", mapFacing.Text);
                        ExitNode();
                    }

                    EnterNode("BottomTiles");
                    foreach (Tile tile in BottomTiles.Tiles)
                    {
                        EnterNode("BottomTile");
                        Writer.WriteAttributeString("Texture", tile.Texture?.Name);
                        Writer.WriteAttributeString("Location", tile.Location?.X.ToString() + "," + tile.Location?.Y.ToString());
                        ExitNode();
                    }
                    ExitNode();

                    EnterNode("MiddleTiles");
                    foreach (Tile tile in MiddleTiles.Tiles)
                    {
                        EnterNode("MiddleTile");
                        Writer.WriteAttributeString("Texture", tile.Texture?.Name);
                        Writer.WriteAttributeString("Location", tile.Location?.X.ToString() + "," + tile.Location?.Y.ToString());
                        ExitNode();
                    }
                    ExitNode();

                    EnterNode("TopTiles");
                    foreach (Tile tile in TopTiles.Tiles)
                    {
                        EnterNode("TopTile");
                        Writer.WriteAttributeString("Texture", tile.Texture?.Name);
                        Writer.WriteAttributeString("Location", tile.Location?.X.ToString() + "," + tile.Location?.Y.ToString());
                        ExitNode();
                    }
                    ExitNode();

                    EnterNode("RoomTiles");
                    foreach (Tile tile in RoomTiles.Tiles)
                    {
                        EnterNode("RoomTile");
                        Writer.WriteAttributeString("Texture", tile.Texture?.Name);
                        Writer.WriteAttributeString("Location", tile.Location?.X.ToString() + "," + tile.Location?.Y.ToString());
                        ExitNode();
                    }
                    ExitNode();

                    Map rooms = new()
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
                                Layer room = new()
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
                                Writer.WriteAttributeString("Texture", tile.Texture?.Name);
                                Writer.WriteAttributeString("Location", tile.Location?.X.ToString() + "," + tile.Location?.Y.ToString());
                                ExitNode();
                            }
                            ExitNode();

                            List<Vector2> exits = GetExits(layer, BottomTiles, MiddleTiles, RoomTiles);
                            if (exits.Count > 0)
                            {
                                EnterNode("Exits");
                                foreach (Vector2 exit in exits)
                                {
                                    EnterNode("Exit");
                                    Writer.WriteAttributeString("Location", exit.X.ToString() + "," + exit.Y.ToString());
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
            if (layer.Region == null)
            {
                return;
            }

            layer.Enabled = false;

            Texture2D? buttonFrame = Handler.GetTexture("ButtonFrame");
            Texture2D? buttonFrame_Highlight = Handler.GetTexture("ButtonFrame_Highlight");

            for (int i = 0; i < Layers.Length; i++)
            {
                Button button = new()
                {
                    ID = Handler.GetID(),
                    Font = AssetManager.Fonts["ControlFont"],
                    Name = "Layer_" + Layers[i],
                    Texture = buttonFrame,
                    Texture_Highlight = buttonFrame_Highlight,
                    Region = new Region(layer.Region.X, layer.Region.Y + (layer.Region.Height * (i + 1)), layer.Region.Width, layer.Region.Height),
                    DrawColor = Color.White,
                    DrawColor_Selected = Color.White,
                    Text = Layers[i],
                    TextColor = Color.White,
                    TextColor_Selected = Color.Red,
                    Enabled = true,
                    Visible = true
                };

                if (button.Texture != null)
                {
                    button.Image = new Rectangle(0, 0, button.Texture.Width, button.Texture.Height);
                }

                Buttons_Layer.Add(button);
            }
        }

        private void HideLayers()
        {
            InputManager.Mouse?.Flush();

            Button? layer = Menu?.GetButton("Layer");
            if (layer != null)
            {
                layer.Enabled = true;
            }

            Buttons_Layer.Clear();
            Selecting_Layer = false;
        }

        private void ShowRoomTypes(Button roomType)
        {
            if (roomType.Region == null)
            {
                return;
            }

            roomType.Enabled = false;

            Texture2D? buttonFrame = Handler.GetTexture("ButtonFrame");
            Texture2D? buttonFrame_Highlight = Handler.GetTexture("ButtonFrame_Highlight");

            for (int i = 0; i < RoomTypes.Count; i++)
            {
                Button button = new()
                {
                    ID = Handler.GetID(),
                    Font = AssetManager.Fonts["ControlFont"],
                    Name = "RoomType_" + RoomTypes[i],
                    Texture = buttonFrame,
                    Texture_Highlight = buttonFrame_Highlight,
                    Region = new Region(roomType.Region.X, roomType.Region.Y + (roomType.Region.Height * (i + 1)), roomType.Region.Width, roomType.Region.Height),
                    DrawColor = Color.White,
                    DrawColor_Selected = Color.White,
                    Text = RoomTypes[i],
                    TextColor = Color.White,
                    TextColor_Selected = Color.Red,
                    Enabled = true,
                    Visible = true
                };

                if (button.Texture != null)
                {
                    button.Image = new Rectangle(0, 0, button.Texture.Width, button.Texture.Height);
                }

                Buttons_RoomType.Add(button);
            }
        }

        private void HideRoomTypes()
        {
            InputManager.Mouse?.Flush();

            Button? roomType = Menu?.GetButton("RoomType");
            if (roomType != null)
            {
                roomType.Enabled = true;
            }

            Buttons_RoomType.Clear();
            Selecting_RoomType = false;
        }

        private void ShowMapTypes(Button mapType)
        {
            if (mapType.Region == null)
            {
                return;
            }

            mapType.Enabled = false;

            Texture2D? buttonFrame = Handler.GetTexture("ButtonFrame");
            Texture2D? buttonFrame_Highlight = Handler.GetTexture("ButtonFrame_Highlight");

            for (int i = 0; i < MapTypes.Length; i++)
            {
                Button button = new()
                {
                    ID = Handler.GetID(),
                    Font = AssetManager.Fonts["ControlFont"],
                    Name = "MapType_" + MapTypes[i],
                    Texture = buttonFrame,
                    Texture_Highlight = buttonFrame_Highlight,
                    Region = new Region(mapType.Region.X, mapType.Region.Y + (mapType.Region.Height * (i + 1)), mapType.Region.Width, mapType.Region.Height),
                    DrawColor = Color.White,
                    DrawColor_Selected = Color.White,
                    Text = MapTypes[i],
                    TextColor = Color.White,
                    TextColor_Selected = Color.Red,
                    Enabled = true,
                    Visible = true
                };

                if (button.Texture != null)
                {
                    button.Image = new Rectangle(0, 0, button.Texture.Width, button.Texture.Height);
                }

                Buttons_MapType.Add(button);
            }
        }

        private void HideMapTypes()
        {
            InputManager.Mouse?.Flush();

            Button? mapType = Menu?.GetButton("MapType");
            if (mapType != null)
            {
                mapType.Enabled = true;
            }

            Buttons_MapType.Clear();
            Selecting_MapType = false;
        }

        private void ShowMapFacings(Button mapFacing)
        {
            if (mapFacing.Region == null)
            {
                return;
            }

            mapFacing.Enabled = false;

            Texture2D? buttonFrame = Handler.GetTexture("ButtonFrame");
            Texture2D? buttonFrame_Highlight = Handler.GetTexture("ButtonFrame_Highlight");

            for (int i = 0; i < MapFacings.Length; i++)
            {
                Button button = new()
                {
                    ID = Handler.GetID(),
                    Font = AssetManager.Fonts["ControlFont"],
                    Name = "MapFacing_" + MapFacings[i],
                    Texture = buttonFrame,
                    Texture_Highlight = buttonFrame_Highlight,
                    Region = new Region(mapFacing.Region.X, mapFacing.Region.Y + (mapFacing.Region.Height * (i + 1)), mapFacing.Region.Width, mapFacing.Region.Height),
                    DrawColor = Color.White,
                    DrawColor_Selected = Color.White,
                    Text = MapFacings[i],
                    TextColor = Color.White,
                    TextColor_Selected = Color.Red,
                    Enabled = true,
                    Visible = true
                };

                if (button.Texture != null)
                {
                    button.Image = new Rectangle(0, 0, button.Texture.Width, button.Texture.Height);
                }

                Buttons_MapFacing.Add(button);
            }
        }

        private void HideMapFacings()
        {
            InputManager.Mouse?.Flush();

            Button? mapFacing = Menu?.GetButton("MapFacing");
            if (mapFacing != null)
            {
                mapFacing.Enabled = true;
            }

            Buttons_MapFacing.Clear();
            Selecting_MapFacing = false;
        }

        private void ScrollTiles_Up(Picture tileWindow)
        {
            if (tileWindow.Region == null)
            {
                return;
            }

            float width = (tileWindow.Region.Width + 8) / 10;

            int count = Tiles.Count;
            for (int i = 0; i < count; i++)
            {
                Tile tile = Tiles[i];
                if (tile.Region == null)
                {
                    continue;
                }

                tile.Region.Y += width + 4;

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

                Picture? selected = Menu?.GetPicture("Selected");
                if (selected?.Region != null &&
                    selected.Region.X == tile.Region.X &&
                    selected.Region.Y == tile.Region.Y)
                {
                    selected.Visible = tile.Visible;
                }
            }
        }

        private void ScrollTiles_Down(Picture tileWindow)
        {
            if (tileWindow.Region == null)
            {
                return;
            }

            float width = (tileWindow.Region.Width + 8) / 10;

            int count = Tiles.Count;
            for (int i = 0; i < count; i++)
            {
                Tile tile = Tiles[i];
                if (tile.Region == null)
                {
                    continue;
                }

                tile.Region.Y -= width + 4;

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

                Picture? selected = Menu?.GetPicture("Selected");
                if (selected?.Region != null &&
                    selected.Region.X == tile.Region.X &&
                    selected.Region.Y == tile.Region.Y)
                {
                    selected.Visible = tile.Visible;
                }
            }
        }

        private void ScrollFurniture_Up(Picture objectWindow)
        {
            if (objectWindow.Region == null)
            {
                return;
            }

            float width = (objectWindow.Region.Width + 8) / 10;

            int count = Furniture.Count;
            for (int i = 0; i < count; i++)
            {
                Tile tile = Furniture[i];
                if (tile.Region == null)
                {
                    continue;
                }

                tile.Region.Y += width + 4;

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

                Picture? selected = Menu?.GetPicture("Selected");
                if (selected?.Region != null &&
                    selected.Region.X == tile.Region.X &&
                    selected.Region.Y == tile.Region.Y)
                {
                    selected.Visible = tile.Visible;
                }
            }
        }

        private void ScrollFurniture_Down(Picture objectWindow)
        {
            if (objectWindow.Region == null)
            {
                return;
            }

            float width = (objectWindow.Region.Width + 8) / 10;

            int count = Furniture.Count;
            for (int i = 0; i < count; i++)
            {
                Tile tile = Furniture[i];
                if (tile.Region == null)
                {
                    continue;
                }

                tile.Region.Y -= width + 4;

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

                Picture? selected = Menu?.GetPicture("Selected");
                if (selected?.Region != null &&
                    selected.Region.X == tile.Region.X &&
                    selected.Region.Y == tile.Region.Y)
                {
                    selected.Visible = tile.Visible;
                }
            }
        }

        private void GenMap()
        {
            Picture? mapWindow = Menu?.GetPicture("MapWindow");
            if (mapWindow?.Region == null)
            {
                return;
            }

            Texture2D? texture = Handler.GetTexture("Grass");
            if (texture == null)
            {
                return;
            }

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
                        DrawColor = Color.White * 0.9f,
                        Visible = true
                    });
                }
            }
        }

        public override void Load()
        {
            NewMap();

            DirectoryInfo modsDir = new(AssetManager.Directories["Mods"]);
            foreach (DirectoryInfo mod in modsDir.GetDirectories())
            {
                LoadTiles(mod);
            }

            LoadFurniture();

            DirectoryInfo dir = new(AssetManager.Directories["Textures"]);
            LoadRoomTypes(dir);
        }

        private void LoadTiles(DirectoryInfo TexturesDir)
        {
            Picture? tileWindow = Menu?.GetPicture("TileWindow");
            if (tileWindow?.Region == null)
            {
                return;
            }

            int x = 0;
            int y = 0;
            float X = tileWindow.Region.X;
            float Y = tileWindow.Region.Y;
            float tileWidth = (tileWindow.Region.Width + 8) / 10;

            foreach (DirectoryInfo sub_dir in TexturesDir.GetDirectories())
            {
                if (sub_dir.Name == "Tiles")
                {
                    foreach (FileInfo file in sub_dir.GetFiles("*.png"))
                    {
                        string name = Path.GetFileNameWithoutExtension(file.FullName);
                        Texture2D? texture = Handler.GetTexture(name);

                        Tile tile = new()
                        {
                            Name = name,
                            Type = "Tile",
                            Location = new Location(x, y, 0),
                            Texture = texture,
                            Region = new Region(X, Y, tileWidth, tileWidth),
                            Dimensions = new Dimension2(1, 1),
                            DrawColor = Color.White,
                            Visible = true
                        };

                        if (tile.Texture != null)
                        {
                            tile.Image = new Rectangle(0, 0, tile.Texture.Width, tile.Texture.Height);
                        }

                        if (Y + tileWidth + 4 > tileWindow.Region.Y + tileWindow.Region.Height)
                        {
                            tile.Visible = false;
                        }

                        Tiles.Add(tile);

                        x++;
                        X += tileWidth + 4;
                        if (X + tileWidth + 4 > tileWindow.Region.X + tileWindow.Region.Width)
                        {
                            x = 0;
                            X = tileWindow.Region.X;

                            y++;
                            Y += tileWidth + 4;
                        }
                    }

                    break;
                }
            }

            Tiles_BottomY = y - 9;
            if (Tiles_BottomY < 0)
            {
                Picture? tileWindow_ArrowDown = Menu?.GetPicture("TileWindow_ArrowDown");
                if (tileWindow_ArrowDown != null)
                {
                    tileWindow_ArrowDown.Visible = false;
                }
            }
        }

        private void LoadFurniture()
        {
            Picture? objectWindow = Menu?.GetPicture("ObjectWindow");
            if (objectWindow?.Region == null)
            {
                return;
            }

            int loc_x = 0;
            int loc_y = 0;
            float X = objectWindow.Region.X;
            float Y = objectWindow.Region.Y;
            float tileWidth = (objectWindow.Region.Width + 8) / 10;

            int count = Handler.Furniture.Count;
            for (int i = 0; i < count; i++)
            {
                Tile existing = Handler.Furniture[i];
                if (existing.Texture == null)
                {
                    continue;
                }

                float width_scale = 1;
                if (existing.Texture.Width > 128)
                {
                    width_scale = existing.Texture.Width / 128;
                }

                float height_scale = 1;
                if (existing.Texture.Height > 128)
                {
                    height_scale = existing.Texture.Height / 128;
                }

                if (i > 0)
                {
                    loc_x++;
                    X += tileWidth + 4;
                    if (X + tileWidth + 4 > objectWindow.Region.X + objectWindow.Region.Width)
                    {
                        loc_x = 0;
                        X = objectWindow.Region.X;

                        loc_y++;
                        Y += tileWidth + 4;
                    }
                }

                Region region = new(X, Y, tileWidth * width_scale, tileWidth * height_scale);

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
                        if (tile.Region == null)
                        {
                            continue;
                        }

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
                        X += tileWidth + 4;
                        if (X + tileWidth + 4 > objectWindow.Region.X + objectWindow.Region.Width)
                        {
                            loc_x = 0;
                            X = objectWindow.Region.X;

                            loc_y++;
                            Y += tileWidth + 4;
                        }

                        region = new Region(X, Y, tileWidth * width_scale, tileWidth * height_scale);
                    }
                }

                if (okay)
                {
                    Tile tile = new()
                    {
                        Name = existing.Name,
                        Type = "Furniture",
                        Location = new Location(loc_x, loc_y, 0),
                        Texture = existing.Texture,
                        Image = new Rectangle(0, 0, existing.Texture.Width, existing.Texture.Height),
                        Region = region,
                        Direction = existing.Direction,
                        BlocksMovement = existing.BlocksMovement,
                        Dimensions = new Dimension2((int)width_scale, (int)height_scale),
                        DrawColor = Color.White,
                        Visible = true
                    };

                    if (Y + (tileWidth * height_scale) + 4 > objectWindow.Region.Y + objectWindow.Region.Height)
                    {
                        tile.Visible = false;
                    }

                    Furniture.Add(tile);
                }
            }

            Furniture_BottomY = loc_y - 8;
            if (Furniture_BottomY < 0)
            {
                Picture? objectWindow_ArrowDown = Menu?.GetPicture("ObjectWindow_ArrowDown");
                if (objectWindow_ArrowDown != null)
                {
                    objectWindow_ArrowDown.Visible = false;
                }
            }
        }

        private void LoadRoomTypes(DirectoryInfo TexturesDir)
        {
            RoomTypes = [];

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
            if (Menu == null)
            {
                return;
            }

            Menu.Clear();

            Texture2D? frame = Handler.GetTexture("Frame");
            Texture2D? frame_Full = Handler.GetTexture("Frame_Full");
            Texture2D? buttonFrame = Handler.GetTexture("ButtonFrame");
            Texture2D? buttonFrame_Highlight = Handler.GetTexture("ButtonFrame_Highlight");

            Texture2D? white = Handler.GetTexture("White");
            Texture2D? arrowIcon_Down = Handler.GetTexture("ArrowIcon_Down");
            Texture2D? arrowIcon_Up = Handler.GetTexture("ArrowIcon_Up");

            Texture2D? grid_Hover = Handler.GetTexture("Grid_Hover");
            Texture2D? selection = Handler.GetTexture("Selection");

            Menu.AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                font = AssetManager.Fonts["ControlFont"],
                name = "New",
                texture = buttonFrame,
                texture_highlight = buttonFrame_Highlight,
                draw_color = Color.White,
                draw_color_selected = Color.White,
                text = "New",
                text_color = Color.White,
                text_selected_color = Color.Red,
                enabled = true,
                visible = true
            });

            Menu.AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                font = AssetManager.Fonts["ControlFont"],
                name = "Open",
                texture = buttonFrame,
                texture_highlight = buttonFrame_Highlight,
                draw_color = Color.White,
                draw_color_selected = Color.White,
                text = "Open",
                text_color = Color.White,
                text_selected_color = Color.Red,
                enabled = true,
                visible = true
            });

            Menu.AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                font = AssetManager.Fonts["ControlFont"],
                name = "Save",
                texture = buttonFrame,
                texture_highlight = buttonFrame_Highlight,
                draw_color = Color.White,
                draw_color_selected = Color.White,
                text = "Save",
                text_color = Color.White,
                text_selected_color = Color.Red,
                enabled = true,
                visible = true
            });

            Menu.AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                font = AssetManager.Fonts["ControlFont"],
                name = "Exit",
                texture = buttonFrame,
                texture_highlight = buttonFrame_Highlight,
                draw_color = Color.White,
                draw_color_selected = Color.White,
                text = "Exit",
                text_color = Color.White,
                text_selected_color = Color.Red,
                enabled = true,
                visible = true
            });

            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Layer", "Layer:", Color.White, new Region(0, 0, 0, 0), true);

            Label? layer = Menu.GetLabel("Layer");
            if (layer != null)
            {
                layer.Alignment_Horizontal = Alignment.Right;
            }

            Menu.AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                font = AssetManager.Fonts["ControlFont"],
                name = "Layer",
                texture = buttonFrame,
                texture_highlight = buttonFrame_Highlight,
                texture_disabled = buttonFrame_Highlight,
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

            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "RoomType", "Room Type:", Color.White, new Region(0, 0, 0, 0), false);

            Label? roomType = Menu.GetLabel("RoomType");
            if (roomType != null)
            {
                roomType.Alignment_Horizontal = Alignment.Right;
            }

            Menu.AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                font = AssetManager.Fonts["ControlFont"],
                name = "RoomType",
                texture = buttonFrame,
                texture_highlight = buttonFrame_Highlight,
                texture_disabled = buttonFrame_Highlight,
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

            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "MapType", "Map Type:", Color.White, new Region(0, 0, 0, 0), true);

            Label? mapType = Menu.GetLabel("MapType");
            if (mapType != null)
            {
                mapType.Alignment_Horizontal = Alignment.Right;
            }

            Menu.AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                font = AssetManager.Fonts["ControlFont"],
                name = "MapType",
                texture = buttonFrame,
                texture_highlight = buttonFrame_Highlight,
                texture_disabled = buttonFrame_Highlight,
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

            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "MapFacing", "Map Facing:", Color.White, new Region(0, 0, 0, 0), false);

            Label? mapFacing = Menu.GetLabel("MapFacing");
            if (mapFacing != null)
            {
                mapFacing.Alignment_Horizontal = Alignment.Right;
            }

            Menu.AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                font = AssetManager.Fonts["ControlFont"],
                name = "MapFacing",
                texture = buttonFrame,
                texture_highlight = buttonFrame_Highlight,
                texture_disabled = buttonFrame_Highlight,
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

            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Tiles", "Tiles:", Color.White, new Region(0, 0, 0, 0), true);

            Label? tiles = Menu.GetLabel("Tiles");
            if (tiles != null)
            {
                tiles.Alignment_Horizontal = Alignment.Left;
            }

            Menu.AddPicture(Handler.GetID(), "TileWindow", white, new Region(0, 0, 0, 0), Color.White, true);
            Menu.AddPicture(Handler.GetID(), "TileWindow_ArrowDown", arrowIcon_Down, new Region(0, 0, 0, 0), Color.White, true);
            Menu.AddPicture(Handler.GetID(), "TileWindow_ArrowUp", arrowIcon_Up, new Region(0, 0, 0, 0), Color.White, false);

            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Objects", "Objects:", Color.White, new Region(0, 0, 0, 0), true);

            Label? objects = Menu.GetLabel("Objects");
            if (objects != null)
            {
                objects.Alignment_Horizontal = Alignment.Left;
            }

            Menu.AddPicture(Handler.GetID(), "ObjectWindow", white, new Region(0, 0, 0, 0), Color.White, true);
            Menu.AddPicture(Handler.GetID(), "ObjectWindow_ArrowDown", arrowIcon_Down, new Region(0, 0, 0, 0), Color.White, true);
            Menu.AddPicture(Handler.GetID(), "ObjectWindow_ArrowUp", arrowIcon_Up, new Region(0, 0, 0, 0), Color.White, false);

            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "MapFile", "Map File: New", Color.White, new Region(0, 0, 0, 0), true);

            Label? mapFile = Menu.GetLabel("MapFile");
            if (mapFile != null)
            {
                mapFile.Alignment_Horizontal = Alignment.Left;
            }

            Menu.AddPicture(Handler.GetID(), "MapWindow", frame_Full, new Region(0, 0, 0, 0), Color.White, true);

            Menu.AddPicture(Handler.GetID(), "Highlight", grid_Hover, new Region(0, 0, 0, 0), Color.Lime, false);
            Menu.AddPicture(Handler.GetID(), "Selected", selection, new Region(0, 0, 0, 0), Color.Lime, false);
            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, frame, new Region(0, 0, 0, 0), false);

            Menu.Visible = true;

            if (Main.Game == null)
            {
                return;
            }
            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            if (Main.Game == null)
            {
                return;
            }

            float buttonHeight = Main.Game.MenuSize_Y / 2;

            Button? new_Button = Menu?.GetButton("New");
            if (new_Button != null)
            {
                new_Button.Region = new Region(0, 0, Main.Game.MenuSize_X * 2, buttonHeight);
            }

            Button? open = Menu?.GetButton("Open");
            if (open != null)
            {
                open.Region = new Region(Main.Game.MenuSize_X * 2, 0, Main.Game.MenuSize_X * 2, buttonHeight);
            }

            Button? save = Menu?.GetButton("Save");
            if (save != null)
            {
                save.Region = new Region(Main.Game.MenuSize_X * 4, 0, Main.Game.MenuSize_X * 2, buttonHeight);
            }

            Button? exit = Menu?.GetButton("Exit");
            if (exit != null)
            {
                exit.Region = new Region(Main.Game.MenuSize_X * 6, 0, Main.Game.MenuSize_X * 2, buttonHeight);
            }

            Label? layer_Label = Menu?.GetLabel("Layer");
            if (layer_Label != null)
            {
                layer_Label.Region = new Region(Main.Game.MenuSize_X * 9, 0, Main.Game.MenuSize_X * 2, buttonHeight);
            }

            Button? layer_Button = Menu?.GetButton("Layer");
            if (layer_Button != null)
            {
                layer_Button.Region = new Region(Main.Game.MenuSize_X * 11, 0, Main.Game.MenuSize_X * 3, buttonHeight);
            }

            Label? roomType_Label = Menu?.GetLabel("RoomType");
            if (roomType_Label != null)
            {
                roomType_Label.Region = new Region(Main.Game.MenuSize_X * 15, 0, Main.Game.MenuSize_X * 2, buttonHeight);
            }

            Button? roomType_Button = Menu?.GetButton("RoomType");
            if (roomType_Button != null)
            {
                roomType_Button.Region = new Region(Main.Game.MenuSize_X * 17, 0, Main.Game.MenuSize_X * 3, buttonHeight);
            }

            Label? mapType_Label = Menu?.GetLabel("MapType");
            if (mapType_Label != null)
            {
                mapType_Label.Region = new Region(Main.Game.MenuSize_X * 21, 0, Main.Game.MenuSize_X * 2, buttonHeight);
            }

            Button? mapType_Button = Menu?.GetButton("MapType");
            if (mapType_Button != null)
            {
                mapType_Button.Region = new Region(Main.Game.MenuSize_X * 23, 0, Main.Game.MenuSize_X * 3, buttonHeight);
            }

            Label? mapFacing_Label = Menu?.GetLabel("MapFacing");
            if (mapFacing_Label != null)
            {
                mapFacing_Label.Region = new Region(Main.Game.MenuSize_X * 27, 0, Main.Game.MenuSize_X * 2, buttonHeight);
            }

            Button? mapFacing_Button = Menu?.GetButton("MapFacing");
            if (mapFacing_Button != null)
            {
                mapFacing_Button.Region = new Region(Main.Game.MenuSize_X * 29, 0, Main.Game.MenuSize_X * 3, buttonHeight);
            }

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

            Label? tiles = Menu?.GetLabel("Tiles");
            if (tiles != null)
            {
                tiles.Region = new Region(0, Y, Main.Game.MenuSize_X * 4, buttonHeight);
            }

            Picture? tileWindow = Menu?.GetPicture("TileWindow");
            if (tileWindow != null)
            {
                tileWindow.Region = new Region(0, Y + buttonHeight, windowHeight, windowHeight);
            }

            Picture? tileWindow_ArrowUp = Menu?.GetPicture("TileWindow_ArrowUp");
            if (tileWindow_ArrowUp != null &&
                tileWindow?.Region != null)
            {
                tileWindow_ArrowUp.Region = new Region(tileWindow.Region.X + tileWindow.Region.Width, tileWindow.Region.Y, tileWidth, tileWidth);
            }

            Picture? tileWindow_ArrowDown = Menu?.GetPicture("TileWindow_ArrowDown");
            if (tileWindow_ArrowDown != null &&
                tileWindow?.Region != null)
            {
                tileWindow_ArrowDown.Region = new Region(tileWindow.Region.X + tileWindow.Region.Width, tileWindow.Region.Y + tileWindow.Region.Height - tileWidth, tileWidth, tileWidth);
            }

            Y = Main.Game.MenuSize_Y + windowHeight + (buttonHeight * 2);

            Label? objects = Menu?.GetLabel("Objects");
            if (objects != null)
            {
                objects.Region = new Region(0, Y, Main.Game.MenuSize_X * 4, buttonHeight);
            }

            Picture? objectWindow = Menu?.GetPicture("ObjectWindow");
            if (objectWindow != null)
            {
                objectWindow.Region = new Region(0, Y + buttonHeight, windowHeight, windowHeight);
            }

            Picture? objectWindow_ArrowUp = Menu?.GetPicture("ObjectWindow_ArrowUp");
            if (objectWindow_ArrowUp != null &&
                objectWindow?.Region != null)
            {
                objectWindow_ArrowUp.Region = new Region(objectWindow.Region.X + objectWindow.Region.Width, objectWindow.Region.Y, tileWidth, tileWidth);
            }

            Picture? objectWindow_ArrowDown = Menu?.GetPicture("ObjectWindow_ArrowDown");
            if (objectWindow_ArrowDown != null &&
                objectWindow?.Region != null)
            {
                objectWindow_ArrowDown.Region = new Region(objectWindow.Region.X + objectWindow.Region.Width, objectWindow.Region.Y + objectWindow.Region.Height - tileWidth, tileWidth, tileWidth);
            }

            Label? mapFile = Menu?.GetLabel("MapFile");
            if (mapFile != null)
            {
                mapFile.Region = new Region(Main.Game.MenuSize_X * 10, Main.Game.MenuSize_Y, Main.Game.MenuSize_X * 4, buttonHeight);
            }

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

            Picture? mapWindow = Menu?.GetPicture("MapWindow");
            if (mapWindow != null)
            {
                mapWindow.Region = new Region(Main.Game.MenuSize_X * 10, Main.Game.MenuSize_Y + buttonHeight, mapHeight, mapHeight);
            }
        }

        #endregion

        #region XML Methods

        private void WriteStream(string path)
        {
            SaveStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            XmlWriterSettings xmlWriterSettings = new()
            {
                Indent = true,
                IndentChars = "\t"
            };
            Writer = XmlWriter.Create(SaveStream, xmlWriterSettings);
            Writer.WriteStartDocument();
        }

        private void EnterNode(string elementName)
        {
            if (Writer == null)
            {
                return;
            }

            Writer.WriteStartElement(elementName);
        }

        private void ExitNode()
        {
            if (Writer == null)
            {
                return;
            }

            Writer.WriteEndElement();
        }

        private void FinalizeWriting()
        {
            if (Writer == null ||
                SaveStream == null)
            {
                return;
            }

            Writer.WriteEndDocument();
            Writer.Close();
            SaveStream.Close();
        }

        private void ParseTiles(string file)
        {
            using (XmlTextReader reader = new(File.OpenRead(file)))
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

                        Button? mapType = Menu?.GetButton("MapType");
                        if (mapType != null)
                        {
                            mapType.Text = reader.Value;

                            if (mapType.Text == "Residential" ||
                                mapType.Text == "Commercial" ||
                                mapType.Text == "Service")
                            {
                                Label? mapFacing_Label = Menu?.GetLabel("MapFacing");
                                if (mapFacing_Label != null)
                                {
                                    mapFacing_Label.Visible = true;
                                }

                                Button? mapFacing_Button = Menu?.GetButton("MapFacing");
                                if (mapFacing_Button != null)
                                {
                                    mapFacing_Button.Visible = true;
                                }
                            }
                            else
                            {
                                Label? mapFacing_Label = Menu?.GetLabel("MapFacing");
                                if (mapFacing_Label != null)
                                {
                                    mapFacing_Label.Visible = false;
                                }

                                Button? mapFacing_Button = Menu?.GetButton("MapFacing");
                                if (mapFacing_Button != null)
                                {
                                    mapFacing_Button.Visible = false;
                                }
                            }
                        }
                        break;

                    case "Name":
                        convert_coords = true;
                        break;

                    case "Direction":
                        Button? mapFacing = Menu?.GetButton("MapFacing");
                        if (mapFacing != null)
                        {
                            mapFacing.Text = reader.Value;
                        }
                        break;
                }
            }
        }

        private void VisitBottomTiles(XmlTextReader reader)
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
                            tile.Texture = Handler.GetTexture(reader.Value);
                            if (tile.Texture != null)
                            {
                                tile.Image = new Rectangle(0, 0, tile.Texture.Width, tile.Texture.Height);
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

                        Picture? mapWindow = Menu?.GetPicture("MapWindow");
                        if (mapWindow?.Region != null)
                        {
                            float X = mapWindow.Region.X;
                            float Y = mapWindow.Region.Y;
                            float width = mapWindow.Region.Width / 20;
                            float height = mapWindow.Region.Height / 20;

                            tile.Region = new(X + (tile.Location.X * width), Y + (tile.Location.Y * height), width, height);
                            tile.Visible = true;

                            for (int i = 0; i < BottomTiles.Tiles.Count; i++)
                            {
                                Tile existing = BottomTiles.Tiles[i];
                                if (existing.Location != null &&
                                    existing.Location.X == tile.Location.X &&
                                    existing.Location.Y == tile.Location.Y)
                                {
                                    BottomTiles.Tiles[i] = tile;
                                    break;
                                }
                            }
                        }
                        break;
                }
            }
        }

        private void VisitMiddleTiles(XmlTextReader reader)
        {
            while (reader.Read())
            {
                if (reader.Name == "MiddleTiles" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "MiddleTile":
                        Tile tile = new();
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
                        string name = reader.Value;
                        if (!string.IsNullOrEmpty(name))
                        {
                            int count = Handler.Furniture.Count;
                            for (int i = 0; i < count; i++)
                            {
                                Tile furniture = Handler.Furniture[i];
                                if (furniture.Texture != null &&
                                    furniture.Texture.Name == name)
                                {
                                    tile.Name = furniture.Name;
                                    tile.Direction = furniture.Direction;
                                    tile.Texture = furniture.Texture;
                                    tile.Image = new Rectangle(0, 0, tile.Texture.Width, tile.Texture.Height);
                                    tile.BlocksMovement = furniture.BlocksMovement;
                                    break;
                                }
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

                        Picture? mapWindow = Menu?.GetPicture("MapWindow");
                        if (mapWindow?.Region != null)
                        {
                            float X = mapWindow.Region.X;
                            float Y = mapWindow.Region.Y;
                            float width = mapWindow.Region.Width / 20;
                            float height = mapWindow.Region.Height / 20;

                            tile.Region = new Region(X + (tile.Location.X * width), Y + (tile.Location.Y * height), width, height);
                            tile.Visible = true;

                            for (int i = 0; i < Furniture.Count; i++)
                            {
                                Tile furniture = Furniture[i];
                                if (furniture.Name == tile.Name &&
                                    furniture.Direction == tile.Direction)
                                {
                                    tile.Region = new Region(tile.Region.X, tile.Region.Y, width * furniture.Dimensions.Width, height * furniture.Dimensions.Height);
                                    break;
                                }
                            }

                            for (int i = 0; i < MiddleTiles.Tiles.Count; i++)
                            {
                                Tile existing = MiddleTiles.Tiles[i];
                                if (existing.Location != null &&
                                    existing.Location.X == tile.Location.X &&
                                    existing.Location.Y == tile.Location.Y)
                                {
                                    MiddleTiles.Tiles[i] = tile;
                                    break;
                                }
                            }
                        }
                        break;
                }
            }
        }

        private void VisitTopTiles(XmlTextReader reader)
        {
            while (reader.Read())
            {
                if (reader.Name == "TopTiles" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "TopTile":
                        Tile tile = new();
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
                            tile.Texture = Handler.GetTexture(reader.Value);
                            if (tile.Texture != null)
                            {
                                tile.Image = new Rectangle(0, 0, tile.Texture.Width, tile.Texture.Height);
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

                        Picture? mapWindow = Menu?.GetPicture("MapWindow");
                        if (mapWindow?.Region != null)
                        {
                            float X = mapWindow.Region.X;
                            float Y = mapWindow.Region.Y;
                            float width = mapWindow.Region.Width / 20;
                            float height = mapWindow.Region.Height / 20;

                            tile.Region = new Region(X + (tile.Location.X * width), Y + (tile.Location.Y * height), width, height);
                            tile.Visible = true;

                            for (int i = 0; i < TopTiles.Tiles.Count; i++)
                            {
                                Tile existing = TopTiles.Tiles[i];
                                if (existing.Location != null &&
                                    existing.Location.X == tile.Location.X &&
                                    existing.Location.Y == tile.Location.Y)
                                {
                                    TopTiles.Tiles[i] = tile;
                                    break;
                                }
                            }
                        }
                        break;
                }
            }
        }

        private void VisitRoomTiles(XmlTextReader reader)
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
                            tile.Texture = Handler.GetTexture(reader.Value);
                            if (tile.Texture != null)
                            {
                                tile.Image = new Rectangle(0, 0, tile.Texture.Width, tile.Texture.Height);
                            }
                        }

                        tile.DrawColor = Color.White * 0.9f;
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

                        Picture? mapWindow = Menu?.GetPicture("MapWindow");
                        if (mapWindow?.Region != null)
                        {
                            float X = mapWindow.Region.X;
                            float Y = mapWindow.Region.Y;
                            float width = mapWindow.Region.Width / 20;
                            float height = mapWindow.Region.Height / 20;

                            tile.Region = new Region(X + (tile.Location.X * width), Y + (tile.Location.Y * height), width, height);
                            tile.Visible = true;

                            for (int i = 0; i < RoomTiles.Tiles.Count; i++)
                            {
                                Tile existing = RoomTiles.Tiles[i];
                                if (existing.Location != null &&
                                    existing.Location.X == tile.Location.X &&
                                    existing.Location.Y == tile.Location.Y)
                                {
                                    RoomTiles.Tiles[i] = tile;
                                    break;
                                }
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
            if (tile.Location == null)
            {
                return false;
            }

            int layerCount = rooms.Layers.Count;
            for (int l = 0; l < layerCount; l++)
            {
                Layer layer = rooms.Layers[l];

                int tileCount = layer.Tiles.Count;
                for (int t = 0; t < tileCount; t++)
                {
                    Tile existing = layer.Tiles[t];
                    if (existing.Location == null)
                    {
                        continue;
                    }

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
            if (tile.Location == null)
            {
                return false;
            }

            int tileCount = room.Tiles.Count;
            for (int t = 0; t < tileCount; t++)
            {
                Tile existing = room.Tiles[t];
                if (existing.Location == null)
                {
                    continue;
                }

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
            if (room_tile.Location == null)
            {
                return;
            }

            room.Tiles.Add(room_tile);

            Tile? north = room_tiles.GetTile(new Vector2(room_tile.Location.X, room_tile.Location.Y - 1));
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

            Tile? east = room_tiles.GetTile(new Vector2(room_tile.Location.X + 1, room_tile.Location.Y));
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

            Tile? south = room_tiles.GetTile(new Vector2(room_tile.Location.X, room_tile.Location.Y + 1));
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

            Tile? west = room_tiles.GetTile(new Vector2(room_tile.Location.X - 1, room_tile.Location.Y));
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

        private List<Vector2> GetExits(Layer room, Layer bottom_tiles, Layer middle_tiles, Layer room_tiles)
        {
            List<Vector2> exits = [];

            if (room != null)
            {
                foreach (Tile tile in room.Tiles)
                {
                    if (tile.Location == null)
                    {
                        continue;
                    }

                    Tile? middle = middle_tiles.GetTile(tile.Location.ToVector2);
                    if (middle != null)
                    {
                        for (float y = -1; y <= 1; y++)
                        {
                            for (float x = -1; x <= 1; x++)
                            {
                                if (Math.Abs(x) == Math.Abs(y))
                                {
                                    continue;
                                }

                                bool addExit = false;

                                Vector2 vector2 = new(tile.Location.X + x, tile.Location.Y + y);
                                Vector3 vector3 = new(tile.Location.X + x, tile.Location.Y + y, 0);

                                Tile? bottom = bottom_tiles.GetTile(vector3);
                                if (bottom != null)
                                {
                                    if (!string.IsNullOrEmpty(bottom.Name) &&
                                        bottom.Name.Contains("Wall"))
                                    {
                                        continue;
                                    }

                                    Tile? middle_exit = middle_tiles.GetTile(vector3);
                                    if (middle_exit == null)
                                    {
                                        continue;
                                    }

                                    Tile? room_exit = room_tiles.GetTile(vector3);
                                    if (room_exit == null)
                                    {
                                        continue;
                                    }

                                    if (string.IsNullOrEmpty(room_exit.Name))
                                    {
                                        if (string.IsNullOrEmpty(middle_exit.Name) ||
                                            middle_exit.Name.Contains("Door"))
                                        {
                                            addExit = true;
                                        }
                                    }
                                    else if (room_exit.Name != tile.Name &&
                                             !middle_exit.BlocksMovement)
                                    {
                                        addExit = true;
                                    }
                                }
                                else
                                {
                                    addExit = true;
                                }

                                if (addExit &&
                                    !exits.Contains(vector2))
                                {
                                    exits.Add(vector2);
                                }
                            }
                        }
                    }
                }
            }

            return exits;
        }

        #endregion
    }
}

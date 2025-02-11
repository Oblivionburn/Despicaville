using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Menus;
using OP_Engine.Scenes;
using OP_Engine.Characters;
using OP_Engine.Inventories;
using OP_Engine.Utility;
using OP_Engine.Jobs;

using Despicaville.Util;

namespace Despicaville.Scenes
{
    public class CharGen : Scene
    {
        #region Variables

        public int Pool = 50;
        public int TownSize = 7;
        public static Dictionary<string, int> Stats = new Dictionary<string, int>();

        public List<Label> Stats_Labels = new List<Label>();
        public List<Button> Stats_Buttons = new List<Button>();

        public List<Button> Appearance_Buttons = new List<Button>();
        public List<Picture> Appearance_Pictures = new List<Picture>();
        public List<Label> Appearance_Labels = new List<Label>();

        public string Gender = "Male";
        public int Gender_Value = 0;
        public string First_Name = "";
        public int FirstName_Value = 0;
        public string Last_Name = "";
        public int LastName_Value = 0;
        public int SkinColor_Value = 0;
        public int HairLength_Value = 0;
        public int HairColor_Value = -1;
        public int HatColor_Value = 0;
        public string[] HatColors = { "No Hat", "Black", "Blue", "Brown", "Cyan", "Green", "Grey", "Orange", "Pink", "Purple", "Red", "White", "Yellow" };
        public int ShirtColor_Value = 0;
        public int PantsColor_Value = 0;
        public int ShoesColor_Value = 0;

        #endregion

        #region Contructor

        public CharGen(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "CharGen";
            Load(content);
        }

        #endregion

        #region Methods

        public override void Update(Game gameRef, ContentManager content)
        {
            if (Visible)
            {
                UpdateControls();
                base.Update(gameRef, content);
            }
        }

        private void UpdateControls()
        {
            bool label_found = false;

            foreach (Button button in Menu.Buttons)
            {
                if (button.Visible &&
                    button.Enabled)
                {
                    if (InputManager.MouseWithin(button.Region.ToRectangle))
                    {
                        if (button.HoverText != null)
                        {
                            label_found = true;
                            GameUtil.Examine(Menu, button.HoverText);
                        }

                        button.Opacity = 1;
                        button.Selected = true;

                        if (InputManager.Mouse_LB_Pressed)
                        {
                            CheckClick(button);
                            break;
                        }
                    }
                    else if (InputManager.Mouse_Moved)
                    {
                        button.Opacity = 0.8f;
                        button.Selected = false;
                    }
                }
            }

            foreach (Label label in Menu.Labels)
            {
                if (label.Visible)
                {
                    if (InputManager.MouseWithin(label.Region.ToRectangle))
                    {
                        if (label.HoverText != null)
                        {
                            label_found = true;
                            Examine(label.HoverText);
                        }

                        break;
                    }
                }
            }

            if (!label_found)
            {
                Menu.GetLabel("Examine").Visible = false;
                Menu.GetLabel("ExamineStat").Visible = false;
            }
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");

            if (button.Name == "Back")
            {
                if (Handler.CharGen_Stage == 0)
                {
                    Back();
                }
                else if (Handler.CharGen_Stage == 1)
                {
                    Handler.CharGen_Stage = 0;
                    ChangeMenu();
                }
            }
            else if (button.Name == "Next")
            {
                if (Handler.CharGen_Stage == 0)
                {
                    Handler.CharGen_Stage = 1;
                    ChangeMenu();
                }
                else if (Handler.CharGen_Stage == 1)
                {
                    Finish();
                }
            }
            else if (button.Name.Contains("Minus"))
            {
                if (Handler.CharGen_Stage == 0)
                {
                    #region Stats

                    if (button.Name.Contains("TownSize"))
                    {
                        Menu.GetButton("TownSize_Plus").Enabled = true;
                        TownSize--;

                        Menu.GetLabel("TownSize_Amount").Text = TownSize.ToString();

                        if (TownSize == 6)
                        {
                            button.Enabled = false;
                        }
                    }
                    else
                    {
                        string[] name = button.Name.Split('_');
                        string stat = name[0];

                        if (Stats[stat] > 1)
                        {
                            Menu.GetButton(stat + "_Plus").Enabled = true;

                            Stats[stat]--;

                            Pool++;
                            Menu.GetLabel("Pool_Amount").Text = Pool.ToString();

                            Menu.GetLabel(stat + "_Amount").Text = Stats[stat].ToString();

                            if (Stats[stat] == 1)
                            {
                                button.Enabled = false;
                            }
                        }
                    }

                    #endregion
                }
                else if (Handler.CharGen_Stage == 1)
                {
                    #region Appearance

                    if (button.Name.Contains("Gender"))
                    {
                        #region Gender

                        if (Gender_Value > 0)
                        {
                            Menu.GetButton("Gender_Plus").Enabled = true;

                            Gender_Value--;
                            if (Gender_Value == 0)
                            {
                                button.Enabled = false;
                            }

                            FirstName_Value = 0;
                            Menu.GetButton("FirstName_Minus").Enabled = false;
                            Menu.GetButton("FirstName_Plus").Enabled = true;

                            if (Gender == "Male")
                            {
                                Gender = "Female";
                                First_Name = CharacterManager.FirstNames_Female[FirstName_Value];
                            }
                            else
                            {
                                Gender = "Male";
                                First_Name = CharacterManager.FirstNames_Male[FirstName_Value];
                            }
                        }

                        Menu.GetLabel("Gender_Value").Text = Gender;
                        Menu.GetLabel("FirstName_Value").Text = First_Name;

                        #endregion
                    }
                    else if (button.Name.Contains("FirstName"))
                    {
                        #region First Name

                        if (FirstName_Value > 0)
                        {
                            Menu.GetButton("FirstName_Plus").Enabled = true;

                            FirstName_Value--;
                            if (FirstName_Value == 0)
                            {
                                button.Enabled = false;
                            }

                            if (Gender == "Male")
                            {
                                First_Name = CharacterManager.FirstNames_Male[FirstName_Value];
                            }
                            else
                            {
                                First_Name = CharacterManager.FirstNames_Female[FirstName_Value];
                            }
                        }

                        Menu.GetLabel("FirstName_Value").Text = First_Name;

                        #endregion
                    }
                    else if (button.Name.Contains("LastName"))
                    {
                        #region Last Name

                        if (LastName_Value > 0)
                        {
                            Menu.GetButton("LastName_Plus").Enabled = true;

                            LastName_Value--;
                            if (LastName_Value == 0)
                            {
                                button.Enabled = false;
                            }

                            Last_Name = CharacterManager.LastNames[LastName_Value];
                        }

                        Menu.GetLabel("LastName_Value").Text = Last_Name;

                        #endregion
                    }
                    else if (button.Name.Contains("Skin"))
                    {
                        #region Skin Color

                        if (SkinColor_Value > 0)
                        {
                            Menu.GetButton("Skin_Plus").Enabled = true;

                            SkinColor_Value--;
                            if (SkinColor_Value == 0)
                            {
                                button.Enabled = false;
                            }
                        }

                        Menu.GetLabel("Skin_Value").Text = Handler.SkinColors[SkinColor_Value];

                        #endregion
                    }
                    else if (button.Name.Contains("HairLength"))
                    {
                        #region Hair Length

                        if (HairLength_Value > 0)
                        {
                            Menu.GetButton("HairLength_Plus").Enabled = true;

                            HairLength_Value--;
                            if (HairLength_Value == 0)
                            {
                                button.Enabled = false;

                                HairColor_Value = -1;
                                Menu.GetButton("HairColor_Minus").Enabled = false;
                                Menu.GetLabel("HairColor_Value").Text = "N/A";
                                Menu.GetButton("HairColor_Plus").Enabled = false;
                            }
                        }

                        Menu.GetLabel("HairLength_Value").Text = Handler.HairLength[HairLength_Value];

                        #endregion
                    }
                    else if (button.Name.Contains("HairColor"))
                    {
                        #region Hair Color

                        if (HairColor_Value > 0)
                        {
                            Menu.GetButton("HairColor_Plus").Enabled = true;

                            HairColor_Value--;
                            if (HairColor_Value == 0)
                            {
                                button.Enabled = false;
                            }
                        }

                        Menu.GetLabel("HairColor_Value").Text = Handler.HairColor[HairColor_Value];

                        #endregion
                    }
                    else if (button.Name.Contains("HatColor"))
                    {
                        #region Hat Color

                        if (HatColor_Value > 0)
                        {
                            Menu.GetButton("HatColor_Plus").Enabled = true;

                            HatColor_Value--;
                            if (HatColor_Value == 0)
                            {
                                button.Enabled = false;
                            }
                        }

                        Menu.GetLabel("HatColor_Value").Text = HatColors[HatColor_Value];

                        #endregion
                    }
                    else if (button.Name.Contains("ShirtColor"))
                    {
                        #region Shirt Color

                        if (ShirtColor_Value > 0)
                        {
                            Menu.GetButton("ShirtColor_Plus").Enabled = true;

                            ShirtColor_Value--;
                            if (ShirtColor_Value == 0)
                            {
                                button.Enabled = false;
                            }
                        }

                        Menu.GetLabel("ShirtColor_Value").Text = Handler.Colors[ShirtColor_Value];

                        #endregion
                    }
                    else if (button.Name.Contains("PantsColor"))
                    {
                        #region Pants Color

                        if (PantsColor_Value > 0)
                        {
                            Menu.GetButton("PantsColor_Plus").Enabled = true;

                            PantsColor_Value--;
                            if (PantsColor_Value == 0)
                            {
                                button.Enabled = false;
                            }
                        }

                        Menu.GetLabel("PantsColor_Value").Text = Handler.Colors[PantsColor_Value];

                        #endregion
                    }
                    else if (button.Name.Contains("ShoesColor"))
                    {
                        #region Shoes Color

                        if (ShoesColor_Value > 0)
                        {
                            Menu.GetButton("ShoesColor_Plus").Enabled = true;

                            ShoesColor_Value--;
                            if (ShoesColor_Value == 0)
                            {
                                button.Enabled = false;
                            }
                        }

                        Menu.GetLabel("ShoesColor_Value").Text = Handler.Colors[ShoesColor_Value];

                        #endregion
                    }

                    UpdateDisplay();

                    #endregion
                }
            }
            else if (button.Name.Contains("Plus"))
            {
                if (Handler.CharGen_Stage == 0)
                {
                    #region Stats

                    if (button.Name.Contains("TownSize"))
                    {
                        Menu.GetButton("TownSize_Minus").Enabled = true;
                        TownSize++;

                        Menu.GetLabel("TownSize_Amount").Text = TownSize.ToString();

                        if (TownSize == 8)
                        {
                            button.Enabled = false;
                        }
                    }
                    else
                    {
                        string[] name = button.Name.Split('_');
                        string stat = name[0];

                        if (Stats[stat] < 100 &&
                            Pool > 0)
                        {
                            Menu.GetButton(stat + "_Minus").Enabled = true;

                            Stats[stat]++;

                            Pool--;
                            Menu.GetLabel("Pool_Amount").Text = Pool.ToString();

                            Menu.GetLabel(stat + "_Amount").Text = Stats[stat].ToString();

                            if (Stats[stat] == 100)
                            {
                                button.Enabled = false;
                            }
                        }
                    }

                    #endregion
                }
                else if (Handler.CharGen_Stage == 1)
                {
                    #region Appearance

                    if (button.Name.Contains("Gender"))
                    {
                        #region Gender

                        if (Gender_Value < 1)
                        {
                            Menu.GetButton("Gender_Minus").Enabled = true;

                            Gender_Value++;
                            if (Gender_Value == 1)
                            {
                                button.Enabled = false;
                            }

                            FirstName_Value = 0;
                            Menu.GetButton("FirstName_Minus").Enabled = false;
                            Menu.GetButton("FirstName_Plus").Enabled = true;

                            if (Gender == "Male")
                            {
                                Gender = "Female";
                                First_Name = CharacterManager.FirstNames_Female[FirstName_Value];
                            }
                            else
                            {
                                Gender = "Male";
                                First_Name = CharacterManager.FirstNames_Male[FirstName_Value];
                            }
                        }

                        Menu.GetLabel("Gender_Value").Text = Gender;
                        Menu.GetLabel("FirstName_Value").Text = First_Name;

                        #endregion
                    }
                    else if (button.Name.Contains("FirstName"))
                    {
                        #region First Name

                        if (Gender == "Male")
                        {
                            if (FirstName_Value < CharacterManager.FirstNames_Male.Count - 1)
                            {
                                Menu.GetButton("FirstName_Minus").Enabled = true;

                                FirstName_Value++;
                                if (FirstName_Value == CharacterManager.FirstNames_Male.Count - 1)
                                {
                                    button.Enabled = false;
                                }

                                First_Name = CharacterManager.FirstNames_Male[FirstName_Value];
                            }
                        }
                        else
                        {
                            if (FirstName_Value < CharacterManager.FirstNames_Female.Count - 1)
                            {
                                Menu.GetButton("FirstName_Minus").Enabled = true;

                                FirstName_Value++;
                                if (FirstName_Value == CharacterManager.FirstNames_Female.Count - 1)
                                {
                                    button.Enabled = false;
                                }

                                First_Name = CharacterManager.FirstNames_Female[FirstName_Value];
                            }
                        }

                        Menu.GetLabel("FirstName_Value").Text = First_Name;

                        #endregion
                    }
                    else if (button.Name.Contains("LastName"))
                    {
                        #region Last Name

                        if (LastName_Value < CharacterManager.LastNames.Count - 1)
                        {
                            Menu.GetButton("LastName_Minus").Enabled = true;

                            LastName_Value++;
                            if (LastName_Value == CharacterManager.LastNames.Count - 1)
                            {
                                button.Enabled = false;
                            }

                            Last_Name = CharacterManager.LastNames[LastName_Value];
                        }

                        Menu.GetLabel("LastName_Value").Text = Last_Name;

                        #endregion
                    }
                    else if (button.Name.Contains("Skin"))
                    {
                        #region Skin Color

                        if (SkinColor_Value < 2)
                        {
                            Menu.GetButton("Skin_Minus").Enabled = true;

                            SkinColor_Value++;
                            if (SkinColor_Value == 2)
                            {
                                button.Enabled = false;
                            }
                        }

                        Menu.GetLabel("Skin_Value").Text = Handler.SkinColors[SkinColor_Value];

                        #endregion
                    }
                    else if (button.Name.Contains("HairLength"))
                    {
                        #region Hair Length

                        if (HairLength_Value < Handler.HairLength.Length - 1)
                        {
                            Menu.GetButton("HairLength_Minus").Enabled = true;

                            if (HairLength_Value == 0)
                            {
                                HairColor_Value = 0;
                                Menu.GetLabel("HairColor_Value").Text = Handler.HairColor[0];
                                Menu.GetButton("HairColor_Plus").Enabled = true;
                            }

                            HairLength_Value++;
                            if (HairLength_Value == Handler.HairLength.Length - 1)
                            {
                                button.Enabled = false;
                            }
                        }

                        Menu.GetLabel("HairLength_Value").Text = Handler.HairLength[HairLength_Value];

                        #endregion
                    }
                    else if (button.Name.Contains("HairColor"))
                    {
                        #region Hair Color

                        if (HairColor_Value < Handler.HairColor.Length - 1)
                        {
                            Menu.GetButton("HairColor_Minus").Enabled = true;

                            HairColor_Value++;
                            if (HairColor_Value == Handler.HairColor.Length - 1)
                            {
                                button.Enabled = false;
                            }
                        }

                        Menu.GetLabel("HairColor_Value").Text = Handler.HairColor[HairColor_Value];

                        #endregion
                    }
                    else if (button.Name.Contains("HatColor"))
                    {
                        #region Hat Color

                        if (HatColor_Value < HatColors.Length - 1)
                        {
                            Menu.GetButton("HatColor_Minus").Enabled = true;

                            HatColor_Value++;
                            if (HatColor_Value == HatColors.Length - 1)
                            {
                                button.Enabled = false;
                            }
                        }

                        Menu.GetLabel("HatColor_Value").Text = HatColors[HatColor_Value];

                        #endregion
                    }
                    else if (button.Name.Contains("ShirtColor"))
                    {
                        #region Shirt Color

                        if (ShirtColor_Value < Handler.Colors.Length - 1)
                        {
                            Menu.GetButton("ShirtColor_Minus").Enabled = true;

                            ShirtColor_Value++;
                            if (ShirtColor_Value == Handler.Colors.Length - 1)
                            {
                                button.Enabled = false;
                            }
                        }

                        Menu.GetLabel("ShirtColor_Value").Text = Handler.Colors[ShirtColor_Value];

                        #endregion
                    }
                    else if (button.Name.Contains("PantsColor"))
                    {
                        #region Pants Color

                        if (PantsColor_Value < Handler.Colors.Length - 1)
                        {
                            Menu.GetButton("PantsColor_Minus").Enabled = true;

                            PantsColor_Value++;
                            if (PantsColor_Value == Handler.Colors.Length - 1)
                            {
                                button.Enabled = false;
                            }
                        }

                        Menu.GetLabel("PantsColor_Value").Text = Handler.Colors[PantsColor_Value];

                        #endregion
                    }
                    else if (button.Name.Contains("ShoesColor"))
                    {
                        #region Shoes Color

                        if (ShoesColor_Value < Handler.Colors.Length - 1)
                        {
                            Menu.GetButton("ShoesColor_Minus").Enabled = true;

                            ShoesColor_Value++;
                            if (ShoesColor_Value == Handler.Colors.Length - 1)
                            {
                                button.Enabled = false;
                            }
                        }

                        Menu.GetLabel("ShoesColor_Value").Text = Handler.Colors[ShoesColor_Value];

                        #endregion
                    }

                    UpdateDisplay();

                    #endregion
                }
            }

            button.Opacity = 0.8f;
            button.Selected = false;
        }

        private void ChangeMenu()
        {
            foreach (Button control in Stats_Buttons)
            {
                control.Visible = false;
            }

            foreach (Label label in Stats_Labels)
            {
                label.Visible = false;
            }

            foreach (Button control in Appearance_Buttons)
            {
                control.Visible = false;
            }

            foreach (Label label in Appearance_Labels)
            {
                label.Visible = false;
            }

            foreach (Picture control in Appearance_Pictures)
            {
                control.Visible = false;
            }

            if (Handler.CharGen_Stage == 0)
            {
                foreach (Button control in Stats_Buttons)
                {
                    control.Visible = true;
                }

                foreach (Label label in Stats_Labels)
                {
                    label.Visible = true;
                }
            }
            else if (Handler.CharGen_Stage == 1)
            {
                foreach (Button control in Appearance_Buttons)
                {
                    control.Visible = true;
                }

                foreach (Label label in Appearance_Labels)
                {
                    label.Visible = true;
                }

                foreach (Picture control in Appearance_Pictures)
                {
                    control.Visible = true;
                }
            }
        }

        private void UpdateDisplay()
        {
            Picture body = Menu.GetPicture("Body");
            body.Texture = AssetManager.Textures["Down_Naked_" + Handler.SkinColors[SkinColor_Value]];
            body.Image = new Rectangle(body.Texture.Width / 4, 0, body.Texture.Width / 4, body.Texture.Height);

            Picture shirt = Menu.GetPicture("Shirt");
            shirt.Texture = AssetManager.Textures["Down_Shirt"];
            shirt.Image = new Rectangle(shirt.Texture.Width / 4, 0, shirt.Texture.Width / 4, shirt.Texture.Height);
            shirt.DrawColor = GameUtil.ColorFromName(Handler.Colors[ShirtColor_Value]);

            Picture pants = Menu.GetPicture("Pants");
            pants.Texture = AssetManager.Textures["Down_Pants"];
            pants.Image = new Rectangle(pants.Texture.Width / 4, 0, pants.Texture.Width / 4, pants.Texture.Height);
            pants.DrawColor = GameUtil.ColorFromName(Handler.Colors[PantsColor_Value]);

            Picture shoes = Menu.GetPicture("Shoes");
            shoes.Texture = AssetManager.Textures["Down_Shoes"];
            shoes.Image = new Rectangle(shoes.Texture.Width / 4, 0, shoes.Texture.Width / 4, shoes.Texture.Height);
            shoes.DrawColor = GameUtil.ColorFromName(Handler.Colors[ShoesColor_Value]);

            Picture hair = Menu.GetPicture("Hair");
            if (HairLength_Value == 0)
            {
                hair.Texture = AssetManager.Textures["Clear"];
                hair.Image = new Rectangle(0, 0, hair.Texture.Width, hair.Texture.Height);
            }
            else
            {
                hair.Texture = AssetManager.Textures["Down_Hair_" + Handler.HairLength[HairLength_Value]];
                hair.Image = new Rectangle(hair.Texture.Width / 4, 0, hair.Texture.Width / 4, hair.Texture.Height);
                hair.DrawColor = GameUtil.ColorFromName(Handler.HairColor[HairColor_Value]);
            }

            Picture hat = Menu.GetPicture("Hat");
            if (HatColor_Value == 0)
            {
                hat.Texture = AssetManager.Textures["Clear"];
                hat.Image = new Rectangle(0, 0, hat.Texture.Width, hat.Texture.Height);
            }
            else
            {
                hat.Texture = AssetManager.Textures["Down_Hat"];
                hat.Image = new Rectangle(hat.Texture.Width / 4, 0, hat.Texture.Width / 4, hat.Texture.Height);
                hat.DrawColor = GameUtil.ColorFromName(HatColors[HatColor_Value]);
            }
        }

        private void Back()
        {
            Handler.Loading_Stage = 3;

            SceneManager.ChangeScene("Title");

            Menu main = MenuManager.GetMenu("Main");
            main.Active = true;
            main.Visible = true;
        }

        private void Finish()
        {
            Squad players = CharacterManager.GetArmy("Characters").GetSquad("Players");

            Character player = new Character();
            player.ID = Handler.GetID();
            player.Name = First_Name + " " + Last_Name;
            player.Type = "Player";
            player.Animator.Frames = 4;
            player.Speed = 1;
            player.Move_TotalDistance = Main.Game.TileSize.X;
            player.Direction = Direction.Down;
            player.Visible = true;
            player.Region.X = (Main.Game.ScreenWidth / 2) - (Main.Game.TileSize.X / 2);
            player.Region.Y = (Main.Game.ScreenHeight / 2) - (Main.Game.TileSize.Y / 2) - (Main.Game.TileSize.Y * 2);
            player.Region.Width = Main.Game.TileSize.X;
            player.Region.Height = Main.Game.TileSize.Y;
            players.Characters.Add(player);

            JobManager.Jobs.Add(player.Job);

            LoadInventory(player);
            CharacterUtil.LoadStats(player, Stats);

            Reset();

            Handler.MapSize_X = TownSize;
            Handler.MapSize_Y = TownSize;

            SceneManager.ChangeScene("Loading");
        }

        private void LoadInventory(Character player)
        {
            player.Gender = Gender;
            player.Texture = AssetManager.Textures["Naked_" + Handler.SkinColors[SkinColor_Value]];
            player.Image = new Rectangle(0, 0, player.Texture.Width / 4, player.Texture.Height / 4);
            player.Inventory.ID = Handler.GetID();
            player.Inventory.Name = player.Name;

            if (HairLength_Value > 0)
            {
                Item hair = new Item();
                hair.Name = Handler.HairLength[HairLength_Value] + " " + Handler.HairColor[HairColor_Value] + " Hair";
                hair.Equipped = true;
                hair.Type = "Hair_" + Handler.HairLength[HairLength_Value];
                hair.Texture = AssetManager.Textures[hair.Type];
                hair.DrawColor = GameUtil.ColorFromName(Handler.HairColor[HairColor_Value]);
                hair.Image = player.Image;
                hair.Region = player.Region;
                hair.Visible = true;
                player.Inventory.Items.Add(hair);
            }

            if (HatColor_Value > 0)
            {
                Item hat = new Item();
                hat.ID = Handler.GetID();
                hat.Name = HatColors[HatColor_Value] + " Hat";
                hat.Type = "Hat";
                hat.Equipped = true;
                hat.Assignment = "Hat Slot";
                hat.Icon = AssetManager.Textures["Hat_Cap"];
                hat.Icon_Image = new Rectangle(0, 0, hat.Icon.Width, hat.Icon.Height);
                hat.Icon_DrawColor = GameUtil.ColorFromName(HatColors[HatColor_Value]);
                hat.Icon_Visible = true;
                hat.Texture = AssetManager.Textures["Hat"];
                hat.DrawColor = GameUtil.ColorFromName(HatColors[HatColor_Value]);
                hat.Image = player.Image;
                hat.Region = player.Region;
                hat.Visible = true;
                player.Inventory.Items.Add(hat);
            }

            Item shirt = new Item();
            shirt.ID = Handler.GetID();
            shirt.Name = Handler.Colors[ShirtColor_Value] + " Shirt";
            shirt.Type = "Shirt";
            shirt.Icon = AssetManager.Textures["Shirt_T"];
            shirt.Icon_Image = new Rectangle(0, 0, shirt.Icon.Width, shirt.Icon.Height);
            shirt.Icon_DrawColor = GameUtil.ColorFromName(Handler.Colors[ShirtColor_Value]);
            shirt.Icon_Visible = true;
            shirt.Texture = AssetManager.Textures["Shirt"];
            shirt.DrawColor = GameUtil.ColorFromName(Handler.Colors[ShirtColor_Value]);
            shirt.Image = player.Image;
            shirt.Region = player.Region;
            shirt.Equipped = true;
            shirt.Assignment = "Shirt Slot";
            shirt.Visible = true;
            player.Inventory.Items.Add(shirt);

            Item pants = new Item();
            pants.ID = Handler.GetID();
            pants.Name = Handler.Colors[PantsColor_Value] + " Pants";
            pants.Type = "Pants";
            pants.Icon = AssetManager.Textures["Pants_Plain"];
            pants.Icon_Image = new Rectangle(0, 0, pants.Icon.Width, pants.Icon.Height);
            pants.Icon_DrawColor = GameUtil.ColorFromName(Handler.Colors[PantsColor_Value]);
            pants.Icon_Visible = true;
            pants.DrawColor = GameUtil.ColorFromName(Handler.Colors[PantsColor_Value]);
            pants.Equipped = true;
            pants.Assignment = "Pants Slot";
            pants.Visible = false;
            pants.Inventory.ID = Handler.GetID();
            pants.Inventory.Name = pants.Name;
            pants.Inventory.Weight = 4;
            pants.Inventory.Max_Value = 4;
            InventoryManager.Inventories.Add(pants.Inventory);
            player.Inventory.Items.Add(pants);

            Item shoes = new Item();
            shoes.ID = Handler.GetID();
            shoes.Name = Handler.Colors[ShoesColor_Value] + " Shoes";
            shoes.Type = "Shoes";
            shoes.Icon = AssetManager.Textures["Shoes_Plain"];
            shoes.DrawColor = GameUtil.ColorFromName(Handler.Colors[ShoesColor_Value]);
            shoes.Icon_Image = new Rectangle(0, 0, shoes.Icon.Width, shoes.Icon.Height);
            shoes.Icon_DrawColor = GameUtil.ColorFromName(Handler.Colors[ShoesColor_Value]);
            shoes.Icon_Visible = true;
            shoes.Equipped = true;
            shoes.Assignment = "Shoes Slot";
            shoes.Visible = false;
            player.Inventory.Items.Add(shoes);

            InventoryManager.Inventories.Add(player.Inventory);
        }

        public override void Load()
        {
            ChangeMenu();
        }

        public override void Load(ContentManager content)
        {
            Menu.Clear();
            Stats.Clear();
            Stats_Buttons.Clear();
            Stats_Labels.Clear();
            Appearance_Buttons.Clear();

            Menu.AddButton(Handler.GetID(), "Back", AssetManager.Textures["Button_Back"], AssetManager.Textures["Button_Back_Hover"], AssetManager.Textures["Button_Back_Disabled"],
                new Region(0, 0, 0, 0), Color.White, true);
            Menu.GetButton("Back").HoverText = "Back";

            Menu.AddButton(Handler.GetID(), "Next", AssetManager.Textures["Button_Next"], AssetManager.Textures["Button_Next_Hover"], AssetManager.Textures["Button_Next_Disabled"],
                new Region(0, 0, 0, 0), Color.White, true);
            Menu.GetButton("Next").HoverText = "Next";

            #region Stats

            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Pool", "Bonus Points:", Color.White, new Region(0, 0, 0, 0), true);
            Stats_Labels.Add(Menu.GetLabel("Pool"));

            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Pool_Amount", Pool.ToString(), Color.White, new Region(0, 0, 0, 0), true);
            Stats_Labels.Add(Menu.GetLabel("Pool_Amount"));

            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "BodyStats", "---Physical Stats---", Color.White, new Region(0, 0, 0, 0), true);
            Stats_Labels.Add(Menu.GetLabel("BodyStats"));

            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "MindStats", "---Mental Stats---", Color.White, new Region(0, 0, 0, 0), true);
            Stats_Labels.Add(Menu.GetLabel("MindStats"));

            foreach (var stat in Handler.Stats)
            {
                Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), stat.Key, stat.Key + ":", Color.White, new Region(0, 0, 0, 0), true);
                Menu.GetLabel(stat.Key).HoverText = stat.Value;
                Stats_Labels.Add(Menu.GetLabel(stat.Key));

                Menu.AddButton(Handler.GetID(), stat.Key + "_Minus", AssetManager.Textures["Button_Remove"], AssetManager.Textures["Button_Remove_Hover"], AssetManager.Textures["Button_Remove_Disabled"],
                    new Region(0, 0, 0, 0), Color.White, true);
                Stats_Buttons.Add(Menu.GetButton(stat.Key + "_Minus"));

                Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), stat.Key + "_Amount", "50", Color.White, new Region(0, 0, 0, 0), true);
                Stats_Labels.Add(Menu.GetLabel(stat.Key + "_Amount"));

                Menu.AddButton(Handler.GetID(), stat.Key + "_Plus", AssetManager.Textures["Button_Add"], AssetManager.Textures["Button_Add_Hover"], AssetManager.Textures["Button_Add_Disabled"],
                    new Region(0, 0, 0, 0), Color.White, true);
                Stats_Buttons.Add(Menu.GetButton(stat.Key + "_Plus"));

                Stats.Add(stat.Key, 50);
            }

            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "TownSize", "Town Size:", Color.White, new Region(0, 0, 0, 0), true);
            Stats_Labels.Add(Menu.GetLabel("TownSize"));

            Menu.AddButton(Handler.GetID(), "TownSize_Minus", AssetManager.Textures["Button_Remove"], AssetManager.Textures["Button_Remove_Hover"], AssetManager.Textures["Button_Remove_Disabled"],
                new Region(0, 0, 0, 0), Color.White, true);
            Stats_Buttons.Add(Menu.GetButton("TownSize_Minus"));

            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "TownSize_Amount", TownSize.ToString(), Color.White, new Region(0, 0, 0, 0), true);
            Stats_Labels.Add(Menu.GetLabel("TownSize_Amount"));

            Menu.AddButton(Handler.GetID(), "TownSize_Plus", AssetManager.Textures["Button_Add"], AssetManager.Textures["Button_Add_Hover"], AssetManager.Textures["Button_Add_Disabled"],
                new Region(0, 0, 0, 0), Color.White, true);
            Button townsize_plus = Menu.GetButton("TownSize_Plus");
            //townsize_plus.Enabled = false;
            Stats_Buttons.Add(townsize_plus);

            #endregion

            #region Appearance

            //Gender
            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Gender", "Gender:", Color.White, new Region(0, 0, 0, 0), false);
            Appearance_Labels.Add(Menu.GetLabel("Gender"));

            Menu.AddButton(Handler.GetID(), "Gender_Minus", AssetManager.Textures["Button_Remove"], AssetManager.Textures["Button_Remove_Hover"], AssetManager.Textures["Button_Remove_Disabled"],
                new Region(0, 0, 0, 0), Color.White, false);
            Button gender_minus = Menu.GetButton("Gender_Minus");
            gender_minus.Enabled = false;
            Appearance_Buttons.Add(gender_minus);

            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Gender_Value", Gender, Color.White, new Region(0, 0, 0, 0), false);
            Appearance_Labels.Add(Menu.GetLabel("Gender_Value"));

            Menu.AddButton(Handler.GetID(), "Gender_Plus", AssetManager.Textures["Button_Add"], AssetManager.Textures["Button_Add_Hover"], AssetManager.Textures["Button_Add_Disabled"],
                new Region(0, 0, 0, 0), Color.White, false);
            Appearance_Buttons.Add(Menu.GetButton("Gender_Plus"));

            //First Name
            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "FirstName", "First Name:", Color.White, new Region(0, 0, 0, 0), false);
            Appearance_Labels.Add(Menu.GetLabel("FirstName"));

            Menu.AddButton(Handler.GetID(), "FirstName_Minus", AssetManager.Textures["Button_Remove"], AssetManager.Textures["Button_Remove_Hover"], AssetManager.Textures["Button_Remove_Disabled"],
                new Region(0, 0, 0, 0), Color.White, false);
            Button first_name_minus = Menu.GetButton("FirstName_Minus");
            first_name_minus.Enabled = false;
            Appearance_Buttons.Add(first_name_minus);

            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "FirstName_Value", CharacterManager.FirstNames_Male[FirstName_Value], Color.White, new Region(0, 0, 0, 0), false);
            Appearance_Labels.Add(Menu.GetLabel("FirstName_Value"));
            First_Name = CharacterManager.FirstNames_Male[FirstName_Value];

            Menu.AddButton(Handler.GetID(), "FirstName_Plus", AssetManager.Textures["Button_Add"], AssetManager.Textures["Button_Add_Hover"], AssetManager.Textures["Button_Add_Disabled"],
                new Region(0, 0, 0, 0), Color.White, false);
            Appearance_Buttons.Add(Menu.GetButton("FirstName_Plus"));

            //Last Name
            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "LastName", "Last Name:", Color.White, new Region(0, 0, 0, 0), false);
            Appearance_Labels.Add(Menu.GetLabel("LastName"));

            Menu.AddButton(Handler.GetID(), "LastName_Minus", AssetManager.Textures["Button_Remove"], AssetManager.Textures["Button_Remove_Hover"], AssetManager.Textures["Button_Remove_Disabled"],
                new Region(0, 0, 0, 0), Color.White, false);
            Button last_name_minus = Menu.GetButton("LastName_Minus");
            last_name_minus.Enabled = false;
            Appearance_Buttons.Add(last_name_minus);

            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "LastName_Value", CharacterManager.LastNames[LastName_Value], Color.White, new Region(0, 0, 0, 0), false);
            Appearance_Labels.Add(Menu.GetLabel("LastName_Value"));
            Last_Name = CharacterManager.LastNames[LastName_Value];

            Menu.AddButton(Handler.GetID(), "LastName_Plus", AssetManager.Textures["Button_Add"], AssetManager.Textures["Button_Add_Hover"], AssetManager.Textures["Button_Add_Disabled"],
                new Region(0, 0, 0, 0), Color.White, false);
            Appearance_Buttons.Add(Menu.GetButton("LastName_Plus"));

            //Skin Color
            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Skin", "Skin Color:", Color.White, new Region(0, 0, 0, 0), false);
            Appearance_Labels.Add(Menu.GetLabel("Skin"));

            Menu.AddButton(Handler.GetID(), "Skin_Minus", AssetManager.Textures["Button_Remove"], AssetManager.Textures["Button_Remove_Hover"], AssetManager.Textures["Button_Remove_Disabled"],
                new Region(0, 0, 0, 0), Color.White, false);
            Button skin_minus = Menu.GetButton("Skin_Minus");
            skin_minus.Enabled = false;
            Appearance_Buttons.Add(skin_minus);

            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Skin_Value", Handler.SkinColors[SkinColor_Value], Color.White, new Region(0, 0, 0, 0), false);
            Appearance_Labels.Add(Menu.GetLabel("Skin_Value"));

            Menu.AddButton(Handler.GetID(), "Skin_Plus", AssetManager.Textures["Button_Add"], AssetManager.Textures["Button_Add_Hover"], AssetManager.Textures["Button_Add_Disabled"],
                new Region(0, 0, 0, 0), Color.White, false);
            Appearance_Buttons.Add(Menu.GetButton("Skin_Plus"));

            //Hair Length
            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "HairLength", "Hair Style:", Color.White, new Region(0, 0, 0, 0), false);
            Appearance_Labels.Add(Menu.GetLabel("HairLength"));

            Menu.AddButton(Handler.GetID(), "HairLength_Minus", AssetManager.Textures["Button_Remove"], AssetManager.Textures["Button_Remove_Hover"], AssetManager.Textures["Button_Remove_Disabled"],
                new Region(0, 0, 0, 0), Color.White, false);
            Button hair_length_minus = Menu.GetButton("HairLength_Minus");
            hair_length_minus.Enabled = false;
            Appearance_Buttons.Add(hair_length_minus);

            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "HairLength_Value", Handler.HairLength[0], Color.White, new Region(0, 0, 0, 0), false);
            Appearance_Labels.Add(Menu.GetLabel("HairLength_Value"));

            Menu.AddButton(Handler.GetID(), "HairLength_Plus", AssetManager.Textures["Button_Add"], AssetManager.Textures["Button_Add_Hover"], AssetManager.Textures["Button_Add_Disabled"],
                new Region(0, 0, 0, 0), Color.White, false);
            Appearance_Buttons.Add(Menu.GetButton("HairLength_Plus"));

            //Hair Color
            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "HairColor", "Hair Color:", Color.White, new Region(0, 0, 0, 0), false);
            Appearance_Labels.Add(Menu.GetLabel("HairColor"));

            Menu.AddButton(Handler.GetID(), "HairColor_Minus", AssetManager.Textures["Button_Remove"], AssetManager.Textures["Button_Remove_Hover"], AssetManager.Textures["Button_Remove_Disabled"],
                new Region(0, 0, 0, 0), Color.White, false);
            Button hair_color_minus = Menu.GetButton("HairColor_Minus");
            hair_color_minus.Enabled = false;
            Appearance_Buttons.Add(hair_color_minus);

            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "HairColor_Value", "N/A", Color.White, new Region(0, 0, 0, 0), false);
            Appearance_Labels.Add(Menu.GetLabel("HairColor_Value"));

            Menu.AddButton(Handler.GetID(), "HairColor_Plus", AssetManager.Textures["Button_Add"], AssetManager.Textures["Button_Add_Hover"], AssetManager.Textures["Button_Add_Disabled"],
                new Region(0, 0, 0, 0), Color.White, false);
            Button hair_color_plus = Menu.GetButton("HairColor_Plus");
            hair_color_plus.Enabled = false;
            Appearance_Buttons.Add(hair_color_plus);

            //Hat Color
            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "HatColor", "Hat Color:", Color.White, new Region(0, 0, 0, 0), false);
            Appearance_Labels.Add(Menu.GetLabel("HatColor"));

            Menu.AddButton(Handler.GetID(), "HatColor_Minus", AssetManager.Textures["Button_Remove"], AssetManager.Textures["Button_Remove_Hover"], AssetManager.Textures["Button_Remove_Disabled"],
                new Region(0, 0, 0, 0), Color.White, false);
            Button hat_color_minus = Menu.GetButton("HatColor_Minus");
            hat_color_minus.Enabled = false;
            Appearance_Buttons.Add(hat_color_minus);

            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "HatColor_Value", HatColors[0], Color.White, new Region(0, 0, 0, 0), false);
            Appearance_Labels.Add(Menu.GetLabel("HatColor_Value"));

            Menu.AddButton(Handler.GetID(), "HatColor_Plus", AssetManager.Textures["Button_Add"], AssetManager.Textures["Button_Add_Hover"], AssetManager.Textures["Button_Add_Disabled"],
                new Region(0, 0, 0, 0), Color.White, false);
            Appearance_Buttons.Add(Menu.GetButton("HatColor_Plus"));

            //Shirt Color
            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "ShirtColor", "Shirt Color:", Color.White, new Region(0, 0, 0, 0), false);
            Appearance_Labels.Add(Menu.GetLabel("ShirtColor"));

            Menu.AddButton(Handler.GetID(), "ShirtColor_Minus", AssetManager.Textures["Button_Remove"], AssetManager.Textures["Button_Remove_Hover"], AssetManager.Textures["Button_Remove_Disabled"],
                new Region(0, 0, 0, 0), Color.White, false);
            Button shirt_color_minus = Menu.GetButton("ShirtColor_Minus");
            shirt_color_minus.Enabled = false;
            Appearance_Buttons.Add(shirt_color_minus);

            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "ShirtColor_Value", Handler.Colors[0], Color.White, new Region(0, 0, 0, 0), false);
            Appearance_Labels.Add(Menu.GetLabel("ShirtColor_Value"));

            Menu.AddButton(Handler.GetID(), "ShirtColor_Plus", AssetManager.Textures["Button_Add"], AssetManager.Textures["Button_Add_Hover"], AssetManager.Textures["Button_Add_Disabled"],
                new Region(0, 0, 0, 0), Color.White, false);
            Appearance_Buttons.Add(Menu.GetButton("ShirtColor_Plus"));

            //Pants Color
            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "PantsColor", "Pants Color:", Color.White, new Region(0, 0, 0, 0), false);
            Appearance_Labels.Add(Menu.GetLabel("PantsColor"));

            Menu.AddButton(Handler.GetID(), "PantsColor_Minus", AssetManager.Textures["Button_Remove"], AssetManager.Textures["Button_Remove_Hover"], AssetManager.Textures["Button_Remove_Disabled"],
                new Region(0, 0, 0, 0), Color.White, false);
            Button pants_color_minus = Menu.GetButton("PantsColor_Minus");
            pants_color_minus.Enabled = false;
            Appearance_Buttons.Add(pants_color_minus);

            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "PantsColor_Value", Handler.Colors[0], Color.White, new Region(0, 0, 0, 0), false);
            Appearance_Labels.Add(Menu.GetLabel("PantsColor_Value"));

            Menu.AddButton(Handler.GetID(), "PantsColor_Plus", AssetManager.Textures["Button_Add"], AssetManager.Textures["Button_Add_Hover"], AssetManager.Textures["Button_Add_Disabled"],
                new Region(0, 0, 0, 0), Color.White, false);
            Appearance_Buttons.Add(Menu.GetButton("PantsColor_Plus"));

            //Shoes Color
            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "ShoesColor", "Shoes Color:", Color.White, new Region(0, 0, 0, 0), false);
            Appearance_Labels.Add(Menu.GetLabel("ShoesColor"));

            Menu.AddButton(Handler.GetID(), "ShoesColor_Minus", AssetManager.Textures["Button_Remove"], AssetManager.Textures["Button_Remove_Hover"], AssetManager.Textures["Button_Remove_Disabled"],
                new Region(0, 0, 0, 0), Color.White, false);
            Button shoes_color_minus = Menu.GetButton("ShoesColor_Minus");
            shoes_color_minus.Enabled = false;
            Appearance_Buttons.Add(shoes_color_minus);

            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "ShoesColor_Value", Handler.Colors[0], Color.White, new Region(0, 0, 0, 0), false);
            Appearance_Labels.Add(Menu.GetLabel("ShoesColor_Value"));

            Menu.AddButton(Handler.GetID(), "ShoesColor_Plus", AssetManager.Textures["Button_Add"], AssetManager.Textures["Button_Add_Hover"], AssetManager.Textures["Button_Add_Disabled"],
                new Region(0, 0, 0, 0), Color.White, false);
            Appearance_Buttons.Add(Menu.GetButton("ShoesColor_Plus"));

            #endregion

            #region Display

            //Menu.AddPicture(Handler.GetID(), "Grass", Handler.Textures["Grass"], new Rectangle(0, 0, 0, 0), Color.White, false);
            //Appearance_Pictures.Add(Menu.GetPicture("Grass"));

            Menu.AddPicture(Handler.GetID(), "Body", AssetManager.Textures["Down_Naked_Light"], new Region(0, 0, 0, 0), Color.White, false);
            Picture body = Menu.GetPicture("Body");
            body.Image = new Rectangle(body.Texture.Width / 4, 0, body.Texture.Width / 4, body.Texture.Height);
            Appearance_Pictures.Add(Menu.GetPicture("Body"));

            Menu.AddPicture(Handler.GetID(), "Shirt", AssetManager.Textures["Down_Shirt"], new Region(0, 0, 0, 0), GameUtil.ColorFromName("Black"), false);
            Picture shirt = Menu.GetPicture("Shirt");
            shirt.Image = new Rectangle(shirt.Texture.Width / 4, 0, shirt.Texture.Width / 4, shirt.Texture.Height);
            Appearance_Pictures.Add(Menu.GetPicture("Shirt"));

            Menu.AddPicture(Handler.GetID(), "Pants", AssetManager.Textures["Down_Pants"], new Region(0, 0, 0, 0), GameUtil.ColorFromName("Black"), false);
            Picture pants = Menu.GetPicture("Pants");
            pants.Image = new Rectangle(pants.Texture.Width / 4, 0, pants.Texture.Width / 4, pants.Texture.Height);
            Appearance_Pictures.Add(Menu.GetPicture("Pants"));

            Menu.AddPicture(Handler.GetID(), "Shoes", AssetManager.Textures["Down_Shoes"], new Region(0, 0, 0, 0), GameUtil.ColorFromName("Black"), false);
            Picture shoes = Menu.GetPicture("Shoes");
            shoes.Image = new Rectangle(shoes.Texture.Width / 4, 0, shoes.Texture.Width / 4, shoes.Texture.Height);
            Appearance_Pictures.Add(Menu.GetPicture("Shoes"));

            Menu.AddPicture(Handler.GetID(), "Hair", AssetManager.Textures["Clear"], new Region(0, 0, 0, 0), Color.White, false);
            Appearance_Pictures.Add(Menu.GetPicture("Hair"));

            Menu.AddPicture(Handler.GetID(), "Hat", AssetManager.Textures["Clear"], new Region(0, 0, 0, 0), Color.White, false);
            Appearance_Pictures.Add(Menu.GetPicture("Hat"));

            #endregion

            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["Frame"], new Region(0, 0, 0, 0), false);
            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "ExamineStat", "", Color.White, AssetManager.Textures["Frame_Wide"], new Region(0, 0, 0, 0), false);

            Menu.Visible = true;

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            int Width = Main.Game.MenuSize_X;
            int Height = Main.Game.MenuSize_Y;

            int x = Main.Game.ScreenWidth / 2;
            int y = (Main.Game.ScreenHeight / 40) * 29;

            Menu.GetButton("Back").Region = new Region(x - (Width * 2), y + (Height * 3), Width, Height);
            Menu.GetButton("Next").Region = new Region(x + Width, y + (Height * 3), Width, Height);

            #region Stats

            x = Main.Game.ScreenWidth / 2;
            y = (Main.Game.ScreenHeight / 40);

            Menu.GetLabel("Pool").Region = new Region(x - (Width * 4), y, Width * 6, Height);
            Menu.GetLabel("Pool_Amount").Region = new Region(x + (Width * 2), y, Width, Height);
            y += (Main.Game.ScreenHeight / 40) * 6;

            Menu.GetLabel("BodyStats").Region = new Region(x - (Width * 9), y, Width * 7, Height);
            Menu.GetLabel("MindStats").Region = new Region(x + (Width * 2), y, Width * 7, Height);
            y += (Main.Game.ScreenHeight / 40) * 3;

            //Strength
            Menu.GetLabel("Strength").Region = new Region(x - (Width * 9), y, Width * 4, Height);
            Menu.GetButton("Strength_Minus").Region = new Region(x - (Width * 5), y, Width, Height);
            Menu.GetLabel("Strength_Amount").Region = new Region(x - (Width * 4), y, Width, Height);
            Menu.GetButton("Strength_Plus").Region = new Region(x - (Width * 3), y, Width, Height);

            //Intelligence
            Menu.GetLabel("Intelligence").Region = new Region(x + (Width * 2), y, Width * 4, Height);
            Menu.GetButton("Intelligence_Minus").Region = new Region(x + (Width * 6), y, Width, Height);
            Menu.GetLabel("Intelligence_Amount").Region = new Region(x + (Width * 7), y, Width, Height);
            Menu.GetButton("Intelligence_Plus").Region = new Region(x + (Width * 8), y, Width, Height);
            y += (Main.Game.ScreenHeight / 40) * 3;

            //Vitality
            Menu.GetLabel("Vitality").Region = new Region(x - (Width * 9), y, Width * 4, Height);
            Menu.GetButton("Vitality_Minus").Region = new Region(x - (Width * 5), y, Width, Height);
            Menu.GetLabel("Vitality_Amount").Region = new Region(x - (Width * 4), y, Width, Height);
            Menu.GetButton("Vitality_Plus").Region = new Region(x - (Width * 3), y, Width, Height);

            //Perception
            Menu.GetLabel("Perception").Region = new Region(x + (Width * 2), y, Width * 4, Height);
            Menu.GetButton("Perception_Minus").Region = new Region(x + (Width * 6), y, Width, Height);
            Menu.GetLabel("Perception_Amount").Region = new Region(x + (Width * 7), y, Width, Height);
            Menu.GetButton("Perception_Plus").Region = new Region(x + (Width * 8), y, Width, Height);
            y += (Main.Game.ScreenHeight / 40) * 3;

            //Endurance
            Menu.GetLabel("Endurance").Region = new Region(x - (Width * 9), y, Width * 4, Height);
            Menu.GetButton("Endurance_Minus").Region = new Region(x - (Width * 5), y, Width, Height);
            Menu.GetLabel("Endurance_Amount").Region = new Region(x - (Width * 4), y, Width, Height);
            Menu.GetButton("Endurance_Plus").Region = new Region(x - (Width * 3), y, Width, Height);

            //Charisma
            Menu.GetLabel("Charisma").Region = new Region(x + (Width * 2), y, Width * 4, Height);
            Menu.GetButton("Charisma_Minus").Region = new Region(x + (Width * 6), y, Width, Height);
            Menu.GetLabel("Charisma_Amount").Region = new Region(x + (Width * 7), y, Width, Height);
            Menu.GetButton("Charisma_Plus").Region = new Region(x + (Width * 8), y, Width, Height);
            y += (Main.Game.ScreenHeight / 40) * 3;

            //Agility
            Menu.GetLabel("Agility").Region = new Region(x - (Width * 9), y, Width * 4, Height);
            Menu.GetButton("Agility_Minus").Region = new Region(x - (Width * 5), y, Width, Height);
            Menu.GetLabel("Agility_Amount").Region = new Region(x - (Width * 4), y, Width, Height);
            Menu.GetButton("Agility_Plus").Region = new Region(x - (Width * 3), y, Width, Height);

            //Willpower
            Menu.GetLabel("Willpower").Region = new Region(x + (Width * 2), y, Width * 4, Height);
            Menu.GetButton("Willpower_Minus").Region = new Region(x + (Width * 6), y, Width, Height);
            Menu.GetLabel("Willpower_Amount").Region = new Region(x + (Width * 7), y, Width, Height);
            Menu.GetButton("Willpower_Plus").Region = new Region(x + (Width * 8), y, Width, Height);
            y += (Main.Game.ScreenHeight / 40) * 3;

            //Luck
            Menu.GetLabel("Luck").Region = new Region(x - (Width * 9), y, Width * 4, Height);
            Menu.GetButton("Luck_Minus").Region = new Region(x - (Width * 5), y, Width, Height);
            Menu.GetLabel("Luck_Amount").Region = new Region(x - (Width * 4), y, Width, Height);
            Menu.GetButton("Luck_Plus").Region = new Region(x - (Width * 3), y, Width, Height);

            //Sanity
            Menu.GetLabel("Sanity").Region = new Region(x + (Width * 2), y, Width * 4, Height);
            Menu.GetButton("Sanity_Minus").Region = new Region(x + (Width * 6), y, Width, Height);
            Menu.GetLabel("Sanity_Amount").Region = new Region(x + (Width * 7), y, Width, Height);
            Menu.GetButton("Sanity_Plus").Region = new Region(x + (Width * 8), y, Width, Height);
            y += (Main.Game.ScreenHeight / 40) * 6;

            //TownSize
            Menu.GetLabel("TownSize").Region = new Region(x - (Width * 4), y, Width * 4, Height);
            Menu.GetButton("TownSize_Minus").Region = new Region(x + (Width * 0), y, Width, Height);
            Menu.GetLabel("TownSize_Amount").Region = new Region(x + (Width * 1), y, Width, Height);
            Menu.GetButton("TownSize_Plus").Region = new Region(x + (Width * 2), y, Width, Height);

            #endregion

            #region Appearance

            x = Main.Game.ScreenWidth / 2;
            y = (Main.Game.ScreenHeight / 40) * 9;

            //Gender
            Menu.GetLabel("Gender").Region = new Region(x - (Width * 10), y, Width * 4, Height);
            Menu.GetButton("Gender_Minus").Region = new Region(x - (Width * 6), y, Width, Height);
            Menu.GetLabel("Gender_Value").Region = new Region(x - (Width * 5), y, (Width * 2), Height);
            Menu.GetButton("Gender_Plus").Region = new Region(x - (Width * 3), y, Width, Height);

            //Hair Color
            Menu.GetLabel("HairColor").Region = new Region(x + (Width * 2), y, Width * 4, Height);
            Menu.GetButton("HairColor_Minus").Region = new Region(x + (Width * 6), y, Width, Height);
            Menu.GetLabel("HairColor_Value").Region = new Region(x + (Width * 7), y, (Width * 2), Height);
            Menu.GetButton("HairColor_Plus").Region = new Region(x + (Width * 9), y, Width, Height);
            y += (Main.Game.ScreenHeight / 40) * 3;

            //First Name
            Menu.GetLabel("FirstName").Region = new Region(x - (Width * 10), y, Width * 4, Height);
            Menu.GetButton("FirstName_Minus").Region = new Region(x - (Width * 6), y, Width, Height);
            Menu.GetLabel("FirstName_Value").Region = new Region(x - (Width * 5), y, (Width * 2), Height);
            Menu.GetButton("FirstName_Plus").Region = new Region(x - (Width * 3), y, Width, Height);

            //Hat Color
            Menu.GetLabel("HatColor").Region = new Region(x + (Width * 2), y, Width * 4, Height);
            Menu.GetButton("HatColor_Minus").Region = new Region(x + (Width * 6), y, Width, Height);
            Menu.GetLabel("HatColor_Value").Region = new Region(x + (Width * 7), y, (Width * 2), Height);
            Menu.GetButton("HatColor_Plus").Region = new Region(x + (Width * 9), y, Width, Height);
            y += (Main.Game.ScreenHeight / 40) * 3;

            //Last Name
            Menu.GetLabel("LastName").Region = new Region(x - (Width * 10), y, Width * 4, Height);
            Menu.GetButton("LastName_Minus").Region = new Region(x - (Width * 6), y, Width, Height);
            Menu.GetLabel("LastName_Value").Region = new Region(x - (Width * 5), y, (Width * 2), Height);
            Menu.GetButton("LastName_Plus").Region = new Region(x - (Width * 3), y, Width, Height);

            //Shirt Color
            Menu.GetLabel("ShirtColor").Region = new Region(x + (Width * 2), y, Width * 4, Height);
            Menu.GetButton("ShirtColor_Minus").Region = new Region(x + (Width * 6), y, Width, Height);
            Menu.GetLabel("ShirtColor_Value").Region = new Region(x + (Width * 7), y, (Width * 2), Height);
            Menu.GetButton("ShirtColor_Plus").Region = new Region(x + (Width * 9), y, Width, Height);
            y += (Main.Game.ScreenHeight / 40) * 3;

            //Skin Color
            Menu.GetLabel("Skin").Region = new Region(x - (Width * 10), y, Width * 4, Height);
            Menu.GetButton("Skin_Minus").Region = new Region(x - (Width * 6), y, Width, Height);
            Menu.GetLabel("Skin_Value").Region = new Region(x - (Width * 5), y, (Width * 2), Height);
            Menu.GetButton("Skin_Plus").Region = new Region(x - (Width * 3), y, Width, Height);

            //Pants Color
            Menu.GetLabel("PantsColor").Region = new Region(x + (Width * 2), y, Width * 4, Height);
            Menu.GetButton("PantsColor_Minus").Region = new Region(x + (Width * 6), y, Width, Height);
            Menu.GetLabel("PantsColor_Value").Region = new Region(x + (Width * 7), y, (Width * 2), Height);
            Menu.GetButton("PantsColor_Plus").Region = new Region(x + (Width * 9), y, Width, Height);
            y += (Main.Game.ScreenHeight / 40) * 3;

            //Hair Length
            Menu.GetLabel("HairLength").Region = new Region(x - (Width * 10), y, Width * 4, Height);
            Menu.GetButton("HairLength_Minus").Region = new Region(x - (Width * 6), y, Width, Height);
            Menu.GetLabel("HairLength_Value").Region = new Region(x - (Width * 5), y, (Width * 2), Height);
            Menu.GetButton("HairLength_Plus").Region = new Region(x - (Width * 3), y, Width, Height);

            //Shoes Color
            Menu.GetLabel("ShoesColor").Region = new Region(x + (Width * 2), y, Width * 4, Height);
            Menu.GetButton("ShoesColor_Minus").Region = new Region(x + (Width * 6), y, Width, Height);
            Menu.GetLabel("ShoesColor_Value").Region = new Region(x + (Width * 7), y, (Width * 2), Height);
            Menu.GetButton("ShoesColor_Plus").Region = new Region(x + (Width * 9), y, Width, Height);
            y += (Main.Game.ScreenHeight / 40) * 3;

            #endregion

            #region Display

            x = Main.Game.ScreenWidth / 2;
            y = (Main.Game.ScreenHeight / 40) * 31;

            //Menu.GetPicture("Grass").Region = new Rectangle(x - Width, y - Height, (Width * 2), (Height * 2));
            Menu.GetPicture("Body").Region = new Region(x - Width, y - Height, (Width * 2), (Height * 2));
            Menu.GetPicture("Shirt").Region = new Region(x - Width, y - Height, (Width * 2), (Height * 2));
            Menu.GetPicture("Pants").Region = new Region(x - Width, y - Height, (Width * 2), (Height * 2));
            Menu.GetPicture("Shoes").Region = new Region(x - Width, y - Height, (Width * 2), (Height * 2));
            Menu.GetPicture("Hair").Region = new Region(x - Width, y - Height, (Width * 2), (Height * 2));
            Menu.GetPicture("Hat").Region = new Region(x - Width, y - Height, (Width * 2), (Height * 2));

            #endregion
        }

        private void Examine(string text)
        {
            Label examine = Menu.GetLabel("ExamineStat");
            examine.Text = text;

            int width = Main.Game.MenuSize_X * 14;
            int height = Main.Game.MenuSize_Y;

            int X = InputManager.Mouse.X - (width / 2);
            if (X < 0)
            {
                X = 0;
            }
            else if (X > Main.Game.Resolution.X - width)
            {
                X = Main.Game.Resolution.X - width;
            }

            int Y = InputManager.Mouse.Y + 20;
            if (Y < 0)
            {
                Y = 0;
            }
            else if (Y > Main.Game.Resolution.Y - height)
            {
                Y = Main.Game.Resolution.Y - height;
            }

            examine.Region = new Region(X, Y, width, height);
            examine.Visible = true;
        }

        private void Reset()
        {
            foreach (Button control in Stats_Buttons)
            {
                control.Visible = true;
            }

            foreach (Label label in Stats_Labels)
            {
                label.Visible = true;
            }

            foreach (Button control in Appearance_Buttons)
            {
                control.Visible = false;
            }

            foreach (Label label in Appearance_Labels)
            {
                label.Visible = false;
            }

            foreach (Picture control in Appearance_Pictures)
            {
                control.Visible = false;
            }
        }

        #endregion
    }
}

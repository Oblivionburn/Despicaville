using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Menus;
using OP_Engine.Scenes;
using OP_Engine.Characters;
using OP_Engine.Inventories;
using OP_Engine.Utility;
using OP_Engine.Enums;
using Despicaville.Util;

namespace Despicaville.Scenes
{
    public class CharGen : Scene
    {
        #region Variables

        public int Pool = 50;
        public int TownSize = 7;
        public static Dictionary<string, int> Stats = [];

        public List<Label> Stats_Labels = [];
        public List<Button> Stats_Buttons = [];

        public List<Button> Appearance_Buttons = [];
        public List<Picture> Appearance_Pictures = [];
        public List<Label> Appearance_Labels = [];

        public string? Gender = "Male";
        public int Gender_Value = 0;
        public string? First_Name = "";
        public int FirstName_Value = 0;
        public string? Last_Name = "";
        public int LastName_Value = 0;
        public int SkinColor_Value = 0;
        public int HairLength_Value = 0;
        public int HairColor_Value = -1;
        public int HatColor_Value = 0;
        public string[] HatColors = ["No Hat", "Black", "Blue", "Brown", "Cyan", "Green", "Grey", "Orange", "Pink", "Purple", "Red", "White", "Yellow"];
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

        public override void Update(Game? gameRef, ContentManager? content)
        {
            if (Visible)
            {
                UpdateControls();
                base.Update(gameRef, content);
            }
        }

        private void UpdateControls()
        {
            if (Menu == null)
            {
                return;
            }

            bool label_found = false;

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
                    if (label.Region != null &&
                        InputManager.MouseWithin(label.Region.ToRectangle))
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
                Label? examine = Menu.GetLabel("Examine");
                if (examine != null)
                {
                    examine.Visible = false;
                }

                Label? examineStat = Menu.GetLabel("ExamineStat");
                if (examineStat != null)
                {
                    examineStat.Visible = false;
                }
            }
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");

            if (button.Name == null)
            {
                return;
            }

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
            else
            {
                if (button.Name.Contains("Minus") ||
                    button.Name.Contains("Plus"))
                {
                    Label? pool_Amount = Menu?.GetLabel("Pool_Amount");

                    Button? townSize_Minus = Menu?.GetButton("TownSize_Minus");
                    Button? townSize_Plus = Menu?.GetButton("TownSize_Plus");
                    Label? townSize_Amount = Menu?.GetLabel("TownSize_Amount");

                    Button? gender_Minus = Menu?.GetButton("Gender_Minus");
                    Button? gender_plus = Menu?.GetButton("Gender_Plus");
                    Label? gender_Value = Menu?.GetLabel("Gender_Value");

                    Button? firstName_Minus = Menu?.GetButton("FirstName_Minus");
                    Button? firstName_Plus = Menu?.GetButton("FirstName_Plus");
                    Label? firstName_Value = Menu?.GetLabel("FirstName_Value");

                    Button? lastName_Minus = Menu?.GetButton("LastName_Minus");
                    Button? lastName_Plus = Menu?.GetButton("LastName_Plus");
                    Label? lastName_Value = Menu?.GetLabel("LastName_Value");

                    Button? skin_Minus = Menu?.GetButton("Skin_Minus");
                    Button? skin_Plus = Menu?.GetButton("Skin_Plus");
                    Label? skin_Value = Menu?.GetLabel("Skin_Value");

                    Button? hairLength_Minus = Menu?.GetButton("HairLength_Minus");
                    Button? hairLength_Plus = Menu?.GetButton("HairLength_Plus");
                    Label? hairLength_Value = Menu?.GetLabel("HairLength_Value");

                    Button? hairColor_Minus = Menu?.GetButton("HairColor_Minus");
                    Button? hairColor_Plus = Menu?.GetButton("HairColor_Plus");
                    Label? hairColor_Value = Menu?.GetLabel("HairColor_Value");

                    Button? hatColor_Minus = Menu?.GetButton("HatColor_Minus");
                    Button? hatColor_Plus = Menu?.GetButton("HatColor_Plus");
                    Label? hatColor_Value = Menu?.GetLabel("HatColor_Value");

                    Button? shirtColor_Minus = Menu?.GetButton("ShirtColor_Minus");
                    Button? shirtColor_Plus = Menu?.GetButton("ShirtColor_Plus");
                    Label? shirtColor_Value = Menu?.GetLabel("ShirtColor_Value");

                    Button? pantsColor_Minus = Menu?.GetButton("PantsColor_Minus");
                    Button? pantsColor_Plus = Menu?.GetButton("PantsColor_Plus");
                    Label? pantsColor_Value = Menu?.GetLabel("PantsColor_Value");

                    Button? shoesColor_Minus = Menu?.GetButton("ShoesColor_Minus");
                    Button? shoesColor_Plus = Menu?.GetButton("ShoesColor_Plus");
                    Label? shoesColor_Value = Menu?.GetLabel("ShoesColor_Value");

                    if (button.Name.Contains("Minus"))
                    {
                        if (Handler.CharGen_Stage == 0)
                        {
                            #region Stats

                            if (button.Name.Contains("TownSize") &&
                                townSize_Plus != null &&
                                townSize_Amount != null)
                            {
                                townSize_Plus.Enabled = true;
                                TownSize--;

                                townSize_Amount.Text = TownSize.ToString();

                                if (TownSize == 5)
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
                                    Button? stat_Plus = Menu?.GetButton(stat + "_Plus");
                                    if (stat_Plus != null)
                                    {
                                        stat_Plus.Enabled = true;
                                    }

                                    Stats[stat]--;

                                    Pool++;

                                    if (pool_Amount != null)
                                    {
                                        pool_Amount.Text = Pool.ToString();
                                    }

                                    Label? stat_Amount = Menu?.GetLabel(stat + "_Amount");
                                    if (stat_Amount != null)
                                    {
                                        stat_Amount.Text = Stats[stat].ToString();
                                    }

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

                                if (Gender_Value > 0 &&
                                    firstName_Minus != null &&
                                    firstName_Plus != null &&
                                    gender_plus != null)
                                {
                                    gender_plus.Enabled = true;

                                    Gender_Value--;
                                    if (Gender_Value == 0)
                                    {
                                        button.Enabled = false;
                                    }

                                    FirstName_Value = 0;

                                    firstName_Minus.Enabled = false;
                                    firstName_Plus.Enabled = true;

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

                                if (gender_Value != null)
                                {
                                    gender_Value.Text = Gender;
                                }

                                if (firstName_Value != null)
                                {
                                    firstName_Value.Text = First_Name;
                                }

                                #endregion
                            }
                            else if (button.Name.Contains("FirstName"))
                            {
                                #region First Name

                                if (FirstName_Value > 0 &&
                                    firstName_Plus != null)
                                {
                                    firstName_Plus.Enabled = true;

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

                                if (firstName_Value != null)
                                {
                                    firstName_Value.Text = First_Name;
                                }

                                #endregion
                            }
                            else if (button.Name.Contains("LastName"))
                            {
                                #region Last Name

                                if (LastName_Value > 0 &&
                                    lastName_Plus != null)
                                {
                                    lastName_Plus.Enabled = true;

                                    LastName_Value--;
                                    if (LastName_Value == 0)
                                    {
                                        button.Enabled = false;
                                    }

                                    Last_Name = CharacterManager.LastNames[LastName_Value];
                                }

                                if (lastName_Value != null)
                                {
                                    lastName_Value.Text = Last_Name;
                                }

                                #endregion
                            }
                            else if (button.Name.Contains("Skin"))
                            {
                                #region Skin Color

                                if (SkinColor_Value > 0 &&
                                    skin_Plus != null)
                                {
                                    skin_Plus.Enabled = true;

                                    SkinColor_Value--;
                                    if (SkinColor_Value == 0)
                                    {
                                        button.Enabled = false;
                                    }
                                }

                                if (skin_Value != null)
                                {
                                    skin_Value.Text = Handler.SkinColors[SkinColor_Value];
                                }

                                #endregion
                            }
                            else if (button.Name.Contains("HairLength"))
                            {
                                #region Hair Length

                                if (HairLength_Value > 0 &&
                                    hairLength_Plus != null)
                                {
                                    hairLength_Plus.Enabled = true;

                                    HairLength_Value--;
                                    if (HairLength_Value == 0 &&
                                        hairColor_Minus != null &&
                                        hairColor_Plus != null &&
                                        hairColor_Value != null)
                                    {
                                        button.Enabled = false;

                                        HairColor_Value = -1;
                                        hairColor_Minus.Enabled = false;
                                        hairColor_Value.Text = "N/A";
                                        hairColor_Plus.Enabled = false;
                                    }
                                }

                                if (hairLength_Value != null)
                                {
                                    hairLength_Value.Text = Handler.HairLength[HairLength_Value];
                                }

                                #endregion
                            }
                            else if (button.Name.Contains("HairColor"))
                            {
                                #region Hair Color

                                if (HairColor_Value > 0 &&
                                    hairColor_Plus != null)
                                {
                                    hairColor_Plus.Enabled = true;

                                    HairColor_Value--;
                                    if (HairColor_Value == 0)
                                    {
                                        button.Enabled = false;
                                    }
                                }

                                if (hairColor_Value != null)
                                {
                                    hairColor_Value.Text = Handler.HairColor[HairColor_Value];
                                }

                                #endregion
                            }
                            else if (button.Name.Contains("HatColor"))
                            {
                                #region Hat Color

                                if (HatColor_Value > 0 &&
                                    hatColor_Plus != null)
                                {
                                    hatColor_Plus.Enabled = true;

                                    HatColor_Value--;
                                    if (HatColor_Value == 0)
                                    {
                                        button.Enabled = false;
                                    }
                                }

                                if (hatColor_Value != null)
                                {
                                    hatColor_Value.Text = HatColors[HatColor_Value];
                                }

                                #endregion
                            }
                            else if (button.Name.Contains("ShirtColor"))
                            {
                                #region Shirt Color

                                if (ShirtColor_Value > 0 &&
                                    shirtColor_Plus != null)
                                {
                                    shirtColor_Plus.Enabled = true;

                                    ShirtColor_Value--;
                                    if (ShirtColor_Value == 0)
                                    {
                                        button.Enabled = false;
                                    }
                                }

                                if (shirtColor_Value != null)
                                {
                                    shirtColor_Value.Text = Handler.Colors[ShirtColor_Value];
                                }

                                #endregion
                            }
                            else if (button.Name.Contains("PantsColor"))
                            {
                                #region Pants Color

                                if (PantsColor_Value > 0 &&
                                    pantsColor_Plus != null)
                                {
                                    pantsColor_Plus.Enabled = true;

                                    PantsColor_Value--;
                                    if (PantsColor_Value == 0)
                                    {
                                        button.Enabled = false;
                                    }
                                }

                                if (pantsColor_Value != null)
                                {
                                    pantsColor_Value.Text = Handler.Colors[PantsColor_Value];
                                }

                                #endregion
                            }
                            else if (button.Name.Contains("ShoesColor"))
                            {
                                #region Shoes Color

                                if (ShoesColor_Value > 0 &&
                                    shoesColor_Plus != null)
                                {
                                    shoesColor_Plus.Enabled = true;

                                    ShoesColor_Value--;
                                    if (ShoesColor_Value == 0)
                                    {
                                        button.Enabled = false;
                                    }
                                }

                                if (shoesColor_Value != null)
                                {
                                    shoesColor_Value.Text = Handler.Colors[ShoesColor_Value];
                                }

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

                            if (button.Name.Contains("TownSize") &&
                                townSize_Minus != null &&
                                townSize_Amount != null)
                            {
                                townSize_Minus.Enabled = true;
                                TownSize++;

                                townSize_Amount.Text = TownSize.ToString();

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
                                    Button? stat_Minus = Menu?.GetButton(stat + "_Minus");
                                    if (stat_Minus != null)
                                    {
                                        stat_Minus.Enabled = true;
                                    }

                                    Stats[stat]++;

                                    Pool--;

                                    if (pool_Amount != null)
                                    {
                                        pool_Amount.Text = Pool.ToString();
                                    }

                                    Button? stat_Amount = Menu?.GetButton(stat + "_Amount");
                                    if (stat_Amount != null)
                                    {
                                        stat_Amount.Text = Stats[stat].ToString();
                                    }

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

                                if (Gender_Value < 1 &&
                                    gender_Minus != null &&
                                    firstName_Minus != null &&
                                    firstName_Plus != null)
                                {
                                    gender_Minus.Enabled = true;

                                    Gender_Value++;
                                    if (Gender_Value == 1)
                                    {
                                        button.Enabled = false;
                                    }

                                    FirstName_Value = 0;
                                    firstName_Minus.Enabled = false;
                                    firstName_Plus.Enabled = true;

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

                                if (gender_Value != null)
                                {
                                    gender_Value.Text = Gender;
                                }
                                
                                if (firstName_Value != null)
                                {
                                    firstName_Value.Text = First_Name;
                                }

                                #endregion
                            }
                            else if (button.Name.Contains("FirstName") &&
                                     firstName_Minus != null)
                            {
                                #region First Name

                                if (Gender == "Male")
                                {
                                    if (FirstName_Value < CharacterManager.FirstNames_Male.Count - 1)
                                    {
                                        firstName_Minus.Enabled = true;

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
                                        firstName_Minus.Enabled = true;

                                        FirstName_Value++;
                                        if (FirstName_Value == CharacterManager.FirstNames_Female.Count - 1)
                                        {
                                            button.Enabled = false;
                                        }

                                        First_Name = CharacterManager.FirstNames_Female[FirstName_Value];
                                    }
                                }

                                if (firstName_Value != null)
                                {
                                    firstName_Value.Text = First_Name;
                                }

                                #endregion
                            }
                            else if (button.Name.Contains("LastName") &&
                                     lastName_Minus != null)
                            {
                                #region Last Name

                                if (LastName_Value < CharacterManager.LastNames.Count - 1)
                                {
                                    lastName_Minus.Enabled = true;

                                    LastName_Value++;
                                    if (LastName_Value == CharacterManager.LastNames.Count - 1)
                                    {
                                        button.Enabled = false;
                                    }

                                    Last_Name = CharacterManager.LastNames[LastName_Value];
                                }

                                if (lastName_Value != null)
                                {
                                    lastName_Value.Text = Last_Name;
                                }

                                #endregion
                            }
                            else if (button.Name.Contains("Skin"))
                            {
                                #region Skin Color

                                if (SkinColor_Value < 2 &&
                                    skin_Minus != null)
                                {
                                    skin_Minus.Enabled = true;

                                    SkinColor_Value++;
                                    if (SkinColor_Value == 2)
                                    {
                                        button.Enabled = false;
                                    }
                                }

                                if (skin_Value != null)
                                {
                                    skin_Value.Text = Handler.SkinColors[SkinColor_Value];
                                }

                                #endregion
                            }
                            else if (button.Name.Contains("HairLength"))
                            {
                                #region Hair Length

                                if (HairLength_Value < Handler.HairLength.Length - 1 &&
                                    hairLength_Minus != null)
                                {
                                    hairLength_Minus.Enabled = true;

                                    if (HairLength_Value == 0 &&
                                        hairColor_Plus != null &&
                                        hairColor_Value != null)
                                    {
                                        HairColor_Value = 0;
                                        hairColor_Value.Text = Handler.HairColor[0];
                                        hairColor_Plus.Enabled = true;
                                    }

                                    HairLength_Value++;
                                    if (HairLength_Value == Handler.HairLength.Length - 1)
                                    {
                                        button.Enabled = false;
                                    }
                                }

                                if (hairLength_Value != null)
                                {
                                    hairLength_Value.Text = Handler.HairLength[HairLength_Value];
                                }

                                #endregion
                            }
                            else if (button.Name.Contains("HairColor"))
                            {
                                #region Hair Color

                                if (HairColor_Value < Handler.HairColor.Length - 1 &&
                                    hairColor_Minus != null)
                                {
                                    hairColor_Minus.Enabled = true;

                                    HairColor_Value++;
                                    if (HairColor_Value == Handler.HairColor.Length - 1)
                                    {
                                        button.Enabled = false;
                                    }
                                }

                                if (hairColor_Value != null)
                                {
                                    hairColor_Value.Text = Handler.HairColor[HairColor_Value];
                                }

                                #endregion
                            }
                            else if (button.Name.Contains("HatColor"))
                            {
                                #region Hat Color

                                if (HatColor_Value < HatColors.Length - 1 &&
                                    hatColor_Minus != null)
                                {
                                    hatColor_Minus.Enabled = true;

                                    HatColor_Value++;
                                    if (HatColor_Value == HatColors.Length - 1)
                                    {
                                        button.Enabled = false;
                                    }
                                }

                                if (hatColor_Value != null)
                                {
                                    hatColor_Value.Text = HatColors[HatColor_Value];
                                }

                                #endregion
                            }
                            else if (button.Name.Contains("ShirtColor"))
                            {
                                #region Shirt Color

                                if (ShirtColor_Value < Handler.Colors.Length - 1 &&
                                    shirtColor_Minus != null)
                                {
                                    shirtColor_Minus.Enabled = true;

                                    ShirtColor_Value++;
                                    if (ShirtColor_Value == Handler.Colors.Length - 1)
                                    {
                                        button.Enabled = false;
                                    }
                                }

                                if (shirtColor_Value != null)
                                {
                                    shirtColor_Value.Text = Handler.Colors[ShirtColor_Value];
                                }

                                #endregion
                            }
                            else if (button.Name.Contains("PantsColor"))
                            {
                                #region Pants Color

                                if (PantsColor_Value < Handler.Colors.Length - 1 &&
                                    pantsColor_Minus != null)
                                {
                                    pantsColor_Minus.Enabled = true;

                                    PantsColor_Value++;
                                    if (PantsColor_Value == Handler.Colors.Length - 1)
                                    {
                                        button.Enabled = false;
                                    }
                                }

                                if (pantsColor_Value != null)
                                {
                                    pantsColor_Value.Text = Handler.Colors[PantsColor_Value];
                                }

                                #endregion
                            }
                            else if (button.Name.Contains("ShoesColor"))
                            {
                                #region Shoes Color

                                if (ShoesColor_Value < Handler.Colors.Length - 1 &&
                                    shoesColor_Minus != null)
                                {
                                    shoesColor_Minus.Enabled = true;

                                    ShoesColor_Value++;
                                    if (ShoesColor_Value == Handler.Colors.Length - 1)
                                    {
                                        button.Enabled = false;
                                    }
                                }

                                if (shoesColor_Value != null)
                                {
                                    shoesColor_Value.Text = Handler.Colors[ShoesColor_Value];
                                }

                                #endregion
                            }

                            UpdateDisplay();

                            #endregion
                        }
                    }
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
            Picture? body = Menu?.GetPicture("Body");
            if (body != null)
            {
                body.Texture = Handler.GetTexture("Down_Naked_" + Handler.SkinColors[SkinColor_Value]);
                if (body.Texture != null)
                {
                    body.Image = new Rectangle(body.Texture.Width / 4, 0, body.Texture.Width / 4, body.Texture.Height);
                }
            }

            Picture? shirt = Menu?.GetPicture("Shirt");
            if (shirt != null)
            {
                shirt.Texture = Handler.GetTexture("Shirt_" + Handler.Colors[ShirtColor_Value] + "_Down");
                if (shirt.Texture != null)
                {
                    shirt.Image = new Rectangle(shirt.Texture.Width / 4, 0, shirt.Texture.Width / 4, shirt.Texture.Height);
                } 
            }

            Picture? pants = Menu?.GetPicture("Pants");
            if (pants != null)
            {
                pants.Texture = Handler.GetTexture("Pants_" + Handler.Colors[PantsColor_Value] + "_Down");
                if (pants.Texture != null)
                {
                    pants.Image = new Rectangle(pants.Texture.Width / 4, 0, pants.Texture.Width / 4, pants.Texture.Height);
                }
            }

            Picture? shoes = Menu?.GetPicture("Shoes");
            if (shoes != null)
            {
                shoes.Texture = Handler.GetTexture("Shoes_" + Handler.Colors[ShoesColor_Value] + "_Down");
                if (shoes.Texture != null)
                {
                    shoes.Image = new Rectangle(shoes.Texture.Width / 4, 0, shoes.Texture.Width / 4, shoes.Texture.Height);
                }
            }

            Picture? hair = Menu?.GetPicture("Hair");
            if (hair != null)
            {
                if (HairLength_Value == 0)
                {
                    hair.Texture = Handler.GetTexture("Clear");
                    if (hair.Texture != null)
                    {
                        hair.Image = new Rectangle(0, 0, hair.Texture.Width, hair.Texture.Height);
                    }
                }
                else
                {
                    hair.Texture = Handler.GetTexture("Hair_" + Handler.HairLength[HairLength_Value] + "_" + Handler.HairColor[HairColor_Value] + "_Down");
                    if (hair.Texture != null)
                    {
                        hair.Image = new Rectangle(hair.Texture.Width / 4, 0, hair.Texture.Width / 4, hair.Texture.Height);
                    }
                }
            }

            Picture? hat = Menu?.GetPicture("Hat");
            if (hat != null)
            {
                if (HatColor_Value == 0)
                {
                    hat.Texture = Handler.GetTexture("Clear");
                    if (hat.Texture != null)
                    {
                        hat.Image = new Rectangle(0, 0, hat.Texture.Width, hat.Texture.Height);
                    }
                }
                else
                {
                    hat.Texture = Handler.GetTexture("Hat_" + HatColors[HatColor_Value] + "_Down");
                    if (hat.Texture != null)
                    {
                        hat.Image = new Rectangle(hat.Texture.Width / 4, 0, hat.Texture.Width / 4, hat.Texture.Height);
                    }
                }
            }
        }

        private void Back()
        {
            Handler.Loading_Stage = 3;

            SceneManager.ChangeScene("Title");

            Menu? main = MenuManager.GetMenu("Main");
            if (main != null)
            {
                main.Active = true;
                main.Visible = true;
            }
        }

        private void Finish()
        {
            if (Main.Game == null)
            {
                return;
            }

            Squad? players = CharacterManager.GetArmy("Characters")?.GetSquad("Players");
            players?.Characters.Clear();

            float x = (Main.Game.ScreenWidth / 2) - (Main.Game.TileSize.X / 2);
            float y = (Main.Game.ScreenHeight / 2) - (Main.Game.TileSize.Y / 2) - (Main.Game.TileSize.Y * 2);

            Handler.Player = new Character
            {
                ID = Handler.GetID(),
                Name = First_Name + " " + Last_Name,
                Type = "Player",
                MoveSpeed = 1,
                Move_TotalDistance = Main.Game.TileSize.X,
                Direction = Direction.South,
                Region = new Region(x, y, Main.Game.TileSize_X, Main.Game.TileSize_Y),
                Visible = true,
                Frames = 4
            };
            players?.Characters.Add(Handler.Player);

            LoadInventory();
            CharacterUtil.LoadStats(Handler.Player, Stats);

            Reset();

            Handler.MapSize_X = TownSize;
            Handler.MapSize_Y = TownSize;

            SceneManager.ChangeScene("Loading");
        }

        private void LoadInventory()
        {
            if (Handler.Player == null)
            {
                return;
            }

            Handler.Player.Gender = Gender;

            Handler.Player.Texture = Handler.GetTexture("Naked_" + Handler.SkinColors[SkinColor_Value]);
            if (Handler.Player.Texture != null)
            {
                Handler.Player.Image = new Rectangle(0, 0, Handler.Player.Texture.Width / 4, Handler.Player.Texture.Height / 4);
            }
            
            Handler.Player.Inventory.ID = Handler.GetID();
            Handler.Player.Inventory.Name = Handler.Player.Name;

            if (HairLength_Value > 0)
            {
                string hairType = "Hair_" + Handler.HairLength[HairLength_Value] + "_" + Handler.HairColor[HairColor_Value];
                Texture2D? hairTexture = Handler.GetTexture(hairType);

                Item hair = new()
                {
                    Name = Handler.HairLength[HairLength_Value] + " " + Handler.HairColor[HairColor_Value] + " Hair",
                    Equipped = true,
                    Assignment = "Hair",
                    Type = hairType,
                    Texture = hairTexture,
                    Image = Handler.Player.Image,
                    Region = Handler.Player.Region,
                    DrawColor = Color.White,
                    Visible = true
                };
                Handler.Player.Inventory.Items.Add(hair);
            }

            Inventory? assets = InventoryManager.GetInventory("Assets");

            if (HatColor_Value > 0)
            {
                Item? hat = InventoryUtil.NewItem(assets?.GetItem(HatColors[HatColor_Value] + " Hat"));
                if (hat != null)
                {
                    hat.Equipped = true;
                    hat.Assignment = "Hat Slot";
                    hat.Image = Handler.Player.Image;
                    hat.Region = Handler.Player.Region;
                    Handler.Player.Inventory.Items.Add(hat);
                }
            }

            Item? shirt = InventoryUtil.NewItem(assets?.GetItem(Handler.Colors[ShirtColor_Value] + " Shirt"));
            if (shirt != null)
            {
                shirt.Equipped = true;
                shirt.Assignment = "Shirt Slot";
                shirt.Image = Handler.Player.Image;
                shirt.Region = Handler.Player.Region;
                Handler.Player.Inventory.Items.Add(shirt);
            }

            Item? pants = InventoryUtil.NewItem(assets?.GetItem(Handler.Colors[PantsColor_Value] + " Pants"));
            if (pants != null)
            {
                pants.Equipped = true;
                pants.Assignment = "Pants Slot";
                pants.Image = Handler.Player.Image;
                pants.Region = Handler.Player.Region;
                pants.Visible = false;
                InventoryManager.Inventories.Add(pants.Inventory);
                Handler.Player.Inventory.Items.Add(pants);
            }

            Item? shoes = InventoryUtil.NewItem(assets?.GetItem(Handler.Colors[ShoesColor_Value] + " Shoes"));
            if (shoes != null)
            {
                shoes.Equipped = true;
                shoes.Assignment = "Shoes Slot";
                shoes.Image = Handler.Player.Image;
                shoes.Region = Handler.Player.Region;
                shoes.Visible = false;
                Handler.Player.Inventory.Items.Add(shoes);
            }

            InventoryManager.Inventories.Add(Handler.Player.Inventory);
        }

        public override void Load()
        {
            ChangeMenu();
            UpdateDisplay();
        }

        public override void Load(ContentManager content)
        {
            Menu?.Clear();
            Stats.Clear();
            Stats_Buttons.Clear();
            Stats_Labels.Clear();
            Appearance_Buttons.Clear();

            Texture2D? button_Back = Handler.GetTexture("Button_Back");
            Texture2D? button_Back_Hover = Handler.GetTexture("Button_Back_Hover");
            Texture2D? button_Back_Disabled = Handler.GetTexture("Button_Back_Disabled");

            Texture2D? button_Next = Handler.GetTexture("Button_Next");
            Texture2D? button_Next_Hover = Handler.GetTexture("Button_Next_Hover");
            Texture2D? button_Next_Disabled = Handler.GetTexture("Button_Next_Disabled");

            Texture2D? button_Add = Handler.GetTexture("Button_Add");
            Texture2D? button_Add_Hover = Handler.GetTexture("Button_Add_Hover");
            Texture2D? button_Add_Disabled = Handler.GetTexture("Button_Add_Disabled");

            Texture2D? button_Remove = Handler.GetTexture("Button_Remove");
            Texture2D? button_Remove_Hover = Handler.GetTexture("Button_Remove_Hover");
            Texture2D? button_Remove_Disabled = Handler.GetTexture("Button_Remove_Disabled");

            Menu?.AddButton(Handler.GetID(), "Back", button_Back, button_Back_Hover, button_Back_Disabled,
                new Region(0, 0, 0, 0), Color.White, true);

            Button? back = Menu?.GetButton("Back");
            if (back != null)
            {
                back.HoverText = "Back";
            }

            Menu?.AddButton(Handler.GetID(), "Next", button_Next, button_Next_Hover, button_Next_Disabled,
                new Region(0, 0, 0, 0), Color.White, true);

            Button? next = Menu?.GetButton("Next");
            if (next != null)
            {
                next.HoverText = "Next";
            }

            #region Stats

            Menu?.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Pool", "Bonus Points:", Color.White, new Region(0, 0, 0, 0), true);

            Label? pool = Menu?.GetLabel("Pool");
            if (pool != null)
            {
                Stats_Labels.Add(pool);
            }

            Menu?.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Pool_Amount", Pool.ToString(), Color.White, new Region(0, 0, 0, 0), true);

            Label? pool_Amount = Menu?.GetLabel("Pool_Amount");
            if (pool_Amount != null)
            {
                Stats_Labels.Add(pool_Amount);
            }

            Menu?.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "BodyStats", "---Physical Stats---", Color.White, new Region(0, 0, 0, 0), true);

            Label? bodyStats = Menu?.GetLabel("BodyStats");
            if (bodyStats != null)
            {
                Stats_Labels.Add(bodyStats);
            }

            Menu?.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "MindStats", "---Mental Stats---", Color.White, new Region(0, 0, 0, 0), true);

            Label? mindStats = Menu?.GetLabel("MindStats");
            if (mindStats != null)
            {
                Stats_Labels.Add(mindStats);
            }

            foreach (var stat in Handler.Stats)
            {
                Menu?.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), stat.Key, stat.Key + ":", Color.White, new Region(0, 0, 0, 0), true);

                Label? statLabel = Menu?.GetLabel(stat.Key);
                if (statLabel != null)
                {
                    statLabel.HoverText = stat.Value;
                    Stats_Labels.Add(statLabel);
                }

                Menu?.AddButton(Handler.GetID(), stat.Key + "_Minus", button_Remove, button_Remove_Hover, button_Remove_Disabled,
                    new Region(0, 0, 0, 0), Color.White, true);

                Button? stat_Minus = Menu?.GetButton(stat.Key + "_Minus");
                if (stat_Minus != null)
                {
                    Stats_Buttons.Add(stat_Minus);
                }

                Menu?.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), stat.Key + "_Amount", "50", Color.White, new Region(0, 0, 0, 0), true);

                Label? stat_Amount = Menu?.GetLabel(stat.Key + "_Amount");
                if (stat_Amount != null)
                {
                    Stats_Labels.Add(stat_Amount);
                }

                Menu?.AddButton(Handler.GetID(), stat.Key + "_Plus", button_Add, button_Add_Hover, button_Add_Disabled,
                    new Region(0, 0, 0, 0), Color.White, true);

                Button? stat_Plus = Menu?.GetButton(stat.Key + "_Plus");
                if (stat_Plus != null)
                {
                    Stats_Buttons.Add(stat_Plus);
                }

                Stats.Add(stat.Key, 50);
            }

            Menu?.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "TownSize", "Town Size:", Color.White, new Region(0, 0, 0, 0), true);

            Label? townSize = Menu?.GetLabel("TownSize");
            if (townSize != null)
            {
                Stats_Labels.Add(townSize);
            }

            Menu?.AddButton(Handler.GetID(), "TownSize_Minus", button_Remove, button_Remove_Hover, button_Remove_Disabled,
                new Region(0, 0, 0, 0), Color.White, true);

            Button? townSize_Minus = Menu?.GetButton("TownSize_Minus");
            if (townSize_Minus != null)
            {
                Stats_Buttons.Add(townSize_Minus);
            }

            Menu?.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "TownSize_Amount", TownSize.ToString(), Color.White, new Region(0, 0, 0, 0), true);

            Label? townSize_Amount = Menu?.GetLabel("TownSize_Amount");
            if (townSize_Amount != null)
            {
                Stats_Labels.Add(townSize_Amount);
            }

            Menu?.AddButton(Handler.GetID(), "TownSize_Plus", button_Add, button_Add_Hover, button_Add_Disabled,
                new Region(0, 0, 0, 0), Color.White, true);

            Button? townsize_plus = Menu?.GetButton("TownSize_Plus");
            if (townsize_plus != null)
            {
                Stats_Buttons.Add(townsize_plus);
            }

            #endregion

            #region Appearance

            //Gender
            Menu?.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Gender", "Gender:", Color.White, new Region(0, 0, 0, 0), false);

            Label? gender = Menu?.GetLabel("Gender");
            if (gender != null)
            {
                Appearance_Labels.Add(gender);
            }

            Menu?.AddButton(Handler.GetID(), "Gender_Minus", button_Remove, button_Remove_Hover, button_Remove_Disabled,
                new Region(0, 0, 0, 0), Color.White, false);

            Button? gender_Minus = Menu?.GetButton("Gender_Minus");
            if (gender_Minus != null)
            {
                gender_Minus.Enabled = false;
                Appearance_Buttons.Add(gender_Minus);
            }

            Menu?.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Gender_Value", Gender, Color.White, new Region(0, 0, 0, 0), false);

            Label? gender_Value = Menu?.GetLabel("Gender_Value");
            if (gender_Value != null)
            {
                Appearance_Labels.Add(gender_Value);
            }

            Menu?.AddButton(Handler.GetID(), "Gender_Plus", button_Add, button_Add_Hover, button_Add_Disabled,
                new Region(0, 0, 0, 0), Color.White, false);

            Button? gender_Plus = Menu?.GetButton("Gender_Plus");
            if (gender_Plus != null)
            {
                Appearance_Buttons.Add(gender_Plus);
            }

            //First Name
            Menu?.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "FirstName", "First Name:", Color.White, new Region(0, 0, 0, 0), false);

            Label? firstName = Menu?.GetLabel("FirstName");
            if (firstName != null)
            {
                Appearance_Labels.Add(firstName);
            }

            Menu?.AddButton(Handler.GetID(), "FirstName_Minus", button_Remove, button_Remove_Hover, button_Remove_Disabled,
                new Region(0, 0, 0, 0), Color.White, false);

            Button? firstName_Minus = Menu?.GetButton("FirstName_Minus");
            if (firstName_Minus != null)
            {
                firstName_Minus.Enabled = false;
                Appearance_Buttons.Add(firstName_Minus);
            }

            Menu?.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "FirstName_Value", CharacterManager.FirstNames_Male[FirstName_Value], Color.White, new Region(0, 0, 0, 0), false);

            Label? firstName_Value = Menu?.GetLabel("FirstName_Value");
            if (firstName_Value != null)
            {
                Appearance_Labels.Add(firstName_Value);
            }
            
            First_Name = CharacterManager.FirstNames_Male[FirstName_Value];

            Menu?.AddButton(Handler.GetID(), "FirstName_Plus", button_Add, button_Add_Hover, button_Add_Disabled,
                new Region(0, 0, 0, 0), Color.White, false);

            Button? firstName_Plus = Menu?.GetButton("FirstName_Plus");
            if (firstName_Plus != null)
            {
                Appearance_Buttons.Add(firstName_Plus);
            }

            //Last Name
            Menu?.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "LastName", "Last Name:", Color.White, new Region(0, 0, 0, 0), false);

            Label? lastName = Menu?.GetLabel("LastName");
            if (lastName != null)
            {
                Appearance_Labels.Add(lastName);
            }

            Menu?.AddButton(Handler.GetID(), "LastName_Minus", button_Remove, button_Remove_Hover, button_Remove_Disabled,
                new Region(0, 0, 0, 0), Color.White, false);

            Button? lastName_Minus = Menu?.GetButton("LastName_Minus");
            if (lastName_Minus != null)
            {
                lastName_Minus.Enabled = false;
                Appearance_Buttons.Add(lastName_Minus);
            }

            Menu?.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "LastName_Value", CharacterManager.LastNames[LastName_Value], Color.White, new Region(0, 0, 0, 0), false);

            Label? lastName_Value = Menu?.GetLabel("LastName_Value");
            if (lastName_Value != null)
            {
                Appearance_Labels.Add(lastName_Value);
            }
            
            Last_Name = CharacterManager.LastNames[LastName_Value];

            Menu?.AddButton(Handler.GetID(), "LastName_Plus", button_Add, button_Add_Hover, button_Add_Disabled,
                new Region(0, 0, 0, 0), Color.White, false);

            Button? lastName_Plus = Menu?.GetButton("LastName_Plus");
            if (lastName_Plus != null)
            {
                Appearance_Buttons.Add(lastName_Plus);
            }

            //Skin Color
            Menu?.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Skin", "Skin Color:", Color.White, new Region(0, 0, 0, 0), false);

            Label? skin = Menu?.GetLabel("Skin");
            if (skin != null)
            {
                Appearance_Labels.Add(skin);
            }

            Menu?.AddButton(Handler.GetID(), "Skin_Minus", button_Remove, button_Remove_Hover, button_Remove_Disabled,
                new Region(0, 0, 0, 0), Color.White, false);

            Button? skin_Minus = Menu?.GetButton("Skin_Minus");
            if (skin_Minus != null)
            {
                skin_Minus.Enabled = false;
                Appearance_Buttons.Add(skin_Minus);
            }

            Menu?.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Skin_Value", Handler.SkinColors[SkinColor_Value], Color.White, new Region(0, 0, 0, 0), false);

            Label? skin_Value = Menu?.GetLabel("Skin_Value");
            if (skin_Value != null)
            {
                Appearance_Labels.Add(skin_Value);
            }

            Menu?.AddButton(Handler.GetID(), "Skin_Plus", button_Add, button_Add_Hover, button_Add_Disabled,
                new Region(0, 0, 0, 0), Color.White, false);

            Button? skin_Plus = Menu?.GetButton("Skin_Plus");
            if (skin_Plus != null)
            {
                Appearance_Buttons.Add(skin_Plus);
            }

            //Hair Length
            Menu?.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "HairLength", "Hair Style:", Color.White, new Region(0, 0, 0, 0), false);

            Label? hairLength = Menu?.GetLabel("HairLength");
            if (hairLength != null)
            {
                Appearance_Labels.Add(hairLength);
            }

            Menu?.AddButton(Handler.GetID(), "HairLength_Minus", button_Remove, button_Remove_Hover, button_Remove_Disabled,
                new Region(0, 0, 0, 0), Color.White, false);

            Button? hairLength_Minus = Menu?.GetButton("HairLength_Minus");
            if (hairLength_Minus != null)
            {
                hairLength_Minus.Enabled = false;
                Appearance_Buttons.Add(hairLength_Minus);
            }

            Menu?.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "HairLength_Value", Handler.HairLength[0], Color.White, new Region(0, 0, 0, 0), false);

            Label? hairLength_Value = Menu?.GetLabel("HairLength_Value");
            if (hairLength_Value != null)
            {
                Appearance_Labels.Add(hairLength_Value);
            }

            Menu?.AddButton(Handler.GetID(), "HairLength_Plus", button_Add, button_Add_Hover, button_Add_Disabled,
                new Region(0, 0, 0, 0), Color.White, false);

            Button? hairLength_Plus = Menu?.GetButton("HairLength_Plus");
            if (hairLength_Plus != null)
            {
                Appearance_Buttons.Add(hairLength_Plus);
            }

            //Hair Color
            Menu?.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "HairColor", "Hair Color:", Color.White, new Region(0, 0, 0, 0), false);

            Label? hairColor = Menu?.GetLabel("HairColor");
            if (hairColor != null)
            {
                Appearance_Labels.Add(hairColor);
            }

            Menu?.AddButton(Handler.GetID(), "HairColor_Minus", button_Remove, button_Remove_Hover, button_Remove_Disabled,
                new Region(0, 0, 0, 0), Color.White, false);

            Button? hairColor_Minus = Menu?.GetButton("HairColor_Minus");
            if (hairColor_Minus != null)
            {
                hairColor_Minus.Enabled = false;
                Appearance_Buttons.Add(hairColor_Minus);
            }

            Menu?.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "HairColor_Value", "N/A", Color.White, new Region(0, 0, 0, 0), false);

            Label? hairColor_Value = Menu?.GetLabel("HairColor_Value");
            if (hairColor_Value != null)
            {
                Appearance_Labels.Add(hairColor_Value);
            }

            Menu?.AddButton(Handler.GetID(), "HairColor_Plus", button_Add, button_Add_Hover, button_Add_Disabled,
                new Region(0, 0, 0, 0), Color.White, false);

            Button? hairColor_Plus = Menu?.GetButton("HairColor_Plus");
            if (hairColor_Plus != null)
            {
                hairColor_Plus.Enabled = false;
                Appearance_Buttons.Add(hairColor_Plus);
            }

            //Hat Color
            Menu?.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "HatColor", "Hat Color:", Color.White, new Region(0, 0, 0, 0), false);

            Label? hatColor = Menu?.GetLabel("HatColor");
            if (hatColor != null)
            {
                Appearance_Labels.Add(hatColor);
            }

            Menu?.AddButton(Handler.GetID(), "HatColor_Minus", button_Remove, button_Remove_Hover, button_Remove_Disabled,
                new Region(0, 0, 0, 0), Color.White, false);

            Button? hatColor_Minus = Menu?.GetButton("HatColor_Minus");
            if (hatColor_Minus != null)
            {
                hatColor_Minus.Enabled = false;
                Appearance_Buttons.Add(hatColor_Minus);
            }

            Menu?.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "HatColor_Value", HatColors[0], Color.White, new Region(0, 0, 0, 0), false);

            Label? hatColor_Value = Menu?.GetLabel("HatColor_Value");
            if (hatColor_Value != null)
            {
                Appearance_Labels.Add(hatColor_Value);
            }

            Menu?.AddButton(Handler.GetID(), "HatColor_Plus", button_Add, button_Add_Hover, button_Add_Disabled,
                new Region(0, 0, 0, 0), Color.White, false);

            Button? hatColor_Plus = Menu?.GetButton("HatColor_Plus");
            if (hatColor_Plus != null)
            {
                Appearance_Buttons.Add(hatColor_Plus);
            }

            //Shirt Color
            Menu?.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "ShirtColor", "Shirt Color:", Color.White, new Region(0, 0, 0, 0), false);

            Label? shirtColor = Menu?.GetLabel("ShirtColor");
            if (shirtColor != null)
            {
                Appearance_Labels.Add(shirtColor);
            }

            Menu?.AddButton(Handler.GetID(), "ShirtColor_Minus", button_Remove, button_Remove_Hover, button_Remove_Disabled,
                new Region(0, 0, 0, 0), Color.White, false);

            Button? shirtColor_Minus = Menu?.GetButton("ShirtColor_Minus");
            if (shirtColor_Minus != null)
            {
                shirtColor_Minus.Enabled = false;
                Appearance_Buttons.Add(shirtColor_Minus);
            }

            Menu?.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "ShirtColor_Value", Handler.Colors[0], Color.White, new Region(0, 0, 0, 0), false);

            Label? shirtColor_Value = Menu?.GetLabel("ShirtColor_Value");
            if (shirtColor_Value != null)
            {
                Appearance_Labels.Add(shirtColor_Value);
            }

            Menu?.AddButton(Handler.GetID(), "ShirtColor_Plus", button_Add, button_Add_Hover, button_Add_Disabled,
                new Region(0, 0, 0, 0), Color.White, false);

            Button? shirtColor_Plus = Menu?.GetButton("ShirtColor_Plus");
            if (shirtColor_Plus != null)
            {
                Appearance_Buttons.Add(shirtColor_Plus);
            }

            //Pants Color
            Menu?.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "PantsColor", "Pants Color:", Color.White, new Region(0, 0, 0, 0), false);

            Label? pantsColor = Menu?.GetLabel("PantsColor");
            if (pantsColor != null)
            {
                Appearance_Labels.Add(pantsColor);
            }

            Menu?.AddButton(Handler.GetID(), "PantsColor_Minus", button_Remove, button_Remove_Hover, button_Remove_Disabled,
                new Region(0, 0, 0, 0), Color.White, false);

            Button? pantsColor_Minus = Menu?.GetButton("PantsColor_Minus");
            if (pantsColor_Minus != null)
            {
                pantsColor_Minus.Enabled = false;
                Appearance_Buttons.Add(pantsColor_Minus);
            }

            Menu?.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "PantsColor_Value", Handler.Colors[0], Color.White, new Region(0, 0, 0, 0), false);

            Label? pantsColor_Value = Menu?.GetLabel("PantsColor_Value");
            if (pantsColor_Value != null)
            {
                Appearance_Labels.Add(pantsColor_Value);
            }

            Menu?.AddButton(Handler.GetID(), "PantsColor_Plus", button_Add, button_Add_Hover, button_Add_Disabled,
                new Region(0, 0, 0, 0), Color.White, false);

            Button? pantsColor_Plus = Menu?.GetButton("PantsColor_Plus");
            if (pantsColor_Plus != null)
            {
                Appearance_Buttons.Add(pantsColor_Plus);
            }

            //Shoes Color
            Menu?.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "ShoesColor", "Shoes Color:", Color.White, new Region(0, 0, 0, 0), false);

            Label? shoesColor = Menu?.GetLabel("ShoesColor");
            if (shoesColor != null)
            {
                Appearance_Labels.Add(shoesColor);
            }

            Menu?.AddButton(Handler.GetID(), "ShoesColor_Minus", button_Remove, button_Remove_Hover, button_Remove_Disabled,
                new Region(0, 0, 0, 0), Color.White, false);

            Button? shoesColor_Minus = Menu?.GetButton("ShoesColor_Minus");
            if (shoesColor_Minus != null)
            {
                shoesColor_Minus.Enabled = false;
                Appearance_Buttons.Add(shoesColor_Minus);
            }

            Menu?.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "ShoesColor_Value", Handler.Colors[0], Color.White, new Region(0, 0, 0, 0), false);

            Label? shoesColor_Value = Menu?.GetLabel("ShoesColor_Value");
            if (shoesColor_Value != null)
            {
                Appearance_Labels.Add(shoesColor_Value);
            }

            Menu?.AddButton(Handler.GetID(), "ShoesColor_Plus", button_Add, button_Add_Hover, button_Add_Disabled,
                new Region(0, 0, 0, 0), Color.White, false);

            Button? shoesColor_Plus = Menu?.GetButton("ShoesColor_Plus");
            if (shoesColor_Plus != null)
            {
                Appearance_Buttons.Add(shoesColor_Plus);
            }

            #endregion

            #region Display

            Menu?.AddPicture(Handler.GetID(), "Body", null, new Region(0, 0, 0, 0), Color.White, false);

            Picture? body = Menu?.GetPicture("Body");
            if (body != null)
            {
                Appearance_Pictures.Add(body);
            }

            Menu?.AddPicture(Handler.GetID(), "Shirt", null, new Region(0, 0, 0, 0), Color.White, false);

            Picture? shirt = Menu?.GetPicture("Shirt");
            if (shirt != null)
            {
                Appearance_Pictures.Add(shirt);
            }

            Menu?.AddPicture(Handler.GetID(), "Pants", null, new Region(0, 0, 0, 0), Color.White, false);

            Picture? pants = Menu?.GetPicture("Pants");
            if (pants != null)
            {
                Appearance_Pictures.Add(pants);
            }

            Menu?.AddPicture(Handler.GetID(), "Shoes", null, new Region(0, 0, 0, 0), Color.White, false);

            Picture? shoes = Menu?.GetPicture("Shoes");
            if (shoes != null)
            {
                Appearance_Pictures.Add(shoes);
            }

            Menu?.AddPicture(Handler.GetID(), "Hair", null, new Region(0, 0, 0, 0), Color.White, false);

            Picture? hair = Menu?.GetPicture("Hair");
            if (hair != null)
            {
                Appearance_Pictures.Add(hair);
            }

            Menu?.AddPicture(Handler.GetID(), "Hat", null, new Region(0, 0, 0, 0), Color.White, false);

            Picture? hat = Menu?.GetPicture("Hat");
            if (hat != null)
            {
                Appearance_Pictures.Add(hat);
            }

            #endregion

            Menu?.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, Handler.GetTexture("Frame"), new Region(0, 0, 0, 0), false);
            Menu?.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "ExamineStat", "", Color.White, Handler.GetTexture("Frame_Wide"), new Region(0, 0, 0, 0), false);

            if (Menu != null)
            {
                Menu.Visible = true;
            }

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

            int Width = (int)Main.Game.MenuSize_X;
            int Height = (int)Main.Game.MenuSize_Y;

            int x = Main.Game.ScreenWidth / 2;
            int y = (Main.Game.ScreenHeight / 40) * 29;

            Button? back = Menu?.GetButton("Back");
            if (back != null)
            {
                back.Region = new Region(x - (Width * 2), y + (Height * 3), Width, Height);
            }

            Button? next = Menu?.GetButton("Next");
            if (next != null)
            {
                next.Region = new Region(x + Width, y + (Height * 3), Width, Height);
            }

            #region Stats

            x = Main.Game.ScreenWidth / 2;
            y = (Main.Game.ScreenHeight / 40);

            Label? pool = Menu?.GetLabel("Pool");
            if (pool != null)
            {
                pool.Region = new Region(x - (Width * 4), y, Width * 6, Height);
            }

            Label? pool_Amount = Menu?.GetLabel("Pool_Amount");
            if (pool_Amount != null)
            {
                pool_Amount.Region = new Region(x + (Width * 2), y, Width, Height);
            }
            
            y += (Main.Game.ScreenHeight / 40) * 6;

            Label? bodyStats = Menu?.GetLabel("BodyStats");
            if (bodyStats != null)
            {
                bodyStats.Region = new Region(x - (Width * 9), y, Width * 7, Height);
            }

            Label? mindStats = Menu?.GetLabel("MindStats");
            if (mindStats != null)
            {
                mindStats.Region = new Region(x + (Width * 2), y, Width * 7, Height);
            }
            
            y += (Main.Game.ScreenHeight / 40) * 3;

            //Strength
            Label? strength = Menu?.GetLabel("Strength");
            if (strength != null)
            {
                strength.Region = new Region(x - (Width * 9), y, Width * 4, Height);
            }

            Button? strength_Minus = Menu?.GetButton("Strength_Minus");
            if (strength_Minus != null)
            {
                strength_Minus.Region = new Region(x - (Width * 5), y, Width, Height);
            }
            
            Label? strength_Amount = Menu?.GetLabel("Strength_Amount");
            if (strength_Amount != null)
            {
                strength_Amount.Region = new Region(x - (Width * 4), y, Width, Height);
            }
            
            Button? strength_Plus = Menu?.GetButton("Strength_Plus");
            if (strength_Plus != null)
            {
                strength_Plus.Region = new Region(x - (Width * 3), y, Width, Height);
            }

            //Intelligence
            Label? intelligence = Menu?.GetLabel("Intelligence");
            if (intelligence != null)
            {
                intelligence.Region = new Region(x + (Width * 2), y, Width * 4, Height);
            }

            Button? intelligence_Minus = Menu?.GetButton("Intelligence_Minus");
            if (intelligence_Minus != null)
            {
                intelligence_Minus.Region = new Region(x + (Width * 6), y, Width, Height);
            }
            
            Label? intelligence_Amount = Menu?.GetLabel("Intelligence_Amount");
            if (intelligence_Amount != null)
            {
                intelligence_Amount.Region = new Region(x + (Width * 7), y, Width, Height);
            }
            
            Button? intelligence_Plus = Menu?.GetButton("Intelligence_Plus");
            if (intelligence_Plus != null)
            {
                intelligence_Plus.Region = new Region(x + (Width * 8), y, Width, Height);
            }
            
            y += (Main.Game.ScreenHeight / 40) * 3;

            //Vitality
            Label? vitality = Menu?.GetLabel("Vitality");
            if (vitality != null)
            {
                vitality.Region = new Region(x - (Width * 9), y, Width * 4, Height);
            }
            
            Button? vitality_Minus = Menu?.GetButton("Vitality_Minus");
            if (vitality_Minus != null)
            {
                vitality_Minus.Region = new Region(x - (Width * 5), y, Width, Height);
            }
            
            Label? vitality_Amount = Menu?.GetLabel("Vitality_Amount");
            if (vitality_Amount != null)
            {
                vitality_Amount.Region = new Region(x - (Width * 4), y, Width, Height);
            }
            
            Button? vitality_Plus = Menu?.GetButton("Vitality_Plus");
            if (vitality_Plus != null)
            {
                vitality_Plus.Region = new Region(x - (Width * 3), y, Width, Height);
            }

            //Perception
            Label? perception = Menu?.GetLabel("Perception");
            if (perception != null)
            {
                perception.Region = new Region(x + (Width * 2), y, Width * 4, Height);
            }
            
            Button? perception_Minus = Menu?.GetButton("Perception_Minus");
            if (perception_Minus != null)
            {
                perception_Minus.Region = new Region(x + (Width * 6), y, Width, Height);
            }
            
            Label? perception_Amount = Menu?.GetLabel("Perception_Amount");
            if (perception_Amount != null)
            {
                perception_Amount.Region = new Region(x + (Width * 7), y, Width, Height);
            }
            
            Button? perception_Plus = Menu?.GetButton("Perception_Plus");
            if (perception_Plus != null)
            {
                perception_Plus.Region = new Region(x + (Width * 8), y, Width, Height);
            }
            
            y += (Main.Game.ScreenHeight / 40) * 3;

            //Endurance
            Label? endurance = Menu?.GetLabel("Endurance");
            if (endurance != null)
            {
                endurance.Region = new Region(x - (Width * 9), y, Width * 4, Height);
            }
            
            Button? endurance_Minus = Menu?.GetButton("Endurance_Minus");
            if (endurance_Minus != null)
            {
                endurance_Minus.Region = new Region(x - (Width * 5), y, Width, Height);
            }
            
            Label? endurance_Amount = Menu?.GetLabel("Endurance_Amount");
            if (endurance_Amount != null)
            {
                endurance_Amount.Region = new Region(x - (Width * 4), y, Width, Height);
            }
            
            Button? endurance_Plus = Menu?.GetButton("Endurance_Plus");
            if (endurance_Plus != null)
            {
                endurance_Plus.Region = new Region(x - (Width * 3), y, Width, Height);
            }

            //Charisma
            Label? charisma = Menu?.GetLabel("Charisma");
            if (charisma != null)
            {
                charisma.Region = new Region(x + (Width * 2), y, Width * 4, Height);
            }
            
            Button? charisma_Minus = Menu?.GetButton("Charisma_Minus");
            if (charisma_Minus != null)
            {
                charisma_Minus.Region = new Region(x + (Width * 6), y, Width, Height);
            }
            
            Label? charisma_Amount = Menu?.GetLabel("Charisma_Amount");
            if (charisma_Amount != null)
            {
                charisma_Amount.Region = new Region(x + (Width * 7), y, Width, Height);
            }
            
            Button? charisma_Plus = Menu?.GetButton("Charisma_Plus");
            if (charisma_Plus != null)
            {
                charisma_Plus.Region = new Region(x + (Width * 8), y, Width, Height);
            }
            
            y += (Main.Game.ScreenHeight / 40) * 3;

            //Agility
            Label? agility = Menu?.GetLabel("Agility");
            if (agility != null)
            {
                agility.Region = new Region(x - (Width * 9), y, Width * 4, Height);
            }

            Button? agility_Minus = Menu?.GetButton("Agility_Minus");
            if (agility_Minus != null)
            {
                agility_Minus.Region = new Region(x - (Width * 5), y, Width, Height);
            }
            
            Label? agility_Amount = Menu?.GetLabel("Agility_Amount");
            if (agility_Amount != null)
            {
                agility_Amount.Region = new Region(x - (Width * 4), y, Width, Height);
            }
            
            Button? agility_Plus = Menu?.GetButton("Agility_Plus");
            if (agility_Plus != null)
            {
                agility_Plus.Region = new Region(x - (Width * 3), y, Width, Height);
            }

            //Willpower
            Label? willpower = Menu?.GetLabel("Willpower");
            if (willpower != null)
            {
                willpower.Region = new Region(x + (Width * 2), y, Width * 4, Height);
            }
            
            Button? willpower_Minus = Menu?.GetButton("Willpower_Minus");
            if (willpower_Minus != null)
            {
                willpower_Minus.Region = new Region(x + (Width * 6), y, Width, Height);
            }
            
            Label? willpower_Amount = Menu?.GetLabel("Willpower_Amount");
            if (willpower_Amount != null)
            {
                willpower_Amount.Region = new Region(x + (Width * 7), y, Width, Height);
            }
            
            Button? willpower_Plus = Menu?.GetButton("Willpower_Plus");
            if (willpower_Plus != null)
            {
                willpower_Plus.Region = new Region(x + (Width * 8), y, Width, Height);
            }
            
            y += (Main.Game.ScreenHeight / 40) * 3;

            //Luck
            Label? luck = Menu?.GetLabel("Luck");
            if (luck != null)
            {
                luck.Region = new Region(x - (Width * 9), y, Width * 4, Height);
            }
            
            Button? luck_Minus = Menu?.GetButton("Luck_Minus");
            if (luck_Minus != null)
            {
                luck_Minus.Region = new Region(x - (Width * 5), y, Width, Height);
            }
            
            Label? luck_Amount = Menu?.GetLabel("Luck_Amount");
            if (luck_Amount != null)
            {
                luck_Amount.Region = new Region(x - (Width * 4), y, Width, Height);
            }
            
            Button? luck_Plus = Menu?.GetButton("Luck_Plus");
            if (luck_Plus != null)
            {
                luck_Plus.Region = new Region(x - (Width * 3), y, Width, Height);
            }

            //Sanity
            Label? sanity = Menu?.GetLabel("Sanity");
            if (sanity != null)
            {
                sanity.Region = new Region(x + (Width * 2), y, Width * 4, Height);
            }
            
            Button? sanity_Minus = Menu?.GetButton("Sanity_Minus");
            if (sanity_Minus != null)
            {
                sanity_Minus.Region = new Region(x + (Width * 6), y, Width, Height);
            }
            
            Label? sanity_Amount = Menu?.GetLabel("Sanity_Amount");
            if (sanity_Amount != null)
            {
                sanity_Amount.Region = new Region(x + (Width * 7), y, Width, Height);
            }
            
            Button? sanity_Plus = Menu?.GetButton("Sanity_Plus");
            if (sanity_Plus != null)
            {
                sanity_Plus.Region = new Region(x + (Width * 8), y, Width, Height);
            }
            
            y += (Main.Game.ScreenHeight / 40) * 6;

            //TownSize
            Label? townSize = Menu?.GetLabel("TownSize");
            if (townSize != null)
            {
                townSize.Region = new Region(x - (Width * 4), y, Width * 4, Height);
            }
            
            Button? townSize_Minus = Menu?.GetButton("TownSize_Minus");
            if (townSize_Minus != null)
            {
                townSize_Minus.Region = new Region(x + (Width * 0), y, Width, Height);
            }
            
            Label? townSize_Amount = Menu?.GetLabel("TownSize_Amount");
            if (townSize_Amount != null)
            {
                townSize_Amount.Region = new Region(x + (Width * 1), y, Width, Height);
            }
            
            Button? townSize_Plus = Menu?.GetButton("TownSize_Plus");
            if (townSize_Plus != null)
            {
                townSize_Plus.Region = new Region(x + (Width * 2), y, Width, Height);
            }

            #endregion

            #region Appearance

            x = Main.Game.ScreenWidth / 2;
            y = (Main.Game.ScreenHeight / 40) * 9;

            //Gender
            Label? gender = Menu?.GetLabel("Gender");
            if (gender != null)
            {
                gender.Region = new Region(x - (Width * 10), y, Width * 4, Height);
            }
            
            Button? gender_Minus = Menu?.GetButton("Gender_Minus");
            if (gender_Minus != null)
            {
                gender_Minus.Region = new Region(x - (Width * 6), y, Width, Height);
            }
            
            Label? gender_Value = Menu?.GetLabel("Gender_Value");
            if (gender_Value != null)
            {
                gender_Value.Region = new Region(x - (Width * 5), y, (Width * 2), Height);
            }
            
            Button? gender_Plus = Menu?.GetButton("Gender_Plus");
            if (gender_Plus != null)
            {
                gender_Plus.Region = new Region(x - (Width * 3), y, Width, Height);
            }

            //Hair Color
            Label? hairColor = Menu?.GetLabel("HairColor");
            if (hairColor != null)
            {
                hairColor.Region = new Region(x + (Width * 2), y, Width * 4, Height);
            }

            Button? hairColor_Minus = Menu?.GetButton("HairColor_Minus");
            if (hairColor_Minus != null)
            {
                hairColor_Minus.Region = new Region(x + (Width * 6), y, Width, Height);
            }
            
            Label? hairColor_Value = Menu?.GetLabel("HairColor_Value");
            if (hairColor_Value != null)
            {
                hairColor_Value.Region = new Region(x + (Width * 7), y, (Width * 2), Height);
            }
            
            Button? hairColor_Plus = Menu?.GetButton("HairColor_Plus");
            if (hairColor_Plus != null)
            {
                hairColor_Plus.Region = new Region(x + (Width * 9), y, Width, Height);
            }
            
            y += (Main.Game.ScreenHeight / 40) * 3;

            //First Name
            Label? firstName = Menu?.GetLabel("FirstName");
            if (firstName != null)
            {
                firstName.Region = new Region(x - (Width * 10), y, Width * 4, Height);
            }
            
            Button? firstName_Minus = Menu?.GetButton("FirstName_Minus");
            if (firstName_Minus != null)
            {
                firstName_Minus.Region = new Region(x - (Width * 6), y, Width, Height);
            }
            
            Label? firstName_Value = Menu?.GetLabel("FirstName_Value");
            if (firstName_Value != null)
            {
                firstName_Value.Region = new Region(x - (Width * 5), y, (Width * 2), Height);
            }
            
            Button? firstName_Plus = Menu?.GetButton("FirstName_Plus");
            if (firstName_Plus != null)
            {
                firstName_Plus.Region = new Region(x - (Width * 3), y, Width, Height);
            }

            //Hat Color
            Label? hatColor = Menu?.GetLabel("HatColor");
            if (hatColor != null)
            {
                hatColor.Region = new Region(x + (Width * 2), y, Width * 4, Height);
            }
            
            Button? hatColor_Minus = Menu?.GetButton("HatColor_Minus");
            if (hatColor_Minus != null)
            {
                hatColor_Minus.Region = new Region(x + (Width * 6), y, Width, Height);
            }
            
            Label? hatColor_Value = Menu?.GetLabel("HatColor_Value");
            if (hatColor_Value != null)
            {
                hatColor_Value.Region = new Region(x + (Width * 7), y, (Width * 2), Height);
            }
            
            Button? hatColor_Plus = Menu?.GetButton("HatColor_Plus");
            if (hatColor_Plus != null)
            {
                hatColor_Plus.Region = new Region(x + (Width * 9), y, Width, Height);
            }
            
            y += (Main.Game.ScreenHeight / 40) * 3;

            //Last Name
            Label? lastName = Menu?.GetLabel("LastName");
            if (lastName != null)
            {
                lastName.Region = new Region(x - (Width * 10), y, Width * 4, Height);
            }
            
            Button? lastName_Minus = Menu?.GetButton("LastName_Minus");
            if (lastName_Minus != null)
            {
                lastName_Minus.Region = new Region(x - (Width * 6), y, Width, Height);
            }
            
            Label? lastName_Value = Menu?.GetLabel("LastName_Value");
            if (lastName_Value != null)
            {
                lastName_Value.Region = new Region(x - (Width * 5), y, (Width * 2), Height);
            }
            
            Button? lastName_Plus = Menu?.GetButton("LastName_Plus");
            if (lastName_Plus != null)
            {
                lastName_Plus.Region = new Region(x - (Width * 3), y, Width, Height);
            }

            //Shirt Color
            Label? shirtColor = Menu?.GetLabel("ShirtColor");
            if (shirtColor != null)
            {
                shirtColor.Region = new Region(x + (Width * 2), y, Width * 4, Height);
            }
            
            Button? shirtColor_Minus = Menu?.GetButton("ShirtColor_Minus");
            if (shirtColor_Minus != null)
            {
                shirtColor_Minus.Region = new Region(x + (Width * 6), y, Width, Height);
            }
            
            Label? shirtColor_Value = Menu?.GetLabel("ShirtColor_Value");
            if (shirtColor_Value != null)
            {
                shirtColor_Value.Region = new Region(x + (Width * 7), y, (Width * 2), Height);
            }
            
            Button? shirtColor_Plus = Menu?.GetButton("ShirtColor_Plus");
            if (shirtColor_Plus != null)
            {
                shirtColor_Plus.Region = new Region(x + (Width * 9), y, Width, Height);
            }
            
            y += (Main.Game.ScreenHeight / 40) * 3;

            //Skin Color
            Label? skin = Menu?.GetLabel("Skin");
            if (skin != null)
            {
                skin.Region = new Region(x - (Width * 10), y, Width * 4, Height);
            }
            
            Button? skin_Minus = Menu?.GetButton("Skin_Minus");
            if (skin_Minus != null)
            {
                skin_Minus.Region = new Region(x - (Width * 6), y, Width, Height);
            }
            
            Label? skin_Value = Menu?.GetLabel("Skin_Value");
            if (skin_Value != null)
            {
                skin_Value.Region = new Region(x - (Width * 5), y, (Width * 2), Height);
            }
            
            Button? skin_Plus = Menu?.GetButton("Skin_Plus");
            if (skin_Plus != null)
            {
                skin_Plus.Region = new Region(x - (Width * 3), y, Width, Height);
            }

            //Pants Color
            Label? pantsColor = Menu?.GetLabel("PantsColor");
            if (pantsColor != null)
            {
                pantsColor.Region = new Region(x + (Width * 2), y, Width * 4, Height);
            }
            
            Button? pantsColor_Minus = Menu?.GetButton("PantsColor_Minus");
            if (pantsColor_Minus != null)
            {
                pantsColor_Minus.Region = new Region(x + (Width * 6), y, Width, Height);
            }
            
            Label? pantsColor_Value = Menu?.GetLabel("PantsColor_Value");
            if (pantsColor_Value != null)
            {
                pantsColor_Value.Region = new Region(x + (Width * 7), y, (Width * 2), Height);
            }
            
            Button? pantsColor_Plus = Menu?.GetButton("PantsColor_Plus");
            if (pantsColor_Plus != null)
            {
                pantsColor_Plus.Region = new Region(x + (Width * 9), y, Width, Height);
            }
            
            y += (Main.Game.ScreenHeight / 40) * 3;

            //Hair Length
            Label? hairLength = Menu?.GetLabel("HairLength");
            if (hairLength != null)
            {
                hairLength.Region = new Region(x - (Width * 10), y, Width * 4, Height);
            }
            
            Button? hairLength_Minus = Menu?.GetButton("HairLength_Minus");
            if (hairLength_Minus != null)
            {
                hairLength_Minus.Region = new Region(x - (Width * 6), y, Width, Height);
            }
            
            Label? hairLength_Value = Menu?.GetLabel("HairLength_Value");
            if (hairLength_Value != null)
            {
                hairLength_Value.Region = new Region(x - (Width * 5), y, (Width * 2), Height);
            }
            
            Button? hairLength_Plus = Menu?.GetButton("HairLength_Plus");
            if (hairLength_Plus != null)
            {
                hairLength_Plus.Region = new Region(x - (Width * 3), y, Width, Height);
            }

            //Shoes Color
            Label? shoesColor = Menu?.GetLabel("ShoesColor");
            if (shoesColor != null)
            {
                shoesColor.Region = new Region(x + (Width * 2), y, Width * 4, Height);
            }
            
            Button? shoesColor_Minus = Menu?.GetButton("ShoesColor_Minus");
            if (shoesColor_Minus != null)
            {
                shoesColor_Minus.Region = new Region(x + (Width * 6), y, Width, Height);
            }
            
            Label? shoesColor_Value = Menu?.GetLabel("ShoesColor_Value");
            if (shoesColor_Value != null)
            {
                shoesColor_Value.Region = new Region(x + (Width * 7), y, (Width * 2), Height);
            }
            
            Button? shoesColor_Plus = Menu?.GetButton("ShoesColor_Plus");
            if (shoesColor_Plus != null)
            {
                shoesColor_Plus.Region = new Region(x + (Width * 9), y, Width, Height);
            }
            
            y += (Main.Game.ScreenHeight / 40) * 3;

            #endregion

            #region Display

            x = Main.Game.ScreenWidth / 2;
            y = (Main.Game.ScreenHeight / 40) * 31;

            Picture? body = Menu?.GetPicture("Body");
            if (body != null)
            {
                body.Region = new Region(x - Width, y - Height, (Width * 2), (Height * 2));
            }

            Picture? shirt = Menu?.GetPicture("Shirt");
            if (shirt != null)
            {
                shirt.Region = new Region(x - Width, y - Height, (Width * 2), (Height * 2));
            }

            Picture? pants = Menu?.GetPicture("Pants");
            if (pants != null)
            {
                pants.Region = new Region(x - Width, y - Height, (Width * 2), (Height * 2));
            }

            Picture? shoes = Menu?.GetPicture("Shoes");
            if (shoes != null)
            {
                shoes.Region = new Region(x - Width, y - Height, (Width * 2), (Height * 2));
            }

            Picture? hair = Menu?.GetPicture("Hair");
            if (hair != null)
            {
                hair.Region = new Region(x - Width, y - Height, (Width * 2), (Height * 2));
            }

            Picture? hat = Menu?.GetPicture("Hat");
            if (hat != null)
            {
                hat.Region = new Region(x - Width, y - Height, (Width * 2), (Height * 2));
            }

            #endregion
        }

        private void Examine(string text)
        {
            if (Main.Game == null)
            {
                return;
            }
            if (InputManager.Mouse == null)
            {
                return;
            }

            Label? examine = Menu?.GetLabel("ExamineStat");
            if (examine == null)
            {
                return;
            }

            examine.Text = text;

            int width = (int)(Main.Game.MenuSize_X * 14);
            int height = (int)Main.Game.MenuSize_Y;

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

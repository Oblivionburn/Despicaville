using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Menus;
using OP_Engine.Utility;
using OP_Engine.Time;
using OP_Engine.Enums;
using Despicaville.Util;

namespace Despicaville.Menus
{
    public class Menu_UI : Menu
    {
        #region Variables



        #endregion

        #region Constructor

        public Menu_UI(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "UI";
            Load(content);
        }

        #endregion

        #region Methods

        public override void Update(Game? gameRef, ContentManager? content)
        {
            if (Visible ||
                Active)
            {
                UpdateTime();
                UpdateStats();
                UpdateControlsDisplay();

                if (!TimeManager.Paused)
                {
                    UpdateControls();
                }

                base.Update(gameRef, content);
            }
        }

        private void UpdateControls()
        {
            bool hoveringButton = HoveringButton();
            Label? hoveringLabel = HoveringLabel();

            if (!hoveringButton)
            {
                if (hoveringLabel == null ||
                    hoveringLabel?.Name == "Crouching" ||
                    hoveringLabel?.Name == "Running" ||
                    hoveringLabel?.Name == "Pulling" ||
                    hoveringLabel?.Name == "Combat")
                {
                    Label? examine = GetLabel("Examine");
                    if (examine != null)
                    {
                        examine.Visible = false;
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
                    if (button.Region != null &&
                        InputManager.MouseWithin(button.Region.ToRectangle))
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

        private Label? HoveringLabel()
        {
            foreach (Label label in Labels)
            {
                if (label.Visible)
                {
                    if (label.Region != null &&
                        InputManager.MouseWithin(label.Region.ToRectangle))
                    {
                        if (label.HoverText != null)
                        {
                            Examine(label.HoverText);
                        }

                        return label;
                    }
                }
            }

            return null;
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");
            InputManager.Mouse?.Flush();

            Label? examine = GetLabel("Examine");
            if (examine != null)
            {
                examine.Visible = false;
            }

            if (button.Name == "Main")
            {
                MenuManager.GetMenu("Main")?.Open();
            }
            else if (button.Name == "Inventory")
            {
                Handler.Trading = false;
                Handler.Trading_InventoryID.Clear();

                MenuManager.GetMenu("Inventory")?.Open();
            }
            else if (button.Name == "Health")
            {
                if (!Handler.Menu_Health)
                {
                    Handler.Menu_Health = true;

                    Menu? main = MenuManager.GetMenu(button.Name);
                    if (main != null)
                    {
                        main.Load();
                        main.Active = true;
                        main.Visible = true;
                    }
                }
                else
                {
                    Handler.Menu_Health = false;
                    Handler.Selected_BodyPart = "";

                    Menu? main = MenuManager.GetMenu(button.Name);
                    if (main != null)
                    {
                        main.Active = false;
                        main.Visible = false;
                    }
                }
            }
        }

        private void UpdateTime()
        {
            Label? time = GetLabel("Time");
            if (time != null)
            {
                if (TimeManager.Now != null)
                {
                    string[] time_array = TimeManager.Now.ToString(false, true, true).Split(' ');
                    time.Text = time_array[0] + "\n" + time_array[1] + " " + time_array[2];
                }
            }
        }

        private void UpdateStats()
        {
            if (Handler.Player == null)
            {
                return;
            }

            ProgressBar? bar = GetProgressBar("Hunger");
            if (bar != null)
            {
                bar.Value = Handler.Player.Stats.Hunger;
                bar.Update();
            }
            Label? label = GetLabel("Hunger");
            if (label != null)
            {
                label.HoverText = "How hungry you are.\nStamina -1 every hour at 100%.";
                label.Text = "Hunger: " + Handler.Player.Stats.Hunger.ToString("0.##") + "/100%";
            }

            bar = GetProgressBar("Thirst");
            if (bar != null)
            {
                bar.Value = Handler.Player.Stats.Thirst;
                bar.Update();
            }
            label = GetLabel("Thirst");
            if (label != null)
            {
                label.HoverText = "How thirsty you are.\nBlood -1 every hour at 100%.";
                label.Text = "Thirst: " + Handler.Player.Stats.Thirst.ToString("0.##") + "/100%";
            }

            bar = GetProgressBar("Bladder");
            if (bar != null)
            {
                bar.Value = Handler.Player.Stats.Bladder;
                bar.Update();
            }
            label = GetLabel("Bladder");
            if (label != null)
            {
                label.HoverText = "How urgently you need a toilet.\nYou defecate yourself at 100%.";
                label.Text = "Bladder: " + Handler.Player.Stats.Bladder.ToString("0.##") + "/100%";
            }

            bar = GetProgressBar("Grime");
            if (bar != null)
            {
                bar.Value = Handler.Player.Stats.Grime;
                bar.Update();
            }
            label = GetLabel("Grime");
            if (label != null)
            {
                label.HoverText = "How dirty you are.\nImpacts using your Charisma.";
                label.Text = "Grime: " + Handler.Player.Stats.Grime.ToString("0.##") + "/100%";
            }

            bar = GetProgressBar("Pain");
            if (bar != null)
            {
                bar.Value = Handler.Player.Stats.Pain;
                bar.Update();
            }
            label = GetLabel("Pain");
            if (label != null)
            {
                label.HoverText = "How much pain you are feeling.\nConsciousness -5 every second at 100%.";
                label.Text = "Pain: " + Handler.Player.Stats.Pain.ToString("0.##") + "/100%";
            }

            bar = GetProgressBar("Paranoia");
            if (bar != null)
            {
                bar.Value = Handler.Player.Stats.Paranoia;
                bar.Update();
            }
            label = GetLabel("Paranoia");
            if (label != null)
            {
                label.HoverText = "How paranoid you are.\nYou will commit suicide at 100%.";
                label.Text = "Paranoia: " + Handler.Player.Stats.Paranoia.ToString("0.##") + "/100%";
            }

            bar = GetProgressBar("Blood");
            if (bar != null)
            {
                bar.Value = Handler.Player.Stats.Blood;
                bar.Update();
            }
            label = GetLabel("Blood");
            if (label != null)
            {
                label.HoverText = "How much blood you have left.\n0% means you are dead.";
                label.Text = "Blood: " + Handler.Player.Stats.Blood.ToString("0.##") + "/100%";
            }

            bar = GetProgressBar("Consciousness");
            if (bar != null)
            {
                bar.Value = Handler.Player.Stats.Consciousness;
                bar.Update();
            }
            label = GetLabel("Consciousness");
            if (label != null)
            {
                label.HoverText = "How conscious you are.\n0% means you are unconscious.";
                label.Text = "Consciousness: " + Handler.Player.Stats.Consciousness.ToString("0.##") + "/100%";
            }

            bar = GetProgressBar("Stamina");
            if (bar != null)
            {
                bar.Value = Handler.Player.Stats.Stamina;
                bar.Update();
            }
            label = GetLabel("Stamina");
            if (label != null)
            {
                label.HoverText = "How much energy you have.\nConsciousness -1 every second at 0%.";
                label.Text = "Stamina: " + Handler.Player.Stats.Stamina.ToString("0.##") + "/100%";
            }

            bar = GetProgressBar("Comfort");
            if (bar != null)
            {
                bar.Value = Handler.Player.Stats.Comfort;
                bar.Update();
            }
            label = GetLabel("Comfort");
            if (label != null)
            {
                label.HoverText = "How comfortable you are.\nStamina -1 every minute at 0%.";
                label.Text = "Comfort: " + Handler.Player.Stats.Comfort.ToString("0.##") + "/100%";
            }
        }

        private void UpdateControlsDisplay()
        {
            Label? control_interact = GetLabel("Control_Interact");
            if (control_interact != null)
            {
                if (Handler.Combat)
                {
                    control_interact.Text = "Attack: LeftMouse";
                }
                else
                {
                    control_interact.Text = "Interact: LeftMouse";
                }
            }

            Label? control_crouch = GetLabel("Control_Crouch");
            if (control_crouch != null)
            {
                control_crouch.Text = "Crouch: " + InputManager.GetMappedKey("Crouch").ToString();
            }

            Label? control_run = GetLabel("Control_Run");
            if (control_run != null)
            {
                control_run.Text = "Run: " + InputManager.GetMappedKey("Run").ToString();
            }

            Label? control_pull = GetLabel("Control_Pull");
            if (control_pull != null)
            {
                if (Handler.Combat)
                {
                    control_pull.Text = "Push: RightMouse";
                }
                else
                {
                    control_pull.Text = "Pull: RightMouse";
                }
            }

            Label? control_combat = GetLabel("Control_Combat");
            if (control_combat != null)
            {
                if (Handler.Combat)
                {
                    control_combat.Text = "Combat Off: " + InputManager.GetMappedKey("Combat").ToString();
                }
                else
                {
                    control_combat.Text = "Combat On: " + InputManager.GetMappedKey("Combat").ToString();
                }
            }
        }

        public override void Load(ContentManager content)
        {
            Clear();

            SpriteFont controlFont = AssetManager.Fonts["ControlFont"];
            Texture2D? black = Handler.GetTexture("Black");

            Texture2D? frame = Handler.GetTexture("Frame");
            Texture2D? frame_Wide = Handler.GetTexture("Frame_Wide");

            Texture2D? progressBase = Handler.GetTexture("ProgressBase");
            Texture2D? progressBar = Handler.GetTexture("ProgressBar");

            Texture2D? button_Menu = Handler.GetTexture("Button_Menu");
            Texture2D? button_Menu_Hover = Handler.GetTexture("Button_Menu_Hover");

            Texture2D? button_Inventory = Handler.GetTexture("Button_Inventory");
            Texture2D? button_Inventory_Hover = Handler.GetTexture("Button_Inventory_Hover");

            Texture2D? button_Health = Handler.GetTexture("Button_Health");
            Texture2D? button_Health_Hover = Handler.GetTexture("Button_Health_Hover");

            Texture2D? button_Stats = Handler.GetTexture("Button_Stats");
            Texture2D? button_Stats_Hover = Handler.GetTexture("Button_Stats_Hover");

            AddPicture(Handler.GetID(), "message_panel", frame_Wide, new Region(0, 0, 0, 0), Color.White * 0.6f, true);
            AddPicture(Handler.GetID(), "view_panel", frame_Wide, new Region(0, 0, 0, 0), Color.White, false);

            AddLabel(controlFont, Handler.GetID(), "Time", "", Color.White, frame_Wide, new Region(0, 0, 0, 0), Color.White * 0f, true);

            AddLabel(new LabelOptions
            {
                font = controlFont,
                id = Handler.GetID(),
                name = "Control_Crouch",
                texture = black,
                draw_color = Color.White * 0.25f,
                text_color = Color.White,
                alignment_horizontal = Alignment.Left,
                opacity = 1f,
                visible = true
            });

            AddLabel(new LabelOptions
            {
                font = controlFont,
                id = Handler.GetID(),
                name = "Control_Run",
                texture = black,
                draw_color = Color.White * 0.25f,
                text_color = Color.White,
                alignment_horizontal = Alignment.Left,
                opacity = 1f,
                visible = true
            });

            AddLabel(new LabelOptions
            {
                font = controlFont,
                id = Handler.GetID(),
                name = "Control_Interact",
                texture = black,
                draw_color = Color.White * 0.25f,
                text_color = Color.White,
                alignment_horizontal = Alignment.Left,
                opacity = 1f,
                visible = true
            });

            AddLabel(new LabelOptions
            {
                font = controlFont,
                id = Handler.GetID(),
                name = "Control_Pull",
                texture = black,
                draw_color = Color.White * 0.25f,
                text_color = Color.White,
                alignment_horizontal = Alignment.Left,
                opacity = 1f,
                visible = true
            });

            AddLabel(new LabelOptions
            {
                font = controlFont,
                id = Handler.GetID(),
                name = "Control_Combat",
                texture = black,
                draw_color = Color.White * 0.25f,
                text_color = Color.White,
                alignment_horizontal = Alignment.Left,
                opacity = 1f,
                visible = true
            });

            AddProgressBar(Handler.GetID(), "Hunger", 100, 0, 1, progressBase, progressBar,
                new Region(0, 0, 0, 0), new Color(0, 100, 0), true);
            AddLabel(controlFont, Handler.GetID(), "Hunger", "Hunger: 0%", Color.White, new Region(0, 0, 0, 0), true);

            AddProgressBar(Handler.GetID(), "Thirst", 100, 0, 1, progressBase, progressBar,
                new Region(0, 0, 0, 0), new Color(0, 0, 100), true);
            AddLabel(controlFont, Handler.GetID(), "Thirst", "Thirst: 0%", Color.White, new Region(0, 0, 0, 0), true);

            AddProgressBar(Handler.GetID(), "Bladder", 100, 0, 1, progressBase, progressBar,
                new Region(0, 0, 0, 0), new Color(100, 100, 0), true);
            AddLabel(controlFont, Handler.GetID(), "Bladder", "Bladder: 0%", Color.White, new Region(0, 0, 0, 0), true);

            AddProgressBar(Handler.GetID(), "Grime", 100, 0, 1, progressBase, progressBar,
                new Region(0, 0, 0, 0), new Color(50, 40, 30), true);
            AddLabel(controlFont, Handler.GetID(), "Grime", "Grime: 0%", Color.White, new Region(0, 0, 0, 0), true);

            AddProgressBar(Handler.GetID(), "Pain", 100, 0, 1, progressBase, progressBar,
                new Region(0, 0, 0, 0), Color.Red, true);
            AddLabel(controlFont, Handler.GetID(), "Pain", "Pain: 0%", Color.White, new Region(0, 0, 0, 0), true);

            AddProgressBar(Handler.GetID(), "Paranoia", 100, 0, 1, progressBase, progressBar,
                new Region(0, 0, 0, 0), new Color(60, 0, 100), true);
            AddLabel(controlFont, Handler.GetID(), "Paranoia", "Paranoia: 0%", Color.White, new Region(0, 0, 0, 0), true);

            AddLabel(new LabelOptions
            {
                font = controlFont,
                id = Handler.GetID(),
                name = "Crouching",
                text = "Crouching",
                text_color = Color.White,
                opacity = 0.6f,
                visible = true
            });

            AddLabel(new LabelOptions
            {
                font = controlFont,
                id = Handler.GetID(),
                name = "Running",
                text = "Running",
                text_color = Color.White,
                opacity = 0.6f,
                visible = true
            });

            AddLabel(new LabelOptions
            {
                font = controlFont,
                id = Handler.GetID(),
                name = "Pulling",
                text = "Pulling",
                text_color = Color.White,
                opacity = 0.6f,
                visible = true
            });

            AddLabel(new LabelOptions
            {
                font = controlFont,
                id = Handler.GetID(),
                name = "Combat",
                text = "Combat",
                text_color = Color.White,
                opacity = 0.6f,
                visible = true
            });

            AddProgressBar(Handler.GetID(), "Blood", 100, 100, 1, progressBase, progressBar,
                new Region(0, 0, 0, 0), new Color(100, 0, 0), true);
            AddLabel(controlFont, Handler.GetID(), "Blood", "Blood: 100%", Color.White, new Region(0, 0, 0, 0), true);

            AddProgressBar(Handler.GetID(), "Consciousness", 100, 100, 1, progressBase, progressBar,
                new Region(0, 0, 0, 0), new Color(128, 64, 0), true);
            AddLabel(controlFont, Handler.GetID(), "Consciousness", "Consciousness: 100%", Color.White, new Region(0, 0, 0, 0), true);

            AddProgressBar(Handler.GetID(), "Stamina", 100, 100, 1, progressBase, progressBar,
                new Region(0, 0, 0, 0), new Color(0, 64, 128), true);
            AddLabel(controlFont, Handler.GetID(), "Stamina", "Stamina: 100%", Color.White, new Region(0, 0, 0, 0), true);

            AddProgressBar(Handler.GetID(), "Comfort", 100, 100, 1, progressBase, progressBar,
                new Region(0, 0, 0, 0), new Color(64, 128, 0), true);
            AddLabel(controlFont, Handler.GetID(), "Comfort", "Comfort: 100%", Color.White, new Region(0, 0, 0, 0), true);

            AddButton(Handler.GetID(), "Main", button_Menu, button_Menu_Hover, null,
                new Region(0, 0, 0, 0), Color.White, true);

            Button? main = GetButton("Main");
            if (main != null)
            {
                main.HoverText = "System";
            }

            AddButton(Handler.GetID(), "Inventory", button_Inventory, button_Inventory_Hover, null,
                new Region(0, 0, 0, 0), Color.White, true);

            Button? inventory = GetButton("Inventory");
            if (inventory != null)
            {
                inventory.HoverText = "Inventory";
            }

            AddButton(Handler.GetID(), "Health", button_Health, button_Health_Hover, null,
                new Region(0, 0, 0, 0), Color.White, true);

            Button? health = GetButton("Health");
            if (health != null)
            {
                health.HoverText = "Health";
            }

            AddButton(Handler.GetID(), "Stats", button_Stats, button_Stats_Hover, null,
                new Region(0, 0, 0, 0), Color.White, false);

            Button? stats = GetButton("Stats");
            if (stats != null)
            {
                stats.HoverText = "Stats";
            }

            AddLabel(controlFont, Handler.GetID(), "Examine", "", Color.White, frame, new Region(0, 0, 0, 0), false);

            for (int i = 0; i < Handler.MessageMax; i++)
            {
                AddLabel(controlFont, Handler.GetID(), "Message" + i.ToString(), "", Color.Red, new Region(0, 0, 0, 0), true);

                Label? label = GetLabel("Message" + i.ToString());
                if (label != null)
                {
                    label.Alignment_Horizontal = Alignment.Left;
                    label.AutoScale = false;
                    label.Scale = 0.9f;
                }
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

            Label? examine = GetLabel("Examine");
            if (examine != null)
            {
                examine.Region = new Region(0, 0, 0, 0);
            }

            float panel_width = Main.Game.MenuSize_X * 5;
            float panel_width_half = panel_width / 2;
            float panel_height = Main.Game.MenuSize_Y * 3;
            float center_width = Main.Game.ScreenWidth - (panel_width * 2);

            //Upper Left
            Button? main = GetButton("Main");
            if (main != null)
            {
                main.Region = new Region(0, 0, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
            }

            Button? inventory = GetButton("Inventory");
            if (inventory != null)
            {
                inventory.Region = new Region(Main.Game.MenuSize_X, 0, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
            }

            Button? health = GetButton("Health");
            if (health != null)
            {
                health.Region = new Region(Main.Game.MenuSize_X * 2, 0, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
            }

            //Upper Center
            Label? time = GetLabel("Time");
            if (time != null)
            {
                time.Region = new Region(Main.Game.ScreenWidth - panel_width, 0, panel_width, Main.Game.MenuSize_Y * 2);
            }

            //Upper Right
            #region Stats

            float x = Main.Game.ScreenWidth - panel_width;
            float y = Main.Game.MenuSize_Y * 2;
            float height = (Main.Game.MenuSize_Y / 4) * 2;

            ProgressBar? hunger_bar = GetProgressBar("Hunger");
            if (hunger_bar != null)
            {
                hunger_bar.Base_Region = new Region(x, y, panel_width, height);
            }

            Label? hunger_label = GetLabel("Hunger");
            if (hunger_label != null)
            {
                hunger_label.Region = new Region(x, y, panel_width, height);
            }

            ProgressBar? thirst_bar = GetProgressBar("Thirst");
            if (thirst_bar != null)
            {
                thirst_bar.Base_Region = new Region(x, y + height, panel_width, height);
            }

            Label? thirst_label = GetLabel("Thirst");
            if (thirst_label != null)
            {
                thirst_label.Region = new Region(x, y + height, panel_width, height);
            }

            ProgressBar? bladder_bar = GetProgressBar("Bladder");
            if (bladder_bar != null)
            {
                bladder_bar.Base_Region = new Region(x, y + (height * 2), panel_width, height);
            }

            Label? bladder_label = GetLabel("Bladder");
            if (bladder_label != null)
            {
                bladder_label.Region = new Region(x, y + (height * 2), panel_width, height);
            }

            ProgressBar? grime_bar = GetProgressBar("Grime");
            if (grime_bar != null)
            {
                grime_bar.Base_Region = new Region(x, y + (height * 3), panel_width, height);
            }

            Label? grime_label = GetLabel("Grime");
            if (grime_label != null)
            {
                grime_label.Region = new Region(x, y + (height * 3), panel_width, height);
            }

            ProgressBar? pain_bar = GetProgressBar("Pain");
            if (pain_bar != null)
            {
                pain_bar.Base_Region = new Region(x, y + (height * 4), panel_width, height);
            }

            Label? pain_label = GetLabel("Pain");
            if (pain_label != null)
            {
                pain_label.Region = new Region(x, y + (height * 4), panel_width, height);
            }
            
            ProgressBar? paranoia_bar = GetProgressBar("Paranoia");
            if (paranoia_bar != null)
            {
                paranoia_bar.Base_Region = new Region(x, y + (height * 5), panel_width, height);
            }

            Label? paranoia_label = GetLabel("Paranoia");
            if (paranoia_label != null)
            {
                paranoia_label.Region = new Region(x, y + (height * 5), panel_width, height);
            }

            ProgressBar? blood_bar = GetProgressBar("Blood");
            if (blood_bar != null)
            {
                blood_bar.Base_Region = new Region(x, y + (height * 10), panel_width, height);
            }

            Label? blood_label = GetLabel("Blood");
            if (blood_label != null)
            {
                blood_label.Region = new Region(x, y + (height * 10), panel_width, height);
            }
            
            ProgressBar? consciousness_bar = GetProgressBar("Consciousness");
            if (consciousness_bar != null)
            {
                consciousness_bar.Base_Region = new Region(x, y + (height * 11), panel_width, height);
            }

            Label? consciousness_label = GetLabel("Consciousness");
            if (consciousness_label != null)
            {
                consciousness_label.Region = new Region(x, y + (height * 11), panel_width, height);
            }

            ProgressBar? stamina_bar = GetProgressBar("Stamina");
            if (stamina_bar != null)
            {
                stamina_bar.Base_Region = new Region(x, y + (height * 12), panel_width, height);
            }

            Label? stamina_label = GetLabel("Stamina");
            if (stamina_label != null)
            {
                stamina_label.Region = new Region(x, y + (height * 12), panel_width, height);
            }

            ProgressBar? comfort_bar = GetProgressBar("Comfort");
            if (comfort_bar != null)
            {
                comfort_bar.Base_Region = new Region(x, y + (height * 13), panel_width, height);
            }

            Label? comfort_label = GetLabel("Comfort");
            if (comfort_label != null)
            {
                comfort_label.Region = new Region(x, y + (height * 13), panel_width, height);
            }

            #endregion

            Label? crouching = GetLabel("Crouching");
            if (crouching != null)
            {
                crouching.Region = new Region(x, y + (height * 6), panel_width, height);
            }

            Label? running = GetLabel("Running");
            if (running != null)
            {
                running.Region = new Region(x, y + (height * 7), panel_width, height);
            }

            Label? pulling = GetLabel("Pulling");
            if (pulling != null)
            {
                pulling.Region = new Region(x, y + (height * 8), panel_width, height);
            }

            Label? combat = GetLabel("Combat");
            if (combat != null)
            {
                combat.Region = new Region(x, y + (height * 9), panel_width, height);
            }

            //Center
            Picture? view_panel = GetPicture("view_panel");
            if (view_panel != null)
            {
                view_panel.Region = new Region(panel_width, 0, Main.Game.ScreenWidth - (panel_width * 2), Main.Game.ScreenHeight - panel_height);
            }

            //Bottom Left
            #region Controls

            float X = panel_width / 10;
            float Y = Main.Game.ScreenHeight - (height * 6);

            Label? control_combat = GetLabel("Control_Combat");
            if (control_combat != null)
            {
                control_combat.Region = new Region(X, Y, panel_width, height);
            }

            Y += height;
            Label? control_interact = GetLabel("Control_Interact");
            if (control_interact != null)
            {
                control_interact.Region = new Region(X, Y, panel_width, height);
            }

            Y += height;
            Label? control_pull = GetLabel("Control_Pull");
            if (control_pull != null)
            {
                control_pull.Region = new Region(X, Y, panel_width, height);
            }

            Y += height;
            Label? control_crouch = GetLabel("Control_Crouch");
            if (control_crouch != null)
            {
                control_crouch.Region = new Region(X, Y, panel_width, height);
            }

            Y += height;
            Label? control_run = GetLabel("Control_Run");
            if (control_run != null)
            {
                control_run.Region = new Region(X, Y, panel_width, height);
            }
            
            #endregion

            //Bottom Center
            Y = Main.Game.ScreenHeight - panel_height;
            X = panel_width + panel_width_half;

            Picture? message_panel = GetPicture("message_panel");
            if (message_panel != null)
            {
                message_panel.Region = new Region(X, Y, center_width - panel_width, panel_height);

                float message_height = panel_height / Handler.MessageMax;
                for (int i = 0; i < Handler.MessageMax; i++)
                {
                    Label? message = GetLabel("Message" + i.ToString());
                    if (message != null)
                    {
                        message.Region = new Region(message_panel.Region.X, Y, message_panel.Region.Width, message_height);
                        message.Scale = (float)panel_height / 212;
                    }

                    Y += message_height;
                }
            }
        }

        private void Examine(string text)
        {
            if (Main.Game == null)
            {
                return;
            }

            Label? examine = GetLabel("Examine");
            if (examine == null ||
                InputManager.Mouse == null)
            {
                return;
            }

            examine.Text = text;

            int width = (int)(Main.Game.MenuSize_X * 7);
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

        #endregion
    }
}

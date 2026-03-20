using System.IO;
using System.Windows.Forms;
using System.Xml;
using OP_Engine.Inputs;
using OP_Engine.Sounds;
using OP_Engine.Enums;

namespace Despicaville.Util
{
    public static class LoadUtil
    {
        #region Parse INI

        public static void ParseINI(string file)
        {
            using (XmlTextReader reader = new XmlTextReader(File.OpenRead(file)))
            {
                try
                {
                    while (reader.Read())
                    {
                        switch (reader.Name)
                        {
                            case "Game":
                                VisitGame(reader);
                                break;
                        }
                    }
                }
                catch
                {
                    //Config was probably deleted
                }
            }
        }

        private static void VisitGame(XmlTextReader reader)
        {
            while (reader.Read())
            {
                if (reader.Name == "Game" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "Options":
                        VisitOptions(reader);
                        break;

                    case "Controls":
                        VisitControls(reader);
                        break;
                }
            }
        }

        private static void VisitOptions(XmlTextReader reader)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "Fullscreen":
                        if (reader.Value == "True")
                        {
                            Main.Game.ScreenType = ScreenType.BorderlessFullscreen;
                            Main.Game.ResetScreen();
                        }
                        else
                        {
                            Main.Game.ScreenType = ScreenType.Windowed;
                            Main.Game.Form.WindowState = FormWindowState.Normal;
                            Main.Game.ResetScreen();
                        }
                        break;

                    case "MusicEnabled":
                        if (reader.Value == "True")
                        {
                            SoundManager.MusicEnabled = true;
                        }
                        else
                        {
                            SoundManager.MusicEnabled = false;
                        }
                        break;

                    case "MusicVolume":
                        SoundManager.MusicVolume = float.Parse(reader.Value) / 10;
                        break;

                    case "AmbientEnabled":
                        if (reader.Value == "True")
                        {
                            SoundManager.AmbientEnabled = true;
                        }
                        else
                        {
                            SoundManager.AmbientEnabled = false;
                        }
                        break;

                    case "AmbientVolume":
                        SoundManager.AmbientVolume = float.Parse(reader.Value) / 10;
                        break;

                    case "SoundEnabled":
                        if (reader.Value == "True")
                        {
                            SoundManager.SoundEnabled = true;
                        }
                        else
                        {
                            SoundManager.SoundEnabled = false;
                        }
                        break;

                    case "SoundVolume":
                        SoundManager.SoundVolume = float.Parse(reader.Value) / 10;
                        break;

                    case "Zoom":
                        Main.Game.Zoom = int.Parse(reader.Value);
                        break;
                }
            }
        }

        private static void VisitControls(XmlTextReader reader)
        {
            while (reader.MoveToNextAttribute())
            {
                foreach (var map in InputManager.Keyboard.KeysMapped)
                {
                    if ("Key_" + map.Key == reader.Name)
                    {
                        InputManager.Keyboard.KeysMapped[map.Key] = InputManager.GetKey(reader.Value);
                        break;
                    }
                }
            }
        }

        #endregion
    }
}

using System;
using System.IO;
using System.Xml;

using OP_Engine.Inputs;
using OP_Engine.Sounds;
using OP_Engine.Utility;

namespace Despicaville.Util
{
    public static class Save
    {
        #region Variables

        private static Stream SaveStream;
        private static XmlWriter Writer;

        #endregion

        #region XML Methods

        private static void WriteStream(string path)
        {
            SaveStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.IndentChars = "\t";
            Writer = XmlWriter.Create(SaveStream, xmlWriterSettings);
            Writer.WriteStartDocument();
        }

        private static void EnterNode(string elementName)
        {
            Writer.WriteStartElement(elementName);
        }

        private static void ExitNode()
        {
            Writer.WriteEndElement();
        }

        private static void FinalizeWriting()
        {
            if (Writer != null &&
                SaveStream != null)
            {
                if (Writer.WriteState != WriteState.Error)
                {
                    Writer.WriteEndDocument();
                    Writer.Close();
                    SaveStream.Close();
                }
                else
                {
                    Writer.Close();
                    SaveStream.Close();
                }
            }
        }

        #endregion

        #region Export INI

        public static void ExportINI()
        {
            try
            {
                if (AssetManager.Files.ContainsKey("Config"))
                {
                    string file = AssetManager.Files["Config"];
                    WriteStream(file);
                    SaveINI();
                }
            }
            catch (Exception e)
            {
                string error = e.Message;
            }
            finally
            {
                FinalizeWriting();
            }
            GC.Collect();
        }

        private static void SaveINI()
        {
            EnterNode("Game");

            #region Options

            EnterNode("Options");
            Writer.WriteAttributeString("Fullscreen", Main.Game.GraphicsManager.IsFullScreen.ToString());
            Writer.WriteAttributeString("MusicEnabled", SoundManager.MusicEnabled.ToString());
            Writer.WriteAttributeString("MusicVolume", (SoundManager.MusicVolume * 10).ToString());
            Writer.WriteAttributeString("AmbientEnabled", SoundManager.AmbientEnabled.ToString());
            Writer.WriteAttributeString("AmbientVolume", (SoundManager.AmbientVolume * 10).ToString());
            Writer.WriteAttributeString("SoundEnabled", SoundManager.SoundEnabled.ToString());
            Writer.WriteAttributeString("SoundVolume", (SoundManager.SoundVolume * 10).ToString());
            Writer.WriteAttributeString("Zoom", Main.Game.Zoom.ToString());
            ExitNode();

            #endregion

            #region Controls

            EnterNode("Controls");

            EnterNode("Cancel");
            Writer.WriteAttributeString("Key_Cancel", InputManager.GetMappedKey("Cancel").ToString());
            ExitNode();

            EnterNode("Up");
            Writer.WriteAttributeString("Key_Up", InputManager.GetMappedKey("Up").ToString());
            ExitNode();

            EnterNode("Right");
            Writer.WriteAttributeString("Key_Right", InputManager.GetMappedKey("Right").ToString());
            ExitNode();

            EnterNode("Down");
            Writer.WriteAttributeString("Key_Down", InputManager.GetMappedKey("Down").ToString());
            ExitNode();

            EnterNode("Left");
            Writer.WriteAttributeString("Key_Left", InputManager.GetMappedKey("Left").ToString());
            ExitNode();

            EnterNode("Interact");
            Writer.WriteAttributeString("Key_Interact", InputManager.GetMappedKey("Interact").ToString());
            ExitNode();

            EnterNode("Turn");
            Writer.WriteAttributeString("Key_Turn", InputManager.GetMappedKey("Turn").ToString());
            ExitNode();

            EnterNode("Inventory");
            Writer.WriteAttributeString("Key_Inventory", InputManager.GetMappedKey("Inventory").ToString());
            ExitNode();

            EnterNode("Stats");
            Writer.WriteAttributeString("Key_Stats", InputManager.GetMappedKey("Stats").ToString());
            ExitNode();

            EnterNode("Skills");
            Writer.WriteAttributeString("Key_Skills", InputManager.GetMappedKey("Skills").ToString());
            ExitNode();

            EnterNode("Map");
            Writer.WriteAttributeString("Key_Map", InputManager.GetMappedKey("Map").ToString());
            ExitNode();

            EnterNode("Wait");
            Writer.WriteAttributeString("Key_Wait", InputManager.GetMappedKey("Wait").ToString());
            ExitNode();

            ExitNode();

            #endregion

            ExitNode();
        }

        #endregion
    }
}

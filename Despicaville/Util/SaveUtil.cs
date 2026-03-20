using System;
using System.IO;
using System.Xml;

using OP_Engine.Inputs;
using OP_Engine.Sounds;
using OP_Engine.Utility;

namespace Despicaville.Util
{
    public static class SaveUtil
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
            Writer.WriteAttributeString("Key_Up", InputManager.GetMappedKey("Up").ToString());
            Writer.WriteAttributeString("Key_Right", InputManager.GetMappedKey("Right").ToString());
            Writer.WriteAttributeString("Key_Down", InputManager.GetMappedKey("Down").ToString());
            Writer.WriteAttributeString("Key_Left", InputManager.GetMappedKey("Left").ToString());
            Writer.WriteAttributeString("Key_Crouch", InputManager.GetMappedKey("Crouch").ToString());
            Writer.WriteAttributeString("Key_Run", InputManager.GetMappedKey("Run").ToString());
            Writer.WriteAttributeString("Key_Combat", InputManager.GetMappedKey("Combat").ToString());
            Writer.WriteAttributeString("Key_Cancel", InputManager.GetMappedKey("Cancel").ToString());
            Writer.WriteAttributeString("Key_Inventory", InputManager.GetMappedKey("Inventory").ToString());
            Writer.WriteAttributeString("Key_Map", InputManager.GetMappedKey("Map").ToString());
            Writer.WriteAttributeString("Key_Wait", InputManager.GetMappedKey("Wait").ToString());
            ExitNode();

            #endregion

            ExitNode();
        }

        #endregion
    }
}

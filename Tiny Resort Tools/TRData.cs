using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace TinyResort {

    // TODO: Prefix saving function to save then
    // TODO: Prefix loading function to load then
    // TODO: What happens if you delete save slot in game?
    public static class TRData {

        private static int TRToolsVersion;
        private static Dictionary<string, ModData> Data = new Dictionary<string, ModData>();

        private static int currentSlot => SaveLoad.saveOrLoad.currentSaveSlotNo();
        private static string dataPath => Path.Combine(Application.persistentDataPath, "Slot" + currentSlot, "Mod Data");
        
        /// <summary>Register to this to run events when the mod data is saved.</summary>
        public static OnSave onSave;
        public delegate void OnSave();

        public static void Save(string pluginGuid) {

            // Sends messages to interested mods that the game is saving
            onSave?.Invoke();

            // Creates the folder and finds the save file location
            Directory.CreateDirectory(dataPath);
            var savePath = Path.Combine(dataPath, pluginGuid + ".sav");

            // Back up the previous save file if one exists
            if (File.Exists(savePath)) { File.Copy(savePath, Path.Combine(dataPath, "backup_" + pluginGuid + ".sav")); }

            // Records important information about this save
            Data[pluginGuid].SaveTime = DateTime.Now;
            Data[pluginGuid].TRToolsVersion = TRToolsVersion;

            // Creates file
            var Formatter = new BinaryFormatter();
            var NewFile = File.Create(savePath);
            Formatter.Serialize(NewFile, Data);
            NewFile.Close();

        }

        // Loads a save file
        public static void Load(string pluginGuid) {


            // If there is no save file, create new mod data
            if (!File.Exists(Path.Combine(dataPath, pluginGuid + ".sav"))) { Data[pluginGuid] = new ModData(); }

            // Otherwise, load the latest save file as the mod data
            else {
                var Formatter = new BinaryFormatter();
                var LoadedFile = File.Open(Path.Combine(dataPath, pluginGuid + ".sav"), FileMode.Open);
                Data[pluginGuid] = (ModData) Formatter.Deserialize(LoadedFile);
                LoadedFile.Close();
            }

        }



    }

    [Serializable]
    public class ModData {

        // Save Info
        public DateTime SaveTime;
        public int TRToolsVersion;

        private Dictionary<string, int> savedInts = new Dictionary<string, int>();
        public void SetInt(string name, int value) { savedInts[name] = value; }
        public int GetInt(string name) {
            if (savedInts.TryGetValue(name, out var val)) { return val; }
            return 0;
        }
        
        public Dictionary<string, bool> savedBools = new Dictionary<string, bool>();
        public void SetBool(string name, bool value) { savedBools[name] = value; }
        public bool GetBool(string name) {
            if (savedBools.TryGetValue(name, out var val)) { return val; }
            return false;
        }
        
        public Dictionary<string, float> savedFloats = new Dictionary<string, float>();
        public void SetFloat(string name, float value) { savedFloats[name] = value; }
        public float GetFloat(string name) {
            if (savedFloats.TryGetValue(name, out var val)) { return val; }
            return 0f;
        }
        
        public Dictionary<string, string> savedStrings = new Dictionary<string, string>();
        public void SetString(string name, string value) { savedStrings[name] = value; }
        public string GetString(string name) {
            if (savedStrings.TryGetValue(name, out var val)) { return val; }
            return "";
        }
        

    }

}
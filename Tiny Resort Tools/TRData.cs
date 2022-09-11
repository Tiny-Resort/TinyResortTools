using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace TinyResort {

    public static class TRData {

        private static int DataVersion;

        internal static Dictionary<string, TRModData> Data = new Dictionary<string, TRModData>();

        private static int currentSlot => SaveLoad.saveOrLoad.currentSaveSlotNo();
        private static string slotDataPath => Path.Combine(Application.persistentDataPath, "Slot" + currentSlot, "Mod Data");
        private static string globalDataPath => Path.Combine(Application.persistentDataPath, "Mod Data");

        public delegate void SaveEvent();

        /// <summary> This event will run just before the game saves and before the cleanDataEvent.
        /// This is where you want to ensure you have set all values that you wish to be saved.</summary>
        public static SaveEvent preSaveEvent;

        /// <summary>
        /// This is called before saving the game. If your mod adds anything to the base game's save file (such as custom objects in the world), then
        /// you generally need to remove that before the game saves. This is because if the player removes your mod, the game might freeze or crash when
        /// loading save data.
        /// </summary>
        public static SaveEvent cleanDataEvent;

        /// <summary>This event will run just AFTER the game saves and after the injectDataEvent.</summary>
        public static SaveEvent postSaveEvent;

        /// <summary>
        /// This is called both after the game is saved and when loading in. In your subscribed method, you should carry out any consequences of the data
        /// being loaded. For instance, if you have a custom item in the game and the player has it according to your mod data, then here is where you'd
        /// actually give it to the player.
        /// </summary>
        public static SaveEvent injectDataEvent;
        
        // <summary>
        // This event will run just BEFORE a save slot is loaded from the main menu. Useful for resetting values that shouldn't persist when
        // loading into another save file.
        // </summary>
        public static SaveEvent initialLoadEvent;

        /// <summary>This event will run just BEFORE a save slot is loaded.</summary>
        public static SaveEvent preLoadEvent;

        /// <summary>This event will run just AFTER a save slot is loaded, just before the ijectDataEvent.</summary>
        public static SaveEvent postLoadEvent;

        /// <summary>Subscribes to the save system so that your mod data is saved and loaded properly.</summary>
        /// <param name="fileName">The name of your save file. Could be anything unique, but I recommend using the GUID of your plugin.</param>
        /// <param name="dataFormat">What kind of serialization you want to use.</param>
        /// <param name="globalSave">If true, then this data will be saved to a global save file used for all save slots. Otherwise, your save file will be specific to each save slot.</param>
        /// <returns>A link to the data for your mod. Keep a reference to this. It is used to get and set all saved values.</returns>
        public static TRModData Subscribe(string fileName, TRDataFormats dataFormat, bool globalSave = false) {
            Data[fileName] = new TRModData {
                fileName = fileName,
                format = dataFormat,
                global = globalSave
            };
            Data[fileName].package = new DataPackage();
            postSaveEvent += () => Save(fileName);
            postLoadEvent += () => Load(fileName);
            return Data[fileName];
        }

        /// <summary>Saves any values that have been set to a mod-specific file in the current save slot folder.</summary>
        /// <param name="fileName">The name of your save file. Could be anything unique, but I recommend using the GUID of your plugin.</param>
        internal static void Save(string fileName) {

            if (TRTools.InMainMenu) {
                TRTools.Log(fileName + " is trying to save while in the main menu.", LogSeverity.Error);
                return;
            }

            var path = Data[fileName].global ? globalDataPath : slotDataPath;

            // Creates the folder and finds the save file location
            Directory.CreateDirectory(path);
            var savePath = Path.Combine(path, fileName + ".sav");

            // Back up the previous save file if one exists
            if (File.Exists(savePath)) {
                var backupPath = Path.Combine(path, "backup_" + fileName + ".sav");
                if (File.Exists(backupPath)) { File.Delete(backupPath);}
                File.Copy(savePath, backupPath);
            }

            // Records important information about this save
            Data[fileName].package.SaveTime = DateTime.Now;
            Data[fileName].package.DataVersion = DataVersion;
            
            var NewFile = File.Create(savePath);

            // Saves in binary format
            if (Data[fileName].format == TRDataFormats.Binary) {
                var Formatter = new BinaryFormatter();
                Formatter.Serialize(NewFile, Data[fileName].package);
                NewFile.Close();
            }

            // Saves in JSON format
            else if (Data[fileName].format == TRDataFormats.JSON) {
                string json = JsonUtility.ToJson(Data[fileName].package);
                File.WriteAllText(savePath, json);
            }

        }

        internal static void Load(string fileName) {

            if (TRTools.InMainMenu || !Data.ContainsKey(fileName)) { return; }

            var path = Data[fileName].global ? globalDataPath : slotDataPath;

            // If there is no save file, create new mod data
            if (!File.Exists(Path.Combine(path, fileName + ".sav"))) { Data[fileName].package = new DataPackage(); }

            // Otherwise, load the latest save file as the mod data
            else {
                if (Data[fileName].format == TRDataFormats.Binary) { Data[fileName].package = LoadFromBinary(Path.Combine(path, fileName + ".sav")); }
                else if (Data[fileName].format == TRDataFormats.JSON) { Data[fileName].package = LoadFromJson(Path.Combine(path, fileName + ".sav")); }
            }

        }

        internal static DataPackage LoadFromJson(string path, bool secondTry = false) {
            try {
                var text = File.ReadAllText(path);
                return JsonUtility.FromJson<DataPackage>(text);
            }
            catch {
                if (secondTry) return null;
                return LoadFromBinary(path, true);
            }
        }

        internal static DataPackage LoadFromBinary(string path, bool secondTry = false) {
            try {
                var LoadedFile = File.Open(path, FileMode.Open);
                var Formatter = new BinaryFormatter();
                var pack = (DataPackage)Formatter.Deserialize(LoadedFile);
                LoadedFile.Close();
                return pack;
            }
            catch {
                if (secondTry) return null;
                return LoadFromJson(path, true);
            }
        }

    }

    public class TRModData {
        
        internal DataPackage package = new DataPackage();

        internal string fileName;
        internal TRDataFormats format;
        internal bool global;

        /// <summary>Stores an object in the save data for your mod. Can be called either when an object is updated or during a preSave event.</summary>
        /// <param name="name">An identifier for the variable. Can simply be the name of the variable.</param>
        /// <param name="value">The object that you wish to save. Must be a serializable type.</param>
        public void SetValue(string name, object value) { package.data[name] = value; }

        /// <summary>Gets the stored value for an object of the given name in your mod's save data.</summary>
        /// <param name="name">An identifier for the variable. Can simply be the name of the variable.</param>
        /// <param name="defaultValue">If the value you're getting has never been set, this value is returned instead.</param>
        /// <returns>The object that are loading. You need to cast this to the desired type, but be sure to check if it is null before doing so.</returns>
        public object GetValue(string name, object defaultValue = null) {
            if (package.data.TryGetValue(name, out var val)) { return val; }
            return defaultValue;
        }

        /// <summary>Removes the given variable from the save file completely.</summary>
        /// <param name="name">The name of the variable.</param>
        public void Remove(string name) {
            if (!package.data.ContainsKey(name)) return;
            package.data.Remove(name);
        }

        public void Save() { TRData.Save(fileName); }
        public void Load() { TRData.Load(fileName); }
        
    }

    [Serializable]
    internal class DataPackage {
        public DateTime SaveTime;
        public int DataVersion;
        public Dictionary<string, object> data = new Dictionary<string, object>();
    }
    
    public enum TRDataFormats { Binary, JSON }

}
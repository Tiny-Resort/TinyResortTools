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
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Dictionary<string, ModData> Data = new Dictionary<string, ModData>();

        private static int currentSlot => SaveLoad.saveOrLoad.currentSaveSlotNo();
        private static string dataPath => Path.Combine(Application.persistentDataPath, "Slot" + currentSlot, "Mod Data");

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

        /// <summary>
        /// This is called both after the game is saved and when loading in. In your subscribed method, you should carry out any consequences of the data
        /// being loaded. For instance, if you have a custom item in the game and the player has it according to your mod data, then here is where you'd
        /// actually give it to the player.
        /// </summary>
        public static SaveEvent injectDataEvent;

        /// <summary>This event will run just AFTER the game saves and after the injectDataEvent.</summary>
        public static SaveEvent postSaveEvent;

        /// <summary>This event will run just BEFORE a save slot is loaded.</summary>
        public static SaveEvent preLoadEvent;

        /// <summary>This event will run just AFTER a save slot is loaded, just before the ijectDataEvent.</summary>
        public static SaveEvent postLoadEvent;

        /// <summary>Subscribes to the save system so that your mod data is saved and loaded properly.</summary>
        /// <param name="pluginGuid">This will be the name of your save file and could be anything unique, but I recommend using the GUID of your plugin.</param>
        /// <returns>A link to the data for your mod. Keep a reference to this. It is used to get and set all saved values.</returns>
        public static ModData Subscribe(string pluginGuid) {
            Data[pluginGuid] = new ModData();
            Data[pluginGuid].Package = new DataPackage();
            postSaveEvent += () => Save(pluginGuid);
            postLoadEvent += () => Load(pluginGuid);
            return Data[pluginGuid];
        }
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Save(string pluginGuid) {

            if (TRTools.InMainMenu) { return; }

            // Creates the folder and finds the save file location
            Directory.CreateDirectory(dataPath);
            var savePath = Path.Combine(dataPath, pluginGuid + ".sav");

            // Back up the previous save file if one exists
            if (File.Exists(savePath)) {
                var backupPath = Path.Combine(dataPath, "backup_" + pluginGuid + ".sav");
                if (File.Exists(backupPath)) { File.Delete(backupPath);}
                File.Copy(savePath, backupPath);
            }

            // Records important information about this save
            Data[pluginGuid].Package.SaveTime = DateTime.Now;
            Data[pluginGuid].Package.DataVersion = DataVersion;

            // Creates file
            var Formatter = new BinaryFormatter();
            var NewFile = File.Create(savePath);
            Formatter.Serialize(NewFile, Data[pluginGuid].Package);
            NewFile.Close();

        }
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Load(string pluginGuid) {

            if (TRTools.InMainMenu || !Data.ContainsKey(pluginGuid)) { return; }

            // If there is no save file, create new mod data
            if (!File.Exists(Path.Combine(dataPath, pluginGuid + ".sav"))) { Data[pluginGuid].Package = new DataPackage(); }

            // Otherwise, load the latest save file as the mod data
            else {
                var Formatter = new BinaryFormatter();
                var LoadedFile = File.Open(Path.Combine(dataPath, pluginGuid + ".sav"), FileMode.Open);
                Data[pluginGuid].Package = (DataPackage) Formatter.Deserialize(LoadedFile);
                LoadedFile.Close();
            }

        }

    }

    [Serializable]
    public class ModData {
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        [SerializeField] public DataPackage Package = new DataPackage();

        /// <summary>Stores an object in the save data for your mod. Can be called either when an object is updated or during a preSave event.</summary>
        /// <param name="name">An identifier for the variable. Can simply be the name of the variable.</param>
        /// <param name="value">The object that you wish to save. Must be a serializable type.</param>
        public void SetValue(string name, object value) { Package.data[name] = value; }

        /// <summary>Gets the stored value for an object of the given name in your mod's save data.</summary>
        /// <param name="name">An identifier for the variable. Can simply be the name of the variable.</param>
        /// <param name="defaultValue">If the value you're getting has never been set, this value is returned instead.</param>
        /// <returns>The object that are loading. You need to cast this to the desired type, but be sure to check if it is null before doing so.</returns>
        public object GetValue(string name, object defaultValue = null) {
            if (Package.data.TryGetValue(name, out var val)) { return val; }
            return defaultValue;
        }
        
    }

    [Serializable]
    public class DataPackage {
        public DateTime SaveTime;
        public int DataVersion;
        public Dictionary<string, object> data = new Dictionary<string, object>();
    }

}
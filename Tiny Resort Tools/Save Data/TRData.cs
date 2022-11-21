using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace TinyResort;

/// <summary>Tools for adding custom save data to your mod.</summary>
public static class TRData {

    /// <summary>Delegate for events related to points in the base game saving and loading.</summary>
    public delegate void SaveEvent();

    private static readonly int DataVersion = 0;

    internal static Dictionary<string, TRModData> Data = new();

    /// <summary>
    ///     This event will run just before the game saves and before the cleanDataEvent.
    ///     This is where you want to ensure you have set all values that you wish to be saved.
    /// </summary>
    public static SaveEvent preSaveEvent;

    /// <summary>
    ///     This is called before saving the game. If your mod adds anything to the base game's save file (such as custom
    ///     objects in the world), then
    ///     you generally need to remove that before the game saves. This is because if the player removes your mod, the game
    ///     might freeze or crash when
    ///     loading save data.
    /// </summary>
    public static SaveEvent cleanDataEvent;

    /// <summary>This event will run just AFTER the game saves and after the injectDataEvent.</summary>
    public static SaveEvent postSaveEvent;

    /// <summary>
    ///     This is called both after the game is saved and when loading in. In your subscribed method, you should carry out
    ///     any consequences of the data
    ///     being loaded. For instance, if you have a custom item in the game and the player has it according to your mod data,
    ///     then here is where you'd
    ///     actually give it to the player.
    /// </summary>
    public static SaveEvent injectDataEvent;
    /*
            /// <summary>
            /// This event will run just AFTER a save slot is loaded from the main menu. Useful for resetting values that shouldn't persist when
            /// loading into another save file.
            /// </summary>
            public static SaveEvent initialLoadEvent;
    */
    /// <summary>This event will run just BEFORE a save slot is loaded.</summary>
    public static SaveEvent preLoadEvent;

    /// <summary>This event will run just AFTER a save slot is loaded, just before the ijectDataEvent.</summary>
    public static SaveEvent postLoadEvent;

    // These events are only accessible to us so that we can make all custom data is fully saved or loaded before the postLoad and postSave events can be used by mod authors
    internal static SaveEvent trueSaveEvent;
    internal static SaveEvent trueLoadEvent;

    private static int currentSlot => SaveLoad.saveOrLoad.currentSaveSlotNo();
    private static string slotDataPath =>
        Path.Combine(Application.persistentDataPath, "Slot" + currentSlot, "Mod Data");
    private static string globalDataPath => Path.Combine(Application.persistentDataPath, "Mod Data");

    /// <summary>Subscribes to the save system so that your mod data is saved and loaded properly.</summary>
    /// <param name="fileName">
    ///     The name of your save file. Could be anything unique, but I recommend using the GUID of your
    ///     plugin.
    /// </param>
    /// <param name="globalSave">
    ///     If true, then this data will be saved to a global save file used for all save slots.
    ///     Otherwise, your save file will be specific to each save slot.
    /// </param>
    /// <returns>A link to the data for your mod. Keep a reference to this. It is used to get and set all saved values.</returns>
    public static TRModData Subscribe(string fileName, bool globalSave = false) {
        Data[fileName] = new TRModData { fileName = fileName, global = globalSave };
        Data[fileName].package = new DataPackage();
        trueSaveEvent += () => Save(fileName);
        trueLoadEvent += () => Load(fileName);
        return Data[fileName];
    }

    /// <summary>Saves any values that have been set to a mod-specific file in the current save slot folder.</summary>
    /// <param name="fileName">
    ///     The name of your save file. Could be anything unique, but I recommend using the GUID of your
    ///     plugin.
    /// </param>
    internal static void Save(string fileName) {

        // If the player is in the main menu and the save file is per-slot then don't allow saving
        if (TRTools.InMainMenu && !Data[fileName].global) {
            TRTools.LogError(
                fileName + " is trying to save while in the main menu but is not a global save file. "
                         + "To save a non-global save file, you must be loaded into an existing save slot."
            );
            return;
        }

        // Creates the folder and finds the save file location
        var path = Data[fileName].global ? globalDataPath : slotDataPath;
        Directory.CreateDirectory(path);
        var savePath = Path.Combine(path, fileName + ".sav");

        // Back up the previous save file if one exists
        if (File.Exists(savePath)) {
            var backupPath = Path.Combine(path, "backup_" + fileName + ".sav");
            if (File.Exists(backupPath)) File.Delete(backupPath);
            File.Copy(savePath, backupPath);
        }

        // Records important information about this save
        Data[fileName].package.LastSave = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        Data[fileName].package.DataVersion = DataVersion;

        // Saves in binary format
        var NewFile = File.Create(savePath);
        var Formatter = new BinaryFormatter();
        Formatter.Serialize(NewFile, Data[fileName].package);
        NewFile.Close();

    }

    internal static void Load(string fileName) {

        // If the player is in the main menu and the save file is per-slot then don't allow loading
        if (!TRTools.LeavingMainMenu && !Data[fileName].global) {
            TRTools.LogError(
                fileName + " is trying to load while in the main menu but is not a global save file. "
                         + "To load a non-global save file, you must be already loaded into an existing save slot."
            );
            return;
        }

        var folderPath = Data[fileName].global ? globalDataPath : slotDataPath;
        var filePath = Path.Combine(folderPath, fileName + ".sav");

        // If there is no save file, create new mod data
        if (!File.Exists(filePath)) { Data[fileName].package = new DataPackage(); }

        else {
            var LoadedFile = File.Open(filePath, FileMode.Open);
            var Formatter = new BinaryFormatter();
            Data[fileName].package = (DataPackage)Formatter.Deserialize(LoadedFile);
            LoadedFile.Close();
        }

    }
}

/// <summary>Container for custom save data and functions used to affect it.</summary>
public class TRModData {

    internal string fileName;
    internal bool global;

    internal DataPackage package = new();

    /// <summary>
    ///     Stores an object in the save data for your mod. Can be called either when an object is updated or during a
    ///     preSave event.
    /// </summary>
    /// <param name="name">An identifier for the variable. Can simply be the name of the variable.</param>
    /// <param name="value">The object that you wish to save. Must be a serializable type.</param>
    public void SetValue(string name, object value) => package.data[name] = value;

    /// <summary>Gets the stored value for an object of the given name in your mod's save data.</summary>
    /// <param name="name">An identifier for the variable. Can simply be the name of the variable.</param>
    /// <param name="defaultValue">If the value you're getting has never been set, this value is returned instead.</param>
    /// <returns>
    ///     The object that are loading. You need to cast this to the desired type, but be sure to check if it is null
    ///     before doing so.
    /// </returns>
    public object GetValue(string name, object defaultValue = null) {
        if (package.data.TryGetValue(name, out var val)) return val;
        return defaultValue;
    }

    /// <summary>Removes the given variable from the save file completely.</summary>
    /// <param name="name">The name of the variable.</param>
    public void Remove(string name) {
        if (!package.data.ContainsKey(name)) return;
        package.data.Remove(name);
    }

    /// <summary>Manually creates or updates a save file specific to your mod.</summary>
    public void Save() => TRData.Save(fileName);

    /// <summary>Manually loads a save file specific to your mod, if one exists.</summary>
    public void Load() => TRData.Load(fileName);
}

[Serializable]
internal class DataPackage {
    public string LastSave;
    public int DataVersion;
    public Dictionary<string, object> data = new();
}

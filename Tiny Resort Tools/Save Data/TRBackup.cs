using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Buffers;
using System.IO;
using System.Resources;
using System.Runtime;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.Bootstrap;
using UnityEngine;
using Ionic.Zip;

namespace TinyResort;

internal class TRBackup {

    private static ConfigEntry<string> CustomSavePath;
    private static ConfigEntry<int> BackupCount;
    private static ConfigEntry<bool> TopNotification;
    private static ConfigEntry<bool> UseBackupManager;
    private static List<string> backupList = new();

    private static readonly List<DirectoryInfo> currentBackups = new();
    private static List<PluginInfo> pluginInfos = new();
    internal static bool clientInServer;

    private static string islandName;
    private static int saveSlot;
    private static string savePath;
    private static string saveDestinationSlot;
    private static readonly string DataPath = Application.persistentDataPath;
    private static readonly string SaveDestination = Application.persistentDataPath.Replace("Dinkum", "Backups");
    private static readonly string globalSaveLocation = Path.Combine(SaveDestination, "PreAPI Backups");
    private static TRModData Data;

    internal static void Initialize() {
        Data = TRData.Subscribe("TR.BackupManager");

        #region Configurations

        UseBackupManager = LeadPlugin.instance.Config.Bind(
            "Save Backup Manager", "UseBackupManager", true,
            "Set to false to disable the use of the backup manager."
        );

        CustomSavePath = LeadPlugin.instance.Config.Bind<string>(
            "Save Backup Manager", "BackupLocation", null,
            "Override the default backup location. The default location is in the James Bendon, this is recommended since it is already close to your current saves locations.\nPlease use a foward slash '/' as the path separator. If you want to use a back slash, you need to use two back slashes.\nWARNING: This deleted the oldest file in the directory. There are some safeholds to try to only delete the mod creates, but please use a folder only for the backups and don't store sensitive data in there."
        );
        BackupCount = LeadPlugin.instance.Config.Bind(
            "Save Backup Manager", "BackupCount", 10,
            "The number of backups you want to store per save slot. Set to -1 to disable limit."
        );
        TopNotification = LeadPlugin.instance.Config.Bind(
            "Save Backup Manager", "TopNotification", true, "Show a notification on success or failure of a backup."
        );

        // backupList = LeadPlugin.instance.Config.Bind<string>("Save Backup Manager", "BackupList", null, "DO NOT EDIT. This is storing a list of current backups, so it won't have a chance to delete other files.");

        #endregion

        pluginInfos = UnityChainloader.Instance.Plugins.Values.ToList();
        TRData.postLoadEvent += CreateInitialBackup;
        var plugInfo = pluginInfos.Find(i => i.Metadata.GUID == "dev.TinyResort.SaveBackupManager");
        if (plugInfo == null && UseBackupManager.Value)
            TRData.postSaveEvent += CreateBackup;
        else if (plugInfo != null) TRTools.Log("Loaded Save Backup Manager Mod (Not API).");

    }

    internal static void LoadSavedBackups() =>
        backupList = (List<string>)Data.GetValue("BackupList", new List<string>());

    private static void getOrCreateSavePaths() {
        saveSlot = SaveLoad.saveOrLoad.currentSaveSlotNo();
        savePath = Path.Combine(DataPath, "Slot" + saveSlot);
        if (CustomSavePath.Value.IsNullOrWhiteSpace()) {
            saveDestinationSlot = Path.Combine(SaveDestination, "Slot" + saveSlot);
            TRTools.Log($"Save Destination: {SaveDestination}");
        }
        else { saveDestinationSlot = Path.Combine(CustomSavePath.Value, "Slot" + saveSlot); }
        if (!Directory.Exists(saveDestinationSlot)) Directory.CreateDirectory(saveDestinationSlot);
    }

    private static void RemoveLastBackup(string currentFolder) {

        var slot = new DirectoryInfo(currentFolder);

        currentBackups.Clear();

        // Grabs all files in the directory
        foreach (var zip in slot.GetDirectories())
            if (zip.FullName.Contains("Server") || zip.FullName.Contains(islandName))
                currentBackups.Add(zip);

        // Create new list from currentBackups iff its included in BackupListInfo (from config file)
        //for (int i = 0; i < currentBackups.Count; i++) { TRTools.Log($"CurrentBackup Name List: {currentBackups[i].Name}"); }
        var filesFromMod = currentBackups.Where(i => backupList.Any(l => l.Contains(i.Name))).ToList();
        filesFromMod = filesFromMod.OrderBy(i => i.CreationTime).ToList();

        //foreach (var save in filesFromMod) { TRTools.LogError($"Backup List {save.FullName}"); }
        // Print out all lists to make sure all info is correct
        //for (var i = 0; i < currentBackups.Count; i++)
        //    TRTools.Log($"currentBackups List: {currentBackups[i].FullName}");
        //for (var i = 0; i < backupList.Count; i++) TRTools.Log($"Backup Info List: {backupList[i]}");
        //for (var i = 0; i < filesFromMod.Count; i++) TRTools.Log($"Temp List: {filesFromMod[i].FullName}");

        // If the final list is larger than the max backup count, delete the first created file and remove from backupList

        if (filesFromMod.Count > BackupCount.Value) {
            filesFromMod[0].Delete(true);
            backupList.Remove(filesFromMod[0].Name);
        }

        SaveBackupList();

        //for (int i = 0; i < filesFromMod.Count; i++) { TRTools.Log($"AFTER Temp List: {filesFromMod[i].FullName}"); }
        //for (int i = 0; i < backupList.Count; i++) { TRTools.Log($"AFTER Backup Info List: {backupList[i]}"); }

    }

    private static void SaveBackupList() {
        Data.SetValue("BackupList", backupList);
        Data.Save();
    }

    internal static void CopyToNewDirectory(string folderToCreate) {
        foreach (var dirPath in Directory.GetDirectories(savePath, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(dirPath.Replace(savePath, folderToCreate));

        //Copy all the files & Replaces any files with the same name
        foreach (var newPath in Directory.GetFiles(savePath, "*.*", SearchOption.AllDirectories))
            File.Copy(
                newPath, newPath.Replace(savePath, folderToCreate)
              , true
            );
    }

    internal static void CreateInitialBackup() {

        var date = WorldManager.Instance.getDateSave();
        if (date.year == 1 && date.day == 1 && date.week == 1 && date.month == 1) return;

        getOrCreateSavePaths();

        if (!Directory.Exists(globalSaveLocation)) Directory.CreateDirectory(globalSaveLocation);
        var currentGlobalSaves = Directory.GetFiles(globalSaveLocation);
        var currentGlobalSavesFolder = Directory.GetDirectories(globalSaveLocation);

        var allCurrentSaves = new List<string>();
        foreach (var file in currentGlobalSavesFolder) allCurrentSaves.Add(file);

        var initialBackupName = "PreAPISlot" + saveSlot;
        if (allCurrentSaves.Contains(Path.Combine(globalSaveLocation, initialBackupName))) return;

        try {
            TRTools.Log("Creating Initial Backup");
            CopyToNewDirectory(Path.Combine(globalSaveLocation, initialBackupName));
        }
        catch (Exception e) { TRTools.LogError($"IOException: {e}"); }
    }

    internal static void CreateBackup() {
        getOrCreateSavePaths();

        var date = WorldManager.Instance.getDateSave();
        var day = "0" + date.day;
        var year = date.year < 10 ? "0" + date.year : date.year.ToString();
        var season = date.month;
        var week = date.week;
        islandName = Inventory.Instance.islandName.Trim();
        var playerName = Inventory.Instance.playerName.Trim();
        var dateTime = DateTime.Now.ToString().Replace("/", "-").Replace(" ", "-").Replace(":", "-");
        var backupName = !clientInServer
                             ? $"{islandName}-{playerName}-Y{year}-S{season}-W{week}-D{day}-{dateTime}"
                             : $"Server-{playerName}-{string.Format("{0:yyyyMMdd'-'HHmmss}", DateTime.Now)}";

        try {
            CopyToNewDirectory(Path.Combine(saveDestinationSlot, backupName));
            backupList.Add($"{backupName}");
            SaveBackupList();
            if (TopNotification.Value)
                TRTools.TopNotification("Save Backup Manager", "The backup was created succesfully.");
        }
        catch (Exception e) {
            if (TopNotification.Value)
                TRTools.TopNotification("Save Backup Manager", "The backup failed due to an incorrect directory.");
            TRTools.LogError($"IOException: {e}");
            return;
        }
        if (BackupCount.Value != -1) {
            TRTools.Log("Attempting to remove a backup.");
            RemoveLastBackup(saveDestinationSlot);
        }

    }
}

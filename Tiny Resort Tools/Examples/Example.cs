using BepInEx;
using TinyResort;
using UnityEngine;

[BepInPlugin(pluginGuid, pluginName, pluginVersion)]
public class Example : BaseUnityPlugin {

    private static TRPlugin plugin;
    private static ModData modData;
    
    public const string pluginName = "YourPluginName";
    public const string pluginGuid = "your.plugin.guid";
    public const string pluginVersion = "1.0.0";

    private int exampleInteger;
    private string[] exampleArray;

    public static ModLicense exampleLicense;

    /*private void Awake() {
        
        #region General Modding Tools
        
        // Initializes your plugin with TRTools. Keeping a reference allows you to use certain features easily.
        // Ideally, you want to set the nexusID parameter to your mod's ID on nexus. This is the number at the end of your mod's URL. 
        // If the nexusID is set, then it will be used (in the future) to check if there is an update available to players.
        // The nexusID (if it is not -1) and a variable called "debugMode" will be added to your config file automatically.
        plugin = TRTools.Initialize(this, -1);

        // Logs a message to the console using your mod's logger, with options for severity.
        // The main purpose of this function is to have logs that a user can see if they enable a setting for debugging purposes.
        // By initializing with TRTools, a "debugMode" setting is automatically added to your config file.
        // This function ensures that the logged text is only shown if debugMode = true. You can, however,
        // set the debugModeOnly parameter to false if you want it to always show.
        plugin.Log("Example text for a log to the console.", LogSeverity.Warning, true);

        // This is a way to harmony patch methods with one quick line instead of having to write 3-4 lines each time you want to patch
        plugin.QuickPatch(typeof(CharMovement), "Start", typeof(Example), "StartPrefix", "StartPostfix");
        
        #endregion

        #region Custom Save Data
        
        // Tells the mod save data system to keep a save file specific to your plugin.
        // Automatically sets your mod data to be saved and loaded as long as you've called modData.SetValue() for the data you want saved
        modData = TRData.Subscribe(pluginGuid);

        // These events are called when the base game saves or loads.
        // You create your own functions that do what you want and subscribe them to these events
        // See "Examples of Save Data in Use" region for more info on each type of event and why you need them
        TRData.cleanDataEvent += RemoveDangerousVariables;
        TRData.preSaveEvent += SetVariables;
        TRData.postLoadEvent += GetVariables;
        TRData.injectDataEvent += ReintegrateDangerousVariables;

        // There are also post-saving and pre-loading events
        // TRData.postSaveEvent += ExampleSaveMethod;
        // TRData.preLoadEvent += ExampleLoadMethod;
        
        #endregion
        
        #region Custom Licenses
        
        // If your mod is adding a custom license, you want to add the license as below
        exampleLicense = TRLicenses.AddLicense(pluginGuid, 2, "Example License Name", UnityEngine.Color.red, 250, 2, 3, LicenceManager.LicenceTypes.Bridge);
        
        // After adding your custom license, you need to give it descriptions for each level it can be
        exampleLicense.SetDescription(1, "This is an example description for the first level of the custom license.");
        exampleLicense.SetDescription(2, "This is an example description for the second level of the custom license.");
        exampleLicense.SetDescription(3, "This is an example description for the third level of the custom license.");
        
        // Adds a requirement for the custom license so that the player must increase their level in a particular skill
        // in order to purchase the next license level.
        exampleLicense.ConnectToSkill(CharLevelManager.SkillTypes.Mining, 10);
        
        #endregion
        
        #region Drawing Tools
        
        // TODO: We plan to add tools importing and applying textures

        // Draws a circular sprite with the given radius, border thickness and colors
        TRDrawing.CircleSprite(100, Color.white, 3, Color.black);
        
        #endregion
        
        #region Useful Variables
        
        // Automatically updated so its easier to tell if you're in the main menu or not
        var IsInMainMenu = TRTools.InMainMenu;
        
        #endregion
        
        #region Useful Extensions
        
        // The CopyComponent method lets you create an identical copy of any component.
        var objToAddTo = new GameObject();
        var componentToCopy = GetComponent<UnityEngine.UI.Image>();
        componentToCopy.CopyComponent(objToAddTo);
        
        // The DeepCopy method lets you create an identical copy of any class, struct, or object
        var classToCopy = new Licence();
        var newCopy = classToCopy.DeepCopy();

        #endregion

    }*/
    
    #region Examples of Save Data in Use

    public void SetVariables() {

        // Sets a value to be saved to your mod's data file. You can do this at any point in your code,
        // but it will only be saved when the base game is saved (after all preSave events). So, you just need
        // to make sure your values are set before or during preSave events.
        modData.SetValue("exampleInt", exampleInteger);
        modData.SetValue("exampleArray", exampleArray);
        
    }

    // If your mod adds values to the base game that would normally be saved to the base game's save data
    // then you'll want to remove them from the base game save data before the game saves. This ensures that
    // if the player removes your mod that their save is not corrupted.
    public void RemoveDangerousVariables() { }

    public void GetVariables() {

        // When getting a value from your mod's save data, You need to cast the returned object to the approriate type.
        // Its also recommended to set the default value parameter to the appropriate default value for this variable type,
        // or simply to the variables current value if you are storing it separately. 
        exampleArray = (string[]) modData.GetValue("exampleArray", exampleArray);

        // If you choose not to set a default value, it will return null, so make sure to check if its null before using the value
        var exInt = modData.GetValue("exampleInt");
        if (exInt != null) { exampleInteger = (int) exInt; }
        
    }

    // If you previously removed dangerous variables from the game to avoid save corruption,
    // then you'd want to re-add them to the game here (during the onLoad and postSave events).
    public void ReintegrateDangerousVariables() {
        
    }
    
    #endregion

    public static void StartPrefix(CharMovement __instance) { }
    public static void StartPostfix(CharMovement __instance) { }
    
}

using BepInEx;
using BepInEx.Configuration;
using UnityEngine.SceneManagement;

namespace TinyResort;

[BepInPlugin(pluginGuid, pluginName, pluginVersion)]
internal class LeadPlugin : BaseUnityPlugin {

    public const string pluginName = "TRTools";
    public const string pluginGuid = "dev.TinyResort." + pluginName;
    public const string pluginVersion = "0.8.0";

    public static TRPlugin plugin;
    internal static LeadPlugin instance;

    internal static ConfigEntry<bool> developerMode;
    public static ConfigEntry<bool> useSlashToOpenChat;

    private void Awake() {

        instance = this;
        plugin = this.Initialize(83, "tr");
        plugin.harmony.PatchAll();

        useSlashToOpenChat = Config.Bind(
            "Chat", "UseSlashToOpenChat", true,
            "If true, then pressing forward slash on the keyboard will open the chat box with a slash already in place."
        );
        developerMode = Config.Bind(
            "Developer", "DeveloperMode", false,
            "If true, allows the use of nexusID being set to '-1'. Default is false, so you will need to update nexusID before releasing for the enduser."
        );

        // If in developer mode, give a big warning about nexus ID usage
        if (developerMode.Value) TRTools.LogError(TRTools.TRDeveloperMode());

        TRLicences.Initialize();
        TRItems.Initialize();
        TRQuickItems.LoadAllQuickItems();

        SceneManager.sceneLoaded += SceneLoaded;

    }

    internal void SceneLoaded(Scene scene, LoadSceneMode mode) {
        TRTools.LogError("LOADING SCENE " + scene.name);
    }

    private void Start() {

        TRInterface.Initialize();
        TRModUpdater.Initialize();
        TRConflictingPlugins.Initialize();
        TRItems.ManageAllItemArray();
        TRIcons.Initialize();
        TRBackup.Initialize();
        /*var TestLicense = plugin.AddLicence(1, "Test License 1",  10);
        TestLicense.SetColor(Color.cyan);
        TestLicense.SetLevelInfo(1, "Level 1: This is a license made for testing the framework.", 500);
        TestLicense.SetLevelInfo(2, "Level 2: This is a license made for testing the framework.", 1500);
        TestLicense.SetLevelInfo(3, "Level 3: This is a license made for testing the framework.", 2500);
        TestLicense.AddSkillRequirement(1, CharLevelManager.SkillTypes.Mining, 10);
        TestLicense.AddSkillRequirement(2, CharLevelManager.SkillTypes.Mining, 20);
        TestLicense.AddSkillRequirement(3, CharLevelManager.SkillTypes.Mining, 30);
        
        var TestLicense2 = plugin.AddLicence(2, "Test License 2", 2);
        TestLicense2.SetLevelInfo(1, "Level 1: This is a license made for testing the framework.", 500);
        TestLicense2.SetLevelInfo(2, "Level 2: This is a license made for testing the framework.", 1500);
        TestLicense2.SetLevelInfo(3, "Level 3: This is a license made for testing the framework.", 3000);
        TestLicense2.AddSkillRequirement(1, CharLevelManager.SkillTypes.Farming, 10);
        TestLicense2.AddSkillRequirement(2, CharLevelManager.SkillTypes.Farming, 20);
        TestLicense2.AddSkillRequirement(3, CharLevelManager.SkillTypes.Farming, 30);

        var TestLicense3 = plugin.AddLicence(3, "Test License 3", 3);
        TestLicense3.SetLevelInfo(1, "Level 1: This is a license made for testing the framework.", 1000);
        TestLicense3.SetLevelInfo(2, "Level 2: This is a license made for testing the framework.", 2000);
        TestLicense3.SetLevelInfo(3, "Level 3: This is a license made for testing the framework.", 5000);
        TestLicense3.AddSkillRequirement(1, CharLevelManager.SkillTypes.Hunting, 10);
        TestLicense3.AddSkillRequirement(2, CharLevelManager.SkillTypes.Farming, 20);
        TestLicense3.AddSkillRequirement(2, CharLevelManager.SkillTypes.Hunting, 40);
        TestLicense3.AddSkillRequirement(3, CharLevelManager.SkillTypes.Farming, 30);
        TestLicense3.AddPrerequisite(TestLicense);*/
    }

    private void Update() {

        TRModUpdater.Update();
        TRConflictingPlugins.Update();

        if (NetworkMapSharer.share.localChar) TRIcons.InitializeIcons();
        if (NetworkMapSharer.share.localChar && !TRItems.fixedRecipes) TRItems.FixRecipes();

        //if (Input.GetKeyDown(KeyCode.F11)) { TRItems.UnloadCustomItems(); }
        //if (Input.GetKeyDown(KeyCode.F12)) { TRItems.CurrentSaveInfo(); }
        //if (Input.GetKeyDown(KeyCode.F10)) { TRItems.LoadCustomItems(); }
        //if (Input.GetKeyDown(KeyCode.F9)) { 
        //     TRTools.Log($"Size: {SaveLoad.saveOrLoad.carryablePrefabs.Length}");
        //     NetworkMapSharer.share.spawnACarryable(SaveLoad.saveOrLoad.carryablePrefabs[10], NetworkMapSharer.share.localChar.transform.position, true);
        // }
    }
}

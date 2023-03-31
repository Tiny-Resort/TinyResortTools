using TMPro;
using UnityEngine;

namespace TinyResort;

public class GPInterface {

    private static TRButton ClientManagementButton;
    private static GameObject modsWindow;

    private static GameObject creditsWindow;
    private static RectTransform updateButtonGrid;

    private static TRButton updateButton;

    private static float scrollPosition;
    private static float scrollMaxPosition;

    public static void CreateUI() {

        creditsWindow = null;

        // Create mod update checker button
        var ClientManagementWindow = OptionsMenu.options.menuParent.transform.GetChild(10);
        ClientManagementButton = TRInterface.CreateButton(ButtonTypes.MainMenu, ClientManagementWindow, "MODS", ToggleClientManagementWindow);
        ClientManagementButton.rectTransform.sizeDelta = new Vector2(146, 44);
        ClientManagementButton.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        ClientManagementButton.rectTransform.anchorMax = new Vector2(0f, 0.5f);
        ClientManagementButton.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        ClientManagementButton.rectTransform.anchoredPosition = new Vector2(-1115, -5f);
        ClientManagementButton.textMesh.alignment = TextAlignmentOptions.Center;
        ClientManagementButton.textMesh.fontSize = 18;

        /*// Create an update button to work with
        updateButton = TRInterface.CreateButton(ButtonTypes.MainMenu, null, "");
        updateButton.windowAnim.openDelay = 0;
        Object.Destroy(updateButton.buttonAnim);
        updateButton.textMesh.fontSize = 12;
        updateButton.textMesh.rectTransform.sizeDelta = new Vector2(500, 50);
        updateButton.textMesh.lineSpacing = -20;*/

    }

    internal static void ToggleClientManagementWindow() => modsWindow.gameObject.SetActive(!modsWindow.gameObject.activeSelf);

    //PopulateModList();
}

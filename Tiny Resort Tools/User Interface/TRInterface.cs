using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TMPro;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace TinyResort; 

public class TRInterface : MonoBehaviour {

    internal static AssetBundle UIBundle;
    internal static Sprite ModLogo;
    internal static TRButton buttonMainMenu;
    internal static GameObject scrollBar;

    internal static void Initialize() {

        // Load Button Asset Bundles
        UIBundle = TRAssets.LoadAssetBundleFromDLL("ui_elements");
        foreach (var asset in UIBundle.LoadAllAssets()) {
            //TRTools.Log($"Asset: {asset.name}");
        }
        ModLogo = UIBundle.LoadAsset<Sprite>("mod_logo");
        buttonMainMenu = LoadButton("Main Menu Button");

        //var newScrollbar = GameObject.Instantiate()

    }

    /// <summary>
    /// Create a button using pre-determined assets in the API.
    /// </summary>
    /// <param name="type">The only current option is MainMenu. This looks like the main buttons on the screen (i.e. Main Menu).</param>
    /// <param name="parent">Put the transform of what you want to attach the new button too.</param>
    /// <param name="text">The text on top of the button.</param>
    /// <param name="clickAction">The method you want to run when clicking the button.</param>
    /// <returns></returns>
    public static TRButton CreateButton(ButtonTypes type, Transform parent, string text, UnityAction clickAction = null) {

        TRButton newButton = null;
        switch (type) {
            case ButtonTypes.MainMenu:
                newButton = buttonMainMenu.Copy(parent, text, clickAction);
                break;
        }

        return newButton;

    }

    // Creates a prefab for a specific button type
    private static TRButton LoadButton(string name) {
        var newObject = Instantiate(UIBundle.LoadAsset<GameObject>(name));
        var newButton = newObject.AddComponent<TRButton>();
        newButton.background = newObject.GetComponentInChildren<Image>();
        newButton.button = newObject.GetComponentInChildren<InvButton>();
        newButton.rectTransform = newObject.GetComponent<RectTransform>();
        newButton.textMesh = newObject.GetComponentInChildren<TextMeshProUGUI>();
        newButton.windowAnim = newObject.GetComponentInChildren<WindowAnimator>();
        newButton.buttonAnim = newObject.GetComponentInChildren<ButtonAnimation>();
        return newButton;
    }

    /// <summary> Creates a sprite in the shape of a circle with a border. </summary>
    /// <param name="pixelRadius">The radius of the circle (in pixels).</param>
    /// <param name="mainColor">The color of the circle.</param>
    /// <param name="borderThickness">Thickness of the border to the circle (in pixels). Set to 0 to have no border.</param>
    /// <param name="borderColor">Color of the border.</param>
    public static Sprite DrawCircle(int pixelRadius, Color mainColor, float borderThickness, Color borderColor) {

        var tex = new Texture2D(pixelRadius * 2, pixelRadius * 2, TextureFormat.RGBA32, false);

        for (var u = 0; u < 2 * pixelRadius + 1; u++) {
            for (var v = 0; v < 2 * pixelRadius + 1; v++) {
                var pos = Mathf.Pow(pixelRadius - u, 2) + Mathf.Pow(pixelRadius - v, 2);
                if (pos < Mathf.Pow(pixelRadius, 2))
                    tex.SetPixel(u, v, pos < Mathf.Pow(pixelRadius - borderThickness, 2) ? mainColor : borderColor);
                else
                    tex.SetPixel(u, v, Color.clear);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(Vector2.zero, Vector2.one * 2 * pixelRadius), Vector2.one * 0.5f);

    }

}

public class TRButton : MonoBehaviour {

    public RectTransform rectTransform;
    public InvButton button;
    public Image background;
    public TextMeshProUGUI textMesh;
    public ButtonAnimation buttonAnim;
    public WindowAnimator windowAnim;

    public TRButton Copy(Transform parent, string text, UnityAction clickAction = null) {
        var copy = Instantiate(gameObject, parent);
        var buttonInfo = copy.GetComponent<TRButton>();
        buttonInfo.textMesh.text = text;
        if (clickAction != null) {
            buttonInfo.button.onButtonPress.RemoveAllListeners();
            buttonInfo.button.onButtonPress.AddListener(clickAction);
        }
        return buttonInfo;
    }

}

public enum ButtonTypes { MainMenu }

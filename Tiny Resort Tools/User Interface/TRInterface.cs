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

namespace TinyResort {

    public class TRInterface : MonoBehaviour {

        internal static AssetBundle UIBundle;
        internal static Sprite ModLogo;
        internal static TRButton buttonMainMenu;
        internal static GameObject scrollBar;

        internal static void Initialize() {

            // Load Button Asset Bundles
            UIBundle = TRAssets.LoadAssetBundleFromDLL("ui_elements");
            ModLogo = UIBundle.LoadAsset<Sprite>("mod_logo");
            buttonMainMenu = LoadButton("Main Menu Button");

            //var newScrollbar = GameObject.Instantiate()

        }

        public static TRButton CreateButton(ButtonTypes type, Transform parent, string text, UnityAction clickAction = null) {  

            TRButton newButton = null;
            switch (type) {
                case ButtonTypes.MainMenu: newButton = buttonMainMenu.Copy(parent, text, clickAction); break; 
            }

            return newButton;

        }

        // Creates a prefab for a specific button type
        private static TRButton LoadButton(string name) {
            TRTools.Log($"Before ui elements");
            //var newObject = Object.Instantiate(TRAssets.LoadBundle(Path.Combine("custom_assets", "user_interface", "ui_elements")).LoadAsset<GameObject>(name));
            var newObject = Object.Instantiate(UIBundle.LoadAsset<GameObject>(name));
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

            Texture2D tex = new Texture2D(pixelRadius * 2, pixelRadius * 2, TextureFormat.RGBA32, false);

            for (int u = 0; u < 2 * pixelRadius + 1; u++) {
                for (int v = 0; v < 2 * pixelRadius + 1; v++) {
                    var pos = Mathf.Pow(pixelRadius - u, 2) + Mathf.Pow(pixelRadius - v, 2);
                    if (pos < Mathf.Pow(pixelRadius, 2)) { tex.SetPixel(u, v, pos < Mathf.Pow(pixelRadius - borderThickness, 2) ? mainColor : borderColor); }
                    else { tex.SetPixel(u, v, Color.clear); }
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
            var copy = Object.Instantiate(gameObject, parent);
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

}

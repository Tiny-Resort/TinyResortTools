using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace TinyResort {

    /// <summary> Tools for drawing (WORK IN PROGRESS)</summary>
    public class TRInterface : MonoBehaviour {

        internal static Dictionary<string, GameObject> currentObjects = new Dictionary<string, GameObject>();

        /// <summary>Searches for and instantiates an object while storing it in a Dictionary to avoid duplication.</summary>
        /// <param name="location">Location of the GameObject you would like to instantiate, i.e. "MapCanvas/Menu".</param>
        /// <param name="parentObject">Parent object you would like to attach your button too.</param>
        public static GameObject InstantiateObject(string location, GameObject parentObject) {
            var toReturn = Instantiate(currentObjects[location], parentObject.transform);
            return toReturn;
        }

        /// <summary>Returns the game object at the specified location from a pre-prepared dictionary.</summary>
        /// <param name="location">Location of the GameObject you would like to return, i.e. "MapCanvas/Menu".</param>
        public static GameObject GetObject(string location) {
            return currentObjects[location];
        }
        
        /// <summary>Creates a button based on a game object and attached it to a parent object.</summary>
        /// <param name="Location">Location of the GameObject you would like to instantiate, i.e. "MapCanvas/Menu"</param>
        /// <param name="parentObject">Parent object you would like to attach your button too.</param>
        /// <param name="Text">The text you would like the button to show.</param>
        /// <param name="fontSize">Font Size of the button's text.</param>
        /// <param name="method">The method you would like to have run when the button is pressed.</param>
        public static GameObject CreateButton(string Location, GameObject parentObject, string Text, int fontSize, UnityAction method) {
            GameObject GO = Instantiate(currentObjects[Location], parentObject.transform);
            GO.name = Text;
            var GOText = GO.transform.GetComponentInChildren<TextMeshProUGUI>();
            GOText.text = Text;
            GOText.fontSize = fontSize;
            /*GOText.outlineWidth = .42f;
            GOText.outlineColor = new Color32(0, 0, 0, 101); */

            var buttonExists = GO.GetComponent<InvButton>();
            if (buttonExists) {
                buttonExists.onButtonPress = new UnityEvent();
                buttonExists.onButtonPress.AddListener(method);

                buttonExists.isACloseButton = false;
                buttonExists.isSnappable = true;
            }

            GO.GetComponent<WindowAnimator>().openDelay = 0f;
            
            var GOIm = GO.GetComponent<Image>();
            GOIm.color = new Color(.502f, .356f, .235f);
            
            return GO;
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
        
        internal static void InitializeAllObjects() {
            Type transformType = typeof(Transform);
            Transform[] toFind = (Transform[])Resources.FindObjectsOfTypeAll(transformType);

            foreach (var trans in toFind) {
                string path = trans.gameObject.name;
                if (trans.parent != null) {
                    Transform parent = trans.parent;
                    while (parent != null) {
                        path = parent.gameObject.name + "/" + path;
                        parent = parent.parent;
                    }
                    currentObjects[path] = trans.gameObject;
                }
            }
        }
        
    }

}

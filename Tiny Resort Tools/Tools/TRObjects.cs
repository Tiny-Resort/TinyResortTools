using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TinyResort;

internal class TRObjects {

    internal static Dictionary<string, GameObject> currentObjects = new();

    public static void Initialize() {

        var transformType = typeof(Transform);
        var toFind = (Transform[])Resources.FindObjectsOfTypeAll(transformType);

        foreach (var trans in toFind) {
            var path = trans.gameObject.name;
            if (trans.parent != null) {
                var parent = trans.parent;
                while (parent != null) {
                    path = parent.gameObject.name + "/" + path;
                    parent = parent.parent;
                }
                currentObjects[path] = trans.gameObject;
            }
        }
    }

    /// <summary>Searches for and instantiates an object while storing it in a Dictionary to avoid duplication.</summary>
    /// <param name="location">Location of the GameObject you would like to instantiate, i.e. "MapCanvas/Menu".</param>
    /// <param name="parentObject">Parent object you would like to attach your button too.</param>
    public static GameObject InstantiateObject(string location, GameObject parentObject) {
        var toReturn = Object.Instantiate(currentObjects[location], parentObject.transform);
        return toReturn;
    }

    /// <summary>Returns the game object at the specified location from a pre-prepared dictionary.</summary>
    /// <param name="location">Location of the GameObject you would like to return, i.e. "MapCanvas/Menu".</param>
    public static GameObject GetObject(string location) => currentObjects[location];

    /// <summary>Creates a button based on a game object and attached it to a parent object.</summary>
    /// <param name="Location">Location of the GameObject you would like to instantiate, i.e. "MapCanvas/Menu"</param>
    /// <param name="parentObject">Parent object you would like to attach your button too.</param>
    /// <param name="Text">The text you would like the button to show.</param>
    /// <param name="fontSize">Font Size of the button's text.</param>
    /// <param name="method">The method you would like to have run when the button is pressed.</param>
    public static GameObject CreateButton(string Location, GameObject parentObject, string Text, int fontSize, UnityAction method) {
        var GO = Object.Instantiate(currentObjects[Location], parentObject.transform);
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
}

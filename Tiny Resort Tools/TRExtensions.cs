using UnityEngine;

namespace TinyResort {

    public static class TRExtensions {

        /// <summary>Allows you to copy any class or data structure by value rather than reference in order to change things about a copy without changing the original.</summary>
        /// <param name="objToCopy">The class/object to copy.</param>
        /// <returns>The new copy of the class/object.</returns>
        public static T DeepCopy<T>(this T objToCopy) where T : class => JsonUtility.FromJson<T>(JsonUtility.ToJson(objToCopy));

        /// <summary>Creates an exact copy of a component and adds it to the gameobject.</summary>
        /// <param name="originalComponent">The component to copy.</param>
        /// <param name="gameObject">The gameobject you want to add the copy to.</param>
        /// <returns>A reference to the new copy.</returns>
        public static T CopyComponent<T>(this T originalComponent, GameObject gameObject) where T : Component {
            var type = originalComponent.GetType();
            var copy = gameObject.AddComponent(type);
            var fields = type.GetFields();
            foreach (var field in fields) { field.SetValue(copy, field.GetValue(originalComponent)); }
            return copy as T;
        }
        
    }

}

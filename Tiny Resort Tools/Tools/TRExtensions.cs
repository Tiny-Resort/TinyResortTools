using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TinyResort
{
    /// <summary>Just a few useful methods.</summary>
    public static class TRExtensions {

        /// <summary>
        ///     Allows you to copy any class or data structure by value rather than reference in order to change things about
        ///     a copy without changing the original.
        /// </summary>
        /// <param name="objToCopy">The class/object to copy.</param>
        /// <returns>The new copy of the class/object.</returns>
        public static T DeepCopy<T>(this T objToCopy) where T : class =>
            JsonUtility.FromJson<T>(JsonUtility.ToJson(objToCopy));

        /// <summary>Creates an exact copy of a component and adds it to the gameobject.</summary>
        /// <param name="originalComponent">The component to copy.</param>
        /// <param name="gameObject">The gameobject you want to add the copy to.</param>
        /// <returns>A reference to the new copy.</returns>
        public static T CopyComponent<T>(this T originalComponent, GameObject gameObject) where T : Component {
            var type = originalComponent.GetType();
            var copy = gameObject.AddComponent(type);
            var fields = type.GetFields();
            foreach (var field in fields) field.SetValue(copy, field.GetValue(originalComponent));
            return copy as T;
        }

        /// <summary>Lets you compare a base string with a list of other strings and finds the closest matches.</summary>
        /// <param name="baseString">The string you want to compare your list against.</param>
        /// <param name="stringsToCompare">A list of strings to see which ones are the closest.</param>
        /// <returns>A list of strings holding all of the strings with the minimum required of steps.</returns>
        internal static List<string> CompareListOfStrings(this string baseString, List<string> stringsToCompare) {

            var results = new Dictionary<string, int>();

            foreach (var stringToTest in stringsToCompare)
                results.Add(stringToTest, ComputeDistance(baseString, stringToTest));

            var minimumModifications = results.Min(j => j.Value);
            var AllMinimumRequired = results.Where(
                i => i.Value == minimumModifications && minimumModifications <= baseString.Length / 1.5f
            );
            var list = new List<string>();
            foreach (var compared in AllMinimumRequired) list.Add(compared.Key);
            return list;
        }

        internal static int ComputeDistance(string baseString, string stringToCompare) {
            var bsLength = baseString.Length;
            var stcLength = stringToCompare.Length;
            var d = new int[bsLength + 1, stcLength + 1];

            // Nothing to compare if either string is empty. 
            if (bsLength == 0) return stcLength;
            if (stcLength == 0) return bsLength;

            // Set each element to the next number
            for (var i = 0; i <= bsLength; d[i, 0] = i++) { }
            for (var j = 0; j <= stcLength; d[0, j] = j++) { }
            for (var i = 1; i <= bsLength; i++) {
                for (var j = 1; j <= stcLength; j++) {

                    // If elements match, 0. If don't match 1. 
                    var cost = stringToCompare[j - 1] == baseString[i - 1] ? 0 : 1;

                    // Seems to set current i/j to the minimum value, so the length will always be the minimum at the end. 
                    // Not sure how this is actually the minimum though. 
                    // Reference for how it works: https://www.codeproject.com/Articles/13525/Fast-memory-efficient-Levenshtein-algorithm-2
                    d[i, j] = Math.Min(
                        Math.Min(
                            d[i - 1, j] + 1,
                            d[i, j - 1] + 1
                        ),
                        d[i - 1, j - 1] + cost
                    );
                }
            }

            return d[bsLength, stcLength];
        }
    }
}

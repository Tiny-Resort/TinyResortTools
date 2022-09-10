using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TinyResort {

    public class TRAssets {

        /// <summary>Loads an image file from the plugins folder as a texture.</summary>
        /// <param name="relativePath">Path to the image file, relative to the BepInEx plugins folder.</param>
        /// <returns>The loaded texture.</returns>
        public static Texture2D ImportTexture(string relativePath) {

            var path = Path.Combine(BepInEx.Paths.PluginPath, relativePath);

            if (!File.Exists(path)) {
                TRTools.Log("No file found at " + path, LogSeverity.Error);
                return null;
            }
            
            byte[] fileData = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
            
            return tex;
            
        }

        /// <summary>Loads an image file from the plugins folder as a Sprite.</summary>
        /// <param name="relativePath">Path to the image file, relative to the BepInEx plugins folder.</param>
        /// <param name="pivot">The center of the sprite. Vector2.zero would be the top left and Vector2.one would be the bottom right.</param>
        /// <returns>The loaded Sprite.</returns>
        public static Sprite ImportSprite(string relativePath, Vector2 pivot) {
            Texture2D tex = ImportTexture(relativePath);
            Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), pivot, 100);
            return newSprite;
        }
        
    }

}

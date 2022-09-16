using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace TinyResort {

    /// <summary>Tools for importing custom assets.</summary>
    public class TRAssets {
        
        #region Scanning Folders
        
        internal static string[] textureFormats = new []{ "bmp", "exr", "gif", "hdr", "iff", "jpg", "pict", "png", "psd", "tga", "tiff" };
        internal static string[] audioFormats = new []{ "aif", "wav", "mp3", "ogg" };

        /// <summary>Returns a list of all files in a folder that can be imported as a texture (or sprite).</summary>
        /// <param name="relativePath">Path to the folder that you want to be scanned, relative to the BepInEx plugins folder.</param>
        public static List<string> ListAllTextures(string relativePath) => ListAllFiles(relativePath, textureFormats);

        /*/// <summary>Returns a list of all files in a folder that can be imported as an AudioClip.</summary>
        /// <param name="folderPath">Path to the folder that you want to be scanned, relative to the BepInEx plugins folder.</param>
        public static List<string> ListAllAudioClips(string folderPath) => ListAllFiles(folderPath, audioFormats);*/

        /// <summary>Returns a list of all files in a folder that have one of the specified extensions.</summary>
        /// <param name="relativePath">Path to the folder that you want to be scanned, relative to the BepInEx plugins folder.</param>
        /// <param name="validExtensions">Any number of file extensions (without the dot).</param>
        public static List<string> ListAllFiles(string relativePath, params string[] validExtensions) {

            var path = Path.Combine(BepInEx.Paths.PluginPath, relativePath);
            var list = new List<string>();

            if (Directory.Exists(path)) {
                var files = Directory.GetFiles(path);
                foreach (var file in files) {
                    var ext = Path.GetExtension(file).Remove(0, 1).ToLower();
                    if (validExtensions.Any(i => i == ext)) { list.Add(file); }
                }
            }

            else { TRTools.Log("Trying to list files in a folder, but the folder '" + path + "' does not exist.", LogSeverity.Error, false); }

            return list;
            
        }
        
        #endregion
        
        #region Importing

        /// <summary>Loads an image file from the plugins folder as a texture.</summary>
        /// <param name="relativePath">Path to the image file, relative to the BepInEx plugins folder.</param>
        /// <returns>The loaded texture.</returns>
        public static Texture2D ImportTexture(string relativePath) {

            var path = Path.Combine(BepInEx.Paths.PluginPath, relativePath);
            if (!File.Exists(path)) {
                TRTools.Log("No file found at " + path, LogSeverity.Error, false);
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

        /*/// <summary>Loads an audio file from the plugins folder as an AudioClip.</summary>
        /// <param name="relativePath">Path to the image file, relative to the BepInEx plugins folder.</param>
        /// <returns>An AudioClipLoader. The audio clip takes a second to load, so you need to yield until the loader's finishedLoading variable is true, then you can access the AudioClip by getting the loader's clip variable.</returns>
        public static AudioClipLoader ImportAudioClip(string relativePath) {

            var loader = new AudioClipLoader();
            
            var path = Path.Combine(BepInEx.Paths.PluginPath, relativePath);
            if (!File.Exists(path)) {
                TRTools.Log("No file found at " + path, LogSeverity.Error, false);
                return loader;
            }

            var ext = Path.GetExtension(path);
            AudioType type;
            switch (ext) {
                case "aif": type = AudioType.AIFF; break;
                case "wav": type = AudioType.WAV; break;
                case "mp3": type = AudioType.MPEG; break;
                case "ogg": type = AudioType.OGGVORBIS; break;
                default: 
                    TRTools.Log("Trying to import audio clip with unsupported file format from file path: " + path, LogSeverity.Error, false);
                    return loader;
            }

            LeadPlugin.instance.StartCoroutine(ImportAudioClip(path, type, loader));
            return loader;

        }

        private static IEnumerator ImportAudioClip(string path, AudioType type, AudioClipLoader loader) {
            
            var dh = new DownloadHandlerAudioClip($"file://{path}", type);
            dh.compressed = true;

            using (UnityWebRequest wr = new UnityWebRequest($"file://{path}", "GET", dh, null)) {
                yield return wr.SendWebRequest();
                if (wr.responseCode == 200) {
                    loader.clip = dh.audioClip;
                    loader.finishedLoading = true;
                }
            }
            
        }*/
        
        #endregion
        
    }

    /*public class AudioClipLoader {
        public AudioClip clip;
        public bool finishedLoading;
    }*/

}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using UnityEngine;
using UnityEngine.Networking;

namespace TinyResort {

    /// <summary>Tools for importing custom assets.</summary>
    public class TRAssets {

        #region Scanning Folders

        internal static string[] textureFormats = new[] {
            "bmp",
            "exr",
            "gif",
            "hdr",
            "iff",
            "jpg",
            "pict",
            "png",
            "psd",
            "tga",
            "tiff"
        };
        internal static string[] audioFormats = new[] { "aif", "wav", "mp3", "ogg" };

        internal static bool checkFileSignature(string file) {
            bool isSafe = false;

            using (BinaryReader binary_reader = new BinaryReader(File.Open(file, FileMode.Open))) {
                byte[] data = binary_reader.ReadBytes(0x10);
                string data_as_hex = BitConverter.ToString(data).Trim();
                if (!data_as_hex.IsNullOrWhiteSpace()) {
                    var startsWith = data_as_hex.Substring(0, 23);
                    if (startsWith.StartsWith("89-50-4E-47-0D-0A-1A-0A")) isSafe = true; // PNG
                    if (startsWith.StartsWith("42-4D")) isSafe = true; // BMP
                    if (startsWith.StartsWith("76-2F-31-01")) isSafe = true; // EXR
                    if (startsWith.StartsWith("47-49-46-38-37-61")) isSafe = true; // GIF
                    if (startsWith.StartsWith("47-49-46-38-39-61")) isSafe = true; // GIF
                    if (startsWith.StartsWith("46-4F-52-4D")) isSafe = true; // IFF
                    if (startsWith.StartsWith("FF-D8-FF-E0")) isSafe = true; // JPG
                    if (startsWith.StartsWith("78-56-34")) isSafe = true; // PICT
                    if (startsWith.StartsWith("38-42-50-53")) isSafe = true; // PSD
                    if (startsWith.StartsWith("49-49-2A-00")) isSafe = true; // TIFF
                    if (startsWith.StartsWith("4D-4D-00-2A")) isSafe = true; // TIFF
                }
            }
            return isSafe;
        }

        internal static AssetBundle LoadAssetBundleFromDLL(string name) {
            Assembly CurrentAssembly = Assembly.GetExecutingAssembly();
            List<string> CurrentAssemblyResourcePaths = CurrentAssembly.GetManifestResourceNames().ToList();
            string TargetResourcePath = CurrentAssemblyResourcePaths.Find(CurrentResource => CurrentResource.Contains(name));
            return AssetBundle.LoadFromStream(CurrentAssembly.GetManifestResourceStream(TargetResourcePath));
        }

        /// <summary>Returns a list of all files in a folder that can be imported as a texture (or sprite). Includes subdirectories.</summary>
        /// <param name="relativePath">Path to the folder that you want to be scanned, relative to the BepInEx plugins folder.</param>
        public static List<string> ListAllTextures(string relativePath) => ListAllFiles(relativePath, textureFormats);

        /*/// <summary>Returns a list of all files in a folder that can be imported as an AudioClip.</summary>
        /// <param name="folderPath">Path to the folder that you want to be scanned, relative to the BepInEx plugins folder.</param>
        public static List<string> ListAllAudioClips(string folderPath) => ListAllFiles(folderPath, audioFormats);*/

        /// <summary>Returns a list of all files in a folder that have one of the specified extensions. Includes subdirectories.</summary>
        /// <param name="relativePath">Path to the folder that you want to be scanned, relative to the BepInEx plugins folder.</param>
        /// <param name="validExtensions">Any number of file extensions (without the dot).</param>
        public static List<string> ListAllFiles(string relativePath, params string[] validExtensions) {

            var path = Path.Combine(Paths.PluginPath, relativePath);
            var list = new List<string>();

            if (Directory.Exists(path)) {
                var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                foreach (var file in files) {
                    var ext = Path.GetExtension(file).Remove(0, 1).ToLower();
                    if (validExtensions.Any(i => i == ext)) { list.Add(file); }
                }
            }

            else { TRTools.LogError("Trying to list files in a folder, but the folder '" + path + "' does not exist."); }

            return list;

        }

        #endregion

        #region Importing

        /// <summary>Loads an asset bundle from the plugins folder.</summary>
        /// <param name="relativePath">Path to the asset bundle, relative to the BepInEx plugins folder.</param>
        /// <returns>The loaded asset bundle.</returns>
        public static AssetBundle LoadBundle(string relativePath) {

            var path = Path.Combine(BepInEx.Paths.PluginPath, relativePath);
            if (!File.Exists(path)) {
                TRTools.LogError("No file found at " + path);
                return null;
            }

            var bundle = AssetBundle.LoadFromFile(path);
            if (bundle == null) {
                TRTools.LogError("Failed to load asset bundle from " + path);
                return null;
            }

            return bundle;

        }

        /// <summary>Loads an image file from the plugins folder as a texture.</summary>
        /// <param name="relativePath">Path to the image file, relative to the BepInEx plugins folder.</param>
        /// <returns>The loaded texture.</returns>
        public static Texture2D LoadTexture(string relativePath) {

            var path = Path.Combine(BepInEx.Paths.PluginPath, relativePath);
            if (!File.Exists(path)) {
                TRTools.LogError("No file found at " + path);
                return null;
            }

            if (checkFileSignature(path)) {
                byte[] fileData = File.ReadAllBytes(path);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(fileData);
                return tex;
            }

            TRTools.LogError("File type is incorrect at " + path);
            return null;
        }

        /// <summary>Loads an image file from the plugins folder as a Sprite.</summary>
        /// <param name="relativePath">Path to the image file, relative to the BepInEx plugins folder.</param>
        /// <param name="pivot">The center of the sprite. Vector2.zero would be the top left and Vector2.one would be the bottom right.</param>
        /// <returns>The loaded Sprite.</returns>
        public static Sprite LoadSprite(string relativePath, Vector2 pivot) {
            Texture2D tex = LoadTexture(relativePath);
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

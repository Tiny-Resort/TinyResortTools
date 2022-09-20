using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class CreateAssetBundles : MonoBehaviour
{
    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles() {
        string assetBundleDirectory = "Assets/_MODDING/Bundles";
        if (!Directory.Exists(assetBundleDirectory)) { Directory.CreateDirectory(assetBundleDirectory); }
        BuildPipeline.BuildAssetBundles(
            assetBundleDirectory,
            BuildAssetBundleOptions.None,
            BuildTarget.StandaloneWindows
        );
    }
}

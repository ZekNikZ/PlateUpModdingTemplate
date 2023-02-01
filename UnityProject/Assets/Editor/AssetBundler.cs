using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class AssetBundler
{
    /// <summary>
    /// The types of assets to search for in checks.
    /// </summary>
    private static readonly string ASSET_SEARCH_QUERY = "t:prefab,t:textAsset";

    /// <summary>
    /// Temporary location for building AssetBundles.
    /// </summary>
    private static readonly string TEMP_BUILD_FOLDER = "Temp/AssetBundles";

    /// <summary>
    /// Name of the output bundle file. This needs to match the bundle that you tag your assets with.
    /// </summary>
    private static readonly string BUNDLE_FILENAME = "mod.assets";

    /// <summary>
    /// The output folder to place the completed bundle in.
    /// </summary>
    private static readonly string OUTPUT_FOLDER = "../content";

    /// <summary>
    /// The folders to not search for assets in.
    /// </summary>
    private static readonly string[] EXCLUDED_FOLDERS = new string[] { "Assets/Editor", "Packages" };

    /// <summary>
    /// The build target of the asset bundle. Should either be StandaloneWindows or StandaloneOSX, depending on your platform.
    /// </summary>
    private BuildTarget TARGET = BuildTarget.StandaloneWindows;

    [MenuItem("PlateUp!/Build Asset Bundle")]
    public static void BuildAssetBundle()
    {
        Debug.LogFormat("Creating \"{0}\" AssetBundle...", BUNDLE_FILENAME);

        AssetBundler bundler = new AssetBundler();

        if (Application.platform == RuntimePlatform.OSXEditor) bundler.TARGET = BuildTarget.StandaloneOSX;

        bool success = false;
        try
        {
            // Check for assets
            bundler.WarnIfAssetsAreNotTagged();
            bundler.WarnIfZeroAssetsAreTagged();

            // Delete the contents of OUTPUT_FOLDER
            bundler.CleanBuildFolder();

            // Lastly, create the asset bundle itself and copy it to the output folder
            bundler.CreateAssetBundle();

            success = true;
        }
        catch (Exception e)
        {
            Debug.LogErrorFormat("Failed to build AssetBundle: {0}\n{1}", e.Message, e.StackTrace);
        }

        if (success)
        {
            Debug.LogFormat("{0} Build complete! Output: {1}", DateTime.Now.ToLocalTime(), OUTPUT_FOLDER + "/" + BUNDLE_FILENAME);
        }
    }

    /// <summary>
    /// Delete and recreate the OUTPUT_FOLDER to ensure a clean build.
    /// </summary>
    protected void CleanBuildFolder()
    {
        Debug.LogFormat("Cleaning {0}...", OUTPUT_FOLDER);

        if (Directory.Exists(OUTPUT_FOLDER))
        {
            Directory.Delete(OUTPUT_FOLDER, true);
        }

        Directory.CreateDirectory(OUTPUT_FOLDER);
    }

    /// <summary>
    /// Build the AssetBundle itself and copy it to the OUTPUT_FOLDER.
    /// </summary>
    protected void CreateAssetBundle()
    {
        Debug.Log("Building AssetBundle...");

        // Build all AssetBundles to the TEMP_BUILD_FOLDER
        if (!Directory.Exists(TEMP_BUILD_FOLDER))
        {
            Directory.CreateDirectory(TEMP_BUILD_FOLDER);
        }

#pragma warning disable 618
        // Build the asset bundle with the CollectDependencies flag. This is necessary or else ScriptableObjects will
        // not be accessible within the asset bundle. Unity has deprecated this flag claiming it is now always active,
        // but due to a bug we must still include it (and ignore the warning).
        BuildPipeline.BuildAssetBundles(
            TEMP_BUILD_FOLDER,
            BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.CollectDependencies,
            TARGET);
#pragma warning restore 618

        // We are only interested in the BUNDLE_FILENAME bundle (and not any extra AssetBundle or the manifest files
        // that Unity makes), so just copy that to the final output folder
        string srcPath = Path.Combine(TEMP_BUILD_FOLDER, BUNDLE_FILENAME);
        string destPath = Path.Combine(OUTPUT_FOLDER, BUNDLE_FILENAME);
        File.Copy(srcPath, destPath, true);
    }

    /// <summary>
    /// Checks if the given path is a search path.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>true if the given path is a search path, otherwise false.</returns>
    protected static bool IsIncludedAssetPath(string path)
    {
        foreach (string excludedPath in EXCLUDED_FOLDERS)
        {
            if (path.StartsWith(excludedPath))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Log a warning for all potential assets that are not currently tagged to be in this AssetBundle.
    /// </summary>
    protected void WarnIfAssetsAreNotTagged()
    {
        string[] assetGUIDs = AssetDatabase.FindAssets(ASSET_SEARCH_QUERY);
        foreach (var assetGUID in assetGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(assetGUID);
            if (!IsIncludedAssetPath(path))
            {
                continue;
            }

            var importer = AssetImporter.GetAtPath(path);
            if (!importer.assetBundleName.Equals(BUNDLE_FILENAME))
            {
                Debug.LogWarningFormat("Asset \"{0}\" is not tagged for {1} and will not be included in the AssetBundle!", path, BUNDLE_FILENAME);
            }
        }
    }

    /// <summary>
    /// Verify that there is at least one asset to be included in the asset bundle.
    /// </summary>
    protected void WarnIfZeroAssetsAreTagged()
    {
        string[] assetsInBundle = AssetDatabase.FindAssets($"{ASSET_SEARCH_QUERY},b:{BUNDLE_FILENAME}");
        if (assetsInBundle.Length == 0)
        {
            throw new Exception(string.Format("No assets have been tagged for inclusion in the {0} AssetBundle.", BUNDLE_FILENAME));
        }
    }
}

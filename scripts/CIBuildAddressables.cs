using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using ThunderRoad;

/// <summary>
/// CI entry points for building addressable asset bundles in batch mode.
/// Injected into BasSDK/Assets/Editor/ at CI time by the shared workflow.
///
/// Only AssetBundleGroup assets found under "Assets/Personal/" are selected —
/// this excludes BasSDK's own groups and the Proto example bundle.
/// </summary>
public static class CIBuildAddressables
{
    public static void BuildWindows()
    {
        Debug.Log("[CI] Starting Windows addressable build...");
        SDKTools.SetWindowsQualityAndPlatform();
        RunBuild();
    }

    public static void BuildAndroid()
    {
        Debug.Log("[CI] Starting Android addressable build...");
        SDKTools.SetAndroidQualityAndPlatform();
        RunBuild();
    }

    private static void RunBuild()
    {
        AssetDatabase.Refresh();

        AssetBundleBuilderGUI.gameExePath = EditorPrefs.GetString("TRAB.GameExePath");
        AssetBundleBuilderGUI.clearCache = EditorPrefs.GetBool("TRAB.ClearCache");
        AssetBundleBuilderGUI.runGameAfterBuild = EditorPrefs.GetBool("TRAB.RunGameAfterBuild");
        AssetBundleBuilderGUI.cleanDestination = EditorPrefs.GetBool("TRAB.CleanDestination");
        AssetBundleBuilderGUI.runGameArguments = EditorPrefs.GetString("TRAB.RunGameArguments");

        AssetBundleBuilderGUI.assetBundleGroups = new List<AssetBundleGroup>();
        foreach (AssetBundleGroup assetBundleGroup in EditorCommon.GetAllProjectAssets<AssetBundleGroup>())
        {
            assetBundleGroup.selected = assetBundleGroup.isMod && assetBundleGroup.folderName != "Proto";
            AssetBundleBuilderGUI.assetBundleGroups.Add(assetBundleGroup);
        }

        try
        {
            //AssetBundleBuilderGUI.BuildSelected();
            typeof(AssetBundleBuilderGUI).GetMethod("BuildSelected", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).Invoke(null, null);
            Debug.Log("[CI] Build completed successfully.");
            EditorApplication.Exit(0);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[CI] Build failed: {ex}");
            EditorApplication.Exit(1);
        }
    }
}

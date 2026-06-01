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
    private const string PersonalAssetsRoot = "Assets/Personal";

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

        string[] guids = AssetDatabase.FindAssets("t:AssetBundleGroup");
        if (guids.Length == 0)
        {
            Debug.LogError("[CI] No AssetBundleGroup assets found.");
            EditorApplication.Exit(1);
            return;
        }

        var personalGroups = new List<AssetBundleGroup>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.StartsWith(PersonalAssetsRoot))
            {
                Debug.Log($"[CI] Skipping (not in Personal/): {path}");
                continue;
            }

            var group = AssetDatabase.LoadAssetAtPath<AssetBundleGroup>(path);
            if (group == null) continue;

            group.selected = true;
            personalGroups.Add(group);
            Debug.Log($"[CI] Selected: {group.name}");
        }

        if (personalGroups.Count == 0)
        {
            Debug.LogError($"[CI] No groups found under '{PersonalAssetsRoot}'.");
            EditorApplication.Exit(1);
            return;
        }

        AssetBundleBuilderGUI.assetBundleGroups = personalGroups;

        try
        {
            AssetBundleBuilderGUI.BuildSelected();
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

using System.Collections.Generic;
using ThunderRoad;
using ThunderRoad.AssetSorcery;
using UnityEngine;
using QualityLevel = ThunderRoad.QualityLevel;

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
        SetWindowsQualityAndPlatform();
        RunBuild();
    }

    public static void BuildAndroid()
    {
        Debug.Log("[CI] Starting Android addressable build...");
        SetAndroidQualityAndPlatform();
        RunBuild();
    }

    private static void RunBuild()
    {
        Debug.Log("[CI] Running build...");
        var assetBundleGroups = new List<AssetBundleGroup>();
        foreach (AssetBundleGroup assetBundleGroup in EditorCommon.GetAllProjectAssets<AssetBundleGroup>())
        {
            Debug.Log($"[CI] Checking asset bundle group: {assetBundleGroup.name}");

            if (!assetBundleGroup.isMod || assetBundleGroup.folderName == "Proto")
                continue;

            assetBundleGroups.Add(assetBundleGroup);
        }

        foreach (var assetBundleGroup in assetBundleGroups)
        {
            Debug.Log($"[CI] Building asset bundle group: {assetBundleGroup.name}");
            AssetBundleBuilder.Build(assetBundleGroup, false);
        }
    }

    public static void SetAndroidQualityAndPlatform()
    {
        // check if the android build support is installed
        if (!UnityEditor.BuildPipeline.IsBuildTargetSupported(UnityEditor.BuildTargetGroup.Android, UnityEditor.BuildTarget.Android))
        {
            Debug.LogWarning($"[CI] Android Build Support is not installed. Please install it via the Unity Hub.");
            return;
        }
        //set the quality to android
        Debug.Log($"[CI] Setting quality to {QualityLevel.Android}");
        QualitySettings.SetQualityLevel((int)QualityLevel.Android);
        Common.GetQualityLevel(true); // Force cache platform 
        AssetSorceryPlatform.AssetSorceryShaderSetPlatform(AssetSorceryPlatformRuntime.AssetSorceryGetBuildPlatform(true));
        //switch the build platform to android
        if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
        {
            Debug.Log("[CI] Platform is already set to Android.");
        }
        else
        {
            UnityEditor.EditorUserBuildSettings.SwitchActiveBuildTarget(UnityEditor.BuildTargetGroup.Android, UnityEditor.BuildTarget.Android);
        }
        Debug.Log("[CI] Set quality to Android and switched platform to Android.");
    }

    public static void SetWindowsQualityAndPlatform()
    {
        //set the quality to android
        Debug.Log($"Setting platform to {QualityLevel.Windows}");
        QualitySettings.SetQualityLevel((int)QualityLevel.Windows);
        Common.GetQualityLevel(true); // Force cache platform 
        AssetSorceryPlatform.AssetSorceryShaderSetPlatform(AssetSorceryPlatformRuntime.AssetSorceryGetBuildPlatform(true));
        //switch the build platform to Windows
        if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.StandaloneWindows64)
        {
            Debug.Log("[CI] Platform is already set to Windows.");
        }
        else
        {
            UnityEditor.EditorUserBuildSettings.SwitchActiveBuildTarget(UnityEditor.BuildTargetGroup.Standalone, UnityEditor.BuildTarget.StandaloneWindows64);
        }
        Debug.Log("[CI] Set quality to Standalone and switched platform to Standalone.");

    }
}

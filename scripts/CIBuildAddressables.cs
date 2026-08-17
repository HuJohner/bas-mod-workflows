using System;
using System.Collections.Generic;
using System.Linq;
using ThunderRoad;
using ThunderRoad.AssetSorcery;
using UnityEditor;
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
    public static void DummyBuild()
    {
        Debug.Log("[CI] Dummy build - Quitting immediately.");
        EditorApplication.Exit(0);
    }

    public static void BuildWindows()
    {
        LogProjectStructure();
        Debug.Log("[CI] Starting Windows addressable build...");
        SetWindowsQualityAndPlatform();
        RunBuild();
    }

    public static void BuildAndroid()
    {
        LogProjectStructure();
        Debug.Log("[CI] Starting Android addressable build...");
        SetAndroidQualityAndPlatform();
        RunBuild();
    }

    private static void LogProjectStructure()
    {
        Debug.Log($"[CI] Application.dataPath: {Application.dataPath}");
        Debug.Log($"[CI] Application.productName: {Application.productName}");
        Debug.Log($"[CI] Application.companyName: {Application.companyName}");

        // Top-level folders directly under Assets/
        string assetsPath = Application.dataPath;
        if (System.IO.Directory.Exists(assetsPath))
        {
            var topLevelDirs = System.IO.Directory.GetDirectories(assetsPath);
            Debug.Log($"[CI] Top-level folders under Assets/ ({topLevelDirs.Length}):");
            foreach (var dir in topLevelDirs)
            {
                var name = System.IO.Path.GetFileName(dir);
                var fileCount = System.IO.Directory.GetFiles(dir, "*", System.IO.SearchOption.AllDirectories).Length;
                Debug.Log($"[CI]   {name}  ({fileCount} files)");
            }
        }
        else
        {
            Debug.LogError($"[CI] Assets path does not exist: {assetsPath}");
        }

        // Confirm the mod symlink actually resolved to real files, not an empty/broken link
        string personalPath = System.IO.Path.Combine(assetsPath, "Personal");
        if (System.IO.Directory.Exists(personalPath))
        {
            foreach (var modDir in System.IO.Directory.GetDirectories(personalPath))
            {
                var count = System.IO.Directory.GetFiles(modDir, "*", System.IO.SearchOption.AllDirectories).Length;
                Debug.Log($"[CI]   Personal/{System.IO.Path.GetFileName(modDir)}  ({count} files)");
            }
        }

        string sdkPath = System.IO.Path.Combine(assetsPath, "SDK");
        if (System.IO.Directory.Exists(sdkPath))
        {
            foreach (var sdkDir in System.IO.Directory.GetDirectories(sdkPath))
            {
                var count = System.IO.Directory.GetFiles(sdkDir, "*", System.IO.SearchOption.AllDirectories).Length;
                Debug.Log($"[CI]   SDK/{System.IO.Path.GetFileName(sdkDir)}  ({count} files)");
            }
        }
    }

    private static void RunBuild()
    {
        Debug.Log($"[CI] Pre-refresh state: isCompiling={EditorApplication.isCompiling}, isUpdating={EditorApplication.isUpdating}");

        // Force any pending import/compile to fully settle before querying the AssetDatabase
        // AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        // Debug.Log($"[CI] Post-refresh state: isCompiling={EditorApplication.isCompiling}, isUpdating={EditorApplication.isUpdating}");
        // Debug.Log($"[CI] Raw t:AssetBundleGroup GUID count: {AssetDatabase.FindAssets("t:AssetBundleGroup").Length}");
        // Debug.Log($"[CI] Total assets in project (t:Object): {AssetDatabase.FindAssets("t:Object").Length}");

        var results = new List<AssetBundleGroup>();
        string[] allFiles = System.IO.Directory.GetFiles(Application.dataPath, "*.asset", System.IO.SearchOption.AllDirectories);
        foreach (string path in allFiles)
        {
            var asset = AssetDatabase.LoadAssetAtPath<AssetBundleGroup>(path);
            if (asset != null)
                results.Add(asset);
        }
        Debug.Log($"[CI] Found {results.Count} AssetBundleGroup assets via LoadAssetAtPath");

        AssetBundleBuilderGUI.gameExePath = EditorPrefs.GetString("TRAB.GameExePath");
        AssetBundleBuilderGUI.clearCache = EditorPrefs.GetBool("TRAB.ClearCache");
        AssetBundleBuilderGUI.runGameAfterBuild = EditorPrefs.GetBool("TRAB.RunGameAfterBuild");
        AssetBundleBuilderGUI.cleanDestination = EditorPrefs.GetBool("TRAB.CleanDestination");
        AssetBundleBuilderGUI.runGameArguments = EditorPrefs.GetString("TRAB.RunGameArguments");

        AssetBundleBuilderGUI.assetBundleGroups = new List<AssetBundleGroup>();
        foreach (AssetBundleGroup assetBundleGroup in EditorCommon.GetAllProjectAssets<AssetBundleGroup>())
        {
            Debug.Log($"[CI] Adding asset bundle group: {assetBundleGroup.name}");
            assetBundleGroup.selected = assetBundleGroup.isMod && assetBundleGroup.folderName != "Proto";
            assetBundleGroup.exportAfterBuild = false;
            AssetBundleBuilderGUI.assetBundleGroups.Add(assetBundleGroup);
        }

        //AssetBundleBuilderGUI.BuildSelected();
        BuildSelected();
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

    public static void BuildSelected()
    {
        try
        {
            Debug.Log("[CI] Opening new scene.");
            // Open a new scene
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);

            Debug.Log("[CI] Unloading unused assets.");
            EditorUtility.UnloadUnusedAssetsImmediate(); // https://issuetracker.unity3d.com/issues/addressables-very-slow-build-when-editor-heap-memory-is-full
            GC.Collect();

            Debug.Log("[CI] Close Addressables Groups window.");
            //AssetBundleBuilderGUI.CloseAddressablesGroupsWindow(); // https://forum.unity.com/threads/buildplayercontent-calculate-asset-dependency-data-takes-forever.1015951/
            var window = EditorWindow.GetWindow(typeof(EditorWindow), false, "Addressables Groups");
            if (window.titleContent.text == "Addressables Groups") window.Close();

            foreach (AssetBundleGroup assetBundleGroup in AssetBundleBuilderGUI.assetBundleGroups)
            {
                Debug.Log($"[CI] Checking asset bundle group: {assetBundleGroup.name} (selected: {assetBundleGroup.selected})");
                if (assetBundleGroup.selected)
                {
                    assetBundleGroup.OnValidate();

                    Debug.Log($"[CI] Building asset bundle group: {assetBundleGroup.name}");
                    AssetBundleBuilder.Build(assetBundleGroup, AssetBundleBuilderGUI.clearCache);

                    if (assetBundleGroup.exportAfterBuild)
                    {
                        Debug.Log($"[CI] Exporting asset bundle group: {assetBundleGroup.name}");
                        AssetBundleBuilderGUI.Export(assetBundleGroup);
                    }
                }
            }

            // The end
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

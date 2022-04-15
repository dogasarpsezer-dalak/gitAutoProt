using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.VersionControl;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class DalakBuild : EditorWindow
{

    /*//TODO: İSİM KOYMACA, EKLENTİ EKLEMECE, REORDERABLEW LIST DÜZELTMECE
    private static void BuildForAndroid(string[] scenePaths, string buildName)
    {
        var buildFolder = Directory.CreateDirectory(folderPath + "/" + buildName);
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        
        PlayerSettings.Android.keystoreName = Path.GetFullPath("../dalak_keystore.keystore");
        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keyaliasPass = "1DalakGames06?";
        PlayerSettings.Android.keystorePass = "1DalakGames06?";
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel30;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel19;
        
        buildPlayerOptions.scenes = scenePaths;
        buildPlayerOptions.locationPathName = buildFolder.FullName + "/" + buildName;
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.Development;



        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;
        
        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }
    }*/
    
    public static string[] FindCommandArguments()
    {
        string[] args = Environment.GetCommandLineArgs();
        int posExecMethod = -1;
        string[] arguments = new string[2];
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-executeMethod")
            {
                posExecMethod = i;
                arguments[0] = args[i + 2];
                arguments[1] = args[i + 3];
                break;
            }
        }
        return arguments;
    }
    
    [MenuItem("Dalak/DalakBuild")]
    public static void Build()
    {
        var arguments = FindCommandArguments();
        var name = arguments[0];
        var path = arguments[1];
        
        var scenesToBuild = EditorBuildSettings.scenes;
        var buildSettings = new BuildPlayerOptions();

        buildSettings.scenes = new string[scenesToBuild.Length];
        int count = 0;
        foreach (var scene in scenesToBuild)
        {
            buildSettings.scenes[count] = scene.path;
            count++;
        }

        buildSettings.target = BuildTarget.StandaloneWindows64;
        buildSettings.locationPathName = path + name + ".exe";
        BuildPipeline.BuildPlayer(buildSettings);

    }

}



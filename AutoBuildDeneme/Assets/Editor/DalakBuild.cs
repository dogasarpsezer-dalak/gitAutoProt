using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class DalakBuild : EditorWindow
{
    [MenuItem("DalakWindow/DalakBuildWindows")]
    public static void InitBuildWındows()
    {
        string projectPath = Application.dataPath;
        string buildTXT = "builds.txt";

        var buildTXTPath = Directory.GetParent(projectPath) + "/" + buildTXT;
        
        if (!File.Exists(buildTXTPath))
        {
            File.CreateText(buildTXTPath).Close();
        }

        List<string> buildTexts = new List<string>();
        StreamReader sr = new StreamReader(buildTXTPath);
        var readLine = sr.ReadLine();
        
        string lastBuild = "";
        while (readLine != null)
        {
            lastBuild = readLine;
            buildTexts.Add(lastBuild);
            readLine = sr.ReadLine();
        }
        sr.Close();
        
        int lastBuildNumber = buildTexts.Count + 1;
        var buildText = "build_" + lastBuildNumber;
        buildTexts.Add(buildText);
        
        File.WriteAllLines(buildTXTPath,buildTexts);
        
        //GIT Process
        var assetsParent = Directory.GetParent(projectPath);
        var gitRepoPath = assetsParent.Parent.FullName;
        
        CommitRepo(gitRepoPath,buildTXTPath,lastBuildNumber);
        
    }

    public static void CommitRepo(string repoPath,string buildsTXTPath, int buildNumber)
    {
        string pathRepo = repoPath.Replace(@"\","/");
        string gitAddCommand = string.Format(" --git-dir={0}/.git --work-tree={0} add {1}",pathRepo,buildsTXTPath);
        string gitCommitCommand = string.Format(" --git-dir={0}/.git --work-tree={0} commit -m \"Build Commit {1}\" ",pathRepo, buildNumber);
        string gitRemoteCommand = string.Format(" --git-dir={0}/.git --work-tree={0} remote add origin https://github.com/dogasarpsezer-dalak/gitAutoProt.git",pathRepo);
        string gitPushCommand = string.Format(" --git-dir={0}/.git --work-tree={0} push origin main",pathRepo);
            
        ProcessStartInfo processStartInfo = new ProcessStartInfo()
        {
            //OS or EXE should start the process
            UseShellExecute = false,
                
            //The Output will be Standard Output Stream
            RedirectStandardOutput = true,
                
            //Application to start
            FileName = "git.exe",
                
            Arguments = gitAddCommand
        };
            
        Process gitProcess = new Process();
        gitProcess.StartInfo = processStartInfo;
        gitProcess.Start();

        processStartInfo.Arguments = gitCommitCommand;
        gitProcess.Start();
        
        processStartInfo.Arguments = gitPushCommand;
        gitProcess.Start();
        
        string output = gitProcess.StandardOutput.ReadToEnd();
        gitProcess.WaitForExit();
        Debug.Log(output);
    }
    
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

    public static void BuildForAndroid()
    {
        var arguments = FindCommandArguments();
        var name = arguments[0];
        var path = arguments[1];
        
        var buildSettings = new BuildPlayerOptions();
        buildSettings.target = BuildTarget.Android;
        buildSettings.locationPathName = path + name + ".apk";
        PlayerSettings.Android.keystoreName = Path.GetFullPath("../dalak_keystore.keystore");
        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keyaliasPass = "1DalakGames06?";
        PlayerSettings.Android.keystorePass = "1DalakGames06?";
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel30;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel19;
        buildSettings.target = BuildTarget.Android;
        buildSettings.options = BuildOptions.Development;
        
        BuildProject(buildSettings);

    }
    
    public static void BuildForWindows64()
    {
        var arguments = FindCommandArguments();
        var name = arguments[0];
        var path = arguments[1];
        
        var buildSettings = new BuildPlayerOptions();
        buildSettings.target = BuildTarget.StandaloneWindows64;
        buildSettings.locationPathName = path + name + ".exe";
        
        BuildProject(buildSettings);
    }
    
    public static void BuildProject(BuildPlayerOptions buildPlayerOptions)
    {
        var scenesToBuild = EditorBuildSettings.scenes;
        buildPlayerOptions.scenes = new string[scenesToBuild.Length];
        int count = 0;
        foreach (var scene in scenesToBuild)
        {
            buildPlayerOptions.scenes[count] = scene.path;
            count++;
        }

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;
        
        if (summary.result == BuildResult.Succeeded)
        {
            //Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }

    }

}



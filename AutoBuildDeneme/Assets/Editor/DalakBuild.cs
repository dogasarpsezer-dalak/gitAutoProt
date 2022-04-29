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
    public static string buildRepoPath = "D://BuildRepo/build_pipeline";
    
    [MenuItem("DalakBuild/Create Local Pipeline Repo")]
    public static void CreatePipelineOnLocal()
    {
        if (Directory.Exists(buildRepoPath))
        {
            Debug.LogError($"Local pipeline repo already exists you can use build! If you want to create it again please remove the local repo first! Path to delete: {buildRepoPath}");
            return;
        }
        
        string buildRepoURL = "https://github.com/dogasarpsezer-dalak/build_pipeline.git";
        string gitCloneCommand = string.Format(" --git-dir=D:// clone {0} {1}",buildRepoURL,buildRepoPath);
        string gitRemoteCommand = string.Format(" --git-dir={0}/.git --work-tree={0} remote add origin {1}",buildRepoPath,buildRepoURL);
        ProcessStartInfo processStartInfo = new ProcessStartInfo()
        {
            //OS or EXE should start the process
            UseShellExecute = false,
                
            //The Output will be Standard Output Stream
            RedirectStandardOutput = true,
                
            //Application to start
            FileName = "git.exe",
                
            Arguments = gitCloneCommand
        };
            
        Process gitProcess = new Process();
        gitProcess.StartInfo = processStartInfo;
        gitProcess.Start();

        processStartInfo.Arguments = gitRemoteCommand;
        gitProcess.Start();
        
        gitProcess.WaitForExit();
    }
    [MenuItem("DalakBuild/Add To Build Queue")]
    public static void InitBuildWindows()
    {
        
        var queuePath = buildRepoPath + "/build_queue.txt";
        if (!File.Exists(queuePath))
        {
            Debug.LogError($"Build Queue is missing please remove the repo path and create it again! Path to delete: {buildRepoPath}");
            return;
        }
        
        string projectPath = Application.dataPath;
        var assetsParent = Directory.GetParent(projectPath);
        var gitRepoPath = assetsParent.Parent.FullName;
        Debug.Log(gitRepoPath);
        string gitRepoURLCommand = string.Format("--git-dir={0}/.git --work-tree={0} config --get remote.origin.url",gitRepoPath);
        
        ProcessStartInfo processStartInfo = new ProcessStartInfo()
        {
            //OS or EXE should start the process
            UseShellExecute = false,
                
            //The Output will be Standard Output Stream
            RedirectStandardOutput = true,
                
            //Application to start
            FileName = "git.exe",
                
            Arguments = gitRepoURLCommand
        };

        Process gitProcess = new Process();
        gitProcess.StartInfo = processStartInfo;
        gitProcess.Start();
        string gitRepoURL = gitProcess.StandardOutput.ReadLine();
        gitProcess.WaitForExit();

        using (StreamWriter streamWriter = File.AppendText(queuePath))
        {
            streamWriter.Write(gitRepoURL + "\t|IN QUEUE|\t|DATE & TIME OF INSERTION: " + DateTime.Now.ToString("MM/dd/yyyy h:mm tt") + "|" + "\n");
            streamWriter.Close();
        }
        
        CommitBuildRepo(buildRepoPath,queuePath);
    }

    public static void CommitBuildRepo(string repoPath, string txtPath)
    {
        string gitAddCommand = string.Format("--git-dir={0}/.git --work-tree={0} add {1}",repoPath,txtPath);
        string gitStatusCommand = string.Format( "--git-dir={0}/.git --work-tree={0} status",repoPath);
        string gitDiffCommand = string.Format( "--git-dir={0}/.git --work-tree={0} diff",repoPath);
        string gitCommitCommand = string.Format("--git-dir={0}/.git --work-tree={0} commit -m \"Build Commit\"",repoPath);
        string gitPushCommand = string.Format("--git-dir={0}/.git --work-tree={0} push origin main",repoPath);
            
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
        gitProcess.WaitForExit();

        processStartInfo.Arguments = gitStatusCommand;
        gitProcess.Start();
        gitProcess.WaitForExit();
        
        processStartInfo.Arguments = gitCommitCommand;
        gitProcess.Start();
        gitProcess.WaitForExit();

        processStartInfo.Arguments = gitPushCommand;
        gitProcess.Start();
        
        gitProcess.WaitForExit();
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



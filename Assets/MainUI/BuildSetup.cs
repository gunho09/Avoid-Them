#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class BuildSetup
{
    [MenuItem("Tools/Fix Build Settings")]
    public static void FixBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
        
        string mainUIPath = "Assets/MainUI/MainUI.unity";
        string gamePath = "Assets/Map/Game.unity";
        string clearUIPath = "Assets/MainUI/ClearUI.unity";
        
     
        if (File.Exists(mainUIPath))
        {
            scenes.Add(new EditorBuildSettingsScene(mainUIPath, true));
            Debug.Log("Added MainUI to Build Settings at Index 0.");
        }
        else
        {
            Debug.LogError($"Could not find MainUI scene at {mainUIPath}");
        }

      
        if (File.Exists(gamePath))
        {
            scenes.Add(new EditorBuildSettingsScene(gamePath, true));
            Debug.Log("Added Game to Build Settings at Index 1.");
        }
        else
        {
            Debug.LogError($"Could not find Game scene at {gamePath}");
        }

        if (File.Exists(clearUIPath))
        {
            scenes.Add(new EditorBuildSettingsScene(clearUIPath, true));
            Debug.Log("Added ClearUI to Build Settings.");
        }
        else
        {
            Debug.LogError($"Could not find ClearUI scene at {clearUIPath}");
        }

        string practiceRoomPath = "Assets/MainUI/practiceRoom.unity";
        if (File.Exists(practiceRoomPath))
        {
            scenes.Add(new EditorBuildSettingsScene(practiceRoomPath, true));
            Debug.Log("Added practiceRoom to Build Settings.");
        }
        else
        {
            Debug.LogError($"Could not find practiceRoom scene at {practiceRoomPath}");
        }

       
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene.path != mainUIPath && scene.path != gamePath && scene.path != clearUIPath && scene.path != practiceRoomPath)
            {
                scenes.Add(scene);
            }
        }

        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("Build Settings Updated Successfully. MainUI is first.");
    }
}
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitialScene : MonoBehaviour {
    void Start() {
        List<string> commandLineArgs = new List<string>(Environment.GetCommandLineArgs());
        int index = commandLineArgs.IndexOf("--scene");
        if (index == -1) {
            File.AppendAllText("debug.log", $"No --scene in {Environment.CommandLine}\n");
            Application.Quit();
        }
        else {
            string sceneName = commandLineArgs[index + 1];
            SceneManager.LoadScene(sceneName);
        }
    }
}

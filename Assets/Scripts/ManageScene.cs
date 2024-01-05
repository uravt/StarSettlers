using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ManageScene : MonoBehaviour
{
    public static void LoadSettings()
    {
        SceneManager.LoadScene("Settings");
    }
    public static void LoadGame()
    {
        SceneManager.LoadScene("Game");
    }
    public static void LoadMainMenu()
    {
        SceneManager.LoadScene("StartScene");
    }
    public static void exitGame(){
        Application.Quit();
    }
}

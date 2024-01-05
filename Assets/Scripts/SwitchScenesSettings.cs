using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScenesSettings : MonoBehaviour
{
    public void switchScene()
    {
        SceneManager.LoadScene("Settings");
    }
    public void backToMain()
    {
        SceneManager.LoadScene("StartScreen");
    }
    

}

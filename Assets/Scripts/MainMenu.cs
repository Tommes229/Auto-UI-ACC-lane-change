using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private int ACC = 0;
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        PlayerPrefs.SetInt("GameValue", 0);
        PlayerPrefs.SetInt("ACC", ACC);
        
    }

    public void Test1()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        PlayerPrefs.SetInt("GameValue", 1);
        PlayerPrefs.SetInt("ACC", ACC);
    }

    public void Test2()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        PlayerPrefs.SetInt("GameValue", 2);
        PlayerPrefs.SetInt("ACC", ACC);

    }

    public void QuitGame()
    {
        Application.Quit();
    }
     
    public void ACCONOFF() {
        ACC = ACC == 1 ? 0 : 1;
    }   
}

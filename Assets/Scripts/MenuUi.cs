using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuUi : MonoBehaviour
{
    public GameObject LoadingScreen;

    public void PlayButton()
    {
        StartCoroutine(StartPlay());
    }

    IEnumerator StartPlay()
    {
        LoadingScreenOn();
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("Caravan Talent Agent");
    }

    public void ExitButton()
    {
        Application.Quit();
    }

    public void LoadingScreenOn()
    {
        LoadingScreen.SetActive(true);
        
    }

}

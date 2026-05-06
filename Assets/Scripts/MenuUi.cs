using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuUi : MonoBehaviour
{
    public GameObject LoadingScreen;

    public AudioClip ButtonClick;

    public void PlayButton()
    {
        StartCoroutine(StartPlay());
    }

    IEnumerator StartPlay()
    {
        GetComponent<AudioSource>().PlayOneShot(ButtonClick);
        LoadingScreenOn();
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("Caravan Talent Agent");
    }

    public void ExitButton()
    {
        GetComponent<AudioSource>().PlayOneShot(ButtonClick);
        Application.Quit();
    }

    public void LoadingScreenOn()
    {
        LoadingScreen.SetActive(true);
        
    }

}

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
public class LevelManager : MonoBehaviour
{

    public List<LevelObject> LevelList = new List<LevelObject>();

    public TMP_Text LevelNameObj;
    public TMP_Text LevelDescriptionObj;
    public TMP_Text AccGradeObj;
    public TMP_Text TimeGradeObj;
    public TMP_Text CutGradeObj;



    void Start()
    {
        
    }

    void Update()
    {
        
    }


    public void SelectLevel(int LevelId)
    {
        LevelNameObj.text = LevelList[LevelId].Name;
        LevelDescriptionObj.text = LevelList[LevelId].Description;
        PlayerPrefs.SetInt("SelectedLevel", LevelId);
    }

    public void LoadLevel()
    {
        SceneManager.LoadScene("Main");
    }

}

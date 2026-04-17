using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using UnityEditor.ShaderGraph.Internal;
public class LevelManager : MonoBehaviour
{

    public List<LevelObject> LevelList = new List<LevelObject>();

    public DualContouring DCScript;

    public TMP_Text LevelNameObj;
    public TMP_Text LevelDescriptionObj;
    public TMP_Text AccGradeObj;
    public TMP_Text TimeGradeObj;
    public TMP_Text CutGradeObj;

    int CurrentLevel;

    void Start()
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Main"))
        {
            CurrentLevel = PlayerPrefs.GetInt("SelectedLevel");
            GameObject FirstTarget = Instantiate(LevelList[CurrentLevel].TargetSculptures[0]);
            FirstTarget.name = "Target";
            DCScript.Target = FirstTarget;
        }
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

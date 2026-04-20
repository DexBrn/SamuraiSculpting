using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using UnityEditor.ShaderGraph.Internal;
using static UnityEngine.GraphicsBuffer;
public class LevelManager : MonoBehaviour
{

    public List<LevelObject> LevelList = new List<LevelObject>();

    public DualContouring DCScript;
    ResultsScreen RSScript;
    SculptureCheckScript SCScript;

    public TMP_Text LevelNameObj;
    public TMP_Text LevelDescriptionObj;
    public TMP_Text AccGradeObj;
    public TMP_Text TimeGradeObj;
    public TMP_Text CutGradeObj;

    public int CurrentLevel;

    GameObject CurrentTarget;
    int CurrentTargetIndex;

    void Awake()
    {
        RSScript = GetComponent<ResultsScreen>();
        SCScript = GetComponent<SculptureCheckScript>();

        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Main"))
        {
            CurrentLevel = PlayerPrefs.GetInt("SelectedLevel");
            CurrentTarget = Instantiate(LevelList[CurrentLevel].TargetSculptures[0]);
            CurrentTarget.name = "Target";
            DCScript.Target = CurrentTarget;
            CurrentTargetIndex++;
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Return))
        {
            if (CurrentTargetIndex == 3)
            { RSScript.OpenResultsScreen(); return; }
            SCScript.FullCheck();
            Destroy(CurrentTarget);
            DCScript.ResetMarble();
            CurrentTarget = Instantiate(LevelList[CurrentLevel].TargetSculptures[CurrentTargetIndex]);
            CurrentTarget.name = "Target";
            DCScript.Target = CurrentTarget;
            CurrentTargetIndex++;
            
        }
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

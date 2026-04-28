using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEditor.ShaderGraph.Internal;
using static UnityEngine.GraphicsBuffer;
public class LevelManager : MonoBehaviour
{

    public List<LevelObject> LevelList = new List<LevelObject>();

    public DualContouring DCScript;
    ResultsScreen RSScript;
    SculptureCheckScript SCScript;
    Dialogue Dialogue;
    Slicing Slicing;
    Timer Timer;

    public Transform LevelGrid;
    public TMP_Text LevelNameObj;
    public TMP_Text LevelDescriptionObj;
    public TMP_Text AccGradeObj;
    public TMP_Text TimeGradeObj;
    public TMP_Text CutGradeObj;

    public int CurrentLevel;

    GameObject CurrentTarget;
    int CurrentTargetIndex;

    public SaveFile SaveFile;

    public string[] PossibleGrades;
    public string[] PossibleFinalGrades;
    public Color[] GradeColours;
    public Color[] FinalGradeColours;

    public Vector3 PreviewPosition;

    public GameObject LoadingScreen;

    void Awake()
    {
        RSScript = GetComponent<ResultsScreen>();
        SCScript = GetComponent<SculptureCheckScript>();
        if (GetComponent<Dialogue>())
            Dialogue = GetComponent<Dialogue>();
        if (GetComponent<Slicing>())
            Slicing = GetComponent<Slicing>();
        if (GetComponent<Timer>())
            Timer = GetComponent<Timer>();

        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Main"))
        {
            CurrentLevel = PlayerPrefs.GetInt("SelectedLevel");
            CurrentTarget = Instantiate(LevelList[CurrentLevel].TargetSculptures[0]);
            CurrentTarget.name = "Target";
            DCScript.Target = CurrentTarget;
            CurrentTargetIndex++;
        }
        else
        {
            SaveFile = SaveFile.CreateFromPlayerPrefs();
            if (SaveFile == null)
            {
                print("Created New Save");
                SaveFile = new SaveFile();
                SaveFile.SaveToPlayerPrefs();
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (Slicing.CutCount == 0 || Timer.CurrentTime == 0)
                return;
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

        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Main"))
            if (Input.GetKeyDown(KeyCode.R))
                DCScript.ResetMarble();
            else if (Input.GetKeyDown(KeyCode.Escape))
                SceneManager.LoadScene("Caravan Talent Agent");
    }


    public void SelectLevel(int LevelId)
    {
        LevelNameObj.text = LevelList[LevelId].Name;
        LevelDescriptionObj.text = LevelList[LevelId].Description;

        if (SaveFile.AllAchievedGrades[(LevelId + 1) * 4 - 4 + 0] != 0)
            AccGradeObj.text = $"<color=#{ColorUtility.ToHtmlStringRGB(GradeColours[SaveFile.AllAchievedGrades[(LevelId + 1) * 4 - 4 + 0] - 1])}>{PossibleGrades[SaveFile.AllAchievedGrades[(LevelId + 1) * 4 - 4 + 0] - 1]}";
        else
            AccGradeObj.text = "";

        if (SaveFile.AllAchievedGrades[(LevelId + 1) * 4 - 4 + 1] != 0)
            TimeGradeObj.text = $"<color=#{ColorUtility.ToHtmlStringRGB(GradeColours[SaveFile.AllAchievedGrades[(LevelId + 1) * 4 - 4 + 1] - 1])}>{PossibleGrades[SaveFile.AllAchievedGrades[(LevelId + 1) * 4 - 4 + 1] - 1]}"; 
        else
            TimeGradeObj.text = "";

        if (SaveFile.AllAchievedGrades[(LevelId + 1) * 4 - 4 + 2] != 0)
            CutGradeObj.text = $"<color=#{ColorUtility.ToHtmlStringRGB(GradeColours[SaveFile.AllAchievedGrades[(LevelId + 1) * 4 - 4 + 2] - 1])}>{PossibleGrades[SaveFile.AllAchievedGrades[(LevelId + 1) * 4 - 4 + 2] - 1]}";
        else
            CutGradeObj.text = "";

        Destroy(GameObject.Find("PreviewObject"));
        GameObject NewPreviewObject = Instantiate(LevelList[LevelId].TargetSculptures[0]);
        NewPreviewObject.transform.position = PreviewPosition;
        NewPreviewObject.AddComponent<ContinuousRotation>();
        NewPreviewObject.name = "PreviewObject";
        NewPreviewObject.layer = LayerMask.GetMask("Default");



        PlayerPrefs.SetInt("SelectedLevel", LevelId);
    }

    public void LoadLevel()
    {
        int NextLevel = PlayerPrefs.GetInt("SelectedLevel");
        Dialogue.StartDialogue();
        Dialogue.TextList.Clear();
        for (int i = 0; i < LevelList[NextLevel].PreLevelDialogue.Count; i++)
            Dialogue.TextList.Add(LevelList[NextLevel].PreLevelDialogue[i]);
        Dialogue.StartLevelAfter = true;
        Dialogue.CurrentText = 0;
        StartCoroutine(Dialogue.WriteText(LevelList[NextLevel].PreLevelDialogue[0]));
        //SceneManager.LoadScene("Main");
    }

    public void PopulateLevelGrades()
    {
        for (int i = 0; i < LevelGrid.childCount; i++)
        {
            if (SaveFile.AllAchievedGrades[(i + 1) * 4 - 4 + 3] != 0)
                LevelGrid.GetChild(i).GetChild(1).GetComponent<TMP_Text>().text = $"<color=#{ColorUtility.ToHtmlStringRGB(FinalGradeColours[SaveFile.AllAchievedGrades[(i + 1) * 4 - 4 + 3] - 1])}>{PossibleFinalGrades[SaveFile.AllAchievedGrades[(i + 1) * 4 - 4 + 3] - 1]}"; 
            else
                LevelGrid.GetChild(i).GetChild(1).GetComponent<TMP_Text>().text = "";
        }
    }

    public void LockLevels()
    {
        for (int i = 1; i < LevelGrid.childCount; i++)
        {
            if (SaveFile.AllAchievedGrades[(i) * 4 - 4 + 3] != 0 && SaveFile.AllAchievedGrades[(i) * 4 - 4 + 3] < 12)
                LevelGrid.GetChild(i).GetComponent<Image>().color = new Color(1, 0.6273585f, 0.7986355f);
            else
            { LevelGrid.GetChild(i).GetComponent<Image>().color = new Color(0.172549f, 0.172549f, 0.172549f); LevelGrid.GetChild(i).GetComponent<Button>().enabled = false ; }
        }
    }



    public void NextLevel()
    {
        PlayerPrefs.SetInt("SelectedLevel", CurrentLevel+1);
        StartCoroutine(LoadMain());
    }
    
    public void RetryLevel()
    {
        SceneManager.LoadScene("Main");
    }
     
    public void GoToMenu()
    {
        SceneManager.LoadScene("Caravan Talent Agent");
    }

    public IEnumerator LoadMain()
    {
        LoadingScreen.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("Main");
    }

}

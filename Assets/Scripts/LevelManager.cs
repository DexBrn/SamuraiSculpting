using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.UI;

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
    public int CurrentTargetIndex;

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
            GameObject VisibleTarget = Instantiate(LevelList[CurrentLevel].TargetSculptures[0]);
            VisibleTarget.layer = LayerMask.GetMask("Default");
            VisibleTarget.transform.parent = CurrentTarget.transform;
            VisibleTarget.name = "VisibleTarget";
            var Outline = VisibleTarget.AddComponent<BetterOutline>();
            Outline.OutlineMode = BetterOutline.Mode.OutlineVisible;
            Outline.OutlineColor = Color.black;
            Outline.OutlineWidth = 4.15f;
            if (VisibleTarget.transform.childCount > 0)
            { VisibleTarget.transform.GetChild(0).gameObject.layer = LayerMask.GetMask("Default"); if (VisibleTarget.transform.childCount > 0) { VisibleTarget.transform.GetChild(0).GetChild(0).gameObject.layer = LayerMask.GetMask("Default"); } }
            DCScript.Target = CurrentTarget;
            DCScript.VisibleTarget = VisibleTarget;
            VisibleTarget.SetActive(false);
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

            GameObject VisibleTarget = Instantiate(LevelList[CurrentLevel].TargetSculptures[CurrentTargetIndex]);
            VisibleTarget.layer = LayerMask.GetMask("Default");
            VisibleTarget.transform.parent = CurrentTarget.transform;
            VisibleTarget.name = "VisibleTarget";
            if (VisibleTarget.transform.childCount > 0)
            { VisibleTarget.transform.GetChild(0).gameObject.layer = LayerMask.GetMask("Default"); if (VisibleTarget.transform.GetChild(0).childCount > 0) { VisibleTarget.transform.GetChild(0).GetChild(0).gameObject.layer = LayerMask.GetMask("Default"); } }
            var Outline = VisibleTarget.AddComponent<BetterOutline>();
            Outline.OutlineMode = BetterOutline.Mode.OutlineVisible;
            Outline.OutlineColor = Color.black;
            Outline.OutlineWidth = 4.15f;

            DCScript.Target = CurrentTarget;
            DCScript.VisibleTarget = VisibleTarget;
            VisibleTarget.SetActive(false);
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
        Dialogue.LoadLevelDialogue(LevelList[NextLevel]);
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
        PlayerPrefs.SetInt("Continuing", 1);
        StartCoroutine(LoadVan());
    }
    
    public void RetryLevel()
    {
        SceneManager.LoadScene("Main");
    }
     
    public void GoToMenu()
    {
        StartCoroutine(LoadVan());
    }

    public IEnumerator LoadMain()
    {
        LoadingScreen.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("Main");
    }

    public IEnumerator LoadVan()
    {
        LoadingScreen.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("Caravan Talent Agent");
    }

}

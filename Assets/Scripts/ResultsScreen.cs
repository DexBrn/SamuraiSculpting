using UnityEngine;
using TMPro;
using System.Collections;
using static UnityEngine.Rendering.DebugUI;
using System.Linq;
public class ResultsScreen : MonoBehaviour
{

    public GameObject ResultsPanel;
    public TMP_Text AccuracyText;
    public TMP_Text TimeTakenText;
    public TMP_Text CutCountText;
    public TMP_Text GradeText;
    public GameObject ContinueButton;
    public GameObject RetryButton;
    public GameObject MenuButton;

    public float Accuracy;
    public float TimeTaken;
    public float CutCount;
    public string Grade;

    int TotalGrades;

    public string[] PossibleGrades;
    public string[] PossibleFinalGrades;
    public Color[] GradeColours;
    public Color[] FinalGradeColours;

    int AccGrade;
    int CutsGrade;
    int TimeGrade;


    SculptureCheckScript SCS;
    Slicing Slicing;
    Timer Timer;
    LevelManager LevelManager;
    public SaveFile SaveFile;

    void Start()
    {
        SCS = GetComponent<SculptureCheckScript>();
        Slicing = GetComponent<Slicing>();
        Timer = GetComponent<Timer>();
        LevelManager = GetComponent<LevelManager>();


        //SaveFile = new SaveFile();
        //SaveFile.SaveToPlayerPrefs();
        SaveFile = SaveFile.CreateFromPlayerPrefs();

        if (SaveFile == null)
        {
            print("Created New Save");
            SaveFile = new SaveFile();
            SaveFile.SaveToPlayerPrefs();
        }

    }


    void Update()
    {


    }

    public void OpenResultsScreen()
    {
        SCS.FullCheck();
        CheckGrades();
        StartCoroutine(RevealResults());
        Timer.TimerOn = false;

    }

    public IEnumerator RevealResults()
    {
        ResultsPanel.SetActive(true);
        yield return new WaitForSeconds(1);
        AccuracyText.enabled = true;
        AccuracyText.text = $"Accuracy: {Mathf.RoundToInt(SCS.TotalAccuracy)}% <color=#{ColorUtility.ToHtmlStringRGB(GradeColours[AccGrade])}>{PossibleGrades[AccGrade]}</color>";
        yield return new WaitForSeconds(1);
        TimeTakenText.enabled = true;
        TimeTakenText.text = $"Time: {Mathf.RoundToInt(Timer.CurrentTime)}s <color=#{ColorUtility.ToHtmlStringRGB(GradeColours[TimeGrade])}>{PossibleGrades[TimeGrade]}</color>";
        yield return new WaitForSeconds(1);
        CutCountText.enabled = true;
        CutCountText.text = $"Cuts: {Slicing.CutCount} <color=#{ColorUtility.ToHtmlStringRGB(GradeColours[CutsGrade])}>{PossibleGrades[CutsGrade]}</color>";
        yield return new WaitForSeconds(1);
        GradeText.enabled = true;
        GradeText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(FinalGradeColours[TotalGrades])}>{PossibleFinalGrades[TotalGrades]}</color>";
        yield return new WaitForSeconds(1);
        ContinueButton.SetActive(true);
        RetryButton.SetActive(true);
        MenuButton.SetActive(true);
    }

    public void CheckGrades()
    {
        int CurrentLevel = LevelManager.CurrentLevel;

        for (int i = 0; i < LevelManager.LevelList[CurrentLevel].Accuracy.Count; i++)
        {
            if (SCS.TotalAccuracy > LevelManager.LevelList[CurrentLevel].Accuracy[i])
            { TotalGrades += i; AccGrade = i; break; }
            if (i == LevelManager.LevelList[CurrentLevel].Accuracy.Count - 1)
            { TotalGrades += 5; AccGrade = 5; }
        }
        for (int i = 0; i < LevelManager.LevelList[CurrentLevel].TimeTaken.Count; i++)
        {
            if (Timer.CurrentTime < LevelManager.LevelList[CurrentLevel].TimeTaken[i])
            { TotalGrades += i; TimeGrade = i; break; }
            if (i == LevelManager.LevelList[CurrentLevel].TimeTaken.Count - 1)
            { TotalGrades += 5; TimeGrade = 5; }
        }
        for (int i = 0; i < LevelManager.LevelList[CurrentLevel].Cuts.Count; i++)
        {
            if (Slicing.CutCount < LevelManager.LevelList[CurrentLevel].Cuts[i])
            { TotalGrades += i; CutsGrade = i; break; }
            if (i == LevelManager.LevelList[CurrentLevel].Cuts.Count - 1)
            { TotalGrades += 5; CutsGrade = 5; }
        }

        if (SaveFile.AllAchievedGrades[((CurrentLevel + 1) * 4) - 4] > AccGrade+1 || SaveFile.AllAchievedGrades[((CurrentLevel + 1) * 4) - 4] == 0)
            SaveFile.AllAchievedGrades[((CurrentLevel + 1) * 4) - 4] = AccGrade+1;
        if (SaveFile.AllAchievedGrades[((CurrentLevel + 1) * 4) - 4+1] > TimeGrade + 1 || SaveFile.AllAchievedGrades[((CurrentLevel + 1) * 4) - 4+1] == 0)
            SaveFile.AllAchievedGrades[((CurrentLevel + 1) * 4) - 4 + 1] = TimeGrade+1;
        if (SaveFile.AllAchievedGrades[((CurrentLevel + 1) * 4) - 4 + 2] > CutsGrade + 1 || SaveFile.AllAchievedGrades[((CurrentLevel + 1) * 4) - 4 + 2] == 0)
            SaveFile.AllAchievedGrades[((CurrentLevel + 1) * 4) - 4 + 2] = CutsGrade + 1;
        if (SaveFile.AllAchievedGrades[((CurrentLevel + 1) * 4) - 4 + 3] > TotalGrades + 1 || SaveFile.AllAchievedGrades[((CurrentLevel + 1) * 4) - 4 + 3] == 0)
            SaveFile.AllAchievedGrades[((CurrentLevel + 1) * 4) - 4 + 3] = TotalGrades + 1;
        SaveFile.SaveToPlayerPrefs();

        //Make It so now it only changes if new grade is better

       
    }
}

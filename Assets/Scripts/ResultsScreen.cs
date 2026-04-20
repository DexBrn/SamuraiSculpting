using UnityEngine;
using TMPro;
using System.Collections;
using static UnityEngine.Rendering.DebugUI;
public class ResultsScreen : MonoBehaviour
{

    public GameObject ResultsPanel;
    public TMP_Text AccuracyText;
    public TMP_Text TimeTakenText;
    public TMP_Text CutCountText;
    public TMP_Text GradeText;

    public float Accuracy;
    public float TimeTaken;
    public float CutCount;
    public string Grade;

    int TotalGrades;

    SculptureCheckScript SCS;
    Slicing Slicing;
    Timer Timer;
    LevelManager LevelManager;

    void Start()
    {
        SCS = GetComponent<SculptureCheckScript>();
        Slicing = GetComponent<Slicing>();
        Timer = GetComponent<Timer>();
        LevelManager = GetComponent<LevelManager>();
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            return;
            SCS.SecondCheck();
            StartCoroutine(RevealResults());
            Timer.TimerOn = false;

        }
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
        AccuracyText.text = $"Accuracy: {Mathf.RoundToInt(SCS.TotalAccuracy)}%";
        yield return new WaitForSeconds(1);
        TimeTakenText.enabled = true;
        TimeTakenText.text = $"Time: {Mathf.RoundToInt(Timer.CurrentTime)}s";
        yield return new WaitForSeconds(1);
        CutCountText.enabled = true;
        CutCountText.text = $"Cuts: {Slicing.CutCount}";
        yield return new WaitForSeconds(1);
        GradeText.enabled = true;
        GradeText.text = $"S";
    }

    public void CheckGrades()
    {
        int CurrentLevel = LevelManager.CurrentLevel;

        for (int i = 0; i < LevelManager.LevelList[CurrentLevel].Accuracy.Count; i++)
        {
            if (SCS.TotalAccuracy > LevelManager.LevelList[CurrentLevel].Accuracy[i])
            { TotalGrades += i; break; }
            if (i == LevelManager.LevelList[CurrentLevel].Accuracy.Count - 1)
                TotalGrades += 100;
        }
        for (int i = 0; i < LevelManager.LevelList[CurrentLevel].TimeTaken.Count; i++)
        {
            if (Timer.CurrentTime > LevelManager.LevelList[CurrentLevel].TimeTaken[i])
            { TotalGrades += i; break; }
            if (i == LevelManager.LevelList[CurrentLevel].TimeTaken.Count - 1)
                TotalGrades += 100;
        }
        for (int i = 0; i < LevelManager.LevelList[CurrentLevel].Cuts.Count; i++)
        {
            if (Slicing.CutCount > LevelManager.LevelList[CurrentLevel].Cuts[i])
            { TotalGrades += i; break; }
            if (i == LevelManager.LevelList[CurrentLevel].Cuts.Count - 1)
                TotalGrades += 100;
        }



        print(TotalGrades);
    }
}

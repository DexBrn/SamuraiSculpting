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



    SculptureCheckScript SCS;
    Slicing Slicing;
    Timer Timer;

    void Start()
    {
        SCS = GetComponent<SculptureCheckScript>();
        Slicing = GetComponent<Slicing>();
        Timer = GetComponent<Timer>();
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
        SCS.CheckTarget(LayerMask.GetMask("Xray"));
        SCS.SecondCheck();
        StartCoroutine(RevealResults());
        Timer.TimerOn = false;
    }

    public IEnumerator RevealResults()
    {
        ResultsPanel.SetActive(true);
        yield return new WaitForSeconds(1);
        AccuracyText.enabled = true;
        AccuracyText.text = $"Accuracy: {Mathf.RoundToInt(SCS.Accuracy)}%";
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


}

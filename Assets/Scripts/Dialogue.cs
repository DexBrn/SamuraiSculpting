using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Dialogue : MonoBehaviour
{

    public float LetterSpeed = 0.07f;
    public TMP_Text DialogueBox;
    public GameObject LevelSelect;
    bool WritingText = false;
    public int CurrentText;
    public bool StartLevelAfter;

    LevelManager LevelManager;

    public Animator BillAnimator;
    public List<string> TextList = new List<string>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LevelManager = GetComponent<LevelManager>();

        if (PlayerPrefs.GetInt("Continuing") == 1)
            LoadLevelDialogue(LevelManager.LevelList[PlayerPrefs.GetInt("SelectedLevel")]);
        else
        {
            string OpeningLine = TextList[Random.Range(0, TextList.Count)];
            TextList.Clear();
            TextList.Add(OpeningLine);
            StartCoroutine(WriteText(OpeningLine));
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) ||  Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Space))
        {
            if (WritingText)
            {
                WritingText = false;
                DialogueBox.text = TextList[CurrentText];
            }
            else
            {
                CurrentText++;

                if (StartLevelAfter && CurrentText >= TextList.Count) //PreLevel Dialogue
                { StartCoroutine(LevelManager.LoadMain()); DialogueBox.transform.parent.gameObject.SetActive(false); return; }

                if (CurrentText >= TextList.Count) //Pre Level Open
                { DialogueBox.transform.parent.gameObject.SetActive(false); LevelSelect.SetActive(true); LevelManager.PopulateLevelGrades(); LevelManager.LockLevels(); var Anim = "Idle1"; BillAnimator.Play(Anim, 0); return; } // + Random.Range(1, 4);

                StartCoroutine(WriteText(TextList[CurrentText]));
            }
        }
    }


    public IEnumerator WriteText(string Text)
    {
        DialogueBox.text = "";
        WritingText = true;

        var Anim = "Talking"+Random.Range(1, 7);
        BillAnimator.Play(Anim, 0);

        for (int i = 0; i < Text.Length; i++)
        {
            
            yield return new WaitForSeconds(LetterSpeed);
            if (!WritingText)
                break;
            DialogueBox.text += Text[i];

        }
        WritingText=false;
    }

    public void StartDialogue()
    {
        DialogueBox.transform.parent.gameObject.SetActive(true); LevelSelect.SetActive(false);
        DialogueBox.transform.parent.GetComponent<Animator>().Play("DialogueBoxOpen");
    }


    public void LoadLevelDialogue(LevelObject Level)
    {
        PlayerPrefs.SetInt("Continuing", 0);
        StartDialogue();
        TextList.Clear();
        for (int i = 0; i < Level.PreLevelDialogue.Count; i++)
            TextList.Add(Level.PreLevelDialogue[i]);
        StartLevelAfter = true;
        CurrentText = 0;
        StartCoroutine(WriteText(Level.PreLevelDialogue[0]));
    }


}

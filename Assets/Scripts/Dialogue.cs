using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Dialogue : MonoBehaviour
{

    float LetterSpeed = 0.035f;
    public TMP_Text DialogueBox;
    public GameObject LevelSelect;
    bool WritingText = false;
    public int CurrentText;
    public bool StartLevelAfter;

    LevelManager LevelManager;

    public List<string> TextList = new List<string>();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LevelManager = GetComponent<LevelManager>();
        StartCoroutine(WriteText(TextList[CurrentText]));
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
                if (StartLevelAfter && CurrentText >= TextList.Count)
                { SceneManager.LoadScene("Main"); DialogueBox.transform.parent.gameObject.SetActive(false); return; }
                if (CurrentText >= TextList.Count)
                { DialogueBox.transform.parent.gameObject.SetActive(false); LevelSelect.SetActive(true); LevelManager.PopulateLevelGrades(); LevelManager.LockLevels(); return; }
                StartCoroutine(WriteText(TextList[CurrentText]));
            }
        }
    }


    public IEnumerator WriteText(string Text)
    {
        DialogueBox.text = "";
        WritingText = true;
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


}

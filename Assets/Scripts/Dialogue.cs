using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Dialogue : MonoBehaviour
{

    float LetterSpeed = 0.035f;
    public TMP_Text DialogueBox;
    bool WritingText = false;
    int CurrentText;

    public List<string> TextList = new List<string>();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

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
                if (CurrentText >= TextList.Count)
                { DialogueBox.transform.parent.gameObject.SetActive(false); return; }
                StartCoroutine(WriteText(TextList[CurrentText]));
            }
        }
    }


    IEnumerator WriteText(string Text)
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




}

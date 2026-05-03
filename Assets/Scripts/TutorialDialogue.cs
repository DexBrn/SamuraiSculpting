using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using System.Text.RegularExpressions;

public class TutorialDialogue : MonoBehaviour
{
    LevelManager LevelManager;
    public List<string> TutorialText = new List<string>();
    public TMP_Text DialogueBox;
    public float LetterSpeed;
    bool WritingText;

    int TutorialIndex;

    public bool DoTutorial = true;
    bool StartedTutorial = false;

    void Start()
    {
        LevelManager = GetComponent<LevelManager>();
        DialogueBox.transform.parent.gameObject.SetActive(false);

        ColourTutorialText();
    }


    void Update()
    {

        if (DoTutorial && GameObject.Find("Sword") && StartedTutorial == false)
        {
            StartedTutorial = true;
            DialogueBox.transform.parent.gameObject.SetActive(true);
            if (LevelManager.CurrentLevel == 0)
                StartCoroutine(WriteText(TutorialText[TutorialIndex]));
            else if (LevelManager.CurrentLevel == 1)
            { TutorialIndex = 4; StartCoroutine(WriteText(TutorialText[TutorialIndex])); }
            else
                DialogueBox.transform.parent.gameObject.SetActive(false);
        }

        if (LevelManager.CurrentLevel > 1)
            TutorialIndex = 0;

        //Conditions for each tutorial continuation
        if (TutorialIndex == 0 && Input.GetMouseButtonDown(0) && !WritingText)
        { TutorialIndex = 1; StartCoroutine(WriteText(TutorialText[TutorialIndex])); };

        if (TutorialIndex == 1 && GameObject.Find("Sword").transform.rotation != Quaternion.identity && Input.GetMouseButtonDown(0) && !WritingText)
        { TutorialIndex = 2; StartCoroutine(WriteText(TutorialText[TutorialIndex])); };

        if (TutorialIndex == 2 && Input.GetKeyDown(KeyCode.Return) && !WritingText)
        { TutorialIndex = 3; DialogueBox.transform.parent.gameObject.SetActive(false); };

        if (TutorialIndex == 3 && LevelManager.CurrentTargetIndex == 3 && !WritingText)
        { WritingText = true; DialogueBox.transform.parent.gameObject.SetActive(true); StartCoroutine(WriteText(TutorialText[TutorialIndex])); TutorialIndex = 4; };

        if (TutorialIndex == 4 && !WritingText && LevelManager.CurrentLevel == 0)
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E))
                DialogueBox.transform.parent.gameObject.SetActive(false);

        if (TutorialIndex == 4 && Input.GetKeyDown(KeyCode.Alpha2) && !WritingText)
        { TutorialIndex = 5; StartCoroutine(WriteText(TutorialText[TutorialIndex])); };

        if (TutorialIndex == 5 && Input.GetMouseButtonUp(0) && !WritingText)
        { TutorialIndex = 6; StartCoroutine(WriteText(TutorialText[TutorialIndex])); };

        if (TutorialIndex == 6 && Input.GetMouseButtonUp(0) && !WritingText)
        { TutorialIndex = 7; StartCoroutine(WriteText(TutorialText[TutorialIndex])); };

        if (TutorialIndex == 7 && Input.GetKeyDown(KeyCode.Alpha1) && !WritingText)
        { TutorialIndex = 8; StartCoroutine(WriteText(TutorialText[TutorialIndex])); };

        if (TutorialIndex == 8 && !WritingText)
            if (Input.GetKeyDown(KeyCode.H) || Input.GetKeyDown(KeyCode.R))
                { DialogueBox.transform.parent.gameObject.SetActive(false); };

    }

    public IEnumerator WriteText(string Text)
    {
        DialogueBox.text = "";
        WritingText = true;
        for (int i = 0; i < Text.Length; i++)
        {
            if (Text[i] == '<')
            {
                int ClosingIndex = Text.IndexOf('>', i);

                if (ClosingIndex != -1)
                {
                    string Tag = Text.Substring(i, ClosingIndex - i + 1);
                    DialogueBox.text += Tag;
                    i = ClosingIndex; // Skip to end of tag
                    continue;
                }
            }
            yield return new WaitForSeconds(LetterSpeed);
            if (!WritingText)
                break;
            DialogueBox.text += Text[i];

        }
        WritingText = false;
    }

    void ColourTutorialText()
    {
        for (int i = 0; i < TutorialText.Count; i++)
        {
            TutorialText[i] = TutorialText[i].Replace("KATANA", $"<color=#ffe48a>KATANA</color>");
            TutorialText[i] = TutorialText[i].Replace("TANTO", $"<color=#ffe48a>TANTO</color>");
            TutorialText[i] = TutorialText[i].Replace("TANTO", $"<color=#ffe48a>TANTO</color>");
            TutorialText[i] = TutorialText[i].Replace("BLUE TARGET", $"<color=#8af7ff>BLUE TARGET</color>");
            TutorialText[i] = Regex.Replace(TutorialText[i], @"\[(.*?)\]", "<color=#90EE90>[$1]</color>");





        }
            
    }
}

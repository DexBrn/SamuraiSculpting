using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Level", menuName = "Level")]
public class LevelObject : ScriptableObject
{

    [Header("Generic Info")]
    public float LevelID;
    public string Name;
    public string Description;
    public List<GameObject> TargetSculptures = new List<GameObject>();

    [Header("Grade Boundaries")]
    public List<float> Accuracy = new List<float>(); //0-S, 1-A, 2-B, 3-C, 4-D
    public List<float> TimeTaken = new List<float>(); //0-S, 1-A, 2-B, 3-C, 4-D
    public List<float> Cuts = new List<float>(); //0-S, 1-A, 2-B, 3-C, 4-D

    [Header("Dialogue")]
    public List<string> PreLevelDialogue = new List<string>();


}

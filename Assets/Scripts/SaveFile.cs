using System;
using UnityEngine;

[Serializable]
public class SaveFile
{

    public int[] AllAchievedGrades = new int[48];

    public const string PlayerPrefsKeyName = "SaveFile";

    public void SaveToPlayerPrefs()
    {
        //convert Save File to json
        string json = JsonUtility.ToJson(this);

        //save json to player prefs
        PlayerPrefs.SetString(PlayerPrefsKeyName, json);
        PlayerPrefs.Save();
    }



    public static SaveFile CreateFromPlayerPrefs()
    {

        if (!PlayerPrefs.HasKey(PlayerPrefsKeyName)) //If key doesn't exist then cant create lol
            return null;

        //retrieve json from player prefs
        string json = PlayerPrefs.GetString(PlayerPrefsKeyName);

        //Deserialise the json string into a new savefile object and RETURN it
        return JsonUtility.FromJson<SaveFile>(json);
    }



}

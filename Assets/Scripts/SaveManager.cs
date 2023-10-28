using UnityEngine;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class SaveData
{
    public int storyChapter;
    public float highScore;

    public SaveData()
    {
        storyChapter = 1;
        highScore = 0;
    }
}

public static class SaveManager
{
    private static bool loaded = false;

    public static void SaveData()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string playerSavePath = Application.persistentDataPath + "/save.xd";
        FileStream stream = new FileStream(playerSavePath, FileMode.Create);
        Debug.Log("Saving !");

        formatter.Serialize(stream, GameManager.savedata);
        stream.Close();
    }

    public static void LoadData()
    {
        if (!loaded)
        {
            string playerSavePath = Application.persistentDataPath + "/save.xd";
            if (File.Exists(playerSavePath))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(playerSavePath, FileMode.Open);

                try
                {
                    GameManager.savedata = (SaveData)formatter.Deserialize(stream);
                    Debug.Log("Loaded !");
                }
                catch (SerializationException e)
                {
                    Debug.LogError("Error loading save: " + e.StackTrace);
                    GameManager.savedata = new SaveData();
                }
                stream.Close();
            }
            else
            {
                Debug.LogError("File not found: " + playerSavePath);
                GameManager.savedata = new SaveData();
            }
            loaded = true;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

public class SaveManager : MonoBehaviour
{
    public void saveGame()
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream file = new FileStream(Application.persistentDataPath + "/SpaceGameSave" + DateTime.Now.Month + "_" + DateTime.Now.Day + "_"
            + DateTime.Now.Year + "_"  + DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + DateTime.Now.Second + ".sgs", FileMode.Create);

        SaveData saveInformation = new SaveData();

        GameController gameController = GetComponent<GameController>();

        saveInformation.seed = FindObjectOfType<MapGenerator>().seed;
        saveInformation.turnCount = gameController.turnCount;
        saveInformation.whoseTurn = gameController.whoseTurn;

        int[,] tempControlledTiles;

        saveInformation.players = new (int, int, int, int, int[,])[gameController.players.Length];
        for (int i = 0; i < gameController.players.Length; i++)
        {
            tempControlledTiles = new int[gameController.players[i].controlledTiles.Count, 3];
            for (int j = 0; j < gameController.players[i].controlledTiles.Count; j++)
            {
                tempControlledTiles[j, 0] = gameController.players[i].controlledTiles[j].x;
                tempControlledTiles[j,1] = gameController.players[i].controlledTiles[j].y;
                tempControlledTiles[j, 2] = gameController.players[i].controlledTiles[j].z;
            }
            saveInformation.players[i] = (gameController.players[i].shipLimit, 
                gameController.players[i].fuel, 
                gameController.players[i].ore, 
                gameController.players[i].uranium, 
                tempControlledTiles);
        }

        List<Ship>[] ships = gameController.allShips;
        List<Ship> tempShips = new List<Ship>();

        for (int i = 0; i < ships.Length; i++)
        {
            for (int j = 0; j < ships[i].Count; j++)
            {
                tempShips.Add(ships[i][j]);
            }
        }
        Ship[] tempShipArray = tempShips.ToArray();
        saveInformation.ships = new (int, int, int, int, bool, int[])[tempShipArray.Length];
        for (int i = 0; i < tempShipArray.Length; i++)
        {
            saveInformation.ships[i] = (tempShipArray[i].type, 
                tempShipArray[i].team, 
                tempShipArray[i].attack,
                tempShipArray[i].health, 
                tempShipArray[i].canAct, 
                new int[] { tempShipArray[i].location.x, tempShipArray[i].location.y, tempShipArray[i].location.z });
        }
        binaryFormatter.Serialize(file, saveInformation);
        file.Close();
        StartCoroutine(FindObjectOfType<UIControl>().alertMessage("Game saved!"));
    }

    public static SaveData loadGame(string path)
    {
        if(File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            SaveData saveInformation = formatter.Deserialize(stream) as SaveData;
            return saveInformation;
        }
        else
        {
            Debug.LogError("File not found @ " + path);
            return null;
        }
    }

    public static string[] getSaveFilePaths()
    {
        string[] filePaths = Directory.GetFiles(@Application.persistentDataPath, "*.sgs");
        return filePaths;
    }
}

[Serializable]
public class SaveData
{
    public int seed;
    public int turnCount;
    public int whoseTurn;
    /*player data such as amount resources, controlled ships, and controlled tiles
    Vector3Int is represented by an int array, [x,y,z]
    produce, fuel, ore, uranium, controlledTiles, */
    public (int, int, int, int, int[,])[] players;
    //locations of all the ships
    public (int, int, int, int, bool, int[])[] ships;
}


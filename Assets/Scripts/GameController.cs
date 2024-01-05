using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameController : MonoBehaviour
{
    [SerializeField] CameraController cameraController;

    public int numPlayers;
    public Player[] players;
    public int turnCount;
    public int turnPhase;
    public int whoseTurn;
    Vector3 CENTER;

    [SerializeField]
    MapGenerator mapGenerator;
    public TileData[,] tileArray;
    [SerializeField] Tilemap tilemap;
    private bool coroutineActive;
    public bool readyToMoveOn = false;
    public List<Ship>[] allShips;
    public List<TileData> toReset;
    public List<Ship> shipsToReset;
    public List<Ship> activeShips;

    private void Awake()
    {
        numPlayers = 2;
        turnCount = 1;
        turnPhase = 1;
        whoseTurn = 0;
    }

    public void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        tileArray = mapGenerator.getAllTiles();
        allShips = new List<Ship>[numPlayers];
        for (int i = 0; i < numPlayers; i++)
        {
            allShips[i] = new List<Ship>();
        }
        toReset = new List<TileData>();
        shipsToReset = new List<Ship>();
        activeShips = new List<Ship>();
        players = new Player[numPlayers];
        CENTER = tilemap.CellToWorld(new Vector3Int(mapGenerator.xsize/2, mapGenerator.ysize/2));
        for (int i = 0; i < numPlayers; i++)
        {
            players[i] = new Player();
            if (i == 0)
            {
                players[i].AddTerritory(new Vector3Int(0, 0));
            }
            else
                players[i].AddTerritory(new Vector3Int(mapGenerator.xsize - 1, mapGenerator.ysize - 1));
        }
    }
    private void Update()
    {
        Turn();
    }
    void Turn()
    {
        if (!coroutineActive){
            StartCoroutine("phase1");
        }
    }

    IEnumerator phase1()
    {
        AddPassiveResources();
        
        for (int x = 0; x < players.Length; x++)
        {
            CalculateMaxShipCapacity(players[x]);
        }

        bool endGame = CheckWinCondition();
        if(endGame) yield break;
        
        coroutineActive = true;
        
        yield return new WaitUntil(GetReadyToMoveOn);
        
        readyToMoveOn = false;
        coroutineActive = false;
        
        ResetShips();

        if (whoseTurn < players.Length - 1)
        {
            whoseTurn++;
        }
        else
        {
            turnCount++;
            whoseTurn = 0;
        }
        Vector3 teamCameraLocation = tilemap.CellToWorld(players[whoseTurn].controlledTiles[0]);
        cameraController.MoveCamera(new Vector3(teamCameraLocation.x, 3, teamCameraLocation.z), CENTER);
    }

    public void SetReadyToMoveOn()
    {
        readyToMoveOn = true;
    }

    private bool GetReadyToMoveOn()
    {
        return readyToMoveOn;
    }

    private void ResetShips()
    {
        for (int i = 0; i < toReset.Count; i++)
        {
            TileData x = toReset[i];
            x.setExtracted(false);
        }
        for (int i = 0; i < shipsToReset.Count; i++)
        {
            Ship x = shipsToReset[i];
            x.canAct = true;
        }
        shipsToReset.Clear();
        toReset.Clear();
    }

    private void AddPassiveResources()
    {
        tileArray = FindObjectOfType<MapGenerator>().getAllTiles();
        List<Vector3Int> controlledTiles = players[whoseTurn].controlledTiles;
        for (int x = 0; x < controlledTiles.Count; x++)
        {
            TileData data = tileArray[controlledTiles[x].x, controlledTiles[x].y];
            switch (data.planetType)
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    players[whoseTurn].fuel += (int)(data.resourcePerTurn * 0.2);
                    break;
                case 3:
                    players[whoseTurn].ore += (int)(data.resourcePerTurn * 0.2);
                    break;
                case 4:
                    players[whoseTurn].uranium += (int)(data.resourcePerTurn * 0.2);
                    break;
            }
        }
    }

    private void CalculateMaxShipCapacity(Player player)
    {
        tileArray = FindObjectOfType<MapGenerator>().getAllTiles();
        int count = 0;
        for(int x = 0; x < player.controlledTiles.Count; x++)
        {
            if(tileArray[player.controlledTiles[x].x, player.controlledTiles[x].y].planetType == 1)
            {
                count++;
            }
        }
        player.shipLimit = count;
    }

    //returns 0 for no winner, 1 or 2 if winner exists
    public bool CheckWinCondition()
    {
        bool end = false;
        if (Math.Abs(players[0].controlledTiles.Count - players[1].controlledTiles.Count) >= 5)
        {
            end = true;
            string winner = "";
            if(players[0].controlledTiles.Count > players[1].controlledTiles.Count){
                winner = "Player 1";
            }
            else{
                winner = "Player 2";
            }
            StartCoroutine(FindObjectOfType<UIControl>().alertMessage(winner + " has won the game. Press quit to return to the loading screen."));
        }

        return end;
    }

}

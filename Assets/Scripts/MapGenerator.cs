using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class MapGenerator : MonoBehaviour
{
    Tilemap tm;
    public Tile tile1;
    public Tile tile2;
    public int multiplier;
    public int seed;
    public int xsize;
    public int ysize;
    public float threshold = 0.5f;

    PlanetGenerator pg;
    public TileData[,] allTiles;

    [HideInInspector] public static System.Random random;

    SettingsManager settingsManager;
    GameController gameController;
    public SaveData saveInformation;

    public int getYSize() { return ysize; }
    public int getXSize() { return xsize; }
    public void Start()
    {
        gameController = FindObjectOfType<GameController>();
        settingsManager = FindObjectOfType<SettingsManager>();
        if(settingsManager.loadFile.Equals(""))
        {
            GenerateMap();
        }
        else
        {
            saveInformation = SaveManager.loadGame(settingsManager.loadFile);
            loadGameFromSaveData(saveInformation);
        }
    }

    public void GenerateMap()
    {
        random = new System.Random(seed);
        tm = GetComponent<Tilemap>();
        pg = GetComponent<PlanetGenerator>();
        allTiles = new TileData[xsize, ysize];
        GameObject parent = GameObject.Find("Planets");

        List<Vector3Int> alreadyBeen = new List<Vector3Int>();
        for (int x = 0; x < xsize; x++)
        {
            for (int y = 0; y < ysize; y++)
            {
                if ((x == 0 && y == 0) || (x == xsize - 1 && y == ysize - 1))
                {
                    tm.SetTile(new Vector3Int(x, y, 0), tile2);
                    GameObject planet = pg.GenerateNewPlanet(1, new Vector3Int(x, y, 0));
                    planet.transform.SetPositionAndRotation(tm.CellToWorld(new Vector3Int(x, y, 0)), Quaternion.identity);
                    planet.transform.Translate(new Vector3(0, 0.2f, 0));
                    planet.transform.parent = parent.transform;
                    allTiles[x, y] = new TileData(tile2, new Vector3Int(x, y, 0), planet.GetComponent<Planet>(), true);
                    alreadyBeen.Add(new Vector3Int(x, y));
                }
                else
                {
                    if (Mathf.PerlinNoise((x + seed) * multiplier / 7f, y * 10 / 7f) > threshold)
                    {
                        tm.SetTile(new Vector3Int(x, y, 0), tile1);
                        allTiles[x, y] = new TileData(tile1, new Vector3Int(x, y, 0), false);
                    }else
                    {
                        tm.SetTile(new Vector3Int(x, y, 0), tile2);
                        allTiles[x, y] = new TileData(tile2, new Vector3Int(x, y, 0), true);
                    }
                }
            }
        }
        int iterate = 0;
        for(int x = 0; x < 2; x ++)
        {
            for(int y = 0; y < 2; y ++)
            {

                for (int i = 1; i <= 4; i++)
                {
                    int xcoord, ycoord;
                    do
                    {
                        if (x == 0)
                        {
                            xcoord = random.Next(0, xsize / 3 + 1);
                        }
                        else
                        {
                            xcoord = random.Next(xsize  - xsize / 3, xsize);
                        }

                        if (y == 0)
                        {
                            ycoord = random.Next(0, ysize / 3 + 1);
                        }
                        else
                        {
                            ycoord = random.Next(ysize - ysize/ 3, ysize);
                        }

                        if (iterate > 100) break;
                        iterate++;
                    }
                    while (alreadyBeen.Contains(new Vector3Int(xcoord, ycoord)) || !allTiles[xcoord,ycoord].isBlue || allTiles[xcoord, ycoord].planetType != 0);

                    GameObject planet = pg.GenerateNewPlanet(i, new Vector3Int(xcoord, ycoord));
                    allTiles[xcoord, ycoord] = new TileData(tile2, new Vector3Int(xcoord, ycoord, 0), planet.GetComponent<Planet>(), true);
                    planet.transform.SetPositionAndRotation(tm.CellToWorld(new Vector3Int(xcoord, ycoord, 0)), Quaternion.identity);
                    planet.transform.Translate(new Vector3(0, 0.2f, 0));
                    planet.transform.parent = parent.transform;
                }
            }
        }
        for(int x = 0; x < xsize; x++)
        {
            for(int y = 0; y < ysize; y++)
            {
                if(allTiles[x,y].planetType == 0 && allTiles[x,y].isBlue)
                {
                    double val = random.NextDouble();
                    if(val < 0.33)
                    {
                        GameObject planet = pg.GenerateNewPlanet(random.Next(1,5), new Vector3Int(x, y));
                        allTiles[x, y] = new TileData(tile2, new Vector3Int(x, y, 0), planet.GetComponent<Planet>(), true);
                        planet.transform.SetPositionAndRotation(tm.CellToWorld(new Vector3Int(x, y, 0)), Quaternion.identity);
                        planet.transform.Translate(new Vector3(0, 0.2f, 0));
                        planet.transform.parent = parent.transform;
                    }
                }
            }
        }
    }

    public void oldGenerateMap()
    {
        int seed = (int)Random.Range(-10000f, 10000f);
        tm = GetComponent<Tilemap>();
        pg = GetComponent<PlanetGenerator>();
        allTiles = new TileData[xsize, ysize];

        /* GameObject sun = pg.GenerateSun();
         Vector3 pos1 = tm.CellToWorld(new Vector3Int(xsize / 2, (ysize / 2), 2));
         Vector3 pos2 = tm.CellToWorld(new Vector3Int(xsize / 2 - 1, (ysize / 2) + 1, 0));
         Vector3 finalPos = (pos1 + pos2) / 2;
         sun.transform.SetPositionAndRotation(pos1, Quaternion.identity);*/

        GameObject parent = GameObject.Find("Planets");

        for (int x = 0; x < xsize; x++)
        {
            for (int y = 0; y < ysize; y++)
            {
                if ((x == 0 && y == 0) || (x == xsize - 1 && y == ysize - 1))
                {
                    tm.SetTile(new Vector3Int(x, y, 0), tile2);
                    GameObject planet = null;
                    planet = pg.GenerateNewPlanet(1, new Vector3Int(x, y, 0));
                    planet.transform.SetPositionAndRotation(tm.CellToWorld(new Vector3Int(x, y, 0)), Quaternion.identity);
                    planet.transform.Translate(new Vector3(0, 0.2f, 0));
                    planet.transform.parent = parent.transform;
                    allTiles[x, y] = new TileData(tile2, new Vector3Int(x, y, 0), planet.GetComponent<Planet>());
                }
                else
                {
                    if (Mathf.PerlinNoise(((x + seed) * multiplier) / 7f, (y * 10) / 7f) > 0.6f)
                    {

                        tm.SetTile(new Vector3Int(x, y, 0), tile1);
                        allTiles[x, y] = new TileData(tile1, new Vector3Int(x, y, 0), false);
                    }
                    else
                    {
                        tm.SetTile(new Vector3Int(x, y, 0), tile2);
                        if (Random.Range(0f, 1f) > 0.5f)
                        {
                            GameObject planet = null;
                            //Random.Range(0.5f, 1);
                            float randomVal = Random.Range(0f, 1f);
                            if (randomVal < 0.4f)
                            {
                                planet = pg.GenerateNewPlanet(1, new Vector3Int(x, y, 0));
                            }
                            if (randomVal > 0.4f && randomVal < 0.55f)
                            {
                                planet = pg.GenerateNewPlanet(2, new Vector3Int(x, y, 0));
                            }
                            if (randomVal > 0.55f && randomVal < 0.8f)
                            {
                                planet = pg.GenerateNewPlanet(3, new Vector3Int(x, y, 0));
                            }
                            if (randomVal > 0.8f)
                            {
                                planet = pg.GenerateNewPlanet(4, new Vector3Int(x, y, 0));
                            }

                            allTiles[x, y] = new TileData(tile2, new Vector3Int(x, y, 0), planet.GetComponent<Planet>());
                            planet.transform.SetPositionAndRotation(tm.CellToWorld(new Vector3Int(x, y, 0)), Quaternion.identity);
                            planet.transform.Translate(new Vector3(0, 0.2f, 0));
                            planet.transform.parent = parent.transform;
                        }
                        else
                        {
                            allTiles[x, y] = new TileData(tile2, new Vector3Int(x, y, 0), true);
                        }
                    }
                }
            }
        }
    }

    void loadGameFromSaveData(SaveData saveInformation)
    {
        //TODO make camera position match whose turn it is
        UIControl UI = FindObjectOfType<UIControl>();
        UI.root.Q<Label>("turn").text = "Turn: " + saveInformation.turnCount + ", Player " + (saveInformation.whoseTurn + 1) + "'s Turn";
        gameController.turnCount = saveInformation.turnCount;
        gameController.whoseTurn = saveInformation.whoseTurn;
        seed = saveInformation.seed;
        GenerateMap();
        var players = saveInformation.players;
        gameController.numPlayers = players.Length;
        for(int i = 0; i < players.Length; i++)
        {
            Debug.Log(players[i].Item1);
            gameController.players[i].shipLimit = players[i].Item1;
            gameController.players[i].fuel = players[i].Item2;
            gameController.players[i].ore = players[i].Item3;
            gameController.players[i].uranium = players[i].Item4;
            List<Vector3Int> controlledTiles = new();
            for (int j = 0; j < players[i].Item5.GetLength(0); j++)
            {
                Vector3Int tempLocation = new(players[i].Item5[j, 0], players[i].Item5[j, 1], players[i].Item5[j, 2]);
                controlledTiles.Add(tempLocation);
            }
            gameController.players[i].controlledTiles = controlledTiles;
        }

        GameObject parent = GameObject.Find("Ships");
        var ships = saveInformation.ships;
        for (int j = 0; j < ships.GetLength(0); j++)
        {
            int type = ships[j].Item1;
            int team = ships[j].Item2;
            int health = ships[j].Item4;
            bool canAct = ships[j].Item5;
            Vector3Int tempLocation = new(ships[j].Item6[0], ships[j].Item6[1], ships[j].Item6[2]);
            GameObject ship = null;
            switch (type)
            {
                case 0:
                    ship = UI.explorer;
                    break;
                case 1:
                    ship = UI.flagship;
                    break;
                case 2:
                    ship = UI.miner;
                    break;
            }
            Vector3 spawnLocation = UI.tilemap.CellToWorld(tempLocation);
            spawnLocation.Set(spawnLocation.x, spawnLocation.y + 0.5f, spawnLocation.z);
            if (team == 0)
                ship = Instantiate(ship, spawnLocation, Quaternion.Euler(-90, -90, 0));
            else
                ship = Instantiate(ship, spawnLocation, Quaternion.Euler(-90, 90, 0));
            for (int x = 0; x < ship.GetComponent<Renderer>().materials.Length; x++)
            {
                ship.GetComponent<Renderer>().materials[x].color = UI.teamColors[team];
            }
            ship.transform.parent = parent.transform;
            
            Ship shipComp = ship.AddComponent<Ship>();
            shipComp.Initialize(tempLocation, team, gameController, type);
            shipComp.health = health;
            shipComp.canAct = canAct;
            if (!canAct) gameController.shipsToReset.Add(shipComp);
            gameController.players[team].controlledShips.Add(shipComp);
            allTiles[tempLocation.x, tempLocation.y].setShip(shipComp);
            gameController.activeShips.Add(shipComp);

        }
    }

    public TileData[,] getAllTiles()
    {
        return allTiles;
    }
}

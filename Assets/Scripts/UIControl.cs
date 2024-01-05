
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using System;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Serialization;

public class UIControl : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;

    //Tile and Tilemap management
    [SerializeField]
    public Tilemap tilemap;
    [SerializeField]
    private Tile clickedTile;
    [SerializeField]
    Tile[] teamTiles;
    [SerializeField]
    Tilemap teamTilemap;
    public TileData[,] tileArray;
    private bool waitingForNewTile; //used for movement
    private bool waitingForAttack; //used for attack
    private TileData attackFrom;
    private Vector3Int poppedLoc;
    private TileData toMove;
    private Stack<Vector3Int> selectedTiles;
    private List<Vector3Int> tilesToDeselect;
    private DropdownField selectShip;
    [SerializeField] GameObject explosionEffect;

    //UI element
    public VisualElement root;

    [SerializeField] TextMeshProUGUI alertText;
    float fadeTime = 2f;
    bool alertStarted = false;

    private GameController gc;
    private MapGenerator mg;
    
    [SerializeField] public GameObject explorer, teamSpaceship2, flagship, miner;
    [SerializeField] public Color[] teamColors;

    private bool highlightStarted;
    private bool rotationStarted = false;

    [SerializeField] GameObject gameContainer;
    public static GameObject mainGameController { get; private set; }
    public static VisualElement mainRoot { get; private set; }
    public static UIControl mainUI { get; private set; }

    // Start is called before the first frame update

    void Awake()
    {
        mainGameController = gameContainer;
        mainUI = this;
    }

    void Start()
    {
        selectedTiles = new Stack<Vector3Int>(); //Create stack for user selected tiles
        tilesToDeselect = new List<Vector3Int>();
        gc = FindObjectOfType<GameController>();//Grab reference to game controller object
        mg = FindObjectOfType<MapGenerator>();//Grab reference to map generator object
        Buttons();//Run code for buttons
        Label turn = root.Q<Label>("turn");//Setup turn info
        turn.text = "Turn: 1, Player 1's Turn";
        waitingForNewTile = false;
        waitingForAttack = false;
    }

    // Update is called once per frame
    void Update()
    {
        checkClickOnTile();
        UpdateTileDisplayInformation();
        updateShipDisplayInformation();
        UpdateTeamTilesDisplayed();
        UpdateTeamResourceCount();
        if (waitingForNewTile)
        {
            move();
        }
        if (waitingForAttack)
        {
            attack();
        }
    }

    private void Buttons()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        mainRoot = root;
        Button nextTurn = root.Q<Button>("nextTurn");
        Button generate = root.Q<Button>("genShip");
        Button moveShip = root.Q<Button>("moveTo");
        DropdownField chooseShip = root.Q<DropdownField>("selectShip");
        Button manual = root.Q<Button>("manual");
        Button extractResource = root.Q<Button>("extractResource");
        Button extractAll = root.Q<Button>("extractAll");
        Button establishControl = root.Q<Button>("establishControl");
        Button nextShip = root.Q<Button>("nextShip");
        Button fire = root.Q<Button>("fire");
        Button quit = root.Q<Button>("quit");
        Button save = root.Q<Button>("Save");
        selectShip = root.Q<DropdownField>("selectShip");
        fire.clicked += attack;
        nextShip.clicked += findNextShip;
        generate.clicked += Gen;
        nextTurn.clicked += UpdateTurnInformation;
        moveShip.clicked += move;
        manual.clicked += LoadManual;
        extractResource.clicked += extract;
        extractAll.clicked += ExtractAll;
        establishControl.clicked += takeControl;
        quit.clicked += quitToMenu;
        save.clicked += SaveGame;
    }

    void SaveGame()
    {
        FindObjectOfType<SaveManager>().saveGame(); 
    }

    void LoadManual()
    {
        gameContainer.SetActive(false);
        SceneManager.LoadScene(3, LoadSceneMode.Additive);
        DisableAll();
    }

    private void quitToMenu()
    {
        SceneManager.LoadScene("StartScreen");
    }

    private void findNextShip()
    {
        List<Ship> actableShips = new List<Ship>();
        foreach (Ship ship in gc.allShips[gc.whoseTurn])//get all ships which need to be give actions
        {
            if (ship.canAct)
            {
                actableShips.Add(ship);
            }
        }
        if (!highlightStarted)//check if the highlight coroutine has already been activated, if not then start the highlight effect
        {
            foreach (Ship ship in actableShips)//start effect for all ships which need actions
            {
                StartCoroutine(highlightShip(ship.gameObject));
            }
        }
        if (actableShips.Count > 0) //if there is at least one ship which needs to be given an action
        {
            //code taken straight from check click on tile
            Vector3Int cellCoords = actableShips[0].location;
            highlightTile(cellCoords);
        }
        else// all ships given actions or no ships at all
        {
            if (!alertStarted) StartCoroutine(alertMessage("No possible ships to act with"));
            Debug.Log("No possible ships to act with");
            return;
        }
    }

    IEnumerator highlightShip(GameObject ship)
    {
        highlightStarted = true;
        Material[] materials = ship.GetComponent<Renderer>().materials;
        Renderer shipRenderer = ship.GetComponent<Renderer>();
        Color originalColor = shipRenderer.material.color;
        float highlightTime = 5f;
        float smoothness = 0.1f;
        float count = smoothness;
        while (count < highlightTime)
        {
            for (int i = 0; i < materials.Length; i++)
            {
                shipRenderer.materials[i].color = Color.Lerp(Color.red, originalColor, count);
            }
            yield return new WaitForSeconds(smoothness);
            count++;
        }
        for (int i = 0; i < materials.Length; i++)
        {
            shipRenderer.materials[i].color = originalColor;
        }
        highlightStarted = false;
    }

    public IEnumerator alertMessage(string message)
    {
        alertStarted = true;

        alertText.text = message;
        alertText.color = new Color(alertText.color.r, alertText.color.g, alertText.color.b, 1);
        yield return new WaitForSeconds(fadeTime / 2);
        while (alertText.color.a > 0.0f)
        {
            alertText.color = new Color(alertText.color.r, alertText.color.g, alertText.color.b, alertText.color.a - (Time.deltaTime / (fadeTime / 2)));
            yield return null;
        }

        alertStarted = false;
    }

    private void takeControl()
    {
        poppedLoc = selectedTiles.Peek();
        TileData x = tileArray[poppedLoc.x, poppedLoc.y];
        Planet targetPlanet = x.getPlanet();
        Ship ship = x.getShip();
        if (ship == null || targetPlanet == null || gc.players[gc.whoseTurn].controlledTiles.Contains(poppedLoc))
        {
            return; // can't take control without a ship, planet, or if you already control it
        }
        gc.players[gc.whoseTurn].controlledTiles.Add(poppedLoc);
        if(gc.whoseTurn == 0){
            gc.players[1].controlledTiles.Remove(poppedLoc);
        }
        else{
            gc.players[0].controlledTiles.Remove(poppedLoc);
        }
        gc.shipsToReset.Add(ship);
    }

    private void extract()
    {
        poppedLoc = selectedTiles.Peek();
        TileData x = tileArray[poppedLoc.x, poppedLoc.y];
        Ship ship = x.getShip();
        if (ship == null) { return; }
        Planet targetPlanet = x.getPlanet();
        if (!ship.canAct || targetPlanet == null || x.getExtracted() || !gc.players[gc.whoseTurn].controlledTiles.Contains(poppedLoc))
        {
            // can't extract without a miner sorry broski
            if (!ship.canAct)
            {
                if (!alertStarted) StartCoroutine(alertMessage("Ship has already acted this turn"));
                Debug.Log("Ship has already acted this turn.");
            }
            else
            {
                if (!alertStarted) StartCoroutine(alertMessage("Can't exract"));
                Debug.Log("Can't extract.");
            }
            return;
        }
        int Extractable = x.getResourcePerTurn();
        //check if ship is a miner, if so make it extract more
        if (ship.type == 2)
        {
            Extractable = (int)(Extractable * 1.2);
        }
        else{
            Extractable = (int)(Extractable * 0.8);
        }
        string resource = planetTypeToResource(x.getPlanetType());
        switch (resource)
        {
            case "Fuel":
                gc.players[gc.whoseTurn].setFuel(gc.players[gc.whoseTurn].getFuel() + Extractable);
                break;
            case "Ore":
                gc.players[gc.whoseTurn].setOre(gc.players[gc.whoseTurn].getOre() + Extractable);
                break;
            case "Uranium":
                gc.players[gc.whoseTurn].setUranium(gc.players[gc.whoseTurn].getUranium() + Extractable);
                break;
            case "Produce":
                if (!alertStarted) StartCoroutine(alertMessage("I hate urav"));
                Debug.Log("I hate urav");
                break;
        }
        gc.toReset.Add(x);
        x.setExtracted(true);
        ship.canAct = false;
        gc.shipsToReset.Add(ship);

    }

    private int calculateFuelCost(Vector3Int destination, Vector3Int location, double power)
    {
        Debug.Log(Math.Pow(Utilities.betterComputeDistanceHexGrid(destination, location, tilemap) + 4, power) + 6);
        return (int)Math.Pow(Utilities.betterComputeDistanceHexGrid(destination, location, tilemap) + 4, power) + 6;
    }

    private int calculateAttackCost(Vector3Int destination, Vector3Int location)
    {
        Debug.Log((int)Math.Pow(Utilities.betterComputeDistanceHexGrid(destination, location, tilemap) + 1, 2.4) + 10);
        return (int)Math.Pow(Utilities.betterComputeDistanceHexGrid(destination, location, tilemap) + 1, 2.4) + 10;
    }

    private void attack()
    {
        if (!waitingForAttack)
        {
            poppedLoc = selectedTiles.Peek();
            attackFrom = tileArray[poppedLoc.x, poppedLoc.y];
            Ship attacker = attackFrom.getShip();
            if (attacker == null || attacker.getTeam() != gc.whoseTurn || !attacker.canAct)
            {
                Debug.Log("hi");
                return;
            }
        }
        waitingForAttack = true;
        Vector3Int target = selectedTiles.Peek();
        int uraniumRequired;
        if (!target.Equals(poppedLoc))
        {
            if(attackFrom.getShip().type == 1){
                uraniumRequired = calculateAttackCost(target, attackFrom.getShip().location) + 10;
            }
            else{
                uraniumRequired = calculateAttackCost(target, attackFrom.getShip().location);
            }
            if (gc.players[gc.whoseTurn].uranium < uraniumRequired)
            {
                if (!alertStarted) StartCoroutine(alertMessage("Not enough uranium to move. Uranium required is " + uraniumRequired + ", and you have " + gc.players[gc.whoseTurn].uranium + " uranium."));
                Debug.Log("Not enough uranium to move. Uranium required is " + uraniumRequired + ", and you have " + gc.players[gc.whoseTurn].uranium + " uranium.");
                waitingForAttack = false;
                return;
            }
            if (tileArray[target.x, target.y].getShip() != null && tileArray[target.x, target.y].getShip().team == gc.whoseTurn)
            {//check if destination already has ship
                if (!alertStarted) StartCoroutine(alertMessage("Destination has an allied ship"));
                Debug.Log("Destination has an allied ship.");
                waitingForAttack = false;
                return;
            }
            Ship attacker = attackFrom.getShip();
            Ship victim = tileArray[target.x, target.y].getShip();
            gc.shipsToReset.Add(attacker);
            attacker.canAct = false;
            if (victim != null)
            {
                System.Random rnd = new System.Random();
                victim.health = (rnd.Next() < 0) ? (victim.health - attacker.attack + rnd.Next(5)) : (victim.health - attacker.attack - rnd.Next(5));
                if (victim.health <= 0)
                {
                    gc.players[victim.team].controlledShips.Remove(victim);
                    gc.allShips[gc.whoseTurn].Remove(victim);
                    tileArray[victim.location.x, victim.location.y].ship = null;
                    Destroy(victim.gameObject);
                    Vector3 explosionLocation = tilemap.CellToWorld(victim.location);
                    Instantiate(explosionEffect, new Vector3(explosionLocation.x, explosionLocation.y + 0.5f, explosionLocation.z) , Quaternion.identity);
                }
                Debug.Log(victim.health);
            }
            waitingForAttack = false;
            gc.players[gc.whoseTurn].uranium -= uraniumRequired;
        }
    }

    private void move()
    {
        if (!waitingForNewTile)
        {
            poppedLoc = selectedTiles.Peek();
            toMove = tileArray[poppedLoc.x, poppedLoc.y];
            Ship ship = toMove.getShip().GetComponent<Ship>();
            if (toMove.getShip() == null || !ship.canAct || ship.getTeam() != gc.whoseTurn)
            {
                //inform player that there is no ship
                if (!ship.canAct)
                {
                    if (!alertStarted) StartCoroutine(alertMessage("Ship has already acted this turn"));
                    Debug.Log("Ship has already acted this turn.");
                }
                else
                {
                    if (!alertStarted) StartCoroutine(alertMessage("Can't move"));
                    Debug.Log("Can't move.");
                }
                return;
            }

        }
        waitingForNewTile = true;
        Vector3Int destination = selectedTiles.Peek();
        if (!destination.Equals(poppedLoc))
        {
            double power;
            switch (toMove.getShip().type)
            {
                case 0: //explorer
                    power = 1.5;
                    break;
                case 1: //flagship
                    power = 2.0;
                    break;
                case 2: //miner
                    power = 1.7;
                    break;
                default:
                    power = 0.0;
                    throw new Exception("invalid ship type");
            }
            int fuelRequired = calculateFuelCost(destination, toMove.getShip().GetComponent<Ship>().location, power);
            if (gc.players[gc.whoseTurn].fuel < fuelRequired)
            {
                if (!alertStarted) StartCoroutine(alertMessage("Not enough fuel to move. Fuel required is " + fuelRequired + ", and you have " + gc.players[gc.whoseTurn].fuel + " fuel."));
                Debug.Log("Not enough fuel to move. Fuel required is " + fuelRequired + ", and you have " + gc.players[gc.whoseTurn].fuel + " fuel.");
                waitingForNewTile = false;
                return;
            }
            if (tileArray[destination.x, destination.y].getShip() != null && tileArray[destination.x, destination.y].getShip().GetComponent<Ship>().team == gc.whoseTurn)
            {//check if destination already has ship
                if (!alertStarted) StartCoroutine(alertMessage("Destination already has an allied ship"));
                Debug.Log("Destination already has an allied ship.");
                waitingForNewTile = false;
                return;
            }
            Vector3 b = tilemap.CellToWorld(destination) + new Vector3();
            b.Set(b.x, b.y + 0.5f, b.z);
            Ship shipToMove = toMove.getShip();
            //shipToMove.transform.position = b;
            StartCoroutine(movingAnimation(shipToMove.gameObject, b));
            shipToMove.canAct = false;
            gc.shipsToReset.Add(shipToMove);
            tileArray[destination.x, destination.y].setShip(shipToMove);
            shipToMove.location = new Vector3Int(destination.x, destination.y);
            toMove.setShip(null);
            waitingForNewTile = false;
            gc.players[gc.whoseTurn].fuel -= fuelRequired;
        }

    }

    IEnumerator movingAnimation(GameObject obj, Vector3 newPosition)
    {
        float timeSinceStarted = 0f;
        //if(!rotationStarted)
        //{
        //    StartCoroutine(RotateTowards(obj, newPosition, 2));
        //}
        //yield return new WaitUntil(GetReadyToRotate);
        while (true)
        {
            timeSinceStarted += Time.deltaTime;
            obj.transform.position = Vector3.Lerp(obj.transform.position, newPosition, timeSinceStarted);

            // If the object has arrived, stop the coroutine
            if (obj.transform.position == newPosition)
            {
                yield break;
            }

            // Otherwise, continue next frame
            yield return null;
        }
    }

    IEnumerator RotateTowards(GameObject toRotate, Vector3 toRotateTo, float totalSeconds)
    {
        rotationStarted = true;
        float smoothness = 50;
        Vector2 tempVector = new Vector2(toRotateTo.x - toRotate.transform.position.x, toRotateTo.z - toRotate.transform.position.z);
        float angle = -90 + Mathf.Acos(tempVector.y / Mathf.Sqrt(Mathf.Pow(tempVector.x, 2) + Mathf.Pow(tempVector.y, 2))) * 180f / Mathf.PI;
        float originalRotation = toRotate.transform.eulerAngles.z;
        for (int x = 0; x < smoothness; x++)
        {
            toRotate.transform.eulerAngles = new Vector3(-90, toRotate.transform.eulerAngles.y, Mathf.Lerp(originalRotation, angle, totalSeconds * (x / smoothness)));
            Debug.Log(Mathf.Lerp(originalRotation, angle, totalSeconds * (x / smoothness)));
            yield return new WaitForSeconds(totalSeconds / smoothness);
        }
        rotationStarted = false;
    }

    private void Gen()
    {
        if(gc.players[gc.whoseTurn].controlledShips.Count >= gc.players[gc.whoseTurn].shipLimit)
        {
            StartCoroutine(alertMessage("Already at max ship capacity"));
            return;
        }
        Vector3Int poppedLoc = selectedTiles.Peek();
        TileData data = tileArray[poppedLoc.x, poppedLoc.y];
        if (gc.players[gc.whoseTurn].controlledTiles.Contains(poppedLoc) //check if tile is controlled
            && data.getPlanet() != null //check if 
            && data.getShip() == null
            && (poppedLoc.x == 0 && poppedLoc.y == 0 || poppedLoc.x == mg.xsize-1 && poppedLoc.y == mg.ysize - 1) )
        {
            int oreRequired = 30;
            int uraniumRequired = 0;
            GameObject tempShip;
            Vector3 spawnLocation = tilemap.CellToWorld(poppedLoc);
            spawnLocation.Set(spawnLocation.x, spawnLocation.y + 0.5f, spawnLocation.z);
            GameObject ship;
            int Default = 0, Flagship = 1, Miner = 2;
            int type;
            if (selectShip.text.Equals("explorer"))
            {
                if (gc.whoseTurn == 0)
                    ship = explorer;
                else
                    ship = teamSpaceship2;
                type = Default;
            }
            else if (selectShip.text.Equals("flagship"))
            {
                ship = flagship;
                type = Flagship;
                oreRequired = 50;
                uraniumRequired = 25;
            }
            else
            {
                ship = miner;
                type = Miner;
                oreRequired = 40;
            }

            if (gc.players[gc.whoseTurn].ore < oreRequired || gc.players[gc.whoseTurn].uranium < uraniumRequired)
            {
                if (!alertStarted) StartCoroutine(alertMessage("Not enough resources"));
                Debug.Log("Not enough resources");
                return;
            }

            gc.players[gc.whoseTurn].ore -= oreRequired;
            gc.players[gc.whoseTurn].uranium -= uraniumRequired;

            if (gc.whoseTurn == 0)
                tempShip = Instantiate(ship, spawnLocation, Quaternion.Euler(-90, -90, 0));
            else
                tempShip = Instantiate(ship, spawnLocation, Quaternion.Euler(-90, 90, 0));

            for (int x = 0; x < tempShip.GetComponent<Renderer>().materials.Length; x++)
            {
                tempShip.GetComponent<Renderer>().materials[x].color = teamColors[gc.whoseTurn];
            }
            tempShip.transform.parent = GameObject.Find("Ships").transform;

            Ship shipComp = tempShip.AddComponent<Ship>();
            shipComp.Initialize(data.location, gc.whoseTurn, gc, type);
            gc.shipsToReset.Add(shipComp);
            gc.players[gc.whoseTurn].controlledShips.Add(shipComp);
            tileArray[poppedLoc.x, poppedLoc.y].setShip(shipComp);
            gc.activeShips.Add(shipComp);
        }
        else
        {
            if (!alertStarted) StartCoroutine(alertMessage("Can't generate a ship"));
            Debug.Log("Can't generate a ship.");
        }
    }
    private void checkClickOnTile()
    {
        tileArray = FindObjectOfType<MapGenerator>().getAllTiles();
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycasthit))
        {
            try
            {

                if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                {
                    Vector3 hitPos = new Vector3(raycasthit.point.x, 0, raycasthit.point.z);
                    highlightTile(tilemap.WorldToCell(hitPos));
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }

    public void highlightTile(Vector3Int cellCoords)
    {
        Debug.Log(selectedTiles.Count);
        if (selectedTiles.Count > 0)
        {
            Vector3Int poppedLoc = selectedTiles.Pop();
            TileData tileData = tileArray[poppedLoc.x, poppedLoc.y];
            tilemap.SetTile(tileData.location, tileData.tile);

            if (cellCoords.x < tileArray.GetLength(0) && cellCoords.y < tileArray.GetLength(1) && cellCoords.x >= 0 && cellCoords.y >= 0)
            {
                selectedTiles.Push(cellCoords);
                tilemap.SetTile(cellCoords, clickedTile);
            }
        }
        else
        {
            if (cellCoords.x < tileArray.GetLength(0) && cellCoords.y < tileArray.GetLength(1) && cellCoords.x >= 0 && cellCoords.y >= 0)
            {
                selectedTiles.Push(cellCoords);
                tilemap.SetTile(cellCoords, clickedTile);
            }
        }
        //highlightAllTilesInRange(1, tileArray[cellCoords.x, cellCoords.y]);
    }



    //this broken af
    public void highlightAllTilesInRange(int radius, TileData tile)
    {
        List<Vector3Int> tilesToHighlight = Utilities.getTilesWithinRadius(radius, tile, mg.xsize, mg.ysize);
        if (tilesToDeselect.Count != 0)
        {
            for (int x = 0; x < tilesToDeselect.Count; x++)
            {
                tilemap.SetTile(tilesToDeselect[x], tileArray[tilesToDeselect[x].x, tilesToDeselect[x].y].tile);
            }
            tilesToDeselect.Clear();
        }
        foreach (var toHighlight in tilesToHighlight)
        {
            tilemap.SetTile(toHighlight, clickedTile);
            tilesToDeselect.Add(toHighlight);
        }

    }

    public void updateShipDisplayInformation()
    {
        if (selectedTiles.Count > 0)
        {
            Vector3Int coords = selectedTiles.Peek();
            Ship toDisplay = tileArray[coords.x, coords.y].getShip();
            if (toDisplay != null)
            {
                switch(toDisplay.type){
                    case(0):
                        root.Q<Label>("ShipType").text = "Ship Type: Explorer";
                        break;
                    case(1):
                        root.Q<Label>("ShipType").text = "Ship Type: Flagship";
                        break;
                    case(2):
                        root.Q<Label>("ShipType").text = "Ship Type: Miner";
                        break;
                }
                root.Q<Label>("ShipHealth").text = "Ship Health: " + toDisplay.health;
            }
            else
            {
                root.Q<Label>("ShipType").text = "Ship Type: ";
                root.Q<Label>("ShipHealth").text = "Ship Health: ";
            }

        }
    }

    public void UpdateTileDisplayInformation()
    {
        if (selectedTiles.Count > 0)
        {
            Vector3Int coords = selectedTiles.Peek();
            if (coords.x < tileArray.GetLength(0) && coords.y < tileArray.GetLength(1) && coords.x >= 0 && coords.y >= 0)
            {
                TileData data = tileArray[coords.x, coords.y];
                root.Q<Label>("PlanetType").text = "Planet Type: " + planetTypeToString(data.planetType);
                root.Q<Label>("Planet").text = "Name \n" + data.name;
                if (data.planetType != 0 && data.planetType != 1)
                {
                    root.Q<Label>("Resource").text = "Resource \n" + data.resourcePerTurn + " " + planetTypeToResource(data.planetType) + " Per Turn";
                }
                else if (data.planetType == 1)
                {
                    root.Q<Label>("Resource").text = "+1 Ship Limit";
                }
                else
                {
                    root.Q<Label>("Resource").text = "Resource";
                }
            }
        }
    }

    public void UpdateTurnInformation()
    {
        gc.readyToMoveOn = true;
        Label turn = root.Q<Label>("turn");
        int tempWhoseTurn = 0;
        int tempTurnCount = gc.turnCount;
        if (gc.whoseTurn + 1 >= gc.numPlayers)
        {
            tempWhoseTurn = 1;
            tempTurnCount += 1;
        }
        else
        {
            tempWhoseTurn = gc.whoseTurn + 2;
        }
        turn.text = "Turn: " + tempTurnCount + ", Player " + (tempWhoseTurn) + "'s Turn";

    }

    public void UpdateTeamTilesDisplayed()
    {
        for (int i = 0; i < gc.players.Length; i++)
        {
            for (int x = 0; x < gc.players[i].controlledTiles.Count; x++)
            {
                teamTilemap.SetTile(gc.players[i].controlledTiles[x], teamTiles[i]);
            }
        }
    }

    public void UpdateTeamResourceCount()
    {
        Label ShipLimitStat = root.Q<Label>("ShipLimitStat"); 
        Label Ore = root.Q<Label>("OreStat");
        Label Fuel = root.Q<Label>("FuelStat");
        Label Uranium = root.Q<Label>("UraniumStat");
        if (Int32.Parse(ShipLimitStat.text.Split(" ")[2]) != gc.players[gc.whoseTurn].shipLimit)
        {
            ShipLimitStat.text = "Ship Limit: " + gc.players[gc.whoseTurn].shipLimit;
        }
        if (Int32.Parse(Ore.text.Split(" ")[1]) != gc.players[gc.whoseTurn].getOre())
        {
            Ore.text = "Ore: " + gc.players[gc.whoseTurn].getOre();
        }
        if (Int32.Parse(Fuel.text.Split(" ")[1]) != gc.players[gc.whoseTurn].getFuel())
        {
            Fuel.text = "Fuel: " + gc.players[gc.whoseTurn].getFuel();
        }
        if (Int32.Parse(Uranium.text.Split(" ")[1]) != gc.players[gc.whoseTurn].getUranium())
        {
            Uranium.text = "Uranium: " + gc.players[gc.whoseTurn].getUranium();
        }
    }

    public void ExtractAll()
    {
        List<Ship> currentTeamShips = gc.allShips[gc.whoseTurn];
        foreach (Ship ship in currentTeamShips)
        {
            TileData tempTile = tileArray[ship.location.x, ship.location.y];
            if (ship.canAct && tempTile.planetType != 0)
            {
                int Extractable = tempTile.getResourcePerTurn();
                string resource = planetTypeToResource(tempTile.getPlanetType());
                switch (resource)
                {
                    case "Fuel":
                        gc.players[gc.whoseTurn].setFuel(gc.players[gc.whoseTurn].getFuel() + Extractable);
                        break;
                    case "Ore":
                        gc.players[gc.whoseTurn].setOre(gc.players[gc.whoseTurn].getOre() + Extractable);
                        break;
                    case "Uranium":
                        gc.players[gc.whoseTurn].setUranium(gc.players[gc.whoseTurn].getUranium() + Extractable);
                        break;
                    case "Ship Limit":
                        if (!alertStarted) StartCoroutine(alertMessage("I hate urav"));
                        Debug.Log("I hate urav");
                        break;
                }
                gc.toReset.Add(tempTile);
                tempTile.setExtracted(true);
                ship.canAct = false;
                gc.shipsToReset.Add(ship);
            }
        }

    }

    public void DisableAll()
    {
        root.visible = false;
        enabled = false;
    }

    public void EnableAll()
    {
        root.visible = true;
    }

    public bool GetReadyToRotate()
    {
        return !rotationStarted;
    }

    private string planetTypeToString(int x)
    {
        string[] planets = { "", "Terran", "Fuel", "Ore", "Radioactive" };
        return planets[x];
    }

    private string planetTypeToResource(int x)
    {
        string[] planets = { "", "Ship Limit", "Fuel", "Ore", "Uranium" };
        return planets[x];
    }
}


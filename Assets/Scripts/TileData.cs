using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileData
{
    public Tile tile;
    public Vector3Int location;
    //0-redTile, 1-earthlike, 2-venuslike, 3-ore, 4-uranium
    public int planetType;
    public int resourcePerTurn;
    public string name;
    public Planet planet;
    public Ship ship;
    public bool extracted;
    public bool isBlue;

    public TileData(Tile tile, Vector3Int location, Planet planet, bool isBlue)
    {
        this.tile = tile;
        this.location = location;
        this.planetType = planet.planetType;
        this.planet = planet;
        this.isBlue = isBlue;
        this.extracted = false;
        if (planetType != 0)
            this.name = GenerateExoplanetName();
        else
            this.name = "";
        if (planetType != 0 && planetType != 1)
            resourcePerTurn = (int) Mathf.Lerp(50, 10, Mathf.InverseLerp(0, 8, Vector3.Distance(location, new Vector3(6, 6))));
        else
            resourcePerTurn = 0;
    }

    public TileData(Tile tile, Vector3Int location, bool isBlue)
    {
        this.tile = tile;
        this.location = location;
        this.isBlue = isBlue;
        planetType = 0;
        name = "";
        resourcePerTurn = 0;
        extracted = false;
        planet = null;
    }

    public string GenerateExoplanetName()
    {
        int line = (int)Random.Range(0, 33332f);
        TextAsset text = (TextAsset) Resources.Load("ExoplanetNames");
        string[] names = text.text.Split("\n");
        return names[line];
    }

    public void setShip(Ship ship)
    {
        this.ship = ship;
    }

    public Ship getShip()
    {
        return ship;
    }

    public Planet getPlanet(){
        return planet;
    }

    public int getPlanetType(){
        return planetType;
    }
    public bool getExtracted(){
        return extracted;
    } 

    public void setExtracted(bool a){
        extracted = a;
    }

    public int getResourcePerTurn(){
        return resourcePerTurn;
    }
    public bool isControlled(Player x){
        foreach(Vector3Int tile in x.controlledTiles){
            if(tile.Equals(location)){
                return true;
            }
        }
        return false;   
    }

    public override string ToString()
    {
        return "Tile: " + tile + ", Location: " + location + ", Planet Type: " + planetType;
    }
}

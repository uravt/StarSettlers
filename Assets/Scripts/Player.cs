using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player
{
    public int shipLimit;
    public int fuel;
    public int ore;
    public int uranium;
    public List<Vector3Int> controlledTiles;
    public List<Ship> controlledShips;

    public Player()
    {
        shipLimit = 1;
        fuel = 50;
        ore = 50;
        uranium = 0;
        controlledTiles = new List<Vector3Int>();
        controlledShips = new List<Ship>();
    }

    public void AddTerritory(Vector3Int cell)
    {
        controlledTiles.Add(cell);
    }
    
    public int getFuel(){
        return fuel;
    }

    public int getOre(){
        return ore;
    }
    public int getUranium(){
        return uranium;
    }

    public void setFuel(int a){
       fuel = a;
    }

    public void setOre(int a){
       ore = a;
    }  

    public void setUranium(int a){
       uranium = a;
    }  

}

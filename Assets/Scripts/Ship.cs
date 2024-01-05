using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    public int type;
    public int team;
    public int attack;
    public int health;
    public bool canAct;
    public Transform target;
    public Vector3Int location;

    public int Explorer = 0, Flagship = 1, Miner = 2;

    public void Initialize(Vector3Int location, int team, GameController gc, int type)
    {
        this.type = type;
        this.team = team;
        transform.localScale *= 0.5f;
        this.location = location;
        canAct = false;
        gc.allShips[team].Add(this);
        initStats();
    }
    
    private void initStats(){ //initalises the stats
        switch(type){
            case 0: //explorer
                health = 30;
                attack = 15;
                break;
            case 1: //flagship
                health = 100;
                attack = 30;
                break;
            case 2: //miner
                health = 45;
                attack = 5;
                break;
        }
    }

    /*void Update()
    {
        Vector3 relativePos = target.position - transform.position;
        transform.rotation = Quaternion.LookRotation(relativePos);
    }*/
    public int getTeam() { return team; }
    public void setTeam(int team) { this.team = team; }

    public override string ToString()
    {
        string toReturn = "Type: " + type + ", Team: " + team + ", Location: " + location + ", Can Act: " + canAct;
        return toReturn;
    }
}

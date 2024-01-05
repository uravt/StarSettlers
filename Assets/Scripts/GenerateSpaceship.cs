using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class GenerateSpaceship : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject spaceship;
    private TileData[,] tileArray;
    private Camera mainCamera;
    private Tilemap tilemap;
    private Tile clickedTile;
    private Stack<Vector3Int> selectedTiles;
    private Stack<GameObject> ships;
    public Transform target;

    void Update()
    {
        tileArray = FindObjectOfType<MapGenerator>().getAllTiles();
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycasthit))
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 hitPos = new Vector3(raycasthit.point.x, 0, raycasthit.point.z);
                Instantiate(spaceship, hitPos, new Quaternion(0, 0, 0, 0));
            }
        }
    }
}

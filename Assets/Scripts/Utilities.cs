using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Utilities
{
    public static List<Vector3Int> getTilesWithinRadius(int radius, TileData tile, int maxX, int maxY)
    {
        List<Vector3Int> returnLocations = new List<Vector3Int>();
        /*
        int[] rowLengths = new int[2 * radius + 1];
        for(int i = 0; i  < rowLengths.Length; i++)
        {
            rowLengths[i] = rowLengths.Length - Mathf.Abs((rowLengths.Length/2 + 1) - (i+1));
        }
        bool yCoordinateChanges = tile.location.x % 2 == 0;
        int rowIndex = 0;
        for (int x = Mathf.Max(0, tile.location.x - radius); x < Mathf.Min(maxX, tile.location.x + radius); x++)
        {
            if(yCoordinateChanges)
            {
                for (int y = Mathf.Max(0, tile.location.y - 1); y < Mathf.Min(maxY, tile.location.y - 1 + rowLengths[rowIndex]); y++)
                {
                    returnLocations.Add(new Vector3Int(x, y));
                }
            }
            else
            {
                for (int y = Mathf.Max(0, tile.location.y); y < Mathf.Min(maxY, tile.location.y + rowLengths[rowIndex]); y++)
                {
                    returnLocations.Add(new Vector3Int(x, y));
                }
            }
            rowIndex++;
        }
        return returnLocations;*/
        Vector3Int location = tile.location;

        for (int x = location.x - radius;
                  x <= location.x + radius;
                  x++)
        {
            for (int y = location.y - radius;
                    y <= location.y + radius;
                      y++)
            {
                Vector3Int tempPoint = new Vector3Int(x, y);
                if (ComputeDistanceHexGrid(location, tempPoint) <= radius)
                {
                    returnLocations.Add(tempPoint);
                }

            }
        }
        return returnLocations;
    }



    public static int ComputeDistanceHexGrid(Vector3Int A, Vector3Int B)
    {
      // compute distance as we would on a normal grid
      Vector3Int distance = new Vector3Int(0,0);
        distance.x = A.x - B.x;
      distance.y = A.y - B.y;

      // compensate for grid deformation
      // grid is stretched along (-n, n) line so points along that line have
      // a distance of 2 between them instead of 1

      // to calculate the shortest path, we decompose it into one diagonal movement(shortcut)
      // and one straight movement along an axis
      Vector3Int diagonalMovement = new Vector3Int(0, 0);
        int lesserCoord = Mathf.Abs(distance.x) <  Mathf.Abs(distance.y) ? Mathf.Abs(distance.x) : Mathf.Abs(distance.y);
        diagonalMovement.x = (distance.x< 0) ? -lesserCoord : lesserCoord; // keep the sign 
      diagonalMovement.y = (distance.y< 0) ? -lesserCoord : lesserCoord; // keep the sign

      Vector3Int straightMovement = new Vector3Int(0,0);

        // one of x or y should always be 0 because we are calculating a straight
        // line along one of the axis
        straightMovement.x = distance.x - diagonalMovement.x;
      straightMovement.y = distance.y - diagonalMovement.y;

      // calculate distance
      int straightDistance = Mathf.Abs(straightMovement.x) + Mathf.Abs(straightMovement.y);
      int diagonalDistance = Mathf.Abs(diagonalMovement.x);

      // if we are traveling diagonally along the stretch deformation we double
      // the diagonal distance
      if ((diagonalMovement.x< 0 && diagonalMovement.y> 0) || 
           (diagonalMovement.x > 0 && diagonalMovement.y< 0) )
      {
        diagonalDistance *= 2;
      }

      return straightDistance + diagonalDistance;

    }

    public static float betterComputeDistanceHexGrid(Vector3Int a, Vector3Int b, Tilemap tilemap)
    {
        Vector3 aAsNormalVector = tilemap.CellToWorld(a);
        Vector3 bAsNormalVector = tilemap.CellToWorld(b);
        return Vector3.Distance(aAsNormalVector, bAsNormalVector);
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public enum GameState { PlayerSelectTile, PlayerSelectAction, PlayerMoveUnit, PlayerAttackUnit, EnemyTurn };

public class GameController : NetworkBehaviour {

    public static GameController instance;

    public int gridSizeX, gridSizeY;
    public GameObject whiteTilePrefab, blackTilePrefab;
    public GameObject basicLandUnitPrefab;
    public int numPlayerUnits, numEnemyUnits;
    public int tileSize;
    public const int numTeams = 2;
    public TileController[,] mapGrid;
    public List<UnitController>[] unitsInGame;

    void Awake()
    {

        unitsInGame = new List<UnitController>[2];

        for (int i = 0; i < numTeams; i++)
        {
            unitsInGame[i] = new List<UnitController>();
        }
        instance = this;
    }

    void Start()
    {
        
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.S))
        {
            InitializeMap();
            SpawnUnits();
        }

    }

    void InitializeMap()
    {
        //tileList = new List<GameObject>();
        mapGrid = new TileController[gridSizeX, gridSizeY];

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                GameObject tileToBeSpawned = (x + y) % 2 == 0 ? whiteTilePrefab : blackTilePrefab;
                Vector3 newTileLoc = new Vector3(x * tileSize, 0f, y * tileSize);
                GameObject newTile = Instantiate(tileToBeSpawned, newTileLoc, Quaternion.identity);
                newTile.GetComponent<TileController>().curCoords = new IntVector2(x, y);
                mapGrid[x, y] = newTile.GetComponent<TileController>();
                NetworkServer.Spawn(newTile);
                //tileList.Add(newTile);
            }
        }
        //Debug.Log(tileList.Count + " tiles on map");

    }

    public void SpawnUnits()
    {
        for (int i = 0; i < numPlayerUnits; i++)
        {
            Vector3 newUnitLoc = new Vector3(i * tileSize, 0, 0);
            GameObject newUnit = Instantiate(basicLandUnitPrefab, newUnitLoc, Quaternion.identity);
            NetworkServer.Spawn(newUnit);
            //NetworkServer.SpawnWithClientAuthority(newUnit, connectionToClient);
            UnitController newUnitController = newUnit.GetComponent<UnitController>();
            newUnitController.unitTeamID = 0;
            newUnitController.curCoords = new IntVector2(i, 0);
            mapGrid[i, 0].isOccupied = true;
            mapGrid[i, 0].unitOnTile = newUnit;
            unitsInGame[0].Add(newUnitController);
        }

        for (int i = 0; i < numEnemyUnits; i++)
        {
            Vector3 newUnitLoc = new Vector3(i * tileSize, 0, (gridSizeY - 1) * 2);
            GameObject newUnit = Instantiate(basicLandUnitPrefab, newUnitLoc, Quaternion.identity);
            NetworkServer.Spawn(newUnit);
            //NetworkServer.SpawnWithClientAuthority(newUnit, connectionToClient);
            UnitController newUnitController = newUnit.GetComponent<UnitController>();
            newUnitController.unitTeamID = 1;
            newUnitController.curCoords = new IntVector2(i, gridSizeY - 1);
            mapGrid[i, gridSizeY - 1].isOccupied = true;
            mapGrid[i, gridSizeY - 1].unitOnTile = newUnit;
            unitsInGame[1].Add(newUnitController);
        }
    }
}

[System.Serializable]
public struct IntVector2
{
    public int x;
    public int y;
    /// <summary>
    /// new IntVector2(0, 0);
    /// </summary>
    public static IntVector2 zero = new IntVector2(0, 0);
    /// <summary>
    /// new IntVector2(0, 1);
    /// </summary>
    public static IntVector2 coordUp = new IntVector2(0, 1);
    /// <summary>
    /// new IntVector2(0, -1);
    /// </summary>
    public static IntVector2 coordDown = new IntVector2(0, -1);
    /// <summary>
    /// new IntVector2(1, 0);
    /// </summary>
    public static IntVector2 coordRight = new IntVector2(1, 0);
    /// <summary>
    /// new IntVector2(-1, 0);
    /// </summary>
    public static IntVector2 coordLeft = new IntVector2(-1, 0);
    /// <summary>
    /// new IntVector2(-1, 1);
    /// </summary>
    public static IntVector2 coordUpLeft = new IntVector2(-1, 1);
    /// <summary>
    /// new IntVector2(1, 1);
    /// </summary>
    public static IntVector2 coordUpRight = new IntVector2(1, 1);
    /// <summary>
    /// new IntVector2(1, -1);
    /// </summary>
    public static IntVector2 coordDownRight = new IntVector2(1, -1);
    /// <summary>
    /// new IntVector2(-1, -1);
    /// </summary>
    public static IntVector2 coordDownLeft = new IntVector2(-1, -1);

    public IntVector2(int newX, int newY)
    {
        x = newX;
        y = newY;
    }

    /// <summary>
    /// Prints the IntV2 in format: (x, y)
    /// </summary>
    public void Print()
    {
        Debug.Log("(" + x + ", " + y + ")");
    }

    /// <summary>
    /// Returns the IntV2 in string format (x, y)
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return ("(" + x + ", " + y + ")");
    }

    public static bool operator ==(IntVector2 v1, IntVector2 v2)
    {
        if (v1.x == v2.x && v1.y == v2.y)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static bool operator !=(IntVector2 v1, IntVector2 v2)
    {
        if (v1.x == v2.x && v1.y == v2.y)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static IntVector2 operator +(IntVector2 v1, IntVector2 v2)
    {
        IntVector2 sum = new IntVector2(v1.x + v2.x, v1.y + v2.y);
        return sum;
    }

    public static IntVector2 operator -(IntVector2 v1, IntVector2 v2)
    {
        IntVector2 diff = new IntVector2(v1.x - v2.x, v1.y - v2.y);
        return diff;
    }

    /// <summary>
    /// Returns the distance between two points. Distance is the number of IntVector2s that would need to be visited
    /// when traveling from v1 to v2
    /// </summary>
    /// <param name="v1">Starting Point</param>
    /// <param name="v2">Ending Point</param>
    /// <returns></returns>
    public static int Distance(IntVector2 v1, IntVector2 v2)
    {
        IntVector2 diff = v1 - v2;
        int dist = (Mathf.Abs(diff.x) + Mathf.Abs(diff.y));
        return dist;
    }

    /// <summary>
    /// Returns true if the IntVector2 is on the grid
    /// </summary>
    /// <param name="curLoc">Current IntVector2 to test on</param>
    /// <param name="gridSizeX">Horizontal Grid Size</param>
    /// <param name="gridSizeY">Vertical Grid Size</param>
    /// <returns></returns>
    public static bool OnGrid(IntVector2 curLoc, int gridSizeX, int gridSizeY)
    {
        if(curLoc.x < 0 || curLoc.y < 0 || curLoc.x >= gridSizeX || curLoc.y >= gridSizeY)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
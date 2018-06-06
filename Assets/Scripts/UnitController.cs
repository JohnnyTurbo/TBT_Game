using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    public int unitHealth, attackStat, defenseStat, movementRange, AttackRange, numUnitType;
    public float yOffset;
    public string unitType;
    public bool canMove, canAttack;
    public bool isMoving, isAttacking;
    public IntVector2 curCoords;
    public int unitTeamID;
    public int unitIndex;
    //public Material p1SelectMat, p2SelectMat;

    GameController gameController;
    List<TileController> availTiles;
    //public Material startingMat;

    void Awake()
    {
        gameController = GameController.instance;
    }

    void Start()
    {
        //startingMat = gameObject.GetComponent<MeshRenderer>().material;
        canAttack = true;
        canMove = true;
    }

    /// <summary>
    /// Called when the unit is selected.
    /// </summary>
    public void OnUnitSelect()
    {

        //Debug.Log("Unit with index: " + unitIndex + " was selected");

        //Show Menu
        //Debug.Log("OnUnitSelect()");
        if (gameController.playerTeamID == unitTeamID)
        {
            //gameObject.GetComponent<MeshRenderer>().material = p1SelectMat;
            //gameController.curState = GameState.PlayerSelectAction;
            if (canMove)
            {
                //Select Move Action
                MoveUnit();
            }
            else if (canAttack)
            {
                //Select Attack Action
                //gameController.curState = GameState.PlayerAttackUnit;
                AttackUnit();
            }
            else
            {
                //Show Actions Menu
                gameController.ShowMessage("Unit out of Moves!", 1f);
                gameController.curState = GameState.PlayerSelectAction;
            }
            gameController.ShowActionsMenu();
        }
        else
        {
            //gameObject.GetComponent<MeshRenderer>().material = p2SelectMat;
            gameController.curState = GameState.PlayerSelectTile;
        }
        gameController.ShowUnitStats(this);

    }

    public void OnUnitDeselect()
    {
        //Hide Menu
        //Debug.Log("OnUnitDeselect()");
        UnhighlightTiles();
        //gameObject.GetComponent<MeshRenderer>().material = startingMat;
        gameController.HideActionsMenu();
        gameController.HideUnitStats();
        gameController.HideMessage();

        if (isMoving)
        {
            isMoving = false;
        }
        if (isAttacking)
        {
            isAttacking = false;
        }

        gameController.curUnit = null;
        gameController.curState = GameState.PlayerSelectTile;
    }

    public void MoveUnit()
    {
        UnhighlightTiles();
        isMoving = true;
        gameController.curState = GameState.PlayerMoveUnit;
        availTiles = new List<TileController>();
        availTiles = gameController.FindAvailableTiles(curCoords, movementRange);
        //Debug.Log("There are " + availTiles.Count + " tiles available to move on.");
        foreach (TileController curTile in availTiles)
        {
            curTile.ChangeTileMaterial(curTile.movementMat);
        }
    }

    public void AttackUnit()
    {
        UnhighlightTiles();
        isAttacking = true;
        gameController.curState = GameState.PlayerAttackUnit;
        availTiles = new List<TileController>();
        availTiles = gameController.FindAvailableTiles(curCoords, AttackRange);
        int numEnemiesInRange = 0;
        foreach(TileController curTile in availTiles)
        {
            curTile.ChangeTileMaterial(curTile.attackMat);
            if(curTile.unitOnTile != null && curTile.unitOnTile.GetComponent<UnitController>().unitTeamID 
                != gameController.playerTeamID)
            {
                numEnemiesInRange++;
            }
        }
        if(numEnemiesInRange == 0)
        {
            //Debug.Log("No enemies in range");
            gameController.ShowMessage("No Enemies in Range!", 1.5f);
        }
    }

    public void UnhighlightTiles()
    {
        if(availTiles == null)
        {
            return;
        }
        else
        {
            foreach(TileController curTile in availTiles)
            {
                curTile.ChangeTileMaterial(curTile.startingMat);
            }
        }
    }

    public void RotateUnitSprite()
    {
        Vector3 targetVector = Camera.main.transform.position - transform.position;

        float newYAngle = Mathf.Atan2(targetVector.z, targetVector.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, -1 * newYAngle, 0);
    }

    public bool OnSameTeam(UnitController otherUnit)
    {
        if (otherUnit.unitTeamID == unitTeamID)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void GoToBench()
    {
        int playersOnBench = GameController.instance.numPlayersOnBench[unitTeamID];
        GameController.instance.numPlayersOnBench[unitTeamID]++;
        Vector3 newPos = new Vector3(14.5f, yOffset, (unitTeamID * 8) + (playersOnBench * 2) + 1);
        transform.position = newPos;
        curCoords = IntVector2.coordDownLeft;
    }
}

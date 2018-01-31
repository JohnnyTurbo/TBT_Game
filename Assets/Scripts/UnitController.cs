using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    public int numUnits, attackStat, defenseStat, movementRange, AttackRange;
    public bool canMove, canAttack;
    public bool isMoving, isAttacking;
    public IntVector2 curCoords;
    public int unitTeamID;
    public Material p1SelectMat, p2SelectMat;

    GameController gameController;
    List<TileController> availTiles;
    Material startingMat;

    void Awake()
    {
        gameController = GameController.instance;
    }

    void Start()
    {
        startingMat = gameObject.GetComponent<MeshRenderer>().material;
        canAttack = true;
        canMove = true;
    }

    /// <summary>
    /// Called when the unit is selected.
    /// </summary>
    public void OnUnitSelect()
    {
        //Show Menu
        Debug.Log("OnUnitSelect()");
        if (gameController.playerTeamID == unitTeamID)
        {
            gameObject.GetComponent<MeshRenderer>().material = p1SelectMat;
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
                //Maybe display message about no available actions
                gameController.curState = GameState.PlayerSelectAction;
            }
            gameController.ShowActionsMenu();
        }
        else
        {
            gameObject.GetComponent<MeshRenderer>().material = p2SelectMat;
            gameController.curState = GameState.PlayerSelectTile;
        }
        gameController.ShowUnitStats(this);

    }

    public void OnUnitDeselect()
    {
        //Hide Menu
        Debug.Log("OnUnitDeselect()");
        UnhighlightTiles();
        gameObject.GetComponent<MeshRenderer>().material = startingMat;
        gameController.HideActionsMenu();
        gameController.HideUnitStats();

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

        foreach(TileController curTile in availTiles)
        {
            curTile.ChangeTileMaterial(curTile.attackMat);
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
}

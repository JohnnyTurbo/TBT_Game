using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class UnitController : NetworkBehaviour
{
    public int numUnits, attackStat, defenseStat, movementRange, AttackRange;
    public bool canMove, canAttack;
    public bool isMoving, isAttacking;
    public IntVector2 curCoords;

    [SyncVar]
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
    public void OnUnitSelect(PlayerNetworkObjectController pnoc)
    {
        //Show Menu
        //Debug.Log("OnUnitSelect()");
        if (pnoc.playerTeamID == unitTeamID)
        {
            gameObject.GetComponent<MeshRenderer>().material = p1SelectMat;
            //gameController.curState = GameState.PlayerSelectAction;
            if (canMove)
            {
                //Select Move Action
                MoveUnit(pnoc);
            }
            else if (canAttack)
            {
                //Select Attack Action
                //gameController.curState = GameState.PlayerAttackUnit;
                AttackUnit(pnoc);
            }
            else
            {
                //Show Actions Menu
                pnoc.ShowMessage("Unit out of Moves!", 1f);
                pnoc.curState = GameState.PlayerSelectAction;
            }
            pnoc.ShowActionsMenu();
        }
        else
        {
            gameObject.GetComponent<MeshRenderer>().material = p2SelectMat;
            pnoc.curState = GameState.PlayerSelectTile;
        }
        pnoc.ShowUnitStats(this);

    }

    public void OnUnitDeselect(PlayerNetworkObjectController pnoc)
    {
        //Hide Menu
        //Debug.Log("OnUnitDeselect()");
        UnhighlightTiles();
        gameObject.GetComponent<MeshRenderer>().material = startingMat;
        pnoc.HideActionsMenu();
        pnoc.HideUnitStats();
        pnoc.HideMessage();

        if (isMoving)
        {
            isMoving = false;
        }
        if (isAttacking)
        {
            isAttacking = false;
        }

        pnoc.curUnit = null;
        pnoc.curState = GameState.PlayerSelectTile;
    }

    public void MoveUnit(PlayerNetworkObjectController pnoc)
    {
        UnhighlightTiles();
        isMoving = true;
        pnoc.curState = GameState.PlayerMoveUnit;
        availTiles = new List<TileController>();
        //availTiles = pnoc.FindAvailableTiles(curCoords, movementRange);
        //Debug.Log("There are " + availTiles.Count + " tiles available to move on.");
        foreach (TileController curTile in availTiles)
        {
            curTile.ChangeTileMaterial(curTile.movementMat);
        }
    }

    public void AttackUnit(PlayerNetworkObjectController pnoc)
    {
        UnhighlightTiles();
        isAttacking = true;
        pnoc.curState = GameState.PlayerAttackUnit;
        availTiles = new List<TileController>();
        //availTiles = pnoc.FindAvailableTiles(curCoords, AttackRange);
        int numEnemiesInRange = 0;
        foreach(TileController curTile in availTiles)
        {
            curTile.ChangeTileMaterial(curTile.attackMat);
            if(curTile.unitOnTile != null && curTile.unitOnTile.GetComponent<UnitController>().unitTeamID 
                != pnoc.playerTeamID)
            {
                numEnemiesInRange++;
            }
        }
        if(numEnemiesInRange == 0)
        {
            //Debug.Log("No enemies in range");
            pnoc.ShowMessage("No Enemies in Range!", 1.5f);
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

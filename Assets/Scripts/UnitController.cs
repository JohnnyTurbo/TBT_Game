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
        //Debug.Log("OnUnitSelect()");
        if (gameController.playerTeamID == unitTeamID)
        {
            gameObject.GetComponent<MeshRenderer>().material = p1SelectMat;
            gameController.curState = GameState.PlayerSelectAction;
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
        //Debug.Log("OnUnitDeselect()");
        gameObject.GetComponent<MeshRenderer>().material = startingMat;
        gameController.HideActionsMenu();
        gameController.HideUnitStats();

        if (isMoving)
        {
            gameController.CancelMove();
        }
        if (isAttacking)
        {
            gameController.CancelAttack();
        }

        gameController.curUnit = null;
        gameController.curState = GameState.PlayerSelectTile;
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

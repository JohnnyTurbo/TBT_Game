using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public bool isOccupied = false;
    public int xPos, yPos;
    //public IntVector2 curCoords;
    public Material hoverMat;
    public GameObject unitOnTile;

    Material startingMat;
    GameController gameController;

    void Awake()
    {
        gameController = GameController.instance;
    }

    void Start()
    {
        startingMat = gameObject.GetComponent<MeshRenderer>().material;
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        //Debug.Log("Pointer Entering tile!");
        HoverTileEnter();
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        HoverTileExit();
    }
 
    void HoverTileEnter()
    {
        gameObject.GetComponent<MeshRenderer>().material = hoverMat;
        gameController.curHoveredTile = this;
    }

    void HoverTileExit()
    {
        gameObject.GetComponent<MeshRenderer>().material = startingMat;
        gameController.curHoveredTile = null;
    }

    public void OnTileSelect()
    {
        if (gameController.curUnit != null)
        {
            //Debug.Log("Deselecting GO with ID: " + gameController.curUnit.GetInstanceID());
            gameController.curUnit.OnUnitDeselect();
        }

        if(unitOnTile != null)
        {
            gameController.curUnit = unitOnTile.GetComponent<UnitController>();
            if(gameController.curUnit != null)
            {
                //Unit on tile has a UnitController script
                gameController.curUnit.OnUnitSelect();
            }

            else
            {
                //Unit on tile DOES NOT have a UnitController script. This should not happen.
                Debug.LogError("Unit on tile with GO ID: " + gameObject.GetInstanceID() 
                               + " has a unit with no UnitController script!");
            }
        }
    }

    public bool AttemptUnitMove(UnitController curUnit)
    {
        if (unitOnTile == null && curUnit.movementRange >= CalculateDist(curUnit.curXPos, curUnit.curYPos)) 
        {
            curUnit.transform.position = this.transform.position;
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool AttemptUnitAttack(UnitController curUnit)
    {
        if (unitOnTile != null)
        {
            UnitController otherUnit = unitOnTile.GetComponent<UnitController>();

            if (curUnit.AttackRange >= CalculateDist(curUnit.curXPos, curUnit.curYPos) && !curUnit.OnSameTeam(otherUnit))
            {
                otherUnit.numUnits = Mathf.CeilToInt(((otherUnit.numUnits * otherUnit.defenseStat) - (curUnit.numUnits
                                                    * curUnit.attackStat)) / (float)otherUnit.defenseStat);
                if (otherUnit.numUnits <= 0)
                {
                    Destroy(unitOnTile);
                    unitOnTile = null;
                }
                return true;
            }
        }
        return false;
    }

    /*
    public int CalculateDist(IntVector2 otherCoords)
    {
        return (Mathf.Abs(curCoords.x - otherCoords.x) + Mathf.Abs(curCoords.y - otherCoords.y));
    }
    */

    //Old implementation. Delete Later
    
    public int CalculateDist(int otherXPos, int otherYpos)
    {
        return (Mathf.Abs(xPos - otherXPos) + Mathf.Abs(yPos - otherYpos));
    }
    
}

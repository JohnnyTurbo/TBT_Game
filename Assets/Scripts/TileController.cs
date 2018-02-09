using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class TileController : NetworkBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public bool isOccupied = false;
    //public int xPos, yPos;
    public IntVector2 curCoords;
    public Material startingMat, hoverMat, movementMat, attackMat;

    [SyncVar]
    public GameObject unitOnTile;

    GameController gameController;
    PlayerNetworkObjectController myPnoc;

    void Awake()
    {
        gameController = GameController.instance;
    }

    void Start()
    {

        startingMat = gameObject.GetComponent<MeshRenderer>().material;

        myPnoc = ClientScene.localPlayers[0].gameObject.GetComponent<PlayerNetworkObjectController>();
        //Debug.Log(myPnoc.GetInstanceID());
        //myPnoc.ShowMessage("pnocID: " + myPnoc.GetInstanceID(), 5);
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
        //ChangeTileMaterial(hoverMat);
        myPnoc.curHoveredTile = this;
        myPnoc.cursor.transform.position = gameObject.transform.position + new Vector3 (0f, 0.00001f, 0f);
    }

    void HoverTileExit()
    {
        //ChangeTileMaterial(startingMat);
        //gameController.curHoveredTile = null;
    }

    public void ChangeTileMaterial(Material newMat)
    {
        gameObject.GetComponent<MeshRenderer>().material = newMat;
    }

    public void OnTileSelect()//PlayerNetworkObjectController pnoc)
    {

        myPnoc.curSelectedTile = this;

        if (myPnoc.curUnit != null)
        {
            //Debug.Log("Deselecting GO with ID: " + gameController.curUnit.GetInstanceID());

            myPnoc.curUnit.OnUnitDeselect(myPnoc);
            myPnoc.curUnit = null;
        }

        if(unitOnTile != null)
        {
            myPnoc.curUnit = unitOnTile.GetComponent<UnitController>();
            if(myPnoc.curUnit != null)
            {
                //Unit on tile has a UnitController script
                myPnoc.curUnit.OnUnitSelect(myPnoc);
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
        if (unitOnTile == null && curUnit.movementRange >= CalculateDist(curUnit.curCoords)) 
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

            if (curUnit.AttackRange >= CalculateDist(curUnit.curCoords) && !curUnit.OnSameTeam(otherUnit))
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

    
    public int CalculateDist(IntVector2 otherCoords)
    {
        return (Mathf.Abs(curCoords.x - otherCoords.x) + Mathf.Abs(curCoords.y - otherCoords.y));
    }

    public override void OnStartLocalPlayer()
    {
        Debug.Log("OnStartLocalPlayer()");
        base.OnStartLocalPlayer();
        myPnoc = ClientScene.localPlayers[0].gameObject.GetComponent<PlayerNetworkObjectController>();

    }

}

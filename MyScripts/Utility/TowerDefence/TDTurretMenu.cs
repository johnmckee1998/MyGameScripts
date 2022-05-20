using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TDTurretMenu : MonoBehaviour
{
    public static TDTurretMenu instance;
    //public Dropdown targetType; //target type is determine by type of turret, not changed by player - e.g. land vs AA
    public TMP_Dropdown targetOrder;
    public Toggle constantUpdateToggle;
    private PlayerAutoTurret currentTurret;
    public GameObject reposistionButton;
    public GameObject refillButton;
    private Transform turretParent;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        targetOrder.ClearOptions();
        targetOrder.AddOptions(new List<string> { "First", "Last", "Close", "Far", "Strong" });

        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown("escape"))
            gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if(CharacterControllerScript.instance!=null && CharacterControllerScript.Active)
            CharacterControllerScript.instance.LockMovement(true);
    }


    private void OnDisable()
    {
        if (CharacterControllerScript.instance != null && !CharacterControllerScript.Active)
            CharacterControllerScript.instance.UnlockMovement();
        turretParent = null;
    }


    public void UpdateTurret(PlayerAutoTurret tur)
    {
        currentTurret = tur;
        //targetType = tur.typeOfEnemy;
        if (tur.targettingOrder == PlayerAutoTurret.TargetOrder.First) //this is farily hard coded, could be better
            targetOrder.value = 0;
        else if (tur.targettingOrder == PlayerAutoTurret.TargetOrder.Last) //this is farily hard coded, could be better
            targetOrder.value = 1;
        else if (tur.targettingOrder == PlayerAutoTurret.TargetOrder.Close) //this is farily hard coded, could be better
            targetOrder.value = 2;
        else if (tur.targettingOrder == PlayerAutoTurret.TargetOrder.Far) //this is farily hard coded, could be better
            targetOrder.value = 3;
        else
            targetOrder.value = 4;
        constantUpdateToggle.isOn = tur.constantUpdate;
    }

    public void ChangeUpdate(bool b)
    {
        currentTurret.constantUpdate = b;
    }

    public void ChangeTargetOrder()
    {
        if (targetOrder.value == 0)
            currentTurret.targettingOrder = PlayerAutoTurret.TargetOrder.First;
        else if (targetOrder.value == 1)
            currentTurret.targettingOrder = PlayerAutoTurret.TargetOrder.Last;
        else if (targetOrder.value == 2)
            currentTurret.targettingOrder = PlayerAutoTurret.TargetOrder.Close;
        else if (targetOrder.value == 3)
            currentTurret.targettingOrder = PlayerAutoTurret.TargetOrder.Far;
        else
            currentTurret.targettingOrder = PlayerAutoTurret.TargetOrder.Strong;
    }

    public void RemoveTurret()
    {
        if (turretParent == null)
            Destroy(currentTurret.gameObject);
        else
            Destroy(turretParent.gameObject);
        gameObject.SetActive(false);
    }

    public void Reposition()
    {
        try
        {
            turretParent.SendMessage("ChangePosition", SendMessageOptions.DontRequireReceiver);
        }
        catch
        {
            Debug.Log("Failed: Is Parent gone? " + (turretParent == null));
        }
    }

    public void RepositionButtonState(bool b) //enables/disables repo button depending on if the turret/bot is moveable
    {
        if (reposistionButton != null)
        {
            refillButton.SetActive(!b);
            reposistionButton.SetActive(b);
        }
    }

    public void ParentReference(Transform t) //stores reference to parent object of turret, used by things like: reposistiioning
    {
        turretParent = t;
    }

    public void RefillAmmo(int cost)
    {
        if (PlayerMoney.Money >= cost)
        {
            try
            {
                currentTurret.SendMessage("RefillAmmo", SendMessageOptions.DontRequireReceiver);
                PlayerMoney.Money -= cost;
            }
            catch
            {
                Debug.Log("Failed: Is Turret gone? " + (turretParent == null));
            }
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GenericInteract))]
public class PlayerAutoTurretUIScript : MonoBehaviour
{
    public bool moveable;
    public bool upgradeable;
    private PlayerAutoTurret turret;

    // Start is called before the first frame update
    void Start()
    {
        turret = GetComponent<PlayerAutoTurret>();
        if (turret == null)
            turret = GetComponentInChildren<PlayerAutoTurret>();
    }

    // Update is called once per frame
    public void EnableUI()
    {
        if (!WeaponSelection.instance.IsPlacing())
        {
            TDTurretMenu.instance.gameObject.SetActive(true);
            TDTurretMenu.instance.UpdateTurret(turret);
            TDTurretMenu.instance.RepositionButtonState(moveable);
            if (moveable || upgradeable)
                TDTurretMenu.instance.ParentReference(transform);
        }
    }
}

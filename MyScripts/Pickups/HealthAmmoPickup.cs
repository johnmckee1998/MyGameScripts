using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthAmmoPickup : MonoBehaviour
{
    private AudioSource pickup;

    [Tooltip("0 = Health, 1 = Ammo, 2 = both")]
    public int PickupType = 0;


    private MeshRenderer meshRen;


    private bool used;
    // Start is called before the first frame update
    void Start()
    {
        pickup = GetComponent<AudioSource>();

        meshRen = transform.GetComponent<MeshRenderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (!used) //if either children are still enable or mesh ren is still enabled
            {
                used = true;

                //if((meshRen == null && transform.GetChild(0).gameObject.activeSelf) || (meshRen.enabled))


                if (PickupType.Equals(0))
                    CharacterControllerScript.instance.FillHealth();
                else if (PickupType.Equals(1))
                    WeaponSelection.instance.MaxAmmoAll();
                else if (PickupType.Equals(2))
                {
                    CharacterControllerScript.instance.FillHealth();
                    WeaponSelection.instance.MaxAmmoAll();
                }

                //Destroy(gameObject);
                if(meshRen!=null)
                    transform.GetComponent<MeshRenderer>().enabled = false;
                foreach (Transform child in transform)
                    child.gameObject.SetActive(false);
                //child.GetComponent<MeshRenderer>().enabled = false;

                pickup.PlayOneShot(pickup.clip, 1);

                Destroy(gameObject, 3f);
            }
        }
    }

    

}

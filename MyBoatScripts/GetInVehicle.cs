using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetInVehicle : MonoBehaviour
{
    public GameObject VehicleCam;
    public MonoBehaviour VehicleControls;
    private bool InTrigger = false;
    private GameObject player;

    private void Start()
    {
        
    }

    private void Update()
    {
        if(player == null)
            player = CharacterControllerScript.instance.gameObject;

        if (Input.GetKeyDown("e") && InTrigger && CharacterControllerScript.instance.gameObject.activeSelf)
        {
            CharacterControllerScript.instance.gameObject.SetActive(false);
            CharacterControllerScript.instance.pCam.gameObject.SetActive(false);

            if (VehicleCam != null)
                VehicleCam.SetActive(true);
            if (VehicleControls != null)
                VehicleControls.enabled = true;

            player.transform.parent = VehicleControls.gameObject.transform;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            InTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            InTrigger = false;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportToPoint : MonoBehaviour
{
    public Vector3 teleportOffset;
    public GameObject player;
    private bool playerLooking = false;

    private void OnTriggerEnter(Collider other)
    {
        

        if (other.tag.Equals("Player"))
            player.transform.position += teleportOffset;
    }

    private void OnMouseEnter()
    {
        playerLooking = true;
    }
    private void OnMouseExit()
    {
        playerLooking = false;
    }
}

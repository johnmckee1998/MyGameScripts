using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GasDamageScript : MonoBehaviour
{
    public float dps = 10f;
    public float radius = 2f;
    public float life = 20f;
    public bool neverDelete;

    private void FixedUpdate()
    {
        if (!neverDelete)
        {
            if (life <= 0)
                Destroy(gameObject);
            life -= Time.fixedDeltaTime;
        }
    }

    /*private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Player"))
        {
            GlobalStats.instance.GasOn();
        }
    }*/

    private void OnTriggerStay(Collider other)
    {
        if (other.tag.Equals("Player"))
        {
            CharacterControllerScript.instance.health -= dps * Time.fixedDeltaTime;
            GlobalStats.instance.GasOn();
        }
        else if (other.tag.Equals("TurretsAndBots"))
        {
            other.GetComponent<HitboxScript>().DoDamage(dps*Time.fixedDeltaTime);
        }
    }

    /*private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Player"))
        {
            GlobalStats.instance.GasOff();
        }
    }*/

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}

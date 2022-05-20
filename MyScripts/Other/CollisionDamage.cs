using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDamage : MonoBehaviour
{
    public float damage = 10f;
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.GetComponent<CharacterControllerScript>())//hit player 
        {
            collision.gameObject.GetComponent<CharacterControllerScript>().health -= damage;
            collision.gameObject.GetComponent<CharacterController>().Move(collision.contacts[0].normal);//push player away from laser
        }
        Debug.Log("Hit anything");
    }



    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.GetComponent<CharacterControllerScript>())
        {
            hit.gameObject.GetComponent<CharacterControllerScript>().health -= damage;
            hit.gameObject.GetComponent<CharacterController>().Move(hit.normal);//push player away from laser
        }
        Debug.Log("Hit anything control");
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<CharacterControllerScript>())//hit player 
        {
            other.gameObject.GetComponent<CharacterControllerScript>().health -= damage;
            other.gameObject.GetComponent<CharacterController>().Move(other.gameObject.GetComponent<CharacterController>().velocity*-0.1f);//push player away from laser
        }
    }
}

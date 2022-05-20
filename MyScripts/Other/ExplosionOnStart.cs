using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionOnStart : MonoBehaviour
{
    public Vector3 explosionPos;
    public float force = 2f;
    public float radius = 1f;
    public float upwardsModifier = 3.0f;
    // Start is called before the first frame update
    void Start()
    {
        explosionPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, 1f);
        foreach (Collider hit in colliders) //explosion damage
        {

            Rigidbody rbH = hit.GetComponent<Rigidbody>();
            if (rbH != null)
            {
                rbH.AddExplosionForce(force, explosionPos, radius, upwardsModifier);
                //Debug.Log(hit);
            }
            //else
            //Debug.Log("No rigidBody " + hit);

        }
    }
}

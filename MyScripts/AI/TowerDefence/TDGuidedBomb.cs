using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TDGuidedBomb : MonoBehaviour
{
    public GameObject explosion;
    public float turnRate = 10f;
    public float fallSpeed = 10f;

    private bool armed;
    private Transform target;

    // Start is called before the first frame update
    void Start()
    {
        armed = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (armed)
            Move();
    }

    public void Drop(Transform t)
    {
        armed = true;
        target = t;
    }

    private void Move()
    {
        //move
        transform.position += (transform.forward * fallSpeed)*Time.fixedDeltaTime;

        //turn
        if (target != null)
        {
            Quaternion lookDir = Quaternion.LookRotation((target.position - transform.position).normalized);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookDir, turnRate * Time.fixedDeltaTime);
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        //explode
        if (armed)
        {
            Instantiate(explosion, transform.position, Quaternion.Euler(Vector3.zero));
            Destroy(gameObject);
            armed = false;
        }
    }
}

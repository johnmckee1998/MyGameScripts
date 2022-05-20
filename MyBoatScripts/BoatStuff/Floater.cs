using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floater : MonoBehaviour
{
    public Rigidbody rb;
    public float depthBeforeSubmerged = 1f;
    public float displacementAmount = 3f;

    public float floaterCount = 1f;

    public float waterDrag = 0.99f;
    public float waterAngularDrag = 0.99f;
    public float gravityModifier = 1f;

    public bool underwater = false;

    private void FixedUpdate()
    {
        float waveHeight = WaterManager.instance.GetWaveHeight(transform.position.x, transform.position.z);

        //Manual Gravity
        rb.AddForceAtPosition((Physics.gravity/floaterCount)*gravityModifier, transform.position, ForceMode.Acceleration);

        if (transform.position.y < waveHeight)
        {
            underwater = true;
            float displacementMultiplier = Mathf.Clamp01((waveHeight - transform.position.y) / depthBeforeSubmerged) * displacementAmount;
            rb.AddForceAtPosition(new Vector3(0f, Mathf.Abs(Physics.gravity.y) * displacementMultiplier, 0f), transform.position, ForceMode.Acceleration); //Bouyancy
            rb.AddForce(displacementMultiplier * -rb.velocity * waterDrag * Time.deltaTime, ForceMode.VelocityChange); //Water Drag
            rb.AddTorque(displacementMultiplier * -rb.angularVelocity * waterAngularDrag * Time.deltaTime, ForceMode.VelocityChange); //Water Drag
        }
        else
            underwater = false;
    }
}

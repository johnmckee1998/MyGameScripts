using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicGizmoScript : MonoBehaviour
{
    public float radius = 1f;
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radius);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position+ transform.forward*2);
    }
}

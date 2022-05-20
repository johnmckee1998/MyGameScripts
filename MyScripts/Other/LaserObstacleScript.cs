using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserObstacleScript : MonoBehaviour
{
    public float maxRange = 25f;
    public LayerMask rayCastIgnore;
    public float DPS = 1;
    public float Damage = 25;
    [Range(0.0f, 10.0f)]
    public float moveSpeed;
    RaycastHit rayHit;
    private LineRenderer laser;

    public Transform destPos;

    private Vector3 startPos;
    private bool too; //whether or not laser in travelling too or from destPos


    //stuff for calculating lerp speed
    private float moveDist;

    public AudioSource hitSound;

    private float nextHit;
    // Start is called before the first frame update
    void Start()
    {
        laser = GetComponent<LineRenderer>();

        startPos = transform.localPosition;

        moveDist = Vector3.Distance(startPos, destPos.localPosition);

        nextHit = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if(Physics.Raycast(transform.position, transform.forward, out rayHit, maxRange, ~rayCastIgnore))
        {
            if(rayHit.transform.gameObject.layer == 9 && nextHit<Time.time)//hit player layer
            {
                rayHit.transform.GetComponent<CharacterControllerScript>().health -= Damage; //damage player

                laser.SetPosition(1, new Vector3(0, 0, rayHit.distance)); //update laser length

                if (hitSound != null)
                    hitSound.PlayOneShot(hitSound.clip, hitSound.volume);
                nextHit = Time.time + 0.5f;
            }
            else
                laser.SetPosition(1, new Vector3(0, 0, rayHit.distance)); //update laser length
        }
        else
            laser.SetPosition(1, new Vector3(0, 0, rayHit.distance)); //update laser length

        
    }

    private void FixedUpdate()
    {
        if (Vector3.Distance(transform.localPosition, startPos) < 0.1f)
            too = true;
        else if (Vector3.Distance(transform.localPosition, destPos.localPosition) < 0.1f)
            too = false;

        if (too)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, destPos.localPosition, moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, startPos, moveSpeed * Time.fixedDeltaTime);
        }
    }
}

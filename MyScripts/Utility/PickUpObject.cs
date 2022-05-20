using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpObject : MonoBehaviour
{
    private bool lookingAt;
    public float interactDist;
    public LayerMask rayCastIgnore;
    public float objectSize = 1f;

    private bool held;

    private Rigidbody rb;
    private Collider col;
    private Collision contact;

    public float positioningSpeed = 1f;



    private float currentLerpTime;
    private float lerpTime = 0.5f;
    private Vector3 startPos;
    [Tooltip("If true - rotate to the held position, is false then rotate based on player rotation")]
    public bool rotateToHeld = true;

    public float normalDrag = 0f;
    public float heldDrag = 1f;

    private bool colliding;
    private bool willCollide;



    [Space]
    public Vector3 boxDimensions  = Vector3.one;
    public LayerMask collisionIgnore;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        boxDimensions *= 0.5f; //for overlapbox halfextents are used, so i need to half the given dimensions 
    }

    // Update is called once per frame
    void Update()
    {
        if (CharacterControllerScript.instance != null)
        {
            if (Vector3.Distance(CharacterControllerScript.instance.transform.position, transform.position) < interactDist)
            {
                RaycastHit hit;

                // if raycast hits, it checks if it hit this
                if (Physics.Raycast(CharacterControllerScript.instance.pCam.transform.position, CharacterControllerScript.instance.pCam.transform.forward, out hit, interactDist, ~rayCastIgnore))
                {
                    if (hit.collider.gameObject.Equals(gameObject))
                        lookingAt = true;
                    else
                        lookingAt = false;
                }
                else
                    lookingAt = false;

            }
            else lookingAt = false;
        }

        if (lookingAt && Time.timeScale > 0)
        {
            if (Input.GetButtonDown("Interact"))
            {
                if (held)
                    Drop();
                else
                    Pickup();
            }
        }
        else if (Time.timeScale > 0 && Input.GetButtonDown("Interact") && held)
            Drop();


        if (held)
        {
            UpdatePos();
        }
    }

    private void FixedUpdate()
    {
        if (held)
        {
            if (colliding)
            {
                
                //rb.drag = heldDrag; **

                //bool isLerping = Vector3.Distance(transform.position, CharacterControllerScript.instance.heldObjectPosition.position) <= 0.1 ? false : true; **

                //currentLerpTime += Time.deltaTime;
                //if (currentLerpTime > lerpTime)
                //{
                //    currentLerpTime = lerpTime;
                //    isLerping = false;
                //}

                //lerp
                //float percentComplete = currentLerpTime / lerpTime;
                //rb.MovePosition(Vector3.Lerp(CharacterControllerScript.instance.heldObjectPosition.InverseTransformPoint(startPos), CharacterControllerScript.instance.heldObjectPosition.position, percentComplete));



                //Vector3 movePosition = Vector3.Slerp(transform.position, CharacterControllerScript.instance.heldObjectPosition.position, positioningSpeed * Time.fixedDeltaTime);
                //rb.MovePosition(movePosition);
                //isLerping = Vector3.Distance(transform.position, CharacterControllerScript.instance.heldObjectPosition.position) <= 0.01 ? false : true;


                //Vector3 direction = CharacterControllerScript.instance.heldObjectPosition.position - transform.position;
                //rb.AddRelativeForce(direction.normalized * positioningSpeed, ForceMode.Force);
                //transform.position = CharacterControllerScript.instance.heldObjectPosition.position;


                //raycast to see if collision would occur
                /* **
                RaycastHit hit;
                
                if (Physics.Raycast(transform.position, Vector3.Normalize(transform.position - CharacterControllerScript.instance.heldObjectPosition.position), out hit, objectSize))
                {
                    if (col.bounds.Contains(hit.point))
                    {
                        transform.position = Vector3.MoveTowards(transform.position, hit.point - (Vector3.Normalize(transform.position - CharacterControllerScript.instance.heldObjectPosition.position) * objectSize), 10f * Time.deltaTime);
                        willCollide = true;
                    }
                    else
                        willCollide = false;
                }
                */ // **

                    /* **

                if (isLerping && false)
                {
                    Vector3 target = CharacterControllerScript.instance.heldObjectPosition.transform.position + CharacterControllerScript.instance.heldObjectPosition.transform.forward * 1f;
                    Vector3 force = target - transform.position;
                    rb.AddForce(force * positioningSpeed);
                }
                //transform.rotation = Quaternion.RotateTowards(transform.rotation, CharacterControllerScript.instance.heldObjectPosition.rotation, 5f);
                */
                
                //transform.rotation = new Quaternion( 0f, CharacterControllerScript.instance.transform.rotation.y, 0f, CharacterControllerScript.instance.transform.rotation.w);
            }
            else
            {
                //transform.position = CharacterControllerScript.instance.heldObjectPosition.position; //hanled in update
            }

            if (rotateToHeld)
                transform.rotation = CharacterControllerScript.instance.heldObjectPosition.rotation;
            else
                transform.rotation = CharacterControllerScript.instance.transform.rotation;
        }
        else
            rb.drag = normalDrag;
    }

    private void Pickup()
    {
        held = true;
        //rb.isKinematic = true;
        rb.useGravity = false;
        //transform.parent = CharacterControllerScript.instance.pCam.transform;

        currentLerpTime = 0;
        startPos = CharacterControllerScript.instance.heldObjectPosition.TransformPoint(transform.position); //start pos in the cameras local space
    }

    private void OnCollisionEnter(Collision collision)
    {
        colliding = true;
        contact = collision;

        //Debug.Log("Collide");
    }

    private void OnCollisionStay(Collision collision)
    {
        colliding = true;
        contact = collision;
    }

    private void OnCollisionExit(Collision collision)
    {
        colliding = false;
    }

    private void Drop()
    {
        held = false;
       // rb.isKinematic = false;
        rb.useGravity = true;
        //transform.parent = null;
    }

    private bool CheckPoint() //return true if collides
    {
        Collider[] hits = Physics.OverlapBox(CharacterControllerScript.instance.heldObjectPosition.position, boxDimensions, transform.rotation, ~collisionIgnore);

        if (hits.Length > 0)
        {

            if (hits.Length == 1)
                if (hits[0].gameObject.Equals(gameObject))
                    return false; //if it only hits itself, return false
                    
            
            return true;
        }
        RaycastHit hit;

        // if raycast hits, it checks if it hit this
        //if (Physics.Raycast(CharacterControllerScript.instance.pCam.transform.position, CharacterControllerScript.instance.pCam.transform.position - CharacterControllerScript.instance.heldObjectPosition.position, 
        //    out hit, Vector3.Distance(CharacterControllerScript.instance.pCam.transform.position, CharacterControllerScript.instance.heldObjectPosition.position), ~collisionIgnore))
        //    return true; //if anything is hit while casting towards point, then somthing is blocking the view and the heldpoint is not visible, so dont move towards it
        return false;
    }

    private void UpdatePos()
    {
        //if (colliding)
        //    if (CheckPoint())
        //        return;
        if (!CheckPoint())
        {
            if (Vector3.Distance(transform.position, CharacterControllerScript.instance.heldObjectPosition.position) > 0.5f)
                transform.position = Vector3.MoveTowards(transform.position, CharacterControllerScript.instance.heldObjectPosition.position, 20f * Time.deltaTime);
            else
                transform.position = CharacterControllerScript.instance.heldObjectPosition.position;
        }
        else
            rb.velocity = Vector3.zero;
    }

}

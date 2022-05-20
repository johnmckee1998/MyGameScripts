using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BomberFlyover : MonoBehaviour
{
    public float speed = 10f;

    public GameObject bomb;
    public Transform bombDropPos;
    public float timeBetweenBombs = 2f;
    [Header("Too be used")]
    public Vector2 xMinMaxBoundary;
    public Vector2 zMinMaxBoundary;
    private Vector3 minPosition;
    private Vector3 maxPosition;
    private Vector3 center;
    private float timer =0;

    public Vector2 minMaxWait = new Vector2(1,10);

    private bool startFlying;
    // Start is called before the first frame update
    void Start()
    {
        UpdateBoundary();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //need to put a check for when it gets really far away to despawn/respawn
        if (transform.position.x < minPosition.x - 250f || transform.position.z < minPosition.z - 250f) //once it passes 250m past min value, destroy - will need to be changed if random direction is used, maybe distance from center?
            Destroy(gameObject);
        
        if (startFlying && timer < Time.time && (transform.position.x >= minPosition.x && transform.position.z >= minPosition.z && transform.position.x <= maxPosition.x && transform.position.z <= maxPosition.z))
            DropBomb();

        //Debug.Log(">minx: " + (transform.position.x >= minPosition.x) + " >minz: " + (transform.position.z >= minPosition.z));
        //Debug.Log("<maxX: " + (transform.position.x <= maxPosition.x) + " <maxZ: " + (transform.position.z >= maxPosition.z));

        if(startFlying)
            transform.Translate(transform.forward * speed * Time.fixedDeltaTime, Space.World);
    }

    private void DropBomb()
    {
        Instantiate(bomb, bombDropPos.position, bombDropPos.rotation);
        timer = Time.time + timeBetweenBombs;
    }

    private void UpdateBoundary()
    {
        minPosition = new Vector3(xMinMaxBoundary.x, transform.position.y, zMinMaxBoundary.x);
        maxPosition = new Vector3(xMinMaxBoundary.y, transform.position.y, zMinMaxBoundary.y);
        center = new Vector3((xMinMaxBoundary.x + xMinMaxBoundary.y)/2f, transform.position.y, (zMinMaxBoundary.x + zMinMaxBoundary.y) / 2f);
    }


    private void OnDrawGizmosSelected()
    {
        UpdateBoundary();

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(center, 1f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(minPosition, maxPosition);
        Gizmos.DrawLine(minPosition,new Vector3(xMinMaxBoundary.x, transform.position.y, zMinMaxBoundary.y) ); //00 -> 01
        Gizmos.DrawLine(minPosition,new Vector3(xMinMaxBoundary.y, transform.position.y, zMinMaxBoundary.x) ); //00 -> 10
        Gizmos.DrawLine(new Vector3(xMinMaxBoundary.y, transform.position.y, zMinMaxBoundary.x), maxPosition ); //10 -> 11
        Gizmos.DrawLine(new Vector3(xMinMaxBoundary.x, transform.position.y, zMinMaxBoundary.y), maxPosition ); //01 -> 11
    }

    public void SpecialEvent()
    {
        xMinMaxBoundary.x = ModularBuildingSpawn.instance.transform.position.x;
        zMinMaxBoundary.x = ModularBuildingSpawn.instance.transform.position.z;

        xMinMaxBoundary.y = xMinMaxBoundary.x + (ModularBuildingSpawn.instance.GridX*ModularBuildingSpawn.instance.buildingSize);
        zMinMaxBoundary.y = zMinMaxBoundary.x + (ModularBuildingSpawn.instance.GridZ*ModularBuildingSpawn.instance.buildingSize);

        UpdateBoundary();

        transform.position = new Vector3(maxPosition.x + 250f, transform.position.y, center.z); //move plane to z midpoint and 250 meters past the max x position
        transform.forward = Vector3.left; //face left since it spawned to the right

        StartCoroutine(WaitToStart(Random.Range(minMaxWait.x, minMaxWait.y)));
        //update position and boundaries, start flying
        //add a delay or even wait for another event so the bombing doesnt spawn at the start every time
    }

    IEnumerator WaitToStart(float f)
    {
        yield return new WaitForSeconds(f);
        startFlying = true;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurvivalSpecialRoom : MonoBehaviour
{
    public int roundToActivate = 10;

    [Serializable]
    public struct DoorInfo
    {
        public Transform door;
        public Transform openPos;
        [HideInInspector]
        public float closeRate;
    }

    [Header("Door Stuff")]
    [SerializeField]
    public DoorInfo[] doors;
    public AudioSource openSound;
    public float openSpeedFactor = 1f;

    private bool opened;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < doors.Length; i++) //set up close rates for each door
            doors[i].closeRate = Vector3.Distance(doors[i].door.transform.position, doors[i].openPos.position) * Time.fixedDeltaTime * openSpeedFactor;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (WaveManagerScript.instance.GetRound() >= roundToActivate && !opened)
            StartCoroutine(OpenRoom());
    }

    private IEnumerator OpenRoom()
    {
        Debug.Log("Opening....");
        opened = true;
        yield return new WaitForSeconds(2.5f);
        if (openSound != null)
            openSound.Play();
        bool open = false;
        while (!open)
        {
            Debug.Log("Loop " + Vector3.Distance(doors[0].door.transform.position, doors[0].openPos.position));
            if (Vector3.Distance(doors[0].door.transform.position, doors[0].openPos.position) > 0.1f) //not opened
            {
                for (int i = 0; i < doors.Length; i++)
                    doors[i].door.transform.position = Vector3.MoveTowards(doors[i].door.transform.position, doors[i].openPos.position, doors[i].closeRate);
            }
            else
                open = true;
            yield return new WaitForFixedUpdate();
        }

    }

    public void CloseRoom()
    {

    }

}

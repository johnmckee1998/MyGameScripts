using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GlobalStats : MonoBehaviour
{
    public static GlobalStats instance;

    [HideInInspector]
    public bool isPaused;

    public bool randomiseWind;
    public Vector3 maxWindValues;
    public Vector3 windforce = Vector3.zero;
    [Space]
    public Volume gasPosProcess;
    public Volume burnPostProcess;
    
    [System.Serializable]
    public struct GlobalObjects
    {
        public string id;
        public GameObject obj;
    }
    [Space]
    [Tooltip("Not an object pool - used as a customisable set of universal references - like how the gas and burn post process volumes are stored and reference through this script, this array allows for scene specific global variables/objects")]
    public GlobalObjects[] globalObjs;
    [HideInInspector]
    public bool playerInside;
    private bool gasOn;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        if (randomiseWind)
            windforce = new Vector3(Random.Range(-maxWindValues.x, maxWindValues.x), Random.Range(-maxWindValues.y, maxWindValues.y), Random.Range(-maxWindValues.z, maxWindValues.z));
    }

    private void Update()
    {
        if (gasOn)
        {
            if (gasPosProcess != null)
            {
                if (gasPosProcess.weight < 1)
                {
                    gasPosProcess.weight += Time.deltaTime;
                }
            }
        }
        else
        {
            if (gasPosProcess != null)
            {
                if (gasPosProcess.weight > 0)
                {
                    gasPosProcess.weight -= Time.deltaTime;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (gasOn)
            gasOn = false;//manually turn it off each fixedupdate - will be turned back on if the player is inside gas, but if the gas is destroyed or player leaves gas then this will ensure the pp doesnt stay on (works better than relying on triggerenter/exit
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + windforce);
    }

    public void PlayerInside(bool b)
    {
        playerInside = b;
    }

    public void GasOn()
    {
        gasOn = true;
    }

    public void GasOff()
    {
        gasOn = false;
    }

    public GameObject GetGlobalObj(string ID)
    {
        for (int i = 0; i < globalObjs.Length; i++)
            if (globalObjs[i].id == ID)
                return globalObjs[i].obj;

        return null;
    }
}

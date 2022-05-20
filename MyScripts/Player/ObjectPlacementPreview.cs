using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ObjectPlacementPreview : MonoBehaviour
{
    public LayerMask raycastIgnore;
    public int navmeshLayerMask = 16;
    public float placeDist = 5f;
    public Vector3 offset;
    public float turnRate = 90f;
    public Material validMat;
    public Material invalidMat;
    private MeshRenderer[] rens;
    private bool validPlace;
    public MeshRenderer fovRen;
    [HideInInspector]
    public bool colliding;
    public bool useSnapPoints;
    [Space]
    [Tooltip("if false, uses large turret placement points")]
    public bool useNormalPoints = true;
    private int snapIndex;
    private Transform snapPoint;
    private RaycastHit rHit;
    // Start is called before the first frame update
    void Start()
    {
        rens = GetComponentsInChildren<MeshRenderer>();
        if (useSnapPoints)
            StartCoroutine(UpdateSnapPoint());
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePreview();

        if (Input.GetKeyDown("q") && Time.timeScale > 0)
            transform.eulerAngles -= Vector3.up * 45f;
        else if (Input.GetKeyDown("e") && Time.timeScale > 0)
            transform.eulerAngles += Vector3.up*45f;

        CanvasScript.instance.popUp.text = " Q+E To Rotate \n Left Click to Place ";
        /* //old style, uses constant rotation - more options, but slow and probably not needed
        if (Input.GetKey("q") && Time.timeScale > 0)
            transform.eulerAngles -= Vector3.up * Time.deltaTime * turnRate;
        else if (Input.GetKey("e") && Time.timeScale > 0)
            transform.eulerAngles += Vector3.up * Time.deltaTime * turnRate;
            */
    }

    private void UpdatePreview()
    {
        
        if(Physics.Raycast(CharacterControllerScript.instance.pCam.transform.position, CharacterControllerScript.instance.pCam.transform.forward, out rHit , placeDist, ~raycastIgnore))
        {

            //for (int i = 0; i < rens.Length; i++)
            //    rens[i].enabled = true;
            if (!useSnapPoints)
            {
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(rHit.point, out navHit, 0.1f, navmeshLayerMask))
                {
                    validPlace = true; //hits a vaid point on navmesh
                                       //Debug.Log("Valid Hit!");
                }
                else
                {
                    //NavMesh.SamplePosition(rHit.point, out navHit, 0.1f, NavMesh.AllAreas); //used for debugging - gets area no matter what
                    //Debug.Log("Invalid Area: " + navHit.mask);
                    validPlace = false;
                }
                transform.position = rHit.point + offset;
            }
            else //using snap points
            {
                if (snapIndex >= 0)//valid point
                {
                    validPlace = true;
                    transform.position = snapPoint.position + offset;
                }
                else
                {
                    validPlace = false;
                    transform.position = rHit.point + offset;
                }

            }
            
            
        }
        else
        {
            if (!useSnapPoints || snapIndex < 0)
            {
                validPlace = false;
                //for (int i = 0; i < rens.Length; i++)
                //    if (fovRen == null || !rens[i].Equals(fovRen))
                //        rens[i].material = invalidMat;
                //        rens[i].enabled = false;
                transform.position = CharacterControllerScript.instance.pCam.transform.position + (CharacterControllerScript.instance.pCam.transform.forward * placeDist) + offset;
            }
            else //using snap points
            {
                validPlace = true;
                transform.position = snapPoint.position + offset;
            }
        }
        for (int i = 0; i < rens.Length; i++)
        {
            rens[i].enabled = true;//make sure its enabled coz it may have been disabled last update
            if (fovRen == null || !rens[i].Equals(fovRen))
            {
                if (validPlace && !colliding)
                    rens[i].material = validMat;
                else
                    rens[i].material = invalidMat;
            }
        }
    }

    public bool IsValidPlace()
    {
        if (!useSnapPoints)
            return validPlace && !colliding;
        else
            return (snapIndex >= 0); //if snap index is negative, then invalid
    }

    public Vector3 GetPlacePos()
    {
        return transform.position;
    }
    /*
    private void OnCollisionStay(Collision collision)
    {
        colliding = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        colliding = false;
    }
    */

    private void OnTriggerStay(Collider other)
    {
        colliding = true;
    }

    private void OnTriggerExit(Collider other)
    {
        colliding = false;
    }

    IEnumerator UpdateSnapPoint()
    {
        TowerDefenceWaveManager.instance.ShowTurretPoints(useNormalPoints);
        while (true)
        {
            if (rHit.point != null)
            {
                if (useNormalPoints)
                    snapIndex = TowerDefenceWaveManager.instance.GetClosestTurretPoint(rHit.point - offset, 1.5f);
                else
                    snapIndex = TowerDefenceWaveManager.instance.GetClosestLargeTurretPoint(rHit.point - offset, 1.5f);
            }
            else
            {
                if (useNormalPoints)
                    snapIndex = TowerDefenceWaveManager.instance.GetClosestTurretPoint(CharacterControllerScript.instance.pCam.transform.position + (CharacterControllerScript.instance.pCam.transform.forward * placeDist) - offset, 2.5f);
                else
                    snapIndex = TowerDefenceWaveManager.instance.GetClosestLargeTurretPoint(CharacterControllerScript.instance.pCam.transform.position + (CharacterControllerScript.instance.pCam.transform.forward * placeDist) - offset, 2.5f);
            }
            if (snapIndex>=0)
                if(useNormalPoints)
                    snapPoint = TowerDefenceWaveManager.instance.turretPoints[snapIndex];
                else
                    snapPoint = TowerDefenceWaveManager.instance.turretLargePoints[snapIndex];
            //Debug.Log("SnapUpdated: " + snapIndex);
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void Placed()
    {
        if (useSnapPoints)
            TowerDefenceWaveManager.instance.TurretPointUsage(snapIndex, true, useNormalPoints);
    }

    private void OnDisable()
    {
        TowerDefenceWaveManager.instance.HideTurretPoints(useNormalPoints);
    }
}

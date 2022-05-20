using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class ModularSceneSetup : MonoBehaviour
{
    public ModularBuildingSpawn modBuild;
    public ProceduralEnemySpawner modEnemy;
    public CharacterControllerScript pScript; //might actually be a good idea to leave this as variable and not use charscript.instance, because the player starts disabled so the instance is not assigned yet
    public MonoBehaviour checkScript;

    public ProceduralPickupPlacement modPickups;

    public GameObject InputCanvas;

    public NavMeshSurface nav;

    public GameObject overCam;

    public bool pressed = false;

    [Header("Transition Stuff")]
    public Image transitionImage;
    public SimpleTransition playerTransition;

    private bool useRandomGuns = false;

    public GameObject[] guns;

    private bool useGunChoices; //whether or not to use player chosen guns
    private bool[] gunChoices;

    public bool useSpecialEvent;
    public GameObject specialEvent;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GenerateScene());
        if (transitionImage != null)
            transitionImage.gameObject.SetActive(false);
        if (playerTransition != null)
            playerTransition.startTransition = false;

        gunChoices = new bool[guns.Length];

    }

    IEnumerator GenerateScene()
    {
        pScript.gameObject.SetActive(false); //disable player
        overCam.SetActive(true); //enable overviewcam
        //CanvasScript.instance.gameObject.SetActive(false);
        //Use inputs
        bool started = true;
        while (started)
        {
            if (pressed) { //wait till the start button is pressed
                modBuild.PlaceBuildings(); //place buildings/generate map
                yield return new WaitForSeconds(0.1f);
                //UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
                nav.BuildNavMesh(); //build the navmesh
                yield return new WaitForSeconds(0.1f);
                if (AIPathManager.instance != null)
                    AIPathManager.instance.UpdateAllPaths();
                yield return new WaitForSeconds(0.1f);
                pScript.gameObject.SetActive(true);
                overCam.SetActive(false);
                modEnemy.SetSize(modBuild.buildingSize, modBuild.GridX, modBuild.GridZ);
                modEnemy.PlaceEnemies();
                modPickups.SetSize(modBuild.buildingSize, modBuild.GridX, modBuild.GridZ);
                modPickups.PlacePickups();
                yield return null;
                if(checkScript!=null)
                    checkScript.enabled = true;
                started = false;
                yield return null;
                InputCanvas.SetActive(false);
                //Debug.Log("ye");
                if (useRandomGuns)
                   StartCoroutine(RandomiseGuns());
                else if(useGunChoices)
                    StartCoroutine(GiveGunChoices());
                //CanvasScript.instance.gameObject.SetActive(true);
                if (playerTransition!=null)
                    playerTransition.startTransition = true;
                if (useSpecialEvent)
                    specialEvent.SendMessage("SpecialEvent", SendMessageOptions.DontRequireReceiver);
            }
            yield return null;
        }

        
    }

    public void StartButton()
    {
        StartCoroutine(Transition());
        //pressed = true;
    }

    private IEnumerator Transition()
    {
        if (transitionImage != null)
        {
            transitionImage.gameObject.SetActive(true);
            while (transitionImage.fillAmount < 1)
            {
                transitionImage.fillAmount += Time.deltaTime*2;
                yield return null;
            }
        }
        pressed = true;
    }


    public void RandomGuns(bool b)
    {
        useRandomGuns = b;
    }


    private IEnumerator RandomiseGuns()
    {
        yield return null;
        //remove all exsisting guns
        foreach(Transform child in WeaponSelection.instance.transform)
        {
            Destroy(child.gameObject);
        }
        yield return null;
        //Generate New ones
        if (guns.Length > 1)
        {
            int rand1 = Random.Range(0, guns.Length);
            int rand2 = Random.Range(0, guns.Length);
            while (rand2 == rand1) //make sure reand2 isnt rand1
                rand2 = Random.Range(0, guns.Length);

            GameObject g = Instantiate(guns[rand1]);
            WeaponSelection.instance.AddGun(g);
            yield return null;
            GameObject g2 = Instantiate(guns[rand2]);
            WeaponSelection.instance.AddGun(g2);
            yield return null;
            WeaponSelection.instance.SetCurrentWeapon(0);
        }
        else
        {
            GameObject g = Instantiate(guns[0]);
            WeaponSelection.instance.AddGun(g);
        }


    }

    private IEnumerator GiveGunChoices()
    {
        yield return null;
        //remove all exsisting guns
        foreach (Transform child in WeaponSelection.instance.transform)
        {
            Destroy(child.gameObject);
        }
        yield return null;
        //Generate New ones
        for (int i =0; i < gunChoices.Length; i++)
        {
            if (gunChoices[i])
            {
                GameObject g = Instantiate(guns[i]);
                WeaponSelection.instance.AddGun(g);
                yield return null;
                
            }
        }

        WeaponSelection.instance.SetCurrentWeapon(0);
    }

    public void ToggleGunChoice(int i)
    {
        gunChoices[i] = !gunChoices[i];
    }

    public void UseCustomChoices()
    {
        useGunChoices = !useGunChoices;
    }

    public void UseCustomChoices(bool b)
    {
        useGunChoices = b;
    }
}

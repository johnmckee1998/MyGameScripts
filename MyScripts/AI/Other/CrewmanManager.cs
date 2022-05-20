using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrewmanManager : MonoBehaviour
{
    public static CrewmanManager instance; 
    //tecnically this isnt a good way of setting up a singleton. Usually you would have a private static instance (and all main variable are private) and a public static get method. That way while you could access the instance anywhere, since you
    //only had a get method you could not overwrite the instance with a new one or mess with it in any way that didnt involve a function. My way could result in issues if you wanted to cause them, though the way i use them means i probably wont
    //have any issue

    public List<TurretCrewmanAI> crewman = new List<TurretCrewmanAI>();


    public Transform crewmanGatherPoint;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    public void AddCrewman(TurretCrewmanAI crew)
    {
        crewman.Add(crew);
    }

    public void RemoveCrewman(TurretCrewmanAI crew)
    {
        crewman.Remove(crew);
    }

    public void GetNextCrewmanSearch()
    {
        foreach(TurretCrewmanAI crew in crewman)
        {
            if (crew.IsAvailable())
            {
                crew.FindTurret();
                return;
            }
        }

        CanvasScript.instance.DisplayMessage("No Spare Crew Available", 2.5f);
    }
}

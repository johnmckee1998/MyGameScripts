using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeTurret : MonoBehaviour
{
    [System.Serializable]
    public struct Upgrade
    {
        public GameObject gun;
        public int cost;
        public string txt;
    }
    public Upgrade[] upgrades;
    public PlayerAutoTurret turretScript;
    public GameObject curretGun; 
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

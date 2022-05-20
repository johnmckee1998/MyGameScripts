using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TDStealthGunEnemy : TDGunEnemy
{
    [HideInInspector]
    public bool isVisible;
    public Material mat;
    private float dissolveAmount;
    private MeshRenderer[] rens;

    // Start is called before the first frame update
    void Start()
    {
        navmeshAgent = GetComponent<NavMeshAgent>();
        unistats = GetComponent<UniversalStats>();

        //targetIndex = TDPlayerBase.instance.GetClosestBase(transform.position);

        target = TowerDefenceWaveManager.instance.GetRandomPlayerTarget();
        tarStats = target.GetComponent<UniversalStats>();
        StartCoroutine(DestinationUpdate());

        mat = new Material(mat); //make duplicate that is unqiue to this enemy

        rens = GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer m in rens)
            m.material = mat;

        dissolveAmount = -1;
        mat.SetFloat("Dissolve", dissolveAmount);
    }

    private void Update()
    {
        if (inRange&&seeTarget) //decloak
        {
            dissolveAmount += 0.5f * Time.deltaTime;
            dissolveAmount = Mathf.Clamp(dissolveAmount, -1f, 1f);
            mat.SetFloat("Dissolve", dissolveAmount);
        }
        else //cloak
        {
            dissolveAmount -= 0.5f * Time.deltaTime;
            dissolveAmount = Mathf.Clamp(dissolveAmount, -1f, 1f);
            mat.SetFloat("Dissolve", dissolveAmount);
        }

        
        isVisible = (dissolveAmount >= 0.9);
    }

    protected override void ShootTarget()
    {
        if(isVisible)
            base.ShootTarget();
    }

}

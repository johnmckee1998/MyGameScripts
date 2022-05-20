using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectedAI : MonoBehaviour
{
    public GameObject futureGuard;
    public GameObject replacementGuard;

    private UniversalStats curGuardAi;
    // Start is called before the first frame update
    void Start()
    {
        futureGuard.SetActive(true);
        replacementGuard.SetActive(false);

        curGuardAi = GetComponent<UniversalStats>();
    }

    // Update is called once per frame
    void Update()
    {
        if (curGuardAi.health <= 0)
            SwitchGuards();
    }

    private void SwitchGuards()
    {
        futureGuard.SetActive(false); //kill future instace of this guard
        replacementGuard.SetActive(true); //enable replacement
    }
}

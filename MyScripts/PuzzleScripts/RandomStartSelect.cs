using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomStartSelect : MonoBehaviour
{
    public GameObject[] Sets;
    public int amountToSelect = 1;
    // Start is called before the first frame update
    void Start()
    {

        if (amountToSelect < Sets.Length)
        {
            foreach (GameObject g in Sets) //start by disabling everything
                g.SetActive(false);

            int[] randSelects = new int[amountToSelect];
            for (int i = 0, j = 0; i < randSelects.Length && j < amountToSelect; i++)
            {
                int rand = Random.Range(0, Sets.Length);
                if (j == 0 || !ArrayContains(randSelects, rand)) //if this is the first index or if it does not contain this number
                {
                    randSelects[j] = rand;
                    j++;
                }
                else
                {
                    i--;
                }

            }

            for (int i = 0; i < randSelects.Length; i++)
                Sets[randSelects[i]].SetActive(true);
        }
        else //if amount to select is all, then enable alls
            foreach (GameObject g in Sets)
                g.SetActive(true);
    }

    private void Update()
    {
        Destroy(gameObject);
    }

    private bool ArrayContains(int[] a, int i)
    {
        for (int j = 0; j < a.Length; j++)
            if (a[j] == i)
                return true;

        return false;
    }
}

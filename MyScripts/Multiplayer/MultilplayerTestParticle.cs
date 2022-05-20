using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class MultilplayerTestParticle : NetworkBehaviour
{
    [SerializeField] private GameObject particlePrefab;

    private void Update()
    {
        if (!IsOwner) return;//not ours, fuck off

        if (!Input.GetKeyDown(KeyCode.Space))
        {
            return;
        }
        SpawnParticleServerRpc();

        //spawn locally
        Instantiate(particlePrefab, transform.position, transform.rotation);
    }

    //unreliable means that if there is an issue, dont both trying again since it isnt important 
    //reliable means it would keep trying (reliable is default, no need to specify if using reliable)
    [ServerRpc(Delivery = RpcDelivery.Unreliable)] 
    private void SpawnParticleServerRpc()
    {
        SpawnPrticleClientRpc();
    }

    [ClientRpc(Delivery = RpcDelivery.Unreliable)]
    private void SpawnPrticleClientRpc()
    {
        if (IsOwner) return;//spawned myself anyway, no need to do it again

        Instantiate(particlePrefab, transform.position, transform.rotation);
    }
}

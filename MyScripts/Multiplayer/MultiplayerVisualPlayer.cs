using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Beantlefield.Multi.ConnectionApproval;
using UnityEngine.EventSystems;
using System;

public class MultiplayerVisualPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject playerUIPrefab;
    [SerializeField] private GameObject eventSystem;
    [SerializeField] private float playerHeight;
    [SerializeField] private Collider col;
    [SerializeField] private GameObject[] clientDisable;
    //public GameObject visualModel;

    private GameObject player;
    private GameObject playerUI;
    //private GameObject pModel;

    private NetworkVariable<Vector3> position = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> rotation = new NetworkVariable<Quaternion>();
    private NetworkVariable<Vector3> newScenePosition = new NetworkVariable<Vector3>();

    private void Start()
    {
        if (IsOwner)
        {
            if (PasswordNetworkManager.instance != null)
                PasswordNetworkManager.instance.UpdateLocalPlayer(this);
            player = Instantiate(playerPrefab, transform.position, transform.rotation);
            playerUI = Instantiate(playerUIPrefab);

            foreach (GameObject g in clientDisable)
                g.SetActive(false);// disable these objects -> used to reference things like player model or parts of it which isnt needed on the clinet  (and may interfere with camera view)

            if (EventSystem.current == null)
            {
                Instantiate(eventSystem);
            }

            //pModel = Instantiate(visualModel, transform.position, transform.rotation);
            if (col != null)
                col.enabled = false;
        }
        else
        {
            //enabled = false;
            return;
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            if (player == null)
            {
                //    SpawnNewPlayer();
                Debug.Log("Player null");
                try
                {
                    player = FindObjectOfType<CharacterController>().gameObject;
                }
                catch
                {
                    Debug.Log("Failed to find player");
                }
                return;
            }

            transform.position = player.transform.position + (Vector3.up * playerHeight);
            transform.rotation = player.transform.rotation;

            UpdatePosRotServerRpc(transform.position, transform.rotation);
        }
        /*else
        {
            //non owner use position and rotation
            transform.position = position.Value;
            transform.rotation = rotation.Value;
        }*/
    }

    private void SpawnNewPlayer()
    {
        if (player == null)
        {
            Debug.Log("new player");
            //this happens on scene loading, respawn player
            player = Instantiate(playerPrefab, transform.position, transform.rotation);
        }

        if (playerUI == null)
        {
            playerUI = Instantiate(playerUIPrefab);
            if (EventSystem.current == null)
            {
                Instantiate(eventSystem);
            }
        }
    }

    public void UseNewSpawn(Vector3[] spawns, Vector3 defaultSpawn)
    {
        MultiplayerVisualPlayer[] players = FindObjectsOfType<MultiplayerVisualPlayer>();

        foreach (MultiplayerVisualPlayer p in players)
            p.UpdateRealPlayerPosServerRpc(spawns, defaultSpawn);

        //UpdateRealPlayerPosServerRpc(spawns, defaultSpawn);
    }

    [ServerRpc]
    public void UpdateRealPlayerPosServerRpc(Vector3[] spawns, Vector3 defaultSpawn) //update the actual player pos, not the visual model
    {
        
        if ((int)NetworkManager.Singleton.LocalClientId < spawns.Length)
            newScenePosition.Value = spawns[(int)NetworkManager.Singleton.LocalClientId];
        else
            newScenePosition.Value = defaultSpawn;

        //ClientUpdateRealPlayerPosClientRpc(spawns, defaultSpawn);
    }

    
    [ClientRpc]  
    private void ClientUpdateRealPlayerPosClientRpc(Vector3[] spawns, Vector3 defaultSpawn)
    {
        if ((int)NetworkManager.Singleton.LocalClientId < spawns.Length)
            newScenePosition.Value = spawns[(int)NetworkManager.Singleton.LocalClientId];
        else
            newScenePosition.Value = defaultSpawn;
        /*
        if (player != null)
        {
            if ((int)NetworkManager.Singleton.LocalClientId < spawns.Length)
                player.transform.position = spawns[(int)NetworkManager.Singleton.LocalClientId];
            else
                player.transform.position = defaultSpawn;

            transform.position = player.transform.position;
        }
        else
        {
            if ((int)NetworkManager.Singleton.LocalClientId < spawns.Length)
                transform.position = spawns[(int)NetworkManager.Singleton.LocalClientId];
            else
                transform.position = defaultSpawn;
        }

        Debug.Log("Updated PlayerPos");*/

    }

    [ServerRpc]
    public void UpdatePosRotServerRpc(Vector3 pos, Quaternion rot)
    {
        position.Value = pos;
        rotation.Value = rot;
    }

    private void OnEnable()
    {
        position.OnValueChanged += OnPosChanged;
        rotation.OnValueChanged += OnRotChanged;

        newScenePosition.OnValueChanged += OnNewScenePos;
    }

    private void OnNewScenePos(Vector3 previousValue, Vector3 newValue)
    {
        if (player == null)
            SpawnNewPlayer();

        if (player != null)
        {
            player.transform.position = newScenePosition.Value;
        }
        else
        {
            transform.position = newScenePosition.Value;
        }
        Debug.Log("Updated Position: " + NetworkManager.Singleton.LocalClientId + " Is Host? " + NetworkManager.Singleton.IsHost);
    }

    private void OnDisable()
    {
        position.OnValueChanged -= OnPosChanged;
        rotation.OnValueChanged -= OnRotChanged;
    }

    private void OnPosChanged(Vector3 oldPos, Vector3 newPos)
    {
        if (!IsClient || IsOwner) return; //only clients update? idk 

        //non owner update transform
        transform.position = position.Value;
    }

    private void OnRotChanged(Quaternion oldRot, Quaternion newRot)
    {
        if (!IsClient || IsOwner) return; //only clients update? idk 

        //non owner update transform
        transform.rotation = rotation.Value;
    }

}

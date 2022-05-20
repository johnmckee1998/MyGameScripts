using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using System;
using System.Text;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;

namespace Beantlefield.Multi.ConnectionApproval
{
    public class PasswordNetworkManager : MonoBehaviour
    {
        public static PasswordNetworkManager instance;
        [SerializeField] private TMP_InputField passwordInput;
        //[SerializeField] private UnityTransport transport;
        [SerializeField] private GameObject passwordUI;
        [SerializeField] private GameObject leaveButton;
        [SerializeField] private GameObject tempCamera;
        [SerializeField] private bool passwordCheck = true;

        private MultiplayerVisualPlayer localPlayer;

        private bool sceneLoaded;
        private MultiplayerPlayerSpawns nextSceneInfo;

        private void  Start()
        {
            instance = this;
            //NetworkManager.Singleton.OnServerStarted += HandleServerStarted; //in the tut but no longer needed due to bugfix
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton == null)
                return;

            //NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
        }

        public void Host()
        {
            //transport.ConnectionData.Address = NetworkManager.Singleton//make this use current ip
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            //NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += HandleLoadSceneComplete;
            NetworkManager.Singleton.StartHost();
        }

        public void Client()
        {
            NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(passwordInput.text);
            
            NetworkManager.Singleton.StartClient();
        }

        public void Leave()
        {
            NetworkManager.Singleton.Shutdown();

            if (NetworkManager.Singleton.IsHost)
            {
                //NetworkManager.Singleton.Shutdown();
                NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
            }
            //else if (NetworkManager.Singleton.IsClient)
            //{
            //    NetworkManager.Singleton.Shutdown();
            //}

            if(sceneLoaded)
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= HandleLoadSceneComplete;
            if (passwordUI != null)
                passwordUI.SetActive(true);
            if (leaveButton != null)
                leaveButton.SetActive(false);
            if (tempCamera != null)
                tempCamera.SetActive(true);
        }

        public void LoadScene(MultiplayerPlayerSpawns sceneInfo)
        {
            if (!NetworkManager.Singleton.IsHost)
                return; //only host can load level

            if (Application.CanStreamedLevelBeLoaded(sceneInfo.sceneID)) //checks that it is a valid scene
            {
                nextSceneInfo = sceneInfo;
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += HandleLoadSceneComplete;
                NetworkManager.Singleton.SceneManager.LoadScene(sceneInfo.sceneID, LoadSceneMode.Single);
            }
            else
                Debug.LogWarning("Invalid scene ID (LoadScene): " + sceneInfo.sceneID);

            //NetworkManager.Singleton.clie
            //Debug.Log(NetworkManager.Singleton.LocalClientId);
        }

        public void UpdateLocalPlayer(MultiplayerVisualPlayer g)
        {
            localPlayer = g;
        }

        private void HandleClientConnected(ulong clientID)
        {
            if(clientID == NetworkManager.Singleton.LocalClientId)
            {
                if(passwordUI!=null)
                    passwordUI.SetActive(false);
                if(leaveButton!=null)
                    leaveButton.SetActive(true);
                if(tempCamera!=null)
                    tempCamera.SetActive(false);
            }
        }

        private void HandleClientDisconnect(ulong clientID)
        {
            if (clientID == NetworkManager.Singleton.LocalClientId)
            {
                if (passwordUI != null)
                    passwordUI.SetActive(true);
                if (leaveButton != null)
                    leaveButton.SetActive(false);
                if (tempCamera != null)
                    tempCamera.SetActive(true);
            }
        }

        //connection data in this case is the password
        private void ApprovalCheck(byte[] connectionData, ulong clientID, NetworkManager.ConnectionApprovedDelegate callback)
        {
            //Need to convert byte array to string - its a byte array coz that is how its sent over networks, cant send a string 
            string password = Encoding.ASCII.GetString(connectionData);

            bool approveConnection = password == passwordInput.text;
            if (!passwordCheck)
                approveConnection = true;// override for ignoring password

            //by using null in second param, it will use default player - change this to allow different player types
            //temp setting pos/rot to null, change this later

            Vector3 spawnPos;

            Quaternion spawnRot;

            switch (NetworkManager.Singleton.ConnectedClients.Count)
            {
                case 0:
                    spawnPos = new Vector3(0f, 2f, 0f);
                    spawnRot = Quaternion.identity;
                    break;
                case 1:
                    spawnPos = new Vector3(1f, 2f, 1f);
                    spawnRot = Quaternion.identity;
                    break;
                case 2:
                    spawnPos = new Vector3(2f, 2f, 2f);
                    spawnRot = Quaternion.identity;
                    break;
                default:
                    spawnPos = new Vector3(-1f, 2f, -1f);
                    spawnRot = Quaternion.identity;
                    break;
            }

            if (NetworkManager.Singleton.IsHost)
                callback(true, null, true, spawnPos, spawnRot);
            else
                callback(true, null, approveConnection, spawnPos, spawnRot);
        }

        private void HandleLoadSceneComplete(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            sceneLoaded = true;
            Debug.Log(sceneName + " loaded. Num of clinets: " + clientsCompleted.Count + " Failed connections: " + clientsTimedOut.Count);
            if (localPlayer != null&&nextSceneInfo!=null)
            {
                localPlayer.UseNewSpawn(nextSceneInfo.spawns, nextSceneInfo.defaultSpawn);
            }

            //tell all players to update position
        }

        private void HandleLoadScene(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
        {
            throw new NotImplementedException();
        }
    }
}

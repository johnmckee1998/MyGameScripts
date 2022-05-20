using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Beantlefield.Multi.ConnectionApproval;
using Netcode.Transports;
using System.Text;

public class SteamLobby : MonoBehaviour
{
    [SerializeField] private GameObject ui = null;
    [SerializeField] private ELobbyType lType;
    [SerializeField] private int lobbySize = 4;

    [SerializeField] private PasswordNetworkManager passNetManager;
    [SerializeField] private SteamNetworkingTransport transport;

    [Space]
    [SerializeField] private bool useTest = true;

    //callbacks
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;

    private const string hostAddressKey = "HostAddress";

    //private static ulong hostID;
    //private NetworkManager netManager;

    private void Start()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogWarning("Steam not initialised");
            return;
        }

        if (useTest)
        {
            string myName = SteamFriends.GetPersonaName();

            Debug.Log("ME -> " + myName);
        }

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

        //netManager = GetComponent<NetworkManager>();
    }

    public void HostLobby()
    {
        ui.SetActive(false);

        SteamMatchmaking.CreateLobby(lType, lobbySize);
    }
    

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK) //failed to make lobby
        {
            ui.SetActive(true);
            return;
        }

        passNetManager.Host();

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), hostAddressKey, SteamUser.GetSteamID().ToString());
        //Debug.Log("Lobby Info: " + callback.m_ulSteamIDLobby + " ID: " + SteamUser.GetSteamID());
        //Debug.Log("Lobby Get: " + SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), hostAddressKey) + " ID: " + SteamUser.GetSteamID());
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);


    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (NetworkManager.Singleton.IsHost)
            return; //this bit is only for clients

        string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), hostAddressKey);//not needed? -> steam transport seems to use host id not the ip


        //need to set address!!
        transport.ConnectToSteamID = ulong.Parse(hostAddress);
        passNetManager.Client();
        if (callback.m_bLocked)
            Debug.Log("Blocked?");
        ui.SetActive(false);
    }

}

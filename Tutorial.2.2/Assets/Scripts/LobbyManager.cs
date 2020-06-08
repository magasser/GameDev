﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LobbyManager : NetworkLobbyManager 
{
    // singleton static instance
    private static LobbyManager _instance = null;
    public static LobbyManager instance { get { return _instance; } }

    [Space]                     // Creates a space in the inspector UI
    [Header("UI Reference")]    // Creates a title above the following variables
    public RectTransform        networkPanel;
    public RectTransform        hostAndJoinRect;
    public RectTransform        lobbyRect;
    public RectTransform        playersRect;
    public PlayerListBehaviour  playersListBehaviour;
    public Text                 lobbyTitleText;
    public InputField           remoteIpInput;
    public InputField           playerNameInput;
    public Button               startButton;
    public Button               toggleLobbyButton;

    private RectTransform       _currentPanel;
    private bool                _isServer = false;
    private bool                _showLobbyDuringGame = true;

    // Gets called on startup
    void Start()
    {   
        if(_instance != null) 
        {   Debug.LogError("There can only be one instance of MyLobbyManager at any time.");
            Destroy(gameObject);
        }
        _instance = this;
        _currentPanel = hostAndJoinRect;

        //remoteIpInput.text = Network.player.ipAddress;
    }



    ///////////////////////
    // UI Event Handlers //
    ///////////////////////

    public void OnCreateHostButtonClick()
    {
        //Debug.Log("OnCreateHostButtonClick");
        if (StartHost()!= null)
        {
            _isServer = true;
            ChangeTo(lobbyRect);

            // the start button remains inactive for all others
            startButton.gameObject.SetActive(true);
            toggleLobbyButton.gameObject.SetActive(false);
        }
    }

    public void OnCreateClientButtonClick()
    {
        //Debug.Log("OnCreateClientButtonClick");
        networkAddress = remoteIpInput.text;
        StartClient();
        ChangeTo(lobbyRect);

        // We change the title to "Connecting ..." in case that the host isn't started yet
        lobbyTitleText.text = "Connecting ...";

        toggleLobbyButton.gameObject.SetActive(false);
    }

    public void OnStartButtonClicked()
    {
        //Debug.Log("OnStartButtonClicked");
        ServerChangeScene(playScene);
        startButton.gameObject.SetActive(false);
        toggleLobbyButton.gameObject.SetActive(true);
        ShowLobby(false);
    }

    public void OnStopButtonClicked()
    {
        //Debug.Log("OnStopButtonClicked");
        if(_isServer)
            StopHost();
        else 
            StopClient();

        ShowLobby(true);
        ChangeTo(hostAndJoinRect);
    }

    public void OnToggleLobbyButtuonClicked()
    {
        //Debug.Log("OnToggleLobbyButtuonClicked");
        _showLobbyDuringGame = !_showLobbyDuringGame;
        ShowLobby(_showLobbyDuringGame);
    }

    // Changes between the hostOrJoinRect and the lobbyRect
    void ChangeTo(RectTransform panel)
    {
        _currentPanel.gameObject.SetActive(false);
        _currentPanel = panel;
        _currentPanel.gameObject.SetActive(true);
    }

    // Moves the lobby recangle in to the right or out to the left
    private void ShowLobby(bool show)
    {
        _showLobbyDuringGame = show;
        RectTransform rt = networkPanel.gameObject.GetComponent<RectTransform>();
        rt.anchoredPosition3D = _showLobbyDuringGame ? 
                                    new Vector3(0, 0, 0) : 
                                    new Vector3(-263, 0, 0);
    }



    ///////////////////////////////
    // Overridden Event Handlers //
    ///////////////////////////////

    // Gets called when a client has successfully connected to the server
    public override void OnClientConnect(NetworkConnection conn)
    {
        //Debug.Log("OnClientConnect");
        base.OnClientConnect(conn);
        ChangeTo(lobbyRect);
        lobbyTitleText.text = "Lobby";
        toggleLobbyButton.gameObject.SetActive(false);
    }

    // Gets called when the client has changed into the game scene
    public override void OnLobbyClientSceneChanged(NetworkConnection conn)
    {
        //Debug.Log("OnLobbyClientSceneChanged");
        base.OnLobbyClientSceneChanged(conn);
        ShowLobby(false);
        toggleLobbyButton.gameObject.SetActive(true);
    }

    // Gets called when a client disconnected
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        //Debug.Log("OnClientDisconnect");
        base.OnClientDisconnect(conn);
        StopClient();
        ChangeTo(hostAndJoinRect);
        ShowLobby(true);
    }

    // Gets called when the host has stopped
    public override void OnStopHost()
    {
        //Debug.Log("OnStopHost");
        base.OnStopHost();
        startButton.gameObject.SetActive(false);
    }
}
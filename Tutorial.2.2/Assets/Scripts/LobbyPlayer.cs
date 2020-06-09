using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class LobbyPlayer : NetworkLobbyPlayer 
{
    public RectTransform kickButton;
    public Text nameLabel;

    public GameObject playerPrefab;

    private GameObject _playerCar;

    [SyncVar(hook = "OnNameChanged")]
    public string playerName;

    void Start()
    {
        // activate kick buttons for non local players if we're the server
        kickButton.gameObject.SetActive(isServer && !isLocalPlayer);

        // make sure the name label contains the correct value on creation of the player info object
        OnNameChanged(playerName);
    }

    public override void OnClientEnterLobby()
    {
        //Debug.Log("OnClientEnterLobby");
        base.OnClientEnterLobby();
        LobbyManager.instance.playersListBehaviour.AddPlayer(this);
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        //Debug.Log("OnStartLocalPlayer");
        nameLabel.fontStyle = FontStyle.Bold;
        string name = LobbyManager.instance.playerNameInput.text;

        // Check if player name is unique
        bool nameIsUnique = false;
        while (!nameIsUnique)
        {   nameIsUnique = true;
            foreach (LobbyPlayer lp in LobbyManager.instance.playersListBehaviour.LobbyPlayerList)
            {   if (lp.playerName == name)
                {   name += " 2";
                    nameIsUnique = false;
                    break;
                }
            }
        }
            
        CmdSpawnCar();
        CmdSetName(name);
    }

    [Command]
    void CmdSpawnCar()
    {
        Transform spawnPoint = LobbyManager.instance.playersListBehaviour.GetSpawnPoint();
        if (spawnPoint != null)
        {
            _playerCar = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            NetworkServer.Spawn(_playerCar);
        }

    }

    public void OnNameChanged(string newName)
    {
        //Debug.Log("OnNameChanged: " + newName);
        playerName = newName;
        nameLabel.text = playerName;
    }

    public void OnClientKicked()
    {
        LobbyManager.instance.playersListBehaviour.RemovePlayer(this);
        // Remove kicked players car
        Destroy(_playerCar);
        this.GetComponent<NetworkIdentity>().connectionToClient.Disconnect();
    }

    [Command]
    public void CmdSetName(string newName)
    {
        //Debug.Log("CmdSetName");
        playerName = newName;
    }
}
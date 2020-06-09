using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;

public class TimingNetworkBehaviour : NetworkBehaviour
{
    public TMP_Text serverTime;
    public TMP_Text playerTime;
    public TMP_Text ranking;
    public int countMax = 2;
    [SyncVar(hook = "OnCountdownChanged")]
    private int _countdown;
    [SyncVar]
    private float _pastTime = 0;
    [SyncVar(hook = "OnTextChange")]
    private string rankings = "";
    private float localtime = 0;
    private CarBehaviourNetwork _carScript = null;
    private AudioSource _beep;
    private bool _isStarted = false;
    private bool _isFinished = false;
    void Start()
    {
        Debug.Log("Start called");
        _beep = GetComponent<AudioSource>();
        //serverTime.anchor = TextAnchor.MiddleCenter;
        //serverTime.transform.position = new Vector3(0.5f, 0.9f, 0);
        serverTime.fontSize = (int)(Screen.height / 15.0f);
        //playerTime.anchor = TextAnchor.LowerCenter;
        //playerTime.transform.position = new Vector3(0.093f, 0.4f, 0);
        playerTime.fontSize = (int)(Screen.height / 40.0f);
    }
    // On Start of the client used to attach the car script
    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("OnStartClient");
        StartCoroutine(FindCarScript());
    }
    // FindCarScript Coroutine to attach the localPlayers carScript
    IEnumerator FindCarScript()
    {
        // Wait until the local player car was spawned on this client
        while (_carScript == null)
        {
            _carScript = ClientScene.localPlayers[0].gameObject.GetComponent <CarBehaviourNetwork> ();
            yield return new WaitForSeconds(0.1f);
        }
    }
    // This routine is only called once for the entire game on the server
    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("OnStartServer");
        if (!NetworkServer.active) return;
        StartCoroutine(GameStart());
    }
    // GameStart Coroutine executed only on the server
    IEnumerator GameStart()
    {
        Debug.Log("GameStart");
        // check if client's are ready
        serverTime.text = "Waiting for all clients to be ready ...";
        bool clientsReady = false;
        // Wait until all clients are ready
        while (!clientsReady)
        {
            clientsReady = true;
            foreach (var conn in NetworkServer.connections)
                clientsReady &= conn.isReady;
            yield return new WaitForSeconds(1);
        }
        // Do the countdown loop
        for (_countdown = countMax; _countdown > 0; _countdown--)
            yield return new WaitForSeconds(1);
        RpcStartRace();
    }
    // Syncvar hook to display the time during countdown
    void OnCountdownChanged(int countdown)
    {
        Debug.Log("OnCountdownChanged");
        _countdown = countdown;
        if (_countdown == 0)
            _beep.pitch = 1.5f;
        _beep.Play();
        serverTime.text = _countdown.ToString("0");
    }
    [ClientRpc]
    void RpcStartRace()
    {
        Debug.Log("RpcStartRace");
        if (_carScript == null) return;
        _carScript.thrustEnabled = true;
        _isStarted = true;
        // Move and scale fonts for server & player time text objects
        //serverTime.anchor = TextAnchor.LowerCenter;
        serverTime.transform.position = new Vector3(200f, 0f, 0);
        serverTime.fontSize = (int)(Screen.height / 40.0f);
        //playerTime.anchor = TextAnchor.LowerCenter;
        //playerTime.transform.position = new Vector3(0.093f, 0.37f, 0);
        playerTime.fontSize = (int)(Screen.height / 40.0f);
    }

    // FixedUpdate is increment & display the server time
    void FixedUpdate()
    {
        if (_carScript == null && ClientScene.localPlayers[0].gameObject.GetComponent<NetworkIdentity>().isServer) return;
        _pastTime += Time.deltaTime;
        if (!_isStarted || _isFinished) return;
        serverTime.text = _pastTime.ToString("0.0");
    }

    void OnTextChange(string value)
    {
        if (ranking.gameObject.active)
        {
            rankings = value;
            ranking.text = value;
            Debug.Log(rankings);
            Debug.Log(ranking.text);
        }
    }

    // Trigger event handler when the car passes the gate
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Car" && _carScript != null && _carScript.collider == other)
        {
            _isFinished = true;
            localtime = _pastTime;
            serverTime.text = localtime.ToString("0.0");
            ClientScene.localPlayers[0].gameObject.active = false;
            _carScript.thrustEnabled = false;
            ranking.gameObject.active = true;
            _carScript.CmdActive(false);
            CmdChangeText(LobbyManager.instance.playersListBehaviour.LobbyPlayerList[ClientScene.localPlayers[0].playerControllerId].playerName + ": " + localtime.ToString("0.0") + "\n");
        }
    }

    [Command]
    void CmdChangeText(string value)
    {
        RpcChangeText(rankings + value);
    }

    [ClientRpc]
    void RpcChangeText(string value)
    {
        if (!isLocalPlayer)
            rankings = value;
    }
}
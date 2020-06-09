using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;

public class TimingNetworkBehaviour : NetworkBehaviour
{
    public TMP_Text serverTime;
    public TMP_Text playerTime;
    public int countMax = 2;
    [SyncVar(hook = "OnCountdownChanged")]
    private int _countdown;
    [SyncVar]
    private float _pastTime = 0;
    private CarBehaviourNetwork _carScript = null;
    private AudioSource _beep;
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
        if (_carScript == null) return;
        // ??? Increment _pastTime only on server
        // ??? Display the server time via syncvar on client
    }
    // Trigger event handler when the car passes the gate
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Car" && _carScript != null)
        {
            // ??? Display the server time as player time on the client only
            // ??? Display only the local players time
        }
    }
}
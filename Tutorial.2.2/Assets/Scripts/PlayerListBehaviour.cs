using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class PlayerListBehaviour : MonoBehaviour 
{        
    public  RectTransform       playersRect;

    private VerticalLayoutGroup _layout;
    private List<LobbyPlayer>   _lobbyPlayerList = new List<LobbyPlayer>();


    public List<LobbyPlayer> LobbyPlayerList { get { return _lobbyPlayerList; } }

    public List<Transform> spawnPoints;
    private Stack<Transform> _spawnPoints = new Stack<Transform>();
    private List<Transform> _usedSpawnPoints = new List<Transform>();

    public void OnEnable()
    {
        _layout = playersRect.GetComponent<VerticalLayoutGroup>();
    }

    void Start()
    {
        _spawnPoints.Clear();
        spawnPoints.Reverse();
        foreach (var sp in spawnPoints)
        {
            _spawnPoints.Push(sp);
        }
    }

    public Transform GetSpawnPoint()
    {
        Transform spawnPoint = _spawnPoints.Pop();
        _usedSpawnPoints.Add(spawnPoint);
        return spawnPoint;
    }

    void Update()
    {
        // workaround used in the example network lobby of Unity. 
        // The layout manager doesn't update correctly from time to time
        if(_layout)
            _layout.childAlignment = Time.frameCount % 2 == 0 ? 
                TextAnchor.UpperCenter : 
                TextAnchor.UpperLeft;
    }

    public void AddPlayer(LobbyPlayer lobbyPlayer)
    {
        RectTransform trfrm = lobbyPlayer.gameObject.GetComponent<RectTransform>();
        trfrm.SetParent(playersRect, false);
        _lobbyPlayerList.Add(lobbyPlayer);
    }

    public void RemovePlayer(LobbyPlayer lobbyPlayer)
    {
        lobbyPlayer.gameObject.GetComponent<RectTransform>().SetParent(null, false);
        Transform spawnPoint = _usedSpawnPoints[_lobbyPlayerList.IndexOf(lobbyPlayer)];
        _usedSpawnPoints.Remove(spawnPoint);
        _spawnPoints.Push(spawnPoint);
        _lobbyPlayerList.Remove(lobbyPlayer);
    }
}

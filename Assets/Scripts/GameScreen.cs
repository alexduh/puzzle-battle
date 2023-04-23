using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class GameScreen : NetworkBehaviour
{
    [SerializeField] private Transform _cam;
    [SerializeField] private TMP_Text startCountdown;
    [SerializeField] private Player playerPrefab;
    [SerializeField] private Transform playerLayout;
    [SerializeField] private GameObject readyButton;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject playerList;
    [SerializeField] private GameObject go;

    private bool multiplayer;

    private readonly Dictionary<ulong, bool> players = new();

    public static float started;
    private float readyTime;

    public void SetSinglePlayer()
    {
        multiplayer = false;
    }

    public void SetMultiPlayer()
    {
        multiplayer = true;
    }

    /*    private void OnClientConnectedCallback(ulong playerId)
        {
            if (!IsServer) return;

            // Add locally
            if (!players.ContainsKey(playerId))
            {
                players.Add(playerId, false);
            }

            PropagateToClients();

            UpdateInterface();
        }
    */

    private void StartGame()
    {
        Debug.Log("Started game!");
        started = 3.0f;
        readyTime = 0;
        this.GetComponent<AudioSource>().enabled = true;

        for (int i = 0; i < players.Count; i++)
        {
            playerList.transform.GetChild(i).gameObject.GetComponent<PuyoPuyo>().enabled = true;
        }

        mainMenu.gameObject.SetActive(false);
        readyButton.gameObject.SetActive(false);
        startCountdown.gameObject.SetActive(true);

        /*foreach(var script in scripts)
        {
            
        }*/

        _cam.GetComponent<Camera>().orthographicSize = 7.5f;
        _cam.transform.position = new Vector3(2.5f, 5.5f, -10);
        //_cam.GetComponent<Camera>().orthographicSize = (float)_height * 0.625f;
        //_cam.transform.position = new Vector3((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f, -10);
    }

    // Show "GAME OVER!" message for 5 seconds, then hide message and return to Main Menu
    public void EndGame()
    {
        this.GetComponent<AudioSource>().enabled = false;
        go.SetActive(true);
    }

    public void OnReadyClicked()
    {
        if (multiplayer) {
            SetReadyServerRpc(NetworkManager.Singleton.LocalClientId);
        }
        else
        {
            StartGame();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetReadyServerRpc(ulong playerID)
    {
        players[playerID] = !players[playerID];
        PropagateToClients();
        UpdateInterface(players);
    }

    [ClientRpc]
    private void UpdatePlayerClientRpc(ulong clientId, bool isReady)
    {
        if (IsServer) return;

        if (!players.ContainsKey(clientId)) players.Add(clientId, isReady);
        else players[clientId] = isReady;
        UpdateInterface(players);
    }
    private void UpdateInterface(Dictionary<ulong, bool> players)
    {
        var allActivePlayerIds = players.Keys;

        foreach (var player in players)
        {
            //var currentPanel = _playerPanels.FirstOrDefault(p => p.PlayerId == player.Key);
            if (player.Value)
            {
                GetComponent<Image>().color = Color.green;
            }
            else
            {
                GetComponent<Image>().color = Color.gray;
            }
        }
        
        //_readyButton.SetActive(!_ready);
    }

    private void PropagateToClients()
    {
        foreach (var player in players)
        {
            UpdatePlayerClientRpc(player.Key, player.Value);
        }
    }

    // This function is only used for Single Player mode
    public void CreatePlayer()
    {
        Player p = Instantiate(playerPrefab, playerLayout);
        players.Add(NetworkManager.Singleton.LocalClientId, false);
    }

    bool allReady()
    {
        foreach (var player in players)
        {
            if (!player.Value)
            {
                return false;
            }
        }

        return true;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (started > 0)
        {
            startCountdown.text = Mathf.Round(started).ToString();
            started -= Time.deltaTime;

            if (started <= 1)
            {
                startCountdown.text = "GO!";
            }
            if (started <= 0)
            {
                startCountdown.gameObject.SetActive(false);
            }

            return;

        }

        if (allReady())
        {
            Debug.Log("All players are ready!");
            readyTime += Time.deltaTime;
            if (readyTime >= 1.0f)
            {
                // if all players are ready for a full second, start the game!
                StartGame();

                // TODO: may need to unready players!
            }

            return;
        }

        readyTime = 0;

    }
}

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

    [SerializeField] private TMP_Text numPlayers;
    [SerializeField] private TMP_Text gameMode;

    //private NetworkVariable<ulong> playerID;
    public static bool multiplayer;
    private NetworkVariable<bool> gameStarted = new NetworkVariable<bool>(false);

    private readonly Dictionary<ulong, bool> players = new();

    public static float startTimer;
    private float readyTime;

    public void SetSinglePlayer()
    {
        multiplayer = false;
    }

    public void SetMultiPlayer()
    {
        multiplayer = true;
    }    

    private void StartGame()
    {
        startTimer = 3.0f;
        this.GetComponent<AudioSource>().enabled = true;

        for (int i = 0; i < players.Count; i++)
        {
            playerList.transform.GetChild(i).gameObject.GetComponent<PuyoPuyo>().enabled = true;
        }

        SetReadyServerRpc(NetworkManager.Singleton.LocalClientId);

        mainMenu.gameObject.SetActive(false);
        readyButton.gameObject.SetActive(false);
        startCountdown.gameObject.SetActive(true);

        _cam.GetComponent<Camera>().orthographicSize = 7.5f;
        //_cam.transform.position = new Vector3(2.5f, 5.5f, -10);
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

    public override void OnNetworkSpawn()
    {
        //playerID = new NetworkVariable<ulong>(OwnerClientId);
        gameStarted.OnValueChanged += (bool oldValue, bool newValue) =>
        {
            StartGame();
        };

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            players.Add(NetworkManager.Singleton.LocalClientId, false);
            Debug.Log($"My ID is: {NetworkManager.Singleton.LocalClientId}");
            UpdateInterface(players);
        }

        // Client uses this in case host destroys the lobby
        //NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;

    }

    private void OnClientConnectedCallback(ulong playerID)
    {
        if (!IsServer) 
            return;

        // Add locally
        if (!players.ContainsKey(playerID))
        {
            players.Add(playerID, false);
        }

        PropagateToClients();

        UpdateInterface(players);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetReadyServerRpc(ulong playerID)
    {
        players[playerID] = !players[playerID];
        PropagateToClients();
        UpdateInterface(players);
    }

    private void PropagateToClients()
    {
        foreach (var player in players)
        {
            UpdatePlayerClientRpc(player.Key, player.Value);
        }
    }

    [ClientRpc]
    private void UpdatePlayerClientRpc(ulong clientId, bool isReady)
    {
        if (IsServer) 
            return;

        if (!players.ContainsKey(clientId)) 
            players.Add(clientId, isReady);
        else 
            players[clientId] = isReady;

        UpdateInterface(players);
    }
    private void UpdateInterface(Dictionary<ulong, bool> players)
    {
        Player[] playerList = Object.FindObjectsOfType<Player>();
        
        foreach (Player player in playerList)
        {
            player.transform.GetChild(1).gameObject.SetActive(players[player.OwnerClientId]);
        }

    }

    // This function is only used for Single Player mode
    public void CreatePlayer()
    {
        Instantiate(playerPrefab, playerLayout);
        players.Add(NetworkManager.Singleton.LocalClientId, false);
    }

    bool allReady()
    {
        if (players.Count == 0 || (multiplayer && players.Count <= 1))
        {
            // can't start game unless at least one player is ready
            // can't start multiplayer game unless at least 2 players ready
            return false; 
        }

        foreach (var player in players)
        {
            if (!player.Value)
            {
                return false;
            }
        }

        return true;
    }

    private void OnEnable()
    {
        // disable # Players text, enable Select Game Mode text
        // TODO: update GameScreen based on server
        numPlayers.gameObject.SetActive(false);
        gameMode.gameObject.SetActive(true);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (startTimer > 0)
        {
            startCountdown.text = Mathf.Round(startTimer).ToString();
            startTimer -= Time.deltaTime;

            if (startTimer <= 1)
            {
                startCountdown.text = "GO!";
            }
            if (startTimer <= 0)
            {
                startCountdown.gameObject.SetActive(false);
            }

            return;

        }

        if (allReady())
        {
            readyTime += Time.deltaTime;
            if (readyTime >= 1.0f && !gameStarted.Value)
            {
                // if all players are ready for a full second, start the game!
                gameStarted.Value = true;
            }

            return;
        }

        readyTime = 0;

    }
}

using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;

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
    [SerializeField] private GameObject selectPlayers;
    [SerializeField] private GameObject relayScreen;

    [SerializeField] private TMP_Text numPlayers;
    [SerializeField] private TMP_Text gameMode;

    public static bool multiplayer;
    private bool gameRunning = false;

    private readonly Dictionary<ulong, bool> players = new();

    public static float startTimer;
    private float readyTime;
    private bool shutdownStarted;

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
        gameRunning = true;
        startTimer = 3.0f;
        this.GetComponent<AudioSource>().enabled = true;

        foreach (Transform child in playerList.transform)
        {
            child.gameObject.GetComponent<PuyoPuyo>().enabled = true;
            child.GetChild(0).gameObject.SetActive(false);
        }
            
        SetReadyServerRpc(NetworkManager.Singleton.LocalClientId);

        mainMenu.gameObject.SetActive(false);
        readyButton.gameObject.SetActive(false);
        startCountdown.gameObject.SetActive(true);

        _cam.GetComponent<Camera>().orthographicSize = 7.5f;
        if (multiplayer)
            _cam.transform.position = new Vector3(0, 0, -10);
        else
            _cam.transform.position = new Vector3(.5f, 0, -10);

        //_cam.GetComponent<Camera>().orthographicSize = (float)_height * 0.625f;
        //_cam.transform.position = new Vector3((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f, -10);
    }

    // Show "GAME OVER!" message for 5 seconds, then hide message and return to Main Menu
    public void EndGame(ulong playerId)
    {
        gameRunning = false;
        this.GetComponent<AudioSource>().enabled = false;
        go.SetActive(true);
        if (multiplayer && NetworkManager.Singleton.LocalClientId != playerId)
            go.GetComponent<GameOver>().Win();
        else
            go.GetComponent<GameOver>().Lose();

        foreach (Transform child in playerList.transform)
            child.gameObject.GetComponent<PuyoPuyo>().enabled = false;
    }

    public void OnReadyClicked()
    {
        if (multiplayer)
            SetReadyServerRpc(NetworkManager.Singleton.LocalClientId);
        else
            StartGame();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemovePlayerServerRpc(ulong clientId)
    {
        NetworkManager.DisconnectClient(clientId);
    }

    public void OnResetClicked()
    {
        if (gameRunning)
        {
            if (multiplayer)
                EndGameServerRpc(NetworkManager.Singleton.LocalClientId);
            else
                EndGame(0);
        }
        else
        {
            if (go.GetComponent<GameOver>().gameEnded <= 0)
            {
                if (multiplayer)
                {
                    shutdownStarted = true;
                    NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
                    NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
                    NetworkManager.Singleton.Shutdown();
                        
                    //RemovePlayerServerRpc(NetworkManager.Singleton.LocalClientId);
                    //NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
                }
                else
                    this.gameObject.SetActive(false); // disable GameScreen
            }
        }

    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            players.Add(NetworkManager.Singleton.LocalClientId, false);
            Debug.Log($"My ID is: {NetworkManager.Singleton.LocalClientId}");
            UpdateInterface(players);
        }

        // Client uses this in case host destroys the lobby
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;

    }

    private void OnClientConnectedCallback(ulong playerID)
    {
        if (!IsServer) 
            return;

        // Add locally
        if (!players.ContainsKey(playerID))
            players.Add(playerID, false);

        PropagateToClients();

        UpdateInterface(players);
    }

    private void OnClientDisconnectCallback(ulong playerId)
    {
        if (IsServer)
            if (players.ContainsKey(playerId))
                players.Remove(playerId);
        else
        {
            // This happens when the host disconnects the lobby
            shutdownStarted = true;
            NetworkManager.Singleton.Shutdown();
        }
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
            UpdatePlayerClientRpc(player.Key, player.Value);
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
            if (players.ContainsKey(player.OwnerClientId))
                player.transform.GetChild(1).gameObject.SetActive(players[player.OwnerClientId]);

    }

    // This function is only used for Single Player mode
    public void CreatePlayer()
    {
        Instantiate(playerPrefab, playerLayout);
        players.Add(NetworkManager.Singleton.LocalClientId, false);
    }

    private void DestroyPlayers()
    {
        foreach (Transform child in playerLayout)
            Destroy(child.gameObject);
        
        players.Clear();
    }

    bool AllReady()
    {
        // can't start game unless at least one player is ready
        // can't start multiplayer game unless at least 2 players ready
        if (players.Count == 0 || (multiplayer && players.Count <= 1))
            return false;

        foreach (var player in players)
            if (!player.Value)
                return false;

        return true;
    }

    public void SendGarbage(ulong targetPlayer, int amount)
    {
        foreach (Transform child in playerList.transform)
        {
            Player player = child.gameObject.GetComponent<Player>();
            if (player.OwnerClientId == targetPlayer)
                player.ReceiveGarbage(amount);
        }
            
    }

    public void ChainEnded(ulong targetPlayer)
    {
        foreach (Transform child in playerList.transform)
        {
            Player player = child.gameObject.GetComponent<Player>();
            if (player.OwnerClientId == targetPlayer)
                player.FinishReceiveGarbage();
                
        }
    }

    private void OnDisable()
    {
        gameMode.gameObject.SetActive(false);
        DestroyPlayers();

        if (multiplayer)
        {
            // Show relay screen and buttons, hide room code text
            relayScreen.transform.Find("Room Code").gameObject.GetComponent<TMP_Text>().text = "";
            relayScreen.transform.Find("Relay Buttons").gameObject.SetActive(true);
        }
        else
        {
            numPlayers.gameObject.SetActive(true);
            selectPlayers.SetActive(true);
        }
        
    }

    private void OnEnable()
    {
        // disable # Players text, enable Select Game Mode text
        numPlayers.gameObject.SetActive(false);
        gameMode.gameObject.SetActive(true);
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartGameServerRpc()
    {
        StartGameClientRpc();
    }

    [ClientRpc]
    private void StartGameClientRpc()
    {
        if (!gameRunning)
            StartGame();
    }

    [ServerRpc(RequireOwnership = false)]
    public void EndGameServerRpc(ulong playerId) // ID of the player who was eliminated
    {
        EndGameClientRpc(playerId);
    }

    [ClientRpc]
    private void EndGameClientRpc(ulong playerId)
    {
        if (gameRunning)
            EndGame(playerId);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (shutdownStarted && !NetworkManager.Singleton.ShutdownInProgress)
        {
            this.gameObject.SetActive(false);
            shutdownStarted = false;
        }

        if (startTimer > 0)
        {
            startCountdown.text = Mathf.Round(startTimer).ToString();
            startTimer -= Time.deltaTime;

            if (startTimer <= 1)
                startCountdown.text = "GO!";
            if (startTimer <= 0)
                startCountdown.gameObject.SetActive(false);

            return;
        }

        if (AllReady())
        {
            readyTime += Time.deltaTime;
            if (readyTime >= 1.0f)
                StartGameServerRpc(); // if all players are ready for a full second, start the game!

            return;
        }

        readyTime = 0;
    }
}

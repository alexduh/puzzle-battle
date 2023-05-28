using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UIElements;

public class Player : NetworkBehaviour
{
    [SerializeField] protected int _width, _height;
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] protected GameScreen gameScreen;
    [SerializeField] protected Sprite starNuisance;
    [SerializeField] protected Sprite rockNuisance;
    [SerializeField] protected Sprite largeNuisance;
    [SerializeField] protected Sprite smallNuisance;

    protected Score scoreObject;
    protected Block[,] grid;
    protected Block[,] immediateNext;
    protected Queue<int> next = new Queue<int>();
    private GameObject garbageQueue;

    protected ulong targetPlayerId;
    protected int receivingGarbage;
    protected int incomingGarbage;
    protected int clearedTotal; // Blocks destroyed
    protected int level; // increase based on Blocks destroyed
    protected float dropTime; // decrease based on level
    protected Vector3 playerPos = Vector3.zero;
    protected Vector3 cornerPos = Vector3.zero;

    public void SetWidth(int w)
    {
        _width = w;
    }

    public void SetHeight(int h)
    {
        _height = h;
    }

    void GenerateGrid()
    {
        playerPos = Camera.main.ScreenToWorldPoint(transform.position);
        cornerPos = new Vector3(Mathf.Round(playerPos.x - 2.5f), 6f);
        garbageQueue = transform.GetChild(4).gameObject;
        garbageQueue.transform.position = cornerPos; // garbageQueue position

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var spawnedTile = Instantiate(_tilePrefab, cornerPos + new Vector3(x, y - _height), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";
            }
        }
    }

    public void DeleteGrid()
    {
        for (int i = 0; i < _width; i++)
            for (int j = 0; j < (_height * 2); j++)
                if (grid[i, j])
                    Destroy(grid[i, j].gameObject);

    }

    // Show "GAME OVER!" message for 5 seconds, then hide message and return to Main Menu
    protected void Eliminated()
    {
        if (!GameScreen.multiplayer)
            gameScreen.EndGame(0);
        else if (IsOwner)
            gameScreen.EndGameServerRpc(OwnerClientId);
    }

    protected void ProcessGarbage(int amount)
    {
        if (incomingGarbage > amount)
        {
            incomingGarbage -= amount;
            amount = 0;
        }
        else
        {
            amount -= incomingGarbage;
            incomingGarbage = 0;
        }
        
        if (receivingGarbage > amount)
        {
            receivingGarbage -= amount;
            amount = 0;
        }
        else
        {
            amount -= receivingGarbage;
            receivingGarbage = 0;
        }

        UpdateGarbageQueue();
        //Debug.Log("Player " + OwnerClientId + " Sending Totals - receivingGarbage: " + receivingGarbage + " incomingGarbage: " + incomingGarbage + " amount: " + amount);

        if (amount > 0)
            gameScreen.SendGarbage(targetPlayerId, amount);
    }

    public void ReceiveGarbage(int amount)
    {
        receivingGarbage += amount;
        UpdateGarbageQueue();
    }

    protected void UpdateGarbageQueue()
    {
        int totalGarbage = incomingGarbage + receivingGarbage;
        foreach (Transform child in garbageQueue.transform)
        {
            if (totalGarbage >= 180)
            {
                // add star nuisance
                totalGarbage -= 180;
                child.GetComponent<SpriteRenderer>().sprite = starNuisance;
                continue;
            }
            if (totalGarbage >= 30)
            {
                // add rock nuisance
                totalGarbage -= 30;
                child.GetComponent<SpriteRenderer>().sprite = rockNuisance;
                continue;
            }
            if (totalGarbage >= 6)
            {
                // add large nuisance
                totalGarbage -= 6;
                child.GetComponent<SpriteRenderer>().sprite = largeNuisance;
                continue;
            }
            if (totalGarbage >= 1)
            {
                // add small nuisance
                totalGarbage--;
                child.GetComponent<SpriteRenderer>().sprite = smallNuisance;
                continue;
            }
            child.GetComponent<SpriteRenderer>().sprite = null;
        }
        
    }

    public void FinishReceiveGarbage()
    {
        incomingGarbage += receivingGarbage;
        receivingGarbage = 0;
    }

    void Awake()
    {
        scoreObject = transform.GetChild(2).GetComponent<Score>();
    }

    protected void OnEnable()
    {
        scoreObject.gameObject.SetActive(true); // Show Score
        scoreObject.UpdateScore(0); // Reset Score
        receivingGarbage = 0;
        incomingGarbage = 0;

        clearedTotal = 0;
        level = 1;
        dropTime = 1.0f;
    }

    public override void OnNetworkSpawn()
    {
        GameObject playerTemp = GameObject.Find("Canvas/GameScreen/Players");
        transform.SetParent(playerTemp.transform);
        base.OnNetworkSpawn();
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        GenerateGrid();

        gameScreen = Object.FindObjectOfType<GameScreen>();
        transform.GetChild(3).position = Camera.main.ScreenToWorldPoint(transform.GetChild(3).position) + new Vector3(0, 0, 10);
    }

    // Update is called once per frame
    protected void Update()
    {

    }
}

using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    [SerializeField] protected int _width, _height;
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] protected GameScreen gs;

    protected Score scoreObject;
    protected Block[,] grid;
    protected Block[,] immediateNext;
    protected Queue<int> next = new Queue<int>();

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

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var spawnedTile = Instantiate(_tilePrefab, cornerPos + new Vector3(x, y - _height), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";
            }
        }
    }

    protected void DeleteGrid()
    {
        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < (_height * 2); j++)
            {
                if (grid[i, j])
                {
                    Destroy(grid[i, j].gameObject);
                }
            }
        }
    }

    // Show "GAME OVER!" message for 5 seconds, then hide message and return to Main Menu
    protected void Eliminated()
    {
        if (!GameScreen.multiplayer)
            gs.EndGame(0);
        else if (IsOwner)
            gs.EndGameServerRpc(OwnerClientId);
    }

    protected void ProcessGarbage(int amount)
    {
        if (incomingGarbage > amount)
            incomingGarbage -= amount;
        else
        {
            amount -= incomingGarbage;
            incomingGarbage = 0;
        }
        
        if (receivingGarbage > amount)
            receivingGarbage -= amount;
        else
        {
            amount -= receivingGarbage;
            receivingGarbage = 0;
        }

        if (amount > 0)
            gs.SendGarbage(targetPlayerId, amount);
    }

    public void ReceiveGarbage(int amount)
    {
        receivingGarbage += amount;
    }

    public void FinishReceiveGarbage()
    {
        incomingGarbage = receivingGarbage;
        receivingGarbage = 0;
    }

    void Awake()
    {
        scoreObject = transform.GetChild(2).GetComponent<Score>();
    }

    protected void OnDisable()
    {
        scoreObject.gameObject.SetActive(false); // Hide Score
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

        gs = Object.FindObjectOfType<GameScreen>();
    }

    // Update is called once per frame
    protected void Update()
    {

    }
}

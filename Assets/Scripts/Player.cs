using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    [SerializeField] protected int _width, _height;
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private GameScreen gs;

    protected Block[,] grid;
    protected Block[,] immediateNext;
    protected Queue<int> next = new Queue<int>();

    protected int cleared; // Blocks destroyed
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

    void deleteGrid()
    {
        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < (_height + 2); j++)
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
        deleteGrid();
        gs.EndGame();
        this.enabled = false;
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

    private void OnEnable()
    {
        this.transform.GetChild(0).gameObject.SetActive(false);
        cleared = 0;
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
        if (GameScreen.multiplayer && !IsOwner)
        {
            this.enabled = false;
        }

        gs = Object.FindObjectOfType<GameScreen>();
    }

    // Update is called once per frame
    protected void Update()
    {

    }
}

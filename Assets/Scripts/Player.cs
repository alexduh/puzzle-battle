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

    protected int cleared; // Blocks destroyed
    protected int level; // increase based on Blocks destroyed
    protected float dropTime; // decrease based on level

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
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var spawnedTile = Instantiate(_tilePrefab, new Vector3(x, y), Quaternion.identity);
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

    // Start is called before the first frame update
    protected virtual void Start()
    {
        GenerateGrid();
        gs = Object.FindObjectOfType<GameScreen>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

    }
}
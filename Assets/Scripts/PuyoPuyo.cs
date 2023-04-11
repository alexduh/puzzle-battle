using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PuyoPuyo : GridManager
{
    [SerializeField] private Block _blockPrefab;
    [SerializeField] private Block _redPuyo;
    [SerializeField] private Block _yellowPuyo;
    [SerializeField] private Block _greenPuyo;
    [SerializeField] private Block _bluePuyo;

    [SerializeField] private GameObject gameOverText;
    [SerializeField] private TMP_Text startCountdown;

    private AudioSource[] sounds;
    private AudioSource connect, move, rotate;

    public GameOver go;
    private float started, update, cleared, primaryBlink;

    Block[,] grid;
    Block[] falling = new Block[2];
    int x1, y1, x2, y2;

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
    void GameOver()
    {
        gameOverText.SetActive(true);
        go.update = 5.0f;
        deleteGrid();
        this.GetComponent<AudioSource>().enabled = false;
        this.enabled = false;
    }

    // Create a Puyo Block, with each side a random color
    void CreatePuyoBlock()
    {
        int random = (int)Mathf.Round(Random.Range(0, 15));
        int quotient = random / 4;
        int remainder = random % 4;

        switch(quotient)
        {
            case 0:
                falling[0] = Instantiate(_redPuyo, new Vector3(2, 12), Quaternion.identity);
                break;
            case 1:
                falling[0] = Instantiate(_yellowPuyo, new Vector3(2, 12), Quaternion.identity);
                break;
            case 2:
                falling[0] = Instantiate(_greenPuyo, new Vector3(2, 12), Quaternion.identity);
                break;
            case 3:
                falling[0] = Instantiate(_bluePuyo, new Vector3(2, 12), Quaternion.identity);
                break;
        }

        switch (remainder)
        {
            case 0:
                falling[1] = Instantiate(_redPuyo, new Vector3(2, 13), Quaternion.identity);
                break;
            case 1:
                falling[1] = Instantiate(_yellowPuyo, new Vector3(2, 13), Quaternion.identity);
                break;
            case 2:
                falling[1] = Instantiate(_greenPuyo, new Vector3(2, 13), Quaternion.identity);
                break;
            case 3:
                falling[1] = Instantiate(_bluePuyo, new Vector3(2, 13), Quaternion.identity);
                break;
        }

    }

    bool leftOpen(Block b)
    {
        int x = (int)b.transform.position.x;
        int y = (int)b.transform.position.y;
        return (x > 0 && !grid[x - 1, y]);
    }

    bool rightOpen(Block b)
    {
        int x = (int)b.transform.position.x;
        int y = (int)b.transform.position.y;
        return (x < 5 && !grid[x + 1, y]);
    }

    bool bottomOpen(Block b)
    {
        int x = (int)b.transform.position.x;
        int y = (int)b.transform.position.y;
        return (y > 0 && !grid[x, y - 1]);
    }

    void moveLeft()
    {
        if (leftOpen(falling[0]) && leftOpen(falling[1]))
        {
            falling[0].transform.position = new Vector3(x1 - 1, y1);
            falling[1].transform.position = new Vector3(x2 - 1, y2);
            move.Play();
        }
    }

    void moveRight()
    {
        if (rightOpen(falling[0]) && rightOpen(falling[1]))
        {
            falling[0].transform.position = new Vector3(x1 + 1, y1);
            falling[1].transform.position = new Vector3(x2 + 1, y2);
            move.Play();
        }
    }

    void moveDown()
    {
        falling[0].transform.position = new Vector3(x1, y1 - 1);
        falling[1].transform.position = new Vector3(x2, y2 - 1);
    }

    int bufferedRotate = 0;

    void rotateCCW()
    {
        if (y2 > y1)
        {
            if (leftOpen(falling[0]))
            {
                falling[1].transform.position = new Vector3(x1 - 1, y1);
            }
            else if (rightOpen(falling[0]))
            {
                falling[1].transform.position = new Vector3(x1, y1);
                falling[0].transform.position = new Vector3(x1 + 1, y1);
            }
            else
            {
                bufferedRotate--;
                if (bufferedRotate > -2)
                {
                    return;
                }

                bufferedRotate = 0;
                falling[1].transform.position = new Vector3(x1, y1);
                falling[0].transform.position = new Vector3(x1, y1 + 1);
            }

        }
        else if (y1 > y2)
        {
            if (rightOpen(falling[0]))
            {
                falling[1].transform.position = new Vector3(x1 + 1, y1);
            }
            else if (leftOpen(falling[0]))
            {
                falling[1].transform.position = new Vector3(x1, y1);
                falling[0].transform.position = new Vector3(x1 - 1, y1);
            }
            else
            {
                bufferedRotate--;
                if (bufferedRotate > -2)
                {
                    return;
                }

                bufferedRotate = 0;
                falling[1].transform.position = new Vector3(x1, y1);
                falling[0].transform.position = new Vector3(x1, y1 - 1);
            }
        }
        else if (x2 > x1)
        {
            falling[1].transform.position = new Vector3(x1, y1 + 1);
        }
        else
        {
            if (bottomOpen(falling[0]))
            {
                falling[1].transform.position = new Vector3(x1, y1 - 1);
            }
            else
            {
                falling[1].transform.position = new Vector3(x1, y1);
                falling[0].transform.position = new Vector3(x1, y1 + 1);
            }
        }

        rotate.Play();
    }

    void rotateCW()
    {
        if (y2 > y1)
        {
            if (rightOpen(falling[0]))
            {
                falling[1].transform.position = new Vector3(x1 + 1, y1);
            }
            else if (leftOpen(falling[0]))
            {
                falling[1].transform.position = new Vector3(x1, y1);
                falling[0].transform.position = new Vector3(x1 - 1, y1);
            }
            else
            {
                bufferedRotate++;
                if (bufferedRotate < 2)
                {
                    return;
                }

                bufferedRotate = 0;
                falling[1].transform.position = new Vector3(x1, y1);
                falling[0].transform.position = new Vector3(x1, y1 + 1);
            }
        }
        else if (y1 > y2)
        {
            if (leftOpen(falling[0]))
            {
                falling[1].transform.position = new Vector3(x1 - 1, y1);
            }
            else if (rightOpen(falling[0]))
            {
                falling[1].transform.position = new Vector3(x1, y1);
                falling[0].transform.position = new Vector3(x1 + 1, y1);
            }
            else
            {
                bufferedRotate++;
                if (bufferedRotate < 2)
                {
                    return;
                }

                bufferedRotate = 0;
                falling[1].transform.position = new Vector3(x1, y1);
                falling[0].transform.position = new Vector3(x1, y1 - 1);
            }
        }
        else if (x2 > x1)
        {
            if (bottomOpen(falling[0]))
            {
                falling[1].transform.position = new Vector3(x1, y1 - 1);
            }
            else
            {
                falling[1].transform.position = new Vector3(x1, y1);
                falling[0].transform.position = new Vector3(x1, y1 + 1);
            }

        }
        else
        {
            falling[1].transform.position = new Vector3(x1, y1 + 1);
        }

        rotate.Play();
    }

    /** This function drops all Blocks that can be dropped, and
     *  returns all Blocks that moved from their original position.
     */
    List<Block> dropAll()
    {
        List<Block> moved = new List<Block>();
        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < _height; j++)
            {
                if (grid[i, j])
                {
                    for (int h = 0; h < j; h++)
                    {
                        if (!grid[i, h])
                        {
                            grid[i, h] = grid[i, j];
                            grid[i, j].transform.position = new Vector3(i, h);
                            grid[i, j] = null;
                            moved.Add(grid[i, h]);
                            break;
                        }
                    }
                }
            }
        }

        return moved;
    }

    // DFS function to find all connected Blocks of matching color
    List<Block> findChain(Stack<Block> stack)
    {
        int bx, by;

        List<Block> list = new List<Block>();
        Block current;
        Block.Color match = Block.Color.None;

        while (stack.Count > 0) {
            current = stack.Pop();
            if (list.Contains(current))
            {
                continue;
            }

            list.Add(current);
            bx = (int)current.transform.position.x;
            by = (int)current.transform.position.y;
            if (match == Block.Color.None) {
                match = current.color;
            }

            if (bx > 0 && grid[bx - 1, by] && grid[bx - 1, by].color == match)
            {
                stack.Push(grid[bx - 1, by]);
            }
            if (by > 0 && grid[bx, by - 1] && grid[bx, by - 1].color == match)
            {
                stack.Push(grid[bx, by - 1]);
            }
            if (bx < 5 && grid[bx + 1, by] && grid[bx + 1, by].color == match)
            {
                stack.Push(grid[bx + 1, by]);
            }
            if (by < 12 && grid[bx, by + 1] && grid[bx, by + 1].color == match)
            {
                stack.Push(grid[bx, by + 1]);
            }

        }

        return list;
    }

    /** This function checks for any connections made by Blocks in 'moved',
     *  then marks all connected blocks to be deleted during the Update cycle.
     */
    void checkConnect(List<Block> moved)
    {
        List<Block> list;

        foreach (Block startBlock in moved)
        {           
            if (startBlock.color == Block.Color.None)
            {
                continue;
            }

            Stack<Block> stack = new Stack<Block>();
            stack.Push(startBlock);
            list = findChain(stack);

            if (list.Count >= 4)
            {
                connect.Play();
                cleared = 1.0f;
                foreach (Block block in list)
                {
                    grid[(int)block.transform.position.x, (int)block.transform.position.y] = null;

                    block.destroy = true;
                }
            }
                    
        }
    }

    void OnEnable()
    {
        started = 3.0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        sounds = GetComponents<AudioSource>();
        connect = sounds[1];
        move = sounds[2];
        rotate = sounds[3];
        grid = new Block[_width, _height+2];
    }

    // Update is called once per frame
    void Update()
    {
        if (started > 0)
        {
            startCountdown.gameObject.SetActive(true);
            startCountdown.text = Mathf.Round(started).ToString();
            started -= Time.deltaTime;

            if (started <= 1)
            {
                startCountdown.text = "GO!";
            }
            if (started <= 0)
            {
                startCountdown.gameObject.SetActive(false);
                this.GetComponent<AudioSource>().enabled = true;
            }
            else
            {
                return;
            }
        }

        List<Block> moved = new List<Block>();

        if (cleared > 0)
        {
            cleared -= Time.deltaTime;

            if (cleared <= 0)
            {
                moved = dropAll();
                checkConnect(moved);
            }
            
        }
        else
        {
            if (!falling[0] || !falling[1])
            {
                CreatePuyoBlock();
            }

            x1 = (int)falling[0].transform.position.x;
            y1 = (int)falling[0].transform.position.y;
            x2 = (int)falling[1].transform.position.x;
            y2 = (int)falling[1].transform.position.y;

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                moveLeft();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                moveRight();
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                rotateCCW();
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                rotateCW();
            }

            if (Input.GetKey(KeyCode.DownArrow))
            {
                update += 10 * Time.deltaTime;
            }
            else
            {
                update += Time.deltaTime;
            }

            primaryBlink += Time.deltaTime;

            if (primaryBlink > 0.1f)
            {
                falling[0].GetComponent<Renderer>().enabled ^= true;
                primaryBlink = 0;
            }

            if (update > 1.0f)
            {
                falling[0].GetComponent<Renderer>().enabled = true;

                update -= 1.0f;
                if (y1 == 0 || y2 == 0 || grid[x1, y1 - 1] || grid[x2, y2 - 1])
                {
                    grid[x1, y1] = falling[0];
                    grid[x2, y2] = falling[1];
                    dropAll();

                    moved.Add(falling[0]);
                    moved.Add(falling[1]);

                    checkConnect(moved);
                    if (cleared > 0)
                    {
                        return;
                    }

                    if (grid[2, 11])
                    {
                        GameOver();
                        return;
                    }

                    CreatePuyoBlock();
                }
                else
                {
                    moveDown();
                }
            }
        }
    }
}

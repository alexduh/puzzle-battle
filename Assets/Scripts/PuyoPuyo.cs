using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuyoPuyo : Player
{
    [SerializeField] private Block _blockPrefab;
    [SerializeField] private Block _redPuyo;
    [SerializeField] private Block _yellowPuyo;
    [SerializeField] private Block _greenPuyo;
    [SerializeField] private Block _bluePuyo;

    private AudioSource[] sounds;
    private AudioSource connect, move, rotate;

    private float update, clearedTime, primaryBlink;
    private float keyPressed;

    Block[] falling = new Block[2];
    int x1, y1, x2, y2;

    public void AddToQueue(int selected)
    {
        for (int x = 0; x < immediateNext.GetLength(0); x++)
        {
            if (immediateNext[x, 0] == null)
            {
                immediateNext[x, 0] = CreatePuyoSegment(selected / 4, new Vector3(6, 11 - 2 * x));
                immediateNext[x, 1] = CreatePuyoSegment(selected % 4, new Vector3(6, 12 - 2 * x));
                return;
            }
        }

        next.Enqueue(selected);
    }
    
    Block CreatePuyoSegment(int color, Vector3 location)
    {
        Block ret = null;
        switch (color)
        {
            case 0:
                ret = Instantiate(_redPuyo, location, Quaternion.identity);
                break;
            case 1:
                ret = Instantiate(_yellowPuyo, location, Quaternion.identity);
                break;
            case 2:
                ret = Instantiate(_greenPuyo, location, Quaternion.identity);
                break;
            case 3:
                ret = Instantiate(_bluePuyo, location, Quaternion.identity);
                break;
        }

        return ret;
    }

    // Create a Puyo Block, with each side a random color
    void CreateFallingPuyoBlock()
    {
        falling[0] = immediateNext[0, 0];
        falling[1] = immediateNext[0, 1];
        falling[0].transform.position = new Vector3(2, 12);
        falling[1].transform.position = new Vector3(2, 13);
    }

    void UpdateNext()
    {
        immediateNext[0, 0] = immediateNext[1, 0];
        immediateNext[0, 1] = immediateNext[1, 1];
        immediateNext[0, 0].transform.position = new Vector3(6, 11);
        immediateNext[0, 1].transform.position = new Vector3(6, 12);

        int selected = next.Dequeue();
        immediateNext[1, 0] = CreatePuyoSegment(selected/4, new Vector3(6, 9));
        immediateNext[1, 1] = CreatePuyoSegment(selected%4, new Vector3(6, 10));
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
                clearedTime = 1.0f;
                foreach (Block block in list)
                {
                    grid[(int)block.transform.position.x, (int)block.transform.position.y] = null;

                    block.destroy = true;
                    cleared++;
                    if (cleared >= 20)
                    {
                        level++;
                        dropTime *= .9f; // decrease dropTime by 10%
                        cleared -= 20;
                    }
                }
            }
                    
        }
    }

    // Start is called before the first frame update
    new void Start()
    {
        SetWidth(6);
        SetHeight(12);
        immediateNext = new Block[2, 2];
        base.Start();
        sounds = GetComponents<AudioSource>();
        connect = sounds[0];
        move = sounds[1];
        rotate = sounds[2];
        grid = new Block[_width, _height+2];
    }

    // Update is called once per frame
    void Update()
    {
        if (GameScreen.started > 0)
        {
            return;
        }

        List<Block> moved = new List<Block>();

        if (clearedTime > 0)
        {
            clearedTime -= Time.deltaTime;

            if (clearedTime <= 0)
            {
                moved = dropAll();
                checkConnect(moved);
            }
            
        }
        else
        {
            if (!falling[0] || !falling[1])
            {
                while (next.Count < 1)
                {
                    PieceGenerator.getPuyo(transform.parent.gameObject);
                }

                CreateFallingPuyoBlock();
                UpdateNext();
            }

            x1 = (int)falling[0].transform.position.x;
            y1 = (int)falling[0].transform.position.y;
            x2 = (int)falling[1].transform.position.x;
            y2 = (int)falling[1].transform.position.y;

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                if (keyPressed == 0 || keyPressed > .5f) // move immediately on first press, then zoom after 0.5s
                {
                    moveLeft();
                }

                keyPressed += Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                if (keyPressed == 0 || keyPressed > .5f) // move immediately on first press, then zoom after 0.5s
                {
                    moveRight();
                }

                keyPressed += Time.deltaTime;
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                rotateCCW();
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                rotateCW();
            }

            if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow))
            {
                keyPressed = 0;
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

            if (update > dropTime)
            {
                falling[0].GetComponent<Renderer>().enabled = true;

                update -= dropTime;
                if (y1 == 0 || y2 == 0 || grid[x1, y1 - 1] || grid[x2, y2 - 1])
                {
                    grid[x1, y1] = falling[0];
                    grid[x2, y2] = falling[1];
                    dropAll();

                    moved.Add(falling[0]);
                    moved.Add(falling[1]);

                    checkConnect(moved);
                    if (clearedTime > 0)
                    {
                        return;
                    }

                    if (grid[2, 11])
                    {
                        Eliminated();
                        return;
                    }

                    falling[0] = falling[1] = null;
                }
                else
                {
                    moveDown();
                }
            }
        }
    }
}

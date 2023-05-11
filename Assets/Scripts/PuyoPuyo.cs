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
    private PieceGenerator generator;
    private int touchLimit = 8;
    private int touchCount;

    List<Block> moved;
    Block[] falling = new Block[2];
    int x1, y1, x2, y2, gx1, gx2, gy1, gy2;

    public void AddToQueue(int selected)
    {
        for (int x = 0; x < immediateNext.GetLength(0); x++)
        {
            if (immediateNext[x, 0] == null)
            {
                immediateNext[x, 0] = CreatePuyoSegment(selected / 4, cornerPos + new Vector3(_width, -2 * x - 2));
                immediateNext[x, 1] = CreatePuyoSegment(selected % 4, cornerPos + new Vector3(_width, -2 * x - 1));
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
        falling[0].transform.position = cornerPos + new Vector3(2, 0);
        falling[1].transform.position = cornerPos + new Vector3(2, 1);
        touchCount = 0;
    }

    void UpdateNext()
    {
        immediateNext[0, 0] = immediateNext[1, 0];
        immediateNext[0, 1] = immediateNext[1, 1];
        immediateNext[0, 0].transform.position = cornerPos + new Vector3(_width, -2);
        immediateNext[0, 1].transform.position = cornerPos + new Vector3(_width, -1);

        int selected = next.Dequeue();
        immediateNext[1, 0] = CreatePuyoSegment(selected/4, cornerPos + new Vector3(_width, -4));
        immediateNext[1, 1] = CreatePuyoSegment(selected%4, cornerPos + new Vector3(_width, -3));
    }

    bool LeftOpen(Block b)
    {
        int x = (int)(b.transform.position.x - cornerPos.x);
        int y = (int)(b.transform.position.y - cornerPos.y + _height);
        return (x > 0 && !grid[x - 1, y]);
    }

    bool RightOpen(Block b)
    {
        int x = (int)(b.transform.position.x - cornerPos.x);
        int y = (int)(b.transform.position.y - cornerPos.y + _height);
        return (x < 5 && !grid[x + 1, y]);
    }

    bool BottomOpen(Block b)
    {
        int x = (int)(b.transform.position.x - cornerPos.x);
        int y = (int)(b.transform.position.y - cornerPos.y + _height);
        return (y > 0 && !grid[x, y - 1]);
    }

    void MoveLeft()
    {
        if (LeftOpen(falling[0]) && LeftOpen(falling[1]))
        {
            falling[0].transform.position = new Vector3(x1 - 1, y1);
            falling[1].transform.position = new Vector3(x2 - 1, y2);
            move.Play();
        }
    }

    void MoveRight()
    {
        if (RightOpen(falling[0]) && RightOpen(falling[1]))
        {
            falling[0].transform.position = new Vector3(x1 + 1, y1);
            falling[1].transform.position = new Vector3(x2 + 1, y2);
            move.Play();
        }
    }

    void MoveDown()
    {
        falling[0].transform.position = new Vector3(x1, y1 - 1);
        falling[1].transform.position = new Vector3(x2, y2 - 1);
    }

    int bufferedRotate = 0;

    void RotateCCW()
    {
        if (y2 > y1)
        {
            if (LeftOpen(falling[0]))
            {
                falling[1].transform.position = new Vector3(x1 - 1, y1);
            }
            else if (RightOpen(falling[0]))
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
            if (RightOpen(falling[0]))
            {
                falling[1].transform.position = new Vector3(x1 + 1, y1);
            }
            else if (LeftOpen(falling[0]))
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
            if (BottomOpen(falling[0]))
            {
                falling[1].transform.position = new Vector3(x1, y1 - 1);
            }
            else
            {
                falling[1].transform.position = new Vector3(x1, y1);
                falling[0].transform.position = new Vector3(x1, y1 + 1);
                touchCount++;
            }
        }

        rotate.Play();
    }

    void RotateCW()
    {
        if (y2 > y1)
        {
            if (RightOpen(falling[0]))
            {
                falling[1].transform.position = new Vector3(x1 + 1, y1);
            }
            else if (LeftOpen(falling[0]))
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
            if (LeftOpen(falling[0]))
            {
                falling[1].transform.position = new Vector3(x1 - 1, y1);
            }
            else if (RightOpen(falling[0]))
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
            if (BottomOpen(falling[0]))
            {
                falling[1].transform.position = new Vector3(x1, y1 - 1);
            }
            else
            {
                falling[1].transform.position = new Vector3(x1, y1);
                falling[0].transform.position = new Vector3(x1, y1 + 1);
                touchCount++;
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
    List<Block> DropAll()
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
                            grid[i, j].transform.position = cornerPos + new Vector3(i, h - _height);
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
    List<Block> FindChain(Stack<Block> stack)
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
            bx = (int)(current.transform.position.x - cornerPos.x);
            by = (int)(current.transform.position.y - cornerPos.y + _height);

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
    void CheckConnect(List<Block> moved)
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
            list = FindChain(stack);

            if (list.Count >= 4)
            {
                connect.Play();
                clearedTime = 1.0f;
                foreach (Block block in list)
                {
                    grid[(int)(block.transform.position.x - cornerPos.x), (int)(block.transform.position.y - cornerPos.y + _height)] = null;

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

    void Drop()
    {
        falling[0].GetComponent<Renderer>().enabled = true;
        
        grid[gx1, gy1] = falling[0];
        grid[gx2, gy2] = falling[1];
        DropAll();

        moved.Add(falling[0]);
        moved.Add(falling[1]);

        CheckConnect(moved);
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

    void UpdateCoords()
    {
        x1 = (int)falling[0].transform.position.x;
        y1 = (int)falling[0].transform.position.y;
        x2 = (int)falling[1].transform.position.x;
        y2 = (int)falling[1].transform.position.y;

        // grid coordinates
        gx1 = (int)(x1 - cornerPos.x);
        gy1 = (int)(y1 - cornerPos.y + _height);
        gx2 = (int)(x2 - cornerPos.x);
        gy2 = (int)(y2 - cornerPos.y + _height);
    }

    bool BottomTouching()
    {
        return (y1 == cornerPos.y - _height || y2 == cornerPos.y - _height || grid[gx1, gy1 - 1] || grid[gx2, gy2 - 1]);
    }

    // Start is called before the first frame update
    new void Start()
    {
        SetWidth(6);
        SetHeight(12);
        generator = GameObject.Find("PieceGenerator").GetComponent<PieceGenerator>();
        immediateNext = new Block[2, 2];
        base.Start();
        sounds = GetComponents<AudioSource>();
        connect = sounds[0];
        move = sounds[1];
        rotate = sounds[2];
        grid = new Block[_width, _height+2];
    }

    // Update is called once per frame
    new void Update()
    {
        if (GameScreen.startTimer > 0)
        {
            return;
        }

        moved = new List<Block>();

        if (clearedTime > 0)
        {
            clearedTime -= Time.deltaTime;

            if (clearedTime <= 0)
            {
                moved = DropAll();
                CheckConnect(moved);
            }
            
        }
        else
        {
            if (!falling[0] || !falling[1])
            {
                while (next.Count < 1)
                {
                    if (GameScreen.multiplayer)
                        generator.getPuyoServerRpc();
                    else
                        AddToQueue(UnityEngine.Random.Range(0, 16));
                }

                CreateFallingPuyoBlock();
                UpdateNext();
            }

            UpdateCoords();

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                if (keyPressed == 0 || keyPressed > .5f) // move immediately on first press, then zoom after 0.5s
                {
                    MoveLeft();
                }

                keyPressed += Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                if (keyPressed == 0 || keyPressed > .5f) // move immediately on first press, then zoom after 0.5s
                {
                    MoveRight();
                }

                keyPressed += Time.deltaTime;
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                RotateCCW();
                if (BottomTouching())
                    update = 0; // reset timer to prevent immediate drop

                if (touchCount >= touchLimit)
                {
                    UpdateCoords();
                    Drop();
                    touchCount = 0;
                    return;
                }
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                RotateCW();
                if (BottomTouching())
                    update = 0; // reset timer to prevent immediate drop

                if (touchCount >= touchLimit)
                {
                    UpdateCoords();
                    Drop();
                    touchCount = 0;
                    return;
                }
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
                update -= dropTime;

                if (BottomTouching())
                {
                    Drop();
                }
                else
                    MoveDown();
                
            }
        }
    }
}

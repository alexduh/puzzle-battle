using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PuyoPuyo : Player
{
    [SerializeField] private Block _nuisancePuyo;
    [SerializeField] private Block _redPuyo;
    [SerializeField] private Block _yellowPuyo;
    [SerializeField] private Block _greenPuyo;
    [SerializeField] private Block _bluePuyo;

    private AudioSource[] sounds;
    private AudioSource connect, move, rotate;

    private float update, clearedTime, primaryBlink;
    private float keyPressed;
    private PieceGenerator generator;
    private int touchLimit = 12;
    private int touchCount;

    private int nuisancePosition;
    private int scoreRemainder;
    private int targetScore = 70;
    private int chainCount;

    List<Block> moved;
    Block[] falling = new Block[2];
    int x1, y1, x2, y2, gx1, gx2, gy1, gy2;

    private struct MovementFlags : INetworkSerializable
    {
        public bool movedLeft;
        public bool movedRight;
        public bool movedDown;
        public bool rotatedCW;
        public bool rotatedCCW;
        public bool dropped;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref movedLeft);
            serializer.SerializeValue(ref movedRight);
            serializer.SerializeValue(ref movedDown);
            serializer.SerializeValue(ref rotatedCW);
            serializer.SerializeValue(ref rotatedCCW);
            serializer.SerializeValue(ref dropped);
        }
    }

    new void OnEnable()
    {
        base.OnEnable();
        scoreRemainder = 0;
        nuisancePosition = 0;

        // Set the target player to the opponent
        if (GameScreen.multiplayer)
        {
            foreach (Transform child in transform.parent)
            {
                PuyoPuyo opponent = child.gameObject.GetComponent<PuyoPuyo>();
                if (opponent.OwnerClientId != OwnerClientId)
                    targetPlayerId = opponent.OwnerClientId;
            }
        }   
    }

    new void OnDisable()
    {
        base.OnDisable();
        if (falling[0])
        {
            Destroy(falling[0].gameObject);
            falling[0] = null;
        }
        if (falling[1])
        {
            Destroy(falling[1].gameObject);
            falling[1] = null;
        }

        for (int i = 0; i < immediateNext.GetLength(0); i++)
        {
            for (int j = 0; j < immediateNext.GetLength(1); j++)
            {
                Destroy(immediateNext[i, j].gameObject);
                immediateNext[i, j] = null;
            }
        }

        while (next.Count > 0)
            next.Dequeue();

        DeleteGrid();
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
        grid = new Block[_width, _height * 2];
    }

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

    void GetBlock()
    {
        if (next.Count < 1)
        {
            if (GameScreen.multiplayer)
                generator.GetPuyoServerRpc();
            else
                AddToQueue(UnityEngine.Random.Range(0, 16));
        }
        if (next.Count >= 1)
        {
            CreateFallingPuyoBlock();
            UpdateNext();
        }

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
        UpdateCoords();
        if (LeftOpen(falling[0]) && LeftOpen(falling[1]))
        {
            falling[0].transform.position = new Vector3(x1 - 1, y1);
            falling[1].transform.position = new Vector3(x2 - 1, y2);
            move.Play();
        }
    }

    void MoveRight()
    {
        UpdateCoords();
        if (RightOpen(falling[0]) && RightOpen(falling[1]))
        {
            falling[0].transform.position = new Vector3(x1 + 1, y1);
            falling[1].transform.position = new Vector3(x2 + 1, y2);
            move.Play();
        }
    }

    void MoveDown()
    {
        UpdateCoords();
        falling[0].transform.position = new Vector3(x1, y1 - 1);
        falling[1].transform.position = new Vector3(x2, y2 - 1);
    }

    int bufferedRotate = 0;

    void RotateCCW()
    {
        UpdateCoords();
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
            }
        }

        rotate.Play();
    }

    void RotateCW()
    {
        UpdateCoords();
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
            for (int j = 0; j < _height * 2; j++)
            {
                if (grid[i, j])
                {
                    for (int h = 0; h < j; h++)
                    {
                        if (!grid[i, h])
                        {
                            grid[i, h] = grid[i, j];
                            grid[i, h].falling = true;
                            grid[i, h]._y = h;

                            grid[i, j] = null;
                            if (grid[i, h].color != Block.Color.None) // do not add nuisance blocks to the list, they'll never cause a match
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
                continue;

            list.Add(current);
            bx = (int)current._x;
            by = (int)current._y;

            if (match == Block.Color.None)
                match = current.color;

            if (bx > 0 && grid[bx - 1, by] && grid[bx - 1, by].color == match)
                stack.Push(grid[bx - 1, by]);
            if (by > 0 && grid[bx, by - 1] && grid[bx, by - 1].color == match)
                stack.Push(grid[bx, by - 1]);
            if (bx < 5 && grid[bx + 1, by] && grid[bx + 1, by].color == match)
                stack.Push(grid[bx + 1, by]);
            if (by < 12 && grid[bx, by + 1] && grid[bx, by + 1].color == match)
                stack.Push(grid[bx, by + 1]);

        }

        return list;
    }

    int nCr(int n, int r)
    {
        return n! / (r! * (n - r)!);
    }

    /** This function checks for any connections made by Blocks in 'moved',
     *  then marks all connected blocks to be deleted during the Update cycle.
     */
    void CheckConnect(List<Block> moved)
    {
        List<Block> list;
        bool chainIncremented = false;
        int clearedCurrent = 0;
        List<Block.Color> colors = new List<Block.Color>();

        foreach (Block startBlock in moved)
        {           
            if (startBlock.color == Block.Color.None)
                continue;

            Stack<Block> stack = new Stack<Block>();
            stack.Push(startBlock);
            list = FindChain(stack);

            if (list.Count >= 4)
            {
                if (!colors.Contains(list[0].color))
                    colors.Add(list[0].color); // add color to list of colors cleared

                if (!chainIncremented)
                {
                    chainIncremented = true;
                    chainCount++;
                }

                connect.pitch = .35f + .05f * chainCount;
                connect.Play();
                clearedTime = 1.0f;
                foreach (Block block in list)
                {
                    ClearAdjacentNuisance(block);
                    grid[(int)(block._x), (int)(block._y)] = null;

                    block.destroy = true;
                    clearedCurrent++;
                    clearedTotal++;
                    if (clearedTotal >= 40)
                    {
                        level++;
                        dropTime *= .9f; // decrease dropTime by 10%
                        clearedTotal -= 40;
                    }
                }
            }
        }

        if (clearedCurrent > 0)
        {
            scoreRemainder += scoreObject.CalculateScore(clearedCurrent, chainCount, colors.Count);
            if (GameScreen.multiplayer)
            {
                int garbage = CalculateGarbage();
                ProcessGarbage(garbage);
            }
            
        }
            
        if (!chainIncremented)
        {
            chainCount = 0;
            if (grid.Length == 0) // TODO: fix the length calculation to work!
                scoreRemainder += 2100; // all clear bonus (30 nuisance blocks) TODO: create function for this, add SFX and text popup

            gs.ChainEnded(targetPlayerId);
            SpawnGarbage();
        }
            
    }
    void ClearAdjacentNuisance(Block b)
    {
        int bx = (int)b._x;
        int by = (int)b._y;

        if (bx < _width-1 && grid[bx+1, by] && grid[bx+1, by].color == Block.Color.None)
        {
            grid[bx + 1, by].destroy = true;
            grid[bx + 1, by] = null;
        }
        if (bx > 0 && grid[bx-1, by] && grid[bx-1, by].color == Block.Color.None)
        {
            grid[bx - 1, by].destroy = true;
            grid[bx - 1, by] = null;
        }
        if (by < _height*2 && grid[bx, by+1] && grid[bx, by+1].color == Block.Color.None)
        {
            grid[bx, by + 1].destroy = true;
            grid[bx, by + 1] = null;
        }
        if (by > 0 && grid[bx, by-1] && grid[bx, by-1].color == Block.Color.None)
        {
            grid[bx, by - 1].destroy = true;
            grid[bx, by - 1] = null;
        }
            
    }

    int CalculateGarbage()
    {
        int garbageSent = scoreRemainder / targetScore;
        scoreRemainder %= targetScore;

        return garbageSent;
    }

    void SpawnGarbage()
    {
        int x = nuisancePosition;
        int y = _height - 1;
        if (incomingGarbage > 0)
            Debug.Log("Player " + OwnerClientId + " Incoming garbage: " + incomingGarbage);

        for (; incomingGarbage > 0; incomingGarbage--)
        {
            grid[x, y + _height] = Instantiate(_nuisancePuyo, cornerPos + new Vector3(x, y), Quaternion.identity);
            x++;
            if (x >= _width)
            {
                x = 0;
                y--;
            }
        }

        nuisancePosition = x;
        DropAll();
    }

    void Drop()
    {
        UpdateCoords();
        falling[0].GetComponent<Renderer>().enabled = true;
        
        grid[gx1, gy1] = falling[0];
        grid[gx2, gy2] = falling[1];
        falling[0]._x = gx1;
        falling[0]._y = gy1;
        falling[1]._x = gx2;
        falling[1]._y = gy2;
        DropAll();

        moved.Add(falling[0]);
        moved.Add(falling[1]);

        CheckConnect(moved);
        if (clearedTime > 0)
            return;

        SpawnGarbage();

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
        UpdateCoords();
        return (y1 == cornerPos.y - _height || y2 == cornerPos.y - _height || grid[gx1, gy1 - 1] || grid[gx2, gy2 - 1]);
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdateOpponentServerRpc(ulong clientId, MovementFlags flags)
    {
        UpdateOpponentClientRpc(clientId, flags);
    }

    [ClientRpc]
    void UpdateOpponentClientRpc(ulong clientId, MovementFlags flags)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
            return;

        if (falling[0] && falling[1])
            UpdateCoords();

        if (flags.movedLeft)
            MoveLeft();
        if (flags.movedRight)
            MoveRight();
        if (flags.movedDown)
            MoveDown();
        if (flags.rotatedCW)
            RotateCW();
        if (flags.rotatedCCW)
            RotateCCW();
        if (flags.dropped)
            Drop();
        // Update opponent's grid
    }

    // Update is called once per frame
    new void Update()
    {
        MovementFlags flags = new MovementFlags();
        flags.movedDown = false;
        flags.movedLeft = false;
        flags.movedRight = false;
        flags.rotatedCW = false;
        flags.rotatedCCW = false;
        flags.dropped = false;

        if (GameScreen.startTimer > 0)
            return;

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
                GetBlock();

            if (!falling[0] || !falling[1] || (GameScreen.multiplayer && !IsOwner))
                return;

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                if (keyPressed == 0 || keyPressed > .5f) // move immediately on first press, then zoom after 0.5s
                {
                    MoveLeft();
                    flags.movedLeft = true;
                }

                keyPressed += Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                if (keyPressed == 0 || keyPressed > .5f) // move immediately on first press, then zoom after 0.5s
                {
                    MoveRight();
                    flags.movedRight = true;
                }

                keyPressed += Time.deltaTime;
            }
            if (Input.GetKeyDown(KeyCode.Z))
            {
                RotateCCW();
                flags.rotatedCCW = true;
                if (BottomTouching())
                {
                    touchCount++;
                    update = 0; // reset timer to prevent immediate drop
                }

                if (touchCount >= touchLimit)
                {
                    touchCount = 0;
                    update = dropTime;
                }
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                RotateCW();
                flags.rotatedCW = true;
                if (BottomTouching())
                {
                    touchCount++;
                    update = 0; // reset timer to prevent immediate drop
                }
                    
                if (touchCount >= touchLimit)
                {
                    touchCount = 0;
                    update = dropTime;
                }
            }

            if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow))
                keyPressed = 0;

            if (Input.GetKey(KeyCode.DownArrow))
                update += 10 * Time.deltaTime;
            else
                update += Time.deltaTime;

            primaryBlink += Time.deltaTime;

            if (primaryBlink > 0.1f)
            {
                falling[0].GetComponent<Renderer>().enabled ^= true;
                primaryBlink = 0;
            }

            if (update >= dropTime)
            {
                update -= dropTime;

                if (BottomTouching())
                {
                    Drop();
                    flags.dropped = true;
                }
                else
                {
                    MoveDown();
                    flags.movedDown = true;
                }
                    
            }

            UpdateOpponentServerRpc(OwnerClientId, flags);
        }
    }
}

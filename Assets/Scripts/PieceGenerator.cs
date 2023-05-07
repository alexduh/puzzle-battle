using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceGenerator : MonoBehaviour
{
    enum TetrisPieces
    {
        L,
        J,
        O,
        S,
        Z,
        T,
        I
    }

    private List<int> tetrisBag = new List<int>();

    // TODO: PieceGenerator needs to be able to access every player's pieceQueue!

    // called when any Tetris player's Queue is too small (< 5). Generate an int representing a new Tetris Piece and
    // pushes it out to all Tetris players' Queues
    public void getTetris()
    {
        if (tetrisBag.Count <= 0)
        {
            for (int i = 0; i < 7; i++)
            {
                tetrisBag.Add(i);
            }
        }
        // TODO: put the piece into each player's queue!
        int ret = tetrisBag[Random.Range(0, tetrisBag.Count)];
        tetrisBag.RemoveAt(ret);

    }

    // TODO: called when any Puyo player's Queue is too small (< 3). Generate an int representing a new Puyo Piece and
    // pushes it out to all Puyo players' Queues
    public static void getPuyo(GameObject players)
    {
        int selected = Random.Range(0, 16);
        // place the piece into each puyo player's queue!
        foreach (Transform child in players.transform)
        {
            PuyoPuyo puyoPlayer = child.GetComponent<PuyoPuyo>();
            if (puyoPlayer)
            {
                puyoPlayer.AddToQueue(selected);
            }
            
        }
        return;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

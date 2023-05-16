using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PieceGenerator : NetworkBehaviour
{
    [SerializeField] private GameObject players;

    //private static NetworkVariable<int> puyoPiece = new NetworkVariable<int>(-1);
    private List<int> tetrisBag = new List<int>();

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

    /** Called when any Puyo player's Queue is too small (< 3). Generate an int representing a new Puyo Piece and
    *   pushes it out to all Puyo players' Queues
    **/

    [ServerRpc(RequireOwnership = false)]
    public void GetPuyoServerRpc()
    {
        int selected = Random.Range(0, 16);

        // place the piece into each puyo player's queue!
        foreach (Transform child in players.transform)
        {
            PuyoPuyo puyoPlayer = child.GetComponent<PuyoPuyo>();
            if (puyoPlayer)
                GetPuyoClientRpc(puyoPlayer.OwnerClientId, selected);
            
        }

    }

    [ClientRpc]
    private void GetPuyoClientRpc(ulong p, int selected)
    {
        foreach (Transform child in players.transform)
        {
            PuyoPuyo puyoPlayer = child.GetComponent<PuyoPuyo>();
            if (p == puyoPlayer.OwnerClientId)
                puyoPlayer.AddToQueue(selected);
        }
        
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

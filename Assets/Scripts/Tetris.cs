using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tetris : Player
{
    Block[,] grid;
    private float update, started;

    //enum Piece = {I, L, J, S, Z, O, T};

    void rotateCCW()
    {

    }

    void rotateCW()
    {

    }

    void onEnable()
    {
        // TODO: 3-sec countdown timer to start game
    }

    // Start is called before the first frame update
    void Start()
    {
        grid = new Block[_width, _height + 2];
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow)) {

        }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {

        }
        if (Input.GetKeyDown(KeyCode.RightArrow)) {

        }
        if (Input.GetKeyDown(KeyCode.Z)) {
            rotateCCW();
        }
        if (Input.GetKeyDown(KeyCode.X)) {
            rotateCW();
        }

        update += Time.deltaTime;
        if (update >= 1.0f)
        {
            // TODO: decrease vertical position OR drop piece and update grid
            update -= 1.0f;
        }
    }
}

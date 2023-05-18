using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GameOver : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject playerList;
    [SerializeField] private GameObject readyButton;
    [SerializeField] private AudioSource loseSound;
    [SerializeField] private AudioSource winSound;
    [SerializeField] private TMP_Text endMessage;

    private float update;

    private void OnEnable()
    {
        update = 5.0f;
    }

    public void Lose()
    {
        loseSound.Play();
        if (GameScreen.multiplayer)
            endMessage.text = "You Lose!";
        else
            endMessage.text = "Game Over!";
    }

    public void Win()
    {
        winSound.Play();
        endMessage.text = "You Win!";
    }

    // Update is called once per frame
    void Update()
    {
        update -= Time.deltaTime;
        if (update <= 0)
        {
            this.gameObject.SetActive(false);
            mainMenu.SetActive(true);
            readyButton.gameObject.SetActive(true);

            foreach (Transform child in playerList.transform)
                child.GetChild(0).gameObject.SetActive(true);

        }
    }
}

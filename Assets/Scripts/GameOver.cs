using TMPro;
using UnityEngine;

public class GameOver : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject playerList;
    [SerializeField] private GameObject readyButton;
    [SerializeField] private AudioSource loseSound;
    [SerializeField] private AudioSource winSound;
    [SerializeField] private TMP_Text endMessage;

    public float gameEnded;

    private void OnEnable()
    {
        gameEnded = 5.0f;
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
        gameEnded -= Time.deltaTime;
        if (gameEnded <= 0)
        {
            this.gameObject.SetActive(false);
            mainMenu.SetActive(true);
            readyButton.gameObject.SetActive(true);

            foreach (Transform child in playerList.transform)
            {
                child.gameObject.GetComponent<Player>().DeleteGrid();
                child.GetChild(0).gameObject.SetActive(true);
                child.GetChild(2).gameObject.SetActive(false);
            }

        }
    }
}

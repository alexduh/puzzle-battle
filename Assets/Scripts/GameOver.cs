using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GameOver : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject playerList;
    [SerializeField] private GameObject readyButton;

    public float update;

    private void OnEnable()
    {
        update = 5.0f;
    }

    // Start is called before the first frame update
    void Start()
    {

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

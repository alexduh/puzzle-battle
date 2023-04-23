using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOver : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;

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
        }

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScreen : MonoBehaviour
{
    [SerializeField] private Transform _cam;

    private void Awake()
    {
        _cam.GetComponent<Camera>().orthographicSize = 7.5f;
        _cam.transform.position = new Vector3(2.5f, 5.5f, -10);
        //_cam.GetComponent<Camera>().orthographicSize = (float)_height * 0.625f;
        //_cam.transform.position = new Vector3((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f, -10);
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

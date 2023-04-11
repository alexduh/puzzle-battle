using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    private float update;

    public enum Color
    {
        Red,
        Yellow,
        Green,
        Blue,
        None
    }

    public Color color;
    
    // blocks are not destroyed immediately, to aid in connect logic and animation of destruction
    public bool destroy = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (destroy)
        {
            update += Time.deltaTime;
            int interval = (int)Mathf.Round(update / .2f);
            if (interval % 2 == 0)
            {
                this.GetComponent<Renderer>().enabled = false;
            }
            else
            {
                this.GetComponent<Renderer>().enabled = true;
            }
            
            if (update > 1.0f)
            {
                update = 0.0f;
                Destroy(this.gameObject);
            }
        }
    }
}

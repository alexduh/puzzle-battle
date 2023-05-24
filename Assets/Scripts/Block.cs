using UnityEngine;

public class Block : MonoBehaviour
{
    private float update;
    private float fallTime;

    public enum Color
    {
        Red,
        Yellow,
        Green,
        Blue,
        None
    }

    // blocks are not destroyed immediately, to aid in connect logic and animation of destruction
    public bool destroy = false;
    public bool falling = false;
    public float _x, _y;
    public Color color;

    public void Falling(float endY)
    {
        if (transform.position.y > endY)
            transform.position = new Vector3(transform.position.x, transform.position.y - .1f);
        else
        {
            transform.position = new Vector3(transform.position.x, endY);
            falling = false;
        }
            
    }

    // Update is called once per frame
    void Update()
    {
        if (falling)
            fallTime += Time.deltaTime;
        else
            fallTime = 0;

        if (fallTime >= .0025f) // .3 seconds to fall 12 units
        {
            Falling(_y - 6);
            fallTime -= .0025f;
        }
            
        if (destroy)
        {
            update += Time.deltaTime;
            int interval = (int)Mathf.Round(update / .2f);
            if (interval % 2 == 0)
                this.GetComponent<Renderer>().enabled = false;
            else
                this.GetComponent<Renderer>().enabled = true;
            
            if (update > 1.0f)
            {
                update = 0.0f;
                Destroy(this.gameObject);
            }
        }
    }
}

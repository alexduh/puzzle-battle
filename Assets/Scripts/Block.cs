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

    // blocks are not destroyed immediately, to aid in connect logic and animation of destruction
    public bool destroy = false;
    public bool falling = false;
    public float _x, _y;
    public Color color;

    public void Falling(float endY)
    {
        if (transform.position.y > endY)
            transform.position = new Vector3(transform.position.x, transform.position.y - .25f);
        else
            falling = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (falling)
            Falling(_y - 6);
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

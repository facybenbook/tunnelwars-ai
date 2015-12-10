using UnityEngine;
using System.Collections;

public class GroundControl : MonoBehaviour
{

    public Sprite[] groundSprites;
    private SpriteRenderer rend;
    private int num;
    private bool playing = false;

	// Use this for initialization
    void Start()
    {
        num = 45 - (int) transform.position.x / 64;
        rend = GetComponent<SpriteRenderer>();
        rend.sprite = groundSprites[num];
    }

    void Update()
    {
        if (playing)
        {
            num += 1;
            if (num > 45) num = 0;
            rend.sprite = groundSprites[num];
        }
    }

	void Play()
    {
        playing = true;
	}
}

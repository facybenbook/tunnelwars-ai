using UnityEngine;
using System.Collections;

// NOTE: This script only handles animation. See the control object for the real game logic.

public class PlayerControl : MonoBehaviour
{
    private bool masterDisplay = false;

    private float xPrevious = 0.0f;
    private Animator animator;

    // Initialization
	void Start()
    {
        animator = GetComponent<Animator>();
	}
	
    // Update animation
    void Update()
    {
        if (xPrevious == transform.position.x) animator.SetBool("MovingHorizontally", false);
        else animator.SetBool("MovingHorizontally", true);

        animator.SetBool("Master", masterDisplay);

        xPrevious = transform.position.x;
    }

    void SetMaster(bool mode)
    {
        masterDisplay = mode;
    }
}

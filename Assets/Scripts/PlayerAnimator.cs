using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField]
    private Animator _playerAnimator;

    private void Awake()
    {
        _playerAnimator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SwitchAnimations();
    }

    public void SwitchAnimations()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        //transition to running
        if(horizontalInput != 0 )
        {
            _playerAnimator.SetBool("isRunning", true);
        } 
        else
        {
            _playerAnimator.SetBool("isRunning", false);
        }

        //transition to jumping
        //a checklist for a good jump: power of jump; duration needed to reach max height; max height; force of gravity; duration needed to get back to the ground
    }
}

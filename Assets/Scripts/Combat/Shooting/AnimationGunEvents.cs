using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationGunEvents : MonoBehaviour
{
    [Header("Animation")]
    public Animator anim;
    
    [Header("Audio")]
    public AudioSource animSounds;

    private void Start()
    {
        AudioSource animSounds = gameObject.GetComponent<AudioSource>();
    }

    void StopShooting()
    {
        anim.SetBool("IsShooting", false);
    }
    
    void StopReloading()
    {
        anim.SetBool("IsReloading", false);
    }

    void PlaySound()
    {
        animSounds.Play();
    }
}

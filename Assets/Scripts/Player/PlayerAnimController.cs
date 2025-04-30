using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimController : MonoBehaviour
{
    public Gun gun;
    
        [Header("Animation")]
        public Animator anim;
        
    void Update()
    {
        if (Input.GetButtonDown("Fire1") && Time.time - gun.lastFireTime >= gun.fireRate && !gun.isReloading && gun.currentAmmo > 0)
        {
            Shooting();
        }

        if (gun.isFullAutomatic)
        {
            if (Input.GetButton("Fire1") && Time.time - gun.lastFireTime >= gun.fireRate && gun.currentAmmo > 0)
            {
                Shooting();
            }
        }
    }

    private void Shooting()
    {
        anim.SetBool("IsShooting", true);
    }
    
    private void StopShooting()
    {
        anim.SetBool("IsShooting", false);
    }
}

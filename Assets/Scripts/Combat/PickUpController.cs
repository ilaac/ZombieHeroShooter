using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpController : MonoBehaviour
{
    [Header("Logic")]
    public Gun gunScript;
    public Rigidbody rb;
    public BoxCollider Collider;
    public Transform player, gunContainer, fpsCam;
    public Outline outlineScript;
    public PlayerAnimController playerAnimScript;
    public Animator animator;

    [Header("Pickup Settings")]
    public float pickUpRange;
    public float dropForwardForce, dropUpwardForce;

    [Header("Throw Settings")]
    public int damage;
    public string thrownWeaponTag = "ThrownWeapon";
    public GameObject hitParticlePrefab;
    public Transform throwPosition;
    public bool equipped;
    public static bool slotFull;
    

    private void Start()
    {
        if (!equipped)
        {
            animator.enabled = false;
            gunScript.enabled = false;
            rb.isKinematic = false;
            Collider.isTrigger = false;
            outlineScript.enabled = true;
            playerAnimScript.enabled = false;
        }
        if (equipped)
        {
            animator.enabled = true;
            gunScript.enabled = true;
            playerAnimScript.enabled = true;
            rb.isKinematic = true;
            Collider.isTrigger = true;
            outlineScript.enabled = false;
            slotFull = true;
        }
    }

    private void Update()
    {
        Vector3 distanceToPlayer = player.position - transform.position;

        if (!equipped && distanceToPlayer.magnitude <= pickUpRange && Input.GetKeyDown(KeyCode.E) && !slotFull)
        {
            PickUp();
        }
        if (equipped && Input.GetKeyDown(KeyCode.Q))
        {
            Drop();
        }
    }

    private void PickUp()
    {
        equipped = true;
        gunScript.enabled = true;
        slotFull = true;
        animator.enabled = true;
        playerAnimScript.enabled = true;

        transform.SetParent(gunContainer);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
        transform.localScale = Vector3.one;

        rb.isKinematic = true;
        Collider.enabled = false;

        gunScript.enabled = true;
        outlineScript.enabled = false;

        // Assign animator to PlayerMovement
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.SetAnimator(animator);
        }

        if (equipped)
        {
            playerMovement.SetAnimator(animator);
        }

        // Remove thrown weapon tag when picked up
        gameObject.tag = "Untagged";
    }


    private void Drop()
    {
        equipped = false;
        slotFull = false;
        gunScript.enabled = false;
        animator.enabled = false;
        playerAnimScript.enabled = false;
        
        gunScript.crosshair.SetActive(false);

        transform.SetParent(null);

        rb.isKinematic = false;
        Collider.enabled = true;

        // Reset velocity to avoid any previous momentum issues
        rb.velocity = Vector3.zero;

        // Set the position from which the weapon will be thrown
        Vector3 throwStartPosition = throwPosition.position;

        // Add force from the throw position
        rb.AddForce(fpsCam.forward * dropForwardForce, ForceMode.Impulse);
        rb.AddForce(fpsCam.up * dropUpwardForce, ForceMode.Impulse);

        // Apply random torque to simulate spin
        float random = Random.Range(-1f, 1f);
        rb.AddTorque(new Vector3(random, random, random) * 2.5f, ForceMode.Impulse);

        gunScript.enabled = false;
        outlineScript.enabled = true;

        // Assign the tag for the thrown weapon
        gameObject.tag = thrownWeaponTag;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the weapon is thrown (has the thrown weapon tag)
        if (gameObject.tag == thrownWeaponTag)
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                // Spawn the hit particle effect
                if (hitParticlePrefab != null)
                {
                    GameObject particle = Instantiate(hitParticlePrefab, contact.point, Quaternion.LookRotation(contact.normal));
                    Destroy(particle, 2f);
                }

                // Check if the collided object has the Target component
                Target target = collision.gameObject.GetComponent<Target>();
                if (target != null)
                {
                    // Apply damage and pass the hit position & rotation
                    target.TakeDamage(damage, contact.point, Quaternion.LookRotation(contact.normal));
                }
            }

            // Despawn the weapon after impact
            Destroy(gameObject);
        }
    }
}

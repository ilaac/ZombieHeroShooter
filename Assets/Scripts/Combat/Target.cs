using UnityEngine;

public class Target : MonoBehaviour
{
    [Header("Target Stats")]
    public float health = 50f;
    
    [Header("Blood Effect")]
    public GameObject bloodEffectPrefab;
    public float bloodEffectDuration = 2f;

    public void TakeDamage(float damage, Vector3 hitPoint, Quaternion hitRotation)
    {
        health -= damage;
    
        // Pass the hit point and hit rotation to spawn the blood effect at the right location
        SpawnBloodEffect(hitPoint, hitRotation);
    
        if (health <= 0f)
        {
            Die();
        }
    }

    void SpawnBloodEffect(Vector3 position, Quaternion hitRotation)
    {
        if (bloodEffectPrefab != null)
        {
            // Instead of using the direction relative to the shooter, 
            // use the hit normal (the direction the surface is facing at the hit point)
            Vector3 hitNormal = hitRotation * Vector3.up;

            // Spawn the blood effect at the hit position, facing the normal of the hit surface
            GameObject bloodEffect = Instantiate(bloodEffectPrefab, position, Quaternion.LookRotation(hitNormal));

            Destroy(bloodEffect, bloodEffectDuration);
        }
    }
    
    void Die()
    {
        Debug.Log(gameObject.name + " has been destroyed.");
        Destroy(gameObject);
    }
}
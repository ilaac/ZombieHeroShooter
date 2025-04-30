
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class Gun : MonoBehaviour
{
    [Header("Gun Stats")]
    public float damage;
    public int magSize;
    public int spareAmmo;
    public int roundsPerMinute;
    public int currentAmmo;
    public float reloadTime;
    public bool isFullAutomatic = false;

    [Header("Shotgun Mode")]
    public bool isShotgunMode = false;
    public int pelletsPerShot = 5;
    public float shotgunSpread = 0.1f;
    public float adsspreadTightenFactor = 0.5f;

    [Header("Particles & Effects")]
    public GameObject shootParticlePrefab;
    public GameObject hitParticlePrefab;
    public float hitParticleLifetime = 2f;
    
    [Header("Audio")]
    public AudioClip[] gunshotSounds;
    public AudioClip[] noAmmoSounds;
    private AudioSource audioSource;

    [Header("References")]
    [HideInInspector] public float fireRate;
    [HideInInspector] public float lastFireTime;
    [HideInInspector] public bool isReloading = false;
    public Camera playerCamera;
    public Transform raycastOrigin;
    public Transform gunTransform;
    public Transform shootParticleSpawnPoint;
    public GameObject crosshair;
    public Recoil recoil;
    public ProceduralRecoil recoilScript;
    
    [Header("Animation")]
    public Animator anim;

    private Rigidbody rb;
    private Vector3 originalGunPosition;
    private Quaternion originalGunRotation;
    private Coroutine recoilCoroutine;
    private Vector3 originalCameraPosition;
    private Vector3 cameraRecoilVelocity;
    private Coroutine noAmmoSoundCoroutine;

    [Header("Hipfire Spread Settings")]
    [Range(0f, 1f)] public float spreadX;
    [Range(0f, 1f)] public float spreadY;
    [Range(0f, 1f)] public float spreadZ;

    void Start()
    {
        currentAmmo = magSize;
        originalGunPosition = gunTransform.localPosition;
        originalGunRotation = gunTransform.localRotation;
        originalCameraPosition = playerCamera.transform.localPosition;

        if (shootParticleSpawnPoint == null)
        {
            shootParticleSpawnPoint = gunTransform;
        }

        SetFireRateFromRPM();

        if (crosshair != null)
        {
            crosshair.SetActive(true);
        }

        recoil = GetComponentInChildren<Recoil>();
        recoilScript = GetComponentInParent<ProceduralRecoil>();

        if (recoilScript == null)
        {
            Debug.LogError("ProceduralRecoil script not found in parent! Make sure the gun is a child of the camera.");
        }

        // Initialize AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Set AudioSource properties (optional, customize as needed)
        audioSource.loop = false; // Ensure it's not looping
        audioSource.playOnAwake = false; // Prevents audio from playing automatically when the scene starts
    }

    void Update()
    {
        if (isReloading) return;

        if (isFullAutomatic)
        {
            if (Input.GetButton("Fire1") && Time.time - lastFireTime >= fireRate)
            {
                Shoot();
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1") && Time.time - lastFireTime >= fireRate)
            {
                Shoot();
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
    }

    public void Shoot()
    {
        if (currentAmmo > 0)
        {
            currentAmmo--;
            lastFireTime = Time.time;
            anim.SetBool("IsShooting", true);

            PlayGunshotSound();

            if (recoilCoroutine != null)
            {
                StopCoroutine(recoilCoroutine);
            }
            recoilCoroutine = StartCoroutine(HandleRecoilCoroutine());

            if (shootParticlePrefab != null)
            {
                GameObject shootParticle = Instantiate(shootParticlePrefab, shootParticleSpawnPoint.position, shootParticleSpawnPoint.rotation);
                Destroy(shootParticle, 1f);
            }

            Transform origin = (raycastOrigin != null) ? raycastOrigin : playerCamera.transform;
            Vector3 direction = origin.forward;

            if (isShotgunMode)
            {
                recoilScript.FireRecoil();
                ShootShotgunPellets(origin);
            }
            else
            {

                FireBullet(direction);
            }

            recoilScript.FireRecoil();
        }
        else
        {
            PlayNoAmmoSound(); // Play out of ammo sound
        }
    }

    private void ShootShotgunPellets(Transform origin)
    {
        float currentSpread = shotgunSpread;

        for (int i = 0; i < pelletsPerShot; i++)
        {
            Vector3 spreadDirection = ApplyShotgunSpread(origin.forward, currentSpread);
            FireBullet(spreadDirection);
        }
    }

    private Vector3 ApplyShotgunSpread(Vector3 direction, float spreadAmount)
    {
        direction.x += Random.Range(-spreadAmount, spreadAmount);
        direction.y += Random.Range(-spreadAmount, spreadAmount);
        direction.z += Random.Range(-spreadAmount, spreadAmount);
        return direction;
    }

    private void FireBullet(Vector3 direction)
    {
        if (Physics.Raycast(playerCamera.transform.position, direction, out RaycastHit hit))
        {
            var target = hit.collider.GetComponent<Target>();
            if (target != null)
            {
                Quaternion hitRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                target.TakeDamage(damage, hit.point, hitRotation);
            }

            if (hitParticlePrefab != null && target == null)
            {
                Quaternion hitRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                Vector3 spawnPosition = hit.point + hit.normal * 0.02f;
                GameObject hitParticle = Instantiate(hitParticlePrefab, spawnPosition, hitRotation);
                Destroy(hitParticle, hitParticleLifetime);
            }
        }
    }

    // Hipfire spread application remains as is
    Vector3 ApplyHipFireSpread(Vector3 direction)
    {
        direction.x += Random.Range(-spreadX, spreadX);
        direction.y += Random.Range(-spreadY, spreadY);
        direction.z += Random.Range(-spreadZ, spreadZ);
        return direction;
    }

    void SetFireRateFromRPM()
    {
        fireRate = 60f / roundsPerMinute;
        Debug.Log($"Fire rate set: {fireRate} (RPM: {roundsPerMinute})");
    }

    void Reload()
    {
        if (isReloading || currentAmmo >= magSize) return;
        isReloading = true;
        Invoke(nameof(FinishReload), reloadTime);
        anim.SetBool("IsReloading", true);
    }

    void FinishReload()
    {
        int ammoNeeded = magSize - currentAmmo;
        int ammoToReload = Mathf.Min(ammoNeeded, spareAmmo);
        currentAmmo += ammoToReload;
        spareAmmo -= ammoToReload;
        isReloading = false;
    }

    void PlayGunshotSound()
    {
        if (gunshotSounds.Length > 0 && audioSource != null)
        {
            int randomIndex = Random.Range(0, gunshotSounds.Length);
            AudioClip clipToPlay = gunshotSounds[randomIndex];

            if (clipToPlay == null)
            {
                Debug.LogError("Gunshot sound is missing or null!");
            }
            else
            {
                audioSource.PlayOneShot(clipToPlay);
            }
        }
        else
        {
            Debug.LogError("No gunshot sounds assigned or AudioSource is missing!");
        }
    }

    void PlayNoAmmoSound()
    {
        if (noAmmoSounds.Length > 0 && audioSource != null)
        {
            if (noAmmoSoundCoroutine != null)
            {
                StopCoroutine(noAmmoSoundCoroutine);
            }

            noAmmoSoundCoroutine = StartCoroutine(PlayNoAmmoSoundWithDelay());
        }
        else
        {
            Debug.LogError("No out of ammo sounds assigned or AudioSource is missing!");
        }
    }

    private IEnumerator PlayNoAmmoSoundWithDelay()
    {
        yield return new WaitForSeconds(0.045f);

        int randomIndex = Random.Range(0, noAmmoSounds.Length);
        AudioClip clipToPlay = noAmmoSounds[randomIndex];

        if (clipToPlay == null)
        {
            Debug.LogError("Out of ammo sound is missing or null!");
        }
        else
        {
            audioSource.PlayOneShot(clipToPlay);
        }
    }

    private IEnumerator HandleRecoilCoroutine()
    {
        float elapsedTime = 0f;
        while (elapsedTime < 0.1f)
        {
            recoil.RecoilFire();
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}

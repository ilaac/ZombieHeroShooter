using UnityEngine;

public class ProceduralRecoil : MonoBehaviour
{
    [Header("Recoil Settings")]
    [SerializeField] private float hipfireRecoilVertical;
    [SerializeField] private float hipfireRecoilHorizontal;
    [SerializeField] private float aimRecoilVertical;
    [SerializeField] private float aimRecoilHorizontal;

    [Header("Recoil Smoothness")]
    [SerializeField] private float returnSpeed;
    [SerializeField] private float snappiness;
    [SerializeField] private float shakeMagnitude;
    [SerializeField] private float maxShakeDuration;

    [Header("Random Range Settings")]
    [SerializeField] private float randomRecoilVerticalMultiplier = 0.9f;
    [SerializeField] private float randomRecoilHorizontalMultiplier = 0.9f;

    [Header("References")]
    [SerializeField] private Transform orientationTransform;

    private Vector2 targetRecoil;
    private Vector2 currentRecoil;
    private Vector3 originalCameraPosition;
    private float shakeTimer = 0f;

    private void Start()
    {
        if (orientationTransform == null)
        {
            Debug.LogError("Orientation Transform not assigned!");
        }

        originalCameraPosition = orientationTransform.localPosition;
    }

    private void Update()
    {
        currentRecoil = Vector2.Lerp(currentRecoil, targetRecoil, Time.deltaTime * snappiness);

        Vector3 currentRotation = orientationTransform.localRotation.eulerAngles;
        float recoilX = -currentRecoil.x;
        float recoilY = currentRecoil.y;

        orientationTransform.localRotation = Quaternion.Euler(recoilX, currentRotation.y + recoilY, 0f);
        targetRecoil = Vector2.Lerp(targetRecoil, Vector2.zero, Time.deltaTime * returnSpeed);

        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
        }

        ApplyCameraShake();
    }

    public void FireRecoil()
    {
        {
            targetRecoil += new Vector2(
                Random.Range(hipfireRecoilVertical * randomRecoilVerticalMultiplier, hipfireRecoilVertical),
                Random.Range(-hipfireRecoilHorizontal, hipfireRecoilHorizontal)
            );
        }

        shakeTimer = maxShakeDuration;
    }

    private void ApplyCameraShake()
    {
        if (shakeTimer > 0f)
        {
            float shakeAmount = Mathf.Lerp(shakeMagnitude, 0f, 1 - (shakeTimer / maxShakeDuration));
            Vector3 shakeOffset = new Vector3(
                Random.Range(-shakeAmount, shakeAmount),
                Random.Range(-shakeAmount, shakeAmount),
                0);

            orientationTransform.localPosition = Vector3.Lerp(orientationTransform.localPosition, originalCameraPosition + shakeOffset, Time.deltaTime * returnSpeed);
        }
        else
        {
            orientationTransform.localPosition = Vector3.Lerp(orientationTransform.localPosition, originalCameraPosition, Time.deltaTime * returnSpeed);
        }
    }
}

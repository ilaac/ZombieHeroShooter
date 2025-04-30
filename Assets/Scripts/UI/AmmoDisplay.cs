using UnityEngine;
using TMPro; // Use TextMeshPro namespace

public class AmmoDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI currentAmmoText;
    public TextMeshProUGUI spareAmmoText;

    [Header("Gun Reference")]
    public Gun currentGun;

    void Start()
    {
        if (currentGun == null)
        {
            
        }
    }

    void Update()
    {
        // Check if currentGun is assigned and update the UI text
        if (currentGun != null)
        {
            currentAmmoText.text = currentGun.currentAmmo.ToString();
            spareAmmoText.text = currentGun.spareAmmo.ToString();
        }
    }

    // Method to switch to a new gun
    public void SwitchGun(Gun newGun)
    {
        currentGun = newGun;
    }
}
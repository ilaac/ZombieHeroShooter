using UnityEngine;

public class Recoil : MonoBehaviour
{
    private ProceduralRecoil recoilScript;

    void Start()
    {
        // Get the ProceduralRecoil script from the parent object
        recoilScript = GetComponentInParent<ProceduralRecoil>();

        if (recoilScript == null)
        {
            Debug.LogError("ProceduralRecoil script not found.");
        }
    }
    

    public void RecoilFire()
    {
        // Fire recoil based on whether we are aiming or not
        if (recoilScript != null)
        {
            recoilScript.FireRecoil();
        }
    }
}
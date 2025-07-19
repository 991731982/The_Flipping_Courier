using UnityEngine;
using TMPro; 

public class ShootingMechanic : MonoBehaviour
{
    [Header("Ammo Settings")]
    public int maxAmmo = 99;            
    private int currentAmmo;            

    [Header("UI Settings")]
    public TextMeshProUGUI ammoText;    

    void Start()
    {
        currentAmmo = 10; 
        UpdateAmmoText(); 
    }

    public void AddAmmo(int amount)
    {
        int previousAmmo = currentAmmo; 
        currentAmmo += amount;
        currentAmmo = Mathf.Clamp(currentAmmo, 0, maxAmmo); 

        Debug.Log($"Ammo changed: {previousAmmo} -> {currentAmmo} (Max: {maxAmmo})");
        UpdateAmmoText(); 
    }

    private void UpdateAmmoText()
    {
        if (ammoText != null)
        {
            ammoText.text = $"{currentAmmo}/{maxAmmo}";
        }
        else
        {
            Debug.LogWarning("Ammo Text (TMP) is not assigned!");
        }
    }
}

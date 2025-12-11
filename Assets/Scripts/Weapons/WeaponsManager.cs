using UnityEngine;

public class WeaponsManager : MonoBehaviour
{
    public Transform weaponHolder;
    public PlayerAttack playerAttack;

    [Header("Default Weapon to Toggle")]
    public Weapon defaultWeaponPrefab; // assign in Inspector

    private Weapon currentWeapon;

    void Start()
    {
        if (defaultWeaponPrefab != null)
        {
            // Instantiate once and keep it disabled
            currentWeapon = Instantiate(defaultWeaponPrefab, weaponHolder);
            currentWeapon.gameObject.SetActive(false);
            playerAttack.EquipWeapon(null); // no weapon equipped initially
        }
    }

    // Toggle equip/unequip
    public void ToggleWeapon()
    {
        if (currentWeapon == null) return;

        if (!currentWeapon.gameObject.activeSelf)
        {
            currentWeapon.Equip();
            playerAttack.EquipWeapon(currentWeapon);
        }
        else
        {
            currentWeapon.Unequip();
            playerAttack.UnequipWeapon();
        }
    }
}
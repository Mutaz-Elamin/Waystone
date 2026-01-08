using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Weapon currentWeapon;
    private bool weaponDrawn = false;
    private InventoryManager inventoryManager;
    public void SetWeapon(Weapon weapon)
    {
        currentWeapon = weapon;
        weaponDrawn = false; // start sheathed
    }

    public void ToggleWeaponDraw()
    {
        if (currentWeapon == null) return;

        weaponDrawn = !weaponDrawn;

        if (weaponDrawn)
            currentWeapon.Equip();
        else
            currentWeapon.Unequip();
    }

    public void UnequipWeapon()
    {
        if (currentWeapon == null) return;

        currentWeapon.ResetWeapon();
        currentWeapon.Unequip();
        currentWeapon = null;
        weaponDrawn = false;
    }

    public bool IsWeaponDrawn() => weaponDrawn;

    // ---------- ATTACKS ----------
    public void LightAttack()
    {
        inventoryManager = GetComponent<InventoryManager>();
        if (inventoryManager != null && inventoryManager.IsOpen) return;
        if (!weaponDrawn) return;
        currentWeapon?.LightAttack();
    }

    public void StopLightAttack()
    {
        inventoryManager = GetComponent<InventoryManager>();
        if (inventoryManager != null && inventoryManager.IsOpen) return;
        if (!weaponDrawn) return;
        currentWeapon?.StopLightAttack();
    }

    public void HeavyAttack()
    {
        inventoryManager = GetComponent<InventoryManager>();
        if (inventoryManager != null && inventoryManager.IsOpen) return;
        if (!weaponDrawn) return;
        currentWeapon?.HeavyAttack();
    }

    public void StartHeavyCharge()
    {
        inventoryManager = GetComponent<InventoryManager>();
        if (inventoryManager != null && inventoryManager.IsOpen) return;
        if (!weaponDrawn) return;
        currentWeapon?.StartHeavyCharge();
    }

    public void ReleaseHeavyAttack()
    {
        inventoryManager = GetComponent<InventoryManager>();
        if (inventoryManager != null && inventoryManager.IsOpen) return;
        if (!weaponDrawn) return;
        currentWeapon?.ReleaseHeavyAttack();
    }

    public void StartDefend()
    {
        inventoryManager = GetComponent<InventoryManager>();
        if (inventoryManager != null && inventoryManager.IsOpen) return;
        if (!weaponDrawn) return;
        currentWeapon?.StartDefend();
    }

    public void StopDefend()
    {
        inventoryManager = GetComponent<InventoryManager>();
        if (inventoryManager != null && inventoryManager.IsOpen) return;
        if (!weaponDrawn) return;
        currentWeapon?.StopDefend();
    }
}
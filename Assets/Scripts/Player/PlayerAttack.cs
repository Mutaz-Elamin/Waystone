using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Weapon currentWeapon;

    public void LightAttack()
    {
        currentWeapon?.LightAttack();
    }

    public void StopLightAttack()
    {
        currentWeapon?.StopLightAttack();
    }

    public void HeavyAttack()
    {
        currentWeapon?.HeavyAttack();
    }

    public void StartHeavyCharge()
    {
        currentWeapon?.StartHeavyCharge();
    }

    public void ReleaseHeavyAttack()
    {
        currentWeapon?.ReleaseHeavyAttack();
    }
    public void StartDefend() => currentWeapon?.StartDefend();
    public void StopDefend() => currentWeapon?.StopDefend();
    public void EquipWeapon(Weapon weapon)
    {
        currentWeapon = weapon;
        currentWeapon.Equip();
    }

    public void UnequipWeapon()
    {
        if (currentWeapon != null)
        {
            currentWeapon.Unequip();
            currentWeapon = null;
        }
    }
}
using UnityEngine;

public class WeaponsManager : MonoBehaviour
{
    [Header("Setup")]
    public Transform weaponHolder;        // where weapons spawn
    public PlayerAttack playerAttack;     // reference to player attack script

    [Header("Test Weapon Prefabs")]
    public Weapon stickPrefab;            // example test weapon

    private Weapon currentWeapon;


    public void SpawnTestWeapon(Weapon weaponPrefab)
    {
        // Remove old weapon
        if (currentWeapon != null)
        {
            playerAttack.UnequipWeapon();
            Destroy(currentWeapon.gameObject);
        }
        if (weaponPrefab == null)
        {
            playerAttack.UnequipWeapon();

            Destroy(currentWeapon.gameObject);
            return;
        }

        // Spawn new weapon
        currentWeapon = Instantiate(weaponPrefab, weaponHolder);
        string wName = currentWeapon.name;

        if (wName.Contains("Pickaxe") || wName.Contains("Spear") || wName.Contains("Club"))
        {
            currentWeapon.transform.localPosition += new Vector3(-1f, 0f, 0f);
        }

        

        currentWeapon.Equip();            
        playerAttack.SetWeapon(currentWeapon);
        playerAttack.ToggleWeaponDraw();
    }
    public void RemoveCurrentWeapon()
    {
        if (currentWeapon != null)
        {
            playerAttack.UnequipWeapon();
            Destroy(currentWeapon.gameObject);
            currentWeapon = null;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Inventory/Weapons")]
public class WeaponItems : ItemClass
{
    public GameObject weaponPrefab;
    private WeaponsManager weaponsManager;

    // Start is called before the first frame update
    public override ItemClass GetItem() {
        return this;
            }
    public override void UseItem(GameObject user) {
        return;

    }
    public override void Equip(GameObject user) {
        weaponsManager = user.GetComponent<WeaponsManager>();
        Weapon a = weaponPrefab.GetComponent<Weapon>();
        weaponsManager.SpawnTestWeapon(a);
    }
}

using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public string weaponName;
    public Animator animator;
    public Collider attackCollider;

    [Header("Damage Settings")]
    public float baseDamage = 1.0f;
    public MaterialType materialType = MaterialType.Stone;
    public float heavyAttackMultiplier = 1.5f;

    // Compute damage for light or heavy attack
    public virtual int CalculateDamage(bool isHeavy = false)
    {
        float tier = WeaponBalance.GetMultiplier(materialType);
        float dmg = baseDamage * tier * (isHeavy ? heavyAttackMultiplier : 1f);
        return Mathf.RoundToInt(dmg);
    }


    public virtual void Equip()
    {
        gameObject.SetActive(true);
        attackCollider.enabled = false;
    }

    public virtual void Unequip()
    {
        attackCollider.enabled = false;
    }


    public virtual void LightAttack() { }          
    public virtual void HeavyAttack() { }         
    public virtual void StartHeavyCharge() { }    
    public virtual void ReleaseHeavyAttack() { }
    public virtual void StartDefend() { }
    public virtual void StopDefend() { }
    //for pickaxe only
    public virtual void StopLightAttack() { }
    public abstract void ResetWeapon();

}
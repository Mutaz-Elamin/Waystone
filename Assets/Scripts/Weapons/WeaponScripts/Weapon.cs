using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public string weaponName;
    public Animator animator;
    public Collider attackCollider;


    public virtual void Equip() => gameObject.SetActive(true);
    public virtual void Unequip() => gameObject.SetActive(false);


    public virtual void LightAttack() { }          
    public virtual void HeavyAttack() { }         
    public virtual void StartHeavyCharge() { }    
    public virtual void ReleaseHeavyAttack() { }
    public virtual void StartDefend() { }
    public virtual void StopDefend() { }
}
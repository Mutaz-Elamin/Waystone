using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    [HideInInspector]
    public bool canHit = false;

    [Header("SFX Reference")]
    public WeaponSFX sfx; // assign PlayerSFX in inspector

    private void OnTriggerEnter(Collider other)
    {
        if (!canHit) return;

        if (other.CompareTag("npc"))
        {
            Debug.Log("Sword hit: " + other.gameObject.name);
            other.GetComponent<GeneralNPC>()?.TakeDamage(1, DamageCause.EnemyAttack);

            // Play hit sound depending on the current sword combo step
            Sword sword = GetComponentInParent<Sword>();
            if (sword != null)
            {
                switch (sword.comboStep) // we’ll expose comboStep as a property
                {
                    case 1: sfx?.Sword_Light1HitPlay(); break;
                    case 2: sfx?.Sword_Light2HitPlay(); break;
                    case 3: sfx?.Sword_Light3HitPlay(); break;
                }
            }
            else
            {
                // default heavy hit if we can’t find Sword component
                sfx?.Sword_HeavyHitPlay();
            }
        }

        canHit = false;
    }
}
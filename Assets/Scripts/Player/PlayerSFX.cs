using UnityEngine;

public class WeaponSFX : MonoBehaviour
{
    public AudioSource source;

    [Header("Stick Sounds")]
    public AudioClip[] Stick_Light1Swing;
    public AudioClip[] Stick_HeavySwing;
    public AudioClip[] Stick_LightHit;
    public AudioClip[] Stick_HeavyHit;
    public AudioClip[] Stick_Unsheathe;
    public AudioClip[] Stick_Defend;

    [Header("Sword Sounds")]
    public AudioClip[] Sword_Light1Swing;
    public AudioClip[] Sword_Light2Swing;
    public AudioClip[] Sword_Light3Swing;
    public AudioClip[] Sword_HeavyCharge;
    public AudioClip[] Sword_HeavySwing;
    public AudioClip[] Sword_HeavyHit;
    public AudioClip[] Sword_Light1Hit;
    public AudioClip[] Sword_Light2Hit;
    public AudioClip[] Sword_Light3Hit;
    public AudioClip[] Sword_Unsheathe;
    public AudioClip[] Sword_Defend;

    [Header("Spear Sounds")]
    public AudioClip[] Spear_Light1Swing;
    public AudioClip[] Spear_Light2Swing;
    public AudioClip[] Spear_HeavyCharge;
    public AudioClip[] Spear_HeavySwing;
    public AudioClip[] Spear_HeavyHit;
    public AudioClip[] Spear_Light1Hit;
    public AudioClip[] Spear_Light2Hit;
    public AudioClip[] Spear_Unsheathe;
    public AudioClip[] Spear_Defend;

    [Header("Dagger Sounds")]
    public AudioClip[] Dagger_Light1Swing;
    public AudioClip[] Dagger_Light2Swing;
    public AudioClip[] Dagger_Light3Swing;
    public AudioClip[] Dagger_Light4Swing;
    public AudioClip[] Dagger_Light1Hit;
    public AudioClip[] Dagger_Light2Hit;
    public AudioClip[] Dagger_Light3Hit;
    public AudioClip[] Dagger_Light4Hit;
    public AudioClip[] Dagger_Unsheathe;
    public AudioClip[] Dagger_Defend;

    [Header("Club Sounds")]
    public AudioClip[] Club_Light1Hit;
    public AudioClip[] Club_Light2Hit;
    public AudioClip[] Club_Light3Hit;
    public AudioClip[] Club_LightSwing;
    public AudioClip[] Club_Light2Swing;
    public AudioClip[] Club_Light3Swing;
    public AudioClip[] Club_HeavySwing1;
    public AudioClip[] Club_HeavySwing2;
    public AudioClip[] Club_HeavyHit2;
    public AudioClip[] Club_HeavyHit1;
    public AudioClip[] Club_Unsheathe;
    public AudioClip[] Club_Defend;

    [Header("Axe Sounds")]
    public AudioClip[] Axe_Light1Swing;
    public AudioClip[] Axe_Light2Swing;
    public AudioClip[] Axe_Light3Swing;
    public AudioClip[] Axe_Light1Hit;
    public AudioClip[] Axe_Light2Hit;
    public AudioClip[] Axe_Light3Hit;
    public AudioClip[] Axe_HeavySwing;
    public AudioClip[] Axe_HeavyHit;
    public AudioClip[] Axe_WoodChop;
    public AudioClip[] Axe_Unsheathe;
    public AudioClip[] Axe_Defend;

    [Header("Pickaxe Sounds")]
    public AudioClip[] Pickaxe_Light1;
    public AudioClip[] Pickaxe_HeavySwing;
    public AudioClip[] Pickaxe_HeavyHit;
    public AudioClip[] Pickaxe_StoneHit;
    public AudioClip[] Pickaxe_Unsheathe;
    public AudioClip[] Pickaxe_Defend;

    // --- Helper to play a random clip from any array ---
    private void PlayRandom(AudioClip[] clips, float volume = 1f)
    {
        if (clips == null || clips.Length == 0) return;
        source.PlayOneShot(clips[Random.Range(0, clips.Length)], volume);
    }

    // ----------------- Stick -----------------
    public void Stick_Light1SwingPlay() => PlayRandom(Stick_Light1Swing);
    public void Stick_LightHitPlay() => PlayRandom(Stick_LightHit);
    public void Stick_HeavySwingPlay() => PlayRandom(Stick_HeavySwing);
    public void Stick_HeavyHitPlay() => PlayRandom(Stick_HeavyHit);
    public void Stick_UnsheathePlay() => PlayRandom(Stick_Unsheathe);
    public void Stick_DefendPlay() => PlayRandom(Stick_Defend);

    // ----------------- Sword -----------------
    public void Sword_Light1SwingPlay() => PlayRandom(Sword_Light1Swing);
    public void Sword_Light2SwingPlay() => PlayRandom(Sword_Light2Swing);
    public void Sword_Light3SwingPlay() => PlayRandom(Sword_Light3Swing);
    public void Sword_Light1HitPlay() => PlayRandom(Sword_Light1Hit);
    public void Sword_Light2HitPlay() => PlayRandom(Sword_Light2Hit);
    public void Sword_Light3HitPlay() => PlayRandom(Sword_Light3Hit);
    public void Sword_HeavyChargePlay() => PlayRandom(Sword_HeavyCharge);
    public void Sword_HeavySwingPlay() => PlayRandom(Sword_HeavySwing);
    public void Sword_HeavyHitPlay() => PlayRandom(Sword_HeavyHit);
    public void Sword_UnsheathePlay() => PlayRandom(Sword_Unsheathe);
    public void Sword_DefendPlay() => PlayRandom(Sword_Defend);

    // ----------------- Spear -----------------
    public void Spear_Light1SwingPlay() => PlayRandom(Spear_Light1Swing);
    public void Spear_Light2SwingPlay() => PlayRandom(Spear_Light2Swing);
    public void Spear_Light1HitPlay() => PlayRandom(Spear_Light1Hit);
    public void Spear_Light2HitPlay() => PlayRandom(Spear_Light2Hit);
    public void Spear_HeavyChargePlay() => PlayRandom(Spear_HeavyCharge);
    public void Spear_HeavySwingPlay() => PlayRandom(Spear_HeavySwing);
    public void Spear_HeavyHitPlay() => PlayRandom(Spear_HeavyHit);
    public void Spear_UnsheathePlay() => PlayRandom(Spear_Unsheathe);
    public void Spear_DefendPlay() => PlayRandom(Spear_Defend);

    // ----------------- Dagger -----------------
    public void Dagger_Light1SwingPlay() => PlayRandom(Dagger_Light1Swing);
    public void Dagger_Light2SwingPlay() => PlayRandom(Dagger_Light2Swing);
    public void Dagger_Light3SwingPlay() => PlayRandom(Dagger_Light3Swing);
    public void Dagger_Light4SwingPlay() => PlayRandom(Dagger_Light4Swing);
    public void Dagger_Light1HitPlay() => PlayRandom(Dagger_Light1Hit);
    public void Dagger_Light2HitPlay() => PlayRandom(Dagger_Light2Hit);
    public void Dagger_Light3HitPlay() => PlayRandom(Dagger_Light3Hit);
    public void Dagger_Light4HitPlay() => PlayRandom(Dagger_Light4Hit);
    public void Dagger_UnsheathePlay() => PlayRandom(Dagger_Unsheathe);
    public void Dagger_DefendPlay() => PlayRandom(Dagger_Defend);

    // ----------------- Club -----------------
    public void Club_LightSwingPlay() => PlayRandom(Club_LightSwing);
    public void Club_Light2SwingPlay() => PlayRandom(Club_Light2Swing);
    public void Club_Light3SwingPlay() => PlayRandom(Club_Light3Swing);
    public void Club_Light1HitPlay() => PlayRandom(Club_Light1Hit);
    public void Club_Light2HitPlay() => PlayRandom(Club_Light2Hit);
    public void Club_Light3HitPlay() => PlayRandom(Club_Light3Hit);
    public void Club_HeavySwing1Play() => PlayRandom(Club_HeavySwing1);
    public void Club_HeavySwing2Play() => PlayRandom(Club_HeavySwing2);
    public void Club_HeavyHit1Play() => PlayRandom(Club_HeavyHit1);
    public void Club_HeavyHit2Play() => PlayRandom(Club_HeavyHit2);
    public void Club_UnsheathePlay() => PlayRandom(Club_Unsheathe);
    public void Club_DefendPlay() => PlayRandom(Club_Defend);

    // ----------------- Axe -----------------
    public void Axe_Light1SwingPlay() => PlayRandom(Axe_Light1Swing);
    public void Axe_Light2SwingPlay() => PlayRandom(Axe_Light2Swing);
    public void Axe_Light3SwingPlay() => PlayRandom(Axe_Light3Swing);
    public void Axe_Light1HitPlay() => PlayRandom(Axe_Light1Hit);
    public void Axe_Light2HitPlay() => PlayRandom(Axe_Light2Hit);
    public void Axe_Light3HitPlay() => PlayRandom(Axe_Light3Hit);
    public void Axe_HeavySwingPlay() => PlayRandom(Axe_HeavySwing);
    public void Axe_HeavyHitPlay() => PlayRandom(Axe_HeavyHit);
    public void Axe_WoodChopPlay() => PlayRandom(Axe_WoodChop);
    public void Axe_UnsheathePlay() => PlayRandom(Axe_Unsheathe);
    public void Axe_DefendPlay() => PlayRandom(Axe_Defend);

    // ----------------- Pickaxe -----------------
    public void Pickaxe_Light1Play() => PlayRandom(Pickaxe_Light1);
    public void Pickaxe_HeavySwingPlay() => PlayRandom(Pickaxe_HeavySwing);
    public void Pickaxe_HeavyHitPlay() => PlayRandom(Pickaxe_HeavyHit);
    public void Pickaxe_StoneHitPlay() => PlayRandom(Pickaxe_StoneHit);
    public void Pickaxe_UnsheathePlay() => PlayRandom(Pickaxe_Unsheathe);
    public void Pickaxe_DefendPlay() => PlayRandom(Pickaxe_Defend);
}
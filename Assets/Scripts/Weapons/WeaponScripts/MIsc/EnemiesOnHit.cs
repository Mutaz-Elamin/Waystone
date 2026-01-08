using System.Collections;
using UnityEngine;

public class EnemiesOnHit : MonoBehaviour
{
    /// <summary>
    /// Flashes the renderer firstColor -> secondColor then restores whatever property block existed before.
    /// Uses real-time waits so hitstop (timeScale changes) won't freeze the visual timing.
    /// </summary>
    public IEnumerator FlashEnemy(Renderer renderer, Color firstColor, Color secondColor, float duration)
    {
        if (renderer == null) yield break;

        // Save existing property block (so we can restore it afterwards)
        MaterialPropertyBlock original = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(original);

        // Create a property block to set color; use "_Color" which works for standard/unlit shaders.
        // If your shader uses a different property (e.g. "_BaseColor"), you can add a fallback check.
        MaterialPropertyBlock block = new MaterialPropertyBlock();

        // First color
        block.SetColor("_Color", firstColor);
        renderer.SetPropertyBlock(block);

        // Wait half duration (real-time, not game-time)
        yield return new WaitForSecondsRealtime(duration * 0.5f);

        // Second color
        block.SetColor("_Color", secondColor);
        renderer.SetPropertyBlock(block);

        yield return new WaitForSecondsRealtime(duration * 0.5f);

        // Restore original block (this returns whatever the renderer used before)
        renderer.SetPropertyBlock(original);
    }

    // Keep your ApplyHitStop/ApplyKnockback helpers here (unchanged)
    public IEnumerator HitStopCoroutine(float duration)
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = originalTimeScale;
    }

    public void ApplyHitStop(MonoBehaviour runner, float hitStopDuration = 0.05f)
    {
        if (runner == null) return;
        runner.StartCoroutine(HitStopCoroutine(hitStopDuration));
    }

    // Example knockback - adjust to your NPC movement / rigidbody setup
    public void ApplyKnockback(Collider enemyCollider, Transform source, float force = 1f)
    {
        if (enemyCollider == null || source == null) return;

        Rigidbody rb = enemyCollider.GetComponentInParent<Rigidbody>();
        if (rb == null) return;

        Vector3 dir = (enemyCollider.transform.position - source.position).normalized;
        dir.y = Mathf.Max(dir.y, 0.1f); // small upward component if desired

        rb.AddForce(dir * force, ForceMode.Impulse);
    }
}

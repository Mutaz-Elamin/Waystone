using System.Collections;
using UnityEngine;

public class EnemiesOnHit : MonoBehaviour
{

    public IEnumerator FlashEnemy(Renderer renderer, Color firstColor, Color secondColor, float duration)
    {
        if (renderer == null) yield break;

        MaterialPropertyBlock original = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(original);

        MaterialPropertyBlock block = new MaterialPropertyBlock();

        block.SetColor("_Color", firstColor);

        if (renderer != null) renderer.SetPropertyBlock(block);

        yield return new WaitForSecondsRealtime(duration * 0.5f);


        block.SetColor("_Color", secondColor);
        if (renderer != null) renderer.SetPropertyBlock(block);

        yield return new WaitForSecondsRealtime(duration * 0.5f);

   
        if (renderer != null) renderer.SetPropertyBlock(original);
    }

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


    public void ApplyKnockback(Collider enemyCollider, Transform source, float force = 1f)
    {
        if (enemyCollider == null || source == null) return;

        Rigidbody rb = enemyCollider.GetComponentInParent<Rigidbody>();
        if (rb == null) return;

        Vector3 dir = (enemyCollider.transform.position - source.position).normalized;
        dir.y = Mathf.Max(dir.y, 0.1f); 

        rb.AddForce(dir * force, ForceMode.Impulse);
    }
}

using UnityEngine;

public sealed class ClusterMember : MonoBehaviour
{
    private ClusterController owner;
    private PrefabPool pool;

    public void Initialise(ClusterController owner, PrefabPool pool)
    {
        this.owner = owner;
        this.pool = pool;
    }

    public void Despawn(bool reduceTarget)
    {
        if (pool != null)
            pool.Release(gameObject);
        else
            Destroy(gameObject);
    }

    public void DespawnPermanent()
    {
        if (owner != null)
            owner.ReduceTargetCount(1);

        Despawn(false);
    }
}

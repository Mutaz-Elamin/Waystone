using UnityEngine;
using System.Collections;

public class GenericTreeWind : TerrainAssetScript
{
    [Header("Wind Settings")]
    [SerializeField] protected float maxTiltAngle = 6f;
    [SerializeField] protected float swaySpeed = 1.2f;
    [SerializeField] protected float swaySpeedRandomRange = 1f;
    [SerializeField] protected float gustSpeed = 0.2f;
    [SerializeField] protected Vector3 windAxis = new(1f, 0f, 0.3f);

    protected Quaternion baseRotation;
    protected float speed;

    protected override void Awake()
    {
        baseRotation = transform.rotation;
        speed = swaySpeed + Random.Range(-swaySpeedRandomRange, swaySpeedRandomRange);

        windAxis.Normalize();
    }

    // Method to sway the tree (called regularly by a general parent manager)
    public override void ScriptAction()
    {
        float t = Time.time;

        float angle = Mathf.Sin(t * speed) * maxTiltAngle;
        Quaternion windRotation = Quaternion.AngleAxis(angle, windAxis);
        transform.rotation = windRotation * baseRotation;
    }
}

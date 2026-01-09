using System.Collections.Generic;
using UnityEngine;

public class WolfNPC : EnemyNPC
{
    private Animator WolfAnimator;

    [Header("Colour Shift (HSV)")]
    [SerializeField] private bool colourShiftEnabled = false;
    [SerializeField] private Color targetColour = new Color(0.6f, 0.7f, 1f, 1f);

    [Range(0f, 1f)]
    [SerializeField] private float shiftAmount = 0.5f;

    // Cached renderers + original colours
    private Renderer[] _renderers;
    private Color[] _baseColours;
    private string[] _colourProps;
    private MaterialPropertyBlock _mpb;


    protected override void Awake()
    {
        base.Awake();
        WolfAnimator = GetComponent<Animator>();

        CacheRenderersAndBaseColours();
        ApplyColourShift();
    }

    private void CacheRenderersAndBaseColours()
    {
        _renderers = GetComponentsInChildren<Renderer>(true);
        _baseColours = new Color[_renderers.Length];
        _colourProps = new string[_renderers.Length];
        _mpb = new MaterialPropertyBlock();

        for (int i = 0; i < _renderers.Length; i++)
        {
            var r = _renderers[i];
            var mat = r != null ? r.sharedMaterial : null;

            if (mat == null)
            {
                _colourProps[i] = null;
                _baseColours[i] = Color.white;
                continue;
            }

            else if (mat.HasProperty("_Color")) _colourProps[i] = "_Color";
            else _colourProps[i] = null;

            _baseColours[i] = _colourProps[i] != null ? mat.GetColor(_colourProps[i]) : Color.white;
        }
    }

    private void ApplyColourShift()
    {
        if (_renderers == null || _mpb == null) return;

        for (int i = 0; i < _renderers.Length; i++)
        {
            var r = _renderers[i];
            var prop = _colourProps[i];
            if (r == null || string.IsNullOrEmpty(prop)) continue;

            var from = _baseColours[i];
            var to = colourShiftEnabled ? Color.Lerp(from, targetColour, shiftAmount) : from;


            r.GetPropertyBlock(_mpb);
            _mpb.SetColor(prop, to);
            r.SetPropertyBlock(_mpb);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            CacheRenderersAndBaseColours();
        }
        ApplyColourShift();
    }
#endif

    public void SetColourShift(bool enabled)
    {
        colourShiftEnabled = enabled;
        ApplyColourShift();
    }

    protected override void WanderMovementScript()
    {
        WolfAnimator.SetBool("Chasing", false);
        if (!desPointSet) WolfAnimator.SetBool("Walking", false);
        else WolfAnimator.SetBool("Walking", true);

        base.WanderMovementScript();
    }

    protected override void ChasingMovementScript()
    {
        WolfAnimator.SetBool("Chasing", true);
        WolfAnimator.SetBool("Attacking", false);
        base.ChasingMovementScript();
    }

    protected override void AttackingMovementScript()
    {
        WolfAnimator.SetBool("Chasing", false);
        WolfAnimator.SetBool("Walking", false);
        WolfAnimator.SetBool("Attacking", true);
        base.AttackingMovementScript();
    }

    protected override void SelectAttack()
    {
        float dist = Vector3.Distance(transform.position, player.transform.position);
        if (dist > attacks[1].attackRangeMin && (Time.time - attacks[1].lastAttackTime > attacks[1].attackCooldown))
        {
            currentAttack = attacks[1];
            currentAttackRange = attackRanges[1];
        }
        else
        {
            currentAttack = attacks[0];
            currentAttackRange = attackRanges[0];
        }
    }
}

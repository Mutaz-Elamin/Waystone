using UnityEngine;

public class ChickenNPC : PassiveNPC
{
    private Animator ChickenAnimator;

    [Header("Colour Shift (HSV)")]
    [SerializeField] private bool colourShiftEnabled = false;
    [SerializeField] private Color targetColour = Color.white;

    [Range(0f, 1f)]
    [SerializeField] private float shiftAmount = 0.5f;

    [SerializeField] private bool shiftHue = true;
    [SerializeField] private bool shiftSaturation = true;
    [SerializeField] private bool shiftValue = true;

    private Renderer[] _renderers;
    private Color[] _baseColours;
    private string[] _colourProps;
    private MaterialPropertyBlock _mpb;

    protected override void Awake()
    {
        base.Awake();
        ChickenAnimator = GetComponent<Animator>();

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

    private Color ShiftTowardsTargetHSV(Color baseCol, Color targetCol, float t)
    {
        Color.RGBToHSV(baseCol, out float h0, out float s0, out float v0);
        Color.RGBToHSV(targetCol, out float h1, out float s1, out float v1);

        float hLerp = Mathf.LerpAngle(h0 * 360f, h1 * 360f, t) / 360f;
        float h = shiftHue ? hLerp : h0;
        float s = shiftSaturation ? Mathf.Lerp(s0, s1, t) : s0;
        float v = shiftValue ? Mathf.Lerp(v0, v1, t) : v0;

        Color outCol = Color.HSVToRGB(Mathf.Repeat(h, 1f), Mathf.Clamp01(s), Mathf.Clamp01(v));
        outCol.a = Mathf.Lerp(baseCol.a, targetCol.a, t);
        return outCol;
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
            var to = colourShiftEnabled ? ShiftTowardsTargetHSV(from, targetColour, shiftAmount) : from;

            r.GetPropertyBlock(_mpb);
            _mpb.SetColor(prop, to);
            r.SetPropertyBlock(_mpb);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
            CacheRenderersAndBaseColours();

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
        if (!desPointSet)
        {
            ChickenAnimator.SetBool("Walking", false);
        }
        else
        {
            ChickenAnimator.SetFloat("MoveSpeed", 1f);
            ChickenAnimator.SetBool("Walking", true);
        }

        base.WanderMovementScript();
    }

    protected override void EscapeMovementScript()
    {
        ChickenAnimator.SetBool("Walking", true);
        ChickenAnimator.SetFloat("MoveSpeed", escapeMoveModifier);
        base.EscapeMovementScript();
    }
}

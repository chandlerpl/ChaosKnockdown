using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[DisallowMultipleComponent]
public class HighlightControl : MonoBehaviour
{
    private Material _hInstance, _hMInstance;
    [SerializeField] private Color _color;
    [SerializeField] private float _thickness;
    [SerializeField] private bool _blinking;
    private bool _increaseAlpha = true;

    private Renderer _renderer;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _hInstance = new Material(Shader.Find("Custom/Highlight"));
        _hMInstance = new Material(Shader.Find("Custom/Highlight Mask"));

        SetColor(_color);
        SetThickness(_thickness);

        var materialList = _renderer.materials.ToList();
        materialList.Add(_hInstance);
        materialList.Add(_hMInstance);
        _renderer.materials = materialList.ToArray();
    }

    public void SetColor(Color color)
    {
        _hInstance.SetColor("_HighlightColor",color);
    }

    public void SetThickness(float thickness)
    {
        _hInstance.SetFloat("_HighlightThickness", thickness);
    }

    public void SetBlink(bool flag, float targetAlpha = 1)
    {
        _blinking = flag;
        Color currentColor = _hInstance.GetColor("_HighlightColor");
        _hInstance.SetColor("_HighlightColor", new Color(currentColor.r, currentColor.g, currentColor.b, Mathf.Clamp(targetAlpha,0,1)));
    }

    public void EnableHighlight(bool flag)
    {
        Color currentColor = _hInstance.GetColor("_HighlightColor");
        if (flag)
        {
            _hInstance.SetColor("_HighlightColor", new Color(currentColor.r, currentColor.g, currentColor.b, 1));
        }
        else
        {
            _hInstance.SetColor("_HighlightColor", new Color(currentColor.r, currentColor.g, currentColor.b, 0));
        }
    }

    void Update()
    {
        if (_blinking)
        {
            Color currentColor = _hInstance.GetColor("_HighlightColor");
            float alpha = currentColor.a;

            alpha += Time.deltaTime * (_increaseAlpha ? 1 : -1);

            if (alpha > 1.0f)
            {
                alpha = 1.0f;
                _increaseAlpha = false;
            }
            else if (alpha < 0)
            {
                alpha = 0;
                _increaseAlpha = true;
            }

            currentColor.a = alpha;
            _hInstance.SetColor("_HighlightColor", currentColor);
        }
    }
}

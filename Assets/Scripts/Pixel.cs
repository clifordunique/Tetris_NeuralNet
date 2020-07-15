using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Pixel
{
    // GameObject
    private GameObject _Object;

    public static float _Width;
    public static float _Height;

    public bool _Solid = false;
    public Color _SolidColor = Color.white;

    public void UpdateColor()
    {
        _Object.GetComponent<Image>().color = _SolidColor;
    }

    public Pixel(float x, float y)
    {
        _Object = (GameObject)MonoBehaviour.Instantiate(GameplayManager.GetInstance()._PixelPrefab);
        _Object.transform.position = new Vector3(x * _Width + (_Width * 0.5f), y * _Height + (_Height * 0.5f), 0);
        //_Object.transform.localScale = Vector3.one;
        // Set Parent
        _Object.gameObject.transform.SetParent(GameplayManager.GetInstance()._GameCanvas.gameObject.transform);

        // Set Color Randomly at start
        _SolidColor = UnityEngine.Random.ColorHSV();
        UpdateColor();
    }
}

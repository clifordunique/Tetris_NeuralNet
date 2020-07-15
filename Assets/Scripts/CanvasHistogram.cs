using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class HistogramSquare
{
    public HistogramSquare(float value, GameObject gameobject)
    {
        _Value = value;
        _GameObject = gameobject;
    }
    public void Delete()
    {
        MonoBehaviour.Destroy(_GameObject);
    }

    public float _Value;
    public GameObject _GameObject;
}

public class CanvasHistogram : MonoBehaviour
{
    // Maximum values the histogram can hold
    public int _MaxCount = 100;
    public Canvas _CanvasParent;
    public GameObject _SquarePrefab;

    [Header("Turn off for faster CPU")]
    public bool _HideObjects = false;

    private float   InitHeight;
    private float   InitWidth;
    private Vector3 InitPosition;

    private RectTransform rt;

    private float MinValue = 0;
    private float MaxValue = 0;
    public float GetMaxValue()
    {
        return MaxValue;
    }

    private bool FirstValue = true;

    private Color _Color;

    private List<HistogramSquare> _Squares = new List<HistogramSquare>();

    private void Awake()
    {
        if (_SquarePrefab == null)
            Debug.LogError("Missing Square Prefab");

        rt = GetComponent<RectTransform>();

        InitHeight = rt.rect.height;
        InitWidth = rt.rect.width;
        InitPosition = rt.position;

        _Color = GetComponent<Image>().color;

        GetComponent<Image>().color *= 0.3f;
    }

    public void AddValue(float value)
    {
        if(FirstValue)
        {
            MinValue = value;
            MaxValue = value;
            FirstValue = false;
        }

        // Create GameObject
        GameObject New = (GameObject)MonoBehaviour.Instantiate(_SquarePrefab);
        New.GetComponent<Image>().color = _Color;
        New.transform.SetParent(gameObject.transform);

        // Add to list
        HistogramSquare HQ = new HistogramSquare(value, New);
        _Squares.Add(HQ);
        
        // Limit size
        if (_Squares.Count > _MaxCount)
        {
            _Squares[0].Delete();
            _Squares.RemoveAt(0);
        }
        
        // Update min max
        MinValue = Mathf.Min(MinValue, value);
        MaxValue = Mathf.Max(MaxValue, value);

        // Hide/Show Blocks
        if(_HideObjects)
        {
            for (int i = 0; i < _Squares.Count; i++)
            {
                _Squares[i]._GameObject.SetActive(false);
            }
            return;
        }
        else
        {
            for (int i = 0; i < _Squares.Count; i++)
            {
                _Squares[i]._GameObject.SetActive(true);
            }
        }

        // Scale Y
        float h = (InitHeight / (float)_Squares.Count);

        // Update All Squares
        for (int i = 0; i < _Squares.Count; i++)
        {
            // Update Size
            float w_clamp = Mathf.InverseLerp(MinValue, MaxValue, _Squares[i]._Value);
            //if (value < 0)
            //{
            //    Debug.Log("Min: " + MinValue);
            //    Debug.Log("Max: " + MaxValue);
            //    Debug.Log("Value: " + _Squares[i]._Value);
            //    Debug.Log("CLAMP: " + w_clamp);
            //    Debug.Break();
            //}
            float w = (w_clamp) * InitWidth;
            _Squares[i]._GameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);

            // Update Color
            Color C = Color.Lerp(Color.white, _Color, w_clamp);
            
            // Multiply Color By Height for darkness effect :)
            float darken = (float)i / (float)_Squares.Count;
            C *= Mathf.Sqrt(darken);

            _Squares[i]._GameObject.GetComponent<Image>().color = C;

            // Update Position
            Vector3 Position = Vector3.zero;
            Position.x = InitPosition.x;
            Position.y = InitPosition.y - InitHeight * 0.5f + (h * i) + (h * 0.5f);
            _Squares[i]._GameObject.transform.position = Position;
        }

    }

}

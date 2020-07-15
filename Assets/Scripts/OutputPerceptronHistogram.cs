using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class OutputPerceptronHistogram : MonoBehaviour
{
    // Line Reference
    public GameObject _ReferenceLineRenderer;
    private LineRenderer _lineRenderer;

    // Active
    public bool _Active = true;

    // Vertices per line
    public int _VerticesPerLine = 20;

    // Toggle State
    public void ToggleActiveLines(int index)
    {
        bool PrevActive = _Lines[index].activeSelf;
        _Lines[index].SetActive(!_Lines[index].activeSelf);

        if (!PrevActive && _Lines[index].activeSelf)
            _OutputNameText[index].color *= 2.0f;
        else if (PrevActive && !_Lines[index].activeSelf)
            _OutputNameText[index].color *= 0.5f;
    }
    public void SetActiveLines(int index, bool state)
    {
        bool PrevActive = _Lines[index].activeSelf;
        _Lines[index].SetActive(state);

        if (!PrevActive && _Lines[index].activeSelf)
            _OutputNameText[index].color *= 2.0f;
        else if (PrevActive && !_Lines[index].activeSelf)
            _OutputNameText[index].color *= 0.5f;
    }

    public void ToggleAllActiveLines()
    {
        bool OneOn = false;
        for(int i = 0; i < 7; i++)
        {
            if (_Lines[i].activeSelf)
                OneOn = true;
        }
        for (int i = 0; i < 7; i++)
        {
            SetActiveLines(i, !OneOn);
        }
    }

    // Colors per output
    private static readonly Color[] _OutputColors =
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.cyan,
        Color.yellow,
        Color.magenta,
        Color.gray
    };

    // All Lines
    public Text[] _OutputNameText = new Text[7];
    private GameObject[] _Lines = new GameObject[7];

    // Min Point
    private Vector2 _MinPoint = Vector2.zero;
    private Vector2 _MaxPoint = Vector2.zero;

    // Use this for initialization
    void Awake()
    {
        if (_OutputNameText == null)
            Debug.LogError("Outputnametext is null");
        if (_OutputNameText.Length != 7)
            Debug.LogError("Outputnametext isn't 7 objects");

        if (_ReferenceLineRenderer == null)
            Debug.LogError("Missing Reference Line Renderer");
        if (_ReferenceLineRenderer.GetComponent<LineRenderer>() == false)
            Debug.LogError("A");

        _lineRenderer = _ReferenceLineRenderer.GetComponent<LineRenderer>();

        if (_lineRenderer.positionCount != 2)
            Debug.LogError("Reference Line Renderer must have only 2 points");

        _MinPoint = _lineRenderer.GetPosition(0);
        _MaxPoint = _lineRenderer.GetPosition(1);

        for(int i = 0; i < (int)Tetris.Move.TOTAL; i++)
        {
            GameObject GL = (GameObject)MonoBehaviour.Instantiate(_ReferenceLineRenderer, transform);
            _Lines[i] = GL;
            _Lines[i].GetComponent<LineRenderer>().startColor = _OutputColors[i];
            _Lines[i].GetComponent<LineRenderer>().endColor = _OutputColors[i];
            _Lines[i].GetComponent<LineRenderer>().positionCount = _VerticesPerLine;
            //_Lines[i].GetComponent<LineRenderer>().SetPosition(0, _MinPoint);

            //  _Lines[i].GetComponent<LineRenderer>().positionCount = _VerticesPerLine;
            for(int v = 0; v < _VerticesPerLine; v++)
            {
                //_Lines[i].GetComponent<LineRenderer>().SetPosition(v, Vector3.Lerp(_MinPoint, _MaxPoint, (float)v/(float)_VerticesPerLine));
                float t = (float)v / (float)(_VerticesPerLine - 1.0f);
                _Lines[i].GetComponent<LineRenderer>().SetPosition(v, new Vector2(Mathf.Lerp(_MinPoint.x, _MaxPoint.x, t), _MinPoint.y));
            }

            _Lines[i].transform.SetParent(gameObject.transform);
        }

        _ReferenceLineRenderer.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
            ToggleActiveLines(0);

        else if (Input.GetKeyDown(KeyCode.L))
            ToggleActiveLines(1);

        else if (Input.GetKeyDown(KeyCode.R))
            ToggleActiveLines(2);

        else if (Input.GetKeyDown(KeyCode.D))
            ToggleActiveLines(3);

        else if (Input.GetKeyDown(KeyCode.F))
            ToggleActiveLines(4);

        else if (Input.GetKeyDown(KeyCode.C))
            ToggleActiveLines(5);

        else if (Input.GetKeyDown(KeyCode.W))
            ToggleActiveLines(6);

        else if (Input.GetKeyDown(KeyCode.A))
            ToggleAllActiveLines();
    }

    // Height must be from 0 to 1
    public void UpdateOutput(int index, float height)
    {
        height = height * 0.5f + 0.5f;
        for (int v = 0; v < _VerticesPerLine - 1; v++)
        {
            float t = (float)v / (float)_VerticesPerLine;
            float P1Y = _Lines[index].GetComponent<LineRenderer>().GetPosition(v + 1).y;

            _Lines[index].GetComponent<LineRenderer>().SetPosition(v, new Vector2(_Lines[index].GetComponent<LineRenderer>().GetPosition(v).x, P1Y));

        }
        // Update Last Point
        float LastX = _Lines[index].GetComponent<LineRenderer>().GetPosition(_VerticesPerLine - 1).x;
        float h = Mathf.Lerp(_MinPoint.y, _MaxPoint.y, height);
        _Lines[index].GetComponent<LineRenderer>().SetPosition(_VerticesPerLine - 1, new Vector2(LastX, h));
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

[System.Serializable]
public class PauseItem
{
    public enum Type
    {
        RESUME,
        SAVE_EPOCH,
        SAVE_BEST_EPOCH,
        LOAD_EPOCH
    }

    public Type _type;
    public Text _Text;
}


public class PauseMenu : MonoBehaviour
{
    private static PauseMenu instance;
    public static PauseMenu GetInstance()
    {
        if (instance == null) instance = GameObject.FindObjectOfType<PauseMenu>();
        return instance;
    }

    public float _PauseTime = 0;
    public float _PauseStartTime = 0;

    public bool _Active = true;
    public void SetActive(bool state)
    {
        if (!_Active && state)
            _MenuIndex = 0;

        if (state)
            _PauseStartTime = Time.time;
        else
            _PauseTime += Time.time - _PauseStartTime;

        _Active = state;
    }
    public bool getActive()
    {
        return _Active;
    }
    public void ToggleActive()
    {
        SetActive(!getActive());
    }

    private int _MenuIndex = 0;

    public Text _LoadingText;
    public GameObject _Parent;

    [SerializeField]
    public PauseItem[] _Items = new PauseItem[1];

    public PauseItem getItem(int index)
    {
        if (index < 0 || index >= _Items.Length)
            Debug.LogError("!");

        return _Items[index];
    }

    public Image _FancyBar;

    public Color _SelectedColor;
    public Color _UnselectedColor;

    private void Awake()
    {
        _SelectedColor.a = 255.0f;
        _UnselectedColor.a = 255.0f;

        _Active = _Parent.activeSelf;

        _LoadingText.enabled = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            // Can only unpause if not saving
            if (SavingThread.GetInstance().checkThreadRunning() == false)
                ToggleActive();
        }

        if (_Active)
        {
            // Saving Effect
            bool CheckSaving = SavingThread.GetInstance().checkThreadRunning();
            PauseMenu.GetInstance()._LoadingText.enabled = CheckSaving;
            if(CheckSaving)
            {
                PauseMenu.GetInstance()._LoadingText.text = "Loading... (" +
                    SavingThread.m_Saving_Layer.ToString() + "-" +
                    SavingThread.m_Saving_Perceptron.ToString() + "-" +
                    SavingThread.m_Saving_Weight.ToString() + ")";
            }
            // 

            _Parent.SetActive(true);

            Vector3 Position = _FancyBar.GetComponent<RectTransform>().position;
            Position.y = Mathf.Lerp(Position.y, getItem(_MenuIndex)._Text.GetComponent<RectTransform>().position.y, 0.3f);
            _FancyBar.GetComponent<RectTransform>().position = Position;

            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                _MenuIndex++;
                if (_MenuIndex > _Items.Length - 1)
                    _MenuIndex = 0;
            }
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                _MenuIndex--;
                if (_MenuIndex < 0)
                    _MenuIndex = _Items.Length - 1;
            }

            for (int i = 0; i < _Items.Length; i++)
            {
                if (i != _MenuIndex)
                    _Items[i]._Text.color = Color.Lerp(_Items[i]._Text.color, _UnselectedColor, 0.25f);
            }
            _Items[_MenuIndex]._Text.color = Color.Lerp(_Items[_MenuIndex]._Text.color, _SelectedColor, 0.4f);

            // API for File Browser
            // https://github.com/gkngkc/UnityStandaloneFileBrowser
            //
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            {
                switch (_Items[_MenuIndex]._type)
                {
                    case PauseItem.Type.RESUME:
                        {
                            // Can only unpause if not saving
                            if (SavingThread.GetInstance().checkThreadRunning() == false)
                                ToggleActive();
                        }
                        break;

                    case PauseItem.Type.SAVE_EPOCH:
                        {
                            // Get Current Epoch
                            Epoch epoch = TetrisPerceptronManager.GetEpochManager().GetCurrentEpoch();
                            if (epoch == null)
                            {
                                Debug.LogError("No Current Epoch");
                                return;
                            }

                            string Directory = SimpleFileDialog.SelectFile();
                            if (Directory == "")
                            {
                                Debug.LogError("Directory Not Selected");
                                return;
                            }

                            SavingThread.GetInstance().SaveEpoch(epoch, Directory);
                        }
                        break;

                    case PauseItem.Type.SAVE_BEST_EPOCH:
                        {
                            // Get Best Epoch
                            Epoch epoch = TetrisPerceptronManager.GetEpochManager().GetBestEpochAcrossGeneration();
                            if (epoch == null)
                            {
                                Debug.LogError("No Current Epoch");
                                return;
                            }

                            string Directory = SimpleFileDialog.SelectFile();
                            if (Directory == "")
                            {
                                Debug.LogError("Directory Not Selected");
                                return;
                            }

                            SavingThread.GetInstance().SaveEpoch(epoch, Directory);
                        }
                        break;

                    case PauseItem.Type.LOAD_EPOCH:
                        {
                            // Read Data File as String
                            string Data = SimpleFileDialog.OpenFile();
                            if (Data.Length == 0)
                                return;

                            // Read File
                            Epoch Result = EpochParser.LoadFile(Data);
                            if (Result == null)
                                return;

                            // Set To Current Epoch
                            TetrisPerceptronManager.GetEpochManager().OverrideCurrentEpoch(Result);

                            // Reset Game
                            Tetris.GetInstance().ResetGame();
                        }
                        break;
                }
            }
            
        }
        else
        {
            _Parent.SetActive(false);
        }
    }
}

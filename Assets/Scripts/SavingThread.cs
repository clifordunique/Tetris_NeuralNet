using UnityEngine;
using System.Collections;

public class SavingThread
{
    private static SavingThread instance = null;
    public static SavingThread GetInstance()
    {
        if (instance == null) instance = new SavingThread();
        return instance;
    }

    public static int m_Saving_Layer = 0;
    public static int m_Saving_Perceptron = 0;
    public static int m_Saving_Weight = 0;

    private string m_TempDirectory = "";
    private Epoch m_TempEpoch = null;

    private bool m_ThreadRunning = false;
    public bool checkThreadRunning()
    {
        return m_ThreadRunning;
    }
    private System.Threading.Thread m_Thread = null;

    // Saving Epoch Thread
    private void SaveEpochThread()
    {
        // Get String from Epoch Parser
        string Data = EpochParser.SaveFile(m_TempEpoch);

        SimpleFileDialog.WriteFile(m_TempDirectory, Data);

        m_ThreadRunning = false;
    }
    public void SaveEpoch(Epoch epoch, string Directory)
    {
        if (!m_ThreadRunning)
        {
            // Set Temp Data
            m_TempDirectory = Directory;
            m_TempEpoch = epoch;
            // Set Flag
            m_ThreadRunning = true;
            
            // Start Thread
            m_Thread = new System.Threading.Thread(SaveEpochThread);
            m_Thread.Start();
        }
    }
        
}
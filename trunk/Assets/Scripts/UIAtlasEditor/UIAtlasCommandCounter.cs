using UnityEngine;
using System.Collections;
using UnityEditor;

public class UIAtlasCommandCounter : MonoBehaviour
{
    private int m_CommandCounter = 0;

    private int m_preCommandCounter = 0;
    public int CommandCounter { get { return m_CommandCounter; } set { m_CommandCounter = value; } }
    public int PreCommandCounter { get { return m_preCommandCounter; } set { m_preCommandCounter = value; } }

    public bool IsRedo(out int commandCount)
    {
        bool bRet = false;

        if (m_CommandCounter > m_preCommandCounter)
        {
            commandCount = m_CommandCounter - m_preCommandCounter;
            bRet = true;
        }
        else{
            commandCount = m_preCommandCounter - m_CommandCounter;
            bRet = false;
        }

        return bRet;
    }
}
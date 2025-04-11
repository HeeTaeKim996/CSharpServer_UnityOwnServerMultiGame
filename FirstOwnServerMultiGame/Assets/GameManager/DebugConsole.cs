using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugConsole : MonoBehaviour
{
    private List<string> logStrings = new List<string>();
    [SerializeField]
    private Text debugText;


    private void Awake()
    {
        Application.logMessageReceived += OnLogMessage;
    }

    private void OnLogMessage(string logString, string stackTrace, LogType logType)
    {
        logStrings.Add(logString);
        StartCoroutine(DeleteAfterTime(logString));

        if(logStrings.Count > 10)
        {
            logStrings.RemoveAt(0);
        }

        debugText.text = string.Join('\n', logStrings.ToArray());
    }


    private IEnumerator DeleteAfterTime(string newString)
    {
        yield return new WaitForSeconds(5f);
        if (logStrings.Contains(newString))
        {
            logStrings.Remove(newString);
            debugText.text = string.Join('\n', logStrings.ToArray());
        }
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= OnLogMessage;
    }
}

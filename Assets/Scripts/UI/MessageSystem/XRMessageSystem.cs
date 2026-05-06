using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class XRMessageSystem : MonoBehaviour
{
    [SerializeField]
    float messageLifeTime;
    public static float MessageLifeTime { get; private set; }

    [SerializeField]
    GameObject messagePrefab;

    [SerializeField]
    Color warningColor;

    static XRMessageSystem instance;

    List<GameObject> messages = new List<GameObject>();

    void Start()
    {
        instance = this;
        MessageLifeTime = messageLifeTime;
    }

    void MoveMessages()
    {
        if (messages.Count > 4)
        {
            if (messages[0] == null)
                messages.RemoveAt(0);
            else
            {
                Destroy(messages[0]);
                messages.RemoveAt(0);
            }
        }
        
        for (int i = 0; i < messages.Count; ++i)
        {
            if (messages[i] == null)
            {
                messages.RemoveAt(i);
                continue;
            }
            messages[i].transform.localPosition = messages[i].transform.localPosition + new Vector3(0, 25, 0);
        }
    }

    void CreateMessageObj(string message, bool warning = false)
    {
        if (message == null || message == "") return;

        MoveMessages();

        var messageObj = Instantiate(messagePrefab, transform);
        messageObj.GetComponent<TextMeshProUGUI>().text = message;

        if (warning)
            messageObj.GetComponent<TextMeshProUGUI>().faceColor = warningColor;

        messages.Add(messageObj);
    }

    public static void PrintMessage(string message)
    {
        instance.CreateMessageObj(message);
    }

    public static void PrintWarning(string message)
    {
        instance.CreateMessageObj(message, true);
    }
}

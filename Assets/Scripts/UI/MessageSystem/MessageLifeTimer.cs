using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MessageLifeTimer : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI messageText;

    float timer = 0;
    void Update()
    {
        timer += Time.deltaTime;
        float opacity = 1;
        float lifeTime = XRMessageSystem.MessageLifeTime;

        
        if (timer > lifeTime * 0.75f)
        {
            opacity = Mathf.Lerp(1, 0, (timer - lifeTime * 0.75f) / (lifeTime - lifeTime * 0.75f));
        }

        Color col = messageText.faceColor;
        messageText.faceColor = new Color(col.r, col.g, col.b, opacity);

        if (timer > lifeTime)
        {
            Destroy(gameObject);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class UIAlphaPulse_Image : MonoBehaviour
{
    [SerializeField] private RawImage image;

    [SerializeField] private float minAlpha = 0.3f;
    [SerializeField] private float maxAlpha = 1f;
    [SerializeField] private float speed = 1.5f;

    private float time;

    void Update()
    {
        time += Time.deltaTime * speed;

        float t = Mathf.PingPong(time, 1f);

        Color c = image.color;
        c.a = Mathf.Lerp(minAlpha, maxAlpha, t);
        image.color = c;
    }
}
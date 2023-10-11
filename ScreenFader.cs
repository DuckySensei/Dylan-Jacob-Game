using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public Image fadeImage;

    private void Start()
    {   
        //turn on the image
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0f, 0f, 0f, 0f);

    }
    
    public void FadeToColor(Color color, float duration)
    {
        StartCoroutine(FadeToColorCoroutine(color, duration));
    }

    private IEnumerator FadeToColorCoroutine(Color color, float duration)
    {
        float timer = 0f;
        Color startColor = fadeImage.color;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            fadeImage.color = Color.Lerp(startColor, color, t);
            yield return null;
        }
        fadeImage.color = color;
    }
}
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public Image fadeImage;

    private void Start()
    {   
        //set the color to white 
        fadeImage.color = new Color(1f, 1f, 1f, 1f);
        //fade from white to clear
        FadeToColor(Color.clear, 1f);
        clear();
    }

    IEnumerator clear(){
        yield return new WaitForSeconds(1f);
        fadeImage.gameObject.SetActive(false);}
    
    public void FadeToColor(Color color, float duration)
    {
        //turn on screen fader
        fadeImage.gameObject.SetActive(true);
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
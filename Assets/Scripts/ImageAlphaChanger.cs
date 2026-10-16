using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageAlphaChanger : MonoBehaviour
{
    private Image targetImage;
    private float startAlpha = 0f;
    private float endAlpha = 1.0f;
    private float changeDuration = 1.0f;

    private float currentAlpha;
    private float elapsedTime;
    private bool isChanging = false;

    private void Awake()
    {
        targetImage = GetComponent<Image>();
        targetImage.color = new(1, 1, 1, 0);
    }

    private void Start()
    {
        // Set the initial alpha for the target image
        StartCoroutine(Delay());
    }

    IEnumerator Delay()
    {
        yield return new WaitForSeconds(2f);
        SetImageAlpha(startAlpha);
    }

    private void Update()
    {
        if (isChanging)
        {
            elapsedTime += Time.deltaTime;

            // Calculate the new alpha value based on the lerp progress
            currentAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / changeDuration);


            // Apply the new alpha value to the target image
            SetImageAlpha(currentAlpha);

            if (elapsedTime >= changeDuration)
            {
                // Reached the target alpha, stop changing
                isChanging = false;
            }
        }
    }

    private void SetImageAlpha(float alphaValue)
    {
        var color = targetImage.color;
        color.a = alphaValue;
        targetImage.color = color;
    }

    public void StartChangingAlpha()
    {
        // Begin changing the alpha value of the image over time
        currentAlpha = startAlpha;
        elapsedTime = 0f;
        isChanging = true;
    }
}
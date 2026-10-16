using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ObjectEnlarger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float hoverScaleFactor = 1.2f; // Adjust this variable to control the size increase on hover
    private Vector3 originalScale;

    void Start()
    {
        // Save the original scale of the image
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Increase the size on hover
        transform.localScale = originalScale * hoverScaleFactor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Revert to the original size on exit
        transform.localScale = originalScale;
    }
}

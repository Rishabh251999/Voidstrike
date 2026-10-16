using UnityEngine;

public class TooltipFollower : MonoBehaviour
{
    private RectTransform rectTransform;

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        HideTooltip(); // Initially hide the tooltip
    }

    // Update is called once per frame
    void Update()
    {
        // Follow the mouse position. Note: You might want to offset this slightly so it doesn't directly overlap the cursor.
        Vector2 mousePosition = Input.mousePosition;
        rectTransform.position = mousePosition + new Vector2(300, -100); // Adjust (10, 10) to whatever offset you like
    }

    public void ShowTooltip()
    {
        this.gameObject.SetActive(true);
    }

    public void HideTooltip()
    {
        this.gameObject.SetActive(false);
    }
}
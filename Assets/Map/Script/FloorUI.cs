using UnityEngine;
using TMPro;

public class FloorUI : MonoBehaviour
{
    public static FloorUI Instance;
    public TextMeshProUGUI floorText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UpdateFloor(int floor)
    {
        if (floorText != null)
        {
            floorText.text = $"{floor}F";
        }
    }
}

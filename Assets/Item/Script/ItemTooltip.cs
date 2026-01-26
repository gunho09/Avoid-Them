using UnityEngine;
using TMPro;

public class ItemTooltip : MonoBehaviour
{
    public static ItemTooltip Instance;
    public GameObject panel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;

    private void Awake()
    {
        Instance = this;
        HideTooltip();
    }

    private void Update()
    {
        if (panel.activeSelf)
        {
            transform.position = Input.mousePosition;
        }
    }

    public void ShowTooltip(string name, string desc, ItemRarity rarity)
    {
        panel.SetActive(true);
        nameText.text = name;
        descText.text = desc;
        
        // 등급별 색상 (선택 안해도 무방)
        Color nameColor = Color.white;
        if (rarity == ItemRarity.Rare) nameColor = Color.cyan;
        else if (rarity == ItemRarity.Legend) nameColor = Color.yellow;
        
        nameText.color = nameColor;
    }

    public void HideTooltip()
    {
        panel.SetActive(false);
    }
}

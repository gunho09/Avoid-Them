using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    [TextArea]
    public string description;
    public bool isConsumable;

    [Header("아이템 이펙트")]
    public float plushp;
    public float plusspd;
    public float plusspw;
}
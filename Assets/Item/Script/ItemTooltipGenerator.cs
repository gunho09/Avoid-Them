#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class ItemTooltipGenerator
{
    [MenuItem("Tools/Generate Item Tooltip UI")]
    public static void Generate()
    {
        // 1. Find Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("Created new Canvas");
        }

        // 2. Create Manager Object
        GameObject managerObj = new GameObject("ItemTooltip_Manager");
        managerObj.transform.SetParent(canvas.transform, false);
        ItemTooltip tooltipScript = managerObj.AddComponent<ItemTooltip>();

        // 3. Create Visual Panel
        GameObject panelObj = new GameObject("Tooltip_Panel");
        panelObj.transform.SetParent(managerObj.transform, false);
        
        // Add Image (Background)
        Image bg = panelObj.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.8f); // Dark background
        
        // Add Layout Group (Vertical)
        VerticalLayoutGroup layout = panelObj.AddComponent<VerticalLayoutGroup>();
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false; // Important for content fitting
        layout.childForceExpandWidth = true;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.spacing = 5;

        // Add Content Size Fitter
        ContentSizeFitter fitter = panelObj.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // 4. Create Texts
        // Name Text
        GameObject nameObj = new GameObject("NameText");
        nameObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.fontSize = 24;
        nameText.fontStyle = FontStyles.Bold;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = Color.white;
        nameText.text = "Item Name";

        // Desc Text
        GameObject descObj = new GameObject("DescText");
        descObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
        descText.fontSize = 18;
        descText.alignment = TextAlignmentOptions.TopLeft;
        descText.color = new Color(0.9f, 0.9f, 0.9f);
        descText.text = "Item Description...";
        descText.enableWordWrapping = true;

        // 5. Assign References
        tooltipScript.panel = panelObj;
        tooltipScript.nameText = nameText;
        tooltipScript.descText = descText;

        // 6. Set Panel Active (Script execution order issue might require it to be manually set or handled by script)
        // ItemTooltip.Awake() calls HideTooltip(), so we can leave it active or inactive.
        // Let's leave it active so user can see it, script will hide it on play.
        panelObj.SetActive(true);

        Selection.activeGameObject = managerObj;
        Debug.Log("Item Tooltip UI Generated Successfully!");
    }
}
#endif

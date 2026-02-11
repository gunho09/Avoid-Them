using UnityEngine;
using TMPro;
using UnityEngine.UI; // Layout Group 사용을 위해 추가

public class ItemTooltip : MonoBehaviour
{
    public static ItemTooltip Instance;
    public GameObject panel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;

    public Vector2 tooltipOffset = new Vector2(0, 50); // 툴팁 위치 오프셋 (커서 위로 올림)

    private void Awake()
    {
        Instance = this;
        HideTooltip();

        // 캔버스 정렬 순서를 높여서 다른 UI보미 위에 그려지게 함
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
             parentCanvas.sortingOrder = 100; // 높은 값으로 설정
        }

        // ... (layout setup code omitted for brevity) ...
        // 자동 레이아웃 설정 (글씨 겹침 방지)
        if (panel != null)
        {
            // 세로 정렬 그룹 추가 (제목-설명 세로 배치)
            VerticalLayoutGroup layout = panel.GetComponent<VerticalLayoutGroup>();
            if (layout == null) layout = panel.AddComponent<VerticalLayoutGroup>();

            layout.childAlignment = TextAnchor.UpperCenter; // 위쪽 중앙 정렬
            layout.childControlHeight = true; // 자식 높이 제어
            layout.childForceExpandHeight = false; // 높이 강제 확장 금지 (글씨 크기에 맞춤)
            layout.spacing = 10f; // 제목과 설명 사이 간격
            
            // 패널 크기 자동 조절 추가
            ContentSizeFitter fitter = panel.GetComponent<ContentSizeFitter>();
            if (fitter == null) fitter = panel.AddComponent<ContentSizeFitter>();
            
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize; // 가로 크기 자동
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;   // 세로 크기 자동
        }
    }

    private void Update()
    {
        if (panel.activeSelf)
        {
            // 마우스 위치에 오프셋을 더해서 아이템을 가리지 않게 함
            transform.position = Input.mousePosition + (Vector3)tooltipOffset;
            
            // 화면 밖으로 나가지 않게 클램핑 로직 추가 가능 (선택 사항)
        }
    }

    public void ShowTooltip(string name, string desc, ItemRarity rarity)
    {
        panel.SetActive(true);
        // 맨 위에 그리기
        transform.SetAsLastSibling();
        if (panel.transform.parent != transform) panel.transform.SetAsLastSibling();

        // 이름 옆에 등급 표시 (예: "아이템 이름 (Rare)")
        nameText.text = $"{name} <size=70%>({rarity})</size>";
        descText.text = desc;
        
        // 등급별 색상
        Color nameColor = Color.white;
        if (rarity == ItemRarity.Rare) nameColor = Color.cyan;
        else if (rarity == ItemRarity.Legend) nameColor = Color.yellow;
        
        nameText.color = nameColor;

        // 자동 레이아웃 갱신
        LayoutRebuilder.ForceRebuildLayoutImmediate(panel.GetComponent<RectTransform>());
    }

    public void HideTooltip()
    {
        panel.SetActive(false);
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BicepsCurlUI : MonoBehaviour
{
    [Header("Auto Setup")]
    public bool autoCreateUI = true;
    public Canvas parentCanvas;
    
    [Header("UI Position")]
    public bool positionOnRight = true;
    public float rightMargin = 50f;
    
    [Header("UI Prefabs (Optional)")]
    public GameObject textPrefab;
    public GameObject buttonPrefab;
    public GameObject sliderPrefab;
    public GameObject imagePrefab;
    
    private BicepsCurlCounter bicepsCounter;

    void Start()
    {
        bicepsCounter = GetComponent<BicepsCurlCounter>();
        
        if (autoCreateUI)
        {
            CreateBicepsCurlUI();
        }
    }
    
    void CreateBicepsCurlUI()
    {
        if (parentCanvas == null)
        {
            // Create canvas if not assigned
            GameObject canvasObj = new GameObject("BicepsCurlCanvas");
            parentCanvas = canvasObj.AddComponent<Canvas>();
            parentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create main panel positioned on the right
        GameObject mainPanel = CreateUIPanel("MainPanel", parentCanvas.transform);
        
        // Position panel on the right side of screen
        if (positionOnRight)
        {
            RectTransform panelRect = mainPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1f, 0.5f);  // Right side anchor
            panelRect.anchorMax = new Vector2(1f, 0.5f);  // Right side anchor
            panelRect.anchoredPosition = new Vector2(-rightMargin - 200f, 0); // Offset from right edge
        }
        
        // Create title
        GameObject titleObj = CreateText("TitleText", mainPanel.transform, "CONTADOR DE BÍCEPS", 32);
        titleObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 250);
        
        // Create count display
        GameObject countObj = CreateText("CountText", mainPanel.transform, "CONTAGEM: 0", 42);
        countObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 180);
        countObj.GetComponent<TextMeshProUGUI>().color = Color.green;
        bicepsCounter.countText = countObj.GetComponent<TextMeshProUGUI>();
        
        // Create movement status
        GameObject movementObj = CreateText("MovementText", mainPanel.transform, "MOVIMENTO: PRONTO", 20);
        movementObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 120);
        bicepsCounter.movementText = movementObj.GetComponent<TextMeshProUGUI>();
        
        // Create angle displays
        GameObject leftAngleObj = CreateText("LeftAngleText", mainPanel.transform, "Cotovelo Esquerdo: 0°", 16);
        leftAngleObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 60);
        bicepsCounter.leftElbowAngleText = leftAngleObj.GetComponent<TextMeshProUGUI>();
        
        GameObject rightAngleObj = CreateText("RightAngleText", mainPanel.transform, "Cotovelo Direito: 0°", 16);
        rightAngleObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 30);
        bicepsCounter.rightElbowAngleText = rightAngleObj.GetComponent<TextMeshProUGUI>();
        
        // Create sliders (stacked vertically for right panel)
        GameObject leftSliderObj = CreateSlider("LeftSlider", mainPanel.transform);
        leftSliderObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        leftSliderObj.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 20);
        bicepsCounter.leftElbowSlider = leftSliderObj.GetComponent<Slider>();
        
        GameObject rightSliderObj = CreateSlider("RightSlider", mainPanel.transform);
        rightSliderObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -30);
        rightSliderObj.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 20);
        bicepsCounter.rightElbowSlider = rightSliderObj.GetComponent<Slider>();
        
        // Create visual indicators (side by side)
        GameObject leftIndicatorObj = CreateImage("LeftIndicator", mainPanel.transform, Color.gray);
        leftIndicatorObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-50, -70);
        leftIndicatorObj.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);
        bicepsCounter.leftArmIndicator = leftIndicatorObj.GetComponent<Image>();
        
        GameObject rightIndicatorObj = CreateImage("RightIndicator", mainPanel.transform, Color.gray);
        rightIndicatorObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(50, -70);
        rightIndicatorObj.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);
        bicepsCounter.rightArmIndicator = rightIndicatorObj.GetComponent<Image>();
        
        // Create buttons (side by side)
        GameObject resetButtonObj = CreateButton("ResetButton", mainPanel.transform, "RESETAR");
        resetButtonObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-60, -130);
        resetButtonObj.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 35);
        bicepsCounter.resetButton = resetButtonObj.GetComponent<Button>();
        
        GameObject calibrateButtonObj = CreateButton("CalibrateButton", mainPanel.transform, "CALIBRAR");
        calibrateButtonObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(60, -130);
        calibrateButtonObj.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 35);
        bicepsCounter.calibrateButton = calibrateButtonObj.GetComponent<Button>();
        
        // Create instruction text
        GameObject instructionObj = CreateText("InstructionText", mainPanel.transform, "Posicione-se na frente da câmera", 14);
        instructionObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -200);
        instructionObj.GetComponent<RectTransform>().sizeDelta = new Vector2(350, 60);
        bicepsCounter.instructionText = instructionObj.GetComponent<TextMeshProUGUI>();
        
        Debug.Log("Biceps Curl UI created successfully on the right side!");
    }
    
    GameObject CreateUIPanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(400, 600);  // Made narrower for right panel
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0.8f);  // Slightly more opaque
        
        // Add rounded corners effect (optional)
        return panel;
    }
    
    GameObject CreateText(string name, Transform parent, string text, int fontSize)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent);
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(350, 40);  // Adjusted for right panel
        
        TextMeshProUGUI textComp = textObj.AddComponent<TextMeshProUGUI>();
        textComp.text = text;
        textComp.fontSize = fontSize;
        textComp.alignment = TextAlignmentOptions.Center;
        textComp.color = Color.white;
        textComp.fontStyle = FontStyles.Bold;
        
        return textObj;
    }
    
    GameObject CreateButton(string name, Transform parent, string text)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent);
        
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(120, 40);
        
        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.6f, 1f);
        
        Button button = buttonObj.AddComponent<Button>();
        
        // Add button hover effect
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.3f, 0.7f, 1f);
        colors.pressedColor = new Color(0.1f, 0.5f, 0.9f);
        button.colors = colors;
        
        // Create button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        
        TextMeshProUGUI textComp = textObj.AddComponent<TextMeshProUGUI>();
        textComp.text = text;
        textComp.fontSize = 12;
        textComp.alignment = TextAlignmentOptions.Center;
        textComp.color = Color.white;
        textComp.fontStyle = FontStyles.Bold;
        
        return buttonObj;
    }
    
    GameObject CreateSlider(string name, Transform parent)
    {
        GameObject sliderObj = new GameObject(name);
        sliderObj.transform.SetParent(parent);
        
        RectTransform rect = sliderObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300, 20);
        
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 180f;
        
        // Create background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(sliderObj.transform);
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchoredPosition = Vector2.zero;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f);
        
        // Create fill area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform);
        RectTransform fillRect = fillArea.AddComponent<RectTransform>();
        fillRect.anchoredPosition = Vector2.zero;
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform);
        RectTransform fillImageRect = fill.AddComponent<RectTransform>();
        fillImageRect.sizeDelta = Vector2.zero;
        fillImageRect.anchorMin = Vector2.zero;
        fillImageRect.anchorMax = Vector2.one;
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.3f, 0.8f, 0.3f);
        
        // Create handle
        GameObject handleSlideArea = new GameObject("Handle Slide Area");
        handleSlideArea.transform.SetParent(sliderObj.transform);
        RectTransform handleSlideRect = handleSlideArea.AddComponent<RectTransform>();
        handleSlideRect.sizeDelta = new Vector2(-20, 0);
        handleSlideRect.anchorMin = Vector2.zero;
        handleSlideRect.anchorMax = Vector2.one;
        
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleSlideArea.transform);
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 20);
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = Color.white;
        
        // Configure slider
        slider.fillRect = fillImageRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;
        
        return sliderObj;
    }
    
    GameObject CreateImage(string name, Transform parent, Color color)
    {
        GameObject imageObj = new GameObject(name);
        imageObj.transform.SetParent(parent);
        
        RectTransform rect = imageObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(50, 50);
        
        Image image = imageObj.AddComponent<Image>();
        image.color = color;
        
        return imageObj;
    }
    
    // Public method to reposition UI
    public void SetUIPosition(bool onRight, float margin = 50f)
    {
        positionOnRight = onRight;
        rightMargin = margin;
        
        // Find and reposition existing panel
        Transform panel = parentCanvas.transform.Find("MainPanel");
        if (panel != null)
        {
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            
            if (onRight)
            {
                panelRect.anchorMin = new Vector2(1f, 0.5f);
                panelRect.anchorMax = new Vector2(1f, 0.5f);
                panelRect.anchoredPosition = new Vector2(-margin - 200f, 0);
            }
            else
            {
                panelRect.anchorMin = new Vector2(0f, 0.5f);
                panelRect.anchorMax = new Vector2(0f, 0.5f);
                panelRect.anchoredPosition = new Vector2(margin + 200f, 0);
            }
        }
    }
}
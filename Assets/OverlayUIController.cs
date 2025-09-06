using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OverlayUIController : MonoBehaviour
{
    [Header("UI References")]
    public Toggle enableOverlayToggle;
    public Toggle showOriginalCharacterToggle;
    public Toggle showConnectionsToggle;
    public Toggle showLandmarkPointsToggle;
    public Slider overlayAlphaSlider;
    public Slider scaleSlider;
    public Dropdown characterTypeDropdown;
    public Dropdown characterColorDropdown;
    public Button refreshCharacterButton;
    
    [Header("Position Controls")]
    public Slider positionXSlider;
    public Slider positionYSlider;
    public Slider positionZSlider;
    
    [Header("Component References")]
    public CharacterOverlayManager overlayManager;
    public CharacterBodyOverlay bodyOverlay;
    public BodyTracking bodyTracking;
    
    [Header("Info Display")]
    public Text statusText;
    public Text bodyDetectionText;
    public Text confidenceText;
    
    void Start()
    {
        // Find components if not assigned
        if (overlayManager == null)
            overlayManager = FindObjectOfType<CharacterOverlayManager>();
            
        if (bodyTracking == null)
            bodyTracking = FindObjectOfType<BodyTracking>();
        
        SetupUI();
        SetupEventListeners();
    }
    
    void SetupUI()
    {
        // Setup dropdowns
        if (characterTypeDropdown != null)
        {
            characterTypeDropdown.options.Clear();
            characterTypeDropdown.options.Add(new Dropdown.OptionData("Male"));
            characterTypeDropdown.options.Add(new Dropdown.OptionData("Female"));
        }
        
        if (characterColorDropdown != null)
        {
            characterColorDropdown.options.Clear();
            characterColorDropdown.options.Add(new Dropdown.OptionData("Blue"));
            characterColorDropdown.options.Add(new Dropdown.OptionData("Green"));
            characterColorDropdown.options.Add(new Dropdown.OptionData("Orange"));
            characterColorDropdown.options.Add(new Dropdown.OptionData("Purple"));
            characterColorDropdown.options.Add(new Dropdown.OptionData("Red"));
            characterColorDropdown.options.Add(new Dropdown.OptionData("White"));
            characterColorDropdown.options.Add(new Dropdown.OptionData("Yellow"));
        }
        
        // Setup slider ranges
        if (overlayAlphaSlider != null)
        {
            overlayAlphaSlider.minValue = 0f;
            overlayAlphaSlider.maxValue = 1f;
            overlayAlphaSlider.value = 0.7f;
        }
        
        if (scaleSlider != null)
        {
            scaleSlider.minValue = 0.1f;
            scaleSlider.maxValue = 3f;
            scaleSlider.value = 1f;
        }
        
        if (positionXSlider != null)
        {
            positionXSlider.minValue = -5f;
            positionXSlider.maxValue = 5f;
            positionXSlider.value = 0f;
        }
        
        if (positionYSlider != null)
        {
            positionYSlider.minValue = -5f;
            positionYSlider.maxValue = 5f;
            positionYSlider.value = 0f;
        }
        
        if (positionZSlider != null)
        {
            positionZSlider.minValue = -5f;
            positionZSlider.maxValue = 5f;
            positionZSlider.value = 0f;
        }
    }
    
    void SetupEventListeners()
    {
        // Toggle listeners
        if (enableOverlayToggle != null)
            enableOverlayToggle.onValueChanged.AddListener(OnEnableOverlayChanged);
            
        if (showOriginalCharacterToggle != null)
            showOriginalCharacterToggle.onValueChanged.AddListener(OnShowOriginalCharacterChanged);
            
        if (showConnectionsToggle != null)
            showConnectionsToggle.onValueChanged.AddListener(OnShowConnectionsChanged);
            
        if (showLandmarkPointsToggle != null)
            showLandmarkPointsToggle.onValueChanged.AddListener(OnShowLandmarkPointsChanged);
        
        // Slider listeners
        if (overlayAlphaSlider != null)
            overlayAlphaSlider.onValueChanged.AddListener(OnOverlayAlphaChanged);
            
        if (scaleSlider != null)
            scaleSlider.onValueChanged.AddListener(OnScaleChanged);
            
        if (positionXSlider != null)
            positionXSlider.onValueChanged.AddListener(OnPositionXChanged);
            
        if (positionYSlider != null)
            positionYSlider.onValueChanged.AddListener(OnPositionYChanged);
            
        if (positionZSlider != null)
            positionZSlider.onValueChanged.AddListener(OnPositionZChanged);
        
        // Dropdown listeners
        if (characterTypeDropdown != null)
            characterTypeDropdown.onValueChanged.AddListener(OnCharacterTypeChanged);
            
        if (characterColorDropdown != null)
            characterColorDropdown.onValueChanged.AddListener(OnCharacterColorChanged);
        
        // Button listeners
        if (refreshCharacterButton != null)
            refreshCharacterButton.onClick.AddListener(OnRefreshCharacterClicked);
    }
    
    void Update()
    {
        UpdateStatusDisplay();
    }
    
    void UpdateStatusDisplay()
    {
        if (bodyTracking == null) return;
        
        bool bodyDetected = bodyTracking.IsBodyDetected();
        
        if (bodyDetectionText != null)
        {
            bodyDetectionText.text = bodyDetected ? "Body Detected: YES" : "Body Detected: NO";
            bodyDetectionText.color = bodyDetected ? Color.green : Color.red;
        }
        
        if (confidenceText != null)
        {
            var data = bodyTracking.GetCurrentBodyData();
            if (data?.body_tracking?.body_metrics != null)
            {
                float confidence = data.body_tracking.body_metrics.confidence;
                confidenceText.text = $"Confidence: {confidence:F2}";
                confidenceText.color = Color.Lerp(Color.red, Color.green, confidence);
            }
            else
            {
                confidenceText.text = "Confidence: --";
                confidenceText.color = Color.gray;
            }
        }
        
        if (statusText != null)
        {
            string status = "Status: ";
            if (overlayManager != null && overlayManager.enableOverlay)
                status += "Overlay Active";
            else
                status += "Overlay Inactive";
                
            statusText.text = status;
        }
    }
    
    // Event handlers
    void OnEnableOverlayChanged(bool value)
    {
        if (overlayManager != null)
            overlayManager.SetOverlayEnabled(value);
    }
    
    void OnShowOriginalCharacterChanged(bool value)
    {
        if (bodyOverlay != null)
            bodyOverlay.SetShowOriginalCharacter(value);
    }
    
    void OnShowConnectionsChanged(bool value)
    {
        if (overlayManager != null)
            overlayManager.SetShowConnections(value);
            
        if (bodyTracking != null)
            bodyTracking.ToggleConnections(value);
    }
    
    void OnShowLandmarkPointsChanged(bool value)
    {
        if (bodyTracking != null)
        {
            // Toggle visibility of body points
            for (int i = 0; i < 33; i++)
            {
                var bodyPoint = bodyTracking.GetBodyPoint(i);
                if (bodyPoint != null)
                    bodyPoint.SetActive(value);
            }
        }
    }
    
    void OnOverlayAlphaChanged(float value)
    {
        if (overlayManager != null)
            overlayManager.SetOverlayTransparency(value);
    }
    
    void OnScaleChanged(float value)
    {
        if (overlayManager != null)
            overlayManager.SetCharacterScale(value);
    }
    
    void OnPositionXChanged(float value)
    {
        UpdateCharacterPosition();
    }
    
    void OnPositionYChanged(float value)
    {
        UpdateCharacterPosition();
    }
    
    void OnPositionZChanged(float value)
    {
        UpdateCharacterPosition();
    }
    
    void UpdateCharacterPosition()
    {
        if (overlayManager == null) return;
        
        Vector3 position = new Vector3(
            positionXSlider != null ? positionXSlider.value : 0f,
            positionYSlider != null ? positionYSlider.value : 0f,
            positionZSlider != null ? positionZSlider.value : 0f
        );
        
        overlayManager.SetCharacterPosition(position);
    }
    
    void OnCharacterTypeChanged(int value)
    {
        if (overlayManager != null)
            overlayManager.SetCharacterType(value);
    }
    
    void OnCharacterColorChanged(int value)
    {
        if (overlayManager != null)
            overlayManager.SetCharacterColor(value);
    }
    
    void OnRefreshCharacterClicked()
    {
        if (overlayManager != null)
            overlayManager.RefreshCharacter();
            
        // Update bodyOverlay reference
        bodyOverlay = FindObjectOfType<CharacterBodyOverlay>();
    }
    
    // Public methods for external control
    public void ResetToDefaults()
    {
        if (enableOverlayToggle != null)
            enableOverlayToggle.isOn = true;
            
        if (showOriginalCharacterToggle != null)
            showOriginalCharacterToggle.isOn = true;
            
        if (showConnectionsToggle != null)
            showConnectionsToggle.isOn = true;
            
        if (showLandmarkPointsToggle != null)
            showLandmarkPointsToggle.isOn = true;
            
        if (overlayAlphaSlider != null)
            overlayAlphaSlider.value = 0.7f;
            
        if (scaleSlider != null)
            scaleSlider.value = 1f;
            
        if (positionXSlider != null)
            positionXSlider.value = 0f;
            
        if (positionYSlider != null)
            positionYSlider.value = 0f;
            
        if (positionZSlider != null)
            positionZSlider.value = 0f;
            
        if (characterTypeDropdown != null)
            characterTypeDropdown.value = 0;
            
        if (characterColorDropdown != null)
            characterColorDropdown.value = 0;
    }
    
    public void SavePreferences()
    {
        // Save UI preferences to PlayerPrefs
        PlayerPrefs.SetInt("OverlayEnabled", enableOverlayToggle != null && enableOverlayToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("ShowOriginalCharacter", showOriginalCharacterToggle != null && showOriginalCharacterToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("ShowConnections", showConnectionsToggle != null && showConnectionsToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("ShowLandmarkPoints", showLandmarkPointsToggle != null && showLandmarkPointsToggle.isOn ? 1 : 0);
        PlayerPrefs.SetFloat("OverlayAlpha", overlayAlphaSlider != null ? overlayAlphaSlider.value : 0.7f);
        PlayerPrefs.SetFloat("CharacterScale", scaleSlider != null ? scaleSlider.value : 1f);
        PlayerPrefs.SetFloat("PositionX", positionXSlider != null ? positionXSlider.value : 0f);
        PlayerPrefs.SetFloat("PositionY", positionYSlider != null ? positionYSlider.value : 0f);
        PlayerPrefs.SetFloat("PositionZ", positionZSlider != null ? positionZSlider.value : 0f);
        PlayerPrefs.SetInt("CharacterType", characterTypeDropdown != null ? characterTypeDropdown.value : 0);
        PlayerPrefs.SetInt("CharacterColor", characterColorDropdown != null ? characterColorDropdown.value : 0);
        
        PlayerPrefs.Save();
    }
    
    public void LoadPreferences()
    {
        // Load UI preferences from PlayerPrefs
        if (enableOverlayToggle != null)
            enableOverlayToggle.isOn = PlayerPrefs.GetInt("OverlayEnabled", 1) == 1;
            
        if (showOriginalCharacterToggle != null)
            showOriginalCharacterToggle.isOn = PlayerPrefs.GetInt("ShowOriginalCharacter", 1) == 1;
            
        if (showConnectionsToggle != null)
            showConnectionsToggle.isOn = PlayerPrefs.GetInt("ShowConnections", 1) == 1;
            
        if (showLandmarkPointsToggle != null)
            showLandmarkPointsToggle.isOn = PlayerPrefs.GetInt("ShowLandmarkPoints", 1) == 1;
            
        if (overlayAlphaSlider != null)
            overlayAlphaSlider.value = PlayerPrefs.GetFloat("OverlayAlpha", 0.7f);
            
        if (scaleSlider != null)
            scaleSlider.value = PlayerPrefs.GetFloat("CharacterScale", 1f);
            
        if (positionXSlider != null)
            positionXSlider.value = PlayerPrefs.GetFloat("PositionX", 0f);
            
        if (positionYSlider != null)
            positionYSlider.value = PlayerPrefs.GetFloat("PositionY", 0f);
            
        if (positionZSlider != null)
            positionZSlider.value = PlayerPrefs.GetFloat("PositionZ", 0f);
            
        if (characterTypeDropdown != null)
            characterTypeDropdown.value = PlayerPrefs.GetInt("CharacterType", 0);
            
        if (characterColorDropdown != null)
            characterColorDropdown.value = PlayerPrefs.GetInt("CharacterColor", 0);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BicepsCurlCounter : MonoBehaviour
{
    [Header("UDP Connection")]
    public UDPReceive udpReceive;
    
    [Header("UI Elements")]
    public TextMeshProUGUI countText;
    public TextMeshProUGUI movementText;
    public TextMeshProUGUI leftElbowAngleText;
    public TextMeshProUGUI rightElbowAngleText;
    public TextMeshProUGUI instructionText;
    public Slider leftElbowSlider;
    public Slider rightElbowSlider;
    public Button resetButton;
    public Button calibrateButton;
    
    [Header("Exercise Settings")]
    public bool useLeftArm = true;
    public bool useRightArm = true;
    public float minAngleThreshold = 38f;
    public float maxAngleThreshold = 150f;
    public float cooldownTime = 0.5f;
    
    [Header("Visual Feedback")]
    public Image leftArmIndicator;
    public Image rightArmIndicator;
    public Color upColor = Color.green;
    public Color downColor = Color.blue;
    public Color inactiveColor = Color.gray;
    
    [Header("Audio Feedback")]
    public AudioSource audioSource;
    public AudioClip countSound;
    public AudioClip completeSound;
    
    // Private variables
    private int totalCount = 0;
    private int leftArmCount = 0;
    private int rightArmCount = 0;
    
    private string leftDirection = "down";
    private string rightDirection = "down";
    
    private float lastLeftCountTime = 0f;
    private float lastRightCountTime = 0f;
    
    private float currentLeftElbowAngle = 0f;
    private float currentRightElbowAngle = 0f;
    
    private bool isCalibrating = false;
    private float calibrationTime = 5f;
    private float calibrationTimer = 0f;
    
    private List<float> leftAngleSamples = new List<float>();
    private List<float> rightAngleSamples = new List<float>();

    void Start()
    {
        // Find UDPReceive if not assigned
        if (udpReceive == null)
        {
            udpReceive = FindObjectOfType<UDPReceive>();
        }
        
        // Setup UI
        SetupUI();
        
        // Reset counters
        ResetCount();
        
        Debug.Log("Biceps Curl Counter initialized");
    }
    
    void SetupUI()
    {
        // Setup button events
        if (resetButton != null)
            resetButton.onClick.AddListener(ResetCount);
            
        if (calibrateButton != null)
            calibrateButton.onClick.AddListener(StartCalibration);
        
        // Setup sliders
        if (leftElbowSlider != null)
        {
            leftElbowSlider.minValue = 0f;
            leftElbowSlider.maxValue = 180f;
        }
        
        if (rightElbowSlider != null)
        {
            rightElbowSlider.minValue = 0f;
            rightElbowSlider.maxValue = 180f;
        }
        
        // Setup instruction text
        if (instructionText != null)
        {
            instructionText.text = "Posicione-se na frente da câmera e comece os exercícios de bíceps!";
        }
    }
    
    void Update()
    {
        if (udpReceive != null && udpReceive.IsBodyDetected())
        {
            // Get current angles
            currentLeftElbowAngle = udpReceive.GetAngle("left_elbow");
            currentRightElbowAngle = udpReceive.GetAngle("right_elbow");
            
            // Handle calibration
            if (isCalibrating)
            {
                HandleCalibration();
                return;
            }
            
            // Process biceps curl counting
            if (useLeftArm)
                ProcessArmMovement(true, currentLeftElbowAngle);
                
            if (useRightArm)
                ProcessArmMovement(false, currentRightElbowAngle);
            
            // Update UI
            UpdateUI();
        }
        else
        {
            // No body detected
            if (instructionText != null)
            {
                instructionText.text = "Corpo não detectado! Posicione-se na frente da câmera.";
            }
        }
    }
    
    void ProcessArmMovement(bool isLeftArm, float elbowAngle)
    {
        string currentDirection = isLeftArm ? leftDirection : rightDirection;
        float lastCountTime = isLeftArm ? lastLeftCountTime : lastRightCountTime;
        int currentCount = isLeftArm ? leftArmCount : rightArmCount;
        
        // Check if enough time has passed since last count (cooldown)
        if (Time.time - lastCountTime < cooldownTime)
            return;
        
        bool countChanged = false;
        
        // Biceps curl logic - down to up movement
        if (currentDirection == "down" && elbowAngle <= minAngleThreshold)
        {
            // Arm is fully extended (down position)
            if (isLeftArm)
            {
                leftDirection = "up";
                leftArmCount++;
            }
            else
            {
                rightDirection = "up";
                rightArmCount++;
            }
            
            countChanged = true;
            lastCountTime = Time.time;
            
            // Play count sound
            PlayCountSound();
        }
        else if (currentDirection == "up" && elbowAngle >= maxAngleThreshold)
        {
            // Arm is fully contracted (up position)
            if (isLeftArm)
            {
                leftDirection = "down";
                lastLeftCountTime = Time.time;
            }
            else
            {
                rightDirection = "down";
                lastRightCountTime = Time.time;
            }
        }
        
        // Update total count
        if (countChanged)
        {
            totalCount = leftArmCount + rightArmCount;
            
            // Check for milestones
            CheckMilestones();
        }
        
        // Update last count time
        if (isLeftArm)
            lastLeftCountTime = lastCountTime;
        else
            lastRightCountTime = lastCountTime;
    }
    
    void HandleCalibration()
    {
        calibrationTimer += Time.deltaTime;
        
        // Collect angle samples
        leftAngleSamples.Add(currentLeftElbowAngle);
        rightAngleSamples.Add(currentRightElbowAngle);
        
        // Update calibration UI
        if (instructionText != null)
        {
            float remainingTime = calibrationTime - calibrationTimer;
            instructionText.text = $"Calibrando... Mova os braços naturalmente.\nTempo restante: {remainingTime:F1}s";
        }
        
        // Finish calibration
        if (calibrationTimer >= calibrationTime)
        {
            FinishCalibration();
        }
    }
    
    void StartCalibration()
    {
        isCalibrating = true;
        calibrationTimer = 0f;
        leftAngleSamples.Clear();
        rightAngleSamples.Clear();
        
        if (instructionText != null)
        {
            instructionText.text = "Iniciando calibração...";
        }
        
        Debug.Log("Starting calibration");
    }
    
    void FinishCalibration()
    {
        isCalibrating = false;
        
        // Calculate optimal thresholds based on samples
        if (leftAngleSamples.Count > 0)
        {
            float leftMin = Mathf.Min(leftAngleSamples.ToArray());
            float leftMax = Mathf.Max(leftAngleSamples.ToArray());
            
            minAngleThreshold = leftMin + (leftMax - leftMin) * 0.2f;
            maxAngleThreshold = leftMax - (leftMax - leftMin) * 0.2f;
        }
        
        if (instructionText != null)
        {
            instructionText.text = $"Calibração concluída!\nMin: {minAngleThreshold:F1}° Max: {maxAngleThreshold:F1}°";
        }
        
        Debug.Log($"Calibration complete. Min: {minAngleThreshold:F1}°, Max: {maxAngleThreshold:F1}°");
    }
    
    void UpdateUI()
    {
        // Update count display
        if (countText != null)
        {
            countText.text = $"CONTAGEM: {totalCount}";
            countText.color = totalCount > 0 ? Color.green : Color.white;
        }
        
        // Update movement status
        if (movementText != null)
        {
            string status = "";
            if (useLeftArm && useRightArm)
            {
                status = $"Esquerdo: {leftDirection.ToUpper()} | Direito: {rightDirection.ToUpper()}";
            }
            else if (useLeftArm)
            {
                status = $"Braço Esquerdo: {leftDirection.ToUpper()}";
            }
            else if (useRightArm)
            {
                status = $"Braço Direito: {rightDirection.ToUpper()}";
            }
            
            movementText.text = status;
        }
        
        // Update angle displays
        if (leftElbowAngleText != null)
        {
            leftElbowAngleText.text = $"Cotovelo Esquerdo: {currentLeftElbowAngle:F1}°";
        }
        
        if (rightElbowAngleText != null)
        {
            rightElbowAngleText.text = $"Cotovelo Direito: {currentRightElbowAngle:F1}°";
        }
        
        // Update sliders
        if (leftElbowSlider != null)
        {
            leftElbowSlider.value = currentLeftElbowAngle;
        }
        
        if (rightElbowSlider != null)
        {
            rightElbowSlider.value = currentRightElbowAngle;
        }
        
        // Update visual indicators
        UpdateVisualIndicators();
    }
    
    void UpdateVisualIndicators()
    {
        // Left arm indicator
        if (leftArmIndicator != null)
        {
            if (!useLeftArm)
            {
                leftArmIndicator.color = inactiveColor;
            }
            else
            {
                Color targetColor = leftDirection == "up" ? upColor : downColor;
                leftArmIndicator.color = Color.Lerp(leftArmIndicator.color, targetColor, Time.deltaTime * 5f);
            }
        }
        
        // Right arm indicator
        if (rightArmIndicator != null)
        {
            if (!useRightArm)
            {
                rightArmIndicator.color = inactiveColor;
            }
            else
            {
                Color targetColor = rightDirection == "up" ? upColor : downColor;
                rightArmIndicator.color = Color.Lerp(rightArmIndicator.color, targetColor, Time.deltaTime * 5f);
            }
        }
    }
    
    void CheckMilestones()
    {
        // Check for milestone achievements
        if (totalCount > 0 && totalCount % 10 == 0)
        {
            PlayCompleteSound();
            
            if (instructionText != null)
            {
                instructionText.text = $"Parabéns! {totalCount} repetições completadas!";
            }
        }
    }
    
    void PlayCountSound()
    {
        if (audioSource != null && countSound != null)
        {
            audioSource.PlayOneShot(countSound);
        }
    }
    
    void PlayCompleteSound()
    {
        if (audioSource != null && completeSound != null)
        {
            audioSource.PlayOneShot(completeSound);
        }
    }
    
    public void ResetCount()
    {
        totalCount = 0;
        leftArmCount = 0;
        rightArmCount = 0;
        leftDirection = "down";
        rightDirection = "down";
        lastLeftCountTime = 0f;
        lastRightCountTime = 0f;
        
        if (instructionText != null)
        {
            instructionText.text = "Contadores resetados. Comece os exercícios!";
        }
        
        Debug.Log("Counters reset");
    }
    
    // Public methods for UI control
    public void ToggleLeftArm(bool enable)
    {
        useLeftArm = enable;
    }
    
    public void ToggleRightArm(bool enable)
    {
        useRightArm = enable;
    }
    
    public void SetMinThreshold(float value)
    {
        minAngleThreshold = value;
    }
    
    public void SetMaxThreshold(float value)
    {
        maxAngleThreshold = value;
    }
    
    public void SetCooldownTime(float value)
    {
        cooldownTime = value;
    }
    
    // Getters for external access
    public int GetTotalCount() => totalCount;
    public int GetLeftArmCount() => leftArmCount;
    public int GetRightArmCount() => rightArmCount;
    public float GetLeftElbowAngle() => currentLeftElbowAngle;
    public float GetRightElbowAngle() => currentRightElbowAngle;
}
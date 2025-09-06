using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyTracking : MonoBehaviour
{
    [Header("UDP Connection")]
    public UDPReceive udpReceive;
    
    [Header("Body Points")]
    public GameObject[] bodyPoints = new GameObject[33];
    
    [Header("Scaling")]
    public float scaleX = 5.0f;
    public float scaleY = 5.0f;
    public float scaleZ = 2.0f;
    
    [Header("Smoothing")]
    public bool enableSmoothing = true;
    public float smoothingFactor = 0.8f;
    
    [Header("Visualization")]
    public bool showConnections = true;
    public LineRenderer[] connectionLines;
    public Material lineMaterial;
    
    [Header("Debug")]
    public bool printAngles = false;
    public bool showLandmarkNames = false;
    
    [Header("Line Offset")]
    public float lineOffsetX = 1.15333f;
    public float lineOffsetY = 0f;
    public float lineOffsetZ = 0f;
    
    // Private variables
    private Vector3[] previousPositions = new Vector3[33];
    private bool hasInitialized = false;
    
    // MediaPipe Pose connections (simplified)
    private int[,] connections = new int[,]
    {
        // Face
        {0, 1}, {1, 2}, {2, 3}, {3, 7}, {0, 4}, {4, 5}, {5, 6}, {6, 8},
        {9, 10},
        // Arms
        {11, 12}, {11, 13}, {13, 15}, {12, 14}, {14, 16},
        {15, 17}, {15, 19}, {15, 21}, {16, 18}, {16, 20}, {16, 22},
        // Body
        {11, 23}, {12, 24}, {23, 24},
        // Legs
        {23, 25}, {25, 27}, {27, 29}, {27, 31},
        {24, 26}, {26, 28}, {28, 30}, {28, 32}
    };

    [System.Obsolete]
    void Start()
    {
        // Find UDPReceive if not assigned
        if (udpReceive == null)
        {
            udpReceive = FindObjectOfType<UDPReceive>();
        }
        
        if (udpReceive == null)
        {
            Debug.LogError("UDPReceive component not found! Please assign it in the inspector.");
            return;
        }
        
        // Initialize previous positions
        for (int i = 0; i < previousPositions.Length; i++)
        {
            previousPositions[i] = Vector3.zero;
        }
        
        // Setup connection lines if enabled
        if (showConnections)
        {
            SetupConnectionLines();
        }
        
        Debug.Log("BodyTracking initialized and ready to receive data");
    }
    
    void Update()
    {
        if (udpReceive != null && udpReceive.IsBodyDetected())
        {
            UpdateBodyPoints();
            
            if (printAngles)
            {
                DisplayAngles();
            }
            
            if (showConnections && connectionLines != null)
            {
                UpdateConnectionLines();
            }
        }
    }
    
    void UpdateBodyPoints()
    {
        var currentData = udpReceive.GetCurrentData();
        if (currentData?.body_tracking?.landmarks == null)
            return;
            
        var landmarks = currentData.body_tracking.landmarks;
        
        // Update all landmark positions
        UpdateLandmarkPosition(0, "nose", landmarks.nose);
        UpdateLandmarkPosition(1, "left_eye_inner", landmarks.left_eye_inner);
        UpdateLandmarkPosition(2, "left_eye", landmarks.left_eye);
        UpdateLandmarkPosition(3, "left_eye_outer", landmarks.left_eye_outer);
        UpdateLandmarkPosition(4, "right_eye_inner", landmarks.right_eye_inner);
        UpdateLandmarkPosition(5, "right_eye", landmarks.right_eye);
        UpdateLandmarkPosition(6, "right_eye_outer", landmarks.right_eye_outer);
        UpdateLandmarkPosition(7, "left_ear", landmarks.left_ear);
        UpdateLandmarkPosition(8, "right_ear", landmarks.right_ear);
        UpdateLandmarkPosition(9, "mouth_left", landmarks.mouth_left);
        UpdateLandmarkPosition(10, "mouth_right", landmarks.mouth_right);
        UpdateLandmarkPosition(11, "left_shoulder", landmarks.left_shoulder);
        UpdateLandmarkPosition(12, "right_shoulder", landmarks.right_shoulder);
        UpdateLandmarkPosition(13, "left_elbow", landmarks.left_elbow);
        UpdateLandmarkPosition(14, "right_elbow", landmarks.right_elbow);
        UpdateLandmarkPosition(15, "left_wrist", landmarks.left_wrist);
        UpdateLandmarkPosition(16, "right_wrist", landmarks.right_wrist);
        UpdateLandmarkPosition(17, "left_pinky", landmarks.left_pinky);
        UpdateLandmarkPosition(18, "left_index", landmarks.left_index);
        UpdateLandmarkPosition(19, "left_thumb", landmarks.left_thumb);
        UpdateLandmarkPosition(20, "right_pinky", landmarks.right_pinky);
        UpdateLandmarkPosition(21, "right_index", landmarks.right_index);
        UpdateLandmarkPosition(22, "right_thumb", landmarks.right_thumb);
        UpdateLandmarkPosition(23, "left_hip", landmarks.left_hip);
        UpdateLandmarkPosition(24, "right_hip", landmarks.right_hip);
        UpdateLandmarkPosition(25, "left_knee", landmarks.left_knee);
        UpdateLandmarkPosition(26, "right_knee", landmarks.right_knee);
        UpdateLandmarkPosition(27, "left_ankle", landmarks.left_ankle);
        UpdateLandmarkPosition(28, "right_ankle", landmarks.right_ankle);
        UpdateLandmarkPosition(29, "left_heel", landmarks.left_heel);
        UpdateLandmarkPosition(30, "left_foot_index", landmarks.left_foot_index);
        UpdateLandmarkPosition(31, "right_heel", landmarks.right_heel);
        UpdateLandmarkPosition(32, "right_foot_index", landmarks.right_foot_index);
        
        hasInitialized = true;
    }
    
    void UpdateLandmarkPosition(int index, string name, Landmark landmark)
    {
        if (landmark == null || index >= bodyPoints.Length || bodyPoints[index] == null)
            return;
            
        // Convert normalized coordinates to world position
        float x = (landmark.x - 0.5f) * scaleX;
        float y = (landmark.y - 0.5f) * scaleY;
        float z = landmark.z * scaleZ;
        
        Vector3 targetPosition = new Vector3(x, y, z);
        
        // Apply smoothing if enabled
        if (enableSmoothing && hasInitialized)
        {
            targetPosition = Vector3.Lerp(previousPositions[index], targetPosition, 1.0f - smoothingFactor);
        }
        
        // Update position
        bodyPoints[index].transform.localPosition = targetPosition;
        previousPositions[index] = targetPosition;
        
        // Optional: Scale based on visibility
        float scale = Mathf.Lerp(0.3f, 1.0f, landmark.visibility);
        bodyPoints[index].transform.localScale = Vector3.one * scale;
        
        // Optional: Change color based on visibility
        Renderer renderer = bodyPoints[index].GetComponent<Renderer>();
        if (renderer != null)
        {
            Color color = Color.Lerp(Color.red, Color.green, landmark.visibility);
            renderer.material.color = color;
        }
        
        // Optional: Show landmark names
        if (showLandmarkNames)
        {
            bodyPoints[index].name = $"Landmark_{index}_{name}";
        }
    }
    
    void SetupConnectionLines()
    {
        if (lineMaterial == null)
        {
            // Create default line material
            lineMaterial = new Material(Shader.Find("Sprites/Default"));
            lineMaterial.color = Color.white;
        }
        
        int connectionCount = connections.GetLength(0);
        connectionLines = new LineRenderer[connectionCount];
        
        for (int i = 0; i < connectionCount; i++)
        {
            GameObject lineObj = new GameObject($"Connection_{i}");
            lineObj.transform.SetParent(transform);
            
            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.material = lineMaterial;
            line.startWidth = 0.02f;
            line.endWidth = 0.02f;
            line.positionCount = 2;
            line.useWorldSpace = false;
            
            connectionLines[i] = line;
        }
    }
    
    void UpdateConnectionLines()
    {
        if (connectionLines == null || !hasInitialized)
            return;
            
        for (int i = 0; i < connectionLines.Length && i < connections.GetLength(0); i++)
        {
            int startIndex = connections[i, 0];
            int endIndex = connections[i, 1];
            
            if (startIndex < bodyPoints.Length && endIndex < bodyPoints.Length &&
                bodyPoints[startIndex] != null && bodyPoints[endIndex] != null)
            {
                // Get original positions
                Vector3 startPos = bodyPoints[startIndex].transform.localPosition;
                Vector3 endPos = bodyPoints[endIndex].transform.localPosition;
                
                // Add configurable offset
                startPos.x += lineOffsetX;
                startPos.y += lineOffsetY;
                startPos.z += lineOffsetZ;
                
                endPos.x += lineOffsetX;
                endPos.y += lineOffsetY;
                endPos.z += lineOffsetZ;
                
                connectionLines[i].SetPosition(0, startPos);
                connectionLines[i].SetPosition(1, endPos);
            }
        }
    }
    
    void DisplayAngles()
    {
        if (udpReceive == null)
            return;
            
        Debug.Log($"Body Angles - " +
                 $"Left Shoulder: {udpReceive.GetAngle("left_shoulder"):F1}° | " +
                 $"Right Shoulder: {udpReceive.GetAngle("right_shoulder"):F1}° | " +
                 $"Left Elbow: {udpReceive.GetAngle("left_elbow"):F1}° | " +
                 $"Right Elbow: {udpReceive.GetAngle("right_elbow"):F1}° | " +
                 $"Left Hip: {udpReceive.GetAngle("left_hip"):F1}° | " +
                 $"Right Hip: {udpReceive.GetAngle("right_hip"):F1}° | " +
                 $"Left Knee: {udpReceive.GetAngle("left_knee"):F1}° | " +
                 $"Right Knee: {udpReceive.GetAngle("right_knee"):F1}°");
    }
    
    // Public methods for external access
    public float GetAngle(string angleName)
    {
        return udpReceive?.GetAngle(angleName) ?? 0f;
    }
    
    public Landmark GetLandmark(string landmarkName)
    {
        return udpReceive?.GetLandmark(landmarkName);
    }
    
    public Vector3 GetLandmarkPosition(int index)
    {
        if (index < bodyPoints.Length && bodyPoints[index] != null)
        {
            return bodyPoints[index].transform.localPosition;
        }
        return Vector3.zero;
    }
    
    public Vector3 GetLandmarkWorldPosition(int index)
    {
        if (index < bodyPoints.Length && bodyPoints[index] != null)
        {
            return bodyPoints[index].transform.position;
        }
        return Vector3.zero;
    }
    
    public bool IsBodyDetected()
    {
        return udpReceive?.IsBodyDetected() ?? false;
    }
    
    public BodyTrackingData GetCurrentBodyData()
    {
        return udpReceive?.GetCurrentData();
    }
    
    public GameObject GetBodyPoint(int index)
    {
        if (index >= 0 && index < bodyPoints.Length)
            return bodyPoints[index];
        return null;
    }
    
    public float GetLandmarkVisibility(int index)
    {
        var data = GetCurrentBodyData();
        if (data?.body_tracking?.landmarks == null) return 0f;
        
        // Get visibility for specific landmark
        var landmarks = data.body_tracking.landmarks;
        switch (index)
        {
            case 0: return landmarks.nose?.visibility ?? 0f;
            case 11: return landmarks.left_shoulder?.visibility ?? 0f;
            case 12: return landmarks.right_shoulder?.visibility ?? 0f;
            case 13: return landmarks.left_elbow?.visibility ?? 0f;
            case 14: return landmarks.right_elbow?.visibility ?? 0f;
            case 15: return landmarks.left_wrist?.visibility ?? 0f;
            case 16: return landmarks.right_wrist?.visibility ?? 0f;
            case 23: return landmarks.left_hip?.visibility ?? 0f;
            case 24: return landmarks.right_hip?.visibility ?? 0f;
            case 25: return landmarks.left_knee?.visibility ?? 0f;
            case 26: return landmarks.right_knee?.visibility ?? 0f;
            case 27: return landmarks.left_ankle?.visibility ?? 0f;
            case 28: return landmarks.right_ankle?.visibility ?? 0f;
            default: return 0f;
        }
    }
    
    // Utility methods
    public void SetScale(float x, float y, float z)
    {
        scaleX = x;
        scaleY = y;
        scaleZ = z;
    }
    
    public void ToggleConnections(bool show)
    {
        showConnections = show;
        if (connectionLines != null)
        {
            foreach (var line in connectionLines)
            {
                if (line != null)
                    line.enabled = show;
            }
        }
    }
    
    public void ToggleSmoothing(bool enable)
    {
        enableSmoothing = enable;
    }
}
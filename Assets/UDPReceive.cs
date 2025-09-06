using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

[System.Serializable]
public class BodyTrackingData
{
    public long timestamp;
    public FrameSize frame_size;
    public BodyTrackingInfo body_tracking;
}

[System.Serializable]
public class FrameSize
{
    public int width;
    public int height;
}

[System.Serializable]
public class BodyTrackingInfo
{
    public bool detected;
    public LandmarkDictionary landmarks;
    public Angles angles;
    public BodyMetrics body_metrics;
}

[System.Serializable]
public class LandmarkDictionary
{
    public Landmark nose;
    public Landmark left_eye_inner;
    public Landmark left_eye;
    public Landmark left_eye_outer;
    public Landmark right_eye_inner;
    public Landmark right_eye;
    public Landmark right_eye_outer;
    public Landmark left_ear;
    public Landmark right_ear;
    public Landmark mouth_left;
    public Landmark mouth_right;
    public Landmark left_shoulder;
    public Landmark right_shoulder;
    public Landmark left_elbow;
    public Landmark right_elbow;
    public Landmark left_wrist;
    public Landmark right_wrist;
    public Landmark left_pinky;
    public Landmark left_index;
    public Landmark left_thumb;
    public Landmark right_pinky;
    public Landmark right_index;
    public Landmark right_thumb;
    public Landmark left_hip;
    public Landmark right_hip;
    public Landmark left_knee;
    public Landmark right_knee;
    public Landmark left_ankle;
    public Landmark right_ankle;
    public Landmark left_heel;
    public Landmark left_foot_index;
    public Landmark right_heel;
    public Landmark right_foot_index;
}

[System.Serializable]
public class Landmark
{
    public float x;
    public float y;
    public float z;
    public float visibility;
}

[System.Serializable]
public class Angles
{
    public float left_shoulder;
    public float right_shoulder;
    public float left_elbow;
    public float right_elbow;
    public float left_hip;
    public float right_hip;
    public float left_knee;
    public float right_knee;
}

[System.Serializable]
public class BodyMetrics
{
    public int landmark_count;
    public float confidence;
}

public class UDPReceive : MonoBehaviour
{
    [Header("UDP Settings")]
    public int port = 5052;
    public bool startReceiving = true;
    public bool printToConsole = false;
    
    [Header("Body Tracking")]
    public GameObject[] bodyPoints = new GameObject[33];
    public bool updateBodyPoints = true;
    
    [Header("Scaling")]
    public float scaleX = 5.0f;
    public float scaleY = 5.0f;
    public float scaleZ = 2.0f;
    
    [Header("Debug")]
    public bool showAngles = true;
    public bool showLandmarkInfo = false;
    
    // Private variables
    private Thread receiveThread;
    private UdpClient client;
    private string rawData;
    private BodyTrackingData currentData;
    private bool hasNewData = false;
    
    // Landmark mapping
    private Dictionary<string, int> landmarkIndexMap;

    void Start()
    {
        InitializeLandmarkMapping();
        
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
        
        Debug.Log($"UDP Receiver started on port {port}");
    }
    
    void InitializeLandmarkMapping()
    {
        landmarkIndexMap = new Dictionary<string, int>
        {
            {"nose", 0}, {"left_eye_inner", 1}, {"left_eye", 2}, {"left_eye_outer", 3},
            {"right_eye_inner", 4}, {"right_eye", 5}, {"right_eye_outer", 6}, {"left_ear", 7},
            {"right_ear", 8}, {"mouth_left", 9}, {"mouth_right", 10}, {"left_shoulder", 11},
            {"right_shoulder", 12}, {"left_elbow", 13}, {"right_elbow", 14}, {"left_wrist", 15},
            {"right_wrist", 16}, {"left_pinky", 17}, {"left_index", 18}, {"left_thumb", 19},
            {"right_pinky", 20}, {"right_index", 21}, {"right_thumb", 22}, {"left_hip", 23},
            {"right_hip", 24}, {"left_knee", 25}, {"right_knee", 26}, {"left_ankle", 27},
            {"right_ankle", 28}, {"left_heel", 29}, {"left_foot_index", 30}, {"right_heel", 31},
            {"right_foot_index", 32}
        };
    }

    void Update()
    {
        if (hasNewData)
        {
            ProcessBodyTrackingData();
            hasNewData = false;
        }
    }

    private void ReceiveData()
    {
        client = new UdpClient(port);
        
        while (startReceiving)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] dataByte = client.Receive(ref anyIP);
                rawData = Encoding.UTF8.GetString(dataByte);

                if (printToConsole)
                {
                    Debug.Log($"Received data: {rawData.Substring(0, Mathf.Min(200, rawData.Length))}...");
                }

                // Parse JSON data
                try
                {
                    currentData = JsonUtility.FromJson<BodyTrackingData>(rawData);
                    hasNewData = true;
                }
                catch (Exception parseError)
                {
                    Debug.LogError($"JSON Parse Error: {parseError.Message}");
                }
            }
            catch (Exception err)
            {
                Debug.LogError($"UDP Receive Error: {err.Message}");
            }
        }
    }
    
    private void ProcessBodyTrackingData()
    {
        if (currentData?.body_tracking == null)
            return;

        if (!currentData.body_tracking.detected)
        {
            if (printToConsole)
                Debug.Log("No body detected");
            return;
        }

        // Update body points if enabled
        if (updateBodyPoints && currentData.body_tracking.landmarks != null)
        {
            UpdateBodyPoints();
        }

        // Display angles if enabled
        if (showAngles && currentData.body_tracking.angles != null)
        {
            DisplayAngles();
        }

        // Display landmark info if enabled
        if (showLandmarkInfo)
        {
            DisplayLandmarkInfo();
        }
    }
    
    private void UpdateBodyPoints()
    {
        var landmarks = currentData.body_tracking.landmarks;
        
        // Update each landmark position
        UpdateLandmarkPosition("nose", landmarks.nose);
        UpdateLandmarkPosition("left_shoulder", landmarks.left_shoulder);
        UpdateLandmarkPosition("right_shoulder", landmarks.right_shoulder);
        UpdateLandmarkPosition("left_elbow", landmarks.left_elbow);
        UpdateLandmarkPosition("right_elbow", landmarks.right_elbow);
        UpdateLandmarkPosition("left_wrist", landmarks.left_wrist);
        UpdateLandmarkPosition("right_wrist", landmarks.right_wrist);
        UpdateLandmarkPosition("left_hip", landmarks.left_hip);
        UpdateLandmarkPosition("right_hip", landmarks.right_hip);
        UpdateLandmarkPosition("left_knee", landmarks.left_knee);
        UpdateLandmarkPosition("right_knee", landmarks.right_knee);
        UpdateLandmarkPosition("left_ankle", landmarks.left_ankle);
        UpdateLandmarkPosition("right_ankle", landmarks.right_ankle);
        
        // Add more landmarks as needed...
    }
    
    private void UpdateLandmarkPosition(string landmarkName, Landmark landmark)
    {
        if (landmark == null || !landmarkIndexMap.ContainsKey(landmarkName))
            return;
            
        int index = landmarkIndexMap[landmarkName];
        
        if (index < bodyPoints.Length && bodyPoints[index] != null)
        {
            // Convert normalized coordinates to world position
            float x = (landmark.x - 0.5f) * scaleX;
            float y = (landmark.y - 0.5f) * scaleY;
            float z = landmark.z * scaleZ;
            
            bodyPoints[index].transform.localPosition = new Vector3(x, y, z);
            
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
        }
    }
    
    private void DisplayAngles()
    {
        var angles = currentData.body_tracking.angles;
        
        Debug.Log($"Body Angles - " +
                 $"Shoulders: L{angles.left_shoulder:F1}° R{angles.right_shoulder:F1}° | " +
                 $"Elbows: L{angles.left_elbow:F1}° R{angles.right_elbow:F1}° | " +
                 $"Hips: L{angles.left_hip:F1}° R{angles.right_hip:F1}° | " +
                 $"Knees: L{angles.left_knee:F1}° R{angles.right_knee:F1}°");
    }
    
    private void DisplayLandmarkInfo()
    {
        var metrics = currentData.body_tracking.body_metrics;
        Debug.Log($"Body Metrics - Landmarks: {metrics.landmark_count}, Confidence: {metrics.confidence:F2}");
    }
    
    // Public methods to access data
    public BodyTrackingData GetCurrentData()
    {
        return currentData;
    }
    
    public float GetAngle(string angleName)
    {
        if (currentData?.body_tracking?.angles == null)
            return 0f;
            
        var angles = currentData.body_tracking.angles;
        
        switch (angleName.ToLower())
        {
            case "left_shoulder": return angles.left_shoulder;
            case "right_shoulder": return angles.right_shoulder;
            case "left_elbow": return angles.left_elbow;
            case "right_elbow": return angles.right_elbow;
            case "left_hip": return angles.left_hip;
            case "right_hip": return angles.right_hip;
            case "left_knee": return angles.left_knee;
            case "right_knee": return angles.right_knee;
            default: return 0f;
        }
    }
    
    public Landmark GetLandmark(string landmarkName)
    {
        if (currentData?.body_tracking?.landmarks == null)
            return null;
            
        var landmarks = currentData.body_tracking.landmarks;
        
        switch (landmarkName.ToLower())
        {
            case "nose": return landmarks.nose;
            case "left_shoulder": return landmarks.left_shoulder;
            case "right_shoulder": return landmarks.right_shoulder;
            case "left_elbow": return landmarks.left_elbow;
            case "right_elbow": return landmarks.right_elbow;
            case "left_wrist": return landmarks.left_wrist;
            case "right_wrist": return landmarks.right_wrist;
            case "left_hip": return landmarks.left_hip;
            case "right_hip": return landmarks.right_hip;
            case "left_knee": return landmarks.left_knee;
            case "right_knee": return landmarks.right_knee;
            case "left_ankle": return landmarks.left_ankle;
            case "right_ankle": return landmarks.right_ankle;
            default: return null;
        }
    }
    
    public bool IsBodyDetected()
    {
        return currentData?.body_tracking?.detected ?? false;
    }
    
    void OnApplicationQuit()
    {
        startReceiving = false;
        
        if (receiveThread != null)
        {
            receiveThread.Abort();
        }
        
        if (client != null)
        {
            client.Close();
        }
    }
}

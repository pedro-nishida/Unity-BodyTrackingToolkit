import cv2
from cvzone.PoseModule import PoseDetector
import socket
import math
import json

# Change from video file to camera input (0 = default camera)
cap = cv2.VideoCapture(0)

# Set camera resolution (reduced for better performance)
cap.set(3, 640)  # Width - reduced from 1280
cap.set(4, 480)  # Height - reduced from 720

# Get initial frame to get dimensions
success, img = cap.read()
h, w, _ = img.shape

detector = PoseDetector(detectionCon=0.8)

# Setup UDP socket
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
serverAddressPort = ("127.0.0.1", 5052)

# Reference measurements for normalization
REFERENCE_SHOULDER_WIDTH = 200  # pixels - reference shoulder width
REFERENCE_BODY_HEIGHT = 400     # pixels - reference body height

def calculate_distance(p1, p2):
    """Calculate Euclidean distance between two points"""
    return math.sqrt((p1[0] - p2[0])**2 + (p1[1] - p2[1])**2)

def calculate_angle(p1, p2, p3):
    """Calculate angle between three points (p2 is the vertex)"""
    # Vectors from p2 to p1 and p2 to p3
    v1 = [p1[0] - p2[0], p1[1] - p2[1]]
    v2 = [p3[0] - p2[0], p3[1] - p2[1]]
    
    # Calculate dot product
    dot_product = v1[0] * v2[0] + v1[1] * v2[1]
    
    # Calculate magnitudes
    mag1 = math.sqrt(v1[0]**2 + v1[1]**2)
    mag2 = math.sqrt(v2[0]**2 + v2[1]**2)
    
    # Avoid division by zero
    if mag1 == 0 or mag2 == 0:
        return 0
    
    # Calculate angle in radians, then convert to degrees
    cos_angle = dot_product / (mag1 * mag2)
    # Clamp to avoid math domain error
    cos_angle = max(-1, min(1, cos_angle))
    angle = math.acos(cos_angle)
    
    return math.degrees(angle)

def calculate_body_angles(lmList):
    """Calculate key body angles including hip and shoulder angles"""
    angles = {}
    
    if len(lmList) < 33:
        return angles
    
    # Body landmarks
    left_shoulder = lmList[11]   # Left shoulder
    right_shoulder = lmList[12]  # Right shoulder
    left_elbow = lmList[13]      # Left elbow
    right_elbow = lmList[14]     # Right elbow
    left_wrist = lmList[15]      # Left wrist
    right_wrist = lmList[16]     # Right wrist
    left_hip = lmList[23]        # Left hip
    right_hip = lmList[24]       # Right hip
    left_knee = lmList[25]       # Left knee
    right_knee = lmList[26]      # Right knee
    left_ankle = lmList[27]      # Left ankle
    right_ankle = lmList[28]     # Right ankle
    
    # Calculate shoulder angles (torso-shoulder-arm)
    angles['left_shoulder'] = calculate_angle(right_shoulder, left_shoulder, left_elbow)
    angles['right_shoulder'] = calculate_angle(left_shoulder, right_shoulder, right_elbow)
    
    # Calculate elbow angles (shoulder-elbow-wrist)
    angles['left_elbow'] = calculate_angle(left_shoulder, left_elbow, left_wrist)
    angles['right_elbow'] = calculate_angle(right_shoulder, right_elbow, right_wrist)
    
    # Calculate hip angles (torso-hip-leg)
    angles['left_hip'] = calculate_angle(right_hip, left_hip, left_knee)
    angles['right_hip'] = calculate_angle(left_hip, right_hip, right_knee)
    
    # Calculate knee angles (hip-knee-ankle)
    angles['left_knee'] = calculate_angle(left_hip, left_knee, left_ankle)
    angles['right_knee'] = calculate_angle(right_hip, right_knee, right_ankle)
    
    return angles

def estimate_z_coordinate(lm, shoulder_width_ratio, body_height_ratio, visibility):
    """Estimate Z coordinate based on body proportions and visibility"""
    # Base Z on visibility score
    base_z = visibility
    
    # Adjust Z based on how much the person has moved closer/farther
    distance_factor = 1.0 / shoulder_width_ratio if shoulder_width_ratio > 0 else 1.0
    
    # Combine factors
    estimated_z = base_z * distance_factor
    
    # Clamp between 0 and 1
    return max(0.0, min(1.0, estimated_z))

def normalize_pose(lmList):
    """Normalize pose to fixed size and estimate Z coordinates"""
    if len(lmList) < 33:
        return lmList
    
    # Get key reference points
    left_shoulder = lmList[11]   
    right_shoulder = lmList[12]  
    left_hip = lmList[23]        
    right_hip = lmList[24]       
    
    # Calculate current body measurements
    shoulder_width = calculate_distance(left_shoulder, right_shoulder)
    body_height = calculate_distance(
        [(left_shoulder[0] + right_shoulder[0])/2, (left_shoulder[1] + right_shoulder[1])/2],
        [(left_hip[0] + right_hip[0])/2, (left_hip[1] + right_hip[1])/2]
    )
    
    # Calculate ratios for normalization
    shoulder_width_ratio = shoulder_width / REFERENCE_SHOULDER_WIDTH if shoulder_width > 0 else 1.0
    body_height_ratio = body_height / REFERENCE_BODY_HEIGHT if body_height > 0 else 1.0
    
    # Use average ratio for consistent scaling
    scale_factor = (shoulder_width_ratio + body_height_ratio) / 2.0
    
    # Calculate center point (between shoulders)
    center_x = (left_shoulder[0] + right_shoulder[0]) / 2
    center_y = (left_shoulder[1] + right_shoulder[1]) / 2
    
    # Normalize all landmarks
    normalized_lmList = []
    for i, lm in enumerate(lmList):
        # Normalize X and Y relative to center and scale
        norm_x = ((lm[0] - center_x) / scale_factor) + (w / 2)
        norm_y = ((lm[1] - center_y) / scale_factor) + (h / 2)
        
        # Estimate Z coordinate
        estimated_z = estimate_z_coordinate(lm, shoulder_width_ratio, body_height_ratio, lm[2])
        
        normalized_lmList.append([i, norm_x, norm_y, estimated_z])
    
    return normalized_lmList

def create_json_template(normalized_lmList, angles):
    """Create structured JSON template for Unity"""
    
    # Landmark names for MediaPipe Pose
    landmark_names = [
        "nose", "left_eye_inner", "left_eye", "left_eye_outer", "right_eye_inner", 
        "right_eye", "right_eye_outer", "left_ear", "right_ear", "mouth_left", 
        "mouth_right", "left_shoulder", "right_shoulder", "left_elbow", "right_elbow",
        "left_wrist", "right_wrist", "left_pinky", "left_index", "left_thumb",
        "right_pinky", "right_index", "right_thumb", "left_hip", "right_hip",
        "left_knee", "right_knee", "left_ankle", "right_ankle", "left_heel",
        "left_foot_index", "right_heel", "right_foot_index"
    ]
    
    # Create landmarks dictionary
    landmarks = {}
    for i, lm in enumerate(normalized_lmList):
        if i < len(landmark_names):
            landmarks[landmark_names[i]] = {
                "x": round((lm[1] / w), 4),  # Normalized 0-1
                "y": round((1.0 - lm[2] / h), 4),  # Normalized 0-1, flipped Y
                "z": round(lm[3], 4),  # Estimated depth 0-1
                "visibility": round(lm[3], 4)  # Using z as visibility
            }
    
    # Create JSON structure exactly as requested
    json_data = {
        "timestamp": int(cv2.getTickCount()),
        "frame_size": {
            "width": w,
            "height": h
        },
        "body_tracking": {
            "detected": True,
            "landmarks": landmarks,
            "angles": {
                "left_shoulder": round(angles.get('left_shoulder', 0), 1),
                "right_shoulder": round(angles.get('right_shoulder', 0), 1),
                "left_elbow": round(angles.get('left_elbow', 0), 1),
                "right_elbow": round(angles.get('right_elbow', 0), 1),
                "left_hip": round(angles.get('left_hip', 0), 1),
                "right_hip": round(angles.get('right_hip', 0), 1),
                "left_knee": round(angles.get('left_knee', 0), 1),
                "right_knee": round(angles.get('right_knee', 0), 1)
            },
            "body_metrics": {
                "landmark_count": len(normalized_lmList),
                "confidence": 0.8
            }
        }
    }
    
    return json_data

# Check if camera opened successfully
if not cap.isOpened():
    print("Error: Could not open camera")
    exit()

print("Body tracking started. Press 'q' to quit.")
print(f"Camera resolution: {w}x{h}")
print(f"Sending JSON data to {serverAddressPort}")
print("Sending landmarks and angles in JSON format")

while True:
    success, img = cap.read()
    
    if not success:
        print("Error: Failed to read from camera")
        break
    
    # Find pose and landmarks
    img = detector.findPose(img)
    lmList, bboxInfo = detector.findPosition(img)
    
    if bboxInfo and lmList:
        # Calculate body angles
        angles = calculate_body_angles(lmList)
        
        # Normalize pose to fixed size
        normalized_lmList = normalize_pose(lmList)
        
        # Create JSON template
        json_data = create_json_template(normalized_lmList, angles)
        
        # Convert to JSON string
        json_string = json.dumps(json_data, separators=(',', ':'))
        
        # Send JSON data via UDP
        try:
            sock.sendto(json_string.encode('utf-8'), serverAddressPort)
            print(f"Sent JSON data ({len(json_string)} bytes)")
            
            # Display key angles
            if angles:
                print(f"Shoulders - L: {angles.get('left_shoulder', 0):.1f}°, R: {angles.get('right_shoulder', 0):.1f}°")
                print(f"Elbows - L: {angles.get('left_elbow', 0):.1f}°, R: {angles.get('right_elbow', 0):.1f}°")
                print(f"Hips - L: {angles.get('left_hip', 0):.1f}°, R: {angles.get('right_hip', 0):.1f}°")
                print(f"Knees - L: {angles.get('left_knee', 0):.1f}°, R: {angles.get('right_knee', 0):.1f}°")
                print("---")
                
        except Exception as e:
            print(f"Error sending JSON data: {e}")
    else:
        # Send empty JSON when no body detected
        empty_json = {
            "timestamp": int(cv2.getTickCount()),
            "frame_size": {"width": w, "height": h},
            "body_tracking": {"detected": False}
        }
        json_string = json.dumps(empty_json, separators=(',', ':'))
        try:
            sock.sendto(json_string.encode('utf-8'), serverAddressPort)
        except Exception as e:
            print(f"Error sending empty JSON: {e}")

    # Display the image
    cv2.imshow("Body Tracking", img)
    
    # Press 'q' to quit
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

# Cleanup
cap.release()
cv2.destroyAllWindows()
sock.close()
print("Camera released, windows closed, and socket closed.")
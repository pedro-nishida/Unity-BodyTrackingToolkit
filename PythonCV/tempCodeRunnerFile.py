import cv2
import mediapipe as mp
import socket

# --- Initialization ---

# 1. Webcam and Resolution Setup
# Using a higher resolution as your RTX 3060 can handle it for better quality.
cap = cv2.VideoCapture(0)
cap.set(3, 1280)  # Width
cap.set(4, 720)   # Height

# 2. MediaPipe Pose Setup
mp_pose = mp.solutions.pose
# model_complexity=2 is the most accurate model, perfect for a powerful GPU.
pose = mp_pose.Pose(
    static_image_mode=False,
    model_complexity=2,
    enable_segmentation=False, # We don't need the body mask
    smooth_landmarks=True,
    min_detection_confidence=0.5,
    min_tracking_confidence=0.5
)
mp_drawing = mp.solutions.drawing_utils

# 3. UDP Communication Setup
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
serverAddressPort = ("127.0.0.1", 5052)

# --- Main Loop ---
while cap.isOpened():
    success, img = cap.read()
    if not success:
        print("Ignoring empty camera frame.")
        continue

    # Get image dimensions
    h, w, _ = img.shape

    # Performance improvement: Make image not writeable to pass by reference.
    img.setflags(write=False)
    
    # Correct color conversion: MediaPipe requires RGB.
    img_rgb = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
    
    # Process the image to find pose landmarks
    results = pose.process(img_rgb)
    
    # Revert image to BGR and make it writeable for drawing
    img.setflags(write=True)
    # img = cv2.cvtColor(img_rgb, cv2.COLOR_RGB2BGR) # Not needed if drawing on original 'img'

    data = []
    # Check if any landmarks were detected
    if results.pose_landmarks:
        # Draw the landmarks on the image (for visualization)
        mp_drawing.draw_landmarks(
            img, results.pose_landmarks, mp_pose.POSE_CONNECTIONS)
            
        # Extract landmark data to send over UDP
        for lm in results.pose_landmarks.landmark:
            # MediaPipe returns normalized coordinates (0.0 to 1.0).
            # We convert them to pixel coordinates for Unity.
            px = lm.x * w
            py = lm.y * h
            
            # The data format is [x1, y1, z1, x2, y2, z2, ...]
            # We flip the y-coordinate (h - py) because in OpenCV the origin (0,0)
            # is at the top-left, while in Unity it's often at the bottom-left.
            # lm.z gives depth perception, which is very useful.
            data.extend([px, h - py, lm.z])

        # Send the data to Unity
        sock.sendto(str.encode(str(data)), serverAddressPort)

    # Display the output
    cv2.imshow("Body Tracking Mirror", img)
    
    # Exit condition
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

# --- Cleanup ---
pose.close()
cap.release()
cv2.destroyAllWindows()
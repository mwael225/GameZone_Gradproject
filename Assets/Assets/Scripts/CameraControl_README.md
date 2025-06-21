# Camera Control Setup for Room Scene

This setup provides camera movement control using only the mouse in the Unity room scene, with network support for host/client camera assignment.

## Scripts Included

### 1. RightMouseCameraControl.cs
The main camera control script that handles mouse-only camera movement.

**Features:**
- Hold right mouse button to enable camera rotation
- Mouse movement rotates the camera
- Mouse scroll wheel for zoom in/out
- Automatic cursor locking/unlocking
- Completely free rotation (no restrictions)

**Controls:**
- **Right Mouse Button (Hold):** Enable camera rotation
- **Mouse Movement:** Rotate camera view
- **Mouse Scroll Wheel:** Zoom in/out

### 2. NetworkCameraManager.cs
Handles camera assignment for networked multiplayer games.

**Features:**
- Automatically assigns cameras based on host/client status
- Main Camera assigned to clients
- Main Camera 2 assigned to host
- Automatically adds camera control scripts
- Disables unused cameras to prevent conflicts

**Assignment Logic:**
- **Host:** Gets Main Camera 2
- **Client:** Gets Main Camera
- **Default:** Gets Main Camera (when not connected)

### 3. NetworkCameraSetup.cs
Automatically sets up the network camera system.

**Features:**
- Auto-finds cameras in the scene
- Creates NetworkCameraManager if needed
- Configurable camera names
- Manual camera assignment fallback

### 4. CameraSetup.cs
Automatically sets up a main camera if one doesn't exist in the scene.

**Features:**
- Creates a main camera if none exists
- Automatically adds the RightMouseCameraControl script
- Adds AudioListener component
- Configurable starting position and rotation

### 5. CameraController.cs (Alternative)
A more advanced camera controller with additional features.

## Setup Instructions

### Option 1: Network Camera Setup (Recommended for Multiplayer)
1. Create an empty GameObject in your scene
2. Name it "NetworkCameraSetup" or similar
3. Attach the `NetworkCameraSetup.cs` script to this GameObject
4. Ensure you have at least two cameras in your scene:
   - "Main Camera" (for clients)
   - "Main Camera 2" (for host)
5. Run the scene - it will automatically set up camera assignment

### Option 2: Manual Network Setup
1. Create a new GameObject named "NetworkCameraManager"
2. Add NetworkObject component
3. Attach the `NetworkCameraManager.cs` script
4. Manually assign Main Camera and Main Camera 2 in the inspector
5. Position your cameras where you want them

### Option 3: Single Player Setup
1. Create an empty GameObject in your scene
2. Name it "CameraSetup" or similar
3. Attach the `CameraSetup.cs` script to this GameObject
4. Run the scene - it will automatically create and configure the camera

## Configuration

### RightMouseCameraControl Settings
- **Mouse Sensitivity:** How fast the camera rotates (default: 2)
- **Zoom Speed:** How fast the camera zooms with scroll wheel (default: 5)
- **Min Zoom:** Minimum zoom distance (default: 1)
- **Max Zoom:** Maximum zoom distance (default: 20)

### NetworkCameraManager Settings
- **Main Camera:** Camera assigned to clients
- **Main Camera 2:** Camera assigned to host
- **Enable Right Mouse Control:** Automatically add camera control script

### NetworkCameraSetup Settings
- **Auto Find Cameras:** Automatically find cameras by name
- **Main Camera Name:** Name of camera for clients (default: "Main Camera")
- **Main Camera 2 Name:** Name of camera for host (default: "Main Camera 2")
- **Manual Assignment:** Fallback camera assignments

### CameraSetup Settings
- **Camera Start Position:** Where the camera spawns (default: 0, 5, -10)
- **Camera Start Rotation:** Initial camera rotation (default: 15, 0, 0)

## Network Camera Assignment

### How it Works:
1. **Host Connection:** When a player starts as host, they get Main Camera 2
2. **Client Connection:** When a player joins as client, they get Main Camera
3. **Camera Control:** Both cameras get the RightMouseCameraControl script
4. **Unused Cameras:** All other cameras are disabled to prevent conflicts

### Camera Requirements:
- **Main Camera:** Should be positioned for client view
- **Main Camera 2:** Should be positioned for host view
- **NetworkObject:** Both cameras should have NetworkObject components if they're networked

## Usage Tips

1. **Hold Right Mouse Button:** This enables camera rotation mode
2. **Move Mouse:** Rotates the camera view
3. **Scroll Mouse Wheel:** Zoom in and out
4. **Release Right Mouse Button:** Returns to normal cursor mode
5. **Network Assignment:** Cameras are automatically assigned based on host/client status

## Troubleshooting

- If cameras aren't assigned correctly, check the console for debug messages
- If the camera doesn't rotate, make sure the script is attached to the camera GameObject
- If the cursor doesn't lock/unlock properly, check that the script is running
- If you can't see anything, adjust the camera's starting position
- If zoom doesn't work, check that the mouse scroll wheel is working properly
- For network issues, ensure NetworkManager is properly set up

## Notes

- The camera will automatically lock the cursor when right mouse button is held
- Rotation is completely free with no restrictions
- The script works with any camera tagged as "MainCamera"
- Zoom is limited by min/max zoom settings to prevent getting too close or too far
- Network camera assignment happens automatically when the network spawns
- Only one camera will be active per player to prevent rendering conflicts 
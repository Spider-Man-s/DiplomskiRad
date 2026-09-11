# Cross-platform Meta Quest 3 + XREAL co-location (Photon Fusion 2)

This package replaces the original QR/yaw/XR-origin-moving approach with the architecture discussed in the chat:

- Meta and XREAL each keep their own stable **device/tracking origin** created when the XR scene starts.
- Both users align the same virtual Quest 3 box to the same physical Quest 3 box.
- The virtual box has a `SharedReference` at its **bottom-left-front corner**.
- That reference becomes Shared Space `(0,0,0)` with identity rotation on each device.
- The two devices' local coordinates do **not** have to match.
- User height and initial looking direction are absorbed into each device's local-to-shared rigid transform.
- Fusion spatial state is always represented as **Shared Space position + quaternion rotation**.
- The XR/device origin is never moved to force the worlds to match.

## Files

### `SharedSpaceManager.cs`
Local-only coordinate system service. Stores this device's calibration pose and converts:

- World -> Tracking
- Tracking -> Shared
- Shared -> Tracking
- Shared -> World
- directions/rays as needed

It intentionally has no Fusion networking. Meta and XREAL must store different calibration transforms locally.

### `BoxCalibrationRig.cs`
Defines the canonical physical calibration box geometry. The box root is its center pivot. The `SharedReference` child is automatically placed at bottom-left-front:

`(-width/2, -height/2, -depth/2)`

Canonical axes are:

- +X = box right
- +Y = box up
- +Z = box back
- front is therefore -Z

### `WireframeBoxRenderer.cs`
Builds 12 explicit cuboid edges, avoiding the diagonal lines that a triangle-wireframe shader would show. Optionally shows 8 corner markers and makes the shared-origin corner larger/different.

### `BoxCalibrationController.cs`
Manual alignment/fine-tuning UI logic. The user can nudge the box in local X/Y/Z and rotate it around its center. Presets:

- coarse: 10 mm / 1 degree
- medium: 5 mm / 0.5 degree
- fine: 1 mm / 0.1 degree

`ConfirmAlignment()` captures `SharedReference` into `SharedSpaceManager` and optionally tells Fusion that this device is ready.

### `ColocationSessionState.cs`
Fusion 2 readiness state only. It networks `MetaReady` and `XrealReady` through State Authority. It does **not** network calibration poses.

### `SharedSpaceNetworkTransform.cs`
The main dynamic-object spatial sync component. Its `[Networked]` properties are Shared Space position/rotation, never Meta-local or XREAL-local coordinates.

Use it instead of a normal Fusion `NetworkTransform` on objects whose transform must map to the same physical place on both devices.

### `SharedSpaceGrabBridge.cs`
Two UnityEvent-friendly methods for platform-specific grab systems:

- `OnGrabStarted()` -> start driving shared pose from local manipulation
- `OnGrabEnded()` -> submit final shared pose and stop local driving

### `SharedSpacePlacedObject.cs`
For fixed scene content authored at a known Shared Space pose. Example: a fixed panel 0.5 m right of the reference corner. It converts that canonical pose to each device's Unity world after calibration.

### `SharedSpaceSpawner.cs`
Optional Fusion helper for State Authority to spawn a `NetworkObject` from a Shared Space pose and initialize its `SharedSpaceNetworkTransform`.

### `SharedSpaceRayUtility.cs`
Conversion helper for spatial values that are not Transform components, such as networked pointer-ray origins/directions or world hit points.

### `ColocationReadyGate.cs`
Optional component that keeps gameplay objects disabled until the local device is calibrated and both Fusion peers report ready.

### `SharedSpaceDebugVerifier.cs`
Optional check that the captured `SharedReference` maps to approximately shared position `(0,0,0)` and identity rotation.

---

# Migration from your ORIGINAL scripts

If you have implemented none of the changes discussed in the chat, remove/disable the old co-location behavior:

- `QRPlacementTracker`
- `ManualColocationAligner`
- `ColocationAligner`
- old `ColocationManager`

Do not let any old code rotate/translate `xrOrigin` during calibration.

The new system does not need a QR at all. The box alignment replaces the old calibration source.

---

# Unity scene setup

## 1. Create the local Shared Space manager

Create an empty GameObject:

`SharedSpaceManager`

Attach `SharedSpaceManager.cs`.

Assign `trackingOrigin` to the stable XR/device tracking root that represents the coordinate system established when the scene starts.

**Do not assign the HMD camera.** The HMD moves. The tracking/device origin must remain fixed while the user moves.

If Unity world space itself is already that fixed local tracking frame, leave `trackingOrigin` empty.

Do not use the unreliable XREAL floor origin if your project has already shown it to be inconsistent.

## 2. Prepare the Quest 3 box calibration object

Create:

```
CalibrationBoxRoot        <-- pivot at geometric box center
|-- GhostModel           <-- optional downloaded Quest-box model
|-- SharedReference      <-- BoxCalibrationRig manages this
|-- Generated...         <-- WireframeBoxRenderer creates this at runtime
```

Attach to `CalibrationBoxRoot`:

- `BoxCalibrationRig`
- `WireframeBoxRenderer`
- `BoxCalibrationController`

The root must have scale `(1,1,1)`.

### MEASURE THE REAL BOX

Do not trust an online model's scale for calibration. Measure the actual physical box with a ruler/calipers and enter exact meters into `BoxCalibrationRig.boxSizeMeters`:

- X = width
- Y = height
- Z = depth

The values in the script are placeholders only.

### Optional Sketchfab model

The Sketchfab model is not included in this package. Download/import it separately and follow the model's license/attribution requirements. Use it only as a visual ghost/reference.

Make the imported model a child of `CalibrationBoxRoot` and adjust its **local** rotation/scale so it visually fits the exact measured wireframe cage. Once correct, do not expose scale as a runtime calibration control.

The wireframe dimensions, not the downloaded model scale, are the authoritative dimensions.

## 3. Create materials for the calibration cage

Create simple URP Unlit materials for:

- edge lines
- normal corners
- shared-origin corner

Assign them to `WireframeBoxRenderer`.

Use a high-contrast edge material that is easy to see through both Quest passthrough and XREAL optical display.

For the ghost box model, use a transparent/ghost material if helpful. Keep the real physical box visible through/around it.

## 4. Place the initial virtual box

Place `CalibrationBoxRoot` roughly in front of the user at scene start. It does not need to be accurate.

Both users independently move/fine-tune the virtual box until it overlays the same real Quest 3 box.

The virtual box should stand in a prescribed physical orientation so there is no front/back ambiguity.

Recommended definition:

- Shared origin = bottom-left-front corner
- +X = physical right along box width
- +Y = physical up
- +Z = physical back through the box

Use printed packaging/artwork to make "front" unambiguous.

## 5. Build the calibration UI

Create buttons and wire their OnClick events to `BoxCalibrationController`:

Position:

- `PositionXPlus`
- `PositionXMinus`
- `PositionYPlus`
- `PositionYMinus`
- `PositionZPlus`
- `PositionZMinus`

Rotation:

- `RotationXPlus`
- `RotationXMinus`
- `RotationYPlus`
- `RotationYMinus`
- `RotationZPlus`
- `RotationZMinus`

Precision:

- `SetCoarseSteps`
- `SetMediumSteps`
- `SetFineSteps`

Other:

- `ResetAlignment`
- `ConfirmAlignment`

You can also let the user grab/manipulate `CalibrationBoxRoot` for coarse placement, then use these buttons for final alignment.

## 6. Tell Fusion which platform completed calibration

Your existing project used scripting symbols `META_BUILD` and `XREAL_BUILD`. Keep that pattern:

- Quest build profile: define `META_BUILD`
- XREAL build profile: define `XREAL_BUILD`

Because both devices may be Android, `Application.platform` alone cannot reliably distinguish them.

Without either symbol, `BoxCalibrationController.editorFallbackRole` is used for Editor testing.

## 7. Add Fusion readiness state

Create one NetworkObject in the session/scene and attach `ColocationSessionState`.

Make sure it is actually spawned/recognized by Fusion and has State Authority somewhere in your chosen Fusion topology.

Assign it to each local `BoxCalibrationController` instance if you want both-ready gating.

The only networked calibration information is:

- Meta ready: yes/no
- XREAL ready: yes/no

The local calibration pose itself stays local.

## 8. Gate gameplay until both users are calibrated

Optional but recommended: put `ColocationReadyGate` on a local scene object and assign gameplay roots to `enableWhenReady`.

Gameplay should start only when:

- this client has a valid `SharedSpaceManager` calibration
- both peers report ready

---

# How every shared spatial object works after calibration

The calibration box is only used to establish the transform. After that, all spatial networking follows:

```
Meta/XREAL local Unity world pose
        |
        v
SharedSpaceManager.WorldToShared()
        |
        v
Shared position + quaternion
        |
        v
Photon Fusion 2
        |
        v
Shared position + quaternion
        |
        v
SharedSpaceManager.SharedToWorld()
        |
        v
receiving device local Unity world pose
```

The actual numeric local positions on Meta and XREAL are expected to be different.

---

# Dynamic NetworkObjects

For every movable/shared NetworkObject whose physical pose must match:

1. Add Fusion `NetworkObject`.
2. Add `SharedSpaceNetworkTransform`.
3. **Remove/disable ordinary Fusion `NetworkTransform` if it also controls this Transform.** Do not let two network components fight over the same pose.
4. Add `SharedSpaceGrabBridge` if it can be grabbed.
5. Wire your platform-specific interaction event(s):
   - grab/select started -> `SharedSpaceGrabBridge.OnGrabStarted`
   - grab/select ended -> `SharedSpaceGrabBridge.OnGrabEnded`

During manipulation, the local Unity object moves normally. `SharedSpaceNetworkTransform` converts that movement to Shared Space before Fusion state is updated.

On the remote device, the shared pose is converted back into that device's local Unity coordinates.

## State Authority

Fusion `[Networked]` state is written by State Authority. The supplied component handles two cases:

- if the local manipulator has State Authority: it writes Shared Space state directly
- otherwise: it sends Shared Space pose requests to State Authority by RPC

This is topology-agnostic enough for a prototype, but for high-frequency production interaction you should eventually adapt ownership/input/state-authority flow to your exact Fusion mode rather than relying on per-tick pose RPCs for long drags.

Do not allow two users to manipulate the same object simultaneously without an app-level ownership/lock rule.

---

# Fixed scene objects

If an object never moves but must be in the same physical place for both users, it does not need transform networking.

Add `SharedSpacePlacedObject` and author its canonical:

- `sharedPosition`
- `sharedEulerDegrees`

Example:

```
SharedReference corner = (0,0,0)
Fixed panel = (0.50, 0.30, 0.20)
```

Each device will convert that shared pose to its own local world after calibration.

---

# Spawning objects

Do not choose a spawn pose in a headset's local coordinates and treat it as canonical.

Choose the pose in Shared Space, for example:

```
position = (0.60, 0.25, 0.10)
rotation = identity
```

Then use `SharedSpaceSpawner.SpawnAtSharedPose(...)` from a peer that has State Authority for that spawner.

The prefab should contain `SharedSpaceNetworkTransform`.

---

# Non-spatial network state

Do **not** convert data that is not spatial. These can be networked normally:

- button state
- selected object ID
- current task step
- color
- score
- ownership/permissions
- animation state

Only spatial quantities need Shared Space conversion.

For semantic interactions, prefer events such as "Button 7 pressed" instead of sending a headset-local hit position if the hit position is not actually needed.

---

# Rays / pointers / hit points

If a ray itself must be visualized remotely, convert:

- ray origin as a point (translation + rotation)
- ray direction as a direction (rotation only)

Use `SharedSpaceRayUtility`.

---

# Physics warning

Do not blindly combine this component with Fusion `NetworkRigidbody`/`NetworkRigidbody3D` or another component that also writes the same Transform. Choose one authoritative spatial replication strategy.

For an authoritative Rigidbody prototype:

- State Authority simulates physics
- set `stateAuthorityContinuouslyDrives = true`
- remote clients render the resulting Shared Space pose
- remote Rigidbody should not independently simulate the same dynamic object

A production physics setup should be adapted to the exact Fusion topology and physics components used by your project.

---

# User height / device-origin behavior

Different user height is expected and requires no extra correction.

Example:

- Meta startup/device origin is physically higher
- XREAL startup/device origin is physically lower
- both align the same box

Each device captures a different `SharedOriginInTrackingSpace`, and that vertical difference is naturally included in the rigid transform.

Shared Y=0 means the chosen reference corner's height, not the floor.

---

# Important reliability condition

The device/tracking origin used by `SharedSpaceManager.trackingOrigin` must remain stable after calibration.

If the runtime recenters, relocalizes, or recreates that origin after tracking loss, call:

`SharedSpaceManager.InvalidateCalibration()`

and require the user to align/confirm the box again.

You do not need to keep the physical box tracked continuously during normal operation.

---

# Validation procedure before integrating the entire app

1. Build Meta and XREAL versions from the same Unity project.
2. Put the physical Quest box in one fixed place and do not move it.
3. Start each device from intentionally different positions/heights/directions.
4. Independently align the virtual calibration box on both devices.
5. Confirm on both.
6. Spawn/display a small test marker at Shared `(0,0,0)`. It should land on the chosen physical reference corner for both users.
7. Test markers at:
   - `(0.25,0,0)`
   - `(0,0.25,0)`
   - `(0,0,0.25)`
   to verify axis direction and scale.
8. Test a rotated cube to verify quaternion alignment.
9. Move/grab a networked cube and verify both users see it in the same physical location.
10. Repeat calibration several times and measure the residual error. This tells you how repeatable manual box alignment really is.

If the origin is correct but distant points diverge, suspect rotation error. If all points have a similar constant offset, suspect translation alignment. If distance error grows proportionally, suspect scale/model-dimension mismatch.

---

# Recommended prefab convention

Every spatially shared movable prefab:

```
NetworkObject
SharedSpaceNetworkTransform
SharedSpaceGrabBridge   (if interactable)
Your visual/interaction components
```

Every fixed shared scene object:

```
SharedSpacePlacedObject
Your visual/interaction components
```

Never put local Meta/XREAL tracking coordinates directly into Fusion state for shared spatial content.

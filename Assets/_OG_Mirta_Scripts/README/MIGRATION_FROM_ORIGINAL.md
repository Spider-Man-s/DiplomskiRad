# What the original code did, and what changes now

## Original `QRPlacementTracker`

It sampled the QR object's world position and reduced orientation to one projected yaw angle. That meant calibration did not preserve a general 3D reference quaternion.

## Original `ManualColocationAligner`

It captured local QR position/yaw, then rotated `xrOrigin` around the QR and translated `xrOrigin` so the QR approached world zero. This makes the local XR world imitate a shared world instead of keeping a separate shared coordinate frame.

It also zeroed the vertical correction, which prevented the calibration transform from naturally absorbing different user/device-origin heights.

Its table spawn used `LocalQrPosition + tableOffset` as the spawn pose, so a local headset coordinate could become network spatial state.

## Original `ColocationAligner`

It networked Meta and XREAL marker poses separately, then computed XREAL-to-Meta offsets and moved the XREAL XR origin toward Meta. Meta therefore acted as the reference frame; there was no independent third Shared Space.

## Original `ColocationManager`

It had already moved one step in the right direction by keeping each device's local QR measurement local, but it still represented calibration as position + projected yaw rather than a complete reference pose.

## New architecture

Neither XR origin is moved.

Each device independently stores:

`SharedOriginInTrackingSpace = pose of the aligned SharedReference in that device's stable startup/device-origin coordinates`

The agreed virtual/physical box corner becomes:

- shared position `(0,0,0)`
- shared rotation `identity`

All Fusion spatial content is converted to Shared Space before networking and converted back locally after receiving it.

# Procedural IK Wall-Walking Spider (Computer Graphics Final Project)

A Unity 3D project built for our Rutgers Computer Graphics final project (Fall 2021), featuring a procedurally animated, wall-walking spider navigating a custom Spider-Man–themed Times Square environment, packaged as a playable Unity WebGL build.

> **Upstream attribution:** This project is built on top of [**Unity-Procedural-IK-Wall-Walking-Spider**](https://github.com/PhilS94/Unity-Procedural-IK-Wall-Walking-Spider) by **Philipp Schofield (`PhilS94`)**. The core procedural inverse-kinematics architecture, surface-adhesion mechanics, and base camera/controller scripts originate from that repository.

## What our team built

Building on Philipp Schofield's procedural IK foundation, we created:

- **Custom Spider-Man / Times Square scene (`Assets/Scenes/Spiderman.unity`):** Replaced the upstream test room with a custom environment including the Daily Bugle / J. Jonah Jameson building, Times Square floor, themed billboards and props, trees, and Spider-Man / Goblin elements.
- **Custom spider rig & IK wiring:** Built a custom spider object and configured its legs, `IKChain` assignments, `JointHinge` rotational limits, and `IKStepManager` parameters to work with our rig and scene geometry.
- **NPC, camera, and UI updates:** Added an NPC spider (`SpiderNPCController`), mode-switching and UI updates, and tailored the `CameraAbstract`, `SmoothCamera`, and `SpectatingCamera` scripts for the scene.
- **WebGL build (`Spiderman_web/`):** Resolved WebGL build issues and exported a browser-playable build.

## What comes from upstream

The core procedural animation and wall-walking system comes from [`PhilS94/Unity-Procedural-IK-Wall-Walking-Spider`](https://github.com/PhilS94/Unity-Procedural-IK-Wall-Walking-Spider):

- **Wall-walking & surface adhesion:** Downward and forward sphere-casting with normal-aligned gravity so the spider climbs walls, corners, and ceilings.
- **Procedural leg IK:** Hinge-joint leg chains with rotational limits solved via Cyclic Coordinate Descent (CCD) (`IKSolver`, `IKChain`, `JointHinge`).
- **Step scheduling & target prediction:** Local queue-based and alternating tetrapod gait step management (`IKStepManager`, `IKStepper`) with raycasted surface target prediction.

See the [upstream repository](https://github.com/PhilS94/Unity-Procedural-IK-Wall-Walking-Spider) for Philipp Schofield's full technical breakdown and original showcase.

## Running locally

1. Clone this repository and open the project folder in **Unity**.
2. Open `Assets/Scenes/Spiderman.unity`.
3. Press **Play** to control the spider or toggle NPC mode.

A prebuilt WebGL export is also included under `Spiderman_web/`.

## Attribution & License

- Core procedural IK wall-walking system © **Philipp Schofield** — [`PhilS94/Unity-Procedural-IK-Wall-Walking-Spider`](https://github.com/PhilS94/Unity-Procedural-IK-Wall-Walking-Spider). Per the upstream repository, sublicensing or selling copies in stores such as the Unity Asset Store is prohibited; contact Philipp Schofield (`mail@PhilippSchofield.com`) for commercial licensing.
- This repository is an academic course project created for educational purposes.

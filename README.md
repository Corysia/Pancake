# Pancake

This is a [Unity](https://unity.com/) example project.  It demonstrates how to create a game that can run in either VR or Desktop mode.

**NOTE:** This project is currently illustrating a bug in the XR system where if the XR Rig is disbled, but "Start in VR" is checked in the XR Plugin Management section, the game loop locks up and Update() calls are executed once and once only.  `Assets/Scene/SampleScene` is configured to show the issue I'm running in to.


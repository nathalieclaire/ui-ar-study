# Head-anchored vs Object-anchored AR Study

Prototype developed for a Bachelor Thesis at HTW Berlin.

This repository contains the Unity project for the augmented reality prototype used in the study:

> "Head-anchored vs. Object-anchored Spatial Interfaces: A Comparative Study of Usability, Task Load, and Cognitive Load in Educational Augmented Reality"

The prototype compares static head-anchored and object-anchored instructional UI placements in a structured AR learning scenario.

---

## Hardware Requirements

- Meta Quest 3  
- Passthrough AR enabled  
- Hand tracking enabled  

---

## Software / Engine Version

- Unity 6.0.0 (6000.0.63f1)  
- OpenXR  
- XR Interaction Toolkit  

---

## Experimental Structure

The study followed this sequence:

1. Calibration phase  
2. Onboarding phase (neutral cube interaction)  
3. Anchoring Condition 1 (3 trials)  
4. Questionnaires  
5. Anchoring Condition 2 (3 trials)  

Each condition consisted of three object-centered learning trials involving plant inspection, symbol detection, placement, and a multiple-choice recall task.

**Note on Counterbalancing:**  
This repository contains a single fixed anchoring order (Object-Anchored → Head-Anchored).  
For the counterbalanced condition used in the study, the order of the anchoring phases within the `SceneFlowManager` was manually reversed in a separate local copy of the project and deployed to a second headset. Counterbalancing was therefore implemented at the deployment level rather than dynamically within the application.

---

## Project Architecture (Overview)

Main components include:

- **AppFlowManager** – Handles scene transitions and application state  
- **SceneFlowManager** – Controls trial logic and phase progression  
- **AnchorCalibrationManager** – Stores and reapplies spatial calibration transforms  
- **UI Canvas** – Dynamically re-parented to implement head-anchored vs. object-anchored conditions  

The anchoring manipulation was implemented by re-parenting the UI canvas either to the XR camera (head-anchored) or to the selected object transform (object-anchored).

---

## Third-Party Assets

Some 3D models and audio assets were obtained from licensed sources (e.g., royalty-free libraries). Assets are included for academic review only and remain subject to their respective licenses.

---

## License

This repository is provided for academic and research purposes only.

© Nathalie Claire Huppert – HTW Berlin

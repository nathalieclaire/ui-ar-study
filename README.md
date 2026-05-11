# Head-anchored vs. Object-anchored UI in Educational AR

A research driven AR prototype built for the Meta Quest 3, exploring how spatial interface placement shapes usability, workload, and cognitive load. Developed as my Bachelor's thesis at HTW Berlin (graded 1.0):

> "Head-anchored vs. Object-anchored Spatial Interfaces: A Comparative Study of Usability, Task Load, and Cognitive Load in Educational Augmented Reality"

Supervised by Prof. Dr.-Ing. Nassrin Hajinejad and Prof. Dr.-Ing. Carsten Busch

## What it is

I designed and built a passthrough AR learning prototype on Meta Quest 3 to compare two ways of placing instructional UI in space: anchored to the user's head, or anchored to the object being learned about. 17 participants tested both conditions in a within subject study. **Object anchored placement scored significantly higher on usability (SUS, p = .03)**, with consistent directional advantages on workload and extraneous cognitive load.

The scenario itself: an object centered AR learning task in which users inspect, manipulate, and categorize virtual plants while reading instructional content delivered through a spatial UI. The same task ran under two anchoring conditions, with all other variables held constant. UI design, content, interaction model, feedback, and assessment were identical. Only the _spatial attachment_ of the canvas differed:

**Head anchored**: UI follows the camera, always within FOV.

**Object anchored**: UI billboarded to the manipulated object, scaling with distance.

<table>
  <tr>
    <td width="50%"><img src="docs/object_anchored.png" alt="Object-anchored condition" width="100%"></td>
    <td width="50%"><img src="docs/head_anchored.png" alt="Head-anchored condition" width="100%"></td>
  </tr>
</table>

## Why it matters

Most prior research on spatial UI placement focuses on _adaptive_ or _dynamic_ anchoring in productivity, navigation, or VR contexts. Static anchoring strategies, which are predictable, lower complexity, and widely used in real world AR, remain underexplored, especially in **passthrough AR with hand tracking and 3D object manipulation**. This study isolates anchoring as the only variable, giving a controlled empirical baseline for educational AR design.

## Key Findings

| Measure                   | Object anchored | Head anchored | Result                                          |
| ------------------------- | --------------- | ------------- | ----------------------------------------------- |
| Usability (SUS)           | 76.0            | 70.6          | **Significant**, p = .03, η²p = .27             |
| Task Load (NASA-TLX)      | 25.8            | 31.2          | Directional, p = .14 (sig. under winsorization) |
| Extraneous Cognitive Load | 2.43            | 2.84          | Directional, p = .12                            |
| Intrinsic Cognitive Load  | 2.77            | 2.82          | Stable (as expected)                            |
| Germane Cognitive Load    | 3.00            | 3.27          | n.s., p = .38                                   |

**Takeaway for designers**: when learners need to inspect and manipulate objects, attaching instructional content to the object itself outperforms persistent head fixed overlays, not just statistically, but in qualitative reports of comfort, urgency, and attentional control. Head anchored UI was repeatedly described as "in my face" and "rushed"; object anchored UI as "relaxed" and "easier to remember alongside the plant."

## Design Decisions

A few interaction and UX choices that shaped the prototype:

- **Direct hand interaction only** (no ray casting): preserves embodied manipulation and forces locomotion, which keeps element interactivity high enough for cognitive load differences to surface.

- **Proximity based UI activation**: instructional content fades in only when the user approaches the relevant object, applying spatial temporal contiguity to reduce split attention.

- **Multimodal feedback**: spatial 3D audio for in scene events, 2D audio for UI confirmations, color semantics (green for correct, red for error) to compensate for the absence of haptics.

- **Identical visual styling across conditions**: semi transparent blue gray background, white text, WCAG AA contrast targets, Inter as the typeface. Only the _anchor_ differs, not the look.

- **Calibration based anchoring**: solved unstable AR Foundation anchor persistence across scenes by refactoring to a single scene architecture with experimenter driven spatial calibration before each session.

- **Scaffolded onboarding** with a neutral cube and world anchored UI, deliberately neutral toward both experimental conditions.

## Architecture & Experimental Structure

A centralized, manager based Unity architecture keeps experimental logic separated from scene objects. Scene objects are intentionally passive and event driven; control flow is centralized.

![System architecture](docs/system_architecture.png)

**AppFlowManager**: application level state, scene transitions.

**SceneFlowManager**: phase progression, trial logic, UI state, completion criteria.

**AnchorCalibrationManager**: stores and reapplies real world calibration transforms.

**XR Rig (XR Origin Hands)**: head and hand tracking, OpenXR runtime.

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

## Study at a Glance

Within subject design, N = 17, counterbalanced.

Measures: NASA-TLX (RTLX), Klepsch et al. cognitive load questionnaire (intrinsic, extraneous, germane), System Usability Scale (SUS), open ended qualitative responses.

![Measurement framework](docs/measurement_framework.png)

Pilot tested (N = 2) before main study to validate procedure and instrument sensitivity.

Statistical analysis with repeated measures ANOVAs, robustness checks via 5th to 95th percentile winsorization.

## Limitations and Honest Notes

Sample homogeneous (university students, low to moderate XR experience), so generalizability is bounded. Sample size limits statistical power; some findings are sensitive to outlier treatment. Subjective measures only, no eye tracking or behavioral logging in this iteration. Static anchoring was deliberately chosen for experimental control; real world systems likely benefit from hybrid or adaptive approaches.

These aren't deflections, they're the natural next steps the thesis itself outlines.

## Hardware Requirements

- Meta Quest 3
- Passthrough AR enabled
- Hand tracking enabled

## Software / Engine Version

- Unity 6.0.0 (6000.0.63f1)
- OpenXR
- XR Interaction Toolkit

## A Note on Removed Assets

The original `Assets/Models/` folder of this project contained 3D models obtained from licensed sources (royalty-free libraries) that do not permit public redistribution. To make this repository publicly available while respecting these licenses, the entire `Assets/Models/` directory has been retroactively removed from the full Git history.

As a result, opening this project in Unity will produce missing-reference warnings for the affected scenes and prefabs. The project structure, code, scene logic, and study flow remain fully intact and documented. To run the prototype, the missing models can be replaced with any equivalent assets (own work, CC0 sources, or alternative licensed packs) by placing them in `Assets/Models/` and reassigning the references in the relevant scenes.

## License

This repository is provided for academic and research purposes only.

© Nathalie Claire Huppert - HTW Berlin

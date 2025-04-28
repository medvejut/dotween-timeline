# DOTween Timeline
[![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](LICENSE)

A pocket timeline solution for DOTween Pro. Configure and organize complex tween animations directly in the Inspector.

![ezgif-478bb6b997c38b](https://github.com/user-attachments/assets/1cc3d251-d4a8-476a-9dc5-0b43ebe395d4)

## Installation
> [!IMPORTANT]
> **Required**: [**PRO**](https://dotween.demigiant.com/pro.php) version of DOTween.

### Releases page
Easiest way is to install DOTween Timeline as an asset package.
1. Download the latest ```.unitypackage``` file from the [Releases page](https://github.com/medvejut/dotween-timeline/releases).
2. Import it into your project via **Assets > Import Package > Custom Package**.

### Git UPM
You can also install this package via Git URL using Unity Package Manager.
1. Since DOTween is not distributed as a upm package by default, you need to manually generate `.asmdef` files for it:\
  Open **Tools > Demigiant > DOTween Utility Panel**, click **Create ASMDEF**
2. Then, add the following line to your `Packages/manifest.json`:
```
"com.medvejut.dotweentimeline": "https://github.com/medvejut/dotween-timeline.git#upm"
```

## How to use
Add the **DOTween > DOTween Timeline** component to a GameObject (use a separate GameObject for each animation sequence).

Control the timeline from code:

```c#
[SerializeField] private DOTweenTimeline timeline;

var tween = timeline.Play();

tween.OnComplete(() => Debug.Log("OnComplete"));
tween.Pause();
```

### Sample
A sample scene is included to help you get started. You can find it here: `Plugins/DOTweenTimeline/Sample`\
Open it to see an example of how to configure and use the timeline in practice.
<details>
  <summary>To make the sample scene work, enable TextMeshPro support in DOTween and set up TMP.</summary>
  
  1. Go to **Tools > Demigiant > DOTween Utility Panel**, press **"Setup DOTween..."**, enable **TextMeshPro**:
  ![TextMeshProSupport](https://github.com/user-attachments/assets/1674e9e9-ac6c-4b73-a278-37a548806a23)
  2. **Window > TextMeshPro > Import TMP Essential Resources**
</details>

## Recommendations

#### 1. Disable default DOTween Pro preview controls
In the Inspector, on any DOTween animation component:

![tips_preview_controls](https://github.com/user-attachments/assets/e8e3c39e-a1b0-4d4a-bd2d-de2af567eca7)

#### 2. Use a separate GameObject for each animation sequence
![tips_separate_go](https://github.com/user-attachments/assets/7fa9e9b5-d1af-4f2e-9b0e-28d9576d2198)


## Extras
### DOTween Timeline Player component
Automatically plays animations without code.\
Just add the **DOTween > DOTween Timeline Player** component to the same GameObject with the Timeline.

### Extra Actions
In addition to standard tweens, you can add special timeline actions via the Add dropdown:\
![Mask group (1)](https://github.com/user-attachments/assets/dc48d249-56f2-41cb-8259-b6aa8db3e46e)

#### DOTweenCallback component
A visual replacement for Sequence.InsertCallback().\
Use the `onCallback` UnityEvent in the Inspector or from code:
```c#
[SerializeField] private DOTweenCallback callback;

callback.onCallback.AddListener(() => Debug.Log("Callback"));
```
<img width="477" src="https://github.com/user-attachments/assets/746fca7e-1d70-4127-ba92-330c0f7470e6" />

#### DOTweenFrame component
Triggers immediate state changes in a single frame.\
Perfect for setting position, rotation, color, and more, without animation.

<img width="490" src="https://github.com/user-attachments/assets/df9226e8-dc83-419b-b1ca-daaf6b70811a" />

## Contributing
Please report bugs in [Issues](https://github.com/medvejut/dotween-timeline/issues).\
Feel free to ask questions and share thoughts in [Discussions](https://github.com/medvejut/dotween-timeline/discussions).

**This project is inspired by:**\
[Animation Creator Timeline (UI Tween)](https://assetstore.unity.com/packages/tools/animation/animation-creator-timeline-ui-tween-186589)\, [DOTween Timeline Preview](https://www.youtube.com/watch?v=hrX0xZ3JCXU) & [Jitter](https://jitter.video/)

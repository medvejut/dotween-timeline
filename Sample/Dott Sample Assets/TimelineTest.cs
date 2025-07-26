using DG.Tweening;
using UnityEngine;

namespace Dott.Sample
{
    public class TimelineTest : MonoBehaviour
    {
        [SerializeField] private DOTweenTimeline timeline;
        [SerializeField] private DOTweenCallback callback;

        private void Start()
        {
            timeline.Play().SetLoops(-1);

            callback.onCallback.AddListener(() =>
            {
                Debug.Log("Callback triggered!");
            });
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                timeline.Sequence.Restart();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                timeline.Sequence.TogglePause();
            }
        }
    }
}
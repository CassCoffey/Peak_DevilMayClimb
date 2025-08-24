using UnityEngine;
using UnityEngine.UI;

namespace DevilMayClimb.Monobehavior
{
    public class FillBounce : MonoBehaviour
    {
        private AnimationCurve fillCurve;
        private float fillDuration = 0.5f;

        private Image image;

        private float goalFill = 0f;
        private float currentFill = 0f;
        private float fillStart = 0f;

        public void Start()
        {
            image = GetComponent<Image>();

            // You like magic numbers?
            Keyframe[] fillKeys = new Keyframe[3];

            fillKeys[0] = new Keyframe(0f, 0f, 0f, 1.25f, 0f, 0f);
            fillKeys[0].weightedMode = WeightedMode.None;

            fillKeys[1] = new Keyframe(0.8f, 1f, -0.5780662f, -0.5780662f, 0.09657286f, 0f);
            fillKeys[1].weightedMode = WeightedMode.None;

            fillKeys[2] = new Keyframe(1f, 1f, 0, 0, 0f, 0f);
            fillKeys[2].weightedMode = WeightedMode.None;

            fillCurve = new AnimationCurve(fillKeys);
        }

        public void SetGoal(float newGoal)
        {
            goalFill = newGoal;
            fillStart = Time.time;
            currentFill = image.fillAmount;
        }

        public void Update()
        {
            if (Time.time - fillStart <= fillDuration)
            {
                image.fillAmount = currentFill + fillCurve.Evaluate((Time.time - fillStart) / fillDuration) * (goalFill - currentFill);
            }
        }
    }
}
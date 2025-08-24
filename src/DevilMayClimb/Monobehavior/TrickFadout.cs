using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DevilMayClimb.Monobehavior
{
    public class TrickFadout : MonoBehaviour
    {
        public float lifeTime = 4f;
        public float fadoutTime = 1f;

        public void Update()
        {
            lifeTime -= Time.deltaTime;

            if (lifeTime <= 0f)
            {
                if (lifeTime <= 0f - fadoutTime)
                {
                    Destroy(gameObject);
                    return;
                }

                GetComponent<CanvasGroup>().alpha = Mathf.Lerp(1f, 0f, MathF.Abs(lifeTime) / fadoutTime);
            }
        }
    }
}

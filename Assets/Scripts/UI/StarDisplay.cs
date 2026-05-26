using UnityEngine;

namespace VRLearning.UI
{
    /// <summary>
    /// Shows 0-3 stars after a puzzle is completed.
    /// Stars animate in one at a time with a scale bounce.
    /// </summary>
    public class StarDisplay : MonoBehaviour
    {
        [SerializeField] private GameObject[] stars;
        [SerializeField] private AudioClip    starSFX;
        [SerializeField] private float        starDelay = 0.4f;

        public void Show(int count)
        {
            foreach (var s in stars) s.SetActive(false);
            StartCoroutine(AnimateStars(count));
        }

        private System.Collections.IEnumerator AnimateStars(int count)
        {
            for (int i = 0; i < Mathf.Clamp(count, 0, stars.Length); i++)
            {
                yield return new WaitForSeconds(starDelay);
                stars[i].SetActive(true);
                stars[i].transform.localScale = Vector3.zero;
                Core.AudioManager.Instance?.PlaySFX(starSFX);
                float t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime * 4f;
                    stars[i].transform.localScale = Vector3.LerpUnclamped(
                        Vector3.zero, Vector3.one,
                        EaseOutBack(t));
                    yield return null;
                }
                stars[i].transform.localScale = Vector3.one;
            }
        }

        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f, c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}

using System.Collections;
using UnityEngine;

namespace Sweet_And_Salty_Studios
{
    public class Effect : MonoBehaviour
    {
        #region VARIABLES

        private Vector2 desiredSize;
        private Color32 targetColor;
        private Color32 defaultColor;

        private SpriteRenderer spriteRenderer;
        private AudioSource audioSource;
        private Coroutine iStartEffectCoroutine;

        #endregion VARIABLES

        #region UNITY_FUNCTIONS

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            audioSource = GetComponent<AudioSource>();

            defaultColor = targetColor = spriteRenderer.color;
            targetColor.a = 0;
        }

        private void OnEnable()
        {
            transform.localScale = GameManager.Instance.CurrentMoveBounds;
            desiredSize = transform.localScale * 2f;

            audioSource.pitch = Random.Range(0.9f, 1.1f);
            spriteRenderer.color = defaultColor;

            if(iStartEffectCoroutine == null)
            {
                iStartEffectCoroutine = StartCoroutine(IStartEffect());
            }
        }

        private void OnDisable()
        {
            if(iStartEffectCoroutine != null)
            {
                StopCoroutine(iStartEffectCoroutine);
                iStartEffectCoroutine = null;
            }          
        }

        #endregion UNITY_FUNCTIONS

        #region CUSTOM_FUNCTIONS

        private IEnumerator IStartEffect(float lerpSpeed = 10f)
        {
            var startTime = Time.time;
            var journeyLength = Vector3.Distance(transform.localScale, desiredSize);
            
            audioSource.Play();

            while(transform.localScale.sqrMagnitude < desiredSize.sqrMagnitude)
            {
                var distanceCovered = (Time.time - startTime) * lerpSpeed;
                var fraction = distanceCovered / journeyLength;

                transform.localScale = Vector2.Lerp(transform.localScale, desiredSize, fraction);

                //spriteRenderer.size = transform.localScale * 0.052f;

                spriteRenderer.color = Color.Lerp(spriteRenderer.color, targetColor, fraction);

                yield return null;
            }

            yield return new WaitUntil(() => audioSource == null || audioSource.isPlaying == false);

            ObjectPoolManager.Instance.Despawn(this);
        }

        #endregion CUSTOM_FUNCTIONS
    }
}
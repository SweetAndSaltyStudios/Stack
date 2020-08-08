using System.Collections;
using UnityEngine;

namespace Sweet_And_Salty_Studios
{
    public class CameraEngine : Singelton<CameraEngine>
    {
        #region VARIABLES

        private float power = 0.25f;
        private float duration = 2f;
        private readonly float slowDoenAmount = 1.0f;
        private bool shouldShake = false;
        private Vector3 startPosition;
        private float initialDuration;
        private Camera mainCamera;
        private Coroutine iShakeCamera_Coroutine;
        private AudioSource audioSource;

        #endregion VARIABLES

        #region UNITY_FUNCTIONS

        private void Awake()
        {
            mainCamera = GetComponent<Camera>();
            audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            startPosition = mainCamera.transform.localPosition;
            initialDuration = duration;
            audioSource.loop = true;
        }

        #endregion UNITY_FUNCTIONS

        #region CUSTOM_FUNCTIONS

        public void ShakeCamera()
        {
            if(iShakeCamera_Coroutine != null)
            {
                StopCoroutine(iShakeCamera_Coroutine);
            }

            iShakeCamera_Coroutine = StartCoroutine(IShakeCamera());
        }

        private IEnumerator IShakeCamera()
        {
            shouldShake = true;
            audioSource.Play();

            while(shouldShake)
            {
                if(duration > 0)
                {
                    mainCamera.transform.localPosition = startPosition + Random.insideUnitSphere * power;
                    duration -= Time.deltaTime * slowDoenAmount;
                }
                else
                {
                    shouldShake = false;
                    duration = initialDuration;
                    mainCamera.transform.localPosition = startPosition;
                }

                yield return null;
            }

            audioSource.Stop();
        }

        #endregion CUSTOM_FUNCTIONS
    }
}
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sweet_And_Salty_Studios
{
    public class UIManager : Singelton<UIManager>
    {
        #region VARIABLES

        [SerializeField] private TextMeshProUGUI mainText = null;
        [SerializeField] private TextMeshProUGUI bestText = null;
        //[SerializeField] private Image backgroundImage = null;
        [SerializeField] private Image fadeImage = null;
        private Coroutine iFade_Coroutine;
        private Camera mainCamera;

        #endregion VARIABLES

        #region UNITY_FUNCTIONS

        private void Awake()
        {
            mainCamera = Camera.main;

            mainText.enabled = false;
        }

        #endregion UNITY_FUNCTIONS

        #region CUSTOM_FUNCTIONS

        public void UpdateScore(int scoreToDesplay)
        {
            mainText.text = scoreToDesplay.ToString();
        }

        public void UpdateBestScore(int score)
        {
            bestText.text = score.ToString();
        }

        public void Fade(bool fadeToBlack, float targetFillAmount, float fadeSpeed = 1f)
        {
            mainText.enabled = mainText.enabled == false;

            if(iFade_Coroutine != null)
            {
                StopCoroutine(iFade_Coroutine);
            }

            iFade_Coroutine = StartCoroutine(IFade(fadeToBlack, targetFillAmount, fadeSpeed));
        }

        private IEnumerator IFade(bool fadeToBlack, float targetFillAmount, float fadeSpeed)
        {
            var startTime = Time.time;

            fadeImage.fillAmount = fadeToBlack ? 0 : 1;

            fadeImage.fillMethod = (Image.FillMethod)Random.Range(0, 4);
            fadeImage.fillOrigin = Random.Range(0,4);

            while(targetFillAmount != fadeImage.fillAmount)
            {               
                var fraction = (Time.time - startTime) * fadeSpeed / Mathf.Abs(fadeImage.fillAmount - targetFillAmount);

                fadeImage.fillAmount = Mathf.Lerp(fadeImage.fillAmount, targetFillAmount, fraction);

                yield return null;
            }
        }

        #endregion CUSTOM_FUNCTIONS
    }
}
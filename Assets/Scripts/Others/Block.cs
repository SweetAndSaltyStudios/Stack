using System.Collections;
using UnityEngine;

namespace Sweet_And_Salty_Studios
{
    public class Block : MonoBehaviour
    {
        #region VARIABLES

        private Rigidbody rb;
        private MeshFilter meshFilter;
        private Renderer myRenderer;
        private Coroutine iOnEnable_Coroutine;
        private Vector3 defaultPosition;
        private Vector3 defaultScale;

        private AudioSource audioSource;

        #endregion VARIABLES

        #region PROPERTIES

        public Mesh Mesh
        {
            get
            {
                return meshFilter.mesh;
            }
        }

        #endregion PROPERTIES

        #region UNITY_FUNCTIIONS

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            OnActive();
        }

        private void OnDisable()
        {
            OnInactive();
        }

        #endregion UNITY_FUNCTIIONS

        #region CUSTOM_FUNCTIIONS

        private void Initialize()
        {
            rb = GetComponent<Rigidbody>();
            meshFilter = GetComponentInChildren<MeshFilter>();
            myRenderer = GetComponentInChildren<Renderer>();
            audioSource = GetComponent<AudioSource>();

            defaultPosition = transform.localPosition;
            defaultScale = transform.localScale;
        }       

        private void OnActive()
        {
            GameManager.Instance.ActiveBlocks.Push(this);

            if(iOnEnable_Coroutine != null)
            {
                StopCoroutine(iOnEnable_Coroutine);
            }

            iOnEnable_Coroutine = StartCoroutine(IOnEnable());
        }

        private void OnInactive()
        {
            GameManager.Instance.ActiveBlocks.Pop();

            ResetBlock();
        }

        private IEnumerator IOnEnable()
        {
            yield return new WaitUntil(() => transform.position.y <= -30);

            ObjectPoolManager.Instance.Despawn(this);
        }

        private void ResetBlock()
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.transform.SetPositionAndRotation(defaultPosition, Quaternion.Euler(Vector3.zero));
            transform.localScale = defaultScale;
        }

        public void LockBlock()
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        public void UnlockBlock()
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;
        }

        public void PlaySfx()
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.Play();
        }

        #endregion CUSTOM_FUNCTIIONS
    }
}
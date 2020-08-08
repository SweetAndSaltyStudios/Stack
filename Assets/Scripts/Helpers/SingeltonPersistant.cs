using UnityEngine;

namespace Sweet_And_Salty_Studios
{
    public abstract class SingeltonPersistant<T> : MonoBehaviour where T : Component
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = FindObjectOfType<T>();
                    if(instance == null)
                    {
                        var gameObject = new GameObject
                        {
                            hideFlags = HideFlags.HideAndDontSave
                        };
                        instance = gameObject.AddComponent<T>();

                    }
                }
                return instance;
            }
            set
            {
                instance = value;
            }
        }

        protected virtual void Awake()
        {
            DontDestroyOnLoad(this);

            if(instance == null)
            {
                instance = this as T;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
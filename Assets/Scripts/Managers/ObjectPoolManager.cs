using System.Collections.Generic;
using UnityEngine;

namespace Sweet_And_Salty_Studios
{
    public class ObjectPoolManager : Singelton<ObjectPoolManager>
    {
        private Dictionary<string, Stack<Component>> pooledObjects = new Dictionary<string, Stack<Component>>();

        public T SpawnObject<T>(T prefab, Vector3 position = new Vector3(), Quaternion rotation = new Quaternion()) where T : Component
        {
            if(pooledObjects.ContainsKey(prefab.name))
            {
                if(pooledObjects[prefab.name].Count > 0)
                {
                    var prefabInstance = pooledObjects[prefab.name].Pop();
                    prefabInstance.transform.SetPositionAndRotation(position, rotation);
                    prefabInstance.gameObject.SetActive(true);
                    return prefabInstance as T;
                }
                else
                {
                    return CreatePrefabInstance(prefab, position, rotation);
                }
            }
            else
            {
                pooledObjects.Add(prefab.name, new Stack<Component>());
                return CreatePrefabInstance(prefab, position, rotation);
            }         
        }

        public void Despawn(Component instance)
        {
            instance.gameObject.SetActive(false);

            if(pooledObjects.ContainsKey(instance.name))
            {
                pooledObjects[instance.name].Push(instance);
            }
            else
            {
                Debug.LogWarning("We did not have key " + instance.name + " to store the object");
                pooledObjects.Add(instance.name, new Stack<Component>());
                pooledObjects[instance.name].Push(instance);
            }
        }

        private T CreatePrefabInstance<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            var foo = Instantiate(prefab, position, rotation);
            foo.name = prefab.name;
            return foo;
        }
    }
}

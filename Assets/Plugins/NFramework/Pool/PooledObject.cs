using UnityEngine;

namespace NFramework
{
    public class PooledObject : MonoBehaviour
    {
        private Pool _pool;

        public Pool Pool
        {
            get => _pool;
            set
            {
                if (_pool == null)
                    _pool = value;
                else
                    Debug.LogError($"Cannot set pool, because it had already set");
            }
        }

        protected virtual void OnDestroy()
        {
            if (Pool)
                Pool.HandlePooledObjectOnDestroy(this);
        }

        public void ReturnToPool()
        {
            if (Pool)
            {
                Pool.ReturnToPool(this);
            }
            else
            {
                Debug.LogError($"Pool is null => Destroy!");
                Destroy(gameObject);
            }
        }
    }
}
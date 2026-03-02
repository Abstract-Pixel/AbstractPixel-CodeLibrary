using UnityEngine;

namespace AbstractPixel.Core
{
    public interface ICopyable<T> where T : class
    {
        public void CopyReferencesFrom(T source);

    }
}

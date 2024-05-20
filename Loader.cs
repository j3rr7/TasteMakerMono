using UnityEngine;

namespace TasteMakerMono
{
    public class Loader : MonoBehaviour
    {
        public static bool initialized;
        public static GameObject _gameObject { get; private set; }

        public static void Load()
        {
            _gameObject = new GameObject("TrainerModule");

            _gameObject.AddComponent<CustomConsole>();
            _gameObject.AddComponent<Cheat>();

            DontDestroyOnLoad(_gameObject);
        }

        public static void Unload()
        {
            if (_gameObject)
                DestroyImmediate(_gameObject);
            _gameObject = null;
        }
    }
}

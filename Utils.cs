using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace TasteMakerMono
{
    public static class Utils
    {
        /// <summary>
        /// Finds an object of the specified type with retries.
        /// </summary>
        /// <typeparam name="T">The type of object to find.</typeparam>
        /// <param name="caller">The MonoBehaviour that is calling this method.</param>
        /// <param name="maxAttempts">The maximum number of attempts to find the object.</param>
        /// <param name="delayInSeconds">The delay in seconds between each attempt.</param>
        /// <param name="onFound">A callback action to invoke when the object is found.</param>
        /// <returns>An IEnumerator that can be used with StartCoroutine.</returns>
        public static IEnumerator FindObjectOfTypeWithRetries<T>(int maxAttempts, float delayInSeconds, Action<T> onFound) where T : Component
        {
            int attempts = 0;
            T foundObject = null;

            while (attempts < maxAttempts && foundObject == null)
            {
                foundObject = UnityEngine.Object.FindObjectOfType<T>();
                if (foundObject != null)
                {
                    onFound?.Invoke(foundObject);
                    yield break;
                }

                attempts++;
                yield return new WaitForSeconds(delayInSeconds);
            }

            if (foundObject == null)
            {
                Debug.LogError($"Failed to find object of type {typeof(T).Name} after {maxAttempts} attempts.");
            }
        }

        /// <summary>
        /// Find and return an object of type T using Unity's FindObjectOfType method
        /// If the object is not found, log an error message
        /// </summary>
        /// <typeparam name="T">The type of object to find</typeparam>
        /// <returns>The object of type T if found, otherwise null</returns>
        public static T FindObjectOfTypeImmediately<T>() where T : Component
        {
            T component = UnityEngine.Object.FindObjectOfType<T>();
            if (component == null)
            {
                Debug.LogError($"{typeof(T).Name} object not found!");
            }
            return component;
        }

        /// <summary>
        /// Returns a _private_ Property Value from a given Object. Uses Reflection.
        /// Throws a ArgumentOutOfRangeException if the Property is not found.
        /// </summary>
        /// <typeparam name="T">Type of the Property</typeparam>
        /// <param name="obj">Object from where the Property Value is returned</param>
        /// <param name="propName">Propertyname as string.</param>
        /// <returns>PropertyValue</returns>
        public static T GetPrivatePropertyValue<T>(this object obj, string propName)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            PropertyInfo pi = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (pi == null) throw new ArgumentOutOfRangeException("propName", string.Format("Property {0} was not found in Type {1}", propName, obj.GetType().FullName));
            return (T)pi.GetValue(obj, null);
        }

        /// <summary>
        /// Returns a private Property Value from a given Object. Uses Reflection.
        /// Throws a ArgumentOutOfRangeException if the Property is not found.
        /// </summary>
        /// <typeparam name="T">Type of the Property</typeparam>
        /// <param name="obj">Object from where the Property Value is returned</param>
        /// <param name="propName">Propertyname as string.</param>
        /// <returns>PropertyValue</returns>
        public static T GetPrivateFieldValue<T>(this object obj, string propName)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            Type t = obj.GetType();
            FieldInfo fi = null;
            while (fi == null && t != null)
            {
                fi = t.GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                t = t.BaseType;
            }
            if (fi == null) throw new ArgumentOutOfRangeException("propName", string.Format("Field {0} was not found in Type {1}", propName, obj.GetType().FullName));
            return (T)fi.GetValue(obj);
        }

        /// <summary>
        /// Sets a _private_ Property Value from a given Object. Uses Reflection.
        /// Throws a ArgumentOutOfRangeException if the Property is not found.
        /// </summary>
        /// <typeparam name="T">Type of the Property</typeparam>
        /// <param name="obj">Object from where the Property Value is set</param>
        /// <param name="propName">Propertyname as string.</param>
        /// <param name="val">Value to set.</param>
        /// <returns>PropertyValue</returns>
        public static void SetPrivatePropertyValue<T>(this object obj, string propName, T val)
        {
            Type t = obj.GetType();
            if (t.GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) == null)
                throw new ArgumentOutOfRangeException("propName", string.Format("Property {0} was not found in Type {1}", propName, obj.GetType().FullName));
            t.InvokeMember(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance, null, obj, new object[] { val });
        }

        /// <summary>
        /// Set a private Property Value on a given Object. Uses Reflection.
        /// </summary>
        /// <typeparam name="T">Type of the Property</typeparam>
        /// <param name="obj">Object from where the Property Value is returned</param>
        /// <param name="propName">Propertyname as string.</param>
        /// <param name="val">the value to set</param>
        /// <exception cref="ArgumentOutOfRangeException">if the Property is not found</exception>
        public static void SetPrivateFieldValue<T>(this object obj, string propName, T val)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            Type t = obj.GetType();
            FieldInfo fi = null;
            while (fi == null && t != null)
            {
                fi = t.GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                t = t.BaseType;
            }
            if (fi == null) throw new ArgumentOutOfRangeException("propName", string.Format("Field {0} was not found in Type {1}", propName, obj.GetType().FullName));
            fi.SetValue(obj, val);
        }
    }
}

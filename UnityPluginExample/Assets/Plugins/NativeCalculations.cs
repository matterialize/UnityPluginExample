using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Plugins.NativeCalculations
{
    [Serializable]
    public class CalculationResults
    {
        public float diagonal;
        public int perimeter;
        public int area;
    }

    /*
     * ---------------------------
     * Static class for easy calls
     * ---------------------------
     */

    public static class NativeCalculations
    {
        // Game object is created to receive async messages
        private const string GAME_OBJECT_NAME = "PluginBridge";
        private static GameObject gameObject;

        // Android only variables
        private const string JAVA_OBJECT_NAME = "com.example.nativecalculations.NativeCalculationsPlugin";
        private static AndroidJavaObject androidJavaNativeCalculation;

        // iOS only variables
        #if UNITY_IOS
        [DllImport("__Internal")]
        #endif
        private static extern string syncCalculation(int rectangleHeight, int rectangleWidth);
        #if UNITY_IOS
        [DllImport("__Internal")]
        #endif
        private static extern void asyncCalculation(int rectangleHeight, int rectangleWidth);

        // Save a reference of the callback to pass async messages
        private static Action<CalculationResults> handleAsyncCalculation;

        // Default error message
        private class PlatformNotSupportedException : Exception
        {
            public PlatformNotSupportedException() : base() { }
        }

        static NativeCalculations()
        {

            // Create Game Object to allow sending messages from Java or Objective C to Unity
            gameObject = new GameObject();

            // Object name must match UnitySendMessage call in Java or Objective C
            gameObject.name = GAME_OBJECT_NAME;

            // Attach this class to allow for handling of callbacks from Java or Objective C
            gameObject.AddComponent<NativeCalculationsCallbackHandler>();

            // Do not destroy when loading a new scene
            UnityEngine.Object.DontDestroyOnLoad(gameObject);

            // Initialize Plugin
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    var androidJavaUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");

                    // Current activity does not change for Unity
                    var currentActivity = androidJavaUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                    // Initialize native Java object
                    androidJavaNativeCalculation = new AndroidJavaObject(JAVA_OBJECT_NAME, currentActivity);
                    break;

                case RuntimePlatform.IPhonePlayer:
                    // No initialization needed
                    break;

                default:
                    throw new PlatformNotSupportedException();
            }
        }

        /*
         * -----------------
         * Interface Methods
         * -----------------
         */

        public static CalculationResults PerformSyncCalculation(int rectangleHeight, int rectangleWidth)
        {
            string calculationsResultsJson;
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    calculationsResultsJson = androidJavaNativeCalculation.Call<string>("syncCalculation", rectangleHeight, rectangleWidth);
                    return JsonUtility.FromJson<CalculationResults>(calculationsResultsJson);

                case RuntimePlatform.IPhonePlayer:
                    calculationsResultsJson = syncCalculation(rectangleHeight, rectangleWidth);
                    return JsonUtility.FromJson<CalculationResults>(calculationsResultsJson);

                default:
                    throw new PlatformNotSupportedException();
            }
        }

        public static void PerformAsyncCalculation(int rectangleHeight, int rectangleWidth, Action<CalculationResults> handleAsyncCalculation)
        {
            NativeCalculations.handleAsyncCalculation = handleAsyncCalculation;

            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    androidJavaNativeCalculation.Call("asyncCalculation", rectangleHeight, rectangleWidth);
                    break;

                case RuntimePlatform.IPhonePlayer:
                    asyncCalculation(rectangleHeight, rectangleWidth);
                    break;

                default:
                    throw new PlatformNotSupportedException();
            }
        }

        /*
         * -----------------------------
         * Native Async Callback Handler
         * -----------------------------
         */

        private class NativeCalculationsCallbackHandler : MonoBehaviour
        {
            private void HandleException(string exception)
            {
                throw new Exception(exception);
            }

            private void HandleAsyncCalculation(string calculationResultsJSON)
            {
                var calculationResults = JsonUtility.FromJson<CalculationResults>(calculationResultsJSON);
                handleAsyncCalculation?.Invoke(calculationResults);
            }
        }
    }
}
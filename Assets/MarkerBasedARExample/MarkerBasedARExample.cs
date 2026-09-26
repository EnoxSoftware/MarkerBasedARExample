using OpenCVForUnity.CoreModule;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MarkerBasedARExample
{
    /// <summary>
    /// MarkerBasedAR Example
    /// </summary>
    public class MarkerBasedARExample : MonoBehaviour
    {
        public Text exampleTitle;
        public Text versionInfo;
        public ScrollRect scrollRect;
#if UNITY_6000_5_OR_NEWER
        [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
#endif
        private static float verticalNormalizedPosition = 1f;

        // Use this for initialization
        private void Start()
        {
            exampleTitle.text = "MarkerBasedAR Example " + Application.version;

            versionInfo.text = Core.NATIVE_LIBRARY_NAME + " " + OpenCVForUnity.UnityIntegration.OpenCVForUnityEnv.GetVersion() + " (" + Core.VERSION + ")";
            versionInfo.text += " / UnityEditor " + Application.unityVersion;
            versionInfo.text += " / ";

#if UNITY_EDITOR
            versionInfo.text += "Editor";
#elif UNITY_STANDALONE_WIN
            versionInfo.text += "Windows";
#elif UNITY_STANDALONE_OSX
            versionInfo.text += "Mac OSX";
#elif UNITY_STANDALONE_LINUX
            versionInfo.text += "Linux";
#elif UNITY_ANDROID
            versionInfo.text += "Android";
#elif UNITY_IOS
            versionInfo.text += "iOS";
#elif UNITY_WSA
            versionInfo.text += "WSA";
#elif UNITY_WEBGL
            versionInfo.text += "WebGL";
#endif
            versionInfo.text += " ";
#if ENABLE_MONO
            versionInfo.text += "Mono";
#elif ENABLE_IL2CPP
            versionInfo.text += "IL2CPP";
#elif ENABLE_DOTNET
            versionInfo.text += ".NET";
#endif

            scrollRect.verticalNormalizedPosition = verticalNormalizedPosition;
        }

        // Update is called once per frame
        private void Update()
        {

        }

        public void OnScrollRectValueChanged()
        {
            verticalNormalizedPosition = scrollRect.verticalNormalizedPosition;
        }

        public void OnShowLicenseButtonClick()
        {
            SceneManager.LoadScene("ShowLicense");
        }

        public void OnShowARMarkerButtonClick()
        {
            SceneManager.LoadScene("ShowARMarker");
        }

        public void OnTexture2DMarkerBasedARExampleButtonClick()
        {
            if (GraphicsSettings.currentRenderPipeline == null)
            {
                SceneManager.LoadScene("Texture2DMarkerBasedARExample_Built-in");
            }
            else
            {
                SceneManager.LoadScene("Texture2DMarkerBasedARExample_SRP");
            }
        }

        public void OnMultiSourceMarkerBasedARExampleButtonClick()
        {
            if (GraphicsSettings.currentRenderPipeline == null)
            {
                SceneManager.LoadScene("MultiSourceMarkerBasedARExample_Built-in");
            }
            else
            {
                SceneManager.LoadScene("MultiSourceMarkerBasedARExample_SRP");
            }
        }

        public void OnGyroSensorMarkerBasedARExampleButtonClick()
        {
            if (GraphicsSettings.currentRenderPipeline == null)
            {
                SceneManager.LoadScene("GyroSensorMarkerBasedARExample_Built-in");
            }
            else
            {
                SceneManager.LoadScene("GyroSensorMarkerBasedARExample_SRP");
            }
        }
    }
}

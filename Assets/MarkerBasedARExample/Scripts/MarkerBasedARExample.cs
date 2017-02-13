using UnityEngine;
using System.Collections;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace MarkerBasedARExample
{
    public class MarkerBasedARExample : MonoBehaviour
    {
        
        // Use this for initialization
        void Start ()
        {
            
        }
        
        // Update is called once per frame
        void Update ()
        {
            
        }

        public void OnShowLicenseButton ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("ShowLicense");
            #else
            Application.LoadLevel ("ShowLicense");
#endif
        }

        public void OnShowARMarkerButton ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("ShowARMarker");
            #else
            Application.LoadLevel ("ShowARMarker");
            #endif
        }
        
        public void OnTexture2DMarkerBasedARExample ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("Texture2DMarkerBasedARExample");
            #else
            Application.LoadLevel ("Texture2DMarkerBasedARExample");
            #endif
        }
        
        public void OnWebCamTextureMarkerBasedARExample ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("WebCamTextureMarkerBasedARExample");
            #else
            Application.LoadLevel ("WebCamTextureMarkerBasedARExample");
            #endif
        }

        public void OnGyroSensorMarkerBasedARExample ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("GyroSensorMarkerBasedARExample");
            #else
            Application.LoadLevel ("GyroSensorMarkerBasedARExample");
            #endif
        }
    }
}
using UnityEngine;
using System.Collections;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace MarkerBasedARSample
{
    public class MarkerBasedARSample : MonoBehaviour
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
        
        public void OnTexture2DMarkerBasedARSample ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("Texture2DMarkerBasedARSample");
            #else
            Application.LoadLevel ("Texture2DMarkerBasedARSample");
            #endif
        }
        
        public void OnWebCamTextureMarkerBasedARSample ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("WebCamTextureMarkerBasedARSample");
            #else
            Application.LoadLevel ("WebCamTextureMarkerBasedARSample");
            #endif
        }

        public void OnGyroSensorMarkerBasedARSample ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("GyroSensorMarkerBasedARSample");
            #else
            Application.LoadLevel ("GyroSensorMarkerBasedARSample");
            #endif
        }
    }
}
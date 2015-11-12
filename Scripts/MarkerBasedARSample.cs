using UnityEngine;
using System.Collections;

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
			Application.LoadLevel ("ShowLicense");
		}
		
		public void OnTexture2DMarkerBasedARSample ()
		{
			Application.LoadLevel ("Texture2DMarkerBasedARSample");
		}
		
		public void OnWebCamTextureMarkerBasedARSample ()
		{
			Application.LoadLevel ("WebCamTextureMarkerBasedARSample");
		}
	}
}
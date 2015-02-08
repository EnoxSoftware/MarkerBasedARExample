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
		
		void OnGUI ()
		{
			float screenScale = Screen.height / 240.0f;
			Matrix4x4 scaledMatrix = Matrix4x4.Scale (new Vector3 (screenScale, screenScale, screenScale));
			GUI.matrix = scaledMatrix;
			
			
			GUILayout.BeginVertical ();
			
			if (GUILayout.Button ("Show License")) {
				Application.LoadLevel ("ShowLicense");
			}
			
			if (GUILayout.Button ("Texture2DMarkerBasedARSample")) {
				Application.LoadLevel ("Texture2DMarkerBasedARSample");
			}
			
			if (GUILayout.Button ("WebCamTextureMarkerBasedARSample")) {
				Application.LoadLevel ("WebCamTextureMarkerBasedARSample");
			}
			
			
			GUILayout.EndVertical ();
		}
	}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using OpenCVForUnity;

namespace MarkerBasedARSample
{
		/// <summary>
		/// Show ARMarker.
		/// </summary>
		public class ShowARMarker : MonoBehaviour
		{

				/// <summary>
				/// The marker texture.
				/// </summary>
				public Texture2D[] markerTexture;

				/// <summary>
				/// The index.
				/// </summary>
				int index = 0;

				// Use this for initialization
				void Start ()
				{
			
						float width = gameObject.transform.localScale.x;
						float height = gameObject.transform.localScale.y;

						float widthScale = (float)Screen.width / width;
						float heightScale = (float)Screen.height / height;
						if (widthScale < heightScale) {
								Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
						} else {
								Camera.main.orthographicSize = height / 2;
						}

						gameObject.GetComponent<Renderer> ().material.mainTexture = markerTexture [index];
				}


				// Update is called once per frame
				void Update ()
				{

				}

				/// <summary>
				/// Raises the disable event.
				/// </summary>
				void OnDisable ()
				{

				}
		
				/// <summary>
				/// Raises the back button event.
				/// </summary>
				public void OnBackButton ()
				{
						Application.LoadLevel ("MarkerBasedARSample");
				}

				/// <summary>
				/// Raises the change marker button event.
				/// </summary>
				public void OnChangeMarkerButton ()
				{
						index = (index + 1) % markerTexture.Length;
						gameObject.GetComponent<Renderer> ().material.mainTexture = markerTexture [index];
				}
		}
	
}

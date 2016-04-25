using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace OpenCVMarkerBasedAR
{
		public class MarkerSettings : MonoBehaviour
		{
				/// <summary>
				/// The marker design.
				/// </summary>
				public MarkerDesign markerDesign;

				/// <summary>
				/// The AR game object.
				/// </summary>
				public GameObject ARGameObject;

				/// <summary>
				/// The displayable max count.
				/// </summary>
				[Range(1 , 50)]
				public int
						displayableMaxCount;

				/// <summary>
				/// The AR game object diplicates.
				/// </summary>
				private List<GameObject> ARGameObjectDiplicates;

				/// <summary>
				/// The should not set to inactive per frame.
				/// </summary>
				[Tooltip("If true, displayableMaxCount is limited to 1.")]
				public bool
						shouldNotSetToInactivePerFrame;

				/// <summary>
				/// Gets the marker identifier.
				/// </summary>
				/// <returns>The marker identifier.</returns>
				public int getMarkerId ()
				{
						int id = 0;
						int size = markerDesign.gridSize;
						for (int y=0; y<size; y++) {
								int lineId = y;
								for (int x=0; x<size; x++) {
										if (x > 0)
												lineId <<= 1;
										if (!markerDesign.data [y * size + x])
												lineId |= 1;
								}
								id ^= lineId;

//						Debug.Log ("lineId " + lineId);
//						Debug.Log ("id " + id);
						}
						return id;
				}

				/// <summary>
				/// Gets the AR game object.
				/// </summary>
				/// <returns>The AR game object.</returns>
				public GameObject getARGameObject ()
				{
						if (shouldNotSetToInactivePerFrame)
								return ARGameObjectDiplicates [0];
						foreach (GameObject item in ARGameObjectDiplicates) {
								if (!item.activeSelf)
										return item;
						}
						return null;
				}

				/// <summary>
				/// Sets all AR game objects disable.
				/// </summary>
				public void setAllARGameObjectsDisable ()
				{
						if (shouldNotSetToInactivePerFrame)
								return;
						foreach (GameObject item in ARGameObjectDiplicates) {
								item.SetActive (false);
						}
				}

				void Awake ()
				{
						if (shouldNotSetToInactivePerFrame)
								displayableMaxCount = 1;
						if (displayableMaxCount < 1)
								displayableMaxCount = 1;
						ARGameObjectDiplicates = new List<GameObject> ();
						ARGameObjectDiplicates.Add (ARGameObject);
						for (int i = 1; i < displayableMaxCount; i++) {
								GameObject diplicate = GameObject.Instantiate (ARGameObject);
								diplicate.transform.parent = this.transform;
								ARGameObjectDiplicates.Add (diplicate);
						}

						foreach (GameObject item in ARGameObjectDiplicates) {
								item.SetActive (false);
						}
				}

//		// Use this for initialization
//		void Start ()
//		{
//				
//		}
//	
//	// Update is called once per frame
//	void Update () {
//	
//	}
		}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_5_3
using UnityEngine.SceneManagement;
#endif
using OpenCVForUnity;
using OpenCVMarkerBasedAR;

namespace MarkerBasedARSample
{
/// <summary>
/// Texture2D Marker based AR sample.
/// https://github.com/MasteringOpenCV/code/tree/master/Chapter2_iPhoneAR by using "OpenCV for Unity"
/// </summary>
		public class Texture2DMarkerBasedARSample : MonoBehaviour
		{
				/// <summary>
				/// The image texture.
				/// </summary>
				public Texture2D imgTexture;

				/// <summary>
				/// The AR camera.
				/// </summary>
				public Camera ARCamera;

				/// <summary>
				/// The marker settings.
				/// </summary>
				public MarkerSettings[] markerSettings;

				/// <summary>
				/// The should move AR camera.
				/// </summary>
				[Tooltip("If true, only the first element of markerSettings will be processed.")]
				public bool
						shouldMoveARCamera;
		

				// Use this for initialization
				void Start ()
				{

						gameObject.transform.localScale = new Vector3 (imgTexture.width, imgTexture.height, 1);
						Debug.Log ("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);
			
						float width = 0;
						float height = 0;
			
						width = gameObject.transform.localScale.x;
						height = gameObject.transform.localScale.y;

						float imageScale = 1.0f;
						float widthScale = (float)Screen.width / width;
						float heightScale = (float)Screen.height / height;
						if (widthScale < heightScale) {
								Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
								imageScale = (float)Screen.height / (float)Screen.width;
						} else {
								Camera.main.orthographicSize = height / 2;
						}
						

		
						Mat imgMat = new Mat (imgTexture.height, imgTexture.width, CvType.CV_8UC4);
		
						Utils.texture2DToMat (imgTexture, imgMat);
						Debug.Log ("imgMat dst ToString " + imgMat.ToString ());

						//set cameraparam
						int max_d = (int)Mathf.Max (imgMat.rows (), imgMat.cols ());
						Mat camMatrix = new Mat (3, 3, CvType.CV_64FC1);
						camMatrix.put (0, 0, max_d);
						camMatrix.put (0, 1, 0);
						camMatrix.put (0, 2, imgMat.cols () / 2.0f);
						camMatrix.put (1, 0, 0);
						camMatrix.put (1, 1, max_d);
						camMatrix.put (1, 2, imgMat.rows () / 2.0f);
						camMatrix.put (2, 0, 0);
						camMatrix.put (2, 1, 0);
						camMatrix.put (2, 2, 1.0f);
						Debug.Log ("camMatrix " + camMatrix.dump ());

						MatOfDouble distCoeffs = new MatOfDouble (0, 0, 0, 0);
						Debug.Log ("distCoeffs " + distCoeffs.dump ());


						//calibration camera
						Size imageSize = new Size (imgMat.cols () * imageScale, imgMat.rows () * imageScale);
						double apertureWidth = 0;
						double apertureHeight = 0;
						double[] fovx = new double[1];
						double[] fovy = new double[1];
						double[] focalLength = new double[1];
						Point principalPoint = new Point ();
						double[] aspectratio = new double[1];
		
						Calib3d.calibrationMatrixValues (camMatrix, imageSize, apertureWidth, apertureHeight, fovx, fovy, focalLength, principalPoint, aspectratio);
		
						Debug.Log ("imageSize " + imageSize.ToString ());
						Debug.Log ("apertureWidth " + apertureWidth);
						Debug.Log ("apertureHeight " + apertureHeight);
						Debug.Log ("fovx " + fovx [0]);
						Debug.Log ("fovy " + fovy [0]);
						Debug.Log ("focalLength " + focalLength [0]);
						Debug.Log ("principalPoint " + principalPoint.ToString ());
						Debug.Log ("aspectratio " + aspectratio [0]);

						//Adjust Unity Camera FOV
						if (widthScale < heightScale) {
								ARCamera.fieldOfView = (float)fovx [0];
						} else {
								ARCamera.fieldOfView = (float)fovy [0];
						}


						MarkerDesign[] markerDesigns = new MarkerDesign[markerSettings.Length];
						for (int i = 0; i < markerDesigns.Length; i++) {
								markerDesigns [i] = markerSettings [i].markerDesign;
						}
		 
						MarkerDetector markerDetector = new MarkerDetector (camMatrix, distCoeffs, markerDesigns);

						markerDetector.processFrame (imgMat, 1);


						foreach (MarkerSettings settings in markerSettings) {
								settings.setAllARGameObjectsDisable ();
						}


						if (shouldMoveARCamera) {

								List<Marker> findMarkers = markerDetector.getFindMarkers ();
								if (findMarkers.Count > 0) {
					
										Marker marker = findMarkers [0];
				
										if (markerSettings.Length > 0) {
												MarkerSettings settings = markerSettings [0];
					
												if (marker.id == settings.getMarkerId ()) {
														Matrix4x4 transformationM = marker.transformation;
														Debug.Log ("transformationM " + transformationM.ToString ());
							
														Matrix4x4 invertZM = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (1, 1, -1));
														Debug.Log ("invertZM " + invertZM.ToString ());
		
														Matrix4x4 invertYM = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (1, -1, 1));
														Debug.Log ("invertYM " + invertYM.ToString ());
							
														

														GameObject ARGameObject = settings.getARGameObject ();
														if (ARGameObject != null) {
																Matrix4x4 ARM = ARGameObject.transform.localToWorldMatrix * invertZM * transformationM.inverse * invertYM;
																Debug.Log ("ARM " + ARM.ToString ());
																ARGameObject.SetActive (true);
																ARUtils.SetTransformFromMatrix (ARCamera.transform, ref ARM);
														}
														
												}
										}
								}
						} else {
								List<Marker> findMarkers = markerDetector.getFindMarkers ();
								for (int i = 0; i < findMarkers.Count; i++) {
										Marker marker = findMarkers [i];

										foreach (MarkerSettings settings in markerSettings) {
												if (marker.id == settings.getMarkerId ()) {
														Matrix4x4 transformationM = marker.transformation;
														Debug.Log ("transformationM " + transformationM.ToString ());

														
														Matrix4x4 invertYM = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (1, -1, 1));
														Debug.Log ("invertYM " + invertYM.ToString ());
					
														Matrix4x4 invertZM = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (1, 1, -1));
														Debug.Log ("invertZM " + invertZM.ToString ());
					
														Matrix4x4 ARM = ARCamera.transform.localToWorldMatrix * invertYM * transformationM * invertZM;
														Debug.Log ("ARM " + ARM.ToString ());
					
														GameObject ARGameObject = settings.getARGameObject ();
														if (ARGameObject != null) {
																ARUtils.SetTransformFromMatrix (ARGameObject.transform, ref ARM);
																ARGameObject.SetActive (true);
														}
												}
										}
								}
						}


						Texture2D texture = new Texture2D (imgMat.cols (), imgMat.rows (), TextureFormat.RGBA32, false);
		
						Utils.matToTexture2D (imgMat, texture);
		
						gameObject.GetComponent<Renderer> ().material.mainTexture = texture;
				}
	
				// Update is called once per frame
				void Update ()
				{
	
				}

				public void OnBackButton ()
				{
						#if UNITY_5_3
			SceneManager.LoadScene ("MarkerBasedARSample");
						#else
						Application.LoadLevel ("MarkerBasedARSample");
#endif
				}

		}
}

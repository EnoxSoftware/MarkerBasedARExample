using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using OpenCVForUnity;
using OpenCVForUnity.MarkerBasedAR;

namespace MarkerBasedARSample
{
		/// <summary>
		/// Web cam texture marker based AR sample.
		/// </summary>
		[RequireComponent(typeof(WebCamTextureToMatHelper))]
		public class WebCamTextureMarkerBasedARSample : MonoBehaviour
		{

				/// <summary>
				/// The colors.
				/// </summary>
				Color32[] colors;

				/// <summary>
				/// The texture.
				/// </summary>
				Texture2D texture;

				/// <summary>
				/// The AR camera.
				/// </summary>
				public Camera ARCamera;

				/// <summary>
				/// The cam matrix.
				/// </summary>
				Mat camMatrix;

				/// <summary>
				/// The dist coeffs.
				/// </summary>
				MatOfDouble distCoeffs;

				/// <summary>
				/// The marker detector.
				/// </summary>
				MarkerDetector markerDetector;

				/// <summary>
				/// The invert Y.
				/// </summary>
				Matrix4x4 invertYM;

				/// <summary>
				/// The transformation m.
				/// </summary>
				Matrix4x4 transformationM;

				/// <summary>
				/// The invert Z.
				/// </summary>
				Matrix4x4 invertZM;

				/// <summary>
				/// The ar m.
				/// </summary>
				Matrix4x4 ARM;
		
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

				/// <summary>
				/// The web cam texture to mat helper.
				/// </summary>
				WebCamTextureToMatHelper webCamTextureToMatHelper;
		
				// Use this for initialization
				void Start ()
				{

						webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper> ();
						webCamTextureToMatHelper.Init ();

				}

				/// <summary>
				/// Raises the web cam texture to mat helper inited event.
				/// </summary>
				public void OnWebCamTextureToMatHelperInited ()
				{
						Debug.Log ("OnWebCamTextureToMatHelperInited");
			
						Mat webCamTextureMat = webCamTextureToMatHelper.GetMat ();
			
						colors = new Color32[webCamTextureMat.cols () * webCamTextureMat.rows ()];
						texture = new Texture2D (webCamTextureMat.cols (), webCamTextureMat.rows (), TextureFormat.RGBA32, false);



						gameObject.transform.localScale = new Vector3 (webCamTextureMat.cols (), webCamTextureMat.rows (), 1);
			
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
			
						gameObject.GetComponent<Renderer> ().material.mainTexture = texture;

			
						//set cameraparam
						int max_d = Mathf.Max (webCamTextureMat.rows (), webCamTextureMat.cols ());
						camMatrix = new Mat (3, 3, CvType.CV_64FC1);
						camMatrix.put (0, 0, max_d);
						camMatrix.put (0, 1, 0);
						camMatrix.put (0, 2, webCamTextureMat.cols () / 2.0f);
						camMatrix.put (1, 0, 0);
						camMatrix.put (1, 1, max_d);
						camMatrix.put (1, 2, webCamTextureMat.rows () / 2.0f);
						camMatrix.put (2, 0, 0);
						camMatrix.put (2, 1, 0);
						camMatrix.put (2, 2, 1.0f);
						Debug.Log ("camMatrix " + camMatrix.dump ());
			
						distCoeffs = new MatOfDouble (0, 0, 0, 0);
						Debug.Log ("distCoeffs " + distCoeffs.dump ());
			
						//calibration camera
						Size imageSize = new Size (webCamTextureMat.cols () * imageScale, webCamTextureMat.rows () * imageScale);
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
								
						markerDetector = new MarkerDetector (camMatrix, distCoeffs, markerDesigns);


						
						invertYM = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (1, -1, 1));
						Debug.Log ("invertYM " + invertYM.ToString ());
			
						invertZM = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (1, 1, -1));
						Debug.Log ("invertZM " + invertZM.ToString ());
						

						//if WebCamera is frontFaceing,flip Mat.
						if (webCamTextureToMatHelper.GetWebCamDevice ().isFrontFacing) {
								webCamTextureToMatHelper.flipHorizontal = true;
						}
				}
		
				/// <summary>
				/// Raises the web cam texture to mat helper disposed event.
				/// </summary>
				public void OnWebCamTextureToMatHelperDisposed ()
				{
						Debug.Log ("OnWebCamTextureToMatHelperDisposed");
			
				}

				// Update is called once per frame
				void Update ()
				{

						if (webCamTextureToMatHelper.isPlaying () && webCamTextureToMatHelper.didUpdateThisFrame ()) {
				
								Mat rgbaMat = webCamTextureToMatHelper.GetMat ();

								markerDetector.processFrame (rgbaMat, 1);


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
																transformationM = marker.transformation;
//														Debug.Log ("transformationM " + transformationM.ToString ());

						
																GameObject ARGameObject = settings.getARGameObject ();
																if (ARGameObject != null) {
																		ARM = ARGameObject.transform.localToWorldMatrix * invertZM * transformationM.inverse * invertYM;
																		//Debug.Log ("arM " + arM.ToString ());
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
																transformationM = marker.transformation;
//																Debug.Log ("transformationM " + transformationM.ToString ());

																ARM = ARCamera.transform.localToWorldMatrix * invertYM * transformationM * invertZM;
																//Debug.Log ("arM " + arM.ToString ());

																GameObject ARGameObject = settings.getARGameObject ();
																if (ARGameObject != null) {
																		
																		ARUtils.SetTransformFromMatrix (ARGameObject.transform, ref ARM);
																		ARGameObject.SetActive (true);
																}
														}
												}
										}
								}
				

								Utils.matToTexture2D (rgbaMat, texture, colors);
						}
			
				}
		
				/// <summary>
				/// Raises the disable event.
				/// </summary>
				void OnDisable ()
				{
						webCamTextureToMatHelper.Dispose ();
				}
		
				/// <summary>
				/// Raises the back button event.
				/// </summary>
				public void OnBackButton ()
				{
						Application.LoadLevel ("MarkerBasedARSample");
				}
		
				/// <summary>
				/// Raises the play button event.
				/// </summary>
				public void OnPlayButton ()
				{
						webCamTextureToMatHelper.Play ();
				}
		
				/// <summary>
				/// Raises the pause button event.
				/// </summary>
				public void OnPauseButton ()
				{
						webCamTextureToMatHelper.Pause ();
				}
		
				/// <summary>
				/// Raises the stop button event.
				/// </summary>
				public void OnStopButton ()
				{
						webCamTextureToMatHelper.Stop ();
				}
		
				/// <summary>
				/// Raises the change camera button event.
				/// </summary>
				public void OnChangeCameraButton ()
				{
						webCamTextureToMatHelper.Init (null, webCamTextureToMatHelper.requestWidth, webCamTextureToMatHelper.requestHeight, !webCamTextureToMatHelper.requestIsFrontFacing);
				}

		}
	
}

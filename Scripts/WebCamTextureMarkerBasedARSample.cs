using UnityEngine;
using System.Collections;

using OpenCVForUnity;

namespace MarkerBasedARSample
{
		/// <summary>
		/// WebCamTexture to mat sample.
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
				public Camera[] ARCamera;

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
				/// The look at m.
				/// </summary>
				Matrix4x4 lookAtM;

				/// <summary>
				/// The transformation m.
				/// </summary>
				Matrix4x4 transformationM;

				/// <summary>
				/// The invert Z.
				/// </summary>
				Matrix4x4 invertZM;

				/// <summary>
				/// The world to camera m.
				/// </summary>
				Matrix4x4 worldToCameraM;

				/// <summary>
				/// The marker design.
				/// </summary>
				public MarkerDesign markerDesign;

				/// <summary>
				/// The web cam texture to mat helper.
				/// </summary>
				WebCamTextureToMatHelper webCamTextureToMatHelper;
		
				// Use this for initialization
				void Start ()
				{

						webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper> ();
						webCamTextureToMatHelper.Init (OnWebCamTextureToMatHelperInited, OnWebCamTextureToMatHelperDisposed);

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
						for (int i = 0; i < ARCamera.Length; i++) {
								
								if (Screen.height > Screen.width) {
										ARCamera [i].fieldOfView = (float)fovx [0];
								} else {
										ARCamera [i].fieldOfView = (float)fovy [0];
								}
						}
													
			
													
								
						markerDetector = new MarkerDetector (camMatrix, distCoeffs, markerDesign);
			
			
						//Marker Coordinate Initial Matrix
						lookAtM = getLookAtMatrix (new Vector3 (0, 0, 0), new Vector3 (0, 0, 1), new Vector3 (0, -1, 0));
						Debug.Log ("lookAt " + lookAtM.ToString ());
			
						//OpenGL to Unity Coordinate System Convert Matrix
						//http://docs.unity3d.com/ScriptReference/Camera-worldToCameraMatrix.html that camera space matches OpenGL convention: camera's forward is the negative Z axis. This is different from Unity's convention, where forward is the positive Z axis.
						invertZM = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (1, 1, -1));
						Debug.Log ("invertZM " + invertZM.ToString ());
			
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

						if (webCamTextureToMatHelper.isPlaying ()) {
				
								Mat rgbaMat = webCamTextureToMatHelper.GetMat ();

								markerDetector.processFrame (rgbaMat, 1);
				
								//Debug.Log ("markerDetector.getTransformations ().Count " + markerDetector.getTransformations ().Count);
				
				
								for (int i = 0; i < ARCamera.Length; i++) {
										ARCamera [i].gameObject.SetActive (false);
								}
				
								int markerCount = markerDetector.getTransformations ().Count;
								for (int i = 0; i < markerCount; i++) {
										if (i > ARCamera.Length - 1)
												break;
													
										ARCamera [i].gameObject.SetActive (true);
				
										//Marker to Camera Coordinate System Convert Matrix
										transformationM = markerDetector.getTransformations () [i];
										//Debug.Log ("transformationM " + transformationM.ToString ());
								
										worldToCameraM = lookAtM * transformationM * invertZM;
										//Debug.Log ("worldToCameraM " + worldToCameraM.ToString ());
								
										ARCamera [i].worldToCameraMatrix = worldToCameraM;
								}
				
//								Core.putText (rgbaMat, "W:" + rgbaMat.width () + " H:" + rgbaMat.height () + " SO:" + Screen.orientation, new Point (5, rgbaMat.rows () - 10), Core.FONT_HERSHEY_SIMPLEX, 1.0, new Scalar (255, 255, 255, 255), 2, Core.LINE_AA, false);
				
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
						webCamTextureToMatHelper.Init (null, webCamTextureToMatHelper.requestWidth, webCamTextureToMatHelper.requestHeight, !webCamTextureToMatHelper.requestIsFrontFacing, OnWebCamTextureToMatHelperInited, OnWebCamTextureToMatHelperDisposed);
				}

				/// <summary>
				/// Gets the look at matrix.
				/// </summary>
				/// <returns>The look at matrix.</returns>
				/// <param name="pos">Position.</param>
				/// <param name="target">Target.</param>
				/// <param name="up">Up.</param>
				private Matrix4x4 getLookAtMatrix (Vector3 pos, Vector3 target, Vector3 up)
				{
			
						Vector3 z = Vector3.Normalize (pos - target);
						Vector3 x = Vector3.Normalize (Vector3.Cross (up, z));
						Vector3 y = Vector3.Normalize (Vector3.Cross (z, x));
			
						Matrix4x4 result = new Matrix4x4 ();
						result.SetRow (0, new Vector4 (x.x, x.y, x.z, -(Vector3.Dot (pos, x))));
						result.SetRow (1, new Vector4 (y.x, y.y, y.z, -(Vector3.Dot (pos, y))));
						result.SetRow (2, new Vector4 (z.x, z.y, z.z, -(Vector3.Dot (pos, z))));
						result.SetRow (3, new Vector4 (0, 0, 0, 1));
			
						return result;
				}
		
		}
	
}

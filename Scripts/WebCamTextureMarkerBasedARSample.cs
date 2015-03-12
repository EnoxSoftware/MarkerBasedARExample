using UnityEngine;
using System.Collections;

using OpenCVForUnity;

namespace MarkerBasedARSample
{
		/// <summary>
		/// WebCamTexture to mat sample.
		/// </summary>
		public class WebCamTextureMarkerBasedARSample : MonoBehaviour
		{

				/// <summary>
				/// The web cam texture.
				/// </summary>
				WebCamTexture webCamTexture;

				/// <summary>
				/// The colors.
				/// </summary>
				Color32[] colors;

		#if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
		/// <summary>
		/// The is front.
		/// </summary>
		bool isFront = false;
		#endif

				/// <summary>
				/// The width.
				/// </summary>
				int width = 640;

				/// <summary>
				/// The height.
				/// </summary>
				int height = 480;

				/// <summary>
				/// The rgba mat.
				/// </summary>
				Mat rgbaMat;

				/// <summary>
				/// The texture.
				/// </summary>
				Texture2D texture;

				/// <summary>
				/// The init done.
				/// </summary>
				bool initDone = false;

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

		
		
				// Use this for initialization
				void Start ()
				{
						// Checks how many and which cameras are available on the device
						for (int cameraIndex = 0; cameraIndex < WebCamTexture.devices.Length; cameraIndex++) {
				
								#if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
				                if (WebCamTexture.devices [cameraIndex].isFrontFacing == isFront) {
								#endif
					
								Debug.Log (cameraIndex + " name " + WebCamTexture.devices [cameraIndex].name + " isFrontFacing " + WebCamTexture.devices [cameraIndex].isFrontFacing);
					
								//Set the appropriate fps
								webCamTexture = new WebCamTexture (WebCamTexture.devices [cameraIndex].name, width, height, 30);
					
								#if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
					                break;
				                }
								#endif
				
						}
			
						Debug.Log ("width " + webCamTexture.width + " height " + webCamTexture.height + " fps " + webCamTexture.requestedFPS);
			
			
			
						// Starts the camera
						webCamTexture.Play ();
			
			
						StartCoroutine (init ());
			
			
			
				}

				/// <summary>
				/// Init this instance.
				/// </summary>
				private IEnumerator init ()
				{
						while (true) {
								//If you want to use webcamTexture.width and webcamTexture.height on iOS, you have to wait until webcamTexture.didUpdateThisFrame == 1, otherwise these two values will be equal to 16. (http://forum.unity3d.com/threads/webcamtexture-and-error-0x0502.123922/)
								if (webCamTexture.width > 16 && webCamTexture.height > 16) {
										Debug.Log ("width " + webCamTexture.width + " height " + webCamTexture.height + " fps " + webCamTexture.requestedFPS);
					
					
										colors = new Color32[webCamTexture.width * webCamTexture.height];
					
										rgbaMat = new Mat (webCamTexture.height, webCamTexture.width, CvType.CV_8UC4);
					
										texture = new Texture2D (webCamTexture.width, webCamTexture.height, TextureFormat.RGBA32, false);


										//gameObject.transform.eulerAngles = new Vector3 (0, 0, -90);
										gameObject.transform.localScale = new Vector3 (webCamTexture.width, webCamTexture.height, 1);
					
					
					
					
										gameObject.transform.localEulerAngles = new Vector3 (0, 0, 0);
										gameObject.transform.rotation = gameObject.transform.rotation * Quaternion.AngleAxis (webCamTexture.videoRotationAngle, Vector3.back);
					
					
										bool _videoVerticallyMirrored = webCamTexture.videoVerticallyMirrored;
										float scaleX = 1;
										float scaleY = _videoVerticallyMirrored ? -1.0f : 1.0f;
					
										gameObject.transform.localScale = new Vector3 (scaleX * gameObject.transform.localScale.x, scaleY * gameObject.transform.localScale.y, 1);

					
										gameObject.GetComponent<Renderer> ().material.mainTexture = texture;


										Camera.main.orthographicSize = webCamTexture.height / 2;





										//set cameraparam
										int max_d = Mathf.Max (rgbaMat.rows (), rgbaMat.cols ());
										camMatrix = new Mat (3, 3, CvType.CV_64FC1);
										camMatrix.put (0, 0, max_d);
										camMatrix.put (0, 1, 0);
										camMatrix.put (0, 2, rgbaMat.cols () / 2.0f);
										camMatrix.put (1, 0, 0);
										camMatrix.put (1, 1, max_d);
										camMatrix.put (1, 2, rgbaMat.rows () / 2.0f);
										camMatrix.put (2, 0, 0);
										camMatrix.put (2, 1, 0);
										camMatrix.put (2, 2, 1.0f);
										Debug.Log ("camMatrix " + camMatrix.dump ());

										distCoeffs = new MatOfDouble (0, 0, 0, 0);
										Debug.Log ("distCoeffs " + distCoeffs.dump ());

										//calibration camera
										Size imageSize = new Size (rgbaMat.cols (), rgbaMat.rows ());
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
												ARCamera [i].fieldOfView = (float)fovy [0];
												if (_videoVerticallyMirrored)
														ARCamera [i].projectionMatrix = ARCamera [i].projectionMatrix * Matrix4x4.Scale (new Vector3 (1, -1, 1));
										}
										

										
					
										markerDetector = new MarkerDetector (camMatrix, distCoeffs);


										//Marker Coordinate Initial Matrix
										lookAtM = getLookAtMatrix (new Vector3 (0, 0, 0), new Vector3 (0, 0, 1), new Vector3 (0, -1, 0));
										Debug.Log ("lookAt " + lookAtM.ToString ());

										//OpenGL to Unity Coordinate System Convert Matrix
										//http://docs.unity3d.com/ScriptReference/Camera-worldToCameraMatrix.html that camera space matches OpenGL convention: camera's forward is the negative Z axis. This is different from Unity's convention, where forward is the positive Z axis.
										invertZM = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (1, scaleY * 1, -1));
										Debug.Log ("invertZM " + invertZM.ToString ());


					
										initDone = true;
					
										break;
								} else {
										yield return 0;
								}
						}
				}
		
				// Update is called once per frame
				void Update ()
				{
						if (!initDone)
								return;

						if (webCamTexture.width > 16 && webCamTexture.height > 16) {
								
				
								Utils.webCamTextureToMat (webCamTexture, rgbaMat, colors);

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
							
				
								Utils.matToTexture2D (rgbaMat, texture, colors);
				
								gameObject.GetComponent<Renderer> ().material.mainTexture = texture;
				
				
						}
			
				}
		
				void OnDisable ()
				{
						webCamTexture.Stop ();
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

				void OnGUI ()
				{
						float screenScale = Screen.height / 240.0f;
						Matrix4x4 scaledMatrix = Matrix4x4.Scale (new Vector3 (screenScale, screenScale, screenScale));
						GUI.matrix = scaledMatrix;
			
			
						GUILayout.BeginVertical ();
			
						if (GUILayout.Button ("back")) {
								Application.LoadLevel ("MarkerBasedARSample");
						}
			
						GUILayout.EndVertical ();
				}
		}
	
}

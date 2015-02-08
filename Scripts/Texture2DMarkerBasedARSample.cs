using UnityEngine;
using System.Collections;

using OpenCVForUnity;

namespace MarkerBasedARSample
{
/// <summary>
/// Marker based AR sample.
/// https://github.com/MasteringOpenCV/code/tree/master/Chapter2_iPhoneAR by using "OpenCV for Unity"
/// </summary>
		public class Texture2DMarkerBasedARSample : MonoBehaviour
		{

				/// <summary>
				/// The AR camera.
				/// </summary>
				public Camera ARCamera;


				// Use this for initialization
				void Start ()
				{
						Texture2D imgTexture = Resources.Load ("MarkerTest") as Texture2D;


						gameObject.transform.localScale = new Vector3 (imgTexture.width, imgTexture.height, 1);
						Camera.main.orthographicSize = imgTexture.height / 2;

		
						Mat imgMat = new Mat (imgTexture.height, imgTexture.width, CvType.CV_8UC4);
		
						Utils.texture2DToMat (imgTexture, imgMat);
						Debug.Log ("imgMat dst ToString " + imgMat.ToString ());

						//set cameraparam
						int max_d = Mathf.Max (imgMat.rows (), imgMat.cols ());
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
						Size imageSize = new Size (imgMat.cols (), imgMat.rows ());
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
						ARCamera.fieldOfView = (float)fovy [0];


		 
						MarkerDetector markerDetector = new MarkerDetector (camMatrix, distCoeffs);

						markerDetector.processFrame (imgMat, 1);


						//Marker Coordinate Initial Matrix
						Matrix4x4 lookAtM = getLookAtMatrix (new Vector3 (0, 0, 0), new Vector3 (0, 0, 1), new Vector3 (0, -1, 0));
						Debug.Log ("lookAt " + lookAtM.ToString ());

						//Marker to Camera Coordinate System Convert Matrix
						Matrix4x4 transformationM = markerDetector.getTransformations () [0];
						Debug.Log ("transformationM " + transformationM.ToString ());

						//OpenGL to Unity Coordinate System Convert Matrix
						//http://docs.unity3d.com/ScriptReference/Camera-worldToCameraMatrix.html that camera space matches OpenGL convention: camera's forward is the negative Z axis. This is different from Unity's convention, where forward is the positive Z axis. 
						Matrix4x4 invertZM = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (1, 1, -1));
						Debug.Log ("invertZM " + invertZM.ToString ());

						Matrix4x4 worldToCameraM = lookAtM * transformationM * invertZM;
						Debug.Log ("worldToCameraM " + worldToCameraM.ToString ());

						ARCamera.worldToCameraMatrix = worldToCameraM;
		




						Texture2D texture = new Texture2D (imgMat.cols (), imgMat.rows (), TextureFormat.RGBA32, false);
		
						Utils.matToTexture2D (imgMat, texture);
		
						gameObject.GetComponent<Renderer> ().material.mainTexture = texture;
				}
	
				// Update is called once per frame
				void Update ()
				{
	
				}

				/// <summary>
				/// Gets the lookAt matrix.
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

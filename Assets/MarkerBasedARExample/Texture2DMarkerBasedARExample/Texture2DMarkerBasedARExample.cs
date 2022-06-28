using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using OpenCVMarkerBasedAR;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MarkerBasedARExample
{
    /// <summary>
    /// Texture2D Marker Based AR Example
    /// This code is a rewrite of https://github.com/MasteringOpenCV/code/tree/master/Chapter2_iPhoneAR using "OpenCV for Unity".
    /// </summary>
    public class Texture2DMarkerBasedARExample : MonoBehaviour
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
        /// Determines if should move AR camera.
        /// </summary>
        [Tooltip("If true, only the first element of markerSettings will be processed.")]
        public bool shouldMoveARCamera;

        // Use this for initialization
        void Start()
        {
            gameObject.transform.localScale = new Vector3(imgTexture.width, imgTexture.height, 1);
            Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

            Mat imgMat = new Mat(imgTexture.height, imgTexture.width, CvType.CV_8UC4);

            Utils.texture2DToMat(imgTexture, imgMat);
            Debug.Log("imgMat dst ToString " + imgMat.ToString());


            float width = imgMat.width();
            float height = imgMat.height();

            float imageSizeScale = 1.0f;
            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale)
            {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
                imageSizeScale = (float)Screen.height / (float)Screen.width;
            }
            else
            {
                Camera.main.orthographicSize = height / 2;
            }

            //set cameraparam
            int max_d = (int)Mathf.Max(width, height);
            double fx = max_d;
            double fy = max_d;
            double cx = width / 2.0f;
            double cy = height / 2.0f;
            Mat camMatrix = new Mat(3, 3, CvType.CV_64FC1);
            camMatrix.put(0, 0, fx);
            camMatrix.put(0, 1, 0);
            camMatrix.put(0, 2, cx);
            camMatrix.put(1, 0, 0);
            camMatrix.put(1, 1, fy);
            camMatrix.put(1, 2, cy);
            camMatrix.put(2, 0, 0);
            camMatrix.put(2, 1, 0);
            camMatrix.put(2, 2, 1.0f);
            Debug.Log("camMatrix " + camMatrix.dump());

            MatOfDouble distCoeffs = new MatOfDouble(0, 0, 0, 0);
            Debug.Log("distCoeffs " + distCoeffs.dump());

            //calibration camera
            Size imageSize = new Size(width * imageSizeScale, height * imageSizeScale);
            double apertureWidth = 0;
            double apertureHeight = 0;
            double[] fovx = new double[1];
            double[] fovy = new double[1];
            double[] focalLength = new double[1];
            Point principalPoint = new Point(0, 0);
            double[] aspectratio = new double[1];

            Calib3d.calibrationMatrixValues(camMatrix, imageSize, apertureWidth, apertureHeight, fovx, fovy, focalLength, principalPoint, aspectratio);

            Debug.Log("imageSize " + imageSize.ToString());
            Debug.Log("apertureWidth " + apertureWidth);
            Debug.Log("apertureHeight " + apertureHeight);
            Debug.Log("fovx " + fovx[0]);
            Debug.Log("fovy " + fovy[0]);
            Debug.Log("focalLength " + focalLength[0]);
            Debug.Log("principalPoint " + principalPoint.ToString());
            Debug.Log("aspectratio " + aspectratio[0]);


            //To convert the difference of the FOV value of the OpenCV and Unity. 
            double fovXScale = (2.0 * Mathf.Atan((float)(imageSize.width / (2.0 * fx)))) / (Mathf.Atan2((float)cx, (float)fx) + Mathf.Atan2((float)(imageSize.width - cx), (float)fx));
            double fovYScale = (2.0 * Mathf.Atan((float)(imageSize.height / (2.0 * fy)))) / (Mathf.Atan2((float)cy, (float)fy) + Mathf.Atan2((float)(imageSize.height - cy), (float)fy));

            Debug.Log("fovXScale " + fovXScale);
            Debug.Log("fovYScale " + fovYScale);


            //Adjust Unity Camera FOV
            if (widthScale < heightScale)
            {
                ARCamera.fieldOfView = (float)(fovx[0] * fovXScale);
            }
            else
            {
                ARCamera.fieldOfView = (float)(fovy[0] * fovYScale);
            }


            MarkerDesign[] markerDesigns = new MarkerDesign[markerSettings.Length];
            for (int i = 0; i < markerDesigns.Length; i++)
            {
                markerDesigns[i] = markerSettings[i].markerDesign;
            }

            MarkerDetector markerDetector = new MarkerDetector(camMatrix, distCoeffs, markerDesigns);

            markerDetector.processFrame(imgMat, 1);


            foreach (MarkerSettings settings in markerSettings)
            {
                settings.setAllARGameObjectsDisable();
            }


            if (shouldMoveARCamera)
            {
                List<Marker> findMarkers = markerDetector.getFindMarkers();
                if (findMarkers.Count > 0)
                {

                    Marker marker = findMarkers[0];

                    if (markerSettings.Length > 0)
                    {
                        MarkerSettings settings = markerSettings[0];

                        if (marker.id == settings.getMarkerId())
                        {
                            Matrix4x4 transformationM = marker.transformation;
                            Debug.Log("transformationM " + transformationM.ToString());

                            Matrix4x4 invertZM = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
                            Debug.Log("invertZM " + invertZM.ToString());

                            Matrix4x4 invertYM = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, -1, 1));
                            Debug.Log("invertYM " + invertYM.ToString());

                            // right-handed coordinates system (OpenCV) to left-handed one (Unity)
                            // https://stackoverflow.com/questions/30234945/change-handedness-of-a-row-major-4x4-transformation-matrix
                            Matrix4x4 ARM = invertYM * transformationM * invertYM;

                            // Apply Y-axis and Z-axis refletion matrix. (Adjust the posture of the AR object)
                            ARM = ARM * invertYM * invertZM;

                            GameObject ARGameObject = settings.getARGameObject();
                            if (ARGameObject != null)
                            {
                                ARM = ARGameObject.transform.localToWorldMatrix * ARM.inverse;

                                Debug.Log("ARM " + ARM.ToString());

                                ARGameObject.SetActive(true);
                                ARUtils.SetTransformFromMatrix(ARCamera.transform, ref ARM);
                            }
                        }
                    }
                }
            }
            else
            {
                List<Marker> findMarkers = markerDetector.getFindMarkers();
                for (int i = 0; i < findMarkers.Count; i++)
                {
                    Marker marker = findMarkers[i];

                    foreach (MarkerSettings settings in markerSettings)
                    {
                        if (marker.id == settings.getMarkerId())
                        {
                            Matrix4x4 transformationM = marker.transformation;
                            Debug.Log("transformationM " + transformationM.ToString());


                            Matrix4x4 invertYM = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, -1, 1));
                            Debug.Log("invertYM " + invertYM.ToString());

                            Matrix4x4 invertZM = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
                            Debug.Log("invertZM " + invertZM.ToString());

                            // right-handed coordinates system (OpenCV) to left-handed one (Unity)
                            // https://stackoverflow.com/questions/30234945/change-handedness-of-a-row-major-4x4-transformation-matrix
                            Matrix4x4 ARM = invertYM * transformationM * invertYM;

                            // Apply Y-axis and Z-axis refletion matrix. (Adjust the posture of the AR object)
                            ARM = ARM * invertYM * invertZM;

                            ARM = ARCamera.transform.localToWorldMatrix * ARM;

                            Debug.Log("ARM " + ARM.ToString());

                            GameObject ARGameObject = settings.getARGameObject();
                            if (ARGameObject != null)
                            {
                                ARUtils.SetTransformFromMatrix(ARGameObject.transform, ref ARM);
                                ARGameObject.SetActive(true);
                            }
                        }
                    }
                }
            }


            Texture2D texture = new Texture2D(imgMat.cols(), imgMat.rows(), TextureFormat.RGBA32, false);

            Utils.matToTexture2D(imgMat, texture);

            gameObject.GetComponent<Renderer>().material.mainTexture = texture;
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// Raises the back button click event.
        /// </summary>
        public void OnBackButtonClick()
        {
            SceneManager.LoadScene("MarkerBasedARExample");
        }
    }
}

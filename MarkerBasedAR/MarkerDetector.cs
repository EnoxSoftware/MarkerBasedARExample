using UnityEngine;
using System.Collections.Generic;

using System.Linq;

using OpenCVForUnity;

/// <summary>
/// Marker detector.
/// </summary>
public class MarkerDetector
{

		/// <summary>
		/// The m_min contour length allowed.
		/// </summary>
		private float m_minContourLengthAllowed;

		/// <summary>
		/// The size of the marker.
		/// </summary>
		private Size markerSize;

		/// <summary>
		/// The cam matrix.
		/// </summary>
		private Mat camMatrix = new Mat ();

		/// <summary>
		/// The dist coeff.
		/// </summary>
		private MatOfDouble distCoeff = new MatOfDouble ();

		/// <summary>
		/// The m_transformations.
		/// </summary>
		private List<Matrix4x4> m_transformations = new List<Matrix4x4> ();

		/// <summary>
		/// The m_grayscale image.
		/// </summary>
		private Mat m_grayscaleImage = new Mat ();

		/// <summary>
		/// The m_threshold image.
		/// </summary>
		private Mat m_thresholdImg = new Mat ();

		/// <summary>
		/// The canonical marker image.
		/// </summary>
		private Mat canonicalMarkerImage = new Mat ();

		/// <summary>
		/// The m_contours.
		/// </summary>
		private List<MatOfPoint> m_contours = new List<MatOfPoint> ();

		/// <summary>
		/// The m_marker corners3d.
		/// </summary>
		private MatOfPoint3f m_markerCorners3d = new MatOfPoint3f ();

		/// <summary>
		/// The m_marker corners2d.
		/// </summary>
		private MatOfPoint2f m_markerCorners2d = new MatOfPoint2f ();

		/// <summary>
		/// Initializes a new instance of the <see cref="MarkerDetector"/> class.
		/// </summary>
		/// <param name="camMatrix">Cam matrix.</param>
		/// <param name="distCoeff">Dist coeff.</param>
		public MarkerDetector (Mat camMatrix, Mat distCoeff)
		{
				m_minContourLengthAllowed = 100;
				markerSize = new Size (100, 100);


				camMatrix.copyTo (this.camMatrix);
				distCoeff.copyTo (this.distCoeff);


				List<Point3> m_markerCorners3dList = new List<Point3> ();

				m_markerCorners3dList.Add (new Point3 (-0.5f, -0.5f, 0));
				m_markerCorners3dList.Add (new Point3 (+0.5f, -0.5f, 0));
				m_markerCorners3dList.Add (new Point3 (+0.5f, +0.5f, 0));
				m_markerCorners3dList.Add (new Point3 (-0.5f, +0.5f, 0));

				m_markerCorners3d.fromList (m_markerCorners3dList);


				List<Point> m_markerCorners2dList = new List<Point> ();

				m_markerCorners2dList.Add (new Point (0, 0));
				m_markerCorners2dList.Add (new Point (markerSize.width - 1, 0));
				m_markerCorners2dList.Add (new Point (markerSize.width - 1, markerSize.height - 1));
				m_markerCorners2dList.Add (new Point (0, markerSize.height - 1));

				m_markerCorners2d.fromList (m_markerCorners2dList);
		}

		/// <summary>
		/// Processes the frame.
		/// </summary>
		/// <param name="bgraMat">Bgra mat.</param>
		/// <param name="scale">Scale.</param>
		public void processFrame (Mat bgraMat, float scale)
		{
//				Mat resized = new Mat ();
//				Imgproc.resize (bgraMat, resized, new Size (), scale, scale, Imgproc.INTER_LINEAR);


				List<Marker> markers = new List<Marker> ();
//				findMarkers (resized, markers);
				findMarkers (bgraMat, markers);


//				Debug.Log ("markers " + markers  .Count);
		
				m_transformations.Clear ();
				for (int i=0; i<markers.Count; i++) {
						m_transformations.Add (markers [i].transformation);
				}
		}

		/// <summary>
		/// Gets the transformations.
		/// </summary>
		/// <returns>The transformations.</returns>
		public List<Matrix4x4> getTransformations ()
		{
				return m_transformations;
		}

		/// <summary>
		/// Finds the markers.
		/// </summary>
		/// <param name="bgraMat">Bgra mat.</param>
		/// <param name="detectedMarkers">Detected markers.</param>
		void findMarkers (Mat bgraMat, List<Marker> detectedMarkers)
		{
				// Convert the image to grayscale
				Imgproc.cvtColor (bgraMat, m_grayscaleImage, Imgproc.COLOR_BGRA2GRAY);
		
				// Make it binary
				Imgproc.threshold (m_grayscaleImage, m_thresholdImg, 127, 255, Imgproc.THRESH_BINARY_INV);

				// Detect contours
				findContours (m_thresholdImg, m_contours, m_grayscaleImage.cols () / 5);
		
				// Find closed contours that can be approximated with 4 points
				findCandidates (m_contours, detectedMarkers);
		
				// Find is them are markers
				recognizeMarkers (m_grayscaleImage, detectedMarkers);
	
				// Calculate their poses
				estimatePosition (detectedMarkers);



				//Debug
//				for (int i = 0; i < detectedMarkers.Count; i++) {
//						detectedMarkers [i].drawContour (bgraMat, new Scalar (255, 0, 0, 255));
//
//						Marker m = detectedMarkers [i];
//
//						Mat P = new Mat (3, 4, CvType.CV_64FC1);
//						P.put (0, 0,
//			       m.transformation.GetRow (0).x, m.transformation.GetRow (0).y, m.transformation.GetRow (0).z, m.transformation.GetRow (0).w,
//			       m.transformation.GetRow (1).x, m.transformation.GetRow (1).y, m.transformation.GetRow (1).z, m.transformation.GetRow (1).w,
//			       m.transformation.GetRow (2).x, m.transformation.GetRow (2).y, m.transformation.GetRow (2).z, m.transformation.GetRow (2).w
//						);
////						Debug.Log ("P " + P.dump ());
//			
//						
//						Mat KP = new Mat (3, 4, CvType.CV_64FC1);
//						Core.gemm (camMatrix, P, 1, new Mat (3, 4, CvType.CV_64FC1), 0, KP);
////						Debug.Log ("KP " + KP.dump ());
//			
//			
//						Point3[] op = m_markerCorners3d.toArray ();
//						for (int p=0; p<m_markerCorners3d.rows(); p++) {
//								Mat X = new Mat (4, 1, CvType.CV_64FC1);
//								X.put (0, 0, op [p].x, op [p].y, op [p].z, 1.0);
//								//Debug.Log ("X " + X.dump ());
//				
//
//								Mat opt_p = new Mat (4, 1, CvType.CV_64FC1);
//								Core.gemm (KP, X, 1, new Mat (4, 1, CvType.CV_64FC1), 0, opt_p);
//								//Debug.Log ("opt_p " + opt_p.dump ());
//				
//								Point opt_p_img = new Point (opt_p.get (0, 0) [0] / opt_p.get (2, 0) [0], opt_p.get (1, 0) [0] / opt_p.get (2, 0) [0]);
//								//Debug.Log ("opt_p_img " + opt_p_img.ToString ());
//				
//								Core.circle (bgraMat, opt_p_img, 4, new Scalar (0, 0, 255, 255), 1);
//
//								X.Dispose ();
//								opt_p.Dispose ();
//
//						}
//
//						P.Dispose ();
//						KP.Dispose ();
//				}

		}

		/// <summary>
		/// Finds the contours.
		/// </summary>
		/// <param name="thresholdImg">Threshold image.</param>
		/// <param name="contours">Contours.</param>
		/// <param name="minContourPointsAllowed">Minimum contour points allowed.</param>
		void findContours (Mat thresholdImg, List<MatOfPoint> contours, int minContourPointsAllowed)
		{
				List<MatOfPoint> allContours = new List<MatOfPoint> ();
				Imgproc.findContours (thresholdImg, allContours, new Mat (), Imgproc.RETR_LIST, Imgproc.CHAIN_APPROX_NONE);

		
				contours.Clear ();
				for (int i=0; i<allContours.Count; i++) {
						int contourSize = allContours [i].toArray ().Length;
						if (contourSize > minContourPointsAllowed) {
								contours.Add (allContours [i]);
						}
				}
		}

		/// <summary>
		/// Finds the candidates.
		/// </summary>
		/// <param name="contours">Contours.</param>
		/// <param name="detectedMarkers">Detected markers.</param>
		void findCandidates (List<MatOfPoint> contours, List<Marker> detectedMarkers)
		{
				MatOfPoint2f approxCurve = new MatOfPoint2f ();
				
				List<Marker> possibleMarkers = new List<Marker> ();
		
				// For each contour, analyze if it is a parallelepiped likely to be the marker
				for (int i=0; i<contours.Count; i++) {
						// Approximate to a polygon
						double eps = contours [i].toArray ().Length * 0.05;
						Imgproc.approxPolyDP (new MatOfPoint2f (contours [i].toArray ()), approxCurve, eps, true);

						Point[] approxCurveArray = approxCurve.toArray ();
			
						// We interested only in polygons that contains only four points
						if (approxCurveArray.Length != 4)
								continue;
			
						// And they have to be convex
						if (!Imgproc.isContourConvex (new MatOfPoint (approxCurveArray)))
								continue;

			
						// Ensure that the distance between consecutive points is large enough
						float minDist = float.MaxValue;

						for (int p = 0; p < 4; p++) {
								Point side = new Point (approxCurveArray [p].x - approxCurveArray [(p + 1) % 4].x, approxCurveArray [p].y - approxCurveArray [(p + 1) % 4].y);
								float squaredSideLength = (float)side.dot (side);
								minDist = Mathf.Min (minDist, squaredSideLength);
						}
			
						// Check that distance is not very small
						if (minDist < m_minContourLengthAllowed)
								continue;
			
						// All tests are passed. Save marker candidate:
						Marker m = new Marker ();
						m.points = new MatOfPoint ();

						List<Point> markerPointsList = new List<Point> ();
			
						for (int p = 0; p<4; p++)
								markerPointsList.Add (new Point (approxCurveArray [p].x, approxCurveArray [p].y));


			
						// Sort the points in anti-clockwise order
						// Trace a line between the first and second point.
						// If the third point is at the right side, then the points are anti-clockwise
						Point v1 = new Point (markerPointsList [1].x - markerPointsList [0].x, markerPointsList [1].y - markerPointsList [0].y);
						Point v2 = new Point (markerPointsList [2].x - markerPointsList [0].x, markerPointsList [2].y - markerPointsList [0].y);
			
						double o = (v1.x * v2.y) - (v1.y * v2.x);
			
						if (o < 0.0) {		 //if the third point is in the left side, then sort in anti-clockwise order
								Point tmp = markerPointsList [1];
								markerPointsList [1] = markerPointsList [3];
								markerPointsList [3] = tmp;

						}

						m.points.fromList (markerPointsList);
			
						possibleMarkers.Add (m);
				}
				approxCurve.Dispose ();

		        
                //Debug.Log ("possibleMarkers " + possibleMarkers.Count);
		
		
				// Remove these elements which corners are too close to each other.
				// First detect candidates for removal:
				List< Point > tooNearCandidates = new List<Point> ();
				for (int i=0; i<possibleMarkers.Count; i++) {
						Marker m1 = possibleMarkers [i];

						Point[] m1PointsArray = m1.points.toArray ();
			
						//calculate the average distance of each corner to the nearest corner of the other marker candidate
						for (int j=i+1; j<possibleMarkers.Count; j++) {
								Marker m2 = possibleMarkers [j];

								Point[] m2PointsArray = m2.points.toArray ();
				
								float distSquared = 0;
				
								for (int c = 0; c < 4; c++) {
										Point v = new Point (m1PointsArray [c].x - m2PointsArray [c].x, m1PointsArray [c].y - m2PointsArray [c].y);
										distSquared += (float)v.dot (v);
								}
				
								distSquared /= 4;
				
								if (distSquared < 100) {
										tooNearCandidates.Add (new Point (i, j));
								}
						}
				}
		
				// Mark for removal the element of the pair with smaller perimeter
				List<bool> removalMask = new List<bool> (possibleMarkers.Count);
				for (int i = 0; i < possibleMarkers.Count; i++) {
						removalMask.Add (false);
				}
		
				for (int i=0; i<tooNearCandidates.Count; i++) {

						float p1 = perimeter (possibleMarkers [(int)tooNearCandidates [i].x].points);
						float p2 = perimeter (possibleMarkers [(int)tooNearCandidates [i].y].points);
			
						int removalIndex;
						if (p1 > p2)
								removalIndex = (int)tooNearCandidates [i].x;
						else
								removalIndex = (int)tooNearCandidates [i].y;
			
						removalMask [removalIndex] = true;
				}
		
				// Return candidates
				detectedMarkers.Clear ();
				for (int i=0; i<possibleMarkers.Count; i++) {
						if (!removalMask [i])
								detectedMarkers.Add (possibleMarkers [i]);
				}
		}

	    /// <summary>
	    /// Recognizes the markers.
	    /// </summary>
	    /// <param name="grayscale">Grayscale.</param>
	    /// <param name="detectedMarkers">Detected markers.</param>
		void recognizeMarkers (Mat grayscale, List<Marker> detectedMarkers)
		{
				List<Marker> goodMarkers = new List<Marker> ();
		
				// Identify the markers
				for (int i=0; i<detectedMarkers.Count; i++) {
						Marker marker = detectedMarkers [i];

			
						// Find the perspective transformation that brings current marker to rectangular form
						Mat markerTransform = Imgproc.getPerspectiveTransform (new MatOfPoint2f (marker.points.toArray ()), m_markerCorners2d);
				

						// Transform image to get a canonical marker image
						Imgproc.warpPerspective (grayscale, canonicalMarkerImage, markerTransform, markerSize);
			
						MatOfInt nRotations = new MatOfInt (0);
						int id = Marker.getMarkerId (canonicalMarkerImage, nRotations);
						if (id != - 1) {
								marker.id = id;
//				                Debug.Log ("id " + id);

								//sort the points so that they are always in the same order no matter the camera orientation
								List<Point> MarkerPointsList = marker.points.toList ();

				//				std::rotate(marker.points.begin(), marker.points.begin() + 4 - nRotations, marker.points.end());
								MarkerPointsList = MarkerPointsList.Skip (4 - nRotations.toArray () [0]).Concat (MarkerPointsList.Take (4 - nRotations.toArray () [0])).ToList ();

								marker.points.fromList (MarkerPointsList);
				
								goodMarkers.Add (marker);
						}
						nRotations.Dispose ();
				}

//				Debug.Log ("goodMarkers " + goodMarkers.Count);
		
				// Refine marker corners using sub pixel accuracy
				if (goodMarkers.Count > 0) {
						List<Point> preciseCornersPoint = new List<Point> (4 * goodMarkers.Count);
						for (int i = 0; i < preciseCornersPoint.Capacity; i++) {
								preciseCornersPoint.Add (new Point (0, 0));
						}
						

			
						for (int i=0; i<goodMarkers.Count; i++) {
								Marker marker = goodMarkers [i];

								List<Point> markerPointsList = marker.points.toList ();
				
								for (int c = 0; c <4; c++) {
										preciseCornersPoint [i * 4 + c] = markerPointsList [c];
								}
						}

						MatOfPoint2f preciseCorners = new MatOfPoint2f (preciseCornersPoint.ToArray ());

						TermCriteria termCriteria = new TermCriteria (TermCriteria.MAX_ITER | TermCriteria.EPS, 30, 0.01);
						Imgproc.cornerSubPix (grayscale, preciseCorners, new Size (5, 5), new Size (-1, -1), termCriteria);

						preciseCornersPoint = preciseCorners.toList ();
			
						// Copy refined corners position back to markers
						for (int i=0; i<goodMarkers.Count; i++) {
								Marker marker = goodMarkers [i];

								List<Point> markerPointsList = marker.points.toList ();
				
								for (int c=0; c<4; c++) {
										markerPointsList [c] = preciseCornersPoint [i * 4 + c];
								}
						}
						preciseCorners.Dispose ();
				}

				detectedMarkers.Clear ();
				detectedMarkers.AddRange (goodMarkers);

		}

	    /// <summary>
	    /// Estimates the position.
	    /// </summary>
	    /// <param name="detectedMarkers">Detected markers.</param>
		void estimatePosition (List<Marker> detectedMarkers)
		{


				for (int i=0; i<detectedMarkers.Count; i++) {
						Marker m = detectedMarkers [i];
			
						Mat Rvec = new Mat ();
						Mat Tvec = new Mat ();
						Mat raux = new Mat ();
						Mat taux = new Mat ();
						Calib3d.solvePnP (m_markerCorners3d, new MatOfPoint2f (m.points.toArray ()), camMatrix, distCoeff, raux, taux);


						raux.convertTo (Rvec, CvType.CV_32F);
						taux.convertTo (Tvec, CvType.CV_32F);
			
						Mat rotMat = new Mat (3, 3, CvType.CV_64FC1);
						Calib3d.Rodrigues (Rvec, rotMat);


						m.transformation.SetRow (0, new Vector4 ((float)rotMat.get (0, 0) [0], (float)rotMat.get (0, 1) [0], (float)rotMat.get (0, 2) [0], (float)Tvec.get (0, 0) [0]));
						m.transformation.SetRow (1, new Vector4 ((float)rotMat.get (1, 0) [0], (float)rotMat.get (1, 1) [0], (float)rotMat.get (1, 2) [0], (float)Tvec.get (1, 0) [0]));
						m.transformation.SetRow (2, new Vector4 ((float)rotMat.get (2, 0) [0], (float)rotMat.get (2, 1) [0], (float)rotMat.get (2, 2) [0], (float)Tvec.get (2, 0) [0]));
						m.transformation.SetRow (3, new Vector4 (0, 0, 0, 1));

//						Debug.Log ("m.transformation " + m.transformation.ToString ());


						Rvec.Dispose ();
						Tvec.Dispose ();
						raux.Dispose ();
						taux.Dispose ();
						rotMat.Dispose ();

				}
		}

	    /// <summary>
	    /// Perimeter the specified a.
	    /// </summary>
	    /// <param name="a">The alpha component.</param>
		float perimeter (MatOfPoint a)
		{
				List<Point> aList = a.toList ();

				float sum = 0, dx = 0, dy = 0;
			
				for (int i=0; i<aList.Count; i++) {
						int i2 = (i + 1) % aList.Count;
				
						dx = (float)aList [i].x - (float)aList [i2].x;
						dy = (float)aList [i].y - (float)aList [i2].y;
				
						sum += Mathf.Sqrt (dx * dx + dy * dy);
				}
			
				return sum;
		}
		
	    /// <summary>
	    /// Ises the into.
	    /// </summary>
	    /// <returns><c>true</c>, if into was ised, <c>false</c> otherwise.</returns>
	    /// <param name="contour">Contour.</param>
	    /// <param name="b">The blue component.</param>
		bool isInto (MatOfPoint2f contour, List<Point> b)
		{
				for (int i=0; i<b.Count; i++) {
						if (Imgproc.pointPolygonTest (contour, b [i], false) > 0)
								return true;
			    
				}
				return false;
	
		}

}

using UnityEngine;
using System.Collections.Generic;

using OpenCVForUnity;

/// <summary>
/// Marker.
/// </summary>
public class Marker
{

		// Id of  the marker
		public int id;
	
		// Marker transformation with regards to the camera
		public Matrix4x4 transformation;

		//Marker Contour Points
		public MatOfPoint points;

		/// <summary>
		/// Initializes a new instance of the <see cref="Marker"/> class.
		/// </summary>
		public Marker ()
		{
				id = -1;
		}


		/// <summary>
		/// Rotate the specified inMat.
		/// </summary>
		/// <param name="inMat">In mat.</param>
		public static Mat rotate (Mat inMat)
		{
				byte[] b = new byte[1];

				Mat outMat = new Mat ();
				inMat.copyTo (outMat);
				for (int i=0; i<inMat.rows(); i++) {
						for (int j=0; j<inMat.cols(); j++) {
								inMat.get (inMat.cols () - j - 1, i, b);
								outMat.put (i, j, b);

						}
				}
				return outMat;
		}

		/// <summary>
		/// Hamms the dist marker.
		/// </summary>
		/// <returns>The dist marker.</returns>
		/// <param name="bits">Bits.</param>
		public static int hammDistMarker (Mat bits)
		{
				int[][] ids = new int[][]
		{
			new int[]{1,0,0,0,0},
			new int[]{1,0,1,1,1},
			new int[]{0,1,0,0,1},
			new int[]{0,1,1,1,0}
		};
		
				int dist = 0;

				byte[] b = new byte[1];
		
				for (int y=0; y<5; y++) {
						int minSum = 100000; //hamming distance to each possible word
			
						for (int p=0; p<4; p++) {
								int sum = 0;
								//now, count
								for (int x=0; x<5; x++) {

										bits.get (y, x, b);

										sum += (b [0] == ids [p] [x]) ? 0 : 1;
								}
				
								if (minSum > sum)
										minSum = sum;
						}
			
						//do the and
						dist += minSum;
				}
		
				return dist;
		}

		/// <summary>
		/// Mat2id the specified bits.
		/// </summary>
		/// <param name="bits">Bits.</param>
		public static int mat2id (Mat bits)
		{
				int val = 0;
				for (int y=0; y<5; y++) {
						val <<= 1;
						if (bits.get (y, 1) [0] == 1)
								val |= 1;

						val <<= 1;
						if (bits.get (y, 3) [0] == 1)
								val |= 1;

				}
				return val;
		}

		/// <summary>
		/// Gets the marker identifier.
		/// </summary>
		/// <returns>The marker identifier.</returns>
		/// <param name="markerImage">Marker image.</param>
		/// <param name="nRotations">N rotations.</param>
		public static int getMarkerId (Mat markerImage, MatOfInt nRotations)
		{

		
				Mat grey = markerImage;
		
				// Threshold image
				Imgproc.threshold (grey, grey, 125, 255, Imgproc.THRESH_BINARY | Imgproc.THRESH_OTSU);

		
				//Markers  are divided in 7x7 regions, of which the inner 5x5 belongs to marker info
				//the external border should be entirely black
		
				int cellSize = markerImage.rows () / 7;
		
				for (int y=0; y<7; y++) {
						int inc = 6;
			
						if (y == 0 || y == 6)
								inc = 1; //for first and last row, check the whole border
			
						for (int x=0; x<7; x+=inc) {
								int cellX = x * cellSize;
								int cellY = y * cellSize;
								Mat cell = new Mat (grey, new OpenCVForUnity.Rect (cellX, cellY, cellSize, cellSize));

				
								int nZ = Core.countNonZero (cell);

								cell.Dispose ();
				
								if (nZ > (cellSize * cellSize) / 2) {
										return -1;//can not be a marker because the border element is not black!
								}
						}
				}

				Mat bitMatrix = Mat.zeros (5, 5, CvType.CV_8UC1);
		
				//get information(for each inner square, determine if it is  black or white)  
				for (int y=0; y<5; y++) {
						for (int x=0; x<5; x++) {
								int cellX = (x + 1) * cellSize;
								int cellY = (y + 1) * cellSize;
								Mat cell = new Mat (grey, new OpenCVForUnity.Rect (cellX, cellY, cellSize, cellSize));
				
								int nZ = Core.countNonZero (cell);

								if (nZ > (cellSize * cellSize) / 2)
										bitMatrix.put (y, x, new byte[]{1});
								//bitMatrix.at<uchar> (y, x) = 1;

								cell.Dispose ();
						}
				}

//		Debug.Log ("bitMatrix " + bitMatrix.dump());
		
				//check all possible rotations
				Mat[] rotations = new Mat[4];
				for (int i = 0; i < rotations.Length; i++) {
						rotations [i] = new Mat ();
				}
				int[] distances = new int[4];
				
		
				rotations [0] = bitMatrix;  
				distances [0] = hammDistMarker (rotations [0]);



				int first = distances [0];
				int second = 0;
		
				for (int i=1; i<4; i++) {
						//get the hamming distance to the nearest possible word
						rotations [i] = rotate (rotations [i - 1]);
						distances [i] = hammDistMarker (rotations [i]);
			
						if (distances [i] < first) {
								first = distances [i];
								second = i;
						}
				}

//		Debug.Log ("first " + first);

				nRotations.fromArray (second);
				if (first == 0) {
						int id = mat2id (rotations [second]);


						bitMatrix.Dispose ();
						for (int i = 0; i < rotations.Length; i++) {
								rotations [i].Dispose ();
						}

						return id;
				}
		
				return -1;
		}

		/// <summary>
		/// Draws the contour.
		/// </summary>
		/// <param name="image">Image.</param>
		/// <param name="color">Color.</param>
		public void drawContour (Mat image, Scalar color)
		{
				Point[] pointsArray = points.toArray ();

				int thickness = 2;

				Core.line (image, pointsArray [0], pointsArray [1], color, thickness, Core.LINE_AA, 0);
				Core.line (image, pointsArray [1], pointsArray [2], color, thickness, Core.LINE_AA, 0);
				Core.line (image, pointsArray [2], pointsArray [3], color, thickness, Core.LINE_AA, 0);
				Core.line (image, pointsArray [3], pointsArray [0], color, thickness, Core.LINE_AA, 0);

		}

}

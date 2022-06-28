using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using UnityEngine;

namespace OpenCVMarkerBasedAR
{
    /// <summary>
    /// Marker.
    /// This code is a rewrite of https://github.com/MasteringOpenCV/code/tree/master/Chapter2_iPhoneAR using "OpenCV for Unity".
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
        public Marker()
        {
            id = -1;
        }

        /// <summary>
        /// Rotate the specified inMat.
        /// </summary>
        /// <param name="inMat">In mat.</param>
        public static Mat rotate(Mat inMat)
        {
            byte[] b = new byte[1];

            Mat outMat = new Mat();
            inMat.copyTo(outMat);
            for (int i = 0; i < inMat.rows(); i++)
            {
                for (int j = 0; j < inMat.cols(); j++)
                {
                    inMat.get(inMat.cols() - j - 1, i, b);
                    outMat.put(i, j, b);
                }
            }
            return outMat;
        }

        /// <summary>
        /// Hamms the dist marker.
        /// </summary>
        /// <returns>The dist marker.</returns>
        /// <param name="bits">Bits.</param>
        public static int hammDistMarker(Mat bits, byte[,] markerDesign)
        {
            int dist = 0;

            int size = markerDesign.GetLength(0);

            byte[] b = new byte[size * size];

            bits.get(0, 0, b);

            for (int y = 0; y < size; y++)
            {

                int sum = 0;

                for (int x = 0; x < size; x++)
                {

                    sum += (b[y * size + x] == markerDesign[y, x]) ? 0 : 1;
                }

                dist += sum;
            }

            return dist;
        }

        /// <summary>
        /// Mat2id the specified bits.
        /// </summary>
        /// <param name="bits">Bits.</param>
        public static int mat2id(Mat bits)
        {
            int size = bits.rows();
            byte[] bytes = new byte[size * size];
            bits.get(0, 0, bytes);
            bool[] boolArray = new bool[bytes.Length];
            for (int i = 0; i < boolArray.Length; i++)
            {
                if (bytes[i] == 1)
                {
                    boolArray[i] = false;
                }
                else
                {
                    boolArray[i] = true;
                }
            }
            return MarkerSettings.boolArray2id(boolArray);
        }

        /// <summary>
        /// Gets the marker identifier.
        /// </summary>
        /// <returns>The marker identifier.</returns>
        /// <param name="markerImage">Marker image.</param>
        /// <param name="nRotations">N rotations.</param>
        public static int getMarkerId(Mat markerImage, MatOfInt nRotations, byte[,] markerDesign)
        {
            Mat grey = markerImage;

            // Threshold image
            Imgproc.threshold(grey, grey, 125, 255, Imgproc.THRESH_BINARY | Imgproc.THRESH_OTSU);


            //Markers  are divided in 7x7 regions, of which the inner 5x5 belongs to marker info
            //the external border should be entirely black

            int size = markerDesign.GetLength(0);

            int cellSize = markerImage.rows() / (size + 2);

            for (int y = 0; y < (size + 2); y++)
            {
                int inc = size + 1;

                if (y == 0 || y == (size + 1))
                    inc = 1; //for first and last row, check the whole border

                for (int x = 0; x < (size + 2); x += inc)
                {
                    int cellX = x * cellSize;
                    int cellY = y * cellSize;
                    Mat cell = new Mat(grey, new OpenCVForUnity.CoreModule.Rect(cellX, cellY, cellSize, cellSize));


                    int nZ = Core.countNonZero(cell);

                    cell.Dispose();

                    if (nZ > (cellSize * cellSize) / 2)
                    {
                        return -1;//can not be a marker because the border element is not black!
                    }
                }
            }

            Mat bitMatrix = Mat.zeros(size, size, CvType.CV_8UC1);

            //get information(for each inner square, determine if it is  black or white)  
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int cellX = (x + 1) * cellSize;
                    int cellY = (y + 1) * cellSize;
                    Mat cell = new Mat(grey, new OpenCVForUnity.CoreModule.Rect(cellX, cellY, cellSize, cellSize));

                    int nZ = Core.countNonZero(cell);

                    if (nZ > (cellSize * cellSize) / 2)
                        bitMatrix.put(y, x, new byte[] { 1 });

                    cell.Dispose();
                }
            }

            //check all possible rotations
            Mat[] rotations = new Mat[4];
            for (int i = 0; i < rotations.Length; i++)
            {
                rotations[i] = new Mat();
            }
            int[] distances = new int[4];


            rotations[0] = bitMatrix;
            distances[0] = hammDistMarker(rotations[0], markerDesign);


            int first = distances[0];
            int second = 0;

            for (int i = 1; i < 4; i++)
            {
                //get the hamming distance to the nearest possible word
                rotations[i] = rotate(rotations[i - 1]);
                distances[i] = hammDistMarker(rotations[i], markerDesign);

                if (distances[i] < first)
                {
                    first = distances[i];
                    second = i;
                }
            }

            nRotations.fromArray(second);
            if (first == 0)
            {
                int id = mat2id(rotations[second]);


                bitMatrix.Dispose();
                for (int i = 0; i < rotations.Length; i++)
                {
                    rotations[i].Dispose();
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
        public void drawContour(Mat image, Scalar color)
        {
            Point[] pointsArray = points.toArray();

            int thickness = 2;

#if OPENCV_2
            Core.line (image, pointsArray [0], pointsArray [1], color, thickness, Core.LINE_AA, 0);
            Core.line (image, pointsArray [1], pointsArray [2], color, thickness, Core.LINE_AA, 0);
            Core.line (image, pointsArray [2], pointsArray [3], color, thickness, Core.LINE_AA, 0);
            Core.line (image, pointsArray [3], pointsArray [0], color, thickness, Core.LINE_AA, 0);
#else
            Imgproc.line(image, pointsArray[0], pointsArray[1], color, thickness, Imgproc.LINE_AA, 0);
            Imgproc.line(image, pointsArray[1], pointsArray[2], color, thickness, Imgproc.LINE_AA, 0);
            Imgproc.line(image, pointsArray[2], pointsArray[3], color, thickness, Imgproc.LINE_AA, 0);
            Imgproc.line(image, pointsArray[3], pointsArray[0], color, thickness, Imgproc.LINE_AA, 0);
#endif
        }
    }
}

namespace OpenCVMarkerBasedAR
{
    /// <summary>
    /// Marker design.
    /// This code is a rewrite of https://github.com/MasteringOpenCV/code/tree/master/Chapter2_iPhoneAR using "OpenCV for Unity".
    /// </summary>
    [System.Serializable]
    public class MarkerDesign
    {
        public int gridSize = 5;
        public bool[] data = new bool[5 * 5];
    }
}

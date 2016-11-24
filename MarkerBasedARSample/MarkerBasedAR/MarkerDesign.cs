using UnityEngine;
using System.Collections;

namespace OpenCVMarkerBasedAR
{
    [System.Serializable]
    public class MarkerDesign
    {
    
        public int gridSize = 5;
        public bool[] data = new bool[5 * 5];


    }
}

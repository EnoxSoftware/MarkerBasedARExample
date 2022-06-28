using System.Collections.Generic;
using UnityEngine;

namespace OpenCVMarkerBasedAR
{
    /// <summary>
    /// Marker settings.
    /// This code is a rewrite of https://github.com/MasteringOpenCV/code/tree/master/Chapter2_iPhoneAR using "OpenCV for Unity".
    /// </summary>
    public class MarkerSettings : MonoBehaviour
    {
        /// <summary>
        /// The marker design.
        /// </summary>
        public MarkerDesign markerDesign;

        /// <summary>
        /// The AR game object.
        /// </summary>
        public GameObject ARGameObject;

        /// <summary>
        /// The displayable max count.
        /// </summary>
        [Range(1, 50)]
        public int displayableMaxCount;

        /// <summary>
        /// The AR game object diplicates.
        /// </summary>
        private List<GameObject> ARGameObjectDiplicates;

        /// <summary>
        /// The should not set to inactive per frame.
        /// </summary>
        [Tooltip("If true, displayableMaxCount is limited to 1.")]
        public bool
            shouldNotSetToInactivePerFrame;

        /// <summary>
        /// Gets the marker identifier.
        /// </summary>
        /// <returns>The marker identifier.</returns>
        public int getMarkerId()
        {
            return boolArray2id(markerDesign.data);
        }

        public static int boolArray2id(bool[] boolArray)
        {
            int id = 0;
            System.Text.StringBuilder bitString = new System.Text.StringBuilder(32);
            for (int i = 0; i < boolArray.Length; i++)
            {
                if (boolArray[i])
                {
                    bitString.Append(1);
                }
                else
                {
                    bitString.Append(0);
                }

                if (i > 0 && i % 31 == 0)
                {
                    id = id + System.Convert.ToInt32(bitString.ToString(), 2);
                    bitString.Length = 0;
                }
            }

            id = id + System.Convert.ToInt32(bitString.ToString(), 2);

            id = id + boolArray.Length;

            return id;
        }

        /// <summary>
        /// Gets the AR game object.
        /// </summary>
        /// <returns>The AR game object.</returns>
        public GameObject getARGameObject()
        {
            if (shouldNotSetToInactivePerFrame)
                return ARGameObjectDiplicates[0];
            foreach (GameObject item in ARGameObjectDiplicates)
            {
                if (!item.activeSelf)
                    return item;
            }
            return null;
        }

        /// <summary>
        /// Sets all AR game objects disable.
        /// </summary>
        public void setAllARGameObjectsDisable()
        {
            if (shouldNotSetToInactivePerFrame)
                return;
            foreach (GameObject item in ARGameObjectDiplicates)
            {
                item.SetActive(false);
            }
        }

        void Awake()
        {
            if (shouldNotSetToInactivePerFrame)
                displayableMaxCount = 1;
            if (displayableMaxCount < 1)
                displayableMaxCount = 1;
            ARGameObjectDiplicates = new List<GameObject>();
            ARGameObjectDiplicates.Add(ARGameObject);
            for (int i = 1; i < displayableMaxCount; i++)
            {
                GameObject diplicate = GameObject.Instantiate(ARGameObject);
                diplicate.transform.parent = this.transform;
                ARGameObjectDiplicates.Add(diplicate);
            }

            foreach (GameObject item in ARGameObjectDiplicates)
            {
                item.SetActive(false);
            }
        }
    }
}

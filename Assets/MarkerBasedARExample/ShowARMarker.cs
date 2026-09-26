using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MarkerBasedARExample
{
    /// <summary>
    /// Show ARMarker.
    /// </summary>
    public class ShowARMarker : MonoBehaviour
    {
        [Header("Output")]
        /// <summary>
        /// The RawImage for previewing the result.
        /// </summary>
        public RawImage resultPreview;

        [Space(10)]

        /// <summary>
        /// Show ARMarker
        /// </summary>
        public Texture2D[] markerTexture;

        /// <summary>
        /// The index.
        /// </summary>
        private int index = 0;

        // Use this for initialization
        private void Start()
        {
            Texture2D texture = markerTexture[index];
            resultPreview.texture = texture;
            resultPreview.GetComponent<AspectRatioFitter>().aspectRatio = (float)texture.width / texture.height;
        }

        // Update is called once per frame
        private void Update()
        {

        }

        /// <summary>
        /// Raises the disable event.
        /// </summary>
        private void OnDisable()
        {

        }

        /// <summary>
        /// Raises the back button event.
        /// </summary>
        public void OnBackButtonClick()
        {
            SceneManager.LoadScene("MarkerBasedARExample");
        }

        /// <summary>
        /// Raises the change marker button event.
        /// </summary>
        public void OnChangeMarkerButtonClick()
        {
            index = (index + 1) % markerTexture.Length;

            Texture2D texture = markerTexture[index];
            resultPreview.texture = texture;
            resultPreview.GetComponent<AspectRatioFitter>().aspectRatio = (float)texture.width / texture.height;
        }
    }
}

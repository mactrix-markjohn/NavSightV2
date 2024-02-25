using System.Collections;
using System.Collections.Generic;
using MoreMountains.NiceVibrations;
using Niantic.Lightship.AR.ObjectDetection;
using Niantic.Lightship.AR.Semantics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;


public class NavARController : MonoBehaviour
{
    // ----- Meshing Variables -------------
    public Camera Camera;
    public AudioSource audioSource;
    public AudioClip audioClip;

   

    //Debug text

    public TextMeshProUGUI hitDisText;
    public TextMeshProUGUI isGroundText;
    public Image centerOfScreenImage;

    // --------------------------------------



    // ------ Semantic Segmentation Variables -------------

    public ARSemanticSegmentationManager semanticMan;

    private string channel = "ground";
    private string semanticName = "";


    // Each feature channel number corresponds to a label, first is depth and the rest is from
    // semantics channel names.
    private bool isGroundChannel = false;

    // ----------------------------------------------------


    // ------ Object Detection Variables ------------------

    [SerializeField]
    private ARObjectDetectionManager _objectDetectionManager;

    private string objectDetectedName = "";

    // -----------------------------------------------------


    // ----- Text to Speech Engine ------------------------

    public TextToSspeechController textToSspeechController;


    // Start is called before the first frame update
    void Start()
    {
        _objectDetectionManager.enabled = true;
        _objectDetectionManager.MetadataInitialized += OnMetadataInitialized;

        // instruct the users
        Speak("Point the phone camera to your front as you move ");
    }

    //float counterSec = 1;
    //bool endOfTime = false;

    // Update is called once per frame
    void Update()
    {
        SemanticSegmentationUpdate();
        RaycastAndDistance();

        //counterSec -= Time.deltaTime;

        //if (counterSec <= 0)
        //{
        //    // time has ended

        //    textToSspeechController.SpeakClick("This shit is working");
        //    counterSec = 1;
        //}

    }

    // Meshing functions

    public void RaycastAndDistance()
    {

        //var currentFrame = _session.CurrentFrame;
        //if (currentFrame == null) return;

        if (Camera == null) return;

        Vector2 centerOfScreen = new Vector2(Screen.width / 2f, Screen.height / 2f);
        centerOfScreenImage.rectTransform.position = centerOfScreen;

        var worldRay = Camera.ScreenPointToRay(centerOfScreen);
        RaycastHit hit;

        if (Physics.Raycast(worldRay, out hit, 1000f))
        {

            if (hit.transform.gameObject.name.Contains("Mesh") || hit.transform.gameObject.name.Contains("Interior_"))
            {
                Vector3 hitPosition = hit.point;

                float hitDistanceFromSource = hit.distance;
                float distanceFromCamera = Vector3.Distance(Camera.transform.position, hit.point);

                string hitDisMsg = $"{hitDistanceFromSource}";
                string cameraDisMsg = $"{distanceFromCamera}";

                hitDisText.text = hitDisMsg;
                //cameraDisText.text = cameraDisMsg;


                Debug.Log($"Hit.distance result: {hitDistanceFromSource}");
                Debug.Log($"Distance from Camera calculation: {distanceFromCamera}");




                // check if the distance of the user to the mesh is less than 0.6 and 
                // the mesh is not the ground. Then Vibrate and play sound effect

                if (hitDistanceFromSource < 1.0f && !isGroundChannel)
                {
                    // Start vibration and play sound effect

                    StartHapticVib();
                    audioSource.PlayOneShot(audioClip);

                    // Tell the user want they see

                    string[] vowels = { "a", "e", "i", "o", "u" };

                    string aORan = "a";

                    if (aORan.Contains(objectDetectedName.ToLower()[0]))
                    {
                        aORan = "a";
                    }
                    else
                    {
                        aORan = "an";
                    }

                    string spokenwords = $"You are looking at {aORan} {objectDetectedName}";

                    Speak(spokenwords);

                    //Speak(semanticName);

                }



            }

        }

    }

    public void StartHapticVib()
    {
        Handheld.Vibrate();
        MMVibrationManager.Haptic(HapticTypes.Warning);


    }


    // Semantic Segmentation functions

    public void SemanticSegmentationUpdate()
    {
        if (!semanticMan.subsystem.running)
        {
            return;
        }


        // Center of Screen sematics

        // check if the center of the screen is pointing to the ground
        isGroundChannel = semanticMan.DoesChannelExistAt(Screen.width / 2, Screen.height / 2, channel);

        if (isGroundChannel)
        {
            isGroundText.text = "True";
        }
        else
        {
            isGroundText.text = "False";
        }


        // Get the name of the sematic at the center of the screen
        var list = semanticMan.GetChannelNamesAt(Screen.width / 2, Screen.height / 2);

        

        if (list.Count > 0)
        {
            semanticName = list[0];

            // Text to Speech
            //Speak(semanticName);

        }
        else
        {
            semanticName = "";
        }

    }

    public void Speak(string input)
    {
        textToSspeechController.SpeakClick(input);
    }

    // Object detection

    private void OnMetadataInitialized(ARObjectDetectionModelEventArgs args)
    {
        _objectDetectionManager.ObjectDetectionsUpdated += ObjectDetectionsUpdated;

    }

    private void ObjectDetectionsUpdated(ARObjectDetectionsUpdatedEventArgs args)
    {
        //Initialize our output string
        string resultString = "";
        var result = args.Results;

        if (result == null)
        {
            Debug.Log("No results found.");
            return;
        }

        //Reset our results string
        resultString = "";

        //Iterate through our results
        for (int i = 0; i < result.Count; i++)
        {
            var detection = result[i];
            var categorizations = detection.GetConfidentCategorizations(0.45f);
            if (categorizations.Count <= 0)
            {
                break;
            }

            //Sort our categorizations by highest confidence
            categorizations.Sort((a, b) => b.Confidence.CompareTo(a.Confidence));

            //Get the category
            var category = categorizations[0];
            objectDetectedName = category.CategoryName;

            //Text to Speech 
            //Speak(objectDetectedName);


            //Iterate through found categoires and form our string to output
            for (int j = 0; j < categorizations.Count; j++)
            {
                var categoryToDisplay = categorizations[j];

                resultString += "Detected " + $"{categoryToDisplay.CategoryName}: " + "with " + $"{categoryToDisplay.Confidence} Confidence \n";
            }
        }

        //Output our string
        //_objectsDetectedText.text = resultString;
    }

    private void OnDestroy()
    {
        _objectDetectionManager.MetadataInitialized -= OnMetadataInitialized;
        _objectDetectionManager.ObjectDetectionsUpdated -= ObjectDetectionsUpdated;
    }


}

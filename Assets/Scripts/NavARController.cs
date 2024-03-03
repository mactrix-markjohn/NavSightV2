using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.NiceVibrations;
using Niantic.Lightship.AR.ObjectDetection;
using Niantic.Lightship.AR.Semantics;
using NN;
using TMPro;
using Unity.Barracuda;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Assets.Scripts;
using Assets.Scripts.TextureProviders;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine.Profiling;
using System.Linq;
using Meta.WitAi.TTS.Utilities;
using Meta.WitAi.TTS.Data;

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



    // condition to stop alert
    bool isSpeaking = false;


    //-------------- AR Camera Manager -------------------

    public ARCameraManager cameraManager;
    Texture2D m_Texture;



    // YOLO v8 instances

    [Tooltip("File of YOLO model.")]
    [SerializeField]
    protected NNModel ModelFile;

    [Range(0.0f, 1f)]
    [Tooltip("The minimum value of box confidence below which boxes won't be drawn.")]
    [SerializeField]
    protected float MinBoxConfidence = 0.3f;

    protected NNHandler nn;

    YOLOv8 yolo;


    [Tooltip("Text file with classes names separated by coma ','")]
    public TextAsset ClassesTextFile;
    XRCpuImage.Transformation m_Transformation = XRCpuImage.Transformation.MirrorY;


    string[] classesNames;

    public TextMeshProUGUI objectDetectedText;


    // Start is called before the first frame update
    void Start()
    {
        _objectDetectionManager.enabled = true;
        _objectDetectionManager.MetadataInitialized += OnMetadataInitialized;

        // instruct the users
        //Speak("Point the phone camera to your front as you move ");
    }

    //float counterSec = 1;
    //bool endOfTime = false;

    // Update is called once per frame
    void Update()
    {
        SemanticSegmentationUpdate();
        RaycastAndDistance();



        //YOLOUpdate();



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
                    //if (!isSpeaking)
                    //{
                        
                    //}
                    

                    // Tell the user want they see

                    char[] vowels = { 'a', 'e', 'i', 'o', 'u' };

                    string aORan = "a";

                    if (vowels.Contains<char>(objectDetectedName.ToLower()[0]))
                    {
                        aORan = "an";
                    }
                    else
                    {
                        aORan = "a";
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
        audioSource.Pause();
        audioSource.Stop();
        
        isSpeaking = true;
        textToSspeechController.SpeakClick(input);
        StartCoroutine(IsStopSpeaking());
    }

    IEnumerator IsStopSpeaking()
    {
        yield return new WaitForSeconds(5f);
        //isSpeaking = false;
    }

    public void OnStopSpeaking(TTSSpeaker s, TTSClipData d)
    {
        isSpeaking = false;
    }

    public void OnStopSpeakingTextPlayback(string s)
    {
        isSpeaking = false;
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
            var categorizations = detection.GetConfidentCategorizations(0.5f);
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




    // -------- Enable and Disable ----------

    private void OnEnable()
    {
        //cameraManager.frameReceived += OnCameraFrameReceived;

        //nn = new NNHandler(ModelFile);
        //yolo = new YOLOv8Segmentation(nn);

       

        //classesNames = ClassesTextFile.text.Split('\n');
    }

    

    private void OnDisable()
    {
        //cameraManager.frameReceived -= OnCameraFrameReceived;
        //nn.Dispose();
    }

    unsafe void OnCameraFrameReceived(ARCameraFrameEventArgs obj)
    {
        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
            return;

        // Choose an RGBA format.
        // See XRCpuImage.FormatSupported for a complete list of supported formats.
        const TextureFormat format = TextureFormat.RGBA32;

        if (m_Texture == null || m_Texture.width != image.width || m_Texture.height != image.height)
            m_Texture = new Texture2D(image.width, image.height, format, false);

        // Convert the image to format, flipping the image across the Y axis.
        // We can also get a sub rectangle, but we'll get the full image here.
        var conversionParams = new XRCpuImage.ConversionParams(image, format, m_Transformation);

        // Texture2D allows us write directly to the raw texture data
        // This allows us to do the conversion in-place without making any copies.
        var rawTextureData = m_Texture.GetRawTextureData<byte>();
        try
        {
            image.ConvertAsync(conversionParams);
        }
        finally
        {
            // We must dispose of the XRCpuImage after we're finished
            // with it to avoid leaking native resources.
            image.Dispose();
        }

        // Apply the updated texture data to our texture
        m_Texture.Apply();


    }

    void YOLOUpdate()
    {
        YOLOv8OutputReader.DiscardThreshold = MinBoxConfidence;
        if (m_Texture == null)
            return;

        Texture2D texture = m_Texture; //GetNextTexture();

        var boxes = yolo.Run(texture);

        DrawResults(boxes, texture);
    }


    protected void DrawResults(IEnumerable<ResultBox> results, Texture2D img)
    {

        //results.ForEach(box => DrawBox(box, img));

        foreach (var box in results)
        {
            // Access properties or methods of the ResultBox object here
            DrawBox(box, img);
        }
    }

    protected virtual void DrawBox(ResultBox box, Texture2D img)
    {
        
        int boxWidth = (int)(box.score / MinBoxConfidence);
       
        //Debug.Log(box.bestClassIndex);
        if (box.bestClassIndex > classesNames.Length - 1)
        {
            return;
        }
        string classLebal = classesNames[box.bestClassIndex + 1];

        string extractedClass = classLebal.Substring(classLebal.IndexOf(":"),classLebal.Length);
        Debug.Log(extractedClass);

        objectDetectedText.text = extractedClass;
        objectDetectedName = extractedClass;
    }




}

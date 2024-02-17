//// Copyright 2022 Niantic, Inc. All Rights Reserved.

//using System;
//using System.Collections;
//using System.Collections.Generic;
//using MoreMountains.NiceVibrations;
//using UnityEngine;

//using Niantic.ARDK.AR;
//using Niantic.ARDK.AR.ARSessionEventArgs;
//using Niantic.ARDK.AR.Awareness;
//using Niantic.ARDK.AR.Awareness.Semantics;
//using Niantic.ARDK.AR.Mesh;
//using Niantic.ARDK.Extensions.Meshing;
//using Niantic.ARDK.Extensions;
//using Niantic.ARDK.Utilities.Logging;
//using Niantic.ARDK.VirtualStudio.AR.Mock;
//using Niantic.ARDK.Utilities.Input.Legacy;
//using Niantic.LightshipHub.Templates;
//using TMPro;
//using UnityEngine.UI;


//public class NavController : MonoBehaviour
//{
    
//	public Camera Camera;
//	public AudioSource audioSource;
//	public AudioClip audioClip;
	
//	private IARSession _session;


//	//Debug text 
//	public TextMeshProUGUI hitDisText;
//	public TextMeshProUGUI cameraDisText;
//	public Image centerOfScreenImage;
	
//	// Semantic Segmentation fields
	
//	[Header("Context Awareness Managers")]
//	[SerializeField]
//	private ARDepthManager _depthManager = null;

//	[SerializeField]
//	private ARSemanticSegmentationManager _semanticSegmentationManager = null;
	
//	private bool _isDisplayingContextAwareness = false;
//	private bool _isSemanticsTextureDirty;
//	private Texture2D _semanticsTexture;
	
//	// Each feature channel number corresponds to a label, first is depth and the rest is from
//	// semantics channel names.
//	private int _featureChannel = 0;
//	private bool isGroundChannel = false;
	
	
	
	
	
//	private void Awake() 
//	{
		
//	}
	
	
//	private void OnEnable()
//	{
//		_semanticSegmentationManager.SemanticBufferInitialized += OnSemanticBufferInitialized;
//		_semanticSegmentationManager.SemanticBufferUpdated += OnSemanticBufferUpdated;
//	}

//	private void OnDisable()
//	{
//		_semanticSegmentationManager.SemanticBufferInitialized -= OnSemanticBufferInitialized;
//		_semanticSegmentationManager.SemanticBufferUpdated -= OnSemanticBufferUpdated;
//	}


//	void Start() 
//	{
      
//		ARSessionFactory.SessionInitialized += OnSessionInitialized;
      
//	}

//    private void OnSessionInitialized(AnyARSessionInitializedArgs args)
//    {
//      ARSessionFactory.SessionInitialized -= OnSessionInitialized;
//      _session = args.Session;
      
//    }

//	void Update() 
//	{
//		RaycastAndDistance();
//	}


//	public void RaycastAndDistance()
//	{
			
//		var currentFrame = _session.CurrentFrame;
//		if (currentFrame == null) return;

//		if (Camera == null) return;
			
//		Vector2 centerOfScreen = new Vector2(Screen.width / 2f, Screen.height / 2f);
//		centerOfScreenImage.rectTransform.position = centerOfScreen;
			
//		var worldRay = Camera.ScreenPointToRay(centerOfScreen);
//		RaycastHit hit;

//		if (Physics.Raycast(worldRay, out hit, 1000f))
//		{

//			if (hit.transform.gameObject.name.Contains("MeshCollider") || hit.transform.gameObject.name.Contains("Interior_"))
//			{
//				Vector3 hitPosition = hit.point;
				
//				float hitDistanceFromSource = hit.distance;
//				float distanceFromCamera = Vector3.Distance(Camera.transform.position, hit.point);

//				string hitDisMsg = $"{hitDistanceFromSource}";
//				string cameraDisMsg = $"{distanceFromCamera}";

//				hitDisText.text = hitDisMsg;
//				//cameraDisText.text = cameraDisMsg;
				
				
//				Debug.Log($"Hit.distance result: {hitDistanceFromSource}");
//				Debug.Log($"Distance from Camera calculation: {distanceFromCamera}");

				
				

//				// check if the distance of the user to the mesh is less than 0.6 and 
//				// the mesh is not the ground. Then Vibrate and play sound effect
				
//				if (hitDistanceFromSource < 0.6f && !isGroundChannel)
//				{
//					// Start vibration and play sound effect
					
//					StartHapticVib();
//					audioSource.PlayOneShot(audioClip);
					
					
					
//				}
				


//			}

//		}

//	}
	
//	public void StartHapticVib()
//	{
//		Handheld.Vibrate();
//		MMVibrationManager.Haptic(HapticTypes.Warning);
        
        
//	}


//	private void OnSemanticBufferInitialized(ContextAwarenessArgs<ISemanticBuffer> args)
//	{
//		_isSemanticsTextureDirty = true;
//	}

//	private void OnSemanticBufferUpdated(ContextAwarenessStreamUpdatedArgs<ISemanticBuffer> args)
//	{
//		_isSemanticsTextureDirty = _isSemanticsTextureDirty || _featureChannel > 0;

//		string ground = "ground";
		
//		Vector2 centerOfScreen = new Vector2(Screen.width / 2f, Screen.height / 2f);
//		Vector2 viewportPoint = Camera.ScreenToViewportPoint(centerOfScreen);
		
//		isGroundChannel = args.Sender.AwarenessBuffer.DoesChannelExistAt(viewportPoint, "ground");
		

//		if (isGroundChannel)
//		{
//			cameraDisText.text = "True";
//		}
//		else
//		{
//			cameraDisText.text = "False";
//		}

//	}


//}

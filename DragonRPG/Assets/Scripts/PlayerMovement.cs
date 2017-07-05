﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;
using UnityStandardAssets.CrossPlatformInput;


[RequireComponent(typeof(ThirdPersonCharacter))]
[RequireComponent(typeof(CameraRaycaster))]
public class PlayerMovement : MonoBehaviour
{

    [SerializeField] private float _walkMoveStopRadius = 0.2f;

    private bool _isInDirectMode = false;
    private ThirdPersonCharacter _character;
    private CameraRaycaster _cameraRaycaster;
    private Vector3 _currentClickTarget;

    
	private void Start ()
	{
	    _character = GetComponent<ThirdPersonCharacter>();
	    _cameraRaycaster = Camera.main.GetComponent<CameraRaycaster>();
	    _currentClickTarget = transform.position;
	}
	
	private void FixedUpdate ()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            _isInDirectMode = !_isInDirectMode;
            _currentClickTarget = transform.position;
        }

        if (_isInDirectMode)
        {
            ProcessDirectMovement();
        }
        else
        {
            ProcessMouseMovement();
        }
    }

    private void ProcessDirectMovement()
    {
        float h = CrossPlatformInputManager.GetAxis("Horizontal");
        float v = CrossPlatformInputManager.GetAxis("Vertical");
        
        // calculate camera relative direction to move:
        Vector3 camForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 move = v * camForward + h * Camera.main.transform.right;

        _character.Move(move, false, false);
    }

    private void ProcessMouseMovement()
    {
        if (Input.GetMouseButton(0))
        {
            switch (_cameraRaycaster.LayerHit)
            {
                case Layer.Enemy:
                    Debug.Log("Not moving enemy.");
                    break;
                case Layer.Walkable:
                    _currentClickTarget = _cameraRaycaster.Hit.point;
                    break;
                default:
                    Debug.Log("Unexpected layer found.");
                    return;
            }
        }

        var playerToClickPoint = _currentClickTarget - transform.position;

        _character.Move(
            playerToClickPoint.magnitude >= _walkMoveStopRadius ? playerToClickPoint : Vector3.zero,
            false,
            false);
    }
}

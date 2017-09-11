﻿using UnityEngine;

public class AudioTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip _clip;
    [SerializeField] private float _triggerRadius = 5f;
    [SerializeField] private bool _isOneTimeOnly = true;
    [SerializeField] private bool _hasPlayed = false;

    private AudioSource _audioSource;

    void Start()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.volume = 0.5f;
        _audioSource.playOnAwake = false;
        _audioSource.clip = _clip;

        AddAndSetSphereCollider();
    }

    private void AddAndSetSphereCollider()
    {
        SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        sphereCollider.radius = _triggerRadius;
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast"); //TODO: fix it
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            RequestPlayAudioClip();
        }
    }

    void RequestPlayAudioClip()
    {
        if (_isOneTimeOnly && _hasPlayed)
        {
            return;
        }
        else if (_audioSource.isPlaying == false)
        {
            _audioSource.Play();
            _hasPlayed = true;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 255f, 0, .5f);
        Gizmos.DrawWireSphere(transform.position, _triggerRadius);
    }
}
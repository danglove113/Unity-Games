﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using _Camera;
using _Characters.Enemies;
using _Characters.SpecialAbilities;
using _Core;
// TODO: Consider rewiring
using _Weapons;


namespace _Characters
{
    public class Player : MonoBehaviour, IDamageable
    {

        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _baseDamage = 10f;
        [SerializeField] private Weapon _weaponInUse;
        [SerializeField] private AnimatorOverrideController _animatorOverrideController;
        [SerializeField] private List<SpecialAbilityConfig> _specialAbilities;
        [SerializeField] private List<AudioClip> _deathSounds;
        [SerializeField] private List<AudioClip> _takeDamageSounds;

        private float _currentHealth;
        private float _lastHitTime;
        private GameObject _currentTarget;
        private CameraRaycaster _cameraRaycaster;
        private Animator _animator;
        private AudioSource _audioSource;
        private bool _isDying;

        private const string DeathTrigger = "DeathTrigger";
        private const string AttackTrigger = "AttackTrigger";
        private const string AttackAnimationName = "DEAFAULT ATTACK";
       

        void Start()
        {
            RegisterForMouseClick();
            SetCurrentHealthToMax();
            PutWeaponInHand();
            SetAnimator();
            SetAudioSource();
            _specialAbilities[0].AttachComponentTo(gameObject);
        }

        public float HealthAsPercentage
        {
            get { return _currentHealth / _maxHealth; }
        }

        public void TakeDamage(float damage)
        {
            if (_isDying) return;

            PlaySound(GetRandomClipFrom(_takeDamageSounds));
            _currentHealth = Mathf.Clamp(_currentHealth - damage, 0f, _maxHealth);

            if (_currentHealth <= 0)
            {
                StartCoroutine(KillPlayer());
            }
        }

        private IEnumerator KillPlayer()
        {
            var deathClip = GetRandomClipFrom(_deathSounds);
            _isDying = true;
            PlaySound(deathClip);
            _animator.SetTrigger(DeathTrigger);
            yield return new WaitForSeconds(deathClip.length);

            SceneManager.LoadScene(0);
        }

        private void PlaySound(AudioClip clip)
        {
            _audioSource.clip = clip;
            _audioSource.Play();
        }

        private void SetCurrentHealthToMax()
        {
            _currentHealth = _maxHealth;
        }

        private void SetAnimator()
        {
            _animator = GetComponent<Animator>();
            _animator.runtimeAnimatorController = _animatorOverrideController;
            _animatorOverrideController[AttackTrigger] = _weaponInUse.GetAttackAnimationClip();
        }

        private void SetAudioSource()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        private void PutWeaponInHand()
        {
            var weaponPrefab = _weaponInUse.GetWeaponPrefab();
            var dominantHand = RequestDominantHand();
            var spawnedWeapon = Instantiate(weaponPrefab, dominantHand.transform);

            spawnedWeapon.transform.localPosition = _weaponInUse.gripTransform.localPosition;
            spawnedWeapon.transform.localRotation = _weaponInUse.gripTransform.localRotation;
        }

        private GameObject RequestDominantHand()
        {
            var dominantHands = GetComponentsInChildren<DominantHand>();
            int dominantHandsCount = dominantHands.Length;

            Assert.AreNotEqual(0, dominantHandsCount, "No dominant hand for player.");
            Assert.IsFalse(dominantHandsCount > 1, "Multiple dominant hands for player.");

            return dominantHands[0].gameObject;
        }

        private void RegisterForMouseClick()
        {
            _cameraRaycaster = FindObjectOfType<CameraRaycaster>();
            _cameraRaycaster.onMouseOverEnemy += OnMouseOverEnemy;
        }

        private void OnMouseOverEnemy(Enemy enemy)
        {
            if (Input.GetMouseButton(0) && IsTargetInRange(enemy.gameObject))
            {
                AttackTarget(enemy.gameObject);
            }

            if (Input.GetMouseButtonDown(1))
            {
                AttemptSpecialAbility(0, enemy);
            }
        }

        private void AttemptSpecialAbility(int abilityIndex, Enemy enemy)
        {
            var energyComponent = GetComponent<Energy>();
            float energyCost = _specialAbilities[abilityIndex].GetEnergyCost();

            if (energyComponent.IsEnergyAvailable(energyCost))
            {
                energyComponent.ProcessEnergy(energyCost);
                //Self heal configuration
                var specialAbilityParams = new SpecialAbilityParams(gameObject.GetComponent<Player>(), 0);
                _specialAbilities[abilityIndex].UseAbility(specialAbilityParams);
            }
        }

        private bool IsTargetInRange(GameObject target)
        {
            var distanceToTarget = (target.transform.position - transform.position).magnitude;
            return distanceToTarget <= _weaponInUse.GetMaxAttackRange();
        }

        private void AttackTarget(GameObject enemy)
        {
            _currentTarget = enemy;

            if (Time.time - _lastHitTime > _weaponInUse.GetAttackCooldown())
            {
                _animator.SetTrigger(AttackTrigger);
                _currentTarget.GetComponent<Enemy>().TakeDamage(_baseDamage);
                _lastHitTime = Time.time;
            }
        }

        private AudioClip GetRandomClipFrom(List<AudioClip> sounds)
        {
            int randomIndex = Random.Range(0, sounds.Count);
            return sounds[randomIndex];
        }
    }
}

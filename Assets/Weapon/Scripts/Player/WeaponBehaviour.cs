using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WeaponBehaviour : MonoBehaviour
{

    [SerializeField, Range(0.1f, 10.0f)] private float bulletsPerSecond;


    [SerializeField] private float damage;
    [SerializeField] private AudioClip firingSound;
    [SerializeField] private GameObject bulletHole;
    [SerializeField] private Light firingLight;
    [SerializeField] private float lightIntensity;
    [SerializeField] private ParticleSystem smokeSystem;
    [SerializeField] private SpriteRenderer muzzleFlash;
    [SerializeField] private Animator handAnimator;
    [SerializeField] private Animator magnumMainAnimator;
    [SerializeField] private Animator magnumSlideAnimator;
    private static readonly int Shoot = Animator.StringToHash("Shoot");


    private AudioSource _audioSource;

    private float _coolDown;
    private float _timer;

    private float _lightFade;

    private Ray _ray;
    private RaycastHit _rayHit;


    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        muzzleFlash.enabled = false;
    }

    void Start()
    {
        _coolDown = 1.0f / bulletsPerSecond;
        _lightFade = lightIntensity / 4;
    }

    
    void Update()
    {
        if (firingLight.intensity != 0) firingLight.intensity -= _lightFade;
        if (muzzleFlash.enabled) muzzleFlash.enabled = false;
        _timer += Time.deltaTime;
        
        if(_timer < _coolDown) return;
        
        Fire();
    }

    private void Fire()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            muzzleFlash.enabled = true;
            firingLight.intensity = lightIntensity;
            handAnimator.SetTrigger(Shoot);
            magnumMainAnimator.SetTrigger(Shoot);
            magnumSlideAnimator.SetTrigger(Shoot);

            _audioSource.pitch = Random.Range(.95f, 1.05f);
            _audioSource.PlayOneShot(firingSound);
            smokeSystem.Play();

            HitScan();

            _timer = 0f;
            EventHandler.PlayerWeaponFiredEvent(transform.position);
        }
    }

    private void HitScan()
    {
        var transformCache = transform;
        _ray = new Ray(transformCache.position, transformCache.forward);

        if (Physics.Raycast(_ray, out _rayHit, 1000f))
        {

            if (_rayHit.collider.gameObject.isStatic)
            {
                var hole = Instantiate(bulletHole, _rayHit.point, Quaternion.identity);
                hole.transform.forward = _rayHit.normal;
                hole.transform.position += _rayHit.normal * .001f;
                hole.transform.SetParent(_rayHit.transform);
            }

            BlackboardBehaviour target;

            if (_rayHit.collider.gameObject.transform.root.TryGetComponent(out target))
            {
                target.RecieveDamage(damage, _ray.direction);
            }
            

            
        }
        
    }
}

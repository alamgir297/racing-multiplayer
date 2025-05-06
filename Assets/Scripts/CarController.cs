using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.Cinemachine;

public class CarController : NetworkBehaviour {
    private float _speed;

    public Action<float> OnSpeedChange;
    public float Speed
    {
        get { return _speed; }
        private set { _speed = value; }
    }

    private float _delayTime=3.5f;
    private bool _isBraking;
    private float _sideStiffness = 0.2f;

    [SerializeField] private float _maxSteerAngle = 40f;
    [SerializeField] private float _brakeForce = 3000f;
    [SerializeField] private  float _motorForce;
    [SerializeField] private float _handBrakeForce;
    [SerializeField] private float _engineBrakeForce = 2000;


    [SerializeField] private WheelCollider _frontRightWheel;
    [SerializeField] private WheelCollider _frontLeftWheel;
    [SerializeField] private WheelCollider _rearRightWheel;
    [SerializeField] private WheelCollider _rearLeftWheel;
    [SerializeField] private Transform _frontRightWheelMesh;
    [SerializeField] private Transform _frontLeftWheelMesh;
    [SerializeField] private Transform _rearRightWheelMesh;
    [SerializeField] private Transform _rearLeftWheelMesh;

    [SerializeField] private CinemachineCamera _thirdPersonFollowCam;

    private InputSystem_Actions _carController;
    private Rigidbody _rb;
    private InputAction _move;
    private InputAction _brake;

    private AudioController _carAudio;

    private void Awake() {
        _carController = new InputSystem_Actions();
        _rb = GetComponent<Rigidbody>();
        _carAudio = GetComponent<AudioController>();
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        if (!IsOwner) return;
        _thirdPersonFollowCam= GameObject.FindAnyObjectByType<CinemachineCamera>();
        if(_thirdPersonFollowCam!= null) {
            _thirdPersonFollowCam.Follow = transform;
        }

    }
    private void Start() {
        _carAudio.PlayeEnagineStartSound();
        _carAudio.PlayIdleWithDelay(_delayTime);
        //_thirdPersonFollowCam.Follow = transform;


    }
    private void OnEnable() {
        _move = _carController.Player.Move;
        _brake = _carController.Player.Brake;

        _move.Enable();
        _brake.Enable();
    }
    private void OnDisable() {
        _move.Disable();
        _brake.Disable();
    }
    void FixedUpdate() {
        if (!IsOwner) return;
        Move();
        CalculateSpeed();
    }

    private void Fly() {
        Vector2 input = _move.ReadValue<Vector2>();
        Vector3 direction = new Vector3(input.x, 0, input.y);
        transform.position = transform.position + direction;
    }

    private void Move() {
        Vector2 input = _move.ReadValue<Vector2>();
        float forward = input.y;
        float right = input.x;

        float steerAngle = right * _maxSteerAngle;

        float brakeInput = _brake.ReadValue<float>();

        float appliedBrakeForce = 0f;
        float appliedTorque = _motorForce * forward;

        HandleAudioEffects(input);

        if (brakeInput == 1) {
            if (MathF.Abs(right) > 0.001f) {
                HandBrake(_handBrakeForce);
            }
            else {
                appliedBrakeForce = _brakeForce;
                Brake(appliedBrakeForce);
            }
            ApplyRearTorque(0f);
        }
        else if(input== Vector2.zero) {
            EngineBrake(_engineBrakeForce);
        }
        else {
            ResetRearFriction();
            Brake(0f);
        }

        ApplyRearTorque(appliedTorque);
        ApplySteer(steerAngle);

        SyncWheelVisual();
    }

    private void ApplyRearTorque(float force) {
        _rearLeftWheel.motorTorque = force;
        _rearRightWheel.motorTorque = force;
    }

    private void ModifyRearFriction(float sidewaysStiffness) {
        //left
        WheelFrictionCurve rearLeftFriction = _rearLeftWheel.sidewaysFriction;
        rearLeftFriction.stiffness = sidewaysStiffness;
        _rearLeftWheel.sidewaysFriction = rearLeftFriction;

        //right
        WheelFrictionCurve rearRightFriction = _rearRightWheel.sidewaysFriction;
        rearRightFriction.stiffness = sidewaysStiffness;
        _rearRightWheel.sidewaysFriction = rearRightFriction;
    }
    void ResetRearFriction() {
        ModifyRearFriction(1);
    }

    private void ApplySteer(float steerAngle) {
        _frontLeftWheel.steerAngle = steerAngle;
        _frontRightWheel.steerAngle = steerAngle;
    }
    private void SyncWheelVisual() {
        UpdateWheelVisual(_frontLeftWheel, _frontLeftWheelMesh);
        UpdateWheelVisual(_frontRightWheel, _frontRightWheelMesh);
        UpdateWheelVisual(_rearLeftWheel, _rearLeftWheelMesh);
        UpdateWheelVisual(_rearRightWheel, _rearRightWheelMesh);
    }
    private void UpdateWheelVisual(WheelCollider wheelCol, Transform wheelMesh) {
        wheelCol.GetWorldPose(out Vector3 pos, out Quaternion rot);
        wheelMesh.position = pos;
        wheelMesh.rotation = rot;
    }

    private void Brake(float brakeForce) {
        ApplyBrake(_frontLeftWheel, brakeForce);
        ApplyBrake(_frontRightWheel, brakeForce);
        ApplyBrake(_rearLeftWheel, brakeForce);
        ApplyBrake(_rearRightWheel, brakeForce);
    }

    private void HandBrake(float brakeForce) {
        ModifyRearFriction(_sideStiffness);
        _carAudio.PlayDriftSound();
        ApplyBrake(_rearLeftWheel, brakeForce);
        ApplyBrake(_rearRightWheel, brakeForce);
    }
    private void EngineBrake(float brakeForce) {
        ApplyBrake(_rearLeftWheel, brakeForce);
        ApplyBrake(_rearRightWheel, brakeForce);
    }

    private void ApplyBrake(WheelCollider wheelCol ,float brakeForce) {
        wheelCol.brakeTorque = brakeForce;
    }

    private void CalculateSpeed() {
        Speed = _rb.linearVelocity.magnitude * 3.6f;
        OnSpeedChange?.Invoke(Speed);

    }
    private void HandleAudioEffects(Vector2 input) {
        if (input == Vector2.zero) {
            _carAudio.PlayEngineIdleSound();
        }
        else {
            _carAudio.PlayeEngineRunningSound();
        }
    }


}

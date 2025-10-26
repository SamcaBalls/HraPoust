using SteamLobbyTutorial;
using UnityEngine;
using Mirror;

public class Headbob : NetworkBehaviour
{
    [Header("Configuration")]
    [SerializeField] private bool _enable = true;
    [SerializeField, Range(0, 0.1f)] public float _Amplitude = 0.015f;
    [SerializeField, Range(0, 30)] public float _BaseFrequency = 10.0f;
    [SerializeField] private Transform _camera = null;
    [SerializeField] private Transform _cameraHolder = null;
    [SerializeField] private PlayerMovementHandler movScript;

    private Vector3 _startLocalPos;
    private float _bobTimer = 0f;

    private void Awake()
    {
        if (_camera == null || _cameraHolder == null)
        {
            Debug.LogError("Camera or CameraHolder not assigned!");
            enabled = false;
            return;
        }
        _startLocalPos = _camera.localPosition;
    }

    void LateUpdate()
    {
        if (!isLocalPlayer) return;
        if (!_enable) return;

        // spočítáme bob podle pohybu
        Vector3 bobOffset = CalculateHeadbob();

        // aplikujeme headbob relativně k původní lokální pozici kamery
        _camera.localPosition = _startLocalPos + bobOffset;

        // Kamera se může dál volně otáčet podle držáku
        // (_cameraHolder.transform.rotation se stále uplatní)
    }

    private Vector3 CalculateHeadbob()
    {
        Vector3 horizontalVelocity = new Vector3(movScript.controller.velocity.x, 0, movScript.controller.velocity.z);
        float speed = horizontalVelocity.magnitude;

        if (speed < 0.1f)
        {
            _bobTimer = 0f;
            return Vector3.zero;
        }

        // rychlost hráče ovlivní frekvenci víc než lineárně
        float speedFactor = Mathf.Pow(speed / movScript.moveSpeed, 2.5f); // 1.5 můžeš upravit podle pocitu
        float adjustedFrequency = _BaseFrequency * speedFactor;

        // FPS nezávislý timer
        _bobTimer += Time.deltaTime * adjustedFrequency;

        Vector3 offset = Vector3.zero;
        offset.y = Mathf.Sin(_bobTimer) * _Amplitude;           // vertikální bob
        offset.x = Mathf.Cos(_bobTimer / 2f) * _Amplitude * 2;  // horizontální bob

        return offset;
    }

}

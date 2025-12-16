using System.Collections;
using UnityEngine;

public class NF_CameraFollowOBJECT : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _playerTransform;

    [Header("Flip Rotation Stats")]
    [SerializeField] private float _flipYRotationTime = 0.5f;

    private Coroutine _turnCoroutine;
    private CA_PlayerController _player;
    private bool _isFacingRight;

    private void Awake()
    {
        _player = _playerTransform.gameObject.GetComponent<CA_PlayerController>();
        _isFacingRight = _player.isFacingRight;
    }

    private void Update()
    {
        transform.position = _playerTransform.position;
    }

    public void CallTurn()
    {
        if (_turnCoroutine != null)
            StopCoroutine(_turnCoroutine);

        _turnCoroutine = StartCoroutine(FlipYLerp());
    }

    private IEnumerator FlipYLerp()
    {
        float startRotation = transform.localEulerAngles.y;
        float endRotationAmount = DetermineEndRotation();
        float yRotation = 0f;
        float elapsedTime = 0f;

        while (elapsedTime < _flipYRotationTime)
        {
            elapsedTime += Time.deltaTime;
            yRotation = Mathf.Lerp(startRotation, endRotationAmount, elapsedTime / _flipYRotationTime);
            transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
            yield return null;
        }

        transform.rotation = Quaternion.Euler(0f, endRotationAmount, 0f);
    }

    private float DetermineEndRotation()
    {
        _isFacingRight = !_isFacingRight;

        if (_isFacingRight)
        {
            return 0f;
        }
        else
        {
            return 180f;
        }
    }
}

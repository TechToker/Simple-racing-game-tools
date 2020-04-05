using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPoint : MonoBehaviour
{
    [SerializeField] private float _radius;

    public float Radius => _radius;

    [Header("New")]
    [SerializeField] private float _width = 2.5f;
    public float Width => _width;

    [Header("Gizmos")]
    [SerializeField] private Color _gateColor = Color.gray;
    [SerializeField] private float _gizmosScaleY = 2f;
    [SerializeField] private float _gizmosScaleZ = 0.1f;

    public Vector3 Center => new Vector3(transform.position.x, transform.position.y + _gizmosScaleY / 2, transform.position.z);
    public Vector3 LeftBorder => transform.TransformPoint(-_width / 2, _gizmosScaleY / 2, 0);
    public Vector3 RightBorder => transform.TransformPoint(_width / 2, _gizmosScaleY / 2, 0);

    public Vector3 RacingPoint => transform.TransformPoint(_racingLinePoint * Width, _gizmosScaleY / 2, 0);
    public float LocalRacingPoint => _racingLinePoint;
    private float _racingLinePoint = 0;

    private RaceCircuit _circuit;
    public void SetCircuit(RaceCircuit circuit)
    {
        _circuit = circuit;
        _racingLinePoint = 0;
    }

    //Think about refactoring
    public Vector3 ConvertLocalPointToWorld(Vector3 localPoint)
    {
        return transform.TransformPoint(localPoint.x, _gizmosScaleY / 2 + localPoint.y, localPoint.z);
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = _gateColor;
        
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(new Vector3(0, _gizmosScaleY / 2, 0), new Vector3(_width, _gizmosScaleY, _gizmosScaleZ));
    }

    public void SetRacingPoint(float racingPoint)
    {
        _racingLinePoint = Mathf.Clamp(racingPoint, -0.5f, 0.5f);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BehaviourAI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BrakeControlDebugger : MonoBehaviour
{
	[SerializeField] private DriverAI _targetDriver;
	[SerializeField] private RawImage _targetImage;
	
	[Header("Text data")]
	[SerializeField] private TextMeshProUGUI _enterSpeedText;
	[SerializeField] private TextMeshProUGUI _exitSpeedText;
	[SerializeField] private TextMeshProUGUI _brakingDistance;
	
	[Header("Colors")]
	public Color32 bgColor = Color.white;
	public Color32 guidesColor = Color.blue;

	[SerializeField] private int _borderOffset;
	
	private Texture2D m_Texture;
	Color32[] m_PixelsBg;
	Color32[] m_Pixels;

	private Vector2Int _imageSize;
	private Vector2Int _drawableImageSize;
	
	private List<Vector2> _brakingData;
	private bool _isGraphUpdating = true;
	
	void Start()
	{
		// Set up our texture.
		_imageSize = new Vector2Int((int) _targetImage.rectTransform.rect.size.x, (int) _targetImage.rectTransform.rect.size.y);
		m_Texture = new Texture2D(_imageSize.x, _imageSize.y);

		_targetImage.texture = m_Texture;

		m_Pixels = new Color32[_imageSize.x * _imageSize.y];
		m_PixelsBg = new Color32[_imageSize.x * _imageSize.y];

	    for (int i = 0; i < m_Pixels.Length; ++i)
		    m_PixelsBg[i] = bgColor;

	    _brakingData = new List<Vector2>();
	}

	void LateUpdate()
	{
		BrakingData brakingData = ((RacingState) _targetDriver.StateAI).BrakingData;
		if (brakingData == null || (!brakingData.IsRecording && !_isGraphUpdating))
		{
			_brakingData.Clear();
			return;
		}

		// Clear.
		Array.Copy(m_PixelsBg, m_Pixels, m_Pixels.Length);

        DrawPattern();
        DrawCarData(brakingData);
        
		m_Texture.SetPixels32(m_Pixels);
		m_Texture.Apply();
	}

	private void DrawPattern()
	{
		_drawableImageSize = new Vector2Int(_imageSize.x - _borderOffset * 2, _imageSize.y - _borderOffset * 2);
		
		int lowerPx = _borderOffset;
		int upperPx = _imageSize.y - _borderOffset;
		
		int leftPx = _borderOffset;
		int rightPx = _imageSize.x - _borderOffset;

		int markerLength = 5;
		
		//X-Axis
		DrawLine(new Vector2(0, lowerPx), new Vector2(_imageSize.x, lowerPx), guidesColor);
		//Marker on X-axis
		DrawLine(new Vector2(rightPx, lowerPx - markerLength), new Vector2(rightPx, lowerPx + markerLength), guidesColor);
		
		//Y-Axis
		DrawLine(new Vector2(leftPx, 0), new Vector2(leftPx, _imageSize.y), guidesColor);
		//Marker on Y-axis
		DrawLine(new Vector2(leftPx - markerLength, upperPx), new Vector2(leftPx + markerLength, upperPx), guidesColor);
		
		//Default line
		DrawLine(new Vector2(leftPx, lowerPx), new Vector2(rightPx, upperPx), Color.green);
	}

	private void DrawCarData(BrakingData brakingData)
	{
		_enterSpeedText.SetText(brakingData.CornerEnterSpeed.ToString("N1"));
		_exitSpeedText.SetText(brakingData.CornerExitSpeed.ToString("N1"));
		_brakingDistance.SetText(brakingData.StartBrakingDistance.ToString("N1"));

		Vector2 pointInPercent = new Vector2(brakingData.CurrentDistanceProgress / brakingData.StartBrakingDistance,
			(brakingData.CurrentBrakingSpeed - brakingData.CornerExitSpeed) / (brakingData.CornerEnterSpeed - brakingData.CornerExitSpeed));
		
		Vector2 pointInDrawableZone = new Vector2(pointInPercent.x * _drawableImageSize.x + _borderOffset,
			pointInPercent.y * _drawableImageSize.y + _borderOffset);
		
		//Set limit to 80 elements
		if(_brakingData.Count() > 80)
			_brakingData.RemoveAt(0);
		
		_brakingData.Add(pointInDrawableZone);

		for (int i = 1; i < _brakingData.Count; i++)
			DrawLine(_brakingData[i - 1], _brakingData[i], Color.red);

		//Allow last graph after stop recording
		_isGraphUpdating = brakingData.IsRecording;
	}

	void DrawLine(Vector2 from, Vector2 to, Color32 color)
	{
		int i;
		int j;

		if (Mathf.Abs(to.x - from.x) > Mathf.Abs(to.y - from.y))
		{
			// Horizontal line.
			i = 0;
			j = 1;
		}
		else
        {
			// Vertical line.
			i = 1;
			j = 0;
		}

		int x = (int)from[i];
		int delta = (int)Mathf.Sign(to[i] - from[i]);
		while (x != (int)to[i])
		{
			int y = (int)Mathf.Round(from[j] + (x - from[i]) * (to[j] - from[j]) / (to[i] - from[i]));

		    int index;
		    if (i == 0)
		        index = y * _imageSize.x + x;
		    else
		        index = x * _imageSize.x + y;

            index = Mathf.Clamp(index, 0, m_Pixels.Length - 1);
            m_Pixels[index] = color;

			x += delta;
		}
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Grapher.
/// Draw x/y points on a graph using a particle systems.
/// Changeable properties are width and height in world units,
/// Points can be added, the graph will automatically adjust to show the last MAX_POINTS.
/// float is used internally, so points added with double values might lose precision.
/// 
/// TODO: add an option to automatically and periodically re-add the previously added point
/// with some delta (on the x axis usually) in case there is no new point incoming. This
/// is intended to keep the graph updating when the source goes dark and
/// could be shown with smaller size points for those auto points.
/// 
/// TODO: axes and labels
/// </summary>
public class Grapher : MonoBehaviour {
	public const int MAX_POINTS = 100;
	private float _width = 800;
	private float _height = 100;
	public float width = 800;
	public float height = 100;

	private List<Vector2> points = new List<Vector2>(MAX_POINTS);
	private ParticleSystem.Particle[] particles = new ParticleSystem.Particle[MAX_POINTS];
	private int newestIndex = -1;
	// scaling bounds: start at 0,0 so it will always be in the graph initially
	private Vector2 minXY = new Vector2(0f, 0f);
	private Vector2 maxXY = new Vector2(0f, 0f);
	private bool needRescaling = false;

	private ParticleSystem _particleSystem;

	void Awake () {
		this._particleSystem = GetComponent<ParticleSystem>();
		// Adjust the axes for x and y based on width and height
		rescale ();
	}

	void Start () {
		// Add some random points for testing
		AddPoint(0, 0);
		AddPoint(0.5, 0.5);
		AddPoint(1, 1);
/*		AddPoint(15, 41);
		AddPoint(51, 111);
		AddPoint(111, 31);
		AddPoint(31, 11);
*/	}

	void Update () {
		if (width != _width) {
			if (width <= float.Epsilon * 1000) {
				width = _width;
			} else {
				_width = width;
				needRescaling = true;
			}
		}
		if (height != _height) {
			if (height <= float.Epsilon * 1000) {
				height = _height;
			} else {
				_height = height;
				needRescaling = true;
			}
		}
		if (needRescaling) {
			rescale();
		}
		_particleSystem.SetParticles(particles, particles.Length);
	}

	public void AddPoint(double x, double y) {
		AddPoint((float) x, (float) y);
	}

	public void AddPoint(float x, float y) {
		checkBounds(x, y);
		if (points.Count == MAX_POINTS) {
			// check if point to be removed was at one of the minmax limits
			// and recalcBounds if so.
			points.RemoveAt(0);
		}
		points.Add(new Vector2(x, y));
		addParticle(x, y);
	}

	private void addParticle(float x, float y) {
		newestIndex++;
		if (newestIndex == MAX_POINTS) {
			newestIndex = 0;
		}
		// x and y need to be scaled to 0 and width or height, respectively
		float scaledX = scaleValue(x - minXY.x, maxXY.x - minXY.x, _width);
		float scaledY = scaleValue(y - minXY.y, maxXY.y - minXY.y, _height);
		Debug.Log (string.Format("new particle: x {0} y {1} scaledX {2} scaledY {3}", x, y, scaledX, scaledY)); 
		particles[newestIndex].position = new Vector3(scaledX, scaledY, 0f);
		particles[newestIndex].color = new Color(0.5f + scaledX/_width, 0.5f + scaledY/_height, 0.9f);
		particles[newestIndex].size = 5 * _width / MAX_POINTS;
	}

	private float scaleValue(float val, float max, float destMax) {
		if (max < float.Epsilon * 1000f) {
			max = float.Epsilon * 1000f;
		}
		return destMax * (val / max);
	}

	private void checkBounds(float x, float y) {
		if (x < minXY.x) {
			minXY.x = x;
			needRescaling = true;
		}
		if (y < minXY.y) {
			minXY.y = y;
			needRescaling = true;
		}
		if (x > maxXY.x) {
			maxXY.x = x;
			needRescaling = true;
		}
		if (y > maxXY.y) {
			maxXY.y = y;
			needRescaling = true;
		}
		if (needRescaling) {
			rescale();
		}
	}

	private void rescale(bool recalcBounds=false) {
		needRescaling = false;
		// TODO: axis!
		foreach (Transform child in transform) {
			if (child.name == "XAxis") {
				child.localPosition = new Vector3(_width / 2, child.localPosition.y, child.localPosition.z);
				child.localScale = new Vector3(child.localScale.x, _width / 2, child.localScale.z);
			} else if (child.name == "YAxis") {
				child.localPosition  = new Vector3(child.localPosition.x, _height / 2, child.localPosition.z);
				child.localScale = new Vector3(child.localScale.x, _height / 2, child.localScale.z);
			}
		}
		if (points.Count < 1) {
			return;
		}
		// TODO: recalcBounds?
		newestIndex = -1;
		foreach (Vector2 pt in points) {
			addParticle(pt.x, pt.y);
		}
	}

	public bool NearlyEqual(float a, float b, float epsilon) {
		if (a == b) { // shortcut, handles infinities
			return true;
		} else {
			float diff = Math.Abs(a - b);
			if (a == 0 || b == 0 || diff < float.Epsilon) {
				// relative error is less meaningful here
				return diff < (epsilon * float.Epsilon);
			} else {
				// use relative error
				return diff / (Math.Abs(a) + Math.Abs(b)) < epsilon;
			}
		}
	}
}

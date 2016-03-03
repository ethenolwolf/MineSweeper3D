using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour {

	public int x;
	public int y;
	public GameObject flag;

	MeshRenderer renderer;
	Color originalColor;

	void Start () {
		renderer = GetComponent<MeshRenderer>();
		originalColor = renderer.material.color;
	}

	public void SetFlag(bool check) {
		flag.SetActive(check);
	}

	public void SetSelected() {
		renderer.material.color = new Color(1f, 1f, 0.5f, 1.0f);
	}

	public void SetUnSelected() {
		renderer.material.color = originalColor;
	}
	
}

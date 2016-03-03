using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RevealedBlock : MonoBehaviour {

	public Image image;

	public void SetImage(Sprite sprite, Color color) {
		image.sprite = sprite;
		image.color = color;
	}

}

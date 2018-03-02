using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class NoiseTextureSet : MonoBehaviour {

    [SerializeField]
    List<NoiseTexture> _textures;

    [SerializeField]
    int YOffset = 0;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
    }

    private void OnGUI()
    {
        int currentOffset = 0;
        for (int i = 0; i < _textures.Count; ++i)
        {
            Vector2 pos = new Vector2(currentOffset, YOffset);
            this._textures[i].Draw(pos);
            currentOffset += this._textures[i].Resolution;
        }
    }
}

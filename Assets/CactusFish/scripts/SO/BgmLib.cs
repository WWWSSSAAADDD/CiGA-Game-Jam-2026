using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BgmLib", menuName = "BgmLibData")]
public class BgmLib : ScriptableObject
{
    public List<AudioClip> audioClips = new List<AudioClip>();
}

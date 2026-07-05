using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MsfxLib", menuName = "MsfxLibData")]
public class MsfxLib : ScriptableObject
{
    public List<AudioClip> audioClips = new List<AudioClip>();
}

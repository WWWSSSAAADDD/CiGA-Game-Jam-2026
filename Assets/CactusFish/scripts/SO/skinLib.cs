using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skinLib", menuName = "data/skinLib")]
public class skinLib : ScriptableObject
{
    public List<Sprite> sprites = new List<Sprite>();

}

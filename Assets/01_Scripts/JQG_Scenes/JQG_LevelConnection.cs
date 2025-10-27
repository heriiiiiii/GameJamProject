using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Levels/Connection")]
public class JQG_LevelConnection : ScriptableObject
{
   public static JQG_LevelConnection ActiveConnection {  get; set; }
}

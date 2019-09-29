using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILaggable
{
    Laggable lag { get; set;}
    void FixedLagUpdate();
    void LagUpdate();
}

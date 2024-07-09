using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventHandler
{
    public static Action<BlackboardBehaviour> RegisterEntityEvent = delegate {};
    public static Action<Vector3> PlayerWeaponFiredEvent = delegate{};
}

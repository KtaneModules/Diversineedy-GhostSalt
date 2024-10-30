using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Components
{
    public abstract class ModComponent : MonoBehaviour
    {
        public abstract IEnumerator Handle();
        public abstract IEnumerator Disable();
    }
}
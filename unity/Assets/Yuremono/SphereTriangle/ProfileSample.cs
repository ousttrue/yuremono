using System;
using UnityEngine.Profiling;

namespace SphereTriangle
{
    public struct ProfileSample : IDisposable
    {
        public ProfileSample(string name)
        {
            Profiler.BeginSample(name);
        }

        public void Dispose()
        {
            Profiler.EndSample();
        }
    }
}
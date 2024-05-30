namespace RotateParticle
{
    public struct FrameTime
    {
        public readonly float DeltaTime;
        public readonly float SqDt;
        public FrameTime(float deltaTime)
        {
            DeltaTime = deltaTime;
            SqDt = deltaTime * deltaTime;
        }
    }
}
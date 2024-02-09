namespace DynamicCamera
{
    internal static class Utils
    {
        private const float SMOOTHING_FACTOR = 0.03f;
        public static float CameraSmooth(float current, float target)
        {
            if (Math.Abs(current - target) > 0.1f)
            {
                // Determine the direction of movement
                float direction = Math.Sign(target - current);

                // Calculate the weighted average
                current += direction * Math.Abs(target - current) * SMOOTHING_FACTOR;

                return current;
            }
            return current;
        }      
    }
}

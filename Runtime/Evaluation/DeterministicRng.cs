namespace Galaretka.ShapeGraph.Evaluation
{
    public static class DeterministicRng
    {
        public static float Hash01(int seed, int keyA, int keyB = 0)
        {
            unchecked
            {
                uint hash = (uint)seed * 397u;
                hash ^= (uint)keyA * 0x85EBCA6Bu;
                hash ^= (uint)keyB * 17u;

                hash ^= hash >> 13;
                hash *= 0x5BD1E995u;
                hash ^= hash >> 15;

                return (hash & 0x00FFFFFF) / (float)0x01000000;
            }
        }
    }
}

using System;

namespace ZoneUA.AI
{
    public static class NpcTargetScoring
    {
        public static float Score(float squaredDistance, bool hostile, bool alive, bool visible)
        {
            if (!hostile || !alive || !visible)
            {
                return float.NegativeInfinity;
            }

            return -Math.Max(0f, squaredDistance);
        }

        public static bool IsBetter(float candidateScore, float currentBestScore) =>
            candidateScore > currentBestScore;
    }
}

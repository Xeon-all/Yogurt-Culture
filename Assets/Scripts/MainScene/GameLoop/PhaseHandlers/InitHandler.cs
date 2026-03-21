using UnityEngine;

namespace YogurtCulture.GameLoop
{
    public class InitHandler : PhaseHandlerBase
    {
        public override GamePhase Phase => GamePhase.Init;
        public override float Duration => 0f;
    }
}

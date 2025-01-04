using Quantum.Ava;

namespace Quantum
{
    using System.Collections.Generic;
    using Photon.Deterministic;

    public static partial class DeterministicCommandSetup
    {
        static partial void AddCommandFactoriesUser(ICollection<IDeterministicCommandFactory> factories, RuntimeConfig gameConfig, SimulationConfig simulationConfig)
        {
            factories.Add(new DeterministicCommandPool<PauseSimulation>());
            factories.Add(new DeterministicCommandPool<AdvanceOneFrame>());
        }
    }
}
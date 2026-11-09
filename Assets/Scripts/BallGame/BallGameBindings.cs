using Zenject;

namespace BallGame
{
    internal static class BallGameBindings
    {
        public static void Install(
            DiContainer container,
            BallGameConfig config,
            ParticleEffectsConfig particleEffectsConfig,
            BallGameScene scene)
        {
            container.BindInstance(config).AsSingle();
            container.BindInstance(particleEffectsConfig).AsSingle();
            container.Bind<GameWindowManager>().AsSingle();
            container.Bind<ParticleEffectsManager>().AsSingle();
            container.BindInstance(scene).AsSingle();
            container.Bind<BallGameController>().AsSingle();
        }
    }
}

using System.Threading.Tasks;

namespace AbstractPixel.LevelFramework
{
    public interface ILevelTransitionAdapter<TSceneAssetType>
    {
        public Task TransitionToLevel(TSceneAssetType sceneAssetType);
    }
}
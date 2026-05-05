using System.Threading.Tasks;

namespace AbstractPixel.LevelFramework
{
    public interface ILevelTransitionAdapter<TSceneAssetType>
    {
        public void TransitionTo(TSceneAssetType sceneAssetType);
    }
}
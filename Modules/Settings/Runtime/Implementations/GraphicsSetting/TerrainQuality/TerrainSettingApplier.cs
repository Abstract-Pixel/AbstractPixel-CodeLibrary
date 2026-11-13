using UnityEngine;

namespace AbstractPixel.Settings
{
    [RequireComponent(typeof(Terrain))]
    public class TerrainSettingApplier : AbstractSettingApplier<int>
    {
        private Terrain targetTerrain;

        private void Awake()
        {
            TryGetComponent(out targetTerrain);
        }

        protected override void OnLiveSettingBinded(BaseSetting<int> _bindedSetting)
        {
            TerrainQualitySetting terrainSetting = _bindedSetting as TerrainQualitySetting;

            if (terrainSetting != null && targetTerrain != null)
            {
                terrainSetting.RegisterTerrain(targetTerrain);
            }
        }

        protected override void OnLiveSettingUnbinded(BaseSetting<int> _unbindedSetting)
        {
            TerrainQualitySetting terrainSetting = _unbindedSetting as TerrainQualitySetting;

            if (terrainSetting != null && targetTerrain != null)
            {
                terrainSetting.UnregisterTerrain(targetTerrain);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class TerrainQualitySetting : BaseOptionsSetting<int, TerrainOptionData>
    {
        private List<Terrain> registeredTerrains = new List<Terrain>();

        public void RegisterTerrain(Terrain _terrainToRegister)
        {
            if (_terrainToRegister == null || registeredTerrains.Contains(_terrainToRegister) == true)
            {
                return;
            }

            registeredTerrains.Add(_terrainToRegister);

            if (OptionValues != null && CurrentValue >= 0 && CurrentValue < OptionValues.Length)
            {
                ApplyTerrainDataToInstance(_terrainToRegister, OptionValues[CurrentValue]);
            }
        }

        public void UnregisterTerrain(Terrain _terrainToRemove)
        {
            if (_terrainToRemove != null && registeredTerrains.Contains(_terrainToRemove) == true)
            {
                registeredTerrains.Remove(_terrainToRemove);
            }
        }

        protected override void OnInitialize()
        {
            if (OptionValues == null || OptionValues.Length == 0)
            {
                GenerateDefaultOptions();
            }
        }

        private void GenerateDefaultOptions()
        {
            OptionDisplayNames = new string[] { "Low", "Medium", "High" };
            DefaultValue = 1; // Default to "Medium" (Index 1)

            OptionValues = new TerrainOptionData[]
            {
                // Index 0: LOW
                new TerrainOptionData
                {
                    PixelError = 10.0f,
                    BaseMapDistance = 500.0f,
                    DetailDensityScale = 0.25f,
                    DetailDistance = 40.0f,
                    TreeDistance = 1000.0f,
                    BillboardStart = 30.0f,
                    FadeLength = 5.0f,
                    MaxMeshTrees = 20
                },
                // Index 1: MEDIUM
                new TerrainOptionData
                {
                    PixelError = 5.0f,
                    BaseMapDistance = 1000.0f,
                    DetailDensityScale = 0.5f,
                    DetailDistance = 80.0f,
                    TreeDistance = 2500.0f,
                    BillboardStart = 50.0f,
                    FadeLength = 10.0f,
                    MaxMeshTrees = 50
                },
                // Index 2: HIGH
                new TerrainOptionData
                {
                    PixelError = 1.0f,
                    BaseMapDistance = 2000.0f,
                    DetailDensityScale = 1.0f,
                    DetailDistance = 150.0f,
                    TreeDistance = 5000.0f,
                    BillboardStart = 100.0f,
                    FadeLength = 20.0f,
                    MaxMeshTrees = 100
                }
            };
        }

        protected override void OnApplySettingLogic()
        {
            if (OptionValues == null || CurrentValue < 0 || CurrentValue >= OptionValues.Length)
            {
                return;
            }

            TerrainOptionData selectedOption = OptionValues[CurrentValue];

            // If no terrains are explicitly registered via Appliers, fallback to all active scene terrains
            if (registeredTerrains.Count == 0)
            {
                Terrain[] activeTerrainsArray = Terrain.activeTerrains;

                for (int i = 0; i < activeTerrainsArray.Length; i++)
                {
                    Terrain activeTerrain = activeTerrainsArray[i];

                    if (activeTerrain != null)
                    {
                        ApplyTerrainDataToInstance(activeTerrain, selectedOption);
                    }
                }
            }
            else
            {
                for (int i = registeredTerrains.Count - 1; i >= 0; i--)
                {
                    Terrain registeredTerrain = registeredTerrains[i];

                    if (registeredTerrain == null)
                    {
                        registeredTerrains.RemoveAt(i);
                        continue;
                    }

                    ApplyTerrainDataToInstance(registeredTerrain, selectedOption);
                }
            }
        }

        private void ApplyTerrainDataToInstance(Terrain _terrain, TerrainOptionData _data)
        {
            if (_terrain == null)
            {
                return;
            }

            _terrain.heightmapPixelError = _data.PixelError;
            _terrain.basemapDistance = _data.BaseMapDistance;
            _terrain.detailObjectDensity = _data.DetailDensityScale;
            _terrain.detailObjectDistance = _data.DetailDistance;
            _terrain.treeDistance = _data.TreeDistance;
            _terrain.treeBillboardDistance = _data.BillboardStart;
            _terrain.treeCrossFadeLength = _data.FadeLength;
            _terrain.treeMaximumFullLODCount = _data.MaxMeshTrees;
        }

#if UNITY_EDITOR
        protected override void OnValidateInEditor()
        {
            if (OptionValues == null || OptionValues.Length == 0)
            {
                GenerateDefaultOptions();
            }
        }
#endif
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AbstractPixel.Core.UI
{
    public class SpatialUIRow
    {
        public float AveragePositionY { get; private set; }
        public List<Selectable> Elements { get; } = new List<Selectable>();

        public SpatialUIRow(float _initialPositionY, Selectable _firstElement)
        {
            AveragePositionY = _initialPositionY;
            Elements.Add(_firstElement);
        }

        public void AddElement(Selectable _element, float _positionY)
        {
            Elements.Add(_element);
            AveragePositionY = (AveragePositionY * (Elements.Count - 1) + _positionY) / Elements.Count;
        }

        public void SortLeftToRight(Camera _targetCamera)
        {
            Elements.Sort((_first, _second) =>
            {
                float firstX = RectTransformUtility.WorldToScreenPoint(_targetCamera, _first.transform.position).x;
                float secondX = RectTransformUtility.WorldToScreenPoint(_targetCamera, _second.transform.position).x;
                return firstX.CompareTo(secondX);
            });
        }

        public Selectable FindClosestElementByX(float _targetScreenPositionX, Camera _targetCamera)
        {
            if (Elements.Count == 0)
            {
                return null;
            }

            Selectable closestElement = Elements[0];
            float closestX = RectTransformUtility.WorldToScreenPoint(_targetCamera, closestElement.transform.position).x;
            float smallestDistance = Math.Abs(closestX - _targetScreenPositionX);

            for (int index = 1; index < Elements.Count; ++index)
            {
                Selectable candidate = Elements[index];
                float candidateX = RectTransformUtility.WorldToScreenPoint(_targetCamera, candidate.transform.position).x;
                float distance = Math.Abs(candidateX - _targetScreenPositionX);

                if (distance < smallestDistance)
                {
                    smallestDistance = distance;
                    closestElement = candidate;
                }
            }

            return closestElement;
        }
    }
}
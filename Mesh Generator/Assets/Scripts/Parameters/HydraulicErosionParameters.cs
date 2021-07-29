using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Parameters
{
    public class HydraulicErosionParameters
    {
        public static float flatThreshold = 0.00001f;
        // variables -------------------------------------------------------------------------------------------------------
        private int _dropCount = 80;
        private float _erosionRate = 0.04f;
        private float _depositeRate = 0.03f;
        private float _speed = 0.15f;
        private float _friction = 0.7f;
        private float _radius = 0.8f;
        private int _maxIterations = 80;
        private float _iterationScale = 0.04f;

        // accessors + Mutators --------------------------------------------------------------------------------------------
        public int DropCount
        {
            get => _dropCount;
            set => _dropCount = value;
        }

        public float ErosionRate
        {
            get => _erosionRate;
            set => _erosionRate = value;
        }

        public float DepositeRate
        {
            get => _depositeRate;
            set => _depositeRate = value;
        }

        public float Speed
        {
            get => _speed;
            set => _speed = value;
        }

        public float Friction
        {
            get => _friction;
            set => _friction = value;
        }

        public int MaxIterations
        {
            get => _maxIterations;
            set => _maxIterations = value;
        }
        
        public float Radius
        {
            get => _radius;
            set => _radius = value;
        }

        public float IterationScale
        {
            get => _iterationScale;
            set => _iterationScale = value;
        }

        public static bool normIsFlat(Vector3 norm)
        {
            return norm.y >= 1 - flatThreshold && norm.y <= 1 + flatThreshold;
        }
    }
}

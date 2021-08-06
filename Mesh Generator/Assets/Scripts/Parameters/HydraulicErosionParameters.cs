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
        private float _dt = 1.2f;
        private float _startingVolume = 1;
        private float _minVolume = 0.1f;
        private float _density = 1f;
        private float _depositeRate = 0.05f;
        private float _evaporationRate = 0.001f;
        private float _friction = 0.05f;
        private float _radius = 0.8f;
        private bool _flipXY = false;

        // accessors + Mutators --------------------------------------------------------------------------------------------
        public int DropCount
        {
            get => _dropCount;
            set => _dropCount = value;
        }

        public float DT
        {
            get => _dt;
            set => _dt = value;
        }

        public float StartingVolume
        {
            get => _startingVolume;
            set => _startingVolume = value;
        }

        public float MINVolume
        {
            get => _minVolume;
            set => _minVolume = value;
        }

        public float Density
        {
            get => _density;
            set => _density = value;
        }

        public float EvaporationRate
        {
            get => _evaporationRate;
            set => _evaporationRate = value;
        }

        public float DepositeRate
        {
            get => _depositeRate;
            set => _depositeRate = value;
        }

        public float Friction
        {
            get => _friction;
            set => _friction = value;
        }

        public float Radius
        {
            get => _radius;
            set => _radius = value;
        }

        public bool FlipXY
        {
            get => _flipXY;
            set => _flipXY = value;
        }
    }
}

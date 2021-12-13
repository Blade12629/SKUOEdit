using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Game.Map.Statics
{
    public static class StaticUtility
    {
        /// <summary>
        /// The modifier when changing static img size to unity scale and vice versa
        /// </summary>
        const float _SIZE_TO_SCALE_MOD = 31f;

        ///// <summary>
        ///// The base height of a static img
        ///// </summary>
        //const float _BASE_HEIGHT = 30;

        ///// <summary>
        ///// The height per level of a static img
        ///// </summary>
        //const float _HEIGHT_PER_HEIGHT_LEVEL = 4;

        ///// <summary>
        ///// Gets the height excluding the base height
        ///// </summary>
        //public static float GetHeightOffset(float height)
        //{
        //    return height - _BASE_HEIGHT;
        //}

        ///// <summary>
        ///// Adds the base height to an height offset
        ///// </summary>
        //public static float AddBaseHeight(float heightOffset)
        //{
        //    return heightOffset + _BASE_HEIGHT;
        //}

        ///// <summary>
        ///// Gets the level of the height offset
        ///// </summary>
        //public static float GetHeightLevel(float heightOffset)
        //{
        //    return (float)Math.Ceiling(heightOffset / _HEIGHT_PER_HEIGHT_LEVEL);
        //}

        ///// <summary>
        ///// Gets the height of a specified level
        ///// </summary>
        //public static float HeightLevelToHeight(float heightLevel)
        //{
        //    return heightLevel * _HEIGHT_PER_HEIGHT_LEVEL;
        //}
        
        ///// <summary>
        ///// The base height of a static img
        ///// </summary>
        //const float _BASE_HEIGHT = 30;

        ///// <summary>
        ///// The height per level of a static img
        ///// </summary>
        //const float _HEIGHT_PER_HEIGHT_LEVEL = 4;

        ///// <summary>
        ///// Gets the height excluding the base height
        ///// </summary>
        //public static float GetHeightOffset(float height)
        //{
        //    return height - _BASE_HEIGHT;
        //}

        ///// <summary>
        ///// Adds the base height to an height offset
        ///// </summary>
        //public static float AddBaseHeight(float heightOffset)
        //{
        //    return heightOffset + _BASE_HEIGHT;
        //}

        ///// <summary>
        ///// Gets the level of the height offset
        ///// </summary>
        //public static float GetHeightLevel(float heightOffset)
        //{
        //    return (float)Math.Ceiling(heightOffset / _HEIGHT_PER_HEIGHT_LEVEL);
        //}

        ///// <summary>
        ///// Gets the height of a specified level
        ///// </summary>
        //public static float HeightLevelToHeight(float heightLevel)
        //{
        //    return heightLevel * _HEIGHT_PER_HEIGHT_LEVEL;
        //}
        
        /// <summary>
        /// Converts the size of a static to it's equivalent unity scale
        /// </summary>
        public static float SizeToScale(float size)
        {
            return size / _SIZE_TO_SCALE_MOD;
        }

        /// <summary>
        /// Converts the unity scale to it's equivalent static img size
        /// </summary>
        public static float ScaleToSize(float scale)
        {
            return scale * _SIZE_TO_SCALE_MOD;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source
{
    public static class Utils
    {
        /// <summary>
        /// Checks if <paramref name="c"/> is null, if it is null
        /// <para>this will attempt to set c by getting the <see cref="Component"/> (<typeparamref name="T"/>) from <paramref name="obj"/></para>
        /// </summary>
        /// <typeparam name="T">Component type</typeparam>
        /// <param name="c">Reference to variable that holds the component</param>
        /// <param name="obj">Gameobject to get component from incase <paramref name="c"/> is null</param>
        public static T GetCNull<T>(ref T c, GameObject obj) where T : Component
        {
            if (c != null)
                return c;

            c = obj.GetComponent<T>();
            return c;
        }

        /// <summary>
        /// Checks if the reference is null, when true executes <paramref name="whenNull"/> and sets <paramref name="c"/> to it's value
        /// </summary>
        public static T GetNull<T>(ref T c, Func<T> whenNull)
        {
            if (c != null)
                return c;

            c = whenNull();
            return c;
        }


    }
}

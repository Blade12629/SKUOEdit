﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Source
{
    public unsafe static class Pointers
    {
        public static T* Read<T>(ref byte* ptr, int size) where T : unmanaged
        {
            T* result = (T*)ptr;
            ptr += size;

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* Read<T>(ref byte* ptr) where T : unmanaged
        {
            return Read<T>(ref ptr, sizeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadValue<T>(ref byte* ptr, int size) where T : unmanaged
        {
            return *Read<T>(ref ptr, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadValue<T>(ref byte* ptr) where T : unmanaged
        {
            return *Read<T>(ref ptr, sizeof(T));
        }
    }
}

using System;
using System.Linq;
using UnityEngine;

namespace AtelierResleriana.Plugin.Localization
{
    public class TextAssetFactory
    {
        public TextAsset CreateFromBytes(byte[] bytes)
        {
            TextAsset textAsset = new TextAsset(TextAsset.CreateOptions.CreateNativeObject, new string(Enumerable.Repeat(' ', bytes.Length).ToArray()));

            unsafe
            {
                IntPtr dataPointer = textAsset.GetDataPtr();
                fixed (byte* sourcePtr = bytes)
                {
                    Buffer.MemoryCopy(sourcePtr, dataPointer.ToPointer(), bytes.Length, bytes.Length);
                }
            }

            return textAsset;
        }  
    }
}

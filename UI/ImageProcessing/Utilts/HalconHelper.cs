using HalconDotNet;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace UI.ImageProcessing.Utilts
{
    public static class HalconHelper
    {
        private static HDevelopExport HalconScripts = new HDevelopExport();

        public static HObject ConcateAll(params HObject[] objects)
        {
            var objectOut = objects[0];
            for (int i = 1; i < objects.Length; i++)
            {
                HOperatorSet.ConcatObj(objectOut, objects[i], out objectOut);
            }

            return objectOut;
        }
    }
}
using HalconDotNet;

namespace UI.ImageProcessing
{
    public class CoordinateSolver
    {
        private HTuple _changeOfBase, _changeOfBaseInv, _rotationMat, _rotationMatInv, _mapToWorld, _mapToImage;
        private HDevelopExport _halconScripts = new HDevelopExport();

        public CoordinateSolver(HTuple changeOfBase, HTuple changeOfBaseInv, HTuple rotationMat, HTuple rotationMatInv, HTuple mapToWorld, HTuple mapToImage)
        {
            _changeOfBase = changeOfBase;
            _changeOfBaseInv = changeOfBaseInv;
            _rotationMat = rotationMat;
            _rotationMatInv = rotationMatInv;
            _mapToWorld = mapToWorld;
            _mapToImage = mapToImage;
        }

        /// <summary>
        /// Calculate absolute angle in Halcon's representation from an angle of normal representation'
        /// </summary>
        /// <param name="angleNormal"></param>
        /// <returns></returns>
        public double DegreeRelativeToAbsolute(double angleNormal)
        {
            HTuple degreeAbs;
            _halconScripts.DegreeRelativeToAbs(_rotationMat, angleNormal, out degreeAbs);

            return degreeAbs.ToDArr()[0];
        }

        /// <summary>
        /// Calculate an absolute point from a relative point
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Point PointRelativeToAbsolute(double x, double y)
        {
            HTuple xAbs;
            HTuple yAbs;
            _halconScripts.PointRelativeToAbs(x, y, _changeOfBase, out xAbs, out yAbs);
            return new Point(xAbs.D, yAbs.D);
        }

        public FindLineLocation FindLineLocationRelativeToAbsolute(FindLineLocation relativeLocation)
        {
            var point = PointRelativeToAbsolute(relativeLocation.X, relativeLocation.Y);
            var angle = DegreeRelativeToAbsolute(relativeLocation.Angle);

            return new FindLineLocation() {Angle = angle, Len1 = relativeLocation.Len1, Len2 = relativeLocation.Len2, X = point.X, Y = point.Y, ImageIndex = relativeLocation.ImageIndex, Name = relativeLocation.Name};
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Retroherz.Components
{
    // ColliderComponent?
    public class ColliderComponentEX
    {
        private BoundingRectangle _boundingRectangle;

        public Point2 Center
        {
            get => _boundingRectangle.Center;
            set => _boundingRectangle.Center = value;
        }

        public Vector2 HalfExtents
        {
            get => _boundingRectangle.HalfExtents;
            set => _boundingRectangle.HalfExtents = value;
        }       

        public ColliderComponentEX(RectangleF rectangle)
        {
            _boundingRectangle = rectangle;
        }

        ~ColliderComponentEX() {}

        // POPULATE WITH FUNCTIONS

        /// <summary>
        ///     Performs an implicit conversion from a <see cref="BoundingRectangle" /> to a <see cref="RectangleF" />.
        /// </summary>
        /// <param name="boundingRectangle">The bounding rectangle.</param>
        /// <returns>
        ///     The resulting <see cref="Rectangle" />.
        /// </returns>
        public static implicit operator RectangleF(ColliderComponentEX boundingRectangle)
        {
            var minimum = boundingRectangle.Center - boundingRectangle.HalfExtents;
            return new RectangleF(minimum.X, minimum.Y, boundingRectangle.HalfExtents.X * 2,
                boundingRectangle.HalfExtents.Y * 2);
        }

        /// <summary>
        ///     Returns a <see cref="string" /> that represents this <see cref="BoundingRectangle" />.
        /// </summary>
        /// <returns>
        ///     A <see cref="string" /> that represents this <see cref="BoundingRectangle" />.
        /// </returns>
        public override string ToString() => _boundingRectangle.ToString();        
    }
}
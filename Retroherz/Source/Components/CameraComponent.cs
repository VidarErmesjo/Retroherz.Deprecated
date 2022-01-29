using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MonoGame.Extended;

namespace Retroherz.Components
{
    public class CameraComponent
    {
        private readonly OrthographicCamera _camera;

        public OrthographicCamera Get { get => _camera; }

        public CameraComponent(OrthographicCamera camera)
        {
            _camera = camera;
        }        
    }
}
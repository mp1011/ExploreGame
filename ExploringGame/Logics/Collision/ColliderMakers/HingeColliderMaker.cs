using ExploringGame.Services;
using Jitter2.Dynamics;
using System;
using System.Collections.Generic;

namespace ExploringGame.Logics.Collision.ColliderMakers
{
    internal class HingeColliderMaker : IColliderMaker
    {
        private ICollidable _shape, _parent;

        public HingeColliderMaker(ICollidable shape, ICollidable parent)
        {
            _shape = shape;
            _parent = parent;
        }

        public IEnumerable<RigidBody> CreateColliders(Physics physics)
        {
            yield return physics.CreateHingeCollider(_shape, _parent);
        }
    }
}

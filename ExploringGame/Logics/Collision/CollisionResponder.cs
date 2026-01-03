using Jitter2.Dynamics;
using System.Collections.Generic;

namespace ExploringGame.Logics.Collision;

public interface ICollisionResponse
{
    void OnCollision(RigidBody thisBody, RigidBody otherBody);
}

public class CollisionResponder
{
    private List<ICollisionResponse> _responses = new List<ICollisionResponse>();
    private EntityMover _mover;
    private RigidBody _body;
    private HashSet<Arbiter> _activeArbiters = new();

    public CollisionResponder(EntityMover mover)
    {
        _mover = mover;
    }

    public void Subscribe(RigidBody body)
    {
        body.BeginCollide += BeginCollide;
        body.EndCollide += EndCollide;
        _body = body;
    }

    public void Update()
    {
        foreach (var arbiter in _activeArbiters)
        {
            var other = arbiter.Body1 == _body ? arbiter.Body2 : arbiter.Body1;

            foreach (var response in _responses)
            {
                response.OnCollision(_body, other);
            }
        }
    }

    private void EndCollide(Arbiter obj)
    {
        _activeArbiters.Remove(obj);
    }

    public void AddResponse(ICollisionResponse response)
    {
        _responses.Add(response);
    }

    private void BeginCollide(Arbiter obj)
    {
        _activeArbiters.Add(obj);
    }
}

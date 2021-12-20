using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raycasting
{

    public abstract class Cast
    {

        protected PositionRelative start;
        protected PositionRelative dest;

        public Cast()
        {
            start = new PositionRelative(Vector3.zero);
            dest = new PositionRelative(Vector3.zero);
        }

        public Cast(Vector3 newSt, Vector3 angle, float pathLen)
        {
            start = new PositionRelative(newSt);
            dest = new PositionRelative(newSt + angle.normalized * pathLen);
        }

        public Cast(Vector3 newSt, Vector3 enNew)
        {
            start = new PositionRelative(newSt);
            dest = new PositionRelative(enNew);
        }

        public void setOrigin(Vector3 newSt)
        {
            start.setWorldPosition(newSt);
        }

        public Cast(Vector3 newSt, Vector3 angle, float pathLen, Transform oriSt, Transform destEn)
        {
            start = new PositionRelative(newSt, oriSt);
            dest = new PositionRelative(newSt + angle.normalized * pathLen, destEn);
        }
        public abstract bool castRay(out RaycastHit hitInfo, LayerMask layerMask, QueryTriggerInteraction q = QueryTriggerInteraction.Ignore);
        public Vector3 getOrigin()
        {
            return start.getWorldPosition();
        }

        public abstract void draw(Color col, float duration = 0);
        public Vector3 getDirection()
        {
            return (dest.getWorldPosition() - start.getWorldPosition());
        }
        public float getDistance()
        {
            return getDirection().magnitude;
        }

        public abstract RaycastHit[] castRayAll(LayerMask layerMask, QueryTriggerInteraction q = QueryTriggerInteraction.Ignore);
        public void setParentOrigin(Transform enPos)
        {
            start.setParent(enPos);
        }
        public void setParentEnd(Transform enPos)
        {
            dest.setParent(enPos);
        }
        public void setEnd(Vector3 enNew)
        {
            dest.setWorldPosition(enNew);
        }

        public Cast(Vector3 newSt, Vector3 enNew, Transform oriSt, Transform destEn)
        {
            start = new PositionRelative(newSt, oriSt);
            dest = new PositionRelative(enNew, destEn);
        }
        public Vector3 getEnd()
        {
            return dest.getWorldPosition();
        }
        public void setDistance(float pathLen)
        {
            Vector3 p = getOrigin(); dest.setWorldPosition(p + (getEnd() - p).normalized * pathLen);
        }

    }
    public class PositionRelative
    {
        private Vector3 pos;
        private Transform left;


        public void setParent(Transform enPos)
        {
            Vector3 old_pos = getWorldPosition();
            left = enPos;
            setWorldPosition(old_pos);
        }
        public PositionRelative(Vector3 newP, Transform enPos)
        {
            left = enPos;
            setWorldPosition(newP);
        }
        public PositionRelative(Vector3 newP)
        {
            pos = newP;
            left = null;
        }

        public Vector3 getLocalPosition()
        {
            return pos;
        }
        public void setWorldPosition(Vector3 newP)
        {
            if (left == null)
            {
                pos = newP;
            }
            else
            {
                pos = left.InverseTransformPoint(newP);
            }
        }

        public Vector3 getWorldPosition()
        {
            if (left == null)
            {
                return pos;
            }
            else
            {
                return left.TransformPoint(pos);
            }
        }
        public void setLocalPosition(Vector3 newP)
        {
            pos = newP;
        }


        public Transform getParent()
        {
            return left;
        }

    }
    public enum CastMode
    {
        RayCast,
        SphereCast
    }



    public class RayCast : Cast
    {

        public RayCast() : base() { }

        public RayCast(Vector3 newSt, Vector3 angle, float pathLen) : base(newSt, angle, pathLen) { }
        public override bool castRay(out RaycastHit hitInfo, LayerMask layerMask, QueryTriggerInteraction q = QueryTriggerInteraction.Ignore)
        {
            Vector3 v = getDirection();
            return Physics.Raycast(getOrigin(), v.normalized, out hitInfo, v.magnitude, layerMask, q);
        }

        public RayCast(Vector3 newSt, Vector3 enNew) : base(newSt, enNew) { }

        public override RaycastHit[] castRayAll(LayerMask layerMask, QueryTriggerInteraction q = QueryTriggerInteraction.Ignore)
        {
            Vector3 v = getDirection();
            return Physics.RaycastAll(getOrigin(), v, v.magnitude, layerMask, q);
        }

        public RayCast(Vector3 newSt, Vector3 angle, float pathLen, Transform oriSt, Transform destEn) : base(newSt, angle, pathLen, oriSt, destEn) { }

        public RayCast(Vector3 newSt, Vector3 enNew, Transform oriSt, Transform destEn) : base(newSt, enNew, oriSt, destEn) { }

        public override void draw(Color col, float duration = 0)
        {
            Debug.DrawLine(getOrigin(), getEnd(), col, duration);
        }
    }


    public class SphereCast : Cast
    {
        protected float distFOri;

        public SphereCast() : base()
        {
            distFOri = 1.0f;
        }



        public SphereCast(Vector3 newSt, Vector3 angle, float pathLen, float deltaNew) : base(newSt, angle, pathLen)
        {
            distFOri = deltaNew;
        }
        public override bool castRay(out RaycastHit hitInfo, LayerMask layerMask, QueryTriggerInteraction q = QueryTriggerInteraction.Ignore)
        {
            Vector3 v = getDirection();
            return Physics.SphereCast(getOrigin(), getRadius(), v.normalized, out hitInfo, v.magnitude, layerMask, q);
        }


        public SphereCast(Vector3 newSt, Vector3 angle, float pathLen, float deltaNew, Transform oriSt, Transform destEn) : base(newSt, angle, pathLen, oriSt, destEn)
        {
            setRadius(deltaNew);
        }

        public SphereCast(Vector3 newSt, Vector3 enNew, float deltaNew, Transform oriSt, Transform destEn) : base(newSt, enNew, oriSt, destEn)
        {
            setRadius(deltaNew);
        }

        public float getRadius()
        {

            if (start.getParent() == null)
            {
                return distFOri;
            }
            else
            {
                return start.getParent().lossyScale.z * distFOri;
            }
        }
        public SphereCast(float deltaNew) : base()
        {
            distFOri = deltaNew;
        }

        public void setRadius(float deltaNew) { distFOri = (start.getParent() == null) ? deltaNew : deltaNew / start.getParent().lossyScale.z; }
        public override RaycastHit[] castRayAll(LayerMask layerMask, QueryTriggerInteraction q = QueryTriggerInteraction.Ignore)
        {
            Vector3 v = getDirection();
            return Physics.SphereCastAll(getOrigin(), getRadius(), v.normalized, v.magnitude, layerMask, q);
        }


        public override void draw(Color col, float duration = 0)
        {
            DebugShapes.DrawSphereRay(getOrigin(), getEnd(), getRadius(), 5, col, duration);
        }
        public SphereCast(Vector3 newSt, Vector3 enNew, float deltaNew) : base(newSt, enNew)
        {
            distFOri = deltaNew;
        }
    }
}
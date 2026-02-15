using UnityEngine;
using System.Collections.Generic;

namespace EvanGameKits.Mechanic
{
    /// <summary>
    /// Provides realistic physics to a skeletal rope. 
    /// Place this on the Armature Pivot or the Anchor object.
    /// </summary>
    public class RopePhysics : MonoBehaviour
    {
        [Header("Physics Settings")]
        [SerializeField] private float _segmentMass = 0.5f;
        [SerializeField] private float _linearDamping = 1.0f;
        [SerializeField] private float _angularDamping = 2.0f;
        
        [Header("Joint Limits")]
        [SerializeField] private float _bendLimit = 45f;
        [SerializeField] private float _springStrength = 100f;
        [SerializeField] private float _damper = 10f;

        [Header("Anchor & Start")]
        [Tooltip("The object the rope hangs from. If empty, uses this object (the Pivot).")]
        [SerializeField] private Transform _manualAnchor;
        [Tooltip("The first bone of the rope. If empty, uses the first child.")]
        [SerializeField] private Transform _startBone;
        [Tooltip("An optional object to hang at the end (e.g., a Lantern).")]
        [SerializeField] private Rigidbody _weightObject;
        [Tooltip("Where the rope attaches to the lantern (e.g., its handle). If empty, uses the lantern's pivot.")]
        [SerializeField] private Transform _weightAnchor;

        [Header("Setup")]
        [SerializeField] private bool _autoSetupOnStart = true;
        [SerializeField] private List<Transform> _bones = new List<Transform>();

        private void Start()
        {
            if (_autoSetupOnStart) SetupRope();
        }

        [ContextMenu("Setup Rope Physics")]
        public void SetupRope()
        {
            _bones.Clear();
            
            // 1. Identify Start Bone
            Transform rootBone = _startBone != null ? _startBone : (transform.childCount > 0 ? transform.GetChild(0) : null);
            if (rootBone == null) { Debug.LogError("RopePhysics: No start bone found!"); return; }

            // 2. Identify Anchor
            Transform anchorTrans = _manualAnchor != null ? _manualAnchor : transform;
            if (anchorTrans == rootBone) anchorTrans = rootBone.parent;
            if (anchorTrans == null) { Debug.LogError("RopePhysics: No valid anchor found!"); return; }

            // 3. Collect the bone chain
            CollectBones(rootBone);

            // 4. Setup Anchor Rigidbody
            Rigidbody anchorRb = anchorTrans.GetComponent<Rigidbody>();
            if (anchorRb == null) anchorRb = anchorTrans.gameObject.AddComponent<Rigidbody>();
            anchorRb.isKinematic = true; 

            Rigidbody lastRb = anchorRb;

            for (int i = 0; i < _bones.Count; i++)
            {
                Transform bone = _bones[i];
                if (bone == anchorTrans) continue;

                Rigidbody rb = bone.GetComponent<Rigidbody>();
                if (rb == null) rb = bone.gameObject.AddComponent<Rigidbody>();
                
                rb.mass = _segmentMass;
                rb.linearDamping = _linearDamping;
                rb.angularDamping = _angularDamping;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.isKinematic = false;

                ConfigurableJoint joint = bone.GetComponent<ConfigurableJoint>();
                if (joint == null) joint = bone.gameObject.AddComponent<ConfigurableJoint>();

                ConfigureJoint(joint, lastRb);
                
                // Ignore collision with anchor
                Collider col = bone.GetComponent<Collider>();
                Collider anchorCol = anchorTrans.GetComponent<Collider>();
                if (col != null && anchorCol != null) Physics.IgnoreCollision(col, anchorCol);

                lastRb = rb;
            }

            // 5. Setup Weight Object (Lantern) at the end
            if (_weightObject != null)
            {
                // Unparent to ensure clean physics simulation
                _weightObject.transform.SetParent(null);
                
                _weightObject.centerOfMass = new Vector3(0, -0.2f, 0); 
                _weightObject.interpolation = RigidbodyInterpolation.Interpolate;

                ConfigurableJoint weightJoint = _weightObject.GetComponent<ConfigurableJoint>();
                if (weightJoint == null) weightJoint = _weightObject.gameObject.AddComponent<ConfigurableJoint>();

                // Configure attachment point
                weightJoint.connectedBody = lastRb;
                weightJoint.autoConfigureConnectedAnchor = false;
                
                // Anchor is the local offset on the lantern
                if (_weightAnchor != null)
                    weightJoint.anchor = _weightObject.transform.InverseTransformPoint(_weightAnchor.position);
                else
                    weightJoint.anchor = Vector3.zero;

                // Connected anchor is the end of the last bone (zero)
                weightJoint.connectedAnchor = Vector3.zero;

                // Lock linear, free angular
                weightJoint.xMotion = ConfigurableJointMotion.Locked;
                weightJoint.yMotion = ConfigurableJointMotion.Locked;
                weightJoint.zMotion = ConfigurableJointMotion.Locked;
                weightJoint.angularXMotion = ConfigurableJointMotion.Free;
                weightJoint.angularYMotion = ConfigurableJointMotion.Free;
                weightJoint.angularZMotion = ConfigurableJointMotion.Free;

                // Upright stability
                weightJoint.rotationDriveMode = RotationDriveMode.Slerp;
                JointDrive drive = new JointDrive { positionSpring = 50f, positionDamper = 5f, maximumForce = float.MaxValue };
                weightJoint.slerpDrive = drive;

                // Ignore collisions with the rope
                Collider weightCol = _weightObject.GetComponent<Collider>();
                if (weightCol != null && lastRb.GetComponent<Collider>() != null)
                    Physics.IgnoreCollision(weightCol, lastRb.GetComponent<Collider>());
            }
        }

        private void CollectBones(Transform current)
        {
            _bones.Add(current);
            if (current.childCount > 0) CollectBones(current.GetChild(0));
        }

        private void ConfigureJoint(ConfigurableJoint joint, Rigidbody connectedBody)
        {
            joint.connectedBody = connectedBody;
            
            // Use current positions to calculate connection points
            joint.autoConfigureConnectedAnchor = true;

            // Lock linear movement
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;

            // Limit angular movement for realistic bending
            joint.angularXMotion = ConfigurableJointMotion.Limited;
            joint.angularYMotion = ConfigurableJointMotion.Limited;
            joint.angularZMotion = ConfigurableJointMotion.Limited;

            SoftJointLimit limit = new SoftJointLimit { limit = _bendLimit };
            joint.highAngularXLimit = limit;
            limit.limit = -_bendLimit;
            joint.lowAngularXLimit = limit;
            limit.limit = _bendLimit;
            joint.angularYLimit = limit;
            joint.angularZLimit = limit;

            SoftJointLimitSpring spring = new SoftJointLimitSpring { spring = _springStrength, damper = _damper };
            joint.angularXLimitSpring = spring;
            joint.angularYZLimitSpring = spring;
            
            joint.projectionMode = JointProjectionMode.PositionAndRotation;
        }
    }
}



#if (UNITY_EDITOR && UNITY_2022_3_OR_NEWER)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ECE
{
  // So in the current (newer) versions of ECS, authoring is done within subscenes using normal colliders.
  // So no special handling is needed.
  // This change occurs at 1.0 version of ECS which is 2022.3.0+
  public class EasyColliderDOTS
  {
    public void OnInspectorGUI(EasyColliderEditor Editor) { }
  }
}


#elif (UNITY_EDITOR)
namespace ECE
{
  using UnityEditor;
  using System.Collections;
  using System.Collections.Generic;
  using UnityEngine;
  using Unity.Entities;
  using Unity.Physics;
  using Unity.Physics.Authoring;
  using Unity.Physics.Extensions;

  using Collider = UnityEngine.Collider;
  using MeshCollider = UnityEngine.MeshCollider;
  using BoxCollider = UnityEngine.BoxCollider;
  using CapsuleCollider = UnityEngine.CapsuleCollider;
  using SphereCollider = UnityEngine.SphereCollider;
  using System.Linq;
  using ECUI = EasyColliderUIHelpers;
  [System.Serializable]
  public class EasyColliderDOTS
  {
    //UI Seperated to more easily add features in the non-dots part of the asset while still not having to directly modify the DOTS support.
    #region UI
    public bool ShowDOTSPropertyOverrides;
    public string[] PhysicsCategoryNameStrings;
    public string[] PhysicsCustomMaterialTagNameStrings;
    public void OnInspectorGUI(EasyColliderEditor ECEditor)
    {

      if (ECEditor == null) return;
      ECUI.HorizontalLineLight();
      ECUI.LabelBold("DOTS Conversion");

      ShowDOTSPropertyOverrides = EditorGUILayout.Foldout(ShowDOTSPropertyOverrides, "Property Overrides");
      if (ShowDOTSPropertyOverrides)
      {
        // DotsConverter.BoxBevel = EditorGUILayout.FloatField("Box Bevel", DotsConverter.BoxBevel);
        // Unity.Physics.Authoring.PhysicsCategoryNames names = ScriptableObject.FindObjectOfType<Unity.Physics.Authoring.PhysicsCategoryNames>();
        BoxBevel = EditorGUILayout.FloatField(new GUIContent("Box Bevel Radius", "Should be the box bevel radius that is applied, but currently this value is not kept by the PhysicsShapeAuthoring Component."), BoxBevel);
        MaterialTemplate = (PhysicsMaterialTemplate)EditorGUILayout.ObjectField("Physic Material Template", MaterialTemplate, typeof(PhysicsMaterialTemplate), false);
        PhysicsCategoryNames = (PhysicsCategoryNames)EditorGUILayout.ObjectField("Physic Category Names", PhysicsCategoryNames, typeof(PhysicsCategoryNames), false);
        if (PhysicsCategoryNames != null)
        {
          if (PhysicsCategoryNameStrings == null || PhysicsCategoryNameStrings.Length != PhysicsCategoryNames.CategoryNames.Count)
          {
            PhysicsCategoryNameStrings = PhysicsCategoryNames.CategoryNames.ToArray();
          }
          BelongsTo.Value = (uint)EditorGUILayout.MaskField("Belongs To", (int)BelongsTo.Value, PhysicsCategoryNameStrings);
          CollidesWith.Value = (uint)EditorGUILayout.MaskField("Collides With", (int)CollidesWith.Value, PhysicsCategoryNameStrings);
        }
        CustomPhysicsMaterialTagNames = (CustomPhysicsMaterialTagNames)EditorGUILayout.ObjectField("Custom Tags Object", CustomPhysicsMaterialTagNames, typeof(CustomPhysicsMaterialTagNames), false);
        if (CustomPhysicsMaterialTagNames != null)
        {
          if (PhysicsCustomMaterialTagNameStrings == null || PhysicsCustomMaterialTagNameStrings.Length != CustomPhysicsMaterialTagNames.TagNames.Count)
          {
            PhysicsCustomMaterialTagNameStrings = CustomPhysicsMaterialTagNames.TagNames.ToArray();
          }
          CustomTags.Value = (byte)EditorGUILayout.MaskField("Custom Tags", (int)CustomTags.Value, PhysicsCustomMaterialTagNameStrings);
        }
        CollisionResponse = (CollisionResponsePolicy)EditorGUILayout.EnumPopup("Collision Response", CollisionResponse);
      }
      if (GUILayout.Button(new GUIContent("Convert Colliders to Physic Shapes", "When clicked, converts the colliders on the Selected and Attach To fields gameobject and it's children to physic shapes that function with DOTS.")))
      {
        if (ECEditor.AttachToObject == null) { Debug.LogWarning("EasyColliderEditor: Colliders on the Attach To object are converted. Please set an object in that field."); }
        List<Collider> colliders = ECEditor.GetConvertibleColliders().ToList();
        ConvertColliders(colliders);
      }
      ECUI.HorizontalLineLight();
    }
    #endregion


    /// <summary>
    /// Should be the bevel parameter on the Box-type of physic authoring component,
    /// but it is completely ignored by PhysicsShapeAuthoring when passed in with the BoxGeometry object for some reason.
    /// Even though getting the properties immediately after setting it has the correct bevel value, it still just defaults to 0.05...
    /// </summary>
    public float BoxBevel = 0.0f;


    /// <summary>
    /// Physics category names object (so we can display the tag names)
    /// </summary>
    public PhysicsCategoryNames PhysicsCategoryNames;

    /// <summary>
    /// Custom physics material tag names object. (So we can display the tag names)
    /// </summary>
    public CustomPhysicsMaterialTagNames CustomPhysicsMaterialTagNames;

    /// <summary>
    /// Material template to set on all converted colliders.
    /// </summary>
    public PhysicsMaterialTemplate MaterialTemplate;

    /// <summary>
    /// Belongs to property to set on all converted colliders.
    /// </summary>
    public PhysicsCategoryTags BelongsTo;

    /// <summary>
    /// Collids with property to set on all converted colliders.
    /// </summary>
    public PhysicsCategoryTags CollidesWith;

    /// <summary>
    /// Custom tags to set on all converted colliders.
    /// </summary>
    public CustomPhysicsMaterialTags CustomTags;

    /// <summary>
    /// Response policy to set on all converted colliders.
    /// </summary>
    public CollisionResponsePolicy CollisionResponse;


    /// <summary>
    /// Converts a list of Colliders to PhysicShapeAuthoring components.
    /// </summary>
    /// <param name="colliders"></param>
    public void ConvertColliders(List<Collider> colliders)
    {
      foreach (Collider col in colliders)
      {
        ConvertCollider(col);
      }
    }

    /// <summary>
    /// Covers a single collider to a PhysicsShapeAuthoring component.
    /// </summary>
    /// <param name="collider"></param>
    public void ConvertCollider(Collider collider)
    {
      if (collider == null) return;
      PhysicsShapeAuthoring psa = null;
      if (collider is BoxCollider)
      {
        psa = ConvertBoxCollider(collider as BoxCollider);
      }
      else if (collider is MeshCollider)
      {
        psa = ConvertMeshCollider(collider as MeshCollider);
      }
      else if (collider is CapsuleCollider)
      {
        psa = ConvertCapsuleCollider(collider as CapsuleCollider);
      }
      else if (collider is SphereCollider)
      {
        psa = ConvertSphereCollider(collider as SphereCollider);
      }
      if (psa != null)
      {
        if (collider.gameObject.name.Contains("Rotated Capsule Collider") || collider.gameObject.name.Contains("Rotated Box Collider"))
        {
          Undo.DestroyObjectImmediate(collider.gameObject);
        }
        if (MaterialTemplate != null)
        {
          psa.MaterialTemplate = MaterialTemplate;
        }
        psa.BelongsTo = BelongsTo;
        psa.CollidesWith = CollidesWith;
        if (PhysicsCategoryNames == null)
        {
          psa.BelongsTo = PhysicsCategoryTags.Everything;
          psa.CollidesWith = PhysicsCategoryTags.Everything;
        }
        psa.CustomTags = CustomTags;
        psa.CollisionResponse = CollisionResponse;

        if (collider != null)
        {
          Undo.DestroyObjectImmediate(collider);
        }
      }
      else
      {
        Debug.LogWarning("Unable to convert collider " + collider + " on " + collider.gameObject.name, collider.gameObject);
      }
    }

    public GameObject GetNewChildObject(Collider collider, string name)
    {
      GameObject child = new GameObject(name);
      if (collider.transform.name.Contains("Rotated"))
      {
        child.transform.parent = collider.transform.parent;
        child.transform.rotation = collider.transform.rotation;
      }
      else
      {
        child.transform.parent = collider.transform;
        child.transform.localRotation = Quaternion.identity;
      }
      child.transform.position = collider.transform.position;
      // for rotated colliders.
      child.transform.localScale = Vector3.one;
      Undo.RegisterCreatedObjectUndo(child, "Created Collider Holder");
      return child;
    }


    /// <summary>
    /// Converts a box collider to a PhysicsShapeAuthoring.
    /// Note that the BevelRadius of this class is applied, but ignored by the PhysicsShapeAuthoring component.
    /// </summary>
    /// <param name="boxCollider"></param>
    /// <returns></returns>
    private PhysicsShapeAuthoring ConvertBoxCollider(BoxCollider boxCollider)
    {
      //TODO: why does SetBox completely ignore the set BevelRadius, and instead assigns it's own default of 0.05
      // shouldn't it respect the value set on the actually BoxGeometry instance that is passed?
      PhysicsShapeAuthoring box = Undo.AddComponent<PhysicsShapeAuthoring>(GetNewChildObject(boxCollider, "Box Collider"));
      BoxGeometry boxGeometry = new BoxGeometry();
      boxGeometry.Size = boxCollider.size;
      boxGeometry.Center = boxCollider.center;
      // why is this ignored?
      boxGeometry.BevelRadius = BoxBevel;
      box.SetBox(boxGeometry);
      // the value shown is fine, but is ignored after being created.
      // BoxGeometry b = box.GetBoxProperties();
      // Debug.Log(b.BevelRadius);
      return box;
    }


    /// <summary>
    /// Converts a capsule collider to a PhysicsShapeAuthoring
    /// </summary>
    /// <param name="capsuleCollider"></param>
    /// <returns></returns>
    private PhysicsShapeAuthoring ConvertCapsuleCollider(CapsuleCollider capsuleCollider)
    {
      PhysicsShapeAuthoring capsule = Undo.AddComponent<PhysicsShapeAuthoring>(GetNewChildObject(capsuleCollider, "Capsule Collider"));
      CapsuleGeometryAuthoring capsuleGeometry = new CapsuleGeometryAuthoring();
      capsuleGeometry.Radius = capsuleCollider.radius;
      capsuleGeometry.Height = capsuleCollider.height;
      capsuleGeometry.Center = capsuleCollider.center;
      Vector3 fwd = Vector3.zero;
      Vector3 up = Vector3.zero;
      Quaternion orientation = Quaternion.identity;
      bool isRotated = capsuleCollider.gameObject.name.Contains("Rotated Capsule Collider");
      if (!isRotated)
      {
        if (capsuleCollider.direction == 0)
        {
          up = capsuleCollider.transform.forward;
          fwd = capsuleCollider.transform.right;
        }
        else if (capsuleCollider.direction == 1)
        {
          up = capsuleCollider.transform.forward;
          fwd = capsuleCollider.transform.up;
        }
        else
        {
          up = capsuleCollider.transform.right;
          fwd = capsuleCollider.transform.forward;
        }
        capsuleGeometry.Orientation = Quaternion.LookRotation(fwd, up);
      }
      capsule.SetCapsule(capsuleGeometry);
      return capsule;

    }

    /// <summary>
    /// Convets a mesh collider to a PhysicsShapeAuthoring
    /// </summary>
    /// <param name="meshCollider"></param>
    /// <returns></returns>
    private PhysicsShapeAuthoring ConvertMeshCollider(MeshCollider meshCollider)
    {
      PhysicsShapeAuthoring mesh = Undo.AddComponent<PhysicsShapeAuthoring>(GetNewChildObject(meshCollider, "Mesh Collider"));
      mesh.SetMesh(meshCollider.sharedMesh);
      return mesh;
    }

    /// <summary>
    /// Converts a sphere collider to a PhysicsShapeAuthoring
    /// </summary>
    /// <param name="sphereCollider"></param>
    /// <returns></returns>
    private PhysicsShapeAuthoring ConvertSphereCollider(SphereCollider sphereCollider)
    {
      PhysicsShapeAuthoring sphere = Undo.AddComponent<PhysicsShapeAuthoring>(GetNewChildObject(sphereCollider, "Sphere Collider"));
      SphereGeometry sphereGeometry = new SphereGeometry();
      sphereGeometry.Radius = sphereCollider.radius;
      sphereGeometry.Center = sphereCollider.center;
      sphere.SetSphere(sphereGeometry, Quaternion.identity);
      return sphere;
    }
  }
}
#endif
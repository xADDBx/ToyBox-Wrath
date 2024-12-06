using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Converters;
using Newtonsoft.Json.Linq;
using Kingmaker.SharedTypes;
using Kingmaker.ElementsSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ToyBox.PatchTool; 
public class PatchOperation {
    public enum PatchOperationType {
        ModifyPrimitive,
        ModifyUnityReference,
        ModifyBlueprintReference,
        ModifyComplex,
        ModifyCollection
    }
    public enum CollectionPatchOperationType {
        AddAtIndex,
        RemoveAtIndex,
        ModifyAtIndex
    }
    public string FieldName;
    public Type NewValueType;
    public object NewValue;
    public int CollectionIndex;
    public PatchOperation NestedOperation;
    public Type PatchedObjectType;
    public PatchOperationType OperationType;
    public CollectionPatchOperationType CollectionOperationType;
    public PatchOperation() { }
    public PatchOperation(PatchOperationType operationType, string fieldName, Type newValueType, object newValue, Type patchedObjectType, PatchOperation nestedOperation = null) {
        OperationType = operationType;
        FieldName = fieldName;
        NewValue = newValue;
        PatchedObjectType = patchedObjectType;
        NestedOperation = nestedOperation;
        NewValueType = newValueType;
    }
    public PatchOperation(PatchOperationType operationType, string fieldName, Type newValueType, object newValue, Type patchedObjectType, CollectionPatchOperationType collectionOperationType, int collectionIndex, PatchOperation nestedOperation = null) {
        OperationType = operationType;
        FieldName = fieldName;
        NewValue = newValue;
        PatchedObjectType = patchedObjectType;
        CollectionOperationType = collectionOperationType;
        CollectionIndex = collectionIndex;
        NestedOperation = nestedOperation;
        NewValueType = newValueType;
    }
    public FieldInfo GetFieldInfo(Type type) {
        return AccessTools.Field(type, FieldName);
    }
    public object Apply(object instance) {
        var field = GetFieldInfo(PatchedObjectType);
        if (PatchToolUtils.IsListOrArray(field.FieldType) && (OperationType != PatchOperationType.ModifyCollection)) {
            // We're in a collection, so the patched field will point to a collection, meaning we will need to work on the instance itself.
            // By returning the changed instance, the ModifyCollection operation will set the returned value itself.
            field = null;
        }
        if (!(OperationType == PatchOperationType.ModifyCollection || OperationType == PatchOperationType.ModifyComplex) && (field != null && !field.FieldType.IsAssignableFrom(NewValueType))) throw new ArgumentException($"Type to patch {PatchedObjectType}, field {field.Name} with type {field.FieldType} is not assignable from instance type {instance.GetType()}\nField: {FieldName ?? null}, OperationType: {OperationType}, NestedOperationType: {NestedOperation?.OperationType.ToString() ?? "Null"} ");
        
        switch (OperationType) {
            case PatchOperationType.ModifyCollection: {
                    object collection;
                    if (field == null) {
                        collection = instance;
                    } else {
                        collection = field.GetValue(instance);
                    }
                    switch (CollectionOperationType) {
                        case CollectionPatchOperationType.AddAtIndex: {
                                var newInst = Activator.CreateInstance(NewValueType);
                                if (collection.GetType() is Type type && type.IsArray) {
                                    Array array = collection as Array;
                                    if (CollectionIndex == -1) CollectionIndex = array.Length;
                                    var elementType = type.GetElementType();
                                    Array newArray = Array.CreateInstance(elementType, array.Length + 1);
                                    Array.Copy(array, 0, newArray, 0, CollectionIndex);
                                    newArray.SetValue(newInst, CollectionIndex);
                                    Array.Copy(array, CollectionIndex, newArray, CollectionIndex + 1, array.Length - CollectionIndex);
                                    collection = newArray;
                                } else if (collection is IList list) {
                                    if (CollectionIndex == -1) CollectionIndex = list.Count;
                                    list.Insert(CollectionIndex, newInst);
                                    collection = list;
                                }
                                if (newInst is Element e && FieldName != nameof(SimpleBlueprint.m_AllElements)) {
                                    Patcher.CurrentlyPatching.AddToElementsList(e);
                                }
                            }
                            break;
                        case CollectionPatchOperationType.RemoveAtIndex: {
                                if (collection.GetType() is Type type && type.IsArray) {
                                    Array array = collection as Array;
                                    var elementType = type.GetElementType();
                                    var tmpList = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType)) as IList;
                                    foreach (var item in array)
                                        tmpList.Add(item);
                                    tmpList.RemoveAt(CollectionIndex);
                                    Array resizedArray = Array.CreateInstance(elementType, tmpList.Count);
                                    tmpList.CopyTo(resizedArray, 0);
                                    collection = resizedArray;
                                } else if (collection is IList list) {
                                    list.RemoveAt(CollectionIndex);
                                    collection = list;
                                }
                            } 
                            break;
                        case CollectionPatchOperationType.ModifyAtIndex: {
                                if (collection.GetType() is Type type && type.IsArray) {
                                    Array array = collection as Array;
                                    var orig = array.GetValue(CollectionIndex);
                                    var modified = NestedOperation.Apply(orig);
                                    array.SetValue(modified, CollectionIndex);
                                    collection = array;
                                } else if (collection is IList list) {
                                    var orig = list[CollectionIndex];
                                    var modified = NestedOperation.Apply(orig);
                                    list[CollectionIndex] = modified;
                                    collection = list;
                                }
                            }
                            break;
                        default: throw new NotImplementedException($"Unknown CollectionOperation: {CollectionOperationType}");
                    }
                    field.SetValue(instance, collection);
                } 
                break;
            case PatchOperationType.ModifyUnityReference: {
                    throw new NotImplementedException("Modifying Unity Objects is not supported.");
                }
#pragma warning disable CS0162 // Unreachable code detected
                break;
#pragma warning restore CS0162 // Unreachable code detected
            case PatchOperationType.ModifyComplex: {
                    object @object;
                    if (field == null) {
                        @object = instance;
                    } else {
                        @object = field.GetValue(instance);
                    }
                    var patched = NestedOperation.Apply(@object);
                    if (field != null) {
                        field.SetValue(instance, patched);
                    } else {
                        return patched;
                    }
                } break;
            case PatchOperationType.ModifyPrimitive: {
                    object patched;
                    if (typeof(Enum).IsAssignableFrom(NewValueType)) {
                        var tmp = Convert.ChangeType(NewValue, Enum.GetUnderlyingType(NewValueType));
                        patched = Enum.ToObject(NewValueType, tmp);
                    } else {
                        patched = Convert.ChangeType(NewValue, NewValueType);
                    }
                    if (field != null) {
                        field.SetValue(instance, patched);
                    } else {
                        return patched;
                    }
                } break;
            case PatchOperationType.ModifyBlueprintReference: {
                    var bpRef = Activator.CreateInstance(NewValueType) as BlueprintReferenceBase;
                    bpRef.guid = NewValue as string;
                    var patched = Convert.ChangeType(bpRef, NewValueType);
                    if (field != null) {
                        field.SetValue(instance, patched);
                    } else {
                        return patched;
                    }
                } break;
            default: throw new NotImplementedException($"Unknown PatchOperation: {OperationType}");
        }
        return instance;
    }
}

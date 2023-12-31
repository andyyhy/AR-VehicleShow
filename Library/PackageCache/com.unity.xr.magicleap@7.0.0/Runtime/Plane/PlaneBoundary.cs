using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine.XR.MagicLeap.PlaneJobs;

namespace UnityEngine.XR.MagicLeap
{
    /// <summary>
    /// Container for the boundary of a detected planar surface. This is specific
    /// to Magic Leap because the polygon describing the boundary may be concave,
    /// and may contain holes.
    /// </summary>
    public struct PlaneBoundary : IEquatable<PlaneBoundary>
    {
        /// <summary>
        /// Whether this <see cref="PlaneBoundary"/> is valid. You should check
        /// for validity before invoking <see cref="GetPolygon(Allocator, ref NativeArray{Vector2})"/>,
        /// <see cref="GetPolygon(Allocator)"/>, <see cref="GetHole(int, Allocator)"/>, or
        /// <see cref="GetHole(int, Allocator, ref NativeArray{Vector2})"/>.
        /// </summary>
        public bool valid
        {
            get
            {
                unsafe
                {
                    return m_Boundary.polygon != null;
                }
            }
        }

        /// <summary>
        /// Gets the polygon representing a plane's boundary, and, if successful, copies it to <paramref name="polygonOut"/>.
        /// <paramref name="polygonOut"/> is resized or created using <paramref name="allocator"/> if necessary.
        /// The 2D vertices are in plane-space.
        /// </summary>
        /// <param name="allocator">The Allocator to use if <paramref name="polygonOut"/> must be recreated.
        /// Must be <c>Allocator.TempJob</c> or <c>Allocator.Persistent</c>.</param>
        /// <param name="polygonOut">A NativeArray to fill with boundary points. If the array is not the correct size, it is disposed and recreated.</param>
        /// <exception cref="System.InvalidOperationException">Thrown if <see cref="valid"/> is <c>false</c>.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if <paramref name="allocator"/> is <c>Allocator.Temp</c> or <c>Allocator.None</c>.</exception>
        public unsafe void GetPolygon(Allocator allocator, ref NativeArray<Vector2> polygonOut)
        {
            if (!valid)
                throw new InvalidOperationException("This plane boundary is not valid.");

            if (allocator == Allocator.Temp)
                throw new InvalidOperationException("Allocator.Temp is not supported. Use Allocator.TempJob if you wish to use a temporary allocator.");

            if (allocator == Allocator.None)
                throw new InvalidOperationException("Allocator.None is not a valid allocator.");

            TransformMLPolygon(*m_Boundary.polygon, m_Pose, allocator, ref polygonOut);
        }

        /// <summary>
        /// The number of vertices in this boundary's polygon.
        /// </summary>
        public int polygonVertexCount { get; private set; }

        /// <summary>
        /// Gets the polygon representing this boundary. The 2D vertices are in plane-space.
        /// </summary>
        /// <param name="allocator">The allocator to use for the returned NativeArray. Must be <c>Allocator.TempJob</c> or <c>Allocator.Persistent</c>.</param>
        /// <returns>A new NativeArray containing a set of 2D points in plane-space representing a boundary for a plane.
        /// The caller is responsible for disposing the NativeArray.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if <see cref="valid"/> is <c>false</c>.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if <paramref name="allocator"/> is <c>Allocator.Temp</c> or <c>Allocator.None</c>.</exception>
        public NativeArray<Vector2> GetPolygon(Allocator allocator)
        {
            var polygon = new NativeArray<Vector2>();
            GetPolygon(allocator, ref polygon);
            return polygon;
        }

        /// <summary>
        /// The number of holes in this boundary.
        /// </summary>
        public int holeCount
        {
            get
            {
                return (int)m_Boundary.holes_count;
            }
        }

        /// <summary>
        /// Get the polygon representing a hole in this boundary. The 2D vertices are in plane-space.
        /// </summary>
        /// <param name="index">The index of the hole. Must be less than <see cref="holeCount"/>.</param>
        /// <param name="allocator">The allocator to use for the returned NativeArray.
        /// Must be <c>Allocator.TempJob</c> or <c>Allocator.Persistent</c>.</param>
        /// <returns>A new NativeArray allocated with <paramref name="allocator"/> containing a set of 2D vertices
        /// in plane-space describing the hole at <paramref name="index"/>.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if <see cref="valid"/> is false.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if <paramref name="allocator"/> is <c>Allocator.Temp</c> or <c>Allocator.None</c>.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is less than 0 or greater than or equal to <see cref="holeCount"/>.</exception>
        public NativeArray<Vector2> GetHole(int index, Allocator allocator)
        {
            var hole = new NativeArray<Vector2>();
            GetHole(index, allocator, ref hole);
            return hole;
        }

        /// <summary>
        /// Get the polygon representing a hole in this boundary. The 2D vertices are in plane-space.
        /// </summary>
        /// <param name="index">The index of the hole. Must be less than <see cref="holeCount"/>.</param>
        /// <param name="allocator">The allocator to use if <paramref name="polygonOut"/> must be resized.
        /// Must be <c>Allocator.TempJob</c> or <c>Allocator.Persistent</c>.</param>
        /// <param name="polygonOut">The resulting polygon describing the hole at <paramref name="index"/>.</param>
        /// <exception cref="System.InvalidOperationException">Thrown if <see cref="valid"/> is false.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if <paramref name="allocator"/> is <c>Allocator.Temp</c> or <c>Allocator.None</c>.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is less than 0 or greater than or equal to <see cref="holeCount"/>.</exception>
        public unsafe void GetHole(int index, Allocator allocator, ref NativeArray<Vector2> polygonOut)
        {
            if (!valid)
                throw new InvalidOperationException("This plane boundary is not valid.");

            if (allocator == Allocator.Temp)
                throw new InvalidOperationException("Allocator.Temp is not supported. Use Allocator.TempJob if you wish to use a temporary allocator.");

            if (allocator == Allocator.None)
                throw new InvalidOperationException("Allocator.None is not a valid allocator.");

            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "Hole index must be greater than zero.");

            if (index >= holeCount)
                throw new ArgumentOutOfRangeException(nameof(index), $"Hole index must be less than or equal to holeCount ({holeCount}).");

            var holes = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<MLPolygon>(
                m_Boundary.holes,
                holeCount,
                Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECK
            var safetyHandle = AtomicSafetyHandle.Create();
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref holes, safetyHandle);
#endif

            TransformMLPolygon(holes[index], m_Pose, allocator, ref polygonOut);
        }

        /// <summary>
        /// Computes a hash code suitable for use in a <c>Dictionary</c> or <c>HashSet</c>.
        /// </summary>
        /// <returns>A hash code suitable for use in a <c>Dictionary</c> or <c>HashSet</c>.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = m_Boundary.GetHashCode();
                hash = hash * 486187739 + m_Pose.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// IEquatable interface. Compares for equality.
        /// </summary>
        /// <param name="obj">The object to compare for equality.</param>
        /// <returns><c>true</c> if <paramref name="obj"/> is of type <see cref="PlaneBoundary"/> and compares equal with <see cref="Equals(PlaneBoundary)"/>.</returns>
        public override bool Equals(object obj)
        {
            return ((obj is PlaneBoundary) && Equals((PlaneBoundary)obj));
        }

        /// <summary>
        /// IEquatable interface. Compares for equality.
        /// </summary>
        /// <param name="other">The <see cref="PlaneBoundary"/> to compare against.</param>
        /// <returns><c>true</c> if all fields of this <see cref="PlaneBoundary"/> compare equal to <paramref name="other"/>.</returns>
        public bool Equals(PlaneBoundary other)
        {
            return
                m_Boundary.Equals(other.m_Boundary) &&
                m_Pose.Equals(other.m_Pose);
        }

        /// <summary>
        /// Compares for equality. Same as <see cref="Equals(PlaneBoundary)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns><c>true</c> if all fields of this <see cref="PlaneBoundary"/> compare equal to the fields of the other 
        /// <see cref="PlaneBoundary"/>.</returns>
        public static bool operator ==(PlaneBoundary lhs, PlaneBoundary rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Compares for inequality. Same as <c>!</c><see cref="Equals(PlaneBoundary)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns><c>true</c> if any field of this <see cref="PlaneBoundary"/> is not equal to the same field on 
        /// the other <see cref="PlaneBoundary"/>.</returns>
        public static bool operator !=(PlaneBoundary lhs, PlaneBoundary rhs)
        {
            return !lhs.Equals(rhs);
        }

        static unsafe void TransformMLPolygon(MLPolygon mlPolygon, Pose pose, Allocator allocator, ref NativeArray<Vector2> polygonOut)
        {
            var mlVertices = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Vector3>(
                mlPolygon.vertices,
                (int)mlPolygon.vertices_count,
                Allocator.None);

#if ENABLE_UNITY_COLLECTIONS_CHECK
            var safetyHandle = AtomicSafetyHandle.Create();
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref mlVertices, safetyHandle);
#endif

            CreateOrResizeNativeArrayIfNecessary(mlVertices.Length, allocator, ref polygonOut);

            // The vertices are provided in session space, so we need to transform
            // them into plane-space.
            new TransformPlaneBoundaryJob
            {
                m_InvRotation = Quaternion.Inverse(CopyPlaneResultsJob.TransformUnityRotationToML(pose.rotation)),
                m_Position = pose.position,
                m_VerticesIn = mlVertices,
                m_VerticesOut = polygonOut
            }.Schedule(mlVertices.Length, 1).Complete();
        }

        internal unsafe PlaneBoundary(MLPlaneBoundary planeBoundary, Pose pose)
        {
            m_Boundary = planeBoundary;
            m_Pose = pose;

            if (m_Boundary.polygon != null)
            {
                polygonVertexCount = (int)(*m_Boundary.polygon).vertices_count;
            }
            else
            {
                polygonVertexCount = 0;
            }
        }

        static void CreateOrResizeNativeArrayIfNecessary<T>(
            int length,
            Allocator allocator,
            ref NativeArray<T> array) where T : struct
        {
            if (array.IsCreated)
            {
                if (array.Length != length)
                {
                    array.Dispose();
                    array = new NativeArray<T>(length, allocator);
                }
            }
            else
            {
                array = new NativeArray<T>(length, allocator);
            }
        }

        MLPlaneBoundary m_Boundary;

        Pose m_Pose;
    }
}

using System;
using UnityEngine;

namespace Assets.Scripts.Tools
{
    public class BuildHelper : MonoBehaviour
    {
        public enum BuildType
        {
            Frame,
            Surface,
        }
        
        public ChunkMap ChunkMapToEdit = null;
        public ChunkData ChunkDataToEdit = null;
        public BuildType Type;
        public Mesh PreviewMesh = null;
        public Material PreviewMaterial = null;
        public IntVec3 EditPosition;
        public int Face;
        public int MappedFace;
        public int BlockOrSurfaceType;

        public bool Touched = false;

        public readonly float MaxActionTime = 0.5f;
        public float CurrentActionTime;

        public readonly float MaxTimeoutTime = 5.0f;
        public float CurrentTimeoutTime;

        public readonly float TimeBeforeInactive = 0.5f;

        public float LastInteractionTime;

        private void Start()
        {
        }

        public void StartFrameAction(ChunkMap toEdit, int mappedFace,
            int frameType, IntVec3 editPosition,
            Mesh previewMesh, Material previewMaterial)
        {
            if (isActiveAndEnabled)
            {
                return;
            }

            ChunkMapToEdit = toEdit;
            MappedFace = mappedFace;
            BlockOrSurfaceType = frameType;
            EditPosition = editPosition;
            PreviewMesh = previewMesh;
            PreviewMaterial = previewMaterial;

            var pos = IntVec3.BlockCoordToChunkCoord(editPosition);
            ChunkDataToEdit = ChunkMapToEdit.EnsureChunk(pos);

            // reserve action in chunk?

            Type = BuildType.Frame;

            enabled = true;
        }

        public void StartSurfaceAction(ChunkMap toEdit, int mappedFace,
            int surfaceType, IntVec3 editPosition,
            Mesh previewMesh, Material previewMaterial)
        {
            if (isActiveAndEnabled)
            {
                return;
            }

            ChunkMapToEdit = toEdit;
            MappedFace = mappedFace;
            BlockOrSurfaceType = surfaceType;
            EditPosition = editPosition;
            PreviewMesh = previewMesh;
            PreviewMaterial = previewMaterial;

            var pos = IntVec3.BlockCoordToChunkCoord(editPosition);
            ChunkDataToEdit = ChunkMapToEdit.EnsureChunk(pos);

            // reserve action in chunk?

            Type = BuildType.Surface;

            enabled = true;
        }

        public void ActOn()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            Touched = true;
        }

        private void Update()
        {
            var dt = Time.deltaTime;

            if (!isActiveAndEnabled)
            {
                CurrentTimeoutTime += dt;
            }

            if (CurrentTimeoutTime >= MaxTimeoutTime)
            {
                Destroy(gameObject);
                return;
            }

            if (!isActiveAndEnabled)
            {
                return;
            }

            // this time stuff probably wants fsm
            if (Touched)
            {
                CurrentActionTime += dt;
                LastInteractionTime = Time.time;
                CurrentTimeoutTime = 0f;

                Touched = false;
            }
            else if (Time.time - LastInteractionTime >= TimeBeforeInactive)
            {
                // reset build progress if inactive
                CurrentActionTime = 0;
                CurrentTimeoutTime += dt;
            }

            if (CurrentActionTime >= MaxActionTime)
            {
                FinishBuild();
                
                Destroy(gameObject);
                return;
            }

            UpdatePreview();
        }

        private void UpdatePreview()
        {
            var ceTrans = ChunkMapToEdit.transform;
            var pv3 = ChunkDataToEdit.BlockNegativeCornerToWorldSpace(EditPosition);

            Graphics.DrawMesh(PreviewMesh, pv3, ceTrans.rotation, PreviewMaterial, 0);
        }

        private void FinishBuild()
        {
            switch (Type)
            {
                case BuildType.Frame:
                    UpdateFrame();
                    break;
                case BuildType.Surface:
                    UpdateSurface();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // Place block tool -- places a block against the block we hit, sharing the face we hit
        private void UpdateFrame()
        {
            var co = EditPosition;
            ChunkDataToEdit.Contents[co.x, co.y, co.z] = (byte) BlockOrSurfaceType;
            ChunkDataToEdit.generation++;

            //AudioSource.PlayClipAtPoint(PlaceSound, ray.origin + fc.t * ray.direction);
        }

        // Surface placement
        private void UpdateSurface()
        {
            var co = EditPosition;
            ChunkDataToEdit.Faces[co.x, co.y, co.z, MappedFace] = (byte)BlockOrSurfaceType;
            ChunkDataToEdit.generation++;

            //AudioSource.PlayClipAtPoint(PlaceSound, ray.origin + fc.t * ray.direction);
        }

        private void OnGUI()
        {
            if (!GUI.enabled)
            {
                return;
            }
            var cam = Camera.main;
            var pos = ChunkDataToEdit.BlockNegativeCornerToWorldSpace(EditPosition) +
                new Vector3(0.5f, 0.5f, 0.5f);
            var sPos = cam.WorldToScreenPoint(pos);

            string str;
            if (Time.time - LastInteractionTime <= TimeBeforeInactive)
            {
                str = string.Format("{0}%", (Math.Round(CurrentActionTime / MaxActionTime * 100)));
            }
            else
            {
                str = string.Format("!! {0}", MaxTimeoutTime - CurrentTimeoutTime);
            }
            GUI.Label(new Rect(sPos.x, Screen.height - sPos.y, 150, 130), str);
        }
    }
}

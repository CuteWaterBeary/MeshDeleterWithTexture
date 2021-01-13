﻿using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/*
 * Copyright (c) 2019 gatosyocora
 * Released under the MIT license.
 * see LICENSE.txt
 */

namespace Gatosyocora.MeshDeleterWithTexture.Utilities
{
    public static class RendererUtility
    {
        /// <summary>
        /// Meshを取得する
        /// </summary>
        /// <param name="renderer"></param>
        /// <returns></returns>
        public static Mesh GetMesh(Renderer renderer)
        {
            Mesh mesh = null;
            if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
            {
                mesh = skinnedMeshRenderer.sharedMesh;
            }
            else if (renderer is MeshRenderer meshRenderer)
            {
                mesh = renderer.GetComponent<MeshFilter>().sharedMesh;
            }
            return mesh;
        }

        /// <summary>
        /// Meshを設定する
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="mesh"></param>
        public static void SetMesh(Renderer renderer, Mesh mesh)
        {
            if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
            {
                skinnedMeshRenderer.sharedMesh = mesh;
            }
            else if (renderer is MeshRenderer)
            {
                renderer.GetComponent<MeshFilter>().sharedMesh = mesh;
            }
        }

        /// <summary>
        /// Meshのポリゴン数を取得する
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static int GetMeshTriangleCount(Mesh mesh)
            => mesh.triangles.Length / 3;

        /// <summary>
        /// テクスチャを取得する
        /// </summary>
        /// <param name="renderer"></param>
        /// <returns></returns>
        public static Texture2D[] GetMainTextures(Renderer renderer)
        {
            var materials = renderer.sharedMaterials;
            var textures = new Texture2D[materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                textures[i] = GetMainTexture(materials[i]);
            }
            return textures;
        }

        /// <summary>
        /// MainTexを取得する
        /// _MainTexプロパティがなければ一番最初のテクスチャを取得する
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        public static Texture2D GetMainTexture(Material mat)
        {
            var mainTex = mat.mainTexture as Texture2D;

            // シェーダーに_MainTexが含まれていない時, nullになるため
            // Materialの一番始めに設定されているTextureを取得する
            if (mainTex == null)
            {
                foreach (var textureName in mat.GetTexturePropertyNames())
                {
                    mainTex = mat.GetTexture(textureName) as Texture2D;
                    if (mainTex != null) break;
                }
            }

            // この時点でmainTexがnullならmaterialにテクスチャが設定されていない
            return mainTex;
        }

        /// <summary>
        /// mesh保存先のパスを取得する
        /// </summary>
        /// <param name="Mesh"></param>
        /// <returns></returns>
        public static string GetMeshPath(Mesh mesh)
            => Path.GetDirectoryName(AssetDatabase.GetAssetPath(mesh));

        // TODO: メッシュ単位で戻ってしまう（サブメッシュ単位で戻したい）
        /// <summary>
        /// Rendererに設定されたMeshとMaterialsをPrefabの状態に戻す
        /// </summary>
        /// <param name="renderer"></param>
        public static void RevertMeshToPrefab(Renderer renderer)
        {
            if (!PrefabUtility.IsPartOfPrefabAsset(renderer)) return;

            PrefabUtility.ReconnectToLastPrefab(renderer.gameObject);

            SerializedObject so = null;
            if (renderer is SkinnedMeshRenderer)
            {
                so = new SerializedObject(renderer);
            }
            else if (renderer is MeshRenderer)
            {
                so = new SerializedObject(renderer.GetComponent<MeshFilter>());
            }
            so.Update();

            var sp = so.FindProperty("m_Mesh");
#if UNITY_2018_3_OR_NEWER
            PrefabUtility.RevertPropertyOverride(sp, InteractionMode.UserAction);
#else
            sp.prefabOverride = false;
            sp.serializedObject.ApplyModifiedProperties();
#endif
        }
    }
}
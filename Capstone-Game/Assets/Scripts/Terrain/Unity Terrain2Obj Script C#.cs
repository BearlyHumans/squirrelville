using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
 
internal enum SaveFormat { Triangles, Quads }
internal enum SaveResolution { Full=0, Half, Quarter, Eighth, Sixteenth }

internal class Terrain2Obj : EditorWindow
{
   private SaveFormat _saveFormat = SaveFormat.Triangles;
   private SaveResolution _saveResolution = SaveResolution.Half;

   private static TerrainData _terrain;
   private static Vector3 _terrainPos;

   private int _tCount;
   private int _counter;
   private int _totalCount;
   private const int ProgressUpdateInterval = 10000;

   [MenuItem("Terrain/Export To Obj...")]
   private static void Init()
   {
      _terrain = null;
      var terrainObject = Selection.activeObject as Terrain;
      if (!terrainObject)
      {
         terrainObject = Terrain.activeTerrain;
      }
      if (terrainObject)
      {
         _terrain = terrainObject.terrainData;
         _terrainPos = terrainObject.transform.position;
      }
 
      EditorWindow.GetWindow<Terrain2Obj>().Show();
   }

   private void OnGUI()
   {
      if (!_terrain)
      {
         GUILayout.Label("No terrain found");
         if (GUILayout.Button("Cancel"))
         {
            EditorWindow.GetWindow<Terrain2Obj>().Close();
         }
         return;
      }
      _saveFormat = (SaveFormat) EditorGUILayout.EnumPopup("Export Format", _saveFormat);
 
      _saveResolution = (SaveResolution) EditorGUILayout.EnumPopup("Resolution", _saveResolution);
 
      if (GUILayout.Button("Export"))
      {
         Export();
      }
   }

   private void Export()
   {
      var fileName = EditorUtility.SaveFilePanel("Export .obj file", "", "Terrain", "obj");
      var w = _terrain.heightmapResolution;
      var h = _terrain.heightmapResolution;
      var meshScale = _terrain.size;
      var tRes = (int)Mathf.Pow(2, (int)_saveResolution );
      meshScale = new Vector3(meshScale.x / (w - 1) * tRes, meshScale.y, meshScale.z / (h - 1) * tRes);
      var uvScale = new Vector2(1.0f / (w - 1), 1.0f / (h - 1));
      var tData = _terrain.GetHeights(0, 0, w, h);
 
      w = (w - 1) / tRes + 1;
      h = (h - 1) / tRes + 1;
      var tVertices = new Vector3[w * h];
      var tUV = new Vector2[w * h];
 
      int[] tPolys;
 
      tPolys = _saveFormat == SaveFormat.Triangles ? new int[(w - 1) * (h - 1) * 6] : new int[(w - 1) * (h - 1) * 4];
 
      // Build vertices and UVs
      for (var y = 0; y < h; y++)
      {
         for (var x = 0; x < w; x++)
         {
            tVertices[y * w + x] = Vector3.Scale(meshScale, new Vector3(-y, tData[x * tRes, y * tRes], x)) + _terrainPos;
            tUV[y * w + x] = Vector2.Scale( new Vector2(x * tRes, y * tRes), uvScale);
         }
      }
 
      var  index = 0;
      if (_saveFormat == SaveFormat.Triangles)
      {
         // Build triangle indices: 3 indices into vertex array for each triangle
         for (var y = 0; y < h - 1; y++)
         {
            for (var x = 0; x < w - 1; x++)
            {
               // For each grid cell output two triangles
               tPolys[index++] = (y * w) + x;
               tPolys[index++] = ((y + 1) * w) + x;
               tPolys[index++] = (y * w) + x + 1;
 
               tPolys[index++] = ((y + 1) * w) + x;
               tPolys[index++] = ((y + 1) * w) + x + 1;
               tPolys[index++] = (y * w) + x + 1;
            }
         }
      }
      else
      {
         // Build quad indices: 4 indices into vertex array for each quad
         for (var y = 0; y < h - 1; y++)
         {
            for (var x = 0; x < w - 1; x++)
            {
               // For each grid cell output one quad
               tPolys[index++] = (y * w) + x;
               tPolys[index++] = ((y + 1) * w) + x;
               tPolys[index++] = ((y + 1) * w) + x + 1;
               tPolys[index++] = (y * w) + x + 1;
            }
         }
      }
 
      // Export to .obj
      var sw = new StreamWriter(fileName);
      try
      {
 
         sw.WriteLine("# Unity terrain OBJ File");
 
         // Write vertices
         System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
         _counter = _tCount = 0;
         _totalCount = (tVertices.Length * 2 + (_saveFormat == SaveFormat.Triangles ? tPolys.Length / 3 : tPolys.Length / 4)) / ProgressUpdateInterval;
         for (var i = 0; i < tVertices.Length; i++)
         {
            UpdateProgress();
            var sb = new StringBuilder("v ", 20);
            // StringBuilder stuff is done this way because it's faster than using the "{0} {1} {2}"etc. format
            // Which is important when you're exporting huge terrains.
            sb.Append(tVertices[i].x.ToString()).Append(" ").
               Append(tVertices[i].y.ToString()).Append(" ").
               Append(tVertices[i].z.ToString());
            sw.WriteLine(sb);
         }
         // Write UVs
         for (int i = 0; i < tUV.Length; i++)
         {
            UpdateProgress();
            var sb = new StringBuilder("vt ", 22);
            sb.Append(tUV[i].x.ToString()).Append(" ").
               Append(tUV[i].y.ToString());
            sw.WriteLine(sb);
         }
         if (_saveFormat == SaveFormat.Triangles)
         {
            // Write triangles
            for (int i = 0; i < tPolys.Length; i += 3)
            {
               UpdateProgress();
               var sb = new StringBuilder("f ", 43);
               sb.Append(tPolys[i] + 1).Append("/").Append(tPolys[i] + 1).Append(" ").
                  Append(tPolys[i + 1] + 1).Append("/").Append(tPolys[i + 1] + 1).Append(" ").
                  Append(tPolys[i + 2] + 1).Append("/").Append(tPolys[i + 2] + 1);
               sw.WriteLine(sb);
            }
         }
         else
         {
            // Write quads
            for (var i = 0; i < tPolys.Length; i += 4)
            {
               UpdateProgress();
               var sb = new StringBuilder("f ", 57);
               sb.Append(tPolys[i] + 1).Append("/").Append(tPolys[i] + 1).Append(" ").
                  Append(tPolys[i + 1] + 1).Append("/").Append(tPolys[i + 1] + 1).Append(" ").
                  Append(tPolys[i + 2] + 1).Append("/").Append(tPolys[i + 2] + 1).Append(" ").
                  Append(tPolys[i + 3] + 1).Append("/").Append(tPolys[i + 3] + 1);
               sw.WriteLine(sb);
            }
         }
      }
      catch(Exception err)
      {
         Debug.Log("Error saving file: " + err.Message);
      }
      sw.Close();
 
      _terrain = null;
      EditorUtility.DisplayProgressBar("Saving file to disc.", "This might take a while...", 1f);
      EditorWindow.GetWindow<Terrain2Obj>().Close();      
      EditorUtility.ClearProgressBar();
   }

   private void UpdateProgress()
   {
      if (_counter++ != ProgressUpdateInterval) return;
      _counter = 0;
      EditorUtility.DisplayProgressBar("Saving...", "", Mathf.InverseLerp(0, _totalCount, ++_tCount));
   }
}
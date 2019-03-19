//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace JBooth.MicroSplat
{
   public interface IRenderLoopAdapter 
   {
      string GetDisplayName();
      string GetRenderLoopKeyword();
      void Init(string[] paths);
      int GetNumPasses();
      void WriteShaderHeader(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, bool blend);
      void WritePassHeader(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, int pass, bool blend);
      void WriteVertexFunction(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, int pass, bool blend);
      void WriteFragmentFunction(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, int pass, bool blend);
      void WriteShaderFooter(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, bool blend, string baseName);
      void PostProcessShader(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, bool blend);
      void WriteSharedCode(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, int pass, bool blend);
      void WriteTerrainBody(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, int pass, bool blend);
      MicroSplatShaderGUI.PassType GetPassType(int i);
      string GetVersion();
   }
}

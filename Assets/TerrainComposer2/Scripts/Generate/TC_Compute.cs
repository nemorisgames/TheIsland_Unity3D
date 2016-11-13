using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace TerrainComposer2
{
    [ExecuteInEditMode]
    public class TC_Compute : MonoBehaviour
    { 
        static public TC_Compute instance;
        public TC_CamCapture camCapture;
        public Transform target;
        public bool run;
        public ComputeShader shader;
        public string path;
        [NonSerialized] public float threads = 1024;
        public int collisionMask;

        public PerlinNoise m_perlin;

        int[] methodKernel, methodTexKernel, colorMethodTexKernel, multiMethodTexKernel; // multiMethodKernel
        int[] noisePerlinKernel, noiseBillowKernel, noiserRidgedKernel;
        int[] noisePerlin2Kernel, noiseBillow2Kernel, noiseRidged2Kernel, noiseIQKernel, noiseSwissKernel, noiseJordanKernel;
        int noiseRandomKernel, noiseCellNormalKernel, noiseCellFastKernel;

        int colorMethodMultiplyBufferKernel, colorMethodTexLerpMaskKernel;
        int multiMethodMultiplyBufferKernel;

        public int terrainHeightKernel, terrainAngleKernel, terrainSplatmap0Kernel, terrainSplatmap1Kernel, terrainConvexityKernel;
        public int terrainCollisionHeightKernel, terrainCollisionHeightIncludeKernel, terrainCollisionMaskKernel;
        
        int methodLerpMaskKernel, methodTexLerpMaskKernel, multiMethodTexLerpMaskKernel;
        int shapeGradientKernel, shapeCircleKernel, shapeSquareKernel, shapeConstantKernel;
        int rawImageKernel, imageColorKernel, imageColorRangeKernel;
        int currentBlurNormalKernel, currentBlurOutwardKernel, currentBlurInwardKernel, currentExpandKernel, currentShrinkKernel, currentEdgeDetectKernel, currentDistortionKernel;
        int calcColorKernel;
        int calcSplatKernel, normalizeSplatKernel;
        int calcObjectKernel, methodItemTexMaskKernel, methodItemTex0MaskKernel, calcObjectPositionKernel;
        int terrainTexKernel, resultBufferToTexKernel;
        
        int copyBufferKernel, copyRenderTextureKernel;

        int methodItemTexMaxKernel, methodItemTexMinKernel, methodItemTexLerpKernel, methodItemTexLerpMaskKernel;

        Vector3 posOld, scaleOld;
        Quaternion rotOld;
        float bufferLength;

        public RenderTexture[] rtsColor;
        public RenderTexture[] rtsSplatmap;
        public RenderTexture[] rtsResult;
        public RenderTexture rtResult;
        public RenderTexture rtSplatPreview;
        public Texture2D[] texGrassmaps;
        
        public Vector4[] splatColors;
        public Vector4[] colors;

        [NonSerialized] public BytesArray[] bytesArray;


        // TODO: Only set shader parameters that really need to be set and aren't set already
        void OnEnable()
        {
            instance = this;

            methodKernel = new int[9];
            methodTexKernel = new int[9];
            colorMethodTexKernel = new int[9];
            // multiMethodKernel = new int[9];
            multiMethodTexKernel = new int[9];

            noisePerlinKernel = new int[12];
            noiseBillowKernel = new int[12];
            noiserRidgedKernel = new int[12];
            noisePerlin2Kernel = new int[3];
            noiseBillow2Kernel = new int[3];
            noiseRidged2Kernel = new int[3];
            noiseIQKernel = new int[3];
            noiseSwissKernel = new int[3];
            noiseJordanKernel = new int[3];

            TC_Reporter.Log("Init compute");
            TC_Reporter.Log(methodKernel.Length + " - " + methodTexKernel.Length);
            
            string method;

            for (int i = 0; i < 9; i++)
            {
                method = ((Method)i).ToString();
                methodKernel[i] = shader.FindKernel("Method" + method); 
                methodTexKernel[i] = shader.FindKernel("MethodTex" + method);

                // multiMethodKernel[i] = shader.FindKernel("MultiMethod" + method);
                multiMethodTexKernel[i] = shader.FindKernel("MultiMethodTex" + method);
                colorMethodTexKernel[i] = shader.FindKernel("ColorMethodTex" + method);
                // Reporter.Log(methodKernel[i] + ", " + methodTexKernel[i]+ " "+((Method)i).ToString());
                // TC_Reporter.Log(multiMethodKernel[i] + ", " + multiMethodTexKernel[i] + " " + ((Method)i).ToString());
                // Debug.Log(colorMethodTexKernel[i]);
            }

            // Debug.Log(noiseBillowKernel.Length);

            for (int i = 1; i < noisePerlinKernel.Length + 1; i++) noisePerlinKernel[i - 1] = shader.FindKernel("NoisePerlin" + i.ToString());
            for (int i = 1; i < noiseBillowKernel.Length + 1; i++) noiseBillowKernel[i - 1] = shader.FindKernel("NoiseBillow" + i.ToString());
            for (int i = 1; i < noiserRidgedKernel.Length + 1; i++) noiserRidgedKernel[i - 1] = shader.FindKernel("NoiseMultiFractal" + i.ToString());
            for (int i = 0; i < 3; i++)
            {
                noisePerlin2Kernel[i] = shader.FindKernel("NoisePerlin" + Enum.GetName(typeof(NoiseMode), i + 1));
                noiseBillow2Kernel[i] = shader.FindKernel("NoiseBillow" + Enum.GetName(typeof(NoiseMode), i + 1));
                noiseRidged2Kernel[i] = shader.FindKernel("NoiseRidged" + Enum.GetName(typeof(NoiseMode), i + 1));
                noiseIQKernel[i] = shader.FindKernel("NoiseIQ" + Enum.GetName(typeof(NoiseMode), i + 1));
                noiseSwissKernel[i] = shader.FindKernel("NoiseSwiss" + Enum.GetName(typeof(NoiseMode), i + 1));
                noiseJordanKernel[i] = shader.FindKernel("NoiseJordan" + Enum.GetName(typeof(NoiseMode), i + 1));
            }
            noiseCellNormalKernel = shader.FindKernel("NoiseCellNormal");
            noiseCellFastKernel = shader.FindKernel("NoiseCellFast");
            
            colorMethodTexLerpMaskKernel = shader.FindKernel("ColorMethodTexLerpMask");
            // Debug.Log("colorMethodTexLerpMaskKernel " + colorMethodTexLerpMaskKernel);
            colorMethodMultiplyBufferKernel = shader.FindKernel("ColorMethodMultiplyBuffer");
            multiMethodMultiplyBufferKernel = shader.FindKernel("MultiMethodMultiplyBuffer");
            
            terrainHeightKernel = shader.FindKernel("TerrainHeight");
            terrainAngleKernel = shader.FindKernel("TerrainAngle");
            terrainConvexityKernel = shader.FindKernel("TerrainConvexity"); 
            terrainSplatmap0Kernel = shader.FindKernel("TerrainSplatmap0");
            terrainSplatmap1Kernel = shader.FindKernel("TerrainSplatmap1");
            terrainCollisionHeightKernel = shader.FindKernel("TerrainCollisionHeight");
            terrainCollisionHeightIncludeKernel = shader.FindKernel("TerrainCollisionHeightInclude"); 
            terrainCollisionMaskKernel = shader.FindKernel("TerrainCollisionMask");

            noiseRandomKernel = shader.FindKernel("NoiseRandom");

            rawImageKernel = shader.FindKernel("RawImage");
            imageColorKernel = shader.FindKernel("ImageColor"); 
            imageColorRangeKernel = shader.FindKernel("ImageColorRange");

            shapeGradientKernel = shader.FindKernel("ShapeGradient");
            shapeCircleKernel = shader.FindKernel("ShapeCircle");
            shapeSquareKernel = shader.FindKernel("ShapeSquare");
            shapeConstantKernel = shader.FindKernel("ShapeConstant");

            currentBlurNormalKernel = shader.FindKernel("CurrentBlurNormal");
            currentBlurOutwardKernel = shader.FindKernel("CurrentBlurOutward");
            currentBlurInwardKernel = shader.FindKernel("CurrentBlurInward");
            currentExpandKernel = shader.FindKernel("CurrentExpand");
            currentShrinkKernel = shader.FindKernel("CurrentShrink");
            currentEdgeDetectKernel = shader.FindKernel("CurrentEdgeDetect");
            currentDistortionKernel = shader.FindKernel("CurrentDistortion");
            
            methodLerpMaskKernel = shader.FindKernel("MethodLerpMask");
            methodTexLerpMaskKernel = shader.FindKernel("MethodTexLerpMask");
            multiMethodTexLerpMaskKernel = shader.FindKernel("MultiMethodTexLerpMask");

            calcColorKernel = shader.FindKernel("CalcColor");
            calcSplatKernel = shader.FindKernel("CalcSplat");
            calcObjectKernel = shader.FindKernel("CalcObject");
            calcObjectPositionKernel = shader.FindKernel("CalcObjectPosition");
            methodItemTexMaskKernel = shader.FindKernel("MethodItemTexMask");
            methodItemTex0MaskKernel = shader.FindKernel("MethodItemTex0Mask");
            normalizeSplatKernel = shader.FindKernel("NormalizeSplat");
            terrainTexKernel = shader.FindKernel("TerrainTex");
            resultBufferToTexKernel = shader.FindKernel("ResultBufferToTex");

            methodItemTexMaxKernel = shader.FindKernel("MethodItemTexMax");
            methodItemTexMinKernel = shader.FindKernel("MethodItemTexMin");
            methodItemTexLerpKernel = shader.FindKernel("MethodItemTexLerp");
            methodItemTexLerpMaskKernel = shader.FindKernel("MethodItemTexLerpMask");

            copyBufferKernel = shader.FindKernel("CopyBuffer");
            copyRenderTextureKernel = shader.FindKernel("CopyRenderTexture");

            // shader.SetTexture(resultBufferToTexKernel, "resultTex", resultTex); 

            if (TC_Settings.instance == null) return;// Reporter.Log("GM singleton null");
            if (TC_Settings.instance.global == null)
            {
                TC.GetInstallPath();
                if (!TC.LoadGlobalSettings()) return;
            }
            splatColors = Mathw.ColorsToVector4(TC_Settings.instance.global.previewColors);

            TC_Reporter.Log("LerpKernel " + methodLerpMaskKernel + " - " + methodTexLerpMaskKernel);
            TC_Reporter.Log(rawImageKernel + " - " + noisePerlinKernel + " - " + shapeConstantKernel);
        }
        
        void OnDestroy()
        {
            instance = null;
            DisposeTextures();
        }

        public void DisposeTextures()
        {
            DisposeRenderTextures(ref rtsColor);
            DisposeRenderTextures(ref rtsSplatmap);
            DisposeRenderTextures(ref rtsResult);
            DisposeRenderTexture(ref rtResult);
            DisposeRenderTexture(ref rtSplatPreview);

            DisposeTextures(ref texGrassmaps);
            DisposeTexture(ref m_perlin.m_permTable1D);
            DisposeTexture(ref m_perlin.m_permTable2D);
            DisposeTexture(ref m_perlin.m_gradient2D);
            DisposeTexture(ref m_perlin.m_gradient3D);
            DisposeTexture(ref m_perlin.m_gradient4D);
        }

        public void InitCurves(TC_ItemBehaviour item)
        {
            item.localCurve.ConvertCurve();
            item.worldCurve.ConvertCurve();
        }

        public void SetPreviewColors(Vector4[] colors)
        {
            // for (int i = 0; i < colors.Length; i++) shader.SetVector("itemColor" + i.ToString(), colors[i]);
        }

        public void RunColorCompute(TC_NodeGroup nodeGroup, TC_SelectItemGroup itemGroup, ref RenderTexture rt, ref ComputeBuffer resultBuffer)
        {
            ComputeBuffer colorMixBuffer = new ComputeBuffer(itemGroup.colorMixBuffer.Length, 28);
            colorMixBuffer.SetData(itemGroup.colorMixBuffer);

            int kernel = calcColorKernel;

            shader.SetInt("itemCount", itemGroup.colorMixBuffer.Length);

            shader.SetBuffer(kernel, "resultBuffer", resultBuffer);
            shader.SetTexture(kernel, "splatmap0", rt);
            shader.SetTexture(kernel, "splatPreviewTex", nodeGroup.rtColorPreview);

            shader.SetBuffer(kernel, "colorMixBuffer", colorMixBuffer);
            // shader.SetBuffer(kernel, "itemColorBuffer", itemColorBuffer);
            shader.SetVector("resConversion", TC_Area2D.current.resolutionPM);
            shader.SetVector("resToPreview", TC_Area2D.current.resToPreview);
            shader.SetInt("resolutionX", TC_Area2D.current.intResolution.x);
            shader.SetInt("resolutionY", TC_Area2D.current.intResolution.y);

            Int2 resolution = TC_Area2D.current.intResolution;
            bufferLength = resolution.x * resolution.y;

            if (kernel == -1) { Debug.Log("Kernel not found"); return; }
            shader.Dispatch(kernel, Mathf.CeilToInt(bufferLength / threads), 1, 1);

            DisposeBuffer(ref resultBuffer);
            DisposeBuffer(ref colorMixBuffer);
            // DisposeBuffer(ref itemColorBuffer);
        }

        public void RunSplatCompute(TC_NodeGroup nodeGroup, TC_SelectItemGroup itemGroup, ref RenderTexture[] rts, ref ComputeBuffer resultBuffer)
        {
            // ComputeBuffer resultBuffer = RunShader(null, node);
            
            ComputeBuffer splatMixBuffer = new ComputeBuffer(itemGroup.splatMixBuffer.Length, 48);
            splatMixBuffer.SetData(itemGroup.splatMixBuffer);

            ComputeBuffer itemColorBuffer = new ComputeBuffer(8, 16);
            itemColorBuffer.SetData(TC_Settings.instance.global.previewColors);
            // itemColorBuffer.SetData(TC_Area2D.current.currentTCUnityTerrain.splatColors);

            int kernel = calcSplatKernel;

            shader.SetInt("itemCount", itemGroup.splatMixBuffer.Length);

            shader.SetBuffer(kernel, "resultBuffer", resultBuffer);
            shader.SetTexture(kernel, "splatmap0", rts[0]);
            if (rts.Length > 1) shader.SetTexture(kernel, "splatmap1", rts[1]);
            shader.SetTexture(kernel, "splatPreviewTex", nodeGroup.rtColorPreview);

            shader.SetBuffer(kernel, "splatMixBuffer", splatMixBuffer);
            shader.SetBuffer(kernel, "itemColorBuffer", itemColorBuffer);
            shader.SetVector("resConversion", TC_Area2D.current.resolutionPM);
            shader.SetVector("resToPreview", TC_Area2D.current.resToPreview);
            shader.SetInt("resolutionX", TC_Area2D.current.intResolution.x);
            shader.SetInt("resolutionY", TC_Area2D.current.intResolution.y);

            Int2 resolution = TC_Area2D.current.intResolution;
            bufferLength = resolution.x * resolution.y;

            if (kernel == -1) { Debug.Log("Kernel not found"); return; }
            shader.Dispatch(kernel, Mathf.CeilToInt(bufferLength / threads), 1, 1);

            DisposeBuffer(ref resultBuffer);
            DisposeBuffer(ref splatMixBuffer);
            DisposeBuffer(ref itemColorBuffer);
        }

        public void RunItemCompute(TC_Layer layer, ref ComputeBuffer itemMapBuffer, ref ComputeBuffer resultBuffer)
        {
            TC_GlobalSettings global = TC_Settings.instance.global;
            TC_Area2D area2D = TC_Area2D.current;

            TC_SelectItemGroup itemGroup = layer.selectItemGroup;
            TC_NodeGroup nodeGroup = layer.selectNodeGroup;

            int kernel = calcObjectKernel;
            // Debug.Log(kernel);

            ComputeBuffer itemIndexBuffer = new ComputeBuffer(itemGroup.indices.Length, 16);
            itemIndexBuffer.SetData(itemGroup.indices);

            //for (int i = 0; i < itemGroup.indices.Length; i++) Debug.Log(itemGroup.indices[i]);
            //Debug.Log("****************");

            ComputeBuffer itemColorBuffer = new ComputeBuffer(8, 16); 
            itemColorBuffer.SetData(global.previewColors);

            int resolution = area2D.intResolution.x * area2D.intResolution.y;
            itemMapBuffer = new ComputeBuffer(resolution, 24);

            shader.SetBuffer(kernel, "itemIndexBuffer", itemIndexBuffer);
            shader.SetBuffer(kernel, "itemColorBuffer", itemColorBuffer);
            shader.SetBuffer(kernel, "resultBuffer", resultBuffer);
            shader.SetBuffer(kernel, "itemMapBuffer", itemMapBuffer);

            shader.SetTexture(kernel, "splatPreviewTex", nodeGroup.rtColorPreview);
            shader.SetTexture(kernel, "previewTex", layer.rtPreview);

            shader.SetVector("colLayer", global.GetVisualizeColor(layer.listIndex));
            shader.SetInt("itemCount", itemGroup.indices.Length);
            shader.SetInt("resolutionX", area2D.intResolution.x);
            shader.SetInt("resolutionY", area2D.intResolution.y);
            shader.SetFloat("mixValue", itemGroup.mix);

            shader.SetVector("resConversion", TC_Area2D.current.resolutionPM);
            shader.SetVector("resToPreview", TC_Area2D.current.resToPreview);
            shader.SetVector("areaPos", TC_Area2D.current.area.position);
            shader.SetVector("totalAreaPos", TC_Area2D.current.totalArea.position);

            if (kernel == -1) { Debug.Log("Kernel not found"); return; }
            shader.Dispatch(kernel, Mathf.CeilToInt(resolution / threads), 1, 1);

            DisposeBuffer(ref itemIndexBuffer);
            DisposeBuffer(ref itemColorBuffer);
        }

        public void RunItemPositionCompute(ComputeBuffer itemMapBuffer, int outputId)
        {
            int kernel = calcObjectPositionKernel;

            int resolution = TC_Area2D.current.intResolution.x * TC_Area2D.current.intResolution.y;

            List<TC_SelectItem> items;
            if (outputId == TC.treeOutput) items = TC_Area2D.current.terrainLayer.treeSelectItems;
            else items = TC_Area2D.current.terrainLayer.objectSelectItems;

            float[] indices = new float[items.Count];
            if (outputId == TC.treeOutput) for (int i = 0; i < items.Count; i++) indices[i] = items[i].tree.randomPosition / 2;
            else for (int i = 0; i < items.Count; i++) indices[i] = items[i].spawnObject.randomPosition / 2;

            ComputeBuffer resultBuffer = new ComputeBuffer(items.Count, 4);
            resultBuffer.SetData(indices);

            shader.SetBuffer(kernel, "resultBuffer", resultBuffer);
            if (TC_Area2D.current.currentTCTerrain.texHeight != null)
            {
                shader.SetTexture(kernel, "terrainTexRead", TC_Area2D.current.currentTCTerrain.texHeight);
                shader.SetFloat("terrainTexReadResolution", TC_Area2D.current.currentTCTerrain.texHeight.width);
                shader.SetFloat("terrainTexReadNormalResolution", TC_Area2D.current.currentTCTerrain.texHeight.width - TC_Area2D.current.resExpandBorder * 2);
                shader.SetFloat("resExpandBorder", TC_Area2D.current.resExpandBorder);

                // Debug.Log("T " + TC_Area2D.current.currentTCTerrain.texHeight.width);
                // Debug.Log(TC_Area2D.current.currentTCTerrain.texHeight.width - TC_Area2D.current.resExpandBorder * 2);

            }

            shader.SetBuffer(kernel, "itemMapBuffer", itemMapBuffer);
            shader.SetInt("resolutionX", TC_Area2D.current.intResolution.x);
            shader.SetInt("resolutionY", TC_Area2D.current.intResolution.y);

            // Debug.Log("ResolutionX " + TC_Area2D.current.intResolution.x);
            
            shader.SetVector("posOffset", Vector3.zero);// target.position);
            shader.SetVector("texResolution", new Vector2(TC_Settings.instance.global.defaultTerrainSize.x, TC_Settings.instance.global.defaultTerrainSize.z));

            shader.SetVector("resConversion", TC_Area2D.current.resolutionPM);
            shader.SetVector("resToPreview", TC_Area2D.current.resToPreview);
            shader.SetVector("areaPos", TC_Area2D.current.area.position);
            shader.SetVector("totalAreaPos", TC_Area2D.current.totalArea.position);
            shader.SetVector("snapOffset", TC_Area2D.current.snapOffsetUV);
            shader.SetFloat("terrainHeight", TC_Area2D.current.terrainSize.y);

            // Debug.Log("areaPos " + TC_Area2D.current.area.position);
            // Debug.Log("totalAreaPos " + TC_Area2D.current.totalArea.position);
            // Debug.Log("resConversion" + TC_Area2D.current.resolutionPM);

            // Debug.Log("pos" + TC_Area2D.current.startPos);
            // Debug.Log("scale " + TC_Area2D.current.terrainSize);

            if (kernel == -1) { Debug.Log("Kernel not found"); return; }
            shader.Dispatch(kernel, Mathf.CeilToInt(resolution / threads), 1, 1);

            DisposeBuffer(ref resultBuffer);
        }

        public void RunComputeCopyRenderTexture(RenderTexture rtSource, RenderTexture rtDest)
        {
            int kernel = copyRenderTextureKernel;
            Int2 resolution = new Int2(rtSource.width, rtSource.height);

            shader.SetTexture(kernel, "splatmap1", rtDest); 
            shader.SetTexture(kernel, "rightSplatmap1", rtSource);

            if (kernel == -1) { Debug.Log("Kernel not found"); return; }
            shader.Dispatch(kernel, resolution.x / 8, resolution.y / 8, 1);
        }

        public void RunItemComputeMask(TC_ItemBehaviour item, ref RenderTexture rtPreview, RenderTexture rtColorPreview, ref ComputeBuffer itemMapBuffer, ref ComputeBuffer maskBuffer)
        {
            int kernel = item.level == 0 ? methodItemTex0MaskKernel : methodItemTexMaskKernel;
            //a Debug.Log("Layer mask kernel " + kernel);

            int resolution = TC_Area2D.current.intResolution.x * TC_Area2D.current.intResolution.y;
            shader.SetInt("resolutionX", TC_Area2D.current.intResolution.x);
            shader.SetInt("resolutionY", TC_Area2D.current.intResolution.y);
            shader.SetVector("colLayer", TC_Settings.instance.global.GetVisualizeColor(item.listIndex));

            shader.SetTexture(kernel, "leftPreviewTex", rtColorPreview);
            shader.SetTexture(kernel, "splatPreviewTex", rtPreview);
            shader.SetBuffer(kernel, "itemMapBuffer", itemMapBuffer);
            shader.SetBuffer(kernel, "maskBuffer", maskBuffer);

            if (kernel == -1) { Debug.Log("Kernel not found RunItemComputeMask"); return; }
            shader.Dispatch(kernel, Mathf.CeilToInt(resolution / threads), 1, 1);

            DisposeBuffer(ref maskBuffer);
        }

        public ComputeBuffer RunNodeCompute(TC_GroupBehaviour groupItem, TC_Node node, float seedParent, ComputeBuffer rightBuffer = null, bool disposeRightBuffer = false)//, int method, ComputeBuffer leftBuffer, bool last)
        {
            TC_Settings localSettings = TC_Settings.instance;
            // TC_GlobalSettings globalSettings = localSettings.global;

            node.ct.CopySpecial(node);

            Vector2 resolution = TC_Area2D.current.resolution;

            InitCurves(node);
            if (groupItem != null) groupItem.localCurve.ConvertCurve();

            // Reporter.Log("bufferLength " + bufferLength);
            // Reporter.Log("Kernel " + kernel);

            int kernel = 0;

            // Reporter.Log("previewRes " + item.previewTex.width);
            // shader.SetInt("method", method);
            if (node.useConstant) kernel = shapeConstantKernel;
            else if (node.inputKind == InputKind.Terrain)
            {
                // shader.SetInt("totalResolutionX", TC_Area2D.current.currentTerrainArea.heightTexResolution.x);
                // shader.SetInt("totalResolutionY", TC_Area2D.current.currentTerrainArea.heightTexResolution.y);

                // Debug.Log(new Vector2((float)TC_Area2D.current.currentTCUnityTerrain.tileX / (float)TC_Area2D.current.currentTerrainArea.tiles.x, (float)TC_Area2D.current.currentTCUnityTerrain.tileZ / (float)TC_Area2D.current.currentTerrainArea.tiles.y));
                if (node.inputTerrain == InputTerrain.Height)
                {
                    kernel = terrainHeightKernel; 
                }
                else if (node.inputTerrain == InputTerrain.Angle)
                {
                    kernel = terrainAngleKernel;
                }
                else if (node.inputTerrain == InputTerrain.Convexity)
                {
                    kernel = terrainConvexityKernel;
                    float resolutionLevel = Mathf.Log(TC_Area2D.current.currentTCTerrain.texHeight.width, 2) - 6;
                    int mipmapLevel = Mathf.Clamp(node.mipmapLevel + (int)resolutionLevel, 1, 8);
                    shader.SetInt("itemCount", mipmapLevel);
                    shader.SetFloat("overlay", node.convexityMode == ConvexityMode.Convex ? node.convexityStrength : -node.convexityStrength);// / resolutionLevel);
                    // Debug.Log(Mathf.Log(TC_Area2D.current.currentTCTerrain.texHeight.width, 2));
                }
                else if (node.inputTerrain == InputTerrain.Splatmap)
                {
                    // if (node.outputId == TC.treeOutput || node.outputId == TC.objectOutput) kernel = terrainSplatmapItemMapKernel; else
                    Texture[] textures = TC_Area2D.current.currentTerrain.terrainData.alphamapTextures;

                    if (node.splatSelectIndex < 4)
                    {
                        kernel = terrainSplatmap0Kernel;
                        shader.SetTexture(kernel, "leftSplatmap0", textures[0]);
                        shader.SetInt("splatIndex", node.splatSelectIndex);
                    }
                    else
                    {
                        kernel = terrainSplatmap1Kernel;
                        shader.SetTexture(kernel, "leftSplatmap1", textures[1]);
                        shader.SetInt("splatIndex", node.splatSelectIndex - 4);
                    }
                    // RenderTexture[] textures = TC_Area2D.current.currentTerrainArea.rtSplatmaps;
                }
                else if (node.inputTerrain == InputTerrain.Collision) 
                {
                    if (camCapture.collisionMask != node.collisionMask || !camCapture.terrain != TC_Area2D.current.currentTerrain) camCapture.Capture(node.collisionMask, node.collisionDirection, node.outputId);

                    if (node.collisionMode == CollisionMode.Height)
                    {
                        if (node.includeTerrainHeight && node.heightDetectRange) kernel = terrainCollisionHeightIncludeKernel; else kernel = terrainCollisionHeightKernel;
                    }
                    else kernel = terrainCollisionMaskKernel;
                    shader.SetTexture(kernel, "tex1", camCapture.cam.targetTexture);
                    if (rightBuffer != null) shader.SetBuffer(kernel, "rightBuffer", rightBuffer);
                    if (node.heightDetectRange) shader.SetVector("range", node.range / TC_Area2D.current.terrainSize.y); else shader.SetVector("range", new Vector2(0, TC_Area2D.current.terrainSize.y));

                }

                if (node.inputTerrain != InputTerrain.Splatmap && TC_Area2D.current.currentTCTerrain.texHeight != null)
                {
                    shader.SetTexture(kernel, "terrainTexRead", TC_Area2D.current.currentTCTerrain.texHeight);
                    shader.SetFloat("terrainTexReadResolution", TC_Area2D.current.currentTCTerrain.texHeight.width);
                    shader.SetFloat("terrainTexReadNormalResolution", TC_Area2D.current.currentTCTerrain.texHeight.width - TC_Area2D.current.resExpandBorder * 2);
                    shader.SetFloat("resExpandBorder", TC_Area2D.current.resExpandBorder);

                    // Debug.Log("terrainTexRead " + TC_Area2D.current.currentTCUnityTerrain.terrain.name);
                }
            }
            else if (node.inputKind == InputKind.Noise || (node.inputKind == InputKind.Current && node.inputCurrent == InputCurrent.Distortion))
            {
                if (node.inputNoise == InputNoise.Perlin)
                {
                    if (node.noise.mode == NoiseMode.TextureLookup) kernel = noisePerlinKernel[node.noise.octaves - 1];
                    else kernel = noisePerlin2Kernel[((int)node.noise.mode) - 1];
                }
                else if (node.inputNoise == InputNoise.Billow)
                {
                    if (node.noise.mode == NoiseMode.TextureLookup) kernel = noiseBillowKernel[node.noise.octaves - 1];
                    else kernel = noiseBillow2Kernel[((int)node.noise.mode) - 1];
                }
                else if (node.inputNoise == InputNoise.Ridged) 
                {
                    if (node.noise.mode == NoiseMode.TextureLookup) kernel = noiserRidgedKernel[node.noise.octaves - 1];
                    else kernel = noiseRidged2Kernel[((int)node.noise.mode) - 1];
                }
                else if (node.inputNoise == InputNoise.IQ) kernel = noiseIQKernel[((int)node.noise.mode) - 1];
                else if (node.inputNoise == InputNoise.Swiss) kernel = noiseSwissKernel[((int)node.noise.mode) - 1];
                else if (node.inputNoise == InputNoise.Jordan) kernel = noiseJordanKernel[((int)node.noise.mode) - 1];
                else if (node.inputNoise == InputNoise.Cell)
                {
                    if (node.noise.cellMode == CellNoiseMode.Normal) kernel = noiseCellNormalKernel; else kernel = noiseCellFastKernel;

                    shader.SetInt("_CellType", node.noise.cellType);
                    shader.SetInt("_DistanceFunction", node.noise.distanceFunction);
                }
                else if (node.inputNoise == InputNoise.Random) kernel = noiseRandomKernel;

                if (m_perlin.GetPermutationTable2D() == null)
                {
                    m_perlin = new PerlinNoise(0);
                    m_perlin.LoadResourcesFor3DNoise();
                    // Debug.Log("Init perlin textures");
                }

                shader.SetTexture(kernel, "_PermTable2D", m_perlin.GetPermutationTable2D());
                shader.SetTexture(kernel, "_Gradient3D", m_perlin.GetGradient3D());
                shader.SetFloat("_Frequency", node.noise.frequency / 10000);
                shader.SetFloat("_Lacunarity", node.noise.lacunarity);
                shader.SetFloat("_Persistence", node.noise.persistence);
                shader.SetFloat("_Seed", (node.noise.seed + seedParent + TC_Settings.instance.seed));
                // Debug.Log("Seed " + (node.noise.seed + seedParent + TC_Settings.instance.seed));
                
                if (node.noise.mode != NoiseMode.TextureLookup)
                {
                    shader.SetFloat("_Amplitude", node.noise.amplitude);
                    shader.SetInt("_Octaves", node.noise.octaves);
                    shader.SetFloat("_Warp0", node.noise.warp0);
                    shader.SetFloat("_Warp", node.noise.warp);
                    shader.SetFloat("_Damp0", node.noise.damp0);
                    shader.SetFloat("_Damp", node.noise.damp);
                    shader.SetFloat("_DampScale", node.noise.dampScale);
                }
                // Reporter.Log(node.generator.seed);
            }
            else if (node.inputKind == InputKind.Shape)
            {
                if (node.inputShape == InputShape.Gradient) kernel = shapeGradientKernel;
                else if (node.inputShape == InputShape.Circle) kernel = shapeCircleKernel;
                else if (node.inputShape == InputShape.Rectangle)
                {
                    kernel = shapeSquareKernel;
                    shader.SetVector("topResolution", node.shapes.topSize);
                    shader.SetVector("bottomResolution", node.shapes.bottomSize);
                }
                else if (node.inputShape == InputShape.Constant) kernel = shapeConstantKernel;

                shader.SetFloat("shapeSize", node.shapes.size);
            }
            else if (node.inputKind == InputKind.File)
            {
                if (node.inputFile == InputFile.Image)
                {
                    if (node.imageSettings.colSelectMode == ColorSelectMode.Color) kernel = imageColorKernel; else kernel = imageColorRangeKernel;

                    shader.SetTexture(kernel, "leftSplatmap0", node.stampTex);

                    for (int i = 0; i < 4; i++)
                    {
                        ImageSettings.ColChannel colChannel = node.imageSettings.colChannels[i];
                        shader.SetVector(TC.colChannelNamesLowerCase[i] + "Channel", new Vector3(colChannel.active ? 1 : 0, colChannel.range.x / 255.0f, colChannel.range.y / 255.0f));
                    }
                }
                else if (node.inputFile == InputFile.RawImage)
                {
                    if (node.rawImage == null) return null;
                    if (node.rawImage.tex == null)
                    {
                        node.rawImage.LoadRawImage(node.rawImage.path);
                        if (node.rawImage.tex == null) return null;
                    }
                    kernel = rawImageKernel;
                    shader.SetInt("_Octaves", node.mipmapLevel);
                    shader.SetTexture(kernel, "tex1", node.rawImage.tex);
                }
            }
            else if (node.inputKind == InputKind.Portal)
            {
                kernel = copyBufferKernel;
                shader.SetBuffer(kernel, "rightBuffer", TC_Area2D.current.layerGroupBuffer);
            }

            if (node.inputKind == InputKind.Current)
            {
                if (node.inputCurrent == InputCurrent.Blur)
                {
                    if (node.blurMode == BlurMode.Normal) kernel = currentBlurNormalKernel;
                    else if (node.blurMode == BlurMode.Outward) kernel = currentBlurOutwardKernel;
                    else if (node.blurMode == BlurMode.Inward) kernel = currentBlurInwardKernel;
                }
                else if (node.inputCurrent == InputCurrent.Expand) kernel = currentExpandKernel;
                else if (node.inputCurrent == InputCurrent.Shrink) kernel = currentShrinkKernel;
                else if (node.inputCurrent == InputCurrent.EdgeDetect)
                {
                    kernel = currentEdgeDetectKernel;
                    shader.SetVector("range", node.detectRange);
                }
                else if (node.inputCurrent == InputCurrent.Distortion)
                {
                    kernel = currentDistortionKernel;
                    shader.SetFloat("shapeSize", node.radius);
                }
                shader.SetBuffer(kernel, "rightBuffer", rightBuffer);
            }

            shader.SetVector("texResolution", new Vector2(node.size.x, node.size.z));// globalSettings.defaultTerrainSize.ToVector2());
            shader.SetInt("isClamp", node.wrapMode == ImageWrapMode.Clamp ? 1 : 0);
            shader.SetInt("isMirror", node.wrapMode == ImageWrapMode.Mirror ? 1 : 0);
            
            shader.SetInt("preview", localSettings.preview ? 1 : 0);
            InitPreviewRenderTexture(ref node.rtPreview, node.name);
            shader.SetInt("previewResolution", node.rtPreview.width);
            shader.SetTexture(kernel, "previewTex", node.rtPreview);
            InitPreviewRenderTexture(ref groupItem.rtPreview, "Preview");
            shader.SetTexture(kernel, "previewTex2", groupItem.rtPreview);

            bufferLength = (int)resolution.x * (int)resolution.y;
            TC_Reporter.Log("Compute node buffer resolution " + resolution.x + " " + resolution.y);
            ComputeBuffer resultBuffer = new ComputeBuffer((int)bufferLength, 4);
            shader.SetBuffer(kernel, "resultBuffer", resultBuffer);

            shader.SetInt("resolutionX", (int)resolution.x);
            shader.SetInt("resolutionY", (int)resolution.y);
            
            // Debug.Log("resolutionX " + resolution.x);
            // Debug.Log("resolutionY " + resolution.y);
            
            ComputeBuffer localCurveCalc = null, localCurveKeys = null;
            SetComputeCurve("local", kernel, node.localCurve, ref localCurveCalc, ref localCurveKeys);

            ComputeBuffer localGroupCurveCalc = null, localGroupCurveKeys = null;
            if (groupItem != null)
            {
                SetComputeCurve("localGroup", kernel, groupItem.localCurve, ref localGroupCurveCalc, ref localGroupCurveKeys);
            }

            ComputeBuffer worldCurveCalc = null, worldCurveKeys = null;
            SetComputeCurve("world", kernel, node.worldCurve, ref worldCurveCalc, ref worldCurveKeys);

            // Reporter.Log(Area2D.current.currentTerrain.terrainData.size.y);
            // Reporter.Log(item.cT.position + " -- " + item.cT.scale);
             
            if (node.nodeType == NodeGroupType.Mask) shader.SetInt("mask", 1);
            else shader.SetInt("mask", 0);

            shader.SetVector("resConversion", TC_Area2D.current.resolutionPM);
            shader.SetVector("resToPreview", TC_Area2D.current.resToPreview);
            shader.SetVector("terrainSize", new Vector2(TC_Area2D.current.terrainSize.x, TC_Area2D.current.terrainSize.z));
            shader.SetVector("offset", node.ct.position - TC_Area2D.current.startPos);
            shader.SetVector("posOffset", node.ct.posOffset);
            shader.SetVector("areaPos", TC_Area2D.current.area.position);
            shader.SetVector("totalAreaPos", TC_Area2D.current.totalArea.position);
            shader.SetVector("rot", new Vector4(node.ct.rotation.x, node.ct.rotation.y, node.ct.rotation.z, node.ct.rotation.w));

            if (node.outputId == TC.treeOutput || node.outputId == TC.objectOutput) shader.SetVector("uvOffset", new Vector2(0.5f / resolution.x, 0.5f / resolution.y));
            else shader.SetVector("uvOffset", Vector2.zero);

            float scaleY = node.ct.scale.y;
            if (node.inputKind == InputKind.Noise && node.inputNoise == InputNoise.Swiss || node.inputNoise == InputNoise.Jordan)
            {
                if (node.noise.amplitude > 1) scaleY /= node.noise.amplitude;
            }
            shader.SetVector("scale", new Vector3(node.ct.scale.x, scaleY * (node.size.y / 1000), node.ct.scale.z));
            
            // shader.SetFloat("terrainHeight", node.outputId == TC.heightOutput ? Area2D.current.terrainSize.y : 1000);
            shader.SetFloat("terrainHeight", TC_Area2D.current.terrainSize.y);
            // Debug.Log(TC_Area2D.current.terrainSize.y);
            shader.SetInt("outputId", node.outputId);

            // Reporter.Log("Run shader");
            if (kernel == -1) { Debug.Log("Kernel not found"); return null; }
            shader.Dispatch(kernel, Mathf.CeilToInt(bufferLength / threads), 1, 1);

            DisposeBuffers(ref localCurveKeys, ref localCurveCalc);
            DisposeBuffers(ref localGroupCurveKeys, ref localGroupCurveCalc);
            DisposeBuffers(ref worldCurveKeys, ref worldCurveCalc);
            if (disposeRightBuffer) DisposeBuffer(ref rightBuffer);
            
            return resultBuffer;
        }
        
        public void RunComputeMultiMethod(TC_ItemBehaviour item, Method method, bool normalize, ref RenderTexture[] rtsLeft, ref RenderTexture[] rtsRight, ComputeBuffer maskBuffer, RenderTexture rtPreview, ref RenderTexture rtPreviewClone, ref RenderTexture rtLeftPreview, RenderTexture rtRightPreview)
        {
            int kernel = -1;
            int _method = (int)method;

            if (method == Method.Lerp && maskBuffer != null)
            {
                kernel = multiMethodTexLerpMaskKernel;
                TC_Reporter.Log(kernel + " -> Lerp mask");
                shader.SetTexture(kernel, "previewTex2", item.rtPreview);
                shader.SetBuffer(kernel, "maskBuffer", maskBuffer);
            }
            // else if (rtLeftPreview == null) kernel = multiMethodKernel[_method];
            else kernel = multiMethodTexKernel[_method];

            shader.SetFloat("overlay", item.opacity);

            shader.SetInt("doNormalize", normalize ? 1 : 0);

            Int2 resolution = TC_Area2D.current.intResolution;
            shader.SetInt("resolutionX", resolution.x);
            shader.SetInt("resolutionY", resolution.y);

            shader.SetTexture(kernel, "leftSplatmap0", rtsLeft[0]);
            if (rtsLeft.Length > 1) shader.SetTexture(kernel, "leftSplatmap1", rtsLeft[1]);

            shader.SetTexture(kernel, "rightSplatmap0", rtsRight[0]);
            if (rtsRight.Length >1) shader.SetTexture(kernel, "rightSplatmap1", rtsRight[1]);

            shader.SetTexture(kernel, "splatmap0", rtsResult[0]);
            shader.SetTexture(kernel, "splatmap1", rtsResult[1]);

            InitPreviewRenderTexture(ref rtPreviewClone, "Preview");

            shader.SetTexture(kernel, "leftPreviewTex", rtLeftPreview);
            shader.SetTexture(kernel, "rightPreviewTex", rtRightPreview);
            shader.SetTexture(kernel, "splatPreviewTex", rtPreview);
            shader.SetTexture(kernel, "splatPreviewTexClone", rtPreviewClone);

            if (kernel == -1) { Debug.Log("Kernel not found"); return; }
            shader.Dispatch(kernel, resolution.x / 8, resolution.y / 8, 1);
            // for (int i = 0; i < leftRTextures.Length; i++) leftRTextures[i] = resultRTextures[i];

            rtLeftPreview = rtPreviewClone;
            TC.Swap(ref rtsLeft, ref rtsResult);
            // TC.Swap(ref rightPreviewTex, ref previewTex);
            // TC.Swap(ref leftPreviewTex, ref resultTex);

            DisposeRenderTextures(ref rtsRight);
        }

        public void RunComputeMultiMethod(TC_ItemBehaviour item, bool doNormalize, ref RenderTexture[] rtsLeft, ComputeBuffer maskBuffer, RenderTexture rtLeftPreview = null)
        {
            int kernel = -1;

            //if (previewTex == null) kernel = multiMethodMultiplyBufferKernel;
            // else 
            kernel = multiMethodMultiplyBufferKernel;
            shader.SetBuffer(kernel, "rightBuffer", maskBuffer);

            shader.SetInt("doNormalize", doNormalize ? 1 : 0);

            Int2 resolution = TC_Area2D.current.intResolution;
            shader.SetInt("resolutionX", resolution.x);
            shader.SetInt("resolutionY", resolution.y);

            shader.SetTexture(kernel, "leftSplatmap0", rtsLeft[0]);
            shader.SetTexture(kernel, "leftSplatmap1", rtsLeft[1]);

            shader.SetTexture(kernel, "splatmap0", rtsResult[0]);
            shader.SetTexture(kernel, "splatmap1", rtsResult[1]);

            TC_Layer layer = item as TC_Layer;
            if (layer != null) shader.SetTexture(kernel, "leftPreviewTex", layer.selectNodeGroup.rtColorPreview);
            else
            {
                TC_LayerGroup layerGroup = item as TC_LayerGroup;
                if (layerGroup != null) shader.SetTexture(kernel, "leftPreviewTex", rtLeftPreview);
            }
            shader.SetTexture(kernel, "splatPreviewTex", item.rtPreview);

            TC_Reporter.Log("maskbuffer " + resolution.x + " , " + resolution.y);

            if (kernel == -1) { Debug.Log("Kernel not found"); return; }
            shader.Dispatch(kernel, resolution.x / 8, resolution.y / 8, 1);

            // for (int i = 0; i < leftRTextures.Length; i++) leftRTextures[i] = resultRTextures[i];
            TC.Swap(ref rtsLeft, ref rtsResult);
        }

        public void RunComputeColorMethod(TC_ItemBehaviour item, Method method, ref RenderTexture rtLeft, ref RenderTexture rtRight, ComputeBuffer maskBuffer, RenderTexture rtPreview, ref RenderTexture rtPreviewClone, ref RenderTexture rtLeftPreview, RenderTexture rtRightPreview)
        {
            int kernel = -1;
            int _method = (int)method;

            if (method == Method.Lerp && maskBuffer != null)
            {
                kernel = colorMethodTexLerpMaskKernel;
                TC_Reporter.Log(kernel + " -> Lerp mask");
                shader.SetTexture(kernel, "previewTex2", item.rtPreview);
                shader.SetBuffer(kernel, "maskBuffer", maskBuffer);
            }
            // else if (leftPreviewTex == null) kernel = colorMethodTexKernel[_method];
            else kernel = colorMethodTexKernel[_method];

            shader.SetFloat("overlay", item.opacity);

            Int2 resolution = TC_Area2D.current.intResolution;
            shader.SetInt("resolutionX", resolution.x);
            shader.SetInt("resolutionY", resolution.y);

            shader.SetTexture(kernel, "leftSplatmap0", rtLeft);
            shader.SetTexture(kernel, "rightSplatmap0", rtRight);

            // Debug.Log(leftRTexture.width + " - " + resultRTextures[0].width);
            // Debug.Log(resolution.x + " - " + resolution.y);
            shader.SetTexture(kernel, "splatmap0", rtResult);
            // shader.SetTexture(kernel, "splatmap0", leftRTexture);

            InitPreviewRenderTexture(ref rtPreviewClone, "Preview");

            shader.SetTexture(kernel, "leftPreviewTex", rtLeftPreview);
            shader.SetTexture(kernel, "rightPreviewTex", rtRightPreview);
            shader.SetTexture(kernel, "splatPreviewTex", rtPreview);
            shader.SetTexture(kernel, "splatPreviewTexClone", rtPreviewClone);

            // Debug.Log("kernel " + kernel);
            if (kernel == -1) { Debug.Log("Kernel not found"); return; }
            shader.Dispatch(kernel, resolution.x / 8, resolution.y / 8, 1);

            TC.Swap(ref rtLeft, ref rtResult);
            rtLeftPreview = rtPreviewClone;
            // TC.Swap(ref rightPreviewTex, ref previewTex);
            // TC.Swap(ref leftPreviewTex, ref resultTex);

            // DisposeRenderTexture(ref rightRTexture);
        }

        public void RunComputeColorMethod(TC_ItemBehaviour item, ref RenderTexture rtLeft, ComputeBuffer maskBuffer, RenderTexture rtLeftPreview = null)
        {
            int kernel = -1;

            kernel = colorMethodMultiplyBufferKernel;
            shader.SetBuffer(kernel, "rightBuffer", maskBuffer);

            Int2 resolution = TC_Area2D.current.intResolution;
            shader.SetInt("resolutionX", resolution.x);
            shader.SetInt("resolutionY", resolution.y);

            shader.SetTexture(kernel, "leftSplatmap0", rtLeft);
            shader.SetTexture(kernel, "splatmap0", rtResult);
            // shader.SetTexture(kernel, "splatmap0", leftRTexture);

            TC_Layer layer = item as TC_Layer;
            if (layer != null) shader.SetTexture(kernel, "leftPreviewTex", layer.selectNodeGroup.rtColorPreview);
            else
            {
                TC_LayerGroup layerGroup = item as TC_LayerGroup;
                if (layerGroup != null) shader.SetTexture(kernel, "leftPreviewTex", rtLeftPreview);
            }
            shader.SetTexture(kernel, "splatPreviewTex", item.rtPreview);

            TC_Reporter.Log("maskbuffer " + resolution.x + " , " + resolution.y);

            if (kernel == -1) { Debug.Log("Kernel not found"); return; }
            shader.Dispatch(kernel, resolution.x / 8, resolution.y / 8, 1);

            // leftRTexture = resultRTex; 

            TC.Swap(ref rtLeft, ref rtResult);
        }

        public void RunSplatNormalize(TC_LayerGroup layerGroup, ref RenderTexture[] rtsLeft, ref RenderTexture rtPreview)
        {
            if (!TC_Settings.instance.preview && !layerGroup.active) return;

            int kernel = normalizeSplatKernel;

            shader.SetTexture(kernel, "leftSplatmap0", rtsLeft[0]);
            shader.SetTexture(kernel, "leftSplatmap1", rtsLeft[1]);
            shader.SetTexture(kernel, "splatmap0", rtsResult[0]);
            shader.SetTexture(kernel, "splatmap1", rtsResult[1]);

            if (rtPreview != null)
            {
                shader.SetTexture(kernel, "leftPreviewTex", rtPreview);
                shader.SetTexture(kernel, "splatPreviewTex", rtSplatPreview);
            }

            // for (int i = 0; i < leftRTextures.Length; i++) leftRTextures[i] = resultRTextures[i];
            // previewTex = splatPreviewTex;
            TC.Swap(ref rtsLeft, ref rtsResult);
            TC.Swap(ref rtPreview, ref rtSplatPreview);
        }


        public void RunComputeMethod(TC_GroupBehaviour groupItem, TC_ItemBehaviour item, ComputeBuffer resultBuffer, ref ComputeBuffer rightBuffer, int itemCount, RenderTexture rtPreview, ComputeBuffer maskBuffer = null)
        {
            if (!TC_Settings.instance.preview && !item.active) return;

            int kernel = -1;
            int method;

            if (groupItem != null)
            {
                method = (int)item.method;
                // InitCurves(groupItem);
            }
            else
            {
                method = 3;
                shader.SetInt("localCurveKeysLength", 0);
                shader.SetInt("worldCurveKeysLength", 0);
            }

            // if (previewTex == null) Debug.Log("PreviewTex = null");

            if (rtPreview != null && TC_Settings.instance.preview)
            {
                if (maskBuffer == null) { kernel = methodTexKernel[method]; }
                else
                {
                    kernel = methodTexLerpMaskKernel;
                    if (item.rtPreview != null) shader.SetTexture(kernel, "previewTex2", item.rtPreview);
                    shader.SetBuffer(kernel, "maskBuffer", maskBuffer);
                }
                shader.SetTexture(kernel, "previewTex", rtPreview);
            }
            else
            {
                if (maskBuffer == null) kernel = methodKernel[method];
                else
                {
                    kernel = methodLerpMaskKernel;
                    shader.SetBuffer(kernel, "maskBuffer", maskBuffer);
                }
            }

            if (item != null) shader.SetFloat("overlay", item.opacity);

            shader.SetInt("itemCount", itemCount);
            shader.SetBuffer(kernel, "rightBuffer", rightBuffer);
            shader.SetBuffer(kernel, "resultBuffer", resultBuffer);

            if (kernel == -1) { Debug.Log("Kernel not found"); return; }

            if (groupItem != null)
            {
                ComputeBuffer worldCurveCalc = null, worldCurveKeys = null;
                groupItem.worldCurve.ConvertCurve();
                SetComputeCurve("world", kernel, groupItem.worldCurve, ref worldCurveCalc, ref worldCurveKeys);
                
                shader.Dispatch(kernel, Mathf.CeilToInt(bufferLength / threads), 1, 1);

                DisposeBuffers(ref worldCurveKeys, ref worldCurveCalc);
            }
            else
            {
                shader.Dispatch(kernel, Mathf.CeilToInt(bufferLength / threads), 1, 1);
            }

            DisposeBuffer(ref rightBuffer);
        } 
         
        public void RunComputeObjectMethod(TC_GroupBehaviour groupItem, TC_ItemBehaviour item, ComputeBuffer itemMapBuffer, ref ComputeBuffer rightItemMapBuffer, ComputeBuffer maskBuffer, RenderTexture rtPreview, ref RenderTexture rtPreviewClone, ref RenderTexture rtLeftPreview, RenderTexture rtRightPreview)
        {
            TC_Settings settings = TC_Settings.instance;
            
            // if (rtPreview == null) Debug.Log("null"); else Debug.Log("Not null");
            int kernel = -1;

            if (item.method == Method.Max) kernel = methodItemTexMaxKernel;
            else if (item.method == Method.Min) kernel = methodItemTexMinKernel;
            else if (item.method == Method.Lerp)
            {
                if (maskBuffer == null)
                {
                    kernel = methodItemTexLerpKernel;
                    // Debug.Log("method overlay " + kernel);
                }
                else
                {
                    kernel = methodItemTexLerpMaskKernel;
                    shader.SetBuffer(kernel, "maskBuffer", maskBuffer);
                    // Debug.Log("methodItemTexLerpMaskKernel " + kernel);
                }
            }
            // Debug.Log("ObjectMethod " + kernel);

            if (rtPreview == null) TC_Reporter.Log("rtPreview = null");

            if (rtPreview != null && settings.preview)
            {
                InitPreviewRenderTexture(ref rtPreviewClone, "rtPreviewClone_"+TC.outputNames[groupItem.outputId]);

                if (maskBuffer != null)
                {
                    shader.SetTexture(kernel, "previewTex2", item.rtPreview);
                    shader.SetBuffer(kernel, "maskBuffer", maskBuffer);
                    shader.SetVector("colLayer", settings.global.GetVisualizeColor(item.listIndex));
                }

                if (groupItem.level != 0)
                {
                    InitPreviewRenderTexture(ref groupItem.parentItem.rtPreview, "rtPreview LayerGroup" + TC.outputNames[groupItem.outputId]);
                    shader.SetTexture(kernel, "splatmap0", groupItem.parentItem.rtPreview);
                    shader.SetVector("colLayer2", settings.global.GetVisualizeColor(groupItem.parentItem.listIndex));
                } 
                
                if (rtLeftPreview != null) shader.SetTexture(kernel, "leftPreviewTex", rtLeftPreview);
                shader.SetTexture(kernel, "rightPreviewTex", rtRightPreview);
                shader.SetTexture(kernel, "splatPreviewTex", rtPreview);
                shader.SetTexture(kernel, "splatPreviewTexClone", rtPreviewClone);
            }
            else
            {
                if (maskBuffer != null) shader.SetBuffer(kernel, "maskBuffer", maskBuffer);
            }

            if (item != null) shader.SetFloat("overlay", item.opacity);
            
            shader.SetBuffer(kernel, "itemMapBuffer", itemMapBuffer);
            shader.SetBuffer(kernel, "rightItemMapBuffer", rightItemMapBuffer);
            shader.SetVector("resConversion", TC_Area2D.current.resolutionPM);

            Int2 resolution = TC_Area2D.current.intResolution;
            shader.SetInt("resolutionX", resolution.x);
            shader.SetInt("resolutionY", resolution.y);
            shader.SetVector("areaPos", TC_Area2D.current.area.position);
            shader.SetVector("totalAreaPos", TC_Area2D.current.totalArea.position);
            shader.SetVector("resToPreview", TC_Area2D.current.resToPreview);
            
            // Debug.Log("areaPos " + TC_Area2D.current.area.position);
            // Debug.Log("totalAreaPos " + TC_Area2D.current.totalArea.position);
            // Debug.Log("resConversion " + TC_Area2D.current.resToTerrain);
            // Debug.Log("resToPreview " + TC_Area2D.current.resToPreview);

            //a Debug.Log("resConversion " + TC_Area2D.current.resToTerrain);

            if (groupItem != null)
            {
                // ComputeBuffer worldCurveCalc = null, worldCurveKeys = null;
                // groupItem.worldCurve.ConvertCurve();
                // SetComputeCurve("world", kernel, groupItem.worldCurve, ref worldCurveCalc, ref worldCurveKeys);

                if (kernel == -1) { Debug.Log("Kernel not found RunComputeObjectMethod"); return; }
                // Debug.Log(bufferLength + " / " + threads);
                shader.Dispatch(kernel, Mathf.CeilToInt(bufferLength / threads), 1, 1);

                // DisposeBuffers(ref worldCurveKeys, ref worldCurveCalc);
            }

            rtLeftPreview = rtPreviewClone;

            DisposeBuffer(ref rightItemMapBuffer);
        }

        public void RunTerrainTexFromTerrainData(TerrainData terrainData, ref RenderTexture rtHeight)
        {
            Debug.Log("Run terrain tex from TerrainData "+terrainData.name);
            int heightmapResolution = terrainData.heightmapResolution - 1;

            float[,] heights2D = terrainData.GetHeights(0, 0, heightmapResolution, heightmapResolution);

            float[] heights = new float[heightmapResolution * heightmapResolution];

            for (int y = 0; y < heightmapResolution; y++)
            {
                for (int x = 0; x < heightmapResolution; x++)
                {
                    heights[x + (y * heightmapResolution)] = heights2D[y, x];
                }
            }

            ComputeBuffer resultBuffer = new ComputeBuffer(heights.Length, 4);
            resultBuffer.SetData(heights);

            RunTerrainTex(resultBuffer, ref rtHeight, heightmapResolution);

            DisposeBuffer(ref resultBuffer);
        }

        public void RunTerrainTex(ComputeBuffer resultBuffer, ref RenderTexture rtHeight, int resolution, bool useRTP = false)
        {
            TC_Reporter.Log("Run terrain tex");

            // Reporter.Log(Area2D.current.currentTCTerrain.terrain.name);

            // Debug.Log("rb " + resultBuffer.count);
            // Debug.Log("con" + Area2D.current.resToTerrain);
            // Debug.Log("res" + resolution);
            // Debug.Log("Area resolution "+ TC_Area2D.current.resolution.ToString());

            InitRenderTexture(ref rtResult, "rtResult", resolution, RenderTextureFormat.RHalf);
            InitRenderTexture(ref rtHeight, "rtHeight "+TC_Area2D.current.currentTCUnityTerrain.terrain.name, new Int2(resolution, resolution), RenderTextureFormat.ARGB32, false, true);

            shader.SetBuffer(resultBufferToTexKernel, "resultBuffer", resultBuffer);
            shader.SetTexture(resultBufferToTexKernel, "resultTex", rtResult);
            TC_Reporter.Log("result Kernel " + resultBufferToTexKernel);

            float t = Time.realtimeSinceStartup;
            if (resultBufferToTexKernel == -1) { Debug.Log("Kernel not found"); return; }
            TC_Reporter.Log("Area resolution " + TC_Area2D.current.resolution);

            int kernel = resultBufferToTexKernel; 

            if (kernel == -1) { Debug.Log("Kernel not found"); return; }
            shader.Dispatch(kernel, Mathf.CeilToInt(resolution / 8.0f), Mathf.CeilToInt(resolution / 8.0f), 1);

            float f = 1 / (Time.realtimeSinceStartup - t);
            TC_Reporter.Log("Frames compute " + f);

            kernel = terrainTexKernel;
            
            // shader.SetBuffer(terrainTexKernel, "resultBuffer", resultBuffer);

            shader.SetTexture(terrainTexKernel, "terrainTex", rtHeight);
            shader.SetTexture(terrainTexKernel, "resultTexRead", rtResult);

            // --resolution; 
            Vector3 size = TC_Area2D.current.currentTCUnityTerrain.terrain.terrainData.size;
            Vector2 resolutionPM = new Vector2(size.x / (resolution), size.z / (resolution));
            
            shader.SetVector("resConversion", resolutionPM);
            shader.SetInt("resolutionX", resolution);
            shader.SetInt("resolutionY", resolution);

            // Debug.Log("resolution " + resolution);

            // shader.SetInts("idOffset", new int[] { TC_Area2D.current.currentTCUnityTerrain.tileX * resolution, TC_Area2D.current.currentTCUnityTerrain.tileZ * resolution });
            
            // Reporter.Log("terrainTex " + Mathf.FloorToInt(Area2D.current.intResolution.x / 8.0f) * 8 + ", "+resolution.y);
            if (kernel == -1) { Debug.Log("Kernel not found"); return; }
            shader.Dispatch(kernel, Mathf.CeilToInt(resolution / 8.0f), Mathf.CeilToInt(resolution / 8.0f), 1);
        }

        void SetComputeCurve(string name, int kernel, Curve curve, ref ComputeBuffer curveCalc, ref ComputeBuffer curveKeys)
        {
            if (curve.length > 0)
            {
                curveCalc = new ComputeBuffer(curve.c.Length, 16);
                curveKeys = new ComputeBuffer(curve.curveKeys.Length, 4);

                curveCalc.SetData(curve.c);
                curveKeys.SetData(curve.curveKeys);

                shader.SetBuffer(kernel, name + "CurveKeys", curveKeys);
                shader.SetBuffer(kernel, name + "CurveCalc", curveCalc);
            }
            shader.SetInt(name + "CurveKeysLength", curve.length);
            shader.SetVector(name + "CurveRange", new Vector3(curve.range.x, curve.range.y, curve.range.y - curve.range.x));
        }

        static public void InitTextures(ref Texture2D[] textures, string name, int length = 1)
        {
            TC_Reporter.Log("InitTextures", 1);

            if (textures == null) textures = new Texture2D[length];
            else if (textures.Length != 2)
            {
                // TODO: copy old texture array to new array
                DisposeTextures(ref textures);
                textures = new Texture2D[length];
            }

            for (int i = 0; i < textures.Length; i++)
            {
                if (textures[i] != null)
                {
                    TC_Reporter.Log(textures[i].name + " is assigned");
                    if (textures[i].width == TC_Area2D.current.intResolution.x && textures[i].height == TC_Area2D.current.intResolution.y) continue;
                    else
                    {
                        textures[i].Resize(TC_Area2D.current.intResolution.x, TC_Area2D.current.intResolution.y);
                        continue;
                    }
                }
                textures[i] = new Texture2D(TC_Area2D.current.intResolution.x, TC_Area2D.current.intResolution.y, TextureFormat.ARGB32, false, true);
                textures[i].hideFlags = HideFlags.DontSave;
                textures[i].name = name;
            }
        }

        static public void InitTexture(ref Texture2D tex, string name, int resolution = -1, bool mipmap = false) 
        {
            TC_Reporter.Log("InitTextures", 1);
            Int2 intResolution;

            if (resolution == -1) intResolution = TC_Area2D.current.intResolution;
            else intResolution = new Int2(resolution, resolution);

            if (tex != null)
            {
                TC_Reporter.Log(tex.name + " is assigned");
                if (!(tex.mipmapCount == 1 && mipmap))
                {
                    if (tex.width == intResolution.x && tex.height == intResolution.y) return;
                    else
                    {
                        tex.Resize(intResolution.x, intResolution.y);
                        return;
                    }
                }
            }
            TC_Reporter.Log("Create new Texture2D " + name);
            tex = new Texture2D(intResolution.x, intResolution.y, TextureFormat.ARGB32, mipmap, true);
            tex.hideFlags = HideFlags.DontSave;
            tex.name = name;
        }

        static public void InitPreviewRenderTexture(ref RenderTexture rt, string name)
        {
            TC_Reporter.Log("InitPreviewRenderTextures", 1);

            if (TC_Area2D.current == null) return;
            int resolution = TC_Area2D.current.previewResolution;

            if (rt != null)
            {
                if (!rt.IsCreated()) Debug.Log("RenderTexture not Created!");
                if (rt.width != resolution)
                {
                    // Debug.Log("Release RenderTexture "+rt.width+" res "+resolution+" "+name);
                    TC_Reporter.Log("release " + rt.width + " " + resolution);
                    DisposeRenderTexture(ref rt);
                }
            }

            if (rt == null)
            {
                //a Debug.Log("Create RenderTexture "+name);
                rt = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                rt.name = name;
                rt.enableRandomWrite = true; 
                rt.hideFlags = HideFlags.DontSave; 
                rt.Create();
                
                //if (GlobalManager.singleton.saveRenderTextures)
                //{
                //    UnityEditor.AssetDatabase.CreateAsset(renderTexture, "Assets/Power of Nature/TerrainComposer2/RenderTextures/" + renderTexture.GetInstanceID() + ".renderTexture");
                //}
            }
        }

        static public void InitRenderTextures(ref RenderTexture[] rts, string name, int length = 2)
        {
            TC_Reporter.Log("InitRenderTextures");

            if (rts == null) rts = new RenderTexture[length];
            else if (rts.Length != length)
            {
                // TODO: Copy old rts to new array
                DisposeRenderTextures(ref rts);
                rts = new RenderTexture[length];
            }                

            for (int i = 0; i < rts.Length; i++)
            {
                if (rts[i] != null)
                {
                    if (!rts[i].IsCreated()) Debug.Log("RenderTexture not Created!");
                    TC_Reporter.Log(rts[i].name + " is assigned");
                    if (rts[i].width == TC_Area2D.current.intResolution.x && rts[i].height == TC_Area2D.current.intResolution.y) continue;
                    else
                    {
                        // Debug.Log("release ");
                        DisposeRenderTexture(ref rts[i]);
                    }
                }
                // Debug.Log("Create RenderTexture");
                rts[i] = new RenderTexture(TC_Area2D.current.intResolution.x, TC_Area2D.current.intResolution.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                rts[i].enableRandomWrite = true;
                rts[i].name = name;
                rts[i].hideFlags = HideFlags.DontSave;
                rts[i].Create();
            }
        }

        static public void InitRenderTexture(ref RenderTexture rt, string name)
        {
            TC_Reporter.Log("InitRenderTextures");

            if (rt != null)
            {
                if (!rt.IsCreated()) Debug.Log("RenderTexture not Created!");
                TC_Reporter.Log(rt.name + " is assigned");
                if (rt.width == TC_Area2D.current.intResolution.x && rt.height == TC_Area2D.current.intResolution.y) return;
                else
                {
                    TC_Reporter.Log("release " + name + " from " + rt.width + " x " + rt.height +" to "+TC_Area2D.current.intResolution.x +" x " + TC_Area2D.current.intResolution.y);
                    DisposeRenderTexture(ref rt);
                    rt = null;
                }
            }
            // Debug.Log("Create RenderTexture "+name);
            rt = new RenderTexture(TC_Area2D.current.intResolution.x, TC_Area2D.current.intResolution.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            rt.enableRandomWrite = true;
            rt.name = name;
            rt.hideFlags = HideFlags.DontSave;
            rt.Create();
        }

        static public void InitRenderTexture(ref RenderTexture rt, string name, int resolution, RenderTextureFormat format = RenderTextureFormat.ARGB32, bool forceCreate = false)
        {
            TC_Reporter.Log("InitRenderTextures");

            bool create = forceCreate;

            if (rt == null) create = true;
            else
            {
                if (!rt.IsCreated()) Debug.Log("RenderTexture not Created!");
                if (rt.width != resolution)
                {
                    // Debug.Log("Release RenderTexture");
                    TC_Reporter.Log("release " + rt.width + " " + resolution);
                    DisposeRenderTexture(ref rt);
                    create = true;
                }
                else return;
            }

            if (create)
            {
                // Debug.Log("Create RenderTexture");
                rt = new RenderTexture(resolution, resolution, 0, format, RenderTextureReadWrite.Linear);
                rt.name = name;
                rt.hideFlags = HideFlags.DontSave;
                rt.enableRandomWrite = true;
                rt.Create();
            }
        }

        static public void InitRenderTexture(ref RenderTexture rt, string name, Int2 resolution, RenderTextureFormat format = RenderTextureFormat.ARGB32, bool forceCreate = false, bool useMipmap = false)
        {
            TC_Reporter.Log("InitRenderTextures", 1);

            bool create = forceCreate;

            if (rt == null) create = true;
            else
            {
                if (!rt.IsCreated()) Debug.Log("RenderTexture not Created!");
                if (rt.width != resolution.x || rt.height != resolution.y || rt.useMipMap != useMipmap)
                {
                    // Debug.Log("Release RenderTexture");
                    TC_Reporter.Log("release " + rt.width + " " + resolution.x);
                    DisposeRenderTexture(ref rt);
                    create = true;
                }
            }

            if (create)
            {
                // Debug.Log("Create RenderTexture");
                rt = new RenderTexture(resolution.x, resolution.y, 0, format, RenderTextureReadWrite.Linear);
                rt.name = name;
                rt.useMipMap = useMipmap;
                rt.generateMips = useMipmap;
                rt.hideFlags = HideFlags.DontSave;
                rt.enableRandomWrite = true;
                rt.Create();
            }
        }

        public void DisposeBuffer(ref ComputeBuffer buffer, bool warningEmpty = false)
        {
            if (buffer == null)
            {
                if (warningEmpty) TC_Reporter.Log("Dispose buffer is empty");
                return;
            }

            buffer.Dispose(); buffer = null;
        }

        public void DisposeBuffers(ref ComputeBuffer buffer1, ref ComputeBuffer buffer2)
        {
            if (buffer1 != null) { buffer1.Dispose(); buffer1 = null; }
            if (buffer2 != null) { buffer2.Dispose(); buffer2 = null; }
        }

        static public void DisposeRenderTexture(ref RenderTexture rt)
        {
            if (rt == null) return;

            TC_Reporter.Log("DisposeRenderTextures", 1);

            // Debug.Log("Dispose RenderTexture " + renderTexture.name);
            rt.Release();
            #if UNITY_EDITOR
                DestroyImmediate(rt);
            #else
                rt = null;
            #endif
        }

        static public void DisposeRenderTextures(ref RenderTexture[] rts)
        {
            if (rts == null) return;

            TC_Reporter.Log("DisposeRenderTextures");

            for (int i = 0; i < rts.Length; i++)
            {
                if (rts[i] == null) continue;
                // Debug.Log("Dispose RenderTexture " + renderTextures[i].name);
                rts[i].Release();
                #if UNITY_EDITOR
                    DestroyImmediate(rts[i]);
                #else
                    Destroy(rts[i]); 
                #endif
                rts[i] = null;
            }
        }

        static public void DisposeTexture(ref Texture2D tex)
        {
            if (tex == null) return;

            #if UNITY_EDITOR
                DestroyImmediate(tex);
            #else
                Destroy(tex);
            #endif
        }

        static public void DisposeTextures(ref Texture2D[] textures)
        {
            for (int i = 0; i < textures.Length; i++) DisposeTexture(ref textures[i]);
        }

        public void InitBytesArray(int length)
        {
            TC_Reporter.Log("InitByteArray");

            bool create = false;

            if (bytesArray == null) create = true;
            else
            {
                if (bytesArray.Length != length) { create = true; }
                else
                {
                    for (int i = 0; i < bytesArray.Length; i++)
                    {
                        if (bytesArray[i] == null) { create = true; break; }
                    }
                }
            }
            
            if (create)
            {
                bytesArray = new BytesArray[length];
                for (int i = 0; i < length; i++) bytesArray[i] = new BytesArray();
            }
        }

        public class BytesArray
        {
            public byte[] bytes;
        }


        //float Evaluate(float t)
        //{
        //    if (t > curve.keys[c.Length].time) t = curve.keys[c.Length].time;
        //    if (t < curve.keys[0].time) t = curve.keys[0].time;
        //    // Reporter.Log(t);

        //    int i  = 0;
        //    for (int j = 1; j < c.Length + 1; j++)
        //    {
        //        if (curve.keys[j].time >= t) { i = j - 1; break; }
        //    }
        //    Reporter.Log(i);
        //    return c[i].x * t * t * t + c[i].y * t * t + c[i].z * t + c[i].w;
        //}
    }
}
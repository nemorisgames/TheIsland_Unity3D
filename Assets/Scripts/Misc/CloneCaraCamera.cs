using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
[RequireComponent(typeof(AmplifyOcclusionEffect))]
[RequireComponent(typeof(ScionEngine.ScionPostProcess))]
public class CloneCaraCamera : MonoBehaviour
{
	[SerializeField]
	Color color;
	void Awake()
	{
		AmplifyOcclusionEffect occlusion = gameObject.GetComponent<AmplifyOcclusionEffect>();
		if (occlusion == null)
		{
			gameObject.AddComponent<AmplifyOcclusionEffect>();
			occlusion = gameObject.GetComponent<AmplifyOcclusionEffect>();
		}
		occlusion.ApplyMethod = AmplifyOcclusionBase.ApplicationMethod.PostEffect;
		occlusion.SampleCount = AmplifyOcclusionBase.SampleCountLevel.Medium;
		occlusion.PerPixelNormals = AmplifyOcclusionBase.PerPixelNormalSource.Camera;
		occlusion.Intensity = 1;
		occlusion.Radius = 1;
		occlusion.Tint = color;
		occlusion.PowerExponent = 1.8f;
		occlusion.Bias = 0.05f;
		occlusion.CacheAware = false;
		occlusion.Downsample = false;
		occlusion.BlurEnabled = true;
		occlusion.BlurPasses = 1;
		occlusion.BlurRadius = 2;
		occlusion.BlurSharpness = 10;
		ScionEngine.ScionPostProcess postProcess = gameObject.GetComponent<ScionEngine.ScionPostProcess>();
		if (postProcess == null)
		{
			gameObject.AddComponent<ScionEngine.ScionPostProcess>();
			postProcess = gameObject.GetComponent<ScionEngine.ScionPostProcess>();
		}
		postProcess.chromaticAberration = true;
		postProcess.chromaticAberrationDistortion = 0.7f;
		postProcess.chromaticAberrationIntensity = 10;

		postProcess.bloom = true;
		postProcess.bloomThreshold = 0;
		postProcess.bloomIntensity = 0.35f;
		postProcess.bloomDownsamples = 7;
		postProcess.bloomDistanceMultiplier = 1;

		postProcess.grain = true;
		postProcess.grainIntensity = 0.1f;
		postProcess.vignette = true;
		postProcess.vignetteIntensity = 0.7f;
		postProcess.vignetteScale = 0.7f;
		postProcess.vignetteColor = color;

		postProcess.lensFlare = true;
		postProcess.lensFlareGhostSamples = ScionEngine.LensFlareGhostSamples.x3;
		postProcess.lensFlareGhostIntensity = 0.1f;
		postProcess.lensFlareGhostDispersal = 0.2f;
		postProcess.lensFlareGhostDistortion = 0.1f;
		postProcess.lensFlareHaloIntensity = 0.1f;
		postProcess.lensFlareHaloWidth = 0.3f;
		postProcess.lensFlareHaloDistortion = 0.5f;
		postProcess.lensFlareBlurSamples = ScionEngine.LensFlareBlurSamples.x4;
		postProcess.lensFlareBlurStrength = 0.5f;
		postProcess.lensFlareDownsamples = 2;
		postProcess.lensFlareThreshold = 0;
		postProcess.lensDirt = false;

		postProcess.tonemappingMode = ScionEngine.TonemappingMode.Filmic;
		postProcess.whitePoint = 7;

		postProcess.cameraMode = ScionEngine.CameraMode.AutoPriority;
		postProcess.userControlledFocalLength = false;

		postProcess.exposureCompensation = 0;
		postProcess.minMaxExposure = new Vector2(-16, 16);
		postProcess.adaptionSpeed = 1;

		postProcess.depthOfField = true;
		postProcess.exclusionMask = LayerMask.GetMask("Nothing");
		postProcess.depthOfFieldSamples = ScionEngine.DepthOfFieldSamples.Normal_25;
		postProcess.depthOfFieldTemporalSupersampling = true;
		postProcess.depthOfFieldTemporalBlend = 0.9f;
		postProcess.depthOfFieldTemporalSteps = 8;
		postProcess.visualizePointFocus = false;
		postProcess.pointAverageRange = 0.1f;
		postProcess.adaptionSpeed = 15;
		postProcess.visualizeFocalDistance = false;

		postProcess.colorGradingMode = ScionEngine.ColorGradingMode.Off;
	}
}

using System;
using System.Collections.Generic;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    // The settings here are per frame settings.
    // Each camera must have its own per frame settings
    [Serializable]
    public class FrameSettings
    {
        // Lighting
        // Setup by users
        public bool enableShadow = true;
        public bool enableContactShadows = true;
        public bool enableSSR = true; // Depends on DepthPyramid
        public bool enableSSAO = true;
        public bool enableSubsurfaceScattering = true;
        public bool enableTransmission = true;  // Caution: this is only for debug, it doesn't save the cost of Transmission execution
        public bool enableAtmosphericScattering = true;
        public bool enableVolumetrics = true;

        // Setup by system
        public float diffuseGlobalDimmer = 1.0f;
        public float specularGlobalDimmer = 1.0f;

        // View
        public bool enableForwardRenderingOnly = false; // TODO: Currently there is no way to strip the extra forward shaders generated by the shaders compiler, so we can switch dynamically.
        public bool enableDepthPrepassWithDeferredRendering = false;

        public bool enableTransparentPrepass = true;
        public bool enableMotionVectors = true; // Enable/disable whole motion vectors pass (Camera + Object).
        public bool enableObjectMotionVectors = true;
        public bool enableDBuffer = true;
        public bool enableRoughRefraction = true; // Depends on DepthPyramid - If not enable, just do a copy of the scene color (?) - how to disable rough refraction ?
        public bool enableTransparentPostpass = true;
        public bool enableDistortion = true;
        public bool enablePostprocess = true;

        public bool enableStereo = true;
        public bool enableAsyncCompute = true;

        public bool enableOpaqueObjects = true;
        public bool enableTransparentObjects = true;

        public bool enableMSAA = false;
        public MSAASamples msaaSampleCount { get; private set; }

        public bool enableShadowMask = true;

        public LightLoopSettings lightLoopSettings = new LightLoopSettings();

        public void CopyTo(FrameSettings frameSettings)
        {
            frameSettings.enableShadow = this.enableShadow;
            frameSettings.enableContactShadows = this.enableContactShadows;
            frameSettings.enableSSR = this.enableSSR;
            frameSettings.enableSSAO = this.enableSSAO;
            frameSettings.enableSubsurfaceScattering = this.enableSubsurfaceScattering;
            frameSettings.enableTransmission = this.enableTransmission;
            frameSettings.enableAtmosphericScattering = this.enableAtmosphericScattering;
            frameSettings.enableVolumetrics = this.enableVolumetrics;

            frameSettings.diffuseGlobalDimmer = this.diffuseGlobalDimmer;
            frameSettings.specularGlobalDimmer = this.specularGlobalDimmer;

            frameSettings.enableForwardRenderingOnly = this.enableForwardRenderingOnly;
            frameSettings.enableDepthPrepassWithDeferredRendering = this.enableDepthPrepassWithDeferredRendering;

            frameSettings.enableTransparentPrepass = this.enableTransparentPrepass;
            frameSettings.enableMotionVectors = this.enableMotionVectors;
            frameSettings.enableObjectMotionVectors = this.enableObjectMotionVectors;
            frameSettings.enableDBuffer = this.enableDBuffer;
            frameSettings.enableRoughRefraction = this.enableRoughRefraction;
            frameSettings.enableTransparentPostpass = this.enableTransparentPostpass;
            frameSettings.enableDistortion = this.enableDistortion;
            frameSettings.enablePostprocess = this.enablePostprocess;

            frameSettings.enableStereo = this.enableStereo;

            frameSettings.enableOpaqueObjects = this.enableOpaqueObjects;
            frameSettings.enableTransparentObjects = this.enableTransparentObjects;

            frameSettings.enableAsyncCompute = this.enableAsyncCompute;

            frameSettings.enableMSAA = this.enableMSAA;

            frameSettings.enableShadowMask = this.enableShadowMask;

            this.lightLoopSettings.CopyTo(frameSettings.lightLoopSettings);
        }

        // Init a FrameSettings from renderpipeline settings, frame settings and debug settings (if any)
        // This will aggregate the various option
        public static void InitializeFrameSettings(Camera camera, RenderPipelineSettings renderPipelineSettings, FrameSettings srcFrameSettings, ref FrameSettings aggregate)
        {
            if (aggregate == null)
                aggregate = new FrameSettings();

            // When rendering reflection probe we disable specular as it is view dependent
            if (camera.cameraType == CameraType.Reflection)
            {
                aggregate.diffuseGlobalDimmer = 1.0f;
                aggregate.specularGlobalDimmer = 0.0f;
            }
            else
            {
                aggregate.diffuseGlobalDimmer = 1.0f;
                aggregate.specularGlobalDimmer = 1.0f;
            }

            aggregate.enableShadow = srcFrameSettings.enableShadow;
            aggregate.enableContactShadows = srcFrameSettings.enableContactShadows;
            aggregate.enableSSR = camera.cameraType != CameraType.Reflection && srcFrameSettings.enableSSR && renderPipelineSettings.supportSSR;
            aggregate.enableSSAO = srcFrameSettings.enableSSAO && renderPipelineSettings.supportSSAO;
            aggregate.enableSubsurfaceScattering = camera.cameraType != CameraType.Reflection && srcFrameSettings.enableSubsurfaceScattering && renderPipelineSettings.supportSubsurfaceScattering;
            aggregate.enableTransmission = srcFrameSettings.enableTransmission;
            aggregate.enableAtmosphericScattering = srcFrameSettings.enableAtmosphericScattering;
            // We must take care of the scene view fog flags in the editor
            if (!CoreUtils.IsSceneViewFogEnabled(camera))
                aggregate.enableAtmosphericScattering = false;
            // Volumetric are disabled if there is no atmospheric scattering
            aggregate.enableVolumetrics = srcFrameSettings.enableVolumetrics && renderPipelineSettings.supportVolumetrics && aggregate.enableAtmosphericScattering;

            // TODO: Add support of volumetric in planar reflection
            if (camera.cameraType == CameraType.Reflection)
                aggregate.enableVolumetrics = false;

            // We have to fall back to forward-only rendering when scene view is using wireframe rendering mode
            // as rendering everything in wireframe + deferred do not play well together
            aggregate.enableForwardRenderingOnly = srcFrameSettings.enableForwardRenderingOnly || GL.wireframe || renderPipelineSettings.supportOnlyForward;
            aggregate.enableDepthPrepassWithDeferredRendering = srcFrameSettings.enableDepthPrepassWithDeferredRendering;

            aggregate.enableTransparentPrepass = srcFrameSettings.enableTransparentPrepass;
            aggregate.enableMotionVectors = camera.cameraType != CameraType.Reflection && srcFrameSettings.enableMotionVectors && renderPipelineSettings.supportMotionVectors;
            aggregate.enableObjectMotionVectors = camera.cameraType != CameraType.Reflection && srcFrameSettings.enableObjectMotionVectors && renderPipelineSettings.supportMotionVectors;
            aggregate.enableDBuffer = srcFrameSettings.enableDBuffer && renderPipelineSettings.supportDBuffer;
            aggregate.enableRoughRefraction = srcFrameSettings.enableRoughRefraction;
            aggregate.enableTransparentPostpass = srcFrameSettings.enableTransparentPostpass;
            aggregate.enableDistortion = camera.cameraType != CameraType.Reflection && srcFrameSettings.enableDistortion;

            // Planar and real time cubemap doesn't need post process and render in FP16
            aggregate.enablePostprocess = camera.cameraType != CameraType.Reflection && srcFrameSettings.enablePostprocess;
            
            aggregate.enableStereo = (camera.cameraType != CameraType.Reflection) && (camera.cameraType != CameraType.SceneView) && XRGraphicsConfig.enabled && (camera.stereoTargetEye == StereoTargetEyeMask.Both);
            
            aggregate.enableAsyncCompute = srcFrameSettings.enableAsyncCompute && SystemInfo.supportsAsyncCompute;

            aggregate.enableOpaqueObjects = srcFrameSettings.enableOpaqueObjects;
            aggregate.enableTransparentObjects = srcFrameSettings.enableTransparentObjects;

            aggregate.enableMSAA = srcFrameSettings.enableMSAA && renderPipelineSettings.supportMSAA;

            aggregate.enableShadowMask = srcFrameSettings.enableShadowMask && renderPipelineSettings.supportShadowMask;

            aggregate.ConfigureMSAADependentSettings();
            aggregate.ConfigureStereoDependentSettings();

            // Disable various option for the preview except if we are a Camera Editor preview
            if (HDUtils.IsRegularPreviewCamera(camera))
            {
                aggregate.enableShadow = false;
                aggregate.enableContactShadows = false;
                aggregate.enableSSR = false;
                aggregate.enableSSAO = false;
                aggregate.enableAtmosphericScattering = false;
                aggregate.enableVolumetrics = false;
                aggregate.enableTransparentPrepass = false;
                aggregate.enableMotionVectors = false;
                aggregate.enableObjectMotionVectors = false;
                aggregate.enableDBuffer = false;
                aggregate.enableTransparentPostpass = false;
                aggregate.enableDistortion = false;
                aggregate.enablePostprocess = false;
                aggregate.enableStereo = false;
                aggregate.enableShadowMask = false;
            }

            LightLoopSettings.InitializeLightLoopSettings(camera, aggregate, renderPipelineSettings, srcFrameSettings, ref aggregate.lightLoopSettings);
        }

        public void ConfigureMSAADependentSettings()
        {
            if (enableMSAA)
            {
                // Initially, MSAA will only support forward
                enableForwardRenderingOnly = true;

                // TODO: Should we disable enableFptlForForwardOpaque in here, instead of in InitializeLightLoopSettings?
                // We'd have to move this method to after InitializeLightLoopSettings if we did.  It would be nice to centralize
                // all MSAA-dependent settings in this method.

                // Assuming MSAA is being used, TAA, and therefore, motion vectors are not needed
                enableMotionVectors = false;

                // TODO: The work will be implemented piecemeal to support all passes
                enableDBuffer = false; // no decals
                enableDistortion = false; // no gaussian final color
                enablePostprocess = false;
                enableRoughRefraction = false; // no gaussian pre-refraction
                enableSSAO = false;
                enableSSR = false;
                enableSubsurfaceScattering = false;
                enableTransparentObjects = false; // waiting on depth pyramid generation
            }
        }

        public void ConfigureStereoDependentSettings()
        {
            if (enableStereo)
            {
                // Force forward if we request stereo. TODO: We should not enforce that, users should be able to chose deferred
                //enableForwardRenderingOnly = true;

                // TODO: The work will be implemented piecemeal to support all passes
                //enableMotionVectors = false;
                //enableDBuffer = false;
                //enableDistortion = false;
                //enablePostprocess = false;
                //enableRoughRefraction = false;
                //enableSSAO = false;
                //enableSSR = false;
                //enableSubsurfaceScattering = false;
                //enableTransparentObjects = false;
            }
        }

        public static void RegisterDebug(string menuName, FrameSettings frameSettings)
        {
            List<DebugUI.Widget> widgets = new List<DebugUI.Widget>();
            widgets.AddRange(
            new DebugUI.Widget[]
            {
                new DebugUI.Foldout
                {
                    displayName = "Rendering Passes",
                    children =
                    {
                        new DebugUI.BoolField { displayName = "Enable Transparent Prepass", getter = () => frameSettings.enableTransparentPrepass, setter = value => frameSettings.enableTransparentPrepass = value },
                        new DebugUI.BoolField { displayName = "Enable Transparent Postpass", getter = () => frameSettings.enableTransparentPostpass, setter = value => frameSettings.enableTransparentPostpass = value },
                        new DebugUI.BoolField { displayName = "Enable Motion Vectors", getter = () => frameSettings.enableMotionVectors, setter = value => frameSettings.enableMotionVectors = value },
                        new DebugUI.BoolField { displayName = "Enable Object Motion Vectors", getter = () => frameSettings.enableObjectMotionVectors, setter = value => frameSettings.enableObjectMotionVectors = value },
                        new DebugUI.BoolField { displayName = "Enable DBuffer", getter = () => frameSettings.enableDBuffer, setter = value => frameSettings.enableDBuffer = value },
                        new DebugUI.BoolField { displayName = "Enable Rough Refraction", getter = () => frameSettings.enableRoughRefraction, setter = value => frameSettings.enableRoughRefraction = value },
                        new DebugUI.BoolField { displayName = "Enable Distortion", getter = () => frameSettings.enableDistortion, setter = value => frameSettings.enableDistortion = value },
                        new DebugUI.BoolField { displayName = "Enable Postprocess", getter = () => frameSettings.enablePostprocess, setter = value => frameSettings.enablePostprocess = value },
                    }
                },
                new DebugUI.Foldout
                {
                    displayName = "Rendering Settings",
                    children =
                    {
                        new DebugUI.BoolField { displayName = "Forward Only", getter = () => frameSettings.enableForwardRenderingOnly, setter = value => frameSettings.enableForwardRenderingOnly = value },
                        new DebugUI.BoolField { displayName = "Deferred Depth Prepass", getter = () => frameSettings.enableDepthPrepassWithDeferredRendering, setter = value => frameSettings.enableDepthPrepassWithDeferredRendering = value },
                        new DebugUI.BoolField { displayName = "Enable Async Compute", getter = () => frameSettings.enableAsyncCompute, setter = value => frameSettings.enableAsyncCompute = value },
                        new DebugUI.BoolField { displayName = "Enable Opaque Objects", getter = () => frameSettings.enableOpaqueObjects, setter = value => frameSettings.enableOpaqueObjects = value },
                        new DebugUI.BoolField { displayName = "Enable Transparent Objects", getter = () => frameSettings.enableTransparentObjects, setter = value => frameSettings.enableTransparentObjects = value },
                        new DebugUI.BoolField { displayName = "Enable MSAA", getter = () => frameSettings.enableMSAA, setter = value => frameSettings.enableMSAA = value },
                    }
                },
                new DebugUI.Foldout
                {
                    displayName = "XR Settings",
                    children =
                    {
                        new DebugUI.BoolField { displayName = "Enable Stereo Rendering", getter = () => frameSettings.enableStereo, setter = value => frameSettings.enableStereo = value }
                    }
                },
                new DebugUI.Foldout
                {
                    displayName = "Lighting Settings",
                    children =
                    {
                        new DebugUI.BoolField { displayName = "Enable SSR", getter = () => frameSettings.enableSSR, setter = value => frameSettings.enableSSR = value },
                        new DebugUI.BoolField { displayName = "Enable SSAO", getter = () => frameSettings.enableSSAO, setter = value => frameSettings.enableSSAO = value },
                        new DebugUI.BoolField { displayName = "Enable SubsurfaceScattering", getter = () => frameSettings.enableSubsurfaceScattering, setter = value => frameSettings.enableSubsurfaceScattering = value },
                        new DebugUI.BoolField { displayName = "Enable Transmission", getter = () => frameSettings.enableTransmission, setter = value => frameSettings.enableTransmission = value },
                        new DebugUI.BoolField { displayName = "Enable Shadows", getter = () => frameSettings.enableShadow, setter = value => frameSettings.enableShadow = value },
                        new DebugUI.BoolField { displayName = "Enable Contact Shadows", getter = () => frameSettings.enableContactShadows, setter = value => frameSettings.enableContactShadows = value },
                        new DebugUI.BoolField { displayName = "Enable ShadowMask", getter = () => frameSettings.enableShadowMask, setter = value => frameSettings.enableShadowMask = value },
                        new DebugUI.BoolField { displayName = "Enable Atmospheric Scattering", getter = () => frameSettings.enableAtmosphericScattering, setter = value => frameSettings.enableAtmosphericScattering = value },
                        new DebugUI.BoolField { displayName = "Enable volumetrics", getter = () => frameSettings.enableVolumetrics, setter = value => frameSettings.enableVolumetrics = value },
                    }
                }
            });

            LightLoopSettings.RegisterDebug(frameSettings.lightLoopSettings, widgets);

            var panel = DebugManager.instance.GetPanel(menuName, true);
            panel.children.Add(widgets.ToArray());
        }

        public static void UnRegisterDebug(string menuName)
        {
            DebugManager.instance.RemovePanel(menuName);
        }
    }
}

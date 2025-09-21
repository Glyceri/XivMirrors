using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using System.Runtime.InteropServices;

namespace MirrorsEdge.XIVMirrors.Rendering;

[StructLayout(LayoutKind.Explicit)]
internal unsafe struct MyRenderTargetManager
{
    /// <summary>
    /// Normal Map
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x20)] public Texture* NormalMap;

    /// <summary>
    /// Roughness Map
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x28)] public Texture* RoughnessMap;

    /// <summary>
    /// Diffuse Map
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x30)] public Texture* DiffuseMap;

    /// <summary>
    /// The animated cutout buffer for animated objects
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x38)] public Texture* AnimatedObjectsCutout;

    /// <summary>
    /// Bump Map
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x40)] public Texture* BumpMap;

    /// <summary>
    /// The Dynamic light buffer
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x48)] public Texture* DynamicLightBuffer;

    /// <summary>
    /// The static light buffer
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x50)] public Texture* StaticLightBuffer;

    /// <summary>
    /// The Specular Map
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x58)] public Texture* GlareMap;

    /// <summary>
    /// The Bloom Map.
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x60)] public Texture* BloomMap;

    /// <summary>
    /// Back Buffer No UI
    /// </summary>
    [FieldOffset(0x68)] public Texture* BackBufferNoUI;

    /// <summary>
    /// The depth buffer without any transparent elements buffered.
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x70)] public Texture* DepthBufferNoTransparency;

    /// <summary>
    /// Unk78
    /// </summary>
    [FieldOffset(0x78)]  public Texture* Unk78;

    /// <summary>
    /// Depth Buffer No Transparency
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x80)] public Texture* DepthBufferNoTransparencyCopy;

    /// <summary>
    /// Depth Buffer No Transparency Copy 2
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x88)] public Texture* DepthBufferNoTransparencyCopy2;

    /// <summary>
    /// Back buffer with transparency
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x90)] public Texture* DepthBufferTransparency;

    /// <summary>
    /// Transparent normal map
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x98)] public Texture* TransparentNormalMap;

    /// <summary>
    /// Transparent Normal Map 2
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0xA0)] public Texture* TransparentNormalMap2;

    /// <summary>
    /// Transparent Diffuse Map
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0xA8)] public Texture* TransparentDiffuseMap;

    /// <summary>
    /// UnkB0
    /// </summary>
    [FieldOffset(0xB0)] public Texture* UnkB0;

    /// <summary>
    /// Transparent Bump Map
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0xB8)] public Texture* TransparentBumpMap;

    /// <summary>
    /// Transparent Light Map
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0xC0)] public Texture* TransparentLightMap;

    /// <summary>
    /// Transparent Shadow Map
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0xC8)] public Texture* TransparentShadowMap;

    /// <summary>
    /// Transparent Diffuse Light Map
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0xD0)] public Texture* TransparentDiffuseLightMap;

    /// <summary>
    /// Transparent Specular Light Map
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0xD8)] public Texture* TransparentSpecularDiffuseLightMap;

    /// <summary>
    /// UnkE0
    /// </summary>
    [FieldOffset(0xE0)] public Texture* UnkE0;

    /// <summary>
    /// Depth Buffer Transparency Copy
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0xE8)] public Texture* DepthBufferTransparencyCopy;

    /// <summary>
    /// UnkF0
    /// </summary>
    [FieldOffset(0xF0)] public Texture* UnkF0;

    /// <summary>
    /// UnkF8
    /// </summary>
    [FieldOffset(0xF8)] public Texture* UnkF8;

    /// <summary>
    /// Back Buffer No UI Copy
    /// </summary>
    [FieldOffset(0x100)] public Texture* BackBufferNoUICopy;

    /// <summary>
    /// Back Buffer No UI Copy
    /// </summary>
    [FieldOffset(0x108)] public Texture* BackBufferNoUICopy2;

    /// <summary>
    /// Screen Space Coordinates
    /// </summary>
    [FieldOffset(0x110)] public Texture* ScreenSpaceCoordinates;

    /// <summary>
    /// Screen Space Coordinates with animated objects cutout
    /// </summary>
    [FieldOffset(0x118)] public Texture* ScreenSpaceCoordinatesCutout;

    /// <summary>
    /// Unk120
    /// </summary>
    [FieldOffset(0x120)] public Texture* Unk120;

    /// <summary>
    /// The world space depth buffer
    /// </summary>
    [FieldOffset(0x128)] public Texture* WorldSpaceDepthBuffer;

    /// <summary>
    /// Unk130
    /// </summary>
    [FieldOffset(0x130)] public Texture* Unk130;

    /// <summary>
    /// Character Lighting Depth Buffer
    /// </summary>
    [FieldOffset(0x138)] public Texture* CharacterLightingDepthBuffer;

    /// <summary>
    /// Top Down Depth Stencil camera anchored 
    /// </summary>
    [FieldOffset(0x140)] public Texture* TopDownDepthStencil;

    /// <summary>
    /// Yellow
    /// </summary>
    [FieldOffset(0x148)] public Texture* Yellow;

    /// <summary>
    /// Unk150
    /// </summary>
    [FieldOffset(0x150)] public Texture* Unk150;

    /// <summary>
    /// Unk158
    /// </summary>
    [FieldOffset(0x158)] public Texture* Unk158;

    /// <summary>
    /// Shadow Map 1
    /// <para>- 4 x 4 quad</para>
    /// </summary>
    [FieldOffset(0x160)] public Texture* ShadowMap1;

    /// <summary>
    /// Unk Shadow Map 168
    /// </summary>
    [FieldOffset(0x168)] public Texture* UnkShadowMap168;

    /// <summary>
    /// Unk Shadow Map 170
    /// </summary>
    [FieldOffset(0x170)] public Texture* UnkShadowMap170;

    /// <summary>
    /// Unk Shadow Map 178
    /// </summary>
    [FieldOffset(0x178)] public Texture* UnkShadowMap178;

    /// <summary>
    /// Unk Shadow Map 180
    /// </summary>
    [FieldOffset(0x180)] public Texture* UnkShadowMap180;

    /// <summary>
    /// Unk Shadow Map 188
    /// </summary>
    [FieldOffset(0x188)] public Texture* UnkShadowMap188;

    /// <summary>
    /// Unk Shadow Map 190
    /// </summary>
    [FieldOffset(0x190)] public Texture* UnkShadowMap190;

    /// <summary>
    /// Unk Shadow Map 198
    /// </summary>
    [FieldOffset(0x198)] public Texture* UnkShadowMap198;

    /// <summary>
    /// Unk Shadow Map 200
    /// </summary>
    [FieldOffset(0x200)] public Texture* UnkShadowMap200;

    /// <summary>
    /// Unk Shadow Map 208
    /// </summary>
    [FieldOffset(0x208)] public Texture* UnkShadowMap208;

    /// <summary>
    /// Unk Shadow Map 210
    /// </summary>
    [FieldOffset(0x210)] public Texture* UnkShadowMap210;

    /// <summary>
    /// Unk250
    /// </summary>
    [FieldOffset(0x250)] public Texture* Unk250;

    /// <summary>
    /// Back Buffer No UI
    /// </summary>
    [FieldOffset(0x258)] public Texture* BackBufferNoUICopy3;

    /// <summary>
    /// Lightshafts and bloom
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x260)] public Texture* LightshaftsAndBlom;

    /// <summary>
    /// Sun
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x268)] public Texture* Sun;

    /// <summary>
    /// Texture filtering
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x270)] public Texture* TextureFilterMap;

    /// <summary>
    /// Sun Shafts
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x278)] public Texture* SunShafts;

    /// <summary>
    /// Vignetting
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x280)] public Texture* Vignetting;

    /// <summary>
    /// Back Buffer No UI Copy 4
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x288)] public Texture* BackBufferNoUICopy4;

    /// <summary>
    /// Unk290
    /// </summary>
    [FieldOffset(0x290)] public Texture* Unk290;

    /// <summary>
    /// Bloom Map Copy
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x298)] public Texture* BloomMapCopy;

    /// <summary>
    /// Unk330
    /// </summary>
    [FieldOffset(0x330)] public Texture* Unk330;

    /// <summary>
    /// Unk338
    /// </summary>
    [FieldOffset(0x338)] public Texture* Unk338;

    /// <summary>
    /// Unk340
    /// </summary>
    [FieldOffset(0x340)] public Texture* Unk340;

    /// <summary>
    /// Unk348
    /// </summary>
    [FieldOffset(0x348)] public Texture* Unk348;

    /// <summary>
    /// Unk350
    /// </summary>
    [FieldOffset(0x350)] public Texture* Unk350;

    /// <summary>
    /// Screen Space Coordinates Full
    /// </summary>
    [FieldOffset(0x358)] public Texture* ScreenSpaceCoordinatesFull;

    /// <summary>
    /// Chara View Depth Buffer
    /// </summary>
    [FieldOffset(0x360)] public Texture* CharaViewDepthBuffer;

    /// <summary>
    /// Chara View Depth Buffer Copy
    /// </summary>
    [FieldOffset(0x368)] public Texture* CharaViewDepthBufferCopy;

    /// <summary>
    /// Back Buffer
    /// </summary>
    [FieldOffset(0x370)] public Texture* BackBuffer;

    /// <summary>
    /// Skybox
    /// </summary>
    [FieldOffset(0x420)] public Texture* Skybox;

    /// <summary>
    /// Back Buffer No UI Copy 5
    /// </summary>
    [FieldOffset(0x4A8)] public Texture* BackBufferNoUICopy5;

    /// <summary>
    /// Back Buffer No UI Copy 6
    /// </summary>
    [FieldOffset(0x4B0)] public Texture* BackBufferNoUICopy6;

    /// <summary>
    /// Unk4B8
    /// </summary>
    [FieldOffset(0x4B8)] public Texture* Unk4B8;

    /// <summary>
    /// Back Buffer No UI Copy 7
    /// </summary>
    [FieldOffset(0x4C0)] public Texture* BackBufferNoUICopy7;

    /// <summary>
    /// Back Buffer No UI Copy 8
    /// </summary>
    [FieldOffset(0x4C8)] public Texture* BackBufferNoUICopy8;

    /// <summary>
    /// Back Buffer No UI Copy 9
    /// </summary>
    [FieldOffset(0x4D0)] public Texture* BackBufferNoUICopy9;

    /// <summary>
    /// Back Buffer No UI Copy 10
    /// </summary>
    [FieldOffset(0x4D8)] public Texture* BackBufferNoUICopy10;

    /// <summary>
    /// Back Buffer Copy
    /// </summary>
    [FieldOffset(0x4E0)] public Texture* BackBufferCopy;

    /// <summary>
    /// Back Buffer Copy 2
    /// </summary>
    [FieldOffset(0x4E8)] public Texture* BackBufferCopy2;

    /// <summary>
    /// Depth Buffer No Transparency Copy 3
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x528)] public Texture* DepthBufferNoTransparencyCopy3;

    /// <summary>
    /// Depth Buffer No Transparency Copy 4
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x530)] public Texture* DepthBufferNoTransparencyCopy4;

    /// <summary>
    /// Depth Buffer No Transparency Copy 5
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x540)] public Texture* DepthBufferNoTransparencyCopy5;

    /// <summary>
    /// Depth Buffer No Transparency Copy 6
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x548)] public Texture* DepthBufferNoTransparencyCopy6;

    /// <summary>
    /// Depth Buffer No Transparency Copy 7
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x550)] public Texture* DepthBufferNoTransparencyCopy7;

    /// <summary>
    /// Depth Buffer No Transparency Copy 9
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x558)] public Texture* DepthBufferNoTransparencyCopy9;

    /// <summary>
    /// Depth Buffer No Transparency Copy 10
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x560)] public Texture* DepthBufferNoTransparencyCopy10;

    /// <summary>
    /// Depth Buffer No Transparency Copy 11
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x568)] public Texture* DepthBufferNoTransparencyCopy11;

    /// <summary>
    /// Back Buffer Copy 3
    /// </summary>
    [FieldOffset(0x570)] public Texture* BackBufferCopy3;

    /// <summary>
    /// Depth Buffer No Transparency Copy 12
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x578)] public Texture* DepthBufferNoTransparencyCopy12;

    /// <summary>
    /// Unk580
    /// </summary>
    [FieldOffset(0x580)] public Texture* Unk580;

    /// <summary>
    /// Unk588
    /// </summary>
    [FieldOffset(0x588)] public Texture* Unk588;

    /// <summary>
    /// Unk590
    /// </summary>
    [FieldOffset(0x590)] public Texture* Unk590;

    /// <summary>
    /// Unk598
    /// </summary>
    [FieldOffset(0x598)] public Texture* Unk598;

    /// <summary>
    /// Unk5A0
    /// </summary>
    [FieldOffset(0x5A0)] public Texture* Unk5A0;

    /// <summary>
    /// Unk5A8
    /// </summary>
    [FieldOffset(0x5A8)] public Texture* Unk5A8;

    /// <summary>
    /// Unk5B0
    /// </summary>
    [FieldOffset(0x5B0)] public Texture* Unk5B0;

    /// <summary>
    /// Unk5B8
    /// </summary>
    [FieldOffset(0x5B8)] public Texture* Unk5B8;

    /// <summary>
    /// Unk5C0
    /// </summary>
    [FieldOffset(0x5C0)] public Texture* Unk5C0;

    /// <summary>
    /// Unk5C8
    /// </summary>
    [FieldOffset(0x5C8)] public Texture* Unk5C8;

    /// <summary>
    /// Unk5D0
    /// </summary>
    [FieldOffset(0x5D0)] public Texture* Unk5D0;

    /// <summary>
    /// Motion Blur Map
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x5D8)] public Texture* MotionBlurMap;

    /// <summary>
    /// Unk5E0
    /// </summary>
    [FieldOffset(0x5E0)] public Texture* Unk5E0;

    /// <summary>
    /// Yellow Depth Map
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x5E8)] public Texture* YellowDepthMap;

    /// <summary>
    /// Unk5F0
    /// </summary>
    [FieldOffset(0x5F0)] public Texture* Unk5F0;

    /// <summary>
    /// Depth Buffer No Transparency Copy 13
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x5F8)] public Texture* DepthBufferNoTransparencyCopy13;

    /// <summary>
    /// Depth Buffer No Transparency Copy 14
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x600)] public Texture* DepthBufferNoTransparencyCopy14;

    /// <summary>
    /// GTAO Buffer
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x608)] public Texture* GTAOBuffer;


    /// <summary>
    /// GTAO Buffer Copy
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x610)] public Texture* GTAOBufferCopy;

    /// <summary>
    /// GTAO Skybox Cutout
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x618)] public Texture* GTAOSkyboxCutout;

    /// <summary>
    /// GTAO Outline Map
    /// <para>- Follows dynamic resolution</para>
    /// </summary>
    [FieldOffset(0x620)] public Texture* GTAOOutlineMap;

    /// <summary>
    /// Unk628
    /// </summary>
    [FieldOffset(0x628)] public Texture* Unk628;

    /// <summary>
    /// Unk630
    /// </summary>
    [FieldOffset(0x630)] public Texture* Unk630;

    /// <summary>
    /// Unk638
    /// </summary>
    [FieldOffset(0x638)] public Texture* Unk638;

    /// <summary>
    /// Unk640
    /// </summary>
    [FieldOffset(0x640)] public Texture* Unk640;

    /// <summary>
    /// Motion Buffer
    /// </summary>
    [FieldOffset(0x648)] public Texture* MotionBuffer;

    /// <summary>
    /// Unk650
    /// </summary>
    [FieldOffset(0x650)] public Texture* Unk650;

    /// <summary>
    /// Unk658
    /// </summary>
    [FieldOffset(0x658)] public Texture* Unk658;

    /// <summary>
    /// Unk660
    /// </summary>
    [FieldOffset(0x660)] public Texture* Unk660;

    /// <summary>
    /// Unk668
    /// </summary>
    [FieldOffset(0x668)] public Texture* Unk668;

    /// <summary>
    /// Unk670
    /// </summary>
    [FieldOffset(0x670)] public Texture* Unk670;

    /// <summary>
    /// Unk678
    /// </summary>
    [FieldOffset(0x678)] public Texture* Unk678;

    /// <summary>
    /// Unk680
    /// </summary>
    [FieldOffset(0x680)] public Texture* Unk680;

    /// <summary>
    /// Blobs
    /// </summary>
    [FieldOffset(0x688)] public Texture* Blobs;

    /// <summary>
    /// Unk690
    /// </summary>
    [FieldOffset(0x690)] public Texture* Unk690;
}

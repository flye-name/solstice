#ifndef SYNTAX_H
#define SYNTAX_H

// FX is a magic definition provided by the tml-build shader compiler.  Other
// shader compilers should implement this, such as the one in the tml-build
// bootstrap program and in-game asset hot reloaders.
#ifdef FX
#define ATTRIBUTE(type, name, expr) <type name=expr;>

#define _CS_EXPR(expr) ATTRIBUTE(string, csharpExpression, #expr)
#define CS_EXPR(expr) _CS_EXPR(expr)

#define BEGIN_TECHNIQUE(name) technique name {
#define END_TECHNIQUE }

#define BEGIN_PASS(name) pass name {
#define END_PASS }

#define PIXEL_SHADER(expr) PixelShader = expr;
#define VERTEX_SHADER(expr) VertexShader = expr;

// This syntax is kind of ridiculous:
// https://learn.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-sampler
#define SAMPLER(sampler_type) sampler_type
#define SAMPLER_TEXTURE(texture) Texture = <texture>;

#define SAMPLER_VALUE(name, value) name = value;
#else
#define ATTRIBUTE(type, name, expr)

#define CS_EXPR(expr)

#define BEGIN_TECHNIQUE(name)
#define END_TECHNIQUE

#define BEGIN_PASS(name)
#define END_PASS

#define PIXEL_SHADER(expr)
#define VERTEX_SHADER(expr)

#define SAMPLER(sampler_type, texture)
#define SAMPLER_TEXTURE(texture)

#define SAMPLER_VALUE(name, value)
#endif

// https://learn.microsoft.com/en-us/windows/win32/direct3d9/effect-states#sampler-stage-states

// AddressU[16] dword (D3DTEXTUREADDRESS: WRAP, MIRROR, CLAMP, BORDER, MIRRORONCE)
#define ADDRESS_U(value) SAMPLER_VALUE(AddressU, value)
#define ADDRESS_U_IDX(idx, value) SAMPLER_VALUE(AddressU[idx], value)

// AddressV[16] dword (D3DTEXTUREADDRESS: WRAP, MIRROR, CLAMP, BORDER, MIRRORONCE)
#define ADDRESS_V(value) SAMPLER_VALUE(AddressV, value)
#define ADDRESS_V_IDX(idx, value) SAMPLER_VALUE(AddressV[idx], value)

// AddressW[16] dword (D3DTEXTUREADDRESS: WRAP, MIRROR, CLAMP, BORDER, MIRRORONCE)
#define ADDRESS_W(value) SAMPLER_VALUE(AddressW, value)
#define ADDRESS_W_IDX(idx, value) SAMPLER_VALUE(AddressW[idx], value)

// BorderColor[16] D3DCOLOR (D3DTEXTUREFILTERTYPE: NONE, POINT, LINEAR, ANISOTROPIC, PYRAMIDALQUAD, GUASSIANQUAD, CONVOLUTIONMONO)
#define BORDER_COLOR(value) SAMPLER_VALUE(BorderColor, value)
#define BORDER_COLOR_IDX(idx, value) SAMPLER_VALUE(BorderColor[idx], value)

// MagFilter[16] dword (D3DTEXTUREFILTERTYPE: NONE, POINT, LINEAR, ANISOTROPIC, PYRAMIDALQUAD, GUASSIANQUAD, CONVOLUTIONMONO)
#define MAG_FILTER(value) SAMPLER_VALUE(MagFilter, value)
#define MAG_FILTER_IDX(idx, value) SAMPLER_VALUE(MagFilter[idx], value)

// MaxAnisotropy[16] dword
#define MAX_ANISOTROPY(value) SAMPLER_VALUE(MaxAnisotropy, value)
#define MAX_ANISOTROPY_IDX(idx, value) SAMPLER_VALUE(MaxAnisotropy[idx], value)

// MaxMipLevel[16] int
#define MAX_MIP_LEVEL(value) SAMPLER_VALUE(MaxMipLevel, value)
#define MAX_MIP_LEVEL_IDX(idx, value) SAMPLER_VALUE(MaxMipLevel[idx], value)

// MinFilter[16] dword (D3DTEXTUREFILTERTYPE: NONE, POINT, LINEAR, ANISOTROPIC, PYRAMIDALQUAD, GUASSIANQUAD, CONVOLUTIONMONO)
#define MIN_FILTER(value) SAMPLER_VALUE(MinFilter, value)
#define MIN_FILTER_IDX(idx, value) SAMPLER_VALUE(MinFilter[idx], value)

// MipFilter[16] dword (D3DTEXTUREFILTERTYPE: NONE, POINT, LINEAR, ANISOTROPIC, PYRAMIDALQUAD, GUASSIANQUAD, CONVOLUTIONMONO)
#define MIP_FILTER(value) SAMPLER_VALUE(MipFilter, value)
#define MIP_FILTER_IDX(idx, value) SAMPLER_VALUE(MipFilter[idx], value)

// MipMapLoadBias[16] float
#define MIP_MAP_LOAD_BIAS(value) SAMPLER_VALUE(MipMapLoadBias, value)
#define MIP_MAP_LOAD_BIAS_IDX(idx, value) SAMPLER_VALUE(MipMapLoadBias[idx], value)

// SRGBTexture bool
#define SRGB_TEXTURE(value) _SAMPLER_VALUE(SRGBTexture, value)

// Helpers for composing expressions.
#define _CS_VAR(type, name, expr) type name expr;
#define CS_VAR(type, name, expr) _CS_VAR(type, name, CS_EXPR(expr));

#endif
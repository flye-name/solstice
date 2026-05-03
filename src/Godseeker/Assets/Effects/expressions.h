#ifndef _EXPRESSIONS_H_
#define _EXPRESSIONS_H_

#include "syntax.h"

#define _SHADER_MACROS global::RadiantRevival.Common.ShaderMacros

#define TEXTURE_SIZE(name, register) CS_VAR(float2, name, _SHADER_MACROS.TextureSize(register))

#endif // _EXPRESSIONS_H_
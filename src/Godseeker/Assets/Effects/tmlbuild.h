#ifndef TMLBUILD_HLSL
#define TMLBUILD_HLSL

#include "syntax.h"

#define GLOBAL_TIME(name) CS_VAR(float, name, global::Terraria.Main.GlobalTimeWrappedHourly)

#define SCREEN_SIZE_X(name) CS_VAR(float, name, global::Terraria.Main.screenWidth)
#define SCREEN_SIZE_Y(name) CS_VAR(float, name, global::Terraria.Main.screenHeight)
#define SCREEN_SIZE(name) CS_VAR(float2, name, new global::Microsoft.Xna.Framework.Vector2(global::Terraria.Main.screenWidth, global::Terraria.Main.screenHeight))
#define SCREEN_POSITION(name) CS_VAR(float2, name, global::Terraria.Main.screenPosition)

#endif // TMLBUILD_HLSL
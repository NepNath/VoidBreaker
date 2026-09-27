// VoidPortal.hlsl
// Matériau "void" : nébuleuse low-poly (maillage sphérique sans couture)
// projetée comme une skybox secondaire visible au travers du mesh (effet portail).
//
// Portage ligne à ligne de l'aperçu WebGL validé.
// Le hasard utilise un hash entier (PCG) : le motif est identique
// sur tous les GPU et identique à l'aperçu.
//
// Utilisable par :
//   - le shader écrit à la main VoidPortal.shader
//   - un Shader Graph via un node Custom Function (fonction VoidPortal_float)

#ifndef VOID_PORTAL_INCLUDED
#define VOID_PORTAL_INCLUDED

// ---------------------------------------------------------------------------
// Palette (identique à l'aperçu, valeurs sRGB)
// ---------------------------------------------------------------------------
static const float3 VOID_VIOLET = float3(0.55, 0.18, 1.00);
static const float3 VOID_SKY    = float3(0.30, 0.72, 1.00);
static const float3 VOID_GREEN  = float3(0.15, 0.95, 0.55);
static const float3 VOID_RED    = float3(1.00, 0.18, 0.30);

// Réglages internes du maillage (ne pas modifier : calés sur l'aperçu)
#define VOID_JITTER 0.3
#define VOID_DENSITY_SCALE 0.65
#define VOID_SOLVE_ITERATIONS 12

// ---------------------------------------------------------------------------
// Hash entier et bruit
// ---------------------------------------------------------------------------
uint3 Void_Pcg3d(uint3 v)
{
    v = v * 1664525u + 1013904223u;
    v.x += v.y * v.z;
    v.y += v.z * v.x;
    v.z += v.x * v.y;
    v ^= v >> 16u;
    v.x += v.y * v.z;
    v.y += v.z * v.x;
    v.z += v.x * v.y;
    return v;
}

uint3 Void_HashU(int3 c, uint seed)
{
    return Void_Pcg3d(asuint(c) + seed * uint3(0x9E3779B9u, 0x85EBCA6Bu, 0xC2B2AE35u));
}

float Void_Hash1(int3 c, uint seed)
{
    return float(Void_HashU(c, seed).x >> 8u) * (1.0 / 16777216.0);
}

float3 Void_Hash3(int3 c, uint seed)
{
    return float3(Void_HashU(c, seed) >> 8u) * (1.0 / 16777216.0);
}

// Equivalent de smoothstep(edge, 0, x) (bord décroissant)
float Void_FallOff(float edge, float x)
{
    return 1.0 - smoothstep(0.0, edge, x);
}

float Void_Noise(float3 x)
{
    float3 fl = floor(x);
    int3 i = int3(fl);
    float3 f = x - fl;
    f = f * f * (3.0 - 2.0 * f);

    float n000 = Void_Hash1(i, 0u);
    float n100 = Void_Hash1(i + int3(1, 0, 0), 0u);
    float n010 = Void_Hash1(i + int3(0, 1, 0), 0u);
    float n110 = Void_Hash1(i + int3(1, 1, 0), 0u);
    float n001 = Void_Hash1(i + int3(0, 0, 1), 0u);
    float n101 = Void_Hash1(i + int3(1, 0, 1), 0u);
    float n011 = Void_Hash1(i + int3(0, 1, 1), 0u);
    float n111 = Void_Hash1(i + int3(1, 1, 1), 0u);

    return lerp(lerp(lerp(n000, n100, f.x), lerp(n010, n110, f.x), f.y),
                lerp(lerp(n001, n101, f.x), lerp(n011, n111, f.x), f.y), f.z);
}

float Void_Fbm(float3 p)
{
    float v = 0.0;
    float a = 0.5;

    [unroll]
    for (int i = 0; i < 5; i++)
    {
        v += a * Void_Noise(p);
        p = p * 2.03 + float3(1.7, 9.2, 3.1);
        a *= 0.5;
    }

    return v;
}

// ---------------------------------------------------------------------------
// Projection cube : direction -> (face, uv), utilisée uniquement pour les étoiles
// ---------------------------------------------------------------------------
float2 Void_ToFace(float3 d, out float fid)
{
    float3 a = abs(d);

    if (a.x >= a.y && a.x >= a.z)
    {
        fid = d.x > 0.0 ? 0.0 : 1.0;
        return d.yz / a.x;
    }
    else if (a.y >= a.z)
    {
        fid = d.y > 0.0 ? 2.0 : 3.0;
        return d.xz / a.y;
    }

    fid = d.z > 0.0 ? 4.0 : 5.0;
    return d.xy / a.z;
}

// ---------------------------------------------------------------------------
// Maillage de tétraèdres 3D (grille simplex) aux sommets décalés et animés.
// La sphère des directions coupe ce maillage : chaque tétraèdre traversé
// donne une facette plate. Aucune projection cube -> aucune couture.
// ---------------------------------------------------------------------------
float3 Void_Unskew(float3 L)
{
    return L - (L.x + L.y + L.z) / 6.0;
}

void Void_SimplexLocate(float3 x, out float3 L0, out float3 o1, out float3 o2)
{
    L0 = floor(x + (x.x + x.y + x.z) / 3.0);
    float3 x0 = x - Void_Unskew(L0);
    float3 g = step(x0.yzx, x0.xyz);
    float3 l = 1.0 - g;
    o1 = min(g, l.zxy);
    o2 = max(g, l.zxy);
}

float3 Void_LatticeJitter(float3 L, float time, float wobble, float wobbleSpeed)
{
    int3 li = int3(L);
    float3 h = Void_Hash3(li, 11u);
    float3 h2 = Void_Hash3(li, 12u);
    float t = time * wobbleSpeed * (0.7 + 0.6 * h2.x);
    float3 wob = float3(sin(t + h2.y * 6.2832),
                        cos(t * 0.83 + h2.z * 6.2832),
                        sin(t * 1.13 + h2.x * 6.2832)) * wobble;
    return (h - 0.5) * VOID_JITTER + wob;
}

float4 Void_Barycentric(float3 x, float3 a, float3 b, float3 c, float3 d)
{
    float3 e1 = b - a;
    float3 e2 = c - a;
    float3 e3 = d - a;
    float3 v = x - a;
    float det = dot(e1, cross(e2, e3));
    float l1 = dot(v, cross(e2, e3)) / det;
    float l2 = dot(e1, cross(v, e3)) / det;
    float l3 = dot(e1, cross(e2, v)) / det;
    return float4(1.0 - l1 - l2 - l3, l1, l2, l3);
}

// Trouve le tétraèdre (déformé) qui contient p.
// La déformation est inversée par point fixe : x = p - jitter(x).
void Void_TetraCell(float3 p, float time, float wobble, float wobbleSpeed,
                    out float3 cenDir, out int3 tidL, out uint tidCode)
{
    float3 x = p;
    float3 L0;
    float3 o1;
    float3 o2;

    [loop]
    for (int it = 0; it < VOID_SOLVE_ITERATIONS; it++)
    {
        Void_SimplexLocate(x, L0, o1, o2);
        float3 La = L0;
        float3 Lb = L0 + o1;
        float3 Lc = L0 + o2;
        float3 Ld = L0 + 1.0;
        float4 w = Void_Barycentric(x, Void_Unskew(La), Void_Unskew(Lb), Void_Unskew(Lc), Void_Unskew(Ld));
        x = p - (w.x * Void_LatticeJitter(La, time, wobble, wobbleSpeed)
               + w.y * Void_LatticeJitter(Lb, time, wobble, wobbleSpeed)
               + w.z * Void_LatticeJitter(Lc, time, wobble, wobbleSpeed)
               + w.w * Void_LatticeJitter(Ld, time, wobble, wobbleSpeed));
    }

    Void_SimplexLocate(x, L0, o1, o2);
    float3 A = L0;
    float3 B = L0 + o1;
    float3 C = L0 + o2;
    float3 D = L0 + 1.0;
    float3 cen = (Void_Unskew(A) + Void_LatticeJitter(A, time, wobble, wobbleSpeed)
                + Void_Unskew(B) + Void_LatticeJitter(B, time, wobble, wobbleSpeed)
                + Void_Unskew(C) + Void_LatticeJitter(C, time, wobble, wobbleSpeed)
                + Void_Unskew(D) + Void_LatticeJitter(D, time, wobble, wobbleSpeed)) * 0.25;
    cenDir = normalize(cen);
    tidL = int3(L0);
    tidCode = uint(dot(o1, float3(1.0, 2.0, 4.0)) + 8.0 * dot(o2, float3(1.0, 2.0, 4.0)));
}

// ---------------------------------------------------------------------------
// Etoiles en croix
// ---------------------------------------------------------------------------
float3 Void_Sparkle(float2 uv, float fid, float sc, float th, float time)
{
    float2 p = uv * sc;
    float2 fl = floor(p);
    float2 f = p - fl - 0.5;
    int3 k = int3(int2(fl), int(fid) * 128 + int(sc));

    float2 q = f - (Void_Hash3(k, 1u).xy - 0.5) * 0.4;
    float h = Void_Hash1(k, 5u);
    float sz = 0.06 + 0.22 * pow(Void_Hash1(k, 8u), 3.0);

    float core = Void_FallOff(sz * 0.3, length(q));
    float sx = Void_FallOff(0.03, abs(q.y)) * Void_FallOff(sz, abs(q.x));
    float sy = Void_FallOff(0.03, abs(q.x)) * Void_FallOff(sz, abs(q.y));
    float glow = exp(-length(q) * 28.0) * 0.25;
    float tw = 0.6 + 0.4 * sin(time * 2.5 + h * 50.0);

    float3 tint = lerp(float3(1.0, 0.82, 0.55), float3(0.7, 0.8, 1.0), Void_Hash1(k, 2u));
    float3 col = lerp(float3(1.0, 1.0, 1.0), tint, Void_Hash1(k, 3u) * 0.9);

    return col * (max(core, max(sx, sy)) * 1.6 + glow) * step(th, h) * tw;
}

// ---------------------------------------------------------------------------
// Espace complet pour une direction (convention de l'aperçu)
// ---------------------------------------------------------------------------
float3 Void_Space(float3 d, float poly, float transparency, float stars, float wobble, float wobbleSpeed, float time)
{
    float fid;
    float2 uv = Void_ToFace(d, fid);

    float3 cd;
    int3 tl;
    uint tc;
    Void_TetraCell(d * poly * VOID_DENSITY_SCALE, time, wobble, wobbleSpeed, cd, tl, tc);

    // Etoiles : projection cube équi-angulaire (densité uniforme)
    uv = atan(uv) * 1.2732395;

    // Densité des nuages (évaluée au centroïde = teinte plate par facette)
    float2 w = float2(Void_Fbm(cd * 2.0), Void_Fbm(cd * 2.0 + 5.2));
    float m = Void_Fbm(cd * 2.4 + float3(w, w.x) * 1.5);
    float mask = smoothstep(0.38, 0.72, m);

    // Teinte : angle uniforme -> 4 couleurs à 25 % chacune, fondu entre voisines
    float3 hp = cd * 1.8 + float3(w, w.y) * 0.8;
    float ang = atan2(Void_Fbm(hp) - 0.5, Void_Fbm(hp + float3(11.3, 4.1, 7.7)) - 0.5)
              + (Void_Hash1(tl, 100u + tc * 4u) - 0.5) * 0.5;
    float4 hw = max(float4(cos(ang), cos(ang - 1.5708), cos(ang - 3.1416), cos(ang - 4.7124)), 0.0);
    float3 col = (VOID_VIOLET * hw.x + VOID_SKY * hw.y + VOID_GREEN * hw.z + VOID_RED * hw.w)
               / (dot(hw, float4(1, 1, 1, 1)) + 1e-4);

    float lum = dot(col, float3(0.299, 0.587, 0.114));
    col = saturate(lerp(lum.xxx, col, 1.12));

    float bright = 0.45 + 0.55 * Void_Hash1(tl, 101u + tc * 4u);
    float3 c = col * mask * bright * (1.0 - transparency);

    // Fond bleu nuit très sombre entre les nuages
    c += float3(0.02, 0.028, 0.06) * (0.4 + 0.9 * Void_Hash1(tl, 102u + tc * 4u)) * (1.0 - mask);

    // Etoiles
    c += Void_Sparkle(uv, fid, 24.0, 1.0 - 0.10 * stars, time);
    c += Void_Sparkle(uv, fid, 48.0, 1.0 - 0.07 * stars, time) * 0.7;
    c += Void_Sparkle(uv, fid, 95.0, 1.0 - 0.04 * stars, time) * 0.5;

    return c;
}

float3 Void_SRGBToLinear(float3 c)
{
    float3 lo = c / 12.92;
    float3 hi = pow(max((c + 0.055) / 1.055, 0.0), 2.4);
    return float3(c.x <= 0.04045 ? lo.x : hi.x,
                  c.y <= 0.04045 ? lo.y : hi.y,
                  c.z <= 0.04045 ? lo.z : hi.z);
}

// ---------------------------------------------------------------------------
// Point d'entrée (nom compatible node Custom Function : "VoidPortal")
//   ViewDirWS    : position monde du fragment - position caméra (pas besoin de normaliser)
//   Poly         : densité de polygones (35 validé)
//   Transparency : 0..1 (0.35 validé)
//   Stars        : multiplicateur d'étoiles (0.25 validé)
//   Wobble       : amplitude du mouvement des polygones (0.04 validé)
//   WobbleSpeed  : vitesse du mouvement (0.4 validé)
//   Time         : temps en secondes
//   HDRStars     : 0 = sortie bornée à 1 (identique à l'aperçu), 1 = étoiles > 1 pour le bloom
// ---------------------------------------------------------------------------
void VoidPortal_float(float3 ViewDirWS, float Poly, float Transparency, float Stars,
                      float Wobble, float WobbleSpeed, float Time, float HDRStars, out float3 Color)
{
    // Unity est main gauche (+Z devant), l'aperçu main droite (-Z devant) :
    // on retourne Z pour retrouver exactement le même ciel.
    float3 d = normalize(ViewDirWS);
    d.z = -d.z;

    float3 c = Void_Space(d, Poly, Transparency, Stars, Wobble, WobbleSpeed, Time);

    if (HDRStars < 0.5)
    {
        c = saturate(c);
    }

    // L'aperçu écrit directement des valeurs sRGB. En espace Linear (défaut URP),
    // on convertit pour que l'écran affiche les mêmes couleurs.
    #if !defined(UNITY_COLORSPACE_GAMMA)
    c = Void_SRGBToLinear(c);
    #endif

    Color = c;
}

// Variante appelée si le node Custom Function est en précision "Half".
// Elle délègue au calcul float : le maillage a besoin de la précision 32 bits.
void VoidPortal_half(float3 ViewDirWS, float Poly, float Transparency, float Stars,
                     float Wobble, float WobbleSpeed, float Time, float HDRStars, out float3 Color)
{
    VoidPortal_float(ViewDirWS, Poly, Transparency, Stars, Wobble, WobbleSpeed, Time, HDRStars, Color);
}

#endif // VOID_PORTAL_INCLUDED

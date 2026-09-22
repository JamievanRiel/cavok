namespace Cavok;

/// <summary>
/// A forecast icing layer in a military TAF, group <c>6IchihihitL</c>: for example <c>651109</c> is moderate icing in
/// cloud from 11,000 ft, 9,000 ft thick.
/// </summary>
/// <param name="Type">
/// Type of icing, code 0–9: 0 trace, 1 light, 2 light in cloud, 3 light in precipitation, 4 moderate, 5 moderate in
/// cloud, 6 moderate in precipitation, 7 severe, 8 severe in cloud, 9 severe in precipitation.
/// </param>
/// <param name="BaseFeet">Height of the base of the layer in feet (reported in hundreds of feet).</param>
/// <param name="ThicknessFeet">Thickness of the layer in feet (reported in thousands of feet).</param>
public sealed record IcingLayer(int Type, int BaseFeet, int ThicknessFeet);

/// <summary>
/// A forecast turbulence layer in a military TAF, group <c>5BhBhBhBtL</c>: for example <c>520002</c> is occasional
/// moderate turbulence in clear air from the surface to 2,000 ft.
/// </summary>
/// <param name="Type">
/// Type of turbulence, code 0–9: 0 none, 1 light, 2 and 3 moderate in clear air (occasional, frequent), 4 and 5
/// moderate in cloud (occasional, frequent), 6 and 7 severe in clear air (occasional, frequent), 8 and 9 severe in
/// cloud (occasional, frequent).
/// </param>
/// <param name="BaseFeet">Height of the base of the layer in feet (reported in hundreds of feet).</param>
/// <param name="ThicknessFeet">Thickness of the layer in feet (reported in thousands of feet).</param>
public sealed record TurbulenceLayer(int Type, int BaseFeet, int ThicknessFeet);

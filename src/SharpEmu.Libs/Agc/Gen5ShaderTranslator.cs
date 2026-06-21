// Copyright (C) 2026 SharpEmu Emulator Project
// SPDX-License-Identifier: GPL-2.0-or-later

using SharpEmu.HLE;
using SharpEmu.Libs.VideoOut;
using System.Buffers.Binary;

namespace SharpEmu.Libs.Agc;

internal static class Gen5ShaderTranslator
{
    private static readonly uint[] FullscreenBarycentricEs =
    [
        0xBFA00001, 0x7E000000, 0x7E000000, 0x7E000000,
        0x93EBFF03, 0x00080008, 0x8F6A8C6B, 0x8700FF03,
        0x000000FF, 0x887C6A00, 0xBF900009, 0x81EA6BC0,
        0x90FE6AC1, 0xF8000941, 0x00000000, 0x81EA00C0,
        0xBF8CFF0F, 0x90FE6AC1, 0x36040A81, 0x2C060A81,
        0x7E000280, 0x7E0202F2, 0xD7460002, 0x03050302,
        0xD7460003, 0x03050303, 0x7E040B02, 0x7E060B03,
        0xF80008CF, 0x01000302, 0xBF810000,
    ];

    private static readonly uint[] FullscreenBarycentricPs =
    [
        0xD52F0000, 0x00000200,
        0xD52F0001, 0x00000602,
        0xF8001C0F, 0x00000100,
        0xBF810000,
    ];

    public static bool TryTranslate(
        CpuContext ctx,
        ulong exportShaderAddress,
        ulong pixelShaderAddress,
        uint psInputEna,
        uint psInputAddr,
        out GuestDrawKind drawKind)
    {
        drawKind = GuestDrawKind.None;
        if (exportShaderAddress == 0 ||
            pixelShaderAddress == 0 ||
            psInputEna != 0x00000002 ||
            psInputAddr != 0x00000002 ||
            !MatchesProgram(ctx, exportShaderAddress, FullscreenBarycentricEs) ||
            !MatchesProgram(ctx, pixelShaderAddress, FullscreenBarycentricPs))
        {
            return false;
        }

        drawKind = GuestDrawKind.FullscreenBarycentric;
        return true;
    }

    private static bool MatchesProgram(CpuContext ctx, ulong address, ReadOnlySpan<uint> expected)
    {
        var bytes = new byte[expected.Length * sizeof(uint)];
        if (!ctx.Memory.TryRead(address, bytes))
        {
            return false;
        }

        for (var index = 0; index < expected.Length; index++)
        {
            if (BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(index * sizeof(uint))) != expected[index])
            {
                return false;
            }
        }

        return true;
    }
}

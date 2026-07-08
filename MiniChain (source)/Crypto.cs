using System.Security.Cryptography;
using System.Text;

namespace MiniChain;

/// <summary>
/// Thin helpers over SHA-256. In blockchains a hash is used as a
/// deterministic, collision-resistant, one-way "fingerprint" of data.
/// </summary>
public static class Crypto
{
    /// <summary>SHA-256 of raw bytes -> 32 raw bytes.</summary>
    public static byte[] Sha256(byte[] data) => SHA256.HashData(data);

    /// <summary>SHA-256 of a UTF-8 string -> lowercase hex string.</summary>
    public static string Sha256Hex(string text)
        => ToHex(Sha256(Encoding.UTF8.GetBytes(text)));

    /// <summary>Bitcoin-style double hash: SHA256(SHA256(x)).</summary>
    public static byte[] DoubleSha256(byte[] data) => Sha256(Sha256(data));

    public static string ToHex(byte[] bytes)
    {
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    public static byte[] FromHex(string hex)
    {
        var bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        return bytes;
    }

    /// <summary>
    /// Count the number of leading zero *bits* in a hash. This is the real
    /// difficulty measure behind proof-of-work (leading hex zeros is a
    /// coarser, human-friendly proxy).
    /// </summary>
    public static int LeadingZeroBits(byte[] hash)
    {
        int count = 0;
        foreach (var b in hash)
        {
            if (b == 0) { count += 8; continue; }
            // count leading zeros within this byte
            for (int bit = 7; bit >= 0; bit--)
            {
                if ((b & (1 << bit)) == 0) count++;
                else return count;
            }
        }
        return count;
    }
}

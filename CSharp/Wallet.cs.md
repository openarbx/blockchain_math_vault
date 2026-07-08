---
tags: [blockchain, csharp, code]
---

# `Wallet.cs`

> [!info] Links
> [[🏠 Home]] · concept → [[3 - Digital Signatures]] · project → [[MiniChain Overview]]

ECDSA key pair, signing, verification, and address derivation.

```csharp
using System.Security.Cryptography;
using System.Text;

namespace MiniChain;

/// <summary>
/// A wallet is just an elliptic-curve key pair. The private key is a secret
/// integer d; the public key is the curve point Q = d*G. Signing proves you
/// know d without revealing it (ECDSA over the NIST P-256 curve here; Bitcoin
/// uses secp256k1, same math, different constants).
/// </summary>
public sealed class Wallet
{
    private readonly ECDsa _key;

    public Wallet() => _key = ECDsa.Create(ECCurve.NamedCurves.nistP256);

    /// <summary>Public address: hex of the exported public key.</summary>
    public string Address
    {
        get
        {
            var pub = _key.ExportSubjectPublicKeyInfo();
            // Address = last 20 bytes of SHA-256(pubkey), like a shortened hash.
            var h = Crypto.Sha256(pub);
            return Crypto.ToHex(h[^20..]);
        }
    }

    public byte[] PublicKey => _key.ExportSubjectPublicKeyInfo();

    /// <summary>Sign a message: returns an (r,s) DSA signature over SHA-256(message).</summary>
    public byte[] Sign(string message)
        => _key.SignData(Encoding.UTF8.GetBytes(message), HashAlgorithmName.SHA256);

    /// <summary>Anyone can verify a signature given only the public key.</summary>
    public static bool Verify(byte[] publicKey, string message, byte[] signature)
    {
        using var verifier = ECDsa.Create();
        verifier.ImportSubjectPublicKeyInfo(publicKey, out _);
        return verifier.VerifyData(
            Encoding.UTF8.GetBytes(message), signature, HashAlgorithmName.SHA256);
    }

    public static string AddressFromPublicKey(byte[] publicKey)
        => Crypto.ToHex(Crypto.Sha256(publicKey)[^20..]);
}
```

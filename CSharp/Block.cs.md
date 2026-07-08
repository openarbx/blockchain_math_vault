---
tags: [blockchain, csharp, code]
---

# `Block.cs`

> [!info] Links
> [[🏠 Home]] · concept → [[4 - Proof of Work]] · project → [[MiniChain Overview]]

Block header, Merkle root computation, and the proof-of-work mining loop.

```csharp
namespace MiniChain;

/// <summary>
/// A block bundles transactions plus a header. The header is what gets hashed
/// during mining; it commits to the previous block (chain linkage), the
/// Merkle root (transaction commitment), a timestamp, a difficulty target,
/// and a nonce (the free variable miners search over).
/// </summary>
public sealed class Block
{
    public int Index { get; init; }
    public long Timestamp { get; init; }
    public string PreviousHash { get; init; } = "";
    public List<Transaction> Transactions { get; init; } = new();
    public int Difficulty { get; init; }          // required leading zero bits
    public long Nonce { get; set; }
    public string MerkleRoot { get; private set; } = "";
    public string Hash { get; set; } = "";

    public void ComputeMerkleRoot()
        => MerkleRoot = MerkleTree.Root(Transactions.Select(t => t.TxId).ToList());

    /// <summary>The header string that gets hashed. Nonce is the only free knob.</summary>
    public string HeaderString()
        => $"{Index}|{Timestamp}|{PreviousHash}|{MerkleRoot}|{Difficulty}|{Nonce}";

    public byte[] HeaderHash()
        => Crypto.Sha256(System.Text.Encoding.UTF8.GetBytes(HeaderString()));

    /// <summary>
    /// Proof-of-work: increment the nonce until the header hash has at least
    /// <see cref="Difficulty"/> leading zero bits. Returns iterations tried.
    /// </summary>
    public long Mine()
    {
        ComputeMerkleRoot();
        long tries = 0;
        while (true)
        {
            tries++;
            var h = HeaderHash();
            if (Crypto.LeadingZeroBits(h) >= Difficulty)
            {
                Hash = Crypto.ToHex(h);
                return tries;
            }
            Nonce++;
        }
    }

    public bool HasValidProofOfWork()
        => Crypto.LeadingZeroBits(HeaderHash()) >= Difficulty
           && Crypto.ToHex(HeaderHash()) == Hash;
}
```

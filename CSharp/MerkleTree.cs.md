---
tags: [blockchain, csharp, code]
---

# `MerkleTree.cs`

> [!info] Links
> [[🏠 Home]] · concept → [[2 - Merkle Trees]] · project → [[MiniChain Overview]]

Merkle root construction, proof building, and proof verification.

```csharp
using System.Text;

namespace MiniChain;

/// <summary>
/// A Merkle tree reduces N transaction hashes to a single 32-byte root by
/// repeatedly hashing pairs. It lets a light client prove that one
/// transaction is in a block using only log2(N) hashes.
/// </summary>
public static class MerkleTree
{
    /// <summary>
    /// Compute the Merkle root from a list of leaf strings.
    /// Odd levels duplicate the last node (Bitcoin convention).
    /// </summary>
    public static string Root(IReadOnlyList<string> leaves)
    {
        if (leaves.Count == 0)
            return Crypto.Sha256Hex(""); // empty tree convention

        // Level 0: hash each leaf.
        var level = leaves.Select(Crypto.Sha256Hex).ToList();

        while (level.Count > 1)
        {
            var next = new List<string>();
            for (int i = 0; i < level.Count; i += 2)
            {
                string left = level[i];
                string right = (i + 1 < level.Count) ? level[i + 1] : left; // duplicate if odd
                next.Add(Crypto.Sha256Hex(left + right));
            }
            level = next;
        }
        return level[0];
    }

    public record ProofStep(string Hash, bool IsRight);

    /// <summary>
    /// Build a Merkle proof (authentication path) for the leaf at
    /// <paramref name="index"/>: the sibling hashes needed to recompute the root.
    /// </summary>
    public static List<ProofStep> BuildProof(IReadOnlyList<string> leaves, int index)
    {
        var proof = new List<ProofStep>();
        var level = leaves.Select(Crypto.Sha256Hex).ToList();
        int idx = index;

        while (level.Count > 1)
        {
            var next = new List<string>();
            for (int i = 0; i < level.Count; i += 2)
            {
                string left = level[i];
                string right = (i + 1 < level.Count) ? level[i + 1] : left;

                if (i == idx || i + 1 == idx)
                {
                    bool siblingIsRight = (idx % 2 == 0);
                    string sibling = siblingIsRight ? right : left;
                    proof.Add(new ProofStep(sibling, siblingIsRight));
                }
                next.Add(Crypto.Sha256Hex(left + right));
            }
            idx /= 2;
            level = next;
        }
        return proof;
    }

    /// <summary>Recompute a root from a leaf + its proof, to verify membership.</summary>
    public static string VerifyProof(string leaf, List<ProofStep> proof)
    {
        string running = Crypto.Sha256Hex(leaf);
        foreach (var step in proof)
        {
            running = step.IsRight
                ? Crypto.Sha256Hex(running + step.Hash)
                : Crypto.Sha256Hex(step.Hash + running);
        }
        return running;
    }
}
```

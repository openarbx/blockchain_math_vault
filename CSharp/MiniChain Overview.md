---
tags: [blockchain, csharp, code]
---

# MiniChain Overview

> [[🏠 Home]]

A verified mini-blockchain in C#. Every number quoted across this vault is real
output from this program.

## Source files (one per pillar)

| File | Implements | Concept |
|------|------------|---------|
| [[Crypto.cs]] | SHA-256 helpers, leading-zero-bit count | [[1 - Hash Functions]] |
| [[MerkleTree.cs]] | Merkle root, proof build & verify | [[2 - Merkle Trees]] |
| [[Wallet.cs]] | ECDSA key pair, sign/verify, addresses | [[3 - Digital Signatures]] |
| Transaction.cs | signed value transfer + validity rule | [[3 - Digital Signatures]] |
| [[Block.cs]] | header, Merkle root, the mining loop | [[4 - Proof of Work]] |
| [[Blockchain.cs]] | genesis, mempool, balances, full validation | [[5 - Putting It Together]] |
| Program.cs | the demo driver | — |

## Running it

```bash
cd MiniChain
dotnet run -c Release          # normal machines with a working .NET SDK
```

On locked-down / offline machines where the MSBuild worker hangs (no NuGet,
blocked localhost sockets), use the fallback script, which invokes the Roslyn
compiler directly:

```bash
bash build_and_run.sh
```

## Verified full output

```
=== 1. HASHING: avalanche effect ===
SHA256("hello")  = 2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824
SHA256("hellp")  = fdd7585e08c4e2afd71dcabdb4636c89d557a3f42db9e2040c8bbd1708aa4ce7

=== 2. MERKLE TREE + membership proof ===
Merkle root of [A,B,C,D] = 42fe5be41eb4f21b5936d850…
Proof for 'tx-C' has 2 steps; recomputed root matches: True

=== 3. WALLETS + ECDSA signatures ===
Alice address = 8707ea4a0efce61524cbe500d128ee2c014927d1
Bob   address = 665fd2036f40cc0768d58050be8c68d6dcb0f345
Signature verifies (untampered): True
Signature verifies (tampered msg): False

=== 4. MINING + CHAIN ===
Genesis mined. Difficulty = 16 leading zero bits.
Block 1 mined: nonce=60023, hash=0000360a094c18f326d3…
Block 2 mined: nonce=72916, hash=0000e3cb3b72ab387084…
Alice balance = 90   Bob balance = 10

=== 5. VALIDATION + TAMPER DETECTION ===
Chain valid before tampering: True
Chain valid after tampering:  False  ->  Block 2: Merkle root mismatch (tx tampered).
```

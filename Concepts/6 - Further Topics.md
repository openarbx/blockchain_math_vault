---
tags: [blockchain, further-reading]
---

# 6 · Further Topics

> [[🏠 Home]] · prev → [[5 - Putting It Together]]

Each is a natural extension of the same four-pillar math.

> [!note] UTXO vs. account model
> Bitcoin tracks **unspent transaction outputs** (coins as discrete objects);
> Ethereum tracks **balances** in accounts. MiniChain uses the simpler account
> model in `Blockchain.BalanceOf`.

> [!note] Proof-of-Stake
> Replaces the [[4 - Proof of Work|energy lottery]] with a **stake-weighted** one:
> the chance of being chosen to propose a block is proportional to coins staked.
> Same "who appends?" question, different sampling distribution — and no energy burn.

> [!note] Difficulty retargeting
> The feedback algorithm that adjusts the target $T$ so mean block time stays
> constant as total hash-rate rises or falls. Bitcoin retargets every 2016 blocks.

> [!note] secp256k1 + public-key recovery
> Bitcoin's actual curve. Its signatures carry an extra byte $(r, s, v)$ that
> lets a verifier **recover** the signing public key from the signature — saving
> space by not transmitting the key. See [[3 - Digital Signatures]].

> [!note] Schnorr / BLS signatures
> Signature **aggregation** — combining many signatures into one — the basis of
> modern scaling and privacy work (Bitcoin's Taproot, Ethereum consensus).

---
tags: [blockchain, csharp, code]
---

# `Blockchain.cs`

> [!info] Links
> [[🏠 Home]] · concept → [[5 - Putting It Together]] · project → [[MiniChain Overview]]

Genesis block, mempool, balances, and full chain validation.

```csharp
namespace MiniChain;

/// <summary>
/// The chain itself: an ordered list of blocks where each block's header
/// commits to the previous block's hash. Tampering with any past block
/// changes its hash, which breaks every later PreviousHash link.
/// </summary>
public sealed class Blockchain
{
    public List<Block> Chain { get; } = new();
    public int Difficulty { get; }
    public decimal MiningReward { get; }
    private readonly List<Transaction> _mempool = new();

    public Blockchain(int difficulty = 16, decimal miningReward = 50m)
    {
        Difficulty = difficulty;
        MiningReward = miningReward;
        CreateGenesisBlock();
    }

    private void CreateGenesisBlock()
    {
        var genesis = new Block
        {
            Index = 0,
            Timestamp = 0,
            PreviousHash = new string('0', 64),
            Difficulty = Difficulty,
        };
        genesis.Mine();
        Chain.Add(genesis);
    }

    public Block Latest => Chain[^1];

    public void AddTransaction(Transaction tx)
    {
        if (!tx.IsValid()) throw new InvalidOperationException("Invalid transaction signature.");
        _mempool.Add(tx);
    }

    /// <summary>Package the mempool + a coinbase reward into a new mined block.</summary>
    public Block MinePendingTransactions(string minerAddress)
    {
        var txs = new List<Transaction>
        {
            new Transaction { Sender = "COINBASE", Recipient = minerAddress,
                              Amount = MiningReward, Nonce = Latest.Index + 1 }
        };
        txs.AddRange(_mempool);

        var block = new Block
        {
            Index = Latest.Index + 1,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            PreviousHash = Latest.Hash,
            Transactions = txs,
            Difficulty = Difficulty,
        };
        block.Mine();
        Chain.Add(block);
        _mempool.Clear();
        return block;
    }

    /// <summary>Naive balance: sum of received minus sum of sent across the chain.</summary>
    public decimal BalanceOf(string address)
    {
        decimal bal = 0;
        foreach (var b in Chain)
            foreach (var t in b.Transactions)
            {
                if (t.Recipient == address) bal += t.Amount;
                if (t.Sender == address) bal -= t.Amount;
            }
        return bal;
    }

    /// <summary>
    /// Full validation: every block's PoW holds, its stored hash matches its
    /// header, its Merkle root matches its transactions, and the linkage to
    /// the previous block is intact.
    /// </summary>
    public (bool ok, string? reason) IsValid()
    {
        for (int i = 0; i < Chain.Count; i++)
        {
            var b = Chain[i];

            var expectedRoot = MerkleTree.Root(b.Transactions.Select(t => t.TxId).ToList());
            if (b.Transactions.Count > 0 && b.MerkleRoot != expectedRoot)
                return (false, $"Block {i}: Merkle root mismatch (tx tampered).");

            if (!b.HasValidProofOfWork())
                return (false, $"Block {i}: proof-of-work invalid.");

            if (i > 0 && b.PreviousHash != Chain[i - 1].Hash)
                return (false, $"Block {i}: broken link to previous block.");
        }
        return (true, null);
    }
}
```

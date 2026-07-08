using MiniChain;

Console.WriteLine("=== 1. HASHING: avalanche effect ===");
Console.WriteLine($"SHA256(\"hello\")  = {Crypto.Sha256Hex("hello")}");
Console.WriteLine($"SHA256(\"hellp\")  = {Crypto.Sha256Hex("hellp")}  (one letter changed)");
Console.WriteLine();

Console.WriteLine("=== 2. MERKLE TREE + membership proof ===");
var leaves = new List<string> { "tx-A", "tx-B", "tx-C", "tx-D" };
string root = MerkleTree.Root(leaves);
Console.WriteLine($"Merkle root of [A,B,C,D] = {root[..24]}…");
int idx = 2; // prove tx-C
var proof = MerkleTree.BuildProof(leaves, idx);
string recomputed = MerkleTree.VerifyProof(leaves[idx], proof);
Console.WriteLine($"Proof for '{leaves[idx]}' has {proof.Count} steps; recomputed root matches: {recomputed == root}");
Console.WriteLine();

Console.WriteLine("=== 3. WALLETS + ECDSA signatures ===");
var alice = new Wallet();
var bob   = new Wallet();
Console.WriteLine($"Alice address = {alice.Address}");
Console.WriteLine($"Bob   address = {bob.Address}");
var msg = "Alice pays Bob 10";
var sig = alice.Sign(msg);
Console.WriteLine($"Signature verifies (untampered): {Wallet.Verify(alice.PublicKey, msg, sig)}");
Console.WriteLine($"Signature verifies (tampered msg): {Wallet.Verify(alice.PublicKey, "Alice pays Bob 1000", sig)}");
Console.WriteLine();

Console.WriteLine("=== 4. MINING + CHAIN ===");
var chain = new Blockchain(difficulty: 16, miningReward: 50m);
Console.WriteLine($"Genesis mined. Difficulty = {chain.Difficulty} leading zero bits.");

// Give Alice a reward block so she has coins to spend.
var b1 = chain.MinePendingTransactions(alice.Address);
Console.WriteLine($"Block 1 mined: nonce={b1.Nonce}, hash={b1.Hash[..20]}…");

// Alice -> Bob, signed.
var tx = new Transaction { Sender = alice.Address, Recipient = bob.Address, Amount = 10m, Nonce = 1 };
tx.PublicKey = alice.PublicKey;
tx.Signature = alice.Sign(tx.CanonicalForm());
chain.AddTransaction(tx);
var b2 = chain.MinePendingTransactions(alice.Address);
Console.WriteLine($"Block 2 mined: nonce={b2.Nonce}, hash={b2.Hash[..20]}…");
Console.WriteLine($"Alice balance = {chain.BalanceOf(alice.Address)}   Bob balance = {chain.BalanceOf(bob.Address)}");
Console.WriteLine();

Console.WriteLine("=== 5. VALIDATION + TAMPER DETECTION ===");
var (ok1, _) = chain.IsValid();
Console.WriteLine($"Chain valid before tampering: {ok1}");
// Tamper: rewrite an amount in block 2 without re-mining.
chain.Chain[2].Transactions[1] = new Transaction {
    Sender = tx.Sender, Recipient = tx.Recipient, Amount = 9999m, Nonce = tx.Nonce,
    PublicKey = tx.PublicKey, Signature = tx.Signature };
var (ok2, reason) = chain.IsValid();
Console.WriteLine($"Chain valid after tampering:  {ok2}  ->  {reason}");

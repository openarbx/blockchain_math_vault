namespace MiniChain;

/// <summary>
/// A signed value transfer. The signature covers the canonical string form,
/// so any tampering with sender/recipient/amount invalidates it.
/// </summary>
public sealed class Transaction
{
    public string Sender { get; init; } = "";      // address ("COINBASE" for mining reward)
    public string Recipient { get; init; } = "";
    public decimal Amount { get; init; }
    public long Nonce { get; init; }               // per-sender counter -> prevents replay
    public byte[]? PublicKey { get; set; }
    public byte[]? Signature { get; set; }

    /// <summary>Canonical bytes that get signed and hashed. Order is fixed.</summary>
    public string CanonicalForm()
        => $"{Sender}|{Recipient}|{Amount}|{Nonce}";

    public string TxId => Crypto.Sha256Hex(CanonicalForm());

    public bool IsCoinbase => Sender == "COINBASE";

    public bool IsValid()
    {
        if (IsCoinbase) return true;                 // minting reward, no signature
        if (PublicKey is null || Signature is null) return false;
        // 1. Signature must verify against the claimed public key.
        if (!Wallet.Verify(PublicKey, CanonicalForm(), Signature)) return false;
        // 2. The sender address must actually be derived from that public key.
        return Wallet.AddressFromPublicKey(PublicKey) == Sender;
    }

    public override string ToString()
        => IsCoinbase
            ? $"COINBASE -> {Recipient[..8]}…  {Amount}"
            : $"{Sender[..8]}… -> {Recipient[..8]}…  {Amount} (n={Nonce})";
}

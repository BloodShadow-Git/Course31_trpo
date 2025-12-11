namespace Course31_trpo.Sources.Structures
{
    public readonly struct Report(DateOnly dateOfSale, Transfer[] transfers, string filePath)
    {
        public DateOnly DateOfSale { get; } = dateOfSale;
        public Transfer[] Transfers { get; } = transfers;
        public string FilePath { get; } = filePath;
    }

    public readonly struct Transfer(int amount, string company, string description, byte day)
    {
        public int Amount { get; } = amount;
        public string Company { get; } = company;
        public string Descrition { get; } = description;
        public byte Day { get; } = day;
    }
}

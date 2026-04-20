using Nethereum.ABI.FunctionEncoding.Attributes;

namespace ChainNote.Models;

[FunctionOutput]
public class NoteRecord : IFunctionOutputDTO
{
    [Parameter("string", "content", 1)]
    public string Content { get; set; } = string.Empty;

    [Parameter("uint256", "timestamp", 2)]
    public ulong Timestamp { get; set; }

}

[FunctionOutput]
public class GetMyNotesOutputDTO : IFunctionOutputDTO
{
    [Parameter("tuple[]", "", 1)]
    public List<NoteRecord> Notes { get; set; } = [];
}
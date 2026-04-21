using Nethereum.Web3;
using Nethereum.JsonRpc.Client;
using ChainNote.Models;
using Microsoft.AspNetCore.Components;
using Nethereum.RPC.Eth.DTOs;

namespace ChainNote.Services;

public class Web3Service
{
    private readonly Web3 _web3;
    private readonly HttpClient _http;
    private readonly NavigationManager _nav;
    private readonly string _contractAddress =
        "0x5FbDB2315678afecb367f032d93F642f64180aa3";
    private string _abi;

    public Web3Service(HttpClient http, NavigationManager nav)
    {
        _http = http;
        _nav = nav;

        var url = "http://127.0.0.1:8545";
        var rpcClient = new RpcClient(new Uri(url), http);
        _web3 = new Web3(rpcClient);
    }

    public async Task<List<NoteRecord>> GetMyNotesAsync(string userAddress)
    {
        // 1. 確保 ABI 與合約物件已就緒
        if (string.IsNullOrEmpty(_abi))
        {
            var abiUrl = _nav.ToAbsoluteUri("abi.json").ToString();
            _abi = await _http.GetStringAsync(abiUrl);
        }

        // 將ABI興地址結合
        var contract = _web3.Eth.GetContract(_abi, _contractAddress);
        // 鎖定合約裡的特定函式
        var getNotesFunction = contract.GetFunction("getMyNotes");
        
        // 2. 核心修正：手動建立 RPC 請求
        // 函式沒參數，所以 CreateCallInput 括號是空的，
        // 這確保 Data 段是正確的 0 參數編碼
        var callInput = getNotesFunction.CreateCallInput(); 
        callInput.From = userAddress; // 設定 msg.sender

        // 3. 繞過擴充方法，直接透過 Eth.Transactions.Call 請求原始資料
        // 這會強制執行一個 eth_call，完全不涉及參數數量檢查
        var rawHex = await _web3.Eth.Transactions.Call.SendRequestAsync(
            callInput);

        // 4. 手動解碼回 DTO 物件
        // Nethereum 的 Function 物件內建了解碼器，可以直接將 Hex 轉成你的 OutputDTO
        var result = getNotesFunction.DecodeDTOTypeOutput<
            GetMyNotesOutputDTO>(rawHex);

        return result?.Notes ?? [];
    }

    // 建議改用這個更直覺的寫法，分開處理 TransactionInput
    public async Task<(bool success, string message)> AddNoteAsync(
        string userAddress, string content)
    {
        try
        {
            if (string.IsNullOrEmpty(_abi)) await LoadAbiAsync();

            var contract = _web3.Eth.GetContract(_abi, _contractAddress);
            var addNoteFunction = contract.GetFunction("addNote");

            var accounts = await _web3.Eth.Accounts.SendRequestAsync();

            // 使用 CreateTransactionInput 確保參數對齊
            // 這裡只傳入 content，保證參數數量是 1
            var txInput = addNoteFunction.CreateTransactionInput(
                userAddress, content);
            
            // 發送原始交易請求
            var txHash = await _web3.Eth.Transactions.SendTransaction.SendRequestAsync(txInput);

            // 輪詢收據
            TransactionReceipt receipt = null;
            while (receipt == null)
            {
                await Task.Delay(500);
                receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash);
            }

            return (receipt.Status.Value == 1, "成功");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    private async Task LoadAbiAsync()
    {
        if (string.IsNullOrEmpty(_abi))
        {
            var abiUrl = _nav.ToAbsoluteUri("abi.json").ToString();
            _abi = await _http.GetStringAsync(abiUrl);
        }
    }
}
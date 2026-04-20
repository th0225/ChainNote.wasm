// SPDX-License-Identifier: MIT
pragma solidity ^0.8.13;

contract ChainNote {
    //定義筆記結構 (C# struct)
    struct Note {
        string content;
        uint256 timestamp;
    }

    // 儲存空間：地址 => 筆記結構 (C# Dictionary<string, List<Note>)
    mapping(address => Note[]) private userNotes;

    // 定義事件 (C# Event)
    event NoteAdded(address indexed user, string content, uint256 timestamp);

    // 儲存筆記
    function addNote(string memory _content) public {
        Note memory newNote = Note({
            content: _content,
            timestamp: block.timestamp
        });

        userNotes[msg.sender].push(newNote);

        // 觸發事件
        emit NoteAdded(msg.sender, _content, block.timestamp);
    }

    // 讀取篳記 (view不消耗Gas，因為不改動鏈上狀態)
    function getMyNotes() public view returns (Note[] memory) {
        return userNotes[msg.sender];
    }
}
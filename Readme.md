# RIMEtest

Rewrite <code>rime_api_console.cc</code> in C#

https://github.com/rime/librime/blob/master/tools/rime_api_console.cc

## System Requirement
Windows 10 1903 and later (UTF-8 issue)  

## Preparation
1. Put **rime.dll**, **schema** and **opencc** in same folder
2. Set font in your cmd/powershell to show Chinese charactor  
Ex: MingLiU
3. Change code page to show UTF-8  
`chcp 65001`

## Usage
    .\RIMEtest.exe
It will spend more time in first startup.  
Your can start typing after these lines shows up:  

    I20220326 13:33:14.058302 10636 engine.cc:123] updated option: ascii_mode
    I20220326 13:33:14.058302 10636 engine.cc:123] updated option: zh_trad
    I20220326 13:33:14.058302 10636 engine.cc:123] updated option: zh_tw
    I20220326 13:33:14.058302 10636 engine.cc:123] updated option: zh_simp  
### Special key
I only list most used key here:
- {Backspace}
- {Page_Down}
- {Page_Up}
- {Escape}
- {Return}

Full list: https://github.com/rime/librime/blob/master/src/rime/key_table.cc


### Special command
- `print schema list`
- `select schema {0}`  
select schema luna_pinyin  
select schema cangjie5
- `print candidate list`
- `select candidate {0}`
- `set option {0}`  
set option zh_simp  
set option zh_trad  
set option zh_tw  
set option ascii_mode  
set option !ascii_mode


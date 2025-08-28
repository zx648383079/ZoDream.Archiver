## 命令参数输入


在 PackageName 输入

格式

```
<cmd>:<args1>,<args2>.<cmd2>:<args3>,<args4>
```

多个命令以 . 分割

参数以 : 开始

多个参数以 , 分割

数字，byte[] 均以十六进制字符串输入：C601

支持命令


```
fake
fake:<skip:FF>     // 跳过 0xFF 个字节 

xor:<xorpad:FFFF>
xor:<xorpad:C6>,<maxLength:3F> // 在文件开头的 0x3F 个字符需要跟 byte[0xC6] 进行异或

key:<key:FFFFFFF>
```

## Source Code From

1. [AssetRipper](https://github.com/AssetRipper/AssetRipper)
2. [StudioDev](https://github.com/Modder4869/StudioDev)
3. [Studio](https://github.com/AXiX-official/Studio) 
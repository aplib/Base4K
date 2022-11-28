# UTF16 Base4K(alphabetical) encoding for NET Core
[![Nuget](https://img.shields.io/nuget/v/Base4K?label=nuget)](https://www.nuget.org/packages/Base4K)

### Summary

The Base4K encoding scheme represents binary data as text using a set of 4096 characters (letters) of the virtual alphabet.
In this encoding, the data-to-character ratio of 3 bytes roughly corresponds to two Unicode characters of text.
The library implements two data storage in memory formats - in the form of a unary block and in the form of a chain, so far only a scalar algorithm.
This encoding is focused on the compact representation of binary data, hashes and cryptographic keys on a web page or embed into a text document.

Base4K схема кодирования представляет двоичные данные как текст с использованием набора 4096 символов (букв) виртуального алфавита.
В этой кодировке соотношение данных к символам - 3 байта примерно соответствуют двум Unicode символам текста.
В библиотеке реализовано два формата хранения данных в памяти - в виде блока и в виде цепочки, пока еще только скалярный алгоритм.
Кодировка предназначена для компактного представления двоичных данных, хэшей и ключей на веб-странице или в текстовом редакторе.

### Encoding examples

| Primitive  | Bits | Encoded length | Example |
| ------------- | ------------- | ------------- | ------------- |
| Guid  | 128 | 11 | ຯˤYᵞὡὃಳଏΰഽɁ |
| SHA256| 256 | 22 | ǨᇥĪⱿⴊᑲƊԧᵼћᛀላcᶽড়ϐᦶቿKᦱIӌ |
| SHA512  | 512 | 43 | ΊᄮୠඊⳄḚȎԩƯড়ՏፅᴨᴃфণᕢᲚᴘᶳᑑӈṲⳏƫჃᵇᴩཝʇIᓠǾམนᲧӏចḯᧁឥᴗt |

[Online Demo](https://Lex4K.org/Base4K-encoding)

### Scheme

```
[  byte 2 ][  byte 1 ][ byte 0 ]
[     8   ][ 4 ][  4 ][   8    ]
[virtual code 1][virtual code 0]
[  16 <#< 12   ][  16 <#< 12   ]
[unicode char 1][unicode char 0]
```

### Library

```c#
Namespace: Lex4K
Static class: Base4K

IsBase4KChar()
EncodeBlock() - 2 overloads
EncodeBlockToString()
EncodeBlockToStringBuilder()
EncodeChain() - 2 overloads
EncodeChainToString()
EncodeChainToStringBuilder()
DecodeBlock()
DecodeBlockToNewBuffer()
DecodeChain()
DecodeChainToNewBuffer()
```

Apache 2.0 license
2022 © vadim b. (vadim baklanov)

# JWTrek
JWT Token C# Cracker

To crack  JWT tokens.
* HS128, HS256, HS384, HS512 support
* Bruteforce
* Wordlist

![JWTrek screenshot](http://www.georgestaupin.com/wp-content/uploads/2020/01/CaptureJWTrek2.png)

## Usage

Launch EXE and follow the prompt:

* **Token**: the full JWT token string to crack
* **Mode**[BRUTEFORCE]:
    * **Charset**: the alphabet to use for the brute force (default: "abcdefghijklmnopqrstuvwxyz0123456789")
    * **Length**: the length of the string generated during the brute force (default: 6)
* **Mode**[WORDLIST]:
    * **Wordlist path**: the wordlist file path (one word per line)

## Requirements

This software requires .NET 4.5.2 or higher

## Example

Cracking the default [jwt.io example](https://jwt.io):

```
TOKEN > eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWV9.TJVA95OrM7E2cBab30RMHrHDcEfxjoYZgeFONFh7HgQ
CHARSET > abcdefghijklmnopqrstuvwxyz
LENGTH > 6
```

It takes about 15 minutes with a Lenovo T550 (Intel Core i5-5300U @ 2.30 Ghz).

## Contributing

Everyone is very welcome to contribute to this project.

## Operational version

An operational version (.EXE) is available for download here:
[www.georgestaupin.com](https://www.georgestaupin.com/)

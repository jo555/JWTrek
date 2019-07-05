# JWTrek
JWT Token C# Bruteforcer (HS256) 

To crack HS256 JWT tokens.

![JWTrek screenshot](http://www.georgestaupin.com/wp-content/uploads/2019/07/JWTrek-test-no-monitoring-15.png)

## Usage

Launch EXE and follow the prompt:

* **TOKEN**: the full HS256 JWT token string to crack
* **CHARSET**: the alphabet to use for the brute force (default: "abcdefghijklmnopqrstuvwxyz0123456789")
* **LENGTH**: the length of the string generated during the brute force (default: 6)


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

# QuickDeploy
A simple and efficient tool for deploying software to a server

## Creation of your own certificate with a public private key pair.
* Go to http://www.selfsignedcertificate.com/ and create a certificate for example.org named example.org.crt
* Use OpenSSL to create PFX files with private and public key from the certificate:
```
openssl pkcs12 –export –out private.pfx –inkey example.org.key –in example.org.crt
openssl pkcs12 –export -nokeys –out public.pfx –in example.org.crt
```

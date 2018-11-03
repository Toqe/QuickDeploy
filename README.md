# QuickDeploy
A simple and efficient tool for deploying software

### What problem does this software solve?
Complex software often requires many different steps during deployment. After the compiler is finished with the build, it may be necessary to 

* copy files to one or multiple webservers
* run a command on a server to update the database
* stop a Windows service, replace its binaries and start it again

All these steps can be automated in many different ways. But you often have to learn, install and configure a lot of tools and helpers to achieve this.

This is where QuickDeploy comes into play. It requires you only to install a Windows service on the server. Afterwards you can do all the actions mentioned above with simple C# code - or if you like with VB.NET, Powershell or any tool that can use .NET DLLs.

### Features

* Secure communication: Mutually authenticated TLS connection between client and server
* Easy authentication: Just use your Windows credentials to login on the server. Any action on the server will be executed with the Windows user's permissions.
* Efficient file transfer: Files are only copied if they have changed and are compressed for transfer.

## Gettings started using QuickDeploy

* Create your own certificates with public private key pairs as mentioned in the chapter 'Creation of your own certificates with public private key pairs'.

### Setup of server
* Use the QuickDeploy-server-setup.exe from https://github.com/Toqe/QuickDeploy/releases and install it on the server. This will install the service and create a Windows firewall rule for it. Please check the firewall rule and make sure only trusted computers are able to access the service, for example by using IPSec.
* Copy your own created certificates `server-private.pfx` and `client-public.pfx` to the installation directory, for example `C:\Program Files (x86)\QuickDeploy\Server`
* Open `services.msc` and start `QuickDeployService`.

### Using the client
* Create a new Visual Studio C# project.
* Install the QuickDeploy.Client NuGet package by using Visual Studio's 'Manage Packages' or the Package Manager `Install-Package QuickDeploy.Client`
* Copy your own created certificates `server-public.pfx` and `client-private.pfx` to the new project and set "Copy to Output Directory" to "Always copy".
* Use the following code as starting point for your own deployments:
```C#
using System;
using System.IO;
using QuickDeploy.Client;
using QuickDeploy.Common;
using QuickDeploy.Common.DirectorySyncer;
using QuickDeploy.Common.Messages;

namespace QuickDeploy.Example
{
    public class Program
    {
        static void Main(string[] args)
        {
            var credentials = new Credentials
            {
                Domain = "mydomain",
                Username = "mydeploymentuser",
                Password = "mypassword"
            };

            var expectedServerCertificateFilename = "server-public.pfx";
            var clientCertificateFilename = "client-private.pfx";
            var clientCertificatePassword = "";
            var hostname = "targetserver.example.org";
            var port = 9876;
            var client = new QuickDeployTcpSslClient(hostname, port, expectedServerCertificateFilename, clientCertificateFilename, clientCertificatePassword);

            var directorySyncer = new DirectorySyncer(client);

            // ***
            // Example 1: Sync local directory to server (for example for deployment of IIS websites)
            // ***

            // Copy all files from local c:\develop\mywebsite\deploy to remote c:\inetpub\wwwroot\mywebsite
            directorySyncer.Sync(
                Path.Combine(@"c:\develop\mywebsite\deploy"),
                Path.Combine(@"c:\inetpub\wwwroot\mywebsite"),
                credentials);

            // ***
            // Example 2: Windows Service deployment
            // ***

            // Stop service MyWindowsService ...
            client.ChangeServiceStatus(
                new ChangeServiceStatusRequest
                {
                    ServiceName = "MyWindowsService",
                    DesiredServiceStatus = ServiceStatus.Stop,
                    Credentials = credentials
                });

            // ... then copy all files from local c:\develop\myservice\deploy to remote c:\services\myservice ...
            directorySyncer.Sync(
                Path.Combine(@"c:\develop\myservice\deploy"),
                Path.Combine(@"c:\services\myservice"),
                credentials);

            // ... finally restart service
            client.ChangeServiceStatus(
                new ChangeServiceStatusRequest
                {
                    ServiceName = "MyWindowsService",
                    DesiredServiceStatus = ServiceStatus.Start,
                    Credentials = credentials
                });

            // ***
            // Example 3: Execute command on server
            // ***

            // Copy all files from local c:\develop\mydatabase\deploy to c:\tools\mydatabase ...
            directorySyncer.Sync(
                Path.Combine(@"c:\develop\mydatabase\deploy"),
                Path.Combine(@"c:\tools\mydatabase"),
                credentials);

            // ... then execute the just transferred deploy.bat
            var cmdResult = client.ExecuteCommand(
                new ExecuteCommandRequest
                {
                    Command = @"c:\tools\mydatabase\deploy.bat",
                    Credentials = credentials
                });

            if (cmdResult.ExitCode != 0)
            {
                throw new InvalidOperationException($"Build failed: Command exited with code {cmdResult.ExitCode}");
            }
        }
    }
}
```

## Creation of your own certificates with public private key pairs
This software uses self signed certificates for the authentication between server and client and the TLS encryption of the transferred messages. It is recommended to create and use your own certificates. Otherwise any attacker having access to the open port of your server may take control over the server.

You can create the certificates following these steps:

* Creation of server certificate
	* Go to http://www.selfsignedcertificate.com/ and create a certificate for example.org.
	* Download the both files `example.org.key` and `example.org.cert`. Rename them to `server-example.org.key` and `server-example.org.cert`.
	* Use OpenSSL (see https://wiki.openssl.org/index.php/Binaries or https://indy.fulgan.com/SSL/) and the following command line to create PFX files with private and public key from the certificate. When asked for a password you can leave it empty. If you choose a password here you have to fill it in the `QuickDeploy.ServerService.exe.config` in the appSettings sections as value for the key `serverCertificatePassword`.
	```
	openssl pkcs12 –export –out server-private.pfx –inkey server-example.org.key –in server-example.org.cert
	openssl pkcs12 –export -nokeys –out server-public.pfx –in server-example.org.cert
	```
	* For development: Copy the just created `server-public.pfx` and `server-private.pfx` to the `QuickDeploy.Common` directory and replace the existing ones.
	
* Creation of client certificate
	* Go to http://www.selfsignedcertificate.com/ and create yet another certificate for example.org.
	* Download the both files example.org.key and example.org.cert. Rename them to client-example.org.key and client-example.org.cert.
	* Use OpenSSL and the following command line to create PFX files with private and public key from the certificate. When asked for a password you can leave it empty. If you choose a password here you have to fill it in `clientCertificatePassword` when creating an instance of `QuickDeployTcpSslClient`.
	```
	openssl pkcs12 –export –out client-private.pfx –inkey client-example.org.key –in client-example.org.cert
	openssl pkcs12 –export -nokeys –out client-public.pfx –in client-example.org.cert
	```
	* For development: Copy the just created `client-public.pfx` and `client-private.pfx` to `QuickDeploy.Common` and replace the existing ones.

### What are the files used for?
* The server will use `server-private.pfx` to authententicate to the client on connecting and encrypt the connection with TLS. The file contains the private keys of the server and should be handled carefully and not be made publicly available.
* The client will use `server-public.pfx` to check the server's certificate on connecting. The file doesn't contain private keys and can be exposed publicly.
* The client will use `client-private.pfx` to authententicate to the server on connecting. The file contains the private keys of the client and should be handled carefully and not be made publicly available.
* The server will use `client-public.pfx` to check the client's certificate on connecting. The file doesn't contain private keys and can be exposed publicly.

## Changelog
* Version 2.0.0: Added client authentication for better security. This is a breaking change to version 1.0.0, so clients and servers of different versions can't be used together.
* Version 1.0.0: Initial release

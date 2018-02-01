# QuickDeploy
A simple and efficient tool for deploying software

## What problem does this software solve?
Complex software often requires many different steps during deployment. After the compiler is finished with the build, it may be necessary to 
* copy files to a webserver
* run a command on a server to update the database
* stop a Windows service, replace its binaries and start it again
All these steps can be automated in many different ways. But you often have to learn, install and configure a lot of tools and helpers to achieve this.

This is where QuickDeploy comes into play. It requires you only to install a Windows service on the server. Afterwards you only need to make sure you can create a secured connection to the server and do all the actions mentioned above with simple C# code. You will be able to simply use your Windows credentials with which you login to the remote server.

## Step by step walkthrough
### Getting started
* Clone this repository
* Optional, but recommended: Create your own certificate with a public private key pair as mentioned in the chapter below. Replace the stock private.pfx and public.pfx in the `QuickDeploy.Common` directory.
* Build `QuickDeploy.sln`, for example on the Visual Studio command line with `msbuild QuickDeploy.sln`

### Setup of server
* Copy these files from the `bin` directory to the server(s), to which deployments shall be targeted:
    * private.pfx
    * QuickDeploy.Common.dll
    * QuickDeploy.Server.dll
    * QuickDeploy.ServerService.exe
    * QuickDeploy.ServerService.exe.config
    * SimpleImpersonation.dll
    * Topshelf.dll
* Open an administrator command line and run `QuickDeploy.ServerService.exe install` and `QuickDeploy.ServerService.exe start`.
* Make sure that the server's firewall is configured to allow TCP connections for the application `QuickDeploy.ServerService.exe` or port 9876 (or any port you configured in `QuickDeploy.ServerService.exe.config`). Please also make sure that only trusted computers are able to access this port on the server, for example by configuring IPSec for this.

### Using the client
* Create a new Visual Studio C# project.
* Install the QuickDeploy.Client NuGet package by using Visual Studio's 'Manage Packages' or the Package Manager `Install-Package QuickDeploy.Client`
* Copy the `QuickDeploy.Common\public.pfx` to the new project and set "Copy to Output Directory" to "Always copy".
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

            var certificateFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "public.pfx");
            var hostname = "targetserver.example.org";
            var port = 9876;
            var client = new QuickDeployTcpSslClient(hostname, port, certificateFilename);

            var directorySyncer = new DirectorySyncer(client);

            // ***
            // Example 1: IIS deployment
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
            // Example 3: Command execution
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

## Creation of your own certificate with a public private key pair
This software uses a self signed certificate for the authentication of the server to the client and the TLS encryption of the transferred messages. It is recommended to create and use your own certificates. You can do this following these steps:

* Go to http://www.selfsignedcertificate.com/ and create a certificate for example.org named example.org.crt
* Use OpenSSL to create PFX files with private and public key from the certificate:
```
openssl pkcs12 –export –out private.pfx –inkey example.org.key –in example.org.crt
openssl pkcs12 –export -nokeys –out public.pfx –in example.org.crt
```

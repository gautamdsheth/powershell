# Using PnP PowerShell in Azure Functions

In this article we will setup an Azure Function to use PnP PowerShell

> [!Important]
> Notice that the Azure Function scripts in this article run in a separate thread/job. We do this because of possible conflicts between assemblies of already loaded PowerShell modules and PnP PowerShell (for instance, the Az cmdlets that get loaded by default use some of the same assemblies as PnP PowerShell but in different versions which can cause conflicts). By running the script in a separate thread we will not have these conflicts. If PnP PowerShell is the only module currently being used and loaded in your Azure Function you don't need the Start-ThreadJob construct and you can simply write the script as usual.

## Create the function app

As the UI in https://portal.azure.com changes every now and then, but the principles stay the same, follow the following steps:

1. Create a new Function App and navigate there when ready.
1. Make sure you select the option to run PowerShell V3 functions, based upon PowerShell 7.

## Make PnP PowerShell available to all functions in the app

1. Navigate to `App files` which is located the left side menu of the function app under the `Functions` header.
1. In the dropdown presented, select `requirements.psd1`. You'll notice that the function app wants to provide the Azure cmdlets. If you do not need those, consider removing the `Az` entry presented.
1. Add a new entry or replace the whole contents of the file with:
 
   ```powershell
   @{
       'PnP.PowerShell' = '1.8.0'
   }
   ```
1. The version that will be installed will be the specified nightly build.
1. The moment we release a full 1.0 release you can use wildcards too:

    ```powershell
    @{
        'PnP.PowerShell' = '1.*'
     }
    ```
1. This will then automatically download any minor version of the major 1 release when available. Notice that you cannot use wildcards to specify a nightly build.

If you decide to remove the Az cmdlets, save the `requirements.psd1` file and edit the `profile.psd` file. Mark out the following block in the file as follows:

```powershell
# if ($env:MSI_SECRET) {
#     Disable-AzContextAutosave -Scope Process | Out-Null     
#     Connect-AzAccount -Identity
# }
```

Save the file.

## Decide how you want to authenticate in your PowerShell Function

### By using Credentials

#### Create your credentials
1. Navigate to `Configuration` under `Settings` and create a new Application Setting. 
1. Enter `tenant_user` and enter the username you want to authenticate with as the user
1. Enter `tenant_pwd` and enter the password you want to use for that user

#### Create the function

Create a new function and replace the function code with following example:

````powershell
using namespace System.Net

# Input bindings are passed in via param block.
param($Request, $TriggerMetadata)

# Write to the Azure Functions log stream.
Write-Host "PowerShell HTTP trigger function processed a request."

$script = {
    $securePassword = ConvertTo-SecureString $env:tenant_pwd -AsPlainText -Force
    $credentials = New-Object PSCredential ($env:tenant_user, $securePassword)

    Connect-PnPOnline -Url https://yourtenant.sharepoint.com/sites/demo -Credentials $credentials

    $web = Get-PnPWeb;
    $web.Title
}

$webTitle = Start-ThreadJob -Script $script | Receive-Job -Wait

$body = "The title of the web is $($webTitle)"

# Associate values to output bindings by calling 'Push-OutputBinding'.
Push-OutputBinding -Name Response -Value ([HttpResponseContext]@{
        StatusCode = [HttpStatusCode]::OK
        Body = $body
    })
````

In the example above we are retrieving the username and password from the settings as environment variables. We then create a new credentials object which we pass in to the `Connect-PnPOnline` cmdlet. After connecting to SharePoint we output the title of the web through the function.

### By using a certificate

#### Create your certificate

In this following example we create a new Azure AD Application registration which creates your certificates. You can of course do all this work manually too with your own certificates.

```powershell
$password = Read-Host -Prompt "Enter certificate password" -AsSecureString
Register-PnPAzureADApp -ApplicationName "MyDemoApp" -Tenant [yourtenant.onmicrosoft.com] -CertificatePassword $password -DeviceLogin
```

You will be asked to authenticate and then a pkx and a cer file (public/private keypair) will be create and a new Azure AD Application called 'MyDemoApp' will be created and the public key of the certificate will be configured for the application. Make a note of the clientid shown.

- In your function app, navigate to `TLS/SSL Settings` and switch to the `Private Key Certificates (.pfx)` section.
- Click `Upload Certificate` and select the "MyDemoApp.pfx" file that has been created for you. Enter the password you used in the script above.
- After the certificate has been uploaded, copy the thumbprint value shown.
- Navigate to `Configuration` and add a new Application Setting
- Call the setting `WEBSITE_LOAD_CERTIFICATES` and set the thumbprint as a value. To make all the certificates you uploaded available use `*` as the value. See https://docs.microsoft.com/en-gb/azure/app-service/configure-ssl-certificate-in-code for more information.
- Save the settings

Create a new function and replace the function code with following example:

````powershell
using namespace System.Net

# Input bindings are passed in via param block.
param($Request, $TriggerMetadata)

# Write to the Azure Functions log stream.
Write-Host "PowerShell HTTP trigger function processed a request."

$script = {
    Connect-PnPOnline -Url https://yourtenant.sharepoint.com/sites/demo -ClientId [the clientid created earlier] -Thumbprint [the thumbprint you copied] -Tenant [yourtenant.onmicrosoft.com]

    $web = Get-PnPWeb;
    $web.Title
}

$webTitle = Start-ThreadJob -Script $script | Receive-Job -Wait

$body = "The title of the web is $($webTitle)"

# Associate values to output bindings by calling 'Push-OutputBinding'.
Push-OutputBinding -Name Response -Value ([HttpResponseContext]@{
        StatusCode = [HttpStatusCode]::OK
        Body = $body
    })
````

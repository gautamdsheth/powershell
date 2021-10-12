---
online version: https://pnp.github.io/powershell/cmdlets/Get-PnPPowerPlatformEnvironment.html
Module Name: PnP.PowerShell
external help file: PnP.PowerShell.dll-Help.xml
schema: 2.0.0
---
  
# Get-PnPPowerPlatformEnvironment

## SYNOPSIS

**Required Permissions**

* Azure: management.azure.com

Retrieves the Microsoft Power Platform environments for the current tenant.

## SYNTAX

```
Get-PnPPowerPlatformEnvironment [-Connection <PnPConnection>] [<CommonParameters>]
```

## DESCRIPTION
This cmdlet retrieves the Microsoft Power Platform environments for the current tenant

## EXAMPLES

### Example 1
```powershell
Get-PnPPowerPlatformEnvironment
```

This cmdlets returns the Power Platform environments for the current tenant.

## PARAMETERS

### -Connection
Optional connection to be used by the cmdlet.
Retrieve the value for this parameter by either specifying -ReturnConnection on Connect-PnPOnline or by executing Get-PnPConnection.

```yaml
Type: PnPConnection
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

## RELATED LINKS

[Microsoft 365 Patterns and Practices](https://aka.ms/m365pnp)
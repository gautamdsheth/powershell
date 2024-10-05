﻿using PnP.PowerShell.Commands.Base;
using PnP.PowerShell.Commands.Utilities.REST;
using System;
using System.Management.Automation;
using System.Linq;
using PnP.PowerShell.Commands.Base.PipeBinds;
using PnP.PowerShell.Commands.Utilities;
using PnP.PowerShell.Commands.Attributes;

namespace PnP.PowerShell.Commands.PowerPlatform.Environment
{
    [Cmdlet(VerbsCommon.Get, "PnPPowerPlatformCustomConnector")]
    public class GetPowerPlatformCustomConnector : PnPAzureManagementApiCmdlet
    {
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public PowerPlatformEnvironmentPipeBind Environment;

        [Parameter(Mandatory = false)]
        public SwitchParameter AsAdmin;

        [Parameter(Mandatory = false)]
        public PowerPlatformConnectorPipeBind Identity;

        protected override void ExecuteCmdlet()
        {
            string environmentName = null;
            string baseUrl = PowerPlatformUtility.GetPowerAutomateEndpoint(Connection.AzureEnvironment);
            string powerAppsUrl = PowerPlatformUtility.GetPowerAppsEndpoint(Connection.AzureEnvironment);
            if (ParameterSpecified(nameof(Environment)))
            {
                environmentName = Environment.GetName();
                WriteVerbose($"Using environment as provided '{environmentName}'");
            }
            else
            {
                var environments = GraphHelper.GetResultCollection<Model.PowerPlatform.Environment.Environment>(this, Connection, baseUrl + "/providers/Microsoft.ProcessSimple/environments?api-version=2016-11-01", AccessToken);
                environmentName = environments.FirstOrDefault(e => e.Properties.IsDefault.HasValue && e.Properties.IsDefault == true)?.Name;

                if (string.IsNullOrEmpty(environmentName))
                {
                    throw new Exception($"No default environment found, please pass in a specific environment name using the {nameof(Environment)} parameter");
                }

                WriteVerbose($"Using default environment as retrieved '{environmentName}'");
            }

            if (ParameterSpecified(nameof(Identity)))
            {
                var appName = Identity.GetName();

                WriteVerbose($"Retrieving specific Custom Connector with the provided name '{appName}' within the environment '{environmentName}'");

                var result = GraphHelper.Get<Model.PowerPlatform.Environment.PowerPlatformConnector>(this, Connection, $"{powerAppsUrl}/providers/Microsoft.PowerApps{(AsAdmin ? "/scopes/admin/environments/" + environmentName : "")}/apis/{appName}?api-version=2016-11-01&$filter=environment eq '{environmentName}' and isCustomApi eq 'True'", PowerAppsServiceAccessToken);
                WriteObject(result, false);
            }
            else
            {
                WriteVerbose($"Retrieving all Connectors within environment '{environmentName}'");

                var connectors = GraphHelper.GetResultCollection<Model.PowerPlatform.Environment.PowerPlatformConnector>(this, Connection, $"{powerAppsUrl}/providers/Microsoft.PowerApps/apis?api-version=2016-11-01&$filter=environment eq '{environmentName}' and isCustomApi eq 'True'", PowerAppsServiceAccessToken);
                WriteObject(connectors, true);
            }
        }
    }
}
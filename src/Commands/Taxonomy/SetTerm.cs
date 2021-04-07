﻿using System;
using System.Collections;
using System.Globalization;
using System.Management.Automation;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;

using PnP.PowerShell.Commands.Base.PipeBinds;

namespace PnP.PowerShell.Commands.Taxonomy
{
    [Cmdlet(VerbsCommon.Set, "PnPTerm")]
    public class SetTerm : PnPSharePointCmdlet
    {

        private const string ParameterSet_BYID = "By Term Id";
        private const string ParameterSet_BYNAME = "By Term Name";

        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_BYID)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_BYNAME)]
        public TaxonomyTermPipeBind Identity;

        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public string Name;

        [Parameter(Mandatory = false)]
        public int Lcid = CultureInfo.CurrentCulture.LCID;

        [Parameter(Mandatory = false)]
        public string Description;

        [Parameter(Mandatory = false)]
        public Hashtable CustomProperties;

        [Parameter(Mandatory = false)]
        public Hashtable LocalCustomProperties;

        [Parameter(Mandatory = false)]
        public SwitchParameter DeleteAllCustomProperties;

        [Parameter(Mandatory = false)]
        public SwitchParameter DeleteAllLocalCustomProperties;

        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet_BYNAME)]
        public TaxonomyTermSetPipeBind TermSet;

        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet_BYNAME)]
        public TaxonomyTermGroupPipeBind TermGroup;

        [Parameter(Mandatory = false, ValueFromPipeline = true, ParameterSetName = ParameterAttribute.AllParameterSets)]
        public TaxonomyTermStorePipeBind TermStore;

        [Parameter(Mandatory = false)]
        public bool Deprecated;

        protected override void ExecuteCmdlet()
        {
            var taxonomySession = TaxonomySession.GetTaxonomySession(ClientContext);
            // Get Term Store
            TermStore termStore = null;
            if (TermStore == null)
            {
                termStore = taxonomySession.GetDefaultSiteCollectionTermStore();
            }
            else
            {
                termStore = TermStore.GetTermStore(taxonomySession);
            }
            termStore.EnsureProperty(ts => ts.DefaultLanguage);

            var termGroup = TermGroup.GetGroup(termStore);
            var termSet = TermSet.GetTermSet(termGroup);
            var term = Identity.GetTerm(ClientContext, termStore, termSet, false, null);

            if (ParameterSpecified(nameof(Name)))
            {
                term.Name = TaxonomyExtensions.NormalizeName(Name);
            }
            if (ParameterSpecified(nameof(Description)))
            {
                if (!ParameterSpecified(nameof(Lcid)))
                {
                    Lcid = termStore.EnsureProperty(ts => ts.DefaultLanguage);
                }
                term.SetDescription(Description, Lcid);
            }
            if (ParameterSpecified(nameof(DeleteAllCustomProperties)))
            {
                term.DeleteAllCustomProperties();
            }
            if (ParameterSpecified(nameof(DeleteAllLocalCustomProperties)))
            {
                term.DeleteAllLocalCustomProperties();
            }
            if (ParameterSpecified(nameof(CustomProperties)))
            {
                var customProperties = CustomProperties ?? new Hashtable();
                foreach (var key in customProperties.Keys)
                {
                    term.SetCustomProperty(key as string, customProperties[key] as string);
                }
            }
            if (ParameterSpecified(nameof(LocalCustomProperties)))
            {
                var localCustomProperties = LocalCustomProperties ?? new Hashtable();
                foreach (var key in localCustomProperties.Keys)
                {
                    term.SetLocalCustomProperty(key as string, localCustomProperties[key] as string);
                }
            }
            if (ParameterSpecified(nameof(Deprecated)))
            {
                term.Deprecate(Deprecated);
            }
            ClientContext.Load(term);
            termStore.CommitAll();
            ClientContext.ExecuteQueryRetry();
            WriteObject(term);
        }
    }
}


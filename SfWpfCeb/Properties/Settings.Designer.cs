﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Ce code a été généré par un outil.
//     Version du runtime :4.0.30319.42000
//
//     Les modifications apportées à ce fichier peuvent provoquer un comportement incorrect et seront perdues si
//     le code est régénéré.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CompteEstBon.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.3.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("10")]
        public int SolutionTimer {
            get {
                return ((int)(this["SolutionTimer"]));
            }
            set {
                this["SolutionTimer"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool MongoDB {
            get {
                return ((bool)(this["MongoDB"]));
            }
            set {
                this["MongoDB"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("mongodb://127.0.0.1:27017")]
        public string MongoServer {
            get => ((string)(this["MongoServer"]));
            set {
                this["MongoServer"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Mgo+DSMBaFt/QHRqVVhkX1pFdEBBXHxAd1p/VWJYdVt5flBPcDwsT3RfQF5jSH9VdkdgUX5WdndXQw==;Mgo+DSMBPh8sVXJ0S0J+XE9AdVRDX3xKf0x/TGpQb19xflBPallYVBYiSV9jS31Td0RlWX1deHVdQGVYVA==;ORg4AjUWIQA/Gnt2VVhkQlFaclxJXGFWfVJpTGpQdk5xdV9DaVZUTWY/P1ZhSXxQdkRiX39cc3xUTmZbVEE=;OTQ1NTA1QDMyMzAyZTM0MmUzMGRGOEhnc2ZtbVNqby9MMjVRT3RZbE43OFlzM1orcUZldit5ZlpTNGhpOU09;OTQ1NTA2QDMyMzAyZTM0MmUzMEsxVUJLY1NOUFM0YWRGSTVoWSt2KzFjTnE4YVRxWjZCb1I1OXNpVm5RTnM9;NRAiBiAaIQQuGjN/V0Z+WE9EaFtBVmJLYVB3WmpQdldgdVRMZVVbQX9PIiBoS35RdUViWHlecnddR2hbV0Zz;OTQ1NTA4QDMyMzAyZTM0MmUzMGJxZ2U3c0UyVHNXMmNCN0hjbTc2ME03RzU4WjFaOTZwWm9ncmVZMkl2VGM9;OTQ1NTA5QDMyMzAyZTM0MmUzMGVWMUIwUnVWVWRWY2N4YkxsaUxpVDF2aDZ6Q2wwZTJnMUpxMnNXRnl1dnc9;Mgo+DSMBMAY9C3t2VVhkQlFaclxJXGFWfVJpTGpQdk5xdV9DaVZUTWY/P1ZhSXxQdkRiX39cc3xUTmhYUkE=;OTQ1NTExQDMyMzAyZTM0MmUzMGlOUkpNVWZBemZaK0VOTTFoditjdVZITSsyYVNjeHd5Mi9BQVJYM2E3elk9;OTQ1NTEyQDMyMzAyZTM0MmUzMEo2ZzBDS3dvKzF1Qk55SWNrTzIrTzM1cUlOUFFYeUhvMWs2OUI0NjdNMDQ9;OTQ1NTEzQDMyMzAyZTM0MmUzMGJxZ2U3c0UyVHNXMmNCN0hjbTc2ME03RzU4WjFaOTZwWm9ncmVZMkl2VGM9")]
        public string SfLicence {
            get => ((string)(this["SfLicence"]));
            set {
                this["SfLicence"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool AutoCalcul {
            get => ((bool)(this["AutoCalcul"]));
            set {
                this["AutoCalcul"] = value;
            }
        }
    }
}

﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Timeline.Resources.Models.Validation {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Validator {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Validator() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Timeline.Resources.Models.Validation.Validator", typeof(Validator).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to create a validator instance from default constructor. See inner exception..
        /// </summary>
        internal static string ValidateWithAttributeExceptionCreateFail {
            get {
                return ResourceManager.GetString("ValidateWithAttributeExceptionCreateFail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Given type is not assignable to IValidator..
        /// </summary>
        internal static string ValidateWithAttributeExceptionNotValidator {
            get {
                return ResourceManager.GetString("ValidateWithAttributeExceptionNotValidator", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Value is not of type {0}..
        /// </summary>
        internal static string ValidatorMessageBadType {
            get {
                return ResourceManager.GetString("ValidatorMessageBadType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Value can&apos;t be null..
        /// </summary>
        internal static string ValidatorMessageNull {
            get {
                return ResourceManager.GetString("ValidatorMessageNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Validation succeeded..
        /// </summary>
        internal static string ValidatorMessageSuccess {
            get {
                return ResourceManager.GetString("ValidatorMessageSuccess", resourceCulture);
            }
        }
    }
}

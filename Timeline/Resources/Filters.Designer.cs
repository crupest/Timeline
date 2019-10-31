﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Timeline.Resources {
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
    internal class Filters {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Filters() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Timeline.Resources.Filters", typeof(Filters).Assembly);
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
        ///   Looks up a localized string similar to You apply a SelfOrAdminAttribute on an action, but there is no user. Try add AuthorizeAttribute..
        /// </summary>
        internal static string LogSelfOrAdminNoUser {
            get {
                return ResourceManager.GetString("LogSelfOrAdminNoUser", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You apply a SelfOrAdminAttribute on an action, but it does not have a model named username..
        /// </summary>
        internal static string LogSelfOrAdminNoUsername {
            get {
                return ResourceManager.GetString("LogSelfOrAdminNoUsername", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You apply a SelfOrAdminAttribute on an action, found a model named username, but it is not string..
        /// </summary>
        internal static string LogSelfOrAdminUsernameNotString {
            get {
                return ResourceManager.GetString("LogSelfOrAdminUsernameNotString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Header Content-Length is missing or of bad format..
        /// </summary>
        internal static string MessageHeaderContentLengthMissing {
            get {
                return ResourceManager.GetString("MessageHeaderContentLengthMissing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Header Content-Length must not be 0..
        /// </summary>
        internal static string MessageHeaderContentLengthZero {
            get {
                return ResourceManager.GetString("MessageHeaderContentLengthZero", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Header Content-Type is required..
        /// </summary>
        internal static string MessageHeaderContentTypeMissing {
            get {
                return ResourceManager.GetString("MessageHeaderContentTypeMissing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You can&apos;t access the resource unless you are the owner or administrator..
        /// </summary>
        internal static string MessageSelfOrAdminForbid {
            get {
                return ResourceManager.GetString("MessageSelfOrAdminForbid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The user does not exist..
        /// </summary>
        internal static string MessageUserNotExist {
            get {
                return ResourceManager.GetString("MessageUserNotExist", resourceCulture);
            }
        }
    }
}
